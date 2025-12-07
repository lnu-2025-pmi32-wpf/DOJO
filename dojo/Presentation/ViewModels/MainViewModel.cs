using System.Collections.ObjectModel;
using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;
using BLL.Interfaces;

namespace Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ISessionService? _sessionService;
        private ViewMode _currentViewMode = ViewMode.Week;
        private DateTime _selectedDate = DateTime.Today;
        private EventModel? _selectedEvent;
        private string _userEmail = "user@example.com";
        private string _userName = "Користувач";
        private string _userInitials = "U";
        private int _userId;

        public MainViewModel(ISessionService? sessionService = null)
        {
            _sessionService = sessionService;
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
            
            LoadSampleData();
            LoadUserSessionAsync();
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

            // Sample todos
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
            
            // Використовуємо username якщо він є, інакше беремо частину email
            if (!string.IsNullOrEmpty(username))
            {
                UserName = username;
                
                // Обчислюємо ініціали з username
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
                // Якщо username немає, використовуємо email
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

        private async void LoadUserSessionAsync()
        {
            if (_sessionService != null)
            {
                var session = await _sessionService.GetUserSessionAsync();
                if (session.HasValue)
                {
                    SetUserInfo(session.Value.Email, session.Value.Username);
                    UserId = session.Value.UserId;
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
    }
}

