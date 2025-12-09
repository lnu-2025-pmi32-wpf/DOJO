using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;
using BLL.Services;
using BLL.Interfaces;
using DAL.Models;

namespace Presentation.ViewModels
{
    public class AddPlanViewModel : BaseViewModel
    {
        private readonly IGoalService _goalService;
        private readonly ISessionService _sessionService;
        
        private int? _eventId = null;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate = DateTime.Today;
        private TimeSpan _startTime = new TimeSpan(9, 0, 0);
        private DateTime _endDate = DateTime.Today;
        private TimeSpan _endTime = new TimeSpan(10, 0, 0);
        private EventPriority _priority = EventPriority.Normal;
        private string _attachmentPath = string.Empty;
        private bool _isCompleted = false;
        
        private string _titleError = string.Empty;
        private string _dateError = string.Empty;

        public AddPlanViewModel(IGoalService goalService, ISessionService sessionService)
        {
            _goalService = goalService;
            _sessionService = sessionService;
            
            SaveCommand = new AsyncRelayCommand(OnSave, CanSave);
            CancelCommand = new RelayCommand(OnCancel);
            AttachFileCommand = new AsyncRelayCommand(OnAttachFile);
        }

        public bool IsEditMode => _eventId.HasValue;

        public void LoadEventForEdit(int eventId, string title, string description, 
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, 
            EventPriority priority, bool isCompleted)
        {
            _eventId = eventId;
            Title = title;
            Description = description;
            StartDate = startDate;
            StartTime = startTime;
            EndDate = endDate;
            EndTime = endTime;
            Priority = priority;
            _isCompleted = isCompleted;
            
            OnPropertyChanged(nameof(IsEditMode));
        }

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    ValidateTitle();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    OnPropertyChanged(nameof(StartDateText));
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StartDateText => _startDate.ToString("dd.MM.yyyy");

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    OnPropertyChanged(nameof(StartTimeText));
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StartTimeText => _startTime.ToString(@"hh\:mm");

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    OnPropertyChanged(nameof(EndDateText));
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string EndDateText => _endDate.ToString("dd.MM.yyyy");

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    OnPropertyChanged(nameof(EndTimeText));
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string EndTimeText => _endTime.ToString(@"hh\:mm");

        public EventPriority Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }
        
