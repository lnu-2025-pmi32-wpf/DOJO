using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Views;

namespace Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // 🎉 Подія підвищення рівня для показу анімованого popup
        public event EventHandler<(int NewLevel, int ExpGained)>? LevelUp;

        private readonly ISessionService? _sessionService;
        private readonly IPomodoroService? _pomodoroService;
        private readonly IServiceProvider? _serviceProvider;
        private readonly IToDoTaskService? _todoTaskService;
        private ViewMode _currentViewMode = ViewMode.Week;
        private DateTime _selectedDate = DateTime.Today;
        private EventModel? _selectedEvent;
        private string _userEmail = "user@example.com";
        private string _userName = "Користувач";
        private string _userInitials = "U";
        private int _userId;
        private int _userLevel = 1;
        private int _userExp = 0;
        private int _userExpToNextLevel = 600;

        private System.Timers.Timer? _pomodoroTimer;
        private TimeSpan _remainingTime = TimeSpan.FromMinutes(25);
        private bool _isTimerRunning;
        private bool _isWorkSession = true;
        private int _completedCycles;
        private DateTime? _sessionStartTime;
        private bool _isLoadingGoals = false;
        private bool _isMessagingSubscribed = false;
        private readonly IExperienceService? _experienceService;

        private ObservableCollection<DAL.Models.ToDoTask> _todoTasksFromDb = new();

        public MainViewModel(ISessionService? sessionService = null, IPomodoroService? pomodoroService = null, IServiceProvider? serviceProvider = null, IToDoTaskService? todoTaskService = null, IExperienceService? experienceService = null)
        {
            _sessionService = sessionService;
            _pomodoroService = pomodoroService;
            _serviceProvider = serviceProvider;
            _todoTaskService = todoTaskService;
            _experienceService = experienceService;

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

            // TODO Commands
            OpenTodoPopupCommand = new AsyncRelayCommand(OnOpenTodoPopup);
            ToggleTodoTaskCommand = new RelayCommand<DAL.Models.ToDoTask>(async (task) => await OnToggleTodoTask(task));

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

            // Підписуємось на повідомлення про додавання нового TODO
            MessagingCenter.Subscribe<AddTodoViewModel>(this, "TodoAdded", async (sender) =>
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel: Отримано повідомлення про додавання TODO");
                await LoadTodoItems();
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

                    // Завантажуємо TODO завдання
                    await LoadTodoItems();

                    // Завантажуємо прогрес користувача
                    await LoadUserProgress();

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

        public ObservableCollection<DAL.Models.ToDoTask> TodoTasksFromDb
        {
            get => _todoTasksFromDb;
            set => SetProperty(ref _todoTasksFromDb, value);
        }

        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    System.Diagnostics.Debug.WriteLine($"CurrentViewMode змінився на:  {value}");
                    UpdateDateRange();

                    // Коли повертаємося на місячний вигляд - примусово оновлюємо SelectedDate
                    if (value == ViewMode.Month)
                    {
                        // Зберігаємо поточну дату
                        var currentDate = SelectedDate;

                        // Оновлюємо CalendarCurrentMonth
                        CalendarCurrentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

                        // Примусово тригеримо оновлення SelectedDate, навіть якщо значення не змінилось
                        // Це змусить MonthViewGrid перебудуватись
                        OnPropertyChanged(nameof(SelectedDate));

                        System.Diagnostics.Debug.WriteLine($"Місячний вигляд активовано.  SelectedDate: {SelectedDate: yyyy-MM-dd}");
                    }
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
                    System.Diagnostics.Debug.WriteLine($"SelectedDate змінився на: {value:yyyy-MM-dd}");
                    UpdateDateRange();

                    // Оновлюємо CalendarCurrentMonth якщо місяць змінився
                    if (CalendarCurrentMonth.Month != value.Month || CalendarCurrentMonth.Year != value.Year)
                    {
                        CalendarCurrentMonth = new DateTime(value.Year, value.Month, 1);
                    }

                    GenerateCalendarDays();
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

        // Прогрес свинки-героя
        public int UserLevel
        {
            get => _userLevel;
            set
            {
                System.Diagnostics.Debug.WriteLine($"🔄 UserLevel змінюється: {_userLevel} → {value}");
                SetProperty(ref _userLevel, value);
            }
        }

        public int UserExp
        {
            get => _userExp;
            set => SetProperty(ref _userExp, value);
        }

        public int UserExpToNextLevel
        {
            get => _userExpToNextLevel;
            set => SetProperty(ref _userExpToNextLevel, value);
        }

        public string UserExpProgressText => $"{UserExp} / 600 XP";  // 🔥 ЗАВЖДИ /600

        // 🔥 ДОДАЙ ЦЮ НОВУ ВЛАСТИВІСТЬ
        public double UserProgressPercent
        {
            get
            {
                if (UserExpToNextLevel == 0) return 0;
                return (double)UserExp / UserExpToNextLevel;
            }
        }

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        // Статистика для Dashboard
        // Статистика для Dashboard (TODO завдання)
        public int CompletedTasksCount => TodoTasksFromDb.Count(t => t.IsCompleted);
        public int TotalTasksCount => TodoTasksFromDb.Count;
        public double TotalWorkHours
        {
            get
            {
                // Підраховуємо загальний час роботи з Pomodoro або з планів
                // Поки що можна показувати 0 або рахувати з Events
                return Events
                    .Where(e => e.IsCompleted)
                    .Sum(e => (e.EndDateTime - e.StartDateTime).TotalHours);
            }
        }
        public double ProductivityPercentage
        {
            get
            {
                if (TotalTasksCount == 0) return 0;
                return (double)CompletedTasksCount / TotalTasksCount * 100;
            }
        }

        public string PomodoroTimeText
        {
            get
            {
                int minutes = (int)_remainingTime.TotalMinutes;
                int seconds = _remainingTime.Seconds;
                return $"{minutes:00}:{seconds:00}";
            }
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

        private DateTime _week_end_date;
        public DateTime WeekEndDate
        {
            get => _week_end_date;
            set => SetProperty(ref _week_end_date, value);
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

        // TODO Commands
        public ICommand OpenTodoPopupCommand { get; private set; }
        public ICommand ToggleTodoTaskCommand { get; private set; }

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

        /// <summary>
        /// Повертає події відсортовані: невиконані першими, виконані в кінці
        /// </summary>
        public IEnumerable<EventModel> SortedEvents => Events
            .OrderBy(e => e.IsCompleted)
            .ThenBy(e => e.EndDateTime);

        /// <summary>
        /// Позначає план як виконаний/невиконаний та зберігає в БД
        /// </summary>
        public async Task TogglePlanCompletedAsync(EventModel eventModel, bool isCompleted)
        {
            if (_serviceProvider == null || _experienceService == null)
            {
                System.Diagnostics.Debug.WriteLine("TogglePlanCompleted: ServiceProvider не доступний");
                return;
            }

            try
            {
                eventModel.IsCompleted = isCompleted;

                using var scope = _serviceProvider.CreateScope();
                var goalService = scope.ServiceProvider.GetRequiredService<IGoalService>();

                var goal = await goalService.GetGoalByIdAsync(eventModel.Id);
                if (goal != null)
                {
                    bool wasCompleted = goal.IsCompleted;
                    goal.IsCompleted = isCompleted;
                    goal.Progress = isCompleted ? 100 : 0;
                    goal.UpdatedAt = DateTime.Now;

                    await goalService.UpdateGoalAsync(goal);

                    // 🎮 НАРАХОВУЄМО ДОСВІД ПРИ ВИКОНАННІ ПЛАНУ
                    if (isCompleted && !wasCompleted)
                    {
                        int oldLevel = UserLevel;  // 🔥 ЗАПАМ'ЯТОВУЄМО СТАРИЙ РІВЕНЬ

                        int expGained = await _experienceService.AwardExperienceForPlanAsync(UserId, goal.Priority);
                        System.Diagnostics.Debug.WriteLine($"✨ Отримано {expGained} досвіду за Plan (пріоритет {goal.Priority})!");

                        // Оновлюємо прогрес героя
                        await LoadUserProgress();

                        // 🔥 ПЕРЕВІРЯЄМО ЧИ ПІДВИЩИВСЯ РІВЕНЬ
                        if (UserLevel > oldLevel)
                        {
                            // Викликаємо подію для показу анімованого popup
                            System.Diagnostics.Debug.WriteLine($"🎉 Рівень підвищено! {oldLevel} -> {UserLevel}");

                            if (LevelUp != null)
                            {
                                LevelUp.Invoke(this, (UserLevel, expGained));
                            }
                            else
                            {
                                // Fallback якщо подія не підписана
                                System.Diagnostics.Debug.WriteLine("⚠️ LevelUp подія не має підписників, показуємо DisplayAlert");
                                await Application.Current?.MainPage?.DisplayAlert(
                                    "🎉 НОВИЙ РІВЕНЬ!",
                                    $"Вітаємо! Ви досягли {UserLevel} рівня!\n+{expGained} досвіду",
                                    "Чудово!");
                            }
                        }
                        else
                        {
                            await Application.Current?.MainPage?.DisplayAlert(
                                "✨ Досвід отримано!",
                                $"Ви виконали план і отримали {expGained} досвіду!\n{UserExp}/600 XP",
                                "OK");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"TogglePlanCompleted: План '{eventModel.Title}' позначено як {(isCompleted ? "виконаний" : "невиконаний")}");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPropertyChanged(nameof(SortedEvents));
                        OnPropertyChanged(nameof(Events));
                        OnPropertyChanged(nameof(CompletedTasksCount));
                        OnPropertyChanged(nameof(TotalTasksCount));
                        OnPropertyChanged(nameof(TotalWorkHours));
                        OnPropertyChanged(nameof(ProductivityPercentage));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TogglePlanCompleted: Помилка - {ex.Message}");
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
                    try
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
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Помилка збереження Pomodoro: {dbEx.Message}");
                    }
                }

                if (_completedCycles % 4 == 0)
                {
                    _remainingTime = TimeSpan.FromMinutes(15);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await Shell.Current.DisplayAlert(
                                "Чудова робота! 🎉",
                                "Ти завершив 4 цикли Помодоро! Час для довгої перерви — 15 хвилин. Відпочинь як слід!",
                                "Зрозуміло");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Помилка DisplayAlert: {ex.Message}");
                        }
                    });
                }
                else
                {
                    _remainingTime = TimeSpan.FromMinutes(5);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await Shell.Current.DisplayAlert(
                                "Відмінно! ✅",
                                "Робочу сесію завершено. Час для короткої перерви — 5 хвилин. Відпочинь трохи!",
                                "Добре");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Помилка DisplayAlert: {ex.Message}");
                        }
                    });
                }

                _isWorkSession = false;

                // АВТОМАТИЧНО ЗАПУСКАЄМО ТАЙМЕР ПЕРЕРВИ
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OnPropertyChanged(nameof(PomodoroTimeText));
                    OnPropertyChanged(nameof(TimerButtonText));
                });

                // Запускаємо таймер перерви автоматично
                _pomodoroTimer?.Start();
                IsTimerRunning = true;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OnPropertyChanged(nameof(TimerButtonText));
                });
            }
            else
            {
                // Завершили перерву, готуємось до нової робочої сесії
                _remainingTime = TimeSpan.FromMinutes(25);
                _isWorkSession = true;
                _sessionStartTime = null;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await Shell.Current.DisplayAlert(
                            "Перерва завершена! 💪",
                            "Час повертатися до роботи. Натисни кнопку Start, щоб розпочати нову робочу сесію.",
                            "Почати");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Помилка DisplayAlert: {ex.Message}");
                    }

                    // Оновлюємо UI - таймер НЕ запускається автоматично
                    OnPropertyChanged(nameof(PomodoroTimeText));
                    OnPropertyChanged(nameof(TimerButtonText));
                });

                // НЕ запускаємо таймер автоматично - чекаємо на користувача
            }
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
                            IsCompleted = goal.IsCompleted
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
                            System.Diagnostics.Debug.WriteLine($"LoadGoalsFromDatabase: Додано план '{eventModel.Title}' (Start: {eventModel.StartDateTime}, End: {eventModel.EndDateTime}, IsCompleted: {eventModel.IsCompleted})");
                        }

                        System.Diagnostics.Debug.WriteLine("LoadGoalsFromDatabase: Регенерація календаря...");

                        // Примусово оновлюємо прив'язку Events
                        OnPropertyChanged(nameof(CompletedTasksCount));
                        OnPropertyChanged(nameof(TotalTasksCount));
                        OnPropertyChanged(nameof(TotalWorkHours));
                        OnPropertyChanged(nameof(ProductivityPercentage));

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

        // TODO Methods
        private async Task LoadTodoItems()
        {
            if (_todoTaskService == null || UserId == 0)
            {
                System.Diagnostics.Debug.WriteLine("LoadTodoItems: Сервіс або UserId не доступні");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadTodoItems:  Завантаження TODO для користувача {UserId}.. .");
                var tasks = await _todoTaskService.GetTasksByUserIdAsync(UserId);

                var sortedTasks = tasks
                    .OrderBy(t => t.IsCompleted)
                    .ThenByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TodoTasksFromDb.Clear();
                    foreach (var task in sortedTasks)
                    {
                        TodoTasksFromDb.Add(task);
                        System.Diagnostics.Debug.WriteLine($"LoadTodoItems:  Додано '{task.Description}'");
                    }
                    System.Diagnostics.Debug.WriteLine($"LoadTodoItems: Завантажено {sortedTasks.Count} завдань");

                    // Оновлюємо статистику
                    OnPropertyChanged(nameof(CompletedTasksCount));
                    OnPropertyChanged(nameof(TotalTasksCount));
                    OnPropertyChanged(nameof(TotalWorkHours));
                    OnPropertyChanged(nameof(ProductivityPercentage));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading todos: {ex.Message}");
            }
        }

        private async Task OnOpenTodoPopup()
        {
            try
            {
                var popup = App.Current?.Handler?.MauiContext?.Services
                    .GetRequiredService<Views.AddTodoPopup>();
                if (popup != null)
                {
                    await Application.Current!.MainPage!.Navigation.PushModalAsync(popup);
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Помилка",
                    $"Не вдалося відкрити форму: {ex.Message}", "OK");
            }
        }

        private async Task OnToggleTodoTask(DAL.Models.ToDoTask? task)
        {
            if (task == null || _todoTaskService == null || _experienceService == null) return;

            try
            {
                bool wasCompleted = task.IsCompleted;
                task.IsCompleted = !task.IsCompleted;
                task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;

                await _todoTaskService.UpdateTaskAsync(task);

                // 🎮 НАРАХОВУЄМО ДОСВІД ПРИ ВИКОНАННІ TODO
                if (task.IsCompleted && !wasCompleted)
                {
                    int oldLevel = UserLevel;  // 🔥 ЗАПАМ'ЯТОВУЄМО СТАРИЙ РІВЕНЬ

                    int expGained = await _experienceService.AwardExperienceForTodoAsync(UserId, task.Priority);
                    System.Diagnostics.Debug.WriteLine($"✨ Отримано {expGained} досвіду за TODO (пріоритет {task.Priority})!");

                    // Оновлюємо прогрес героя
                    await LoadUserProgress();

                    // 🔥 ПЕРЕВІРЯЄМО ЧИ ПІДВИЩИВСЯ РІВЕНЬ
                    if (UserLevel > oldLevel)
                    {
                        // Викликаємо подію для показу анімованого popup
                        System.Diagnostics.Debug.WriteLine($"🎉 Рівень підвищено через TODO! {oldLevel} -> {UserLevel}");

                        if (LevelUp != null)
                        {
                            LevelUp.Invoke(this, (UserLevel, expGained));
                        }
                        else
                        {
                            // Fallback якщо подія не підписана
                            System.Diagnostics.Debug.WriteLine("⚠️ LevelUp подія не має підписників, показуємо DisplayAlert");
                            await Application.Current?.MainPage?.DisplayAlert(
                                "🎉 НОВИЙ РІВЕНЬ!",
                                $"Вітаємо! Ви досягли {UserLevel} рівня!\n+{expGained} досвіду",
                                "Чудово!");
                        }
                    }
                    else
                    {
                        await Application.Current?.MainPage?.DisplayAlert(
                            "✨ Досвід отримано!",
                            $"Ви отримали {expGained} досвіду за виконання завдання!\n{UserExp}/600 XP",
                            "OK");
                    }
                }

                await LoadTodoItems();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(CompletedTasksCount));
                    OnPropertyChanged(nameof(TotalTasksCount));
                    OnPropertyChanged(nameof(TotalWorkHours));
                    OnPropertyChanged(nameof(ProductivityPercentage));
                });
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Помилка",
                    $"Не вдалося оновити завдання: {ex.Message}", "OK");
            }
        }

        public void ForceRefreshMonthView()
        {
            if (CurrentViewMode == ViewMode.Month)
            {
                System.Diagnostics.Debug.WriteLine("ForceRefreshMonthView:  Примусове оновлення місячного вигляду");
                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(Events));
            }
        }

        /// <summary>
        /// Завантажує прогрес користувача (рівень, досвід)
        /// </summary>
        /// <summary>
        /// Завантажує прогрес користувача (рівень, досвід)
        /// </summary>
        private async Task LoadUserProgress()
        {
            if (_experienceService == null || UserId == 0)
            {
                System.Diagnostics.Debug.WriteLine("LoadUserProgress: Сервіс або UserId не доступні");
                return;
            }

            try
            {
                // 🔥 ТЕПЕР ОТРИМУЄМО 4 ЗНАЧЕННЯ
                var (totalExp, level, expInCurrentLevel, expToNextLevel) = await _experienceService.GetUserProgressAsync(UserId);

                // 🔥 ДОДАЙ ЦІ ЛОГИ ДЛЯ ДЕБАГУ
                System.Diagnostics.Debug.WriteLine($"=== DEBUG LoadUserProgress ===");
                System.Diagnostics.Debug.WriteLine($"UserId: {UserId}");
                System.Diagnostics.Debug.WriteLine($"TotalExp з БД: {totalExp}");
                System.Diagnostics.Debug.WriteLine($"Level з БД: {level}");
                System.Diagnostics.Debug.WriteLine($"ExpInCurrentLevel: {expInCurrentLevel}");
                System.Diagnostics.Debug.WriteLine($"Поточний UserLevel (до оновлення): {UserLevel}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UserLevel = level;
                    UserExp = expInCurrentLevel;  // 🔥 ВИКОРИСТОВУЄМО expInCurrentLevel (завжди 0-599)
                    UserExpToNextLevel = 600;     // 🔥 ЗАВЖДИ 600! 

                    OnPropertyChanged(nameof(UserExpProgressText));
                    OnPropertyChanged(nameof(UserProgressPercent));

                    System.Diagnostics.Debug.WriteLine($"✅ Прогрес завантажено:  Рівень {level}, Досвід {expInCurrentLevel}/600 ({UserProgressPercent:P0}), Всього: {totalExp} XP");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка завантаження прогресу: {ex.Message}");
            }
        }
    }
}
