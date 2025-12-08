using System.Collections.ObjectModel;
using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.Maui.Controls;

namespace Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ISessionService? _sessionService;
        private readonly IPomodoroService? _pomodoroService;
        private readonly IGoalService? _goalService;
        private ViewMode _currentViewMode = ViewMode.Week;
        private DateTime _selectedDate = DateTime.Today;
        private EventModel? _selectedEvent;
        private string _userEmail = "user@example.com";
        private string _userName = "Користувач";
        private string _userInitials = "U";
        private int _userId;

        // Pomodoro Timer Properties
        private System.Timers.Timer? _pomodoroTimer;
        private TimeSpan _remainingTime = TimeSpan.FromMinutes(25);
        private bool _isTimerRunning;
        private bool _isWorkSession = true;
        private int _completedCycles;
        private DateTime? _sessionStartTime;
        private bool _isLoadingGoals = false;

        public MainViewModel(ISessionService? sessionService = null, IPomodoroService? pomodoroService = default, IGoalService? goalService = null)
        {
            _sessionService = sessionService;
            _pomodoroService = pomodoroService;
            _goalService = goalService;
            Events = new ObservableCollection<EventModel>();
            TodoItems = new ObservableCollection<TodoItemModel>();
            
            // Commands
            AddPlanCommand = new RelayCommand(OnAddPlan);
            NavigateNextCommand = new RelayCommand(OnNavigateNext);
            NavigatePreviousCommand = new RelayCommand(OnNavigatePrevious);
            GoToTodayCommand = new RelayCommand(OnGoToToday);
            ChangeModeCommand = new RelayCommand<object>(OnChangeModeObject);
            EditEventCommand = new RelayCommand<EventModel>(OnEditEvent);
            DeleteEventCommand = new RelayCommand<EventModel>(OnDeleteEvent);
            ToggleTodoCommand = new RelayCommand<TodoItemModel>(OnToggleTodo);
            NavigateToStatisticsCommand = new RelayCommand(OnNavigateToStatistics);
            LogoutCommand = new AsyncRelayCommand(OnLogout);
            
            // Pomodoro Commands
            StartPomodoroCommand = new RelayCommand(OnStartPomodoro);
            PausePomodoroCommand = new RelayCommand(OnPausePomodoro);
            ResetPomodoroCommand = new RelayCommand(OnResetPomodoro);
            
        // Calendar Commands
        PreviousMonthCommand = new RelayCommand(OnPreviousMonth);
        NextMonthCommand = new RelayCommand(OnNextMonth);
        SelectDayCommand = new RelayCommand<CalendarDayModel>(OnSelectDay);
        
        // Генеруємо порожній календар
        GenerateCalendarDays();
    }
    
    // Публічний метод для ініціалізації даних (викликається з DashboardPage)
    public void Initialize()
    {
        System.Diagnostics.Debug.WriteLine("MainViewModel: Initialize викликано");
        
        // Підписуємось на повідомлення про додавання нового плану
        MessagingCenter.Subscribe<AddPlanViewModel>(this, "GoalAdded", (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Отримано повідомлення про додавання плану");
            _ = LoadGoalsFromDatabaseAsync();
        });
        
        // Запускаємо завантаження даних в фоновому режимі
        System.Diagnostics.Debug.WriteLine("MainViewModel: Запускаємо фонове завантаження...");
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Початок завантаження сесії...");
            
            if (_sessionService == null)
            {
                System.Diagnostics.Debug.WriteLine("InitializeAsync: SessionService не доступний");
                return;
            }

            (string Email, int UserId, string? Username)? session;
            try
            {
                session = await _sessionService.GetUserSessionAsync().ConfigureAwait(false);
            }
            catch (Exception sessionEx)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeAsync: Помилка отримання сесії - {sessionEx.Message}");
                return;
            }
            
            if (session.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeAsync: Сесія отримана - UserId={session.Value.UserId}");
                
                var sessionValue = session.Value;
                
                // Оновлюємо UI в головному потоці
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetUserInfo(sessionValue.Email, sessionValue.Username ?? sessionValue.Email);
                    UserId = sessionValue.UserId;
                    System.Diagnostics.Debug.WriteLine($"InitializeAsync: Користувач завантажено - {sessionValue.Username}");
                });
                
                // Завантажуємо плани (цей метод сам оброблюе UI потік)
                await LoadGoalsFromDatabaseAsync().ConfigureAwait(false);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("InitializeAsync: Сесія не знайдена");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainViewModel: Помилка Initialize - {ex.Message}");
        }
    }

        // Properties
        public ObservableCollection<EventModel> Events { get; }
        public ObservableCollection<TodoItemModel> TodoItems { get; }

        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    UpdateDateRange();
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateDateRange();
                }
            }
        }

        public EventModel? SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        public string UserEmail
        {
            get => _userEmail;
            set => SetProperty(ref _userEmail, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string UserInitials
        {
            get => _userInitials;
            set => SetProperty(ref _userInitials, value);
        }

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        // Pomodoro Properties
        public string PomodoroTimeText
        {
            get => $"{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
        }

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set => SetProperty(ref _isTimerRunning, value);
        }

        public string TimerButtonText => IsTimerRunning ? "❚❚" : "▶";

        private DateTime _weekStartDate;
        public DateTime WeekStartDate
        {
            get => _weekStartDate;
            set => SetProperty(ref _weekStartDate, value);
        }

        private DateTime _weekEndDate;
        public DateTime WeekEndDate
        {
            get => _weekEndDate;
            set => SetProperty(ref _weekEndDate, value);
        }

        public string DateRangeText => CurrentViewMode switch
        {
            ViewMode.Week => $"{WeekStartDate:dd} — {WeekEndDate:dd MMM yyyy}",
            ViewMode.Month => SelectedDate.ToString("MMMM yyyy"),
            ViewMode.Day => SelectedDate.ToString("dd MMMM yyyy"),
            _ => string.Empty
        };

        // Commands
        public ICommand AddPlanCommand { get; }
        public ICommand NavigateNextCommand { get; }
        public ICommand NavigatePreviousCommand { get; }
        public ICommand GoToTodayCommand { get; }
        public ICommand ChangeModeCommand { get; }
        public ICommand EditEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand ToggleTodoCommand { get; }
        public ICommand NavigateToStatisticsCommand { get; }
        public ICommand LogoutCommand { get; }
        
        // Pomodoro Commands
        public ICommand StartPomodoroCommand { get; }
        public ICommand PausePomodoroCommand { get; }
        public ICommand ResetPomodoroCommand { get; }
        
        // Calendar Properties and Commands
        private DateTime _calendarCurrentMonth = DateTime.Today;
        private CalendarDayModel? _selectedCalendarDay;
        
        public ObservableCollection<CalendarDayModel> CalendarDays { get; } = new();
        
        public DateTime CalendarCurrentMonth
        {
            get => _calendarCurrentMonth;
            set
            {
                if (SetProperty(ref _calendarCurrentMonth, value))
                {
                    GenerateCalendarDays();
                }
            }
        }
        
        public CalendarDayModel? SelectedCalendarDay
        {
            get => _selectedCalendarDay;
            set => SetProperty(ref _selectedCalendarDay, value);
        }
        
        public string CurrentMonthYear => CalendarCurrentMonth.ToString("MMMM yyyy");
        
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SelectDayCommand { get; }

        // Command Handlers
        private async void OnAddPlan()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.AddPlanPage));
            });
        }

        private void OnNavigateNext()
        {
            SelectedDate = CurrentViewMode switch
            {
                ViewMode.Day => SelectedDate.AddDays(1),
                ViewMode.Week => SelectedDate.AddDays(7),
                ViewMode.Month => SelectedDate.AddMonths(1),
                _ => SelectedDate
            };
        }

        private void OnNavigatePrevious()
        {
            SelectedDate = CurrentViewMode switch
            {
                ViewMode.Day => SelectedDate.AddDays(-1),
                ViewMode.Week => SelectedDate.AddDays(-7),
                ViewMode.Month => SelectedDate.AddMonths(-1),
                _ => SelectedDate
            };
        }

        private void OnGoToToday()
        {
            SelectedDate = DateTime.Today;
        }

        private void OnChangeModeObject(object? modeParam)
        {
            if (modeParam == null)
                return;

            ViewMode mode;
            
            // Якщо параметр - це рядок, конвертуємо його в енум
            if (modeParam is string modeString)
            {
                if (Enum.TryParse<ViewMode>(modeString, true, out var parsedMode))
                {
                    mode = parsedMode;
                }
                else
                {
                    return;
                }
            }
            // Якщо параметр вже ViewMode
            else if (modeParam is ViewMode viewMode)
            {
                mode = viewMode;
            }
            else
            {
                return;
            }
            
            OnChangeMode(mode);
        }

        private void OnChangeMode(ViewMode mode)
        {
            CurrentViewMode = mode;
        }


        private void OnEditEvent(EventModel? eventModel)
        {
            if (eventModel != null)
            {
                SelectedEvent = eventModel;
                // Navigate to edit page
            }
        }

        private void OnDeleteEvent(EventModel? eventModel)
        {
            if (eventModel != null)
            {
                Events.Remove(eventModel);
            }
        }

        private void OnToggleTodo(TodoItemModel? todoItem)
        {
            if (todoItem != null)
            {
                todoItem.IsCompleted = !todoItem.IsCompleted;
            }
        }

        private async void OnNavigateToStatistics()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.StatisticsPage));
            });
        }

        private void UpdateDateRange()
        {
            if (CurrentViewMode == ViewMode.Week)
            {
                var diff = (7 + (SelectedDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
                WeekStartDate = SelectedDate.AddDays(-diff).Date;
                WeekEndDate = WeekStartDate.AddDays(6);
            }
            OnPropertyChanged(nameof(DateRangeText));
        }

        private void LoadSampleData()
        {
            // Sample events
            Events.Add(new EventModel
            {
                Id = 1,
                Title = "Зустріч з командою",
                Description = "Обговорення проекту",
                StartDateTime = DateTime.Today.AddHours(10),
                EndDateTime = DateTime.Today.AddHours(11),
                Priority = EventPriority.High,
                Color = Colors.Red
            });

            Events.Add(new EventModel
            {
                Id = 2,
                Title = "Робота над завданням",
                Description = "Розробка UI",
                StartDateTime = DateTime.Today.AddHours(14),
                EndDateTime = DateTime.Today.AddHours(16),
                Priority = EventPriority.Normal,
                Color = Colors.Blue
            });

            TodoItems.Add(new TodoItemModel
            {
                Id = 1,
                Description = "Завершити дизайн інтерфейсу",
                IsCompleted = false,
                Priority = 2
            });

            TodoItems.Add(new TodoItemModel
            {
                Id = 2,
                Description = "Написати тести",
                IsCompleted = false,
                Priority = 1
            });

            UpdateDateRange();
        }

        public void SetUserInfo(string email, string? username = null)
        {
            UserEmail = email;

            if (!string.IsNullOrEmpty(username))
            {
                UserName = username;
                
                var parts = username.Split(new[] { ' ', '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    UserInitials = $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
                }
                else if (parts.Length == 1 && parts[0].Length > 0)
                {
                    UserInitials = char.ToUpper(parts[0][0]).ToString();
                }
            }
            else
            {
                var emailPart = email.Split('@')[0];
                UserName = emailPart;
                
                if (emailPart.Length > 0)
                {
                    // Беремо першу літеру або дві перші якщо є крапка чи підкреслення
                    if (emailPart.Contains('.') || emailPart.Contains('_'))
                    {
                        var parts = emailPart.Split(new[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            UserInitials = $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
                        }
                        else
                        {
                            UserInitials = char.ToUpper(emailPart[0]).ToString();
                        }
                    }
                    else
                    {
                        UserInitials = char.ToUpper(emailPart[0]).ToString();
                    }
                }
            }
        }


        private async Task OnLogout()
        {
            if (_sessionService != null)
            {
                await _sessionService.ClearSessionAsync();
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Очищаємо навігаційний стек і повертаємось на LoginPage
                await Shell.Current.Navigation.PopToRootAsync();
            });
        }

        // Pomodoro Timer Methods
        private void OnStartPomodoro()
        {
            if (IsTimerRunning)
            {
                // Якщо таймер працює - ставимо на паузу
                IsTimerRunning = false;
                _pomodoroTimer?.Stop();
                OnPropertyChanged(nameof(TimerButtonText));
            }
            else
            {
                // Якщо таймер не працює - запускаємо
                IsTimerRunning = true;
                
                if (_sessionStartTime == null)
                {
                    _sessionStartTime = DateTime.Now;
                }

                if (_pomodoroTimer == null)
                {
                    _pomodoroTimer = new System.Timers.Timer(1000); // 1 секунда
                    _pomodoroTimer.Elapsed += OnTimerTick;
                }

                _pomodoroTimer.Start();
                OnPropertyChanged(nameof(TimerButtonText));
            }
        }

        private void OnPausePomodoro()
        {
            if (IsTimerRunning)
            {
                IsTimerRunning = false;
                _pomodoroTimer?.Stop();
                OnPropertyChanged(nameof(TimerButtonText));
            }
        }

        private void OnResetPomodoro()
        {
            IsTimerRunning = false;
            _pomodoroTimer?.Stop();
            _remainingTime = TimeSpan.FromMinutes(25);
            _sessionStartTime = null;
            _completedCycles = 0;
            _isWorkSession = true;
            
            OnPropertyChanged(nameof(PomodoroTimeText));
            OnPropertyChanged(nameof(TimerButtonText));
        }

        private async void OnTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OnPropertyChanged(nameof(PomodoroTimeText));
            });

            if (_remainingTime.TotalSeconds <= 0)
            {
                await OnTimerCompleted();
            }
        }

        private async Task OnTimerCompleted()
        {
            _pomodoroTimer?.Stop();
            IsTimerRunning = false;

            if (_isWorkSession)
            {
                _completedCycles++;

                // Зберігаємо завершену сесію в БД
                if (_pomodoroService != null && _sessionStartTime.HasValue)
                {
                    var pomodoro = new DAL.Models.Pomodoro
                    {
                        UserId = UserId,
                        StartTime = _sessionStartTime.Value,
                        EndTime = DateTime.Now,
                        WorkCycles = 1
                    };

                    await _pomodoroService.AddPomodoroAsync(pomodoro);
                }

                // Перемикаємось на перерву
                if (_completedCycles % 4 == 0)
                {
                    _remainingTime = TimeSpan.FromMinutes(15); // Довга перерва
                    await Shell.Current.DisplayAlert(
                        "Помодоро завершено! 🎉",
                        "Час для довгої перерви (15 хвилин)",
                        "OK");
                }
                else
                {
                    _remainingTime = TimeSpan.FromMinutes(5); // Коротка перерва
                    await Shell.Current.DisplayAlert(
                        "Помодоро завершено! ✅",
                        "Час для короткої перерви (5 хвилин)",
                        "OK");
                }

                _isWorkSession = false;
            }
            else
            {
                // Перерва закінчилась, повертаємось до роботи
                _remainingTime = TimeSpan.FromMinutes(25);
                _isWorkSession = true;
                _sessionStartTime = null;

                await Shell.Current.DisplayAlert(
                    "Перерва завершена! 💪",
                    "Час повертатися до роботи",
                    "OK");
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OnPropertyChanged(nameof(PomodoroTimeText));
                OnPropertyChanged(nameof(TimerButtonText));
            });
        }
        
        // Calendar Methods
        private void GenerateCalendarDays()
        {
            CalendarDays.Clear();
            
            var firstDayOfMonth = new DateTime(CalendarCurrentMonth.Year, CalendarCurrentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            // Понеділок = 1, Неділя = 0 (в C# DayOfWeek)
            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (firstDayOfWeek == 0) firstDayOfWeek = 7; // Неділя стає 7
            
            // Додаємо дні з попереднього місяця
            var previousMonth = firstDayOfMonth.AddMonths(-1);
            var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            
            for (int i = firstDayOfWeek - 1; i > 0; i--)
            {
                var day = daysInPreviousMonth - i + 1;
                var date = new DateTime(previousMonth.Year, previousMonth.Month, day);
                var eventCount = GetEventCountForDate(date);
                
                CalendarDays.Add(new CalendarDayModel
                {
                    Day = day,
                    Date = date,
                    IsCurrentMonth = false,
                    IsToday = false,
                    IsSelected = false,
                    HasEvents = eventCount > 0,
                    EventCount = eventCount
                });
            }
            
            // Додаємо дні поточного місяця
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var date = new DateTime(CalendarCurrentMonth.Year, CalendarCurrentMonth.Month, day);
                var eventCount = GetEventCountForDate(date);
                
                CalendarDays.Add(new CalendarDayModel
                {
                    Day = day,
                    Date = date,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Today,
                    IsSelected = date.Date == SelectedDate.Date,
                    HasEvents = eventCount > 0,
                    EventCount = eventCount
                });
            }
            
            // Додаємо дні з наступного місяця, щоб заповнити сітку
            var totalDays = CalendarDays.Count;
            var remainingDays = (7 - (totalDays % 7)) % 7;
            if (remainingDays > 0 || totalDays < 35)
            {
                var nextMonth = firstDayOfMonth.AddMonths(1);
                var daysToAdd = remainingDays > 0 ? remainingDays : 7;
                
                // Додаємо ще один тиждень якщо менше 35 днів
                if (totalDays + daysToAdd < 35)
                {
                    daysToAdd += 7;
                }
                
                for (int day = 1; day <= daysToAdd; day++)
                {
                    var date = new DateTime(nextMonth.Year, nextMonth.Month, day);
                    var eventCount = GetEventCountForDate(date);
                    
                    CalendarDays.Add(new CalendarDayModel
                    {
                        Day = day,
                        Date = date,
                        IsCurrentMonth = false,
                        IsToday = false,
                        IsSelected = false,
                        HasEvents = eventCount > 0,
                        EventCount = eventCount
                    });
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Calendar generated: {CalendarDays.Count} days");
            OnPropertyChanged(nameof(CurrentMonthYear));
            OnPropertyChanged(nameof(CalendarDays));
        }
        
        private int GetEventCountForDate(DateTime date)
        {
            return Events.Count(e => 
                e.StartDateTime.Date <= date.Date && 
                e.EndDateTime.Date >= date.Date);
        }
        
        private void OnPreviousMonth()
        {
            CalendarCurrentMonth = CalendarCurrentMonth.AddMonths(-1);
        }
        
        private void OnNextMonth()
        {
            CalendarCurrentMonth = CalendarCurrentMonth.AddMonths(1);
        }
        
        private void OnSelectDay(CalendarDayModel? selectedDay)
        {
            if (selectedDay == null) return;

            // Знімаємо виділення з усіх днів
            foreach (var day in CalendarDays)
            {
                day.IsSelected = false;
            }

            // Виділяємо вибраний день
            selectedDay.IsSelected = true;
            SelectedDate = selectedDay.Date;

            // Оновлюємо календар
            OnPropertyChanged(nameof(CalendarDays));
        }
        
        private async Task LoadGoalsFromDatabaseAsync()
        {
            // Якщо вже йде завантаження - не запускаємо нове
            if (_isLoadingGoals)
            {
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Завантаження вже виконується, пропускаємо...");
                return;
            }

            if (_goalService == null)
            {
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Goal сервіс не доступний");
                return;
            }

            if (UserId == 0)
            {
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: UserId не встановлено");
                return;
            }

            _isLoadingGoals = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Завантаження планів для користувача {UserId}...");
                
                // Завантажуємо плани з БД (це асинхронна операція)
                IEnumerable<DAL.Models.Goal> goals;
                try
                {
                    goals = await _goalService.GetGoalsByUserIdAsync(UserId).ConfigureAwait(false);
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Помилка БД - {dbEx.Message}");
                    return;
                }
                
                var goalsList = goals.ToList();
                System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Знайдено {goalsList.Count} планів");

                // Підготовлюємо дані для UI
                var eventModels = new List<EventModel>();
                
                foreach (var goal in goalsList)
                {
                    if (goal.Deadline.HasValue)
                    {
                        DateTime startDateTime = goal.CreatedAt;
                        string title = goal.Description;
                        string description = string.Empty;

                        // Парсимо опис для отримання часу початку та тексту
                        if (goal.Description.StartsWith("START_TIME:"))
                        {
                            var lines = goal.Description.Split('\n');
                            if (lines.Length > 0 && DateTime.TryParse(lines[0].Replace("START_TIME:", ""), out var parsedStartTime))
                            {
                                startDateTime = parsedStartTime;
                            }
                            
                            if (lines.Length > 1)
                            {
                                title = lines[1];
                            }
                            
                            if (lines.Length > 2)
                            {
                                description = string.Join("\n", lines.Skip(2));
                            }
                        }
                        else
                        {
                            // Старий формат - розбираємо опис на назву та детальний опис
                            var descriptionParts = goal.Description.Split('\n', 2);
                            title = descriptionParts.Length > 0 ? descriptionParts[0] : goal.Description;
                            description = descriptionParts.Length > 1 ? descriptionParts[1] : string.Empty;
                        }

                        eventModels.Add(new EventModel
                        {
                            Id = goal.Id,
                            Title = title,
                            Description = description,
                            StartDateTime = startDateTime,
                            EndDateTime = goal.Deadline.Value,
                            Priority = EventPriority.Normal,
                            Color = Colors.Blue
                        });
                    }
                }
                
                // Оновлюємо UI в головному потоці
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Events.Clear();
                        foreach (var eventModel in eventModels)
                        {
                            Events.Add(eventModel);
                            System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Додано план '{eventModel.Title}'");
                        }
                        
                        // Регенеруємо календар після завантаження подій
                        System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Регенерація календаря...");
                        GenerateCalendarDays();
                        System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Завершено успішно");
                    }
                    catch (Exception uiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Помилка UI - {uiEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: ПОМИЛКА - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Stack trace - {ex.StackTrace}");
            }
            finally
            {
                _isLoadingGoals = false;
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Флаг завантаження знято");
            }
        }
        
        // Публічний метод для перезавантаження цілей (викликається після додавання нового плану)
        public async Task ReloadGoals()
        {
            System.Diagnostics.Debug.WriteLine("ReloadGoals: Починаємо перезавантаження...");
            await Task.Delay(300);
            await LoadGoalsFromDatabaseAsync();
        }
    }
}