        // Для біндингу з Picker.SelectedIndex
        public int PriorityIndex
        {
            get => (int)_priority;
            set
            {
                var newPriority = (EventPriority)value;
                if (_priority != newPriority)
                {
                    _priority = newPriority;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        public string AttachmentPath
        {
            get => _attachmentPath;
            set => SetProperty(ref _attachmentPath, value);
        }

        public string TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        public string DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AttachFileCommand { get; }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   string.IsNullOrEmpty(TitleError) &&
                   string.IsNullOrEmpty(DateError);
        }

        private async Task OnSave()
        {
            if (!ValidateTitle() || !ValidateDates())
                return;

            try
            {
                // Зберігаємо заголовок для повідомлення (до очистки форми)
                var savedTitle = Title;
                
                // Отримуємо поточну сесію користувача
                var session = await _sessionService.GetUserSessionAsync();
                if (!session.HasValue)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Не вдалося визначити користувача", "OK");
                    return;
                }

                var startDateTime = StartDate.Date + StartTime;
                var endDateTime = EndDate.Date + EndTime;

                // Зберігаємо заголовок та опис окремо
                var fullDescription = string.IsNullOrEmpty(Description) ? Title : $"{Title}\n{Description}";

                if (IsEditMode && _eventId.HasValue)
                {
                    // Режим редагування - оновлюємо існуючий Goal
                    var existingGoal = await _goalService.GetGoalByIdAsync(_eventId.Value);
                    
                    if (existingGoal != null)
                    {
                        existingGoal.Description = fullDescription;
                        existingGoal.StartTime = startDateTime;
                        existingGoal.EndTime = endDateTime;
                        existingGoal.Progress = _isCompleted ? 100 : existingGoal.Progress;
                        existingGoal.Priority = PriorityIndex;
                        existingGoal.UpdatedAt = DateTime.UtcNow;

                        System.Diagnostics.Debug.WriteLine($"Оновлюємо план: Id={existingGoal.Id}, Title={savedTitle}, StartTime={startDateTime}, EndTime={endDateTime}");

                        await _goalService.UpdateGoalAsync(existingGoal);

                        System.Diagnostics.Debug.WriteLine("План успішно оновлено в БД!");

                        // Очищуємо форму
                        ClearForm();

                        // Виконуємо все в UI потоці
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            // Відправляємо повідомлення про оновлення
                            System.Diagnostics.Debug.WriteLine("Відправляємо GoalUpdated...");
                            MessagingCenter.Send(this, "GoalUpdated");
                            System.Diagnostics.Debug.WriteLine("GoalUpdated відправлено!");
                            
                            await Shell.Current.DisplayAlert(
                                "✅ Успіх", 
                                $"План '{savedTitle}' успішно оновлено!", 
                                "OK");
                            
                            await Shell.Current.Navigation.PopAsync();
                        });
                        
                        return;
                    }
                }
                else
                {
                    // Режим створення - створюємо новий Goal
                    var newGoal = new Goal
                    {
                        UserId = session.Value.UserId,
                        Description = fullDescription,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        Progress = 0,
                        Priority = PriorityIndex
                    };

                    System.Diagnostics.Debug.WriteLine($"Зберігаємо план: UserId={newGoal.UserId}, Title={savedTitle}, StartTime={startDateTime}, EndTime={endDateTime}");

                    await _goalService.AddGoalAsync(newGoal);

                    System.Diagnostics.Debug.WriteLine("План успішно збережено в БД!");

                    // Очищуємо форму
                    ClearForm();

                    // Виконуємо все в UI потоці
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // Відправляємо повідомлення про додавання
                        System.Diagnostics.Debug.WriteLine("Відправляємо GoalAdded...");
                        MessagingCenter.Send(this, "GoalAdded");
                        System.Diagnostics.Debug.WriteLine("GoalAdded відправлено!");
                        
                        await Shell.Current.DisplayAlert(
                            "✅ Успіх", 
                            $"План '{savedTitle}' успішно створено!", 
                            "OK");
                        
                        await Shell.Current.Navigation.PopAsync();
                    });
                    
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка збереження: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Помилка", 
                        $"Не вдалося зберегти план:\n{errorMessage}", 
                        "OK");
                });
            }
        }

        private void OnCancel()
        {
            ClearForm();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NavigateBack();
            });
        }

        private async void NavigateBack()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void ClearForm()
        {
            _eventId = null;
            Title = string.Empty;
            Description = string.Empty;
            StartDate = DateTime.Today;
            StartTime = new TimeSpan(9, 0, 0);
            EndDate = DateTime.Today;
            EndTime = new TimeSpan(10, 0, 0);
            Priority = EventPriority.Normal;
            _isCompleted = false;
            AttachmentPath = string.Empty;
            
            OnPropertyChanged(nameof(IsEditMode));
        }

        private async Task OnAttachFile()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Виберіть файл"
                });

                if (result != null)
                {
                    AttachmentPath = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Помилка", $"Не вдалося вибрати файл: {ex.Message}", "OK");
                });
            }
        }

        private bool ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Назва обов'язкова";
                return false;
            }

            if (Title.Length < 2)
            {
                TitleError = "Назва має містити мінімум 2 символи";
                return false;
            }

            if (Title.Length > 200)
            {
                TitleError = "Назва не може перевищувати 200 символів";
                return false;
            }

            TitleError = string.Empty;
            return true;
        }

        private bool ValidateDates()
        {
            var startDateTime = StartDate.Date + StartTime;
            var endDateTime = EndDate.Date + EndTime;

            if (endDateTime <= startDateTime)
            {
                DateError = "Час завершення має бути пізніше часу початку";
                return false;
            }

            var duration = endDateTime - startDateTime;
            if (duration.TotalMinutes < 5)
            {
                DateError = "Мінімальна тривалість події - 5 хвилин";
                return false;
            }

            if (duration.TotalDays > 30)
            {
                DateError = "Максимальна тривалість події - 30 днів";
                return false;
            }

            DateError = string.Empty;
            return true;
        }
    }
}

