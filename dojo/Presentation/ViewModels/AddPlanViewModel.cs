using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;

namespace Presentation.ViewModels
{
    public class AddPlanViewModel : BaseViewModel
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate = DateTime.Today;
        private TimeSpan _startTime = new TimeSpan(9, 0, 0);
        private DateTime _endDate = DateTime.Today;
        private TimeSpan _endTime = new TimeSpan(10, 0, 0);
        private EventPriority _priority = EventPriority.Normal;
        private string _attachmentPath = string.Empty;
        
        private string _titleError = string.Empty;
        private string _dateError = string.Empty;

        public AddPlanViewModel()
        {
            SaveCommand = new AsyncRelayCommand(OnSave, CanSave);
            CancelCommand = new RelayCommand(OnCancel);
            AttachFileCommand = new AsyncRelayCommand(OnAttachFile);
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
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    ValidateDates();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public EventPriority Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
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

            var startDateTime = StartDate.Date + StartTime;
            var endDateTime = EndDate.Date + EndTime;

            var newEvent = new EventModel
            {
                Title = Title,
                Description = Description,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Priority = Priority,
                Color = Priority switch
                {
                    EventPriority.High => Colors.Red,
                    EventPriority.Normal => Colors.Blue,
                    EventPriority.Low => Colors.Green,
                    _ => Colors.Blue
                }
            };

            // TODO: Save to database via service

            await Shell.Current.GoToAsync("..");
        }

        private void OnCancel()
        {
            Shell.Current.GoToAsync("..");
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
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося вибрати файл: {ex.Message}", "OK");
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

