using System.Collections.ObjectModel;
using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Views;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ISessionService? _sessionService;
        private readonly IPomodoroService? _pomodoroService;
        private readonly IServiceProvider? _serviceProvider;
        private ViewMode _currentViewMode = ViewMode.Week;
        private DateTime _selectedDate = DateTime.Today;
        private EventModel? _selectedEvent;
        private string _userEmail = "user@example.com";
        private string _userName = "Користувач";
        private string _userInitials = "U";
        private int _userId;

        private System.Timers.Timer? _pomodoroTimer;
        private TimeSpan _remainingTime = TimeSpan.FromMinutes(25);
        private bool _isTimerRunning;
        private bool _isWorkSession = true;
        private int _completedCycles;
        private DateTime? _sessionStartTime;
        private bool _isLoadingGoals = false;
        private bool _isMessagingSubscribed = false;

        public MainViewModel(ISessionService? sessionService = null, IPomodoroService? pomodoroService = null, IServiceProvider? serviceProvider = null)
        {
            _sessionService = sessionService;
            _pomodoroService = pomodoroService;
            _serviceProvider = serviceProvider;
            Events = new ObservableCollection<EventModel>();
            TodoItems = new ObservableCollection<TodoItemModel>();
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
            StartPomodoroCommand = new RelayCommand(OnStartPomodoro);
            PausePomodoroCommand = new RelayCommand(OnPausePomodoro);
            ResetPomodoroCommand = new RelayCommand(OnResetPomodoro);
            
        PreviousMonthCommand = new RelayCommand(OnPreviousMonth);
        NextMonthCommand = new RelayCommand(OnNextMonth);
        SelectDayCommand = new RelayCommand<CalendarDayModel>(OnSelectDay);
        
        // Ініціалізуємо дату тижня на поточну
        UpdateDateRange();
        GenerateCalendarDays();
    }
    
    public void Initialize()
    {
        System.Diagnostics.Debug.WriteLine("MainViewModel: Initialize викликано");
        
        // Відписуємося від старих підписок якщо вони є
        if (_isMessagingSubscribed)
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Відписуємося від старих підписок");
            MessagingCenter.Unsubscribe<AddPlanViewModel>(this, "GoalAdded");
            MessagingCenter.Unsubscribe<AddPlanViewModel>(this, "GoalUpdated");
            MessagingCenter.Unsubscribe<ViewPlanViewModel>(this, "GoalDeleted");
        }
        
        _isMessagingSubscribed = true;
        
        MessagingCenter.Subscribe<AddPlanViewModel>(this, "GoalAdded", (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Отримано повідомлення про додавання плану");
            _ = LoadGoalsFromDatabaseAsync();
        });
        
        MessagingCenter.Subscribe<AddPlanViewModel>(this, "GoalUpdated", (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Отримано повідомлення про оновлення плану");
            _ = LoadGoalsFromDatabaseAsync();
        });
        
        MessagingCenter.Subscribe<ViewPlanViewModel>(this, "GoalDeleted", (sender) =>
        {
            System.Diagnostics.Debug.WriteLine("MainViewModel: Отримано повідомлення про видалення плану");
            _ = LoadGoalsFromDatabaseAsync();
        });
        
        System.Diagnostics.Debug.WriteLine("MainViewModel: Запускаємо фонове завантаження...");
        _ = InitializeAsync();
    }

    public async void RefreshData()
    {
        System.Diagnostics.Debug.WriteLine("MainViewModel: RefreshData викликано");
        
        try
        {
            await LoadGoalsFromDatabaseAsync();
            
            // Примусово оновлюємо відображення календаря в UI потоці
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"MainViewModel: RefreshData завершено. Events.Count = {Events.Count}");
                    OnPropertyChanged(nameof(Events));
                    GenerateCalendarDays();
                }
                catch (Exception uiEx)
                {
                    System.Diagnostics.Debug.WriteLine($"MainViewModel: Помилка UI оновлення - {uiEx.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainViewModel: Помилка RefreshData - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"MainViewModel: Stack - {ex.StackTrace}");
        }
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
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    SetUserInfo(sessionValue.Email, sessionValue.Username ?? sessionValue.Email);
                    UserId = sessionValue.UserId;
                    System.Diagnostics.Debug.WriteLine($"InitializeAsync: Користувач завантажено - {sessionValue.Username}");
                });
                
                System.Diagnostics.Debug.WriteLine("InitializeAsync: Завантаження цілей з БД...");
                await LoadGoalsFromDatabaseAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("InitializeAsync: Завершено");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("InitializeAsync: Сесія не знайдена");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ MainViewModel: Помилка Initialize - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
        }
    }

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
        
        public ICommand StartPomodoroCommand { get; }
        public ICommand PausePomodoroCommand { get; }
        public ICommand ResetPomodoroCommand { get; }
     
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

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Використовуємо Window замість застарілого MainPage
                var window = Application.Current?.Windows[0];
                if (window != null)
                {
                    var loginViewModel = _serviceProvider?.GetRequiredService<LoginViewModel>();
                    var sessionService = _serviceProvider?.GetRequiredService<ISessionService>();
                    var loginPage = new LoginPage(loginViewModel!, sessionService!);
                    window.Page = new NavigationPage(loginPage);
                }
            });
        }

        private void OnStartPomodoro()
        {
            if (IsTimerRunning)
            {
                IsTimerRunning = false;
                _pomodoroTimer?.Stop();
                OnPropertyChanged(nameof(TimerButtonText));
            }
            else
            {
                IsTimerRunning = true;
                
                if (_sessionStartTime == null)
                {
                    _sessionStartTime = DateTime.Now;
                }

                if (_pomodoroTimer == null)
                {
                    _pomodoroTimer = new System.Timers.Timer(1000); 
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

                if (_completedCycles % 4 == 0)
                {
                    _remainingTime = TimeSpan.FromMinutes(15); 
                    await Shell.Current.DisplayAlert(
                        "Помодоро завершено! 🎉",
                        "Час для довгої перерви (15 хвилин)",
                        "OK");
                }
                else
                {
                    _remainingTime = TimeSpan.FromMinutes(5); 
                    await Shell.Current.DisplayAlert(
                        "Помодоро завершено! ✅",
                        "Час для короткої перерви (5 хвилин)",
                        "OK");
                }

                _isWorkSession = false;
            }
            else
            {
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
      
        private void GenerateCalendarDays()
        {
            CalendarDays.Clear();
            
            var firstDayOfMonth = new DateTime(CalendarCurrentMonth.Year, CalendarCurrentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (firstDayOfWeek == 0) firstDayOfWeek = 7; 
           
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
            
            var totalDays = CalendarDays.Count;
            var remainingDays = (7 - (totalDays % 7)) % 7;
            if (remainingDays > 0 || totalDays < 35)
            {
                var nextMonth = firstDayOfMonth.AddMonths(1);
                var daysToAdd = remainingDays > 0 ? remainingDays : 7;
                
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
            foreach (var day in CalendarDays)
            {
                day.IsSelected = false;
            }
            selectedDay.IsSelected = true;
            SelectedDate = selectedDay.Date;

            OnPropertyChanged(nameof(CalendarDays));
        }
        
        private async Task LoadGoalsFromDatabaseAsync()
        {
            if (_isLoadingGoals)
            {
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Завантаження вже виконується, пропускаємо...");
                return;
            }

            if (_serviceProvider == null)
            {
                System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: ServiceProvider не доступний");
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
                
                // Створюємо новий scope для кожного запиту
                using var scope = _serviceProvider.CreateScope();
                var goalService = scope.ServiceProvider.GetRequiredService<IGoalService>();
                
                IEnumerable<DAL.Models.Goal> goals;
                try
                {
                    goals = await goalService.GetGoalsByUserIdAsync(UserId).ConfigureAwait(false);
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Помилка БД - {dbEx.Message}");
                    return;
                }
                
                var goalsList = goals.ToList();
                System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Знайдено {goalsList.Count} планів");

                var eventModels = new List<EventModel>();
                
                foreach (var goal in goalsList)
                {
                    if (goal.StartTime.HasValue && goal.EndTime.HasValue)
                    {
                        // Розбиваємо опис на заголовок та деталі
                        var lines = goal.Description.Split('\n', 2);
                        string title = lines.Length > 0 ? lines[0] : goal.Description;
                        string description = lines.Length > 1 ? lines[1] : string.Empty;

                        eventModels.Add(new EventModel
                        {
                            Id = goal.Id,
                            Title = title,
                            Description = description,
                            StartDateTime = goal.StartTime.Value,
                            EndDateTime = goal.EndTime.Value,
                            Priority = (EventPriority)goal.Priority,
                            Color = Colors.Blue,
                            IsCompleted = goal.Progress >= 100
                        });
                    }
                }
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Events.Clear();
                        foreach (var eventModel in eventModels)
                        {
                            Events.Add(eventModel);
                            System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Додано план '{eventModel.Title}' (Start: {eventModel.StartDateTime}, End: {eventModel.EndDateTime})");
                        }
                       
                        System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Регенерація календаря...");
                        
                        // Примусово оновлюємо прив'язку Events
                        OnPropertyChanged(nameof(Events));
                        
                        GenerateCalendarDays();
                        System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Завершено успішно");
                    }
                    catch (Exception uiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Помилка UI - {uiEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Stack trace - {uiEx.StackTrace}");
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
        
        public async Task ReloadGoals()
        {
            System.Diagnostics.Debug.WriteLine("ReloadGoals: Починаємо перезавантаження...");
            await Task.Delay(300);
            await LoadGoalsFromDatabaseAsync();
        }
    }
}

