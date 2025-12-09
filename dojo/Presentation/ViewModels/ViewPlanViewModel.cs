using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Views;

namespace Presentation.ViewModels
{
    public class ViewPlanViewModel : INotifyPropertyChanged
    {
        private int _eventId;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate;
        private TimeSpan _startTime;
        private DateTime _endDate;
        private TimeSpan _endTime;
        private EventPriority _priority;
        private bool _isCompleted;
        private ObservableCollection<AttachmentModel> _attachments = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int EventId
        {
            get => _eventId;
            set => SetProperty(ref _eventId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public EventPriority Priority
        {
            get => _priority;
            set
            {
                if (SetProperty(ref _priority, value))
                {
                    OnPropertyChanged(nameof(PriorityText));
                    OnPropertyChanged(nameof(PriorityColor));
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public ObservableCollection<AttachmentModel> Attachments
        {
            get => _attachments;
            set
            {
                if (SetProperty(ref _attachments, value))
                {
                    OnPropertyChanged(nameof(HasAttachments));
                }
            }
        }

        public string PriorityText => Priority switch
        {
            EventPriority.Low => "Низький",
            EventPriority.Normal => "Нормальний",
            EventPriority.High => "Високий",
            _ => "Нормальний"
        };

        public Color PriorityColor => Priority switch
        {
            EventPriority.Low => Colors.Green,
            EventPriority.Normal => Colors.Blue,
            EventPriority.High => Colors.Red,
            _ => Colors.Blue
        };

        public string StatusText => IsCompleted ? "Завершено" : "Не завершено";

        public bool HasAttachments => Attachments?.Count > 0;

        public ICommand EditCommand { get; }
        public ICommand CloseCommand { get; }

        public ViewPlanViewModel()
        {
            EditCommand = new RelayCommand(OnEdit);
            CloseCommand = new RelayCommand(OnClose);
        }

        public void LoadEvent(EventModel eventModel)
        {
            EventId = eventModel.Id;
            Title = eventModel.Title;
            Description = eventModel.Description;
            StartDate = eventModel.StartDateTime.Date;
            StartTime = eventModel.StartDateTime.TimeOfDay;
            EndDate = eventModel.EndDateTime.Date;
            EndTime = eventModel.EndDateTime.TimeOfDay;
            Priority = eventModel.Priority;
            IsCompleted = eventModel.IsCompleted;
            
            // TODO: Load attachments from database when available
            Attachments = new ObservableCollection<AttachmentModel>();
        }

        private async void OnEdit()
        {
            // Отримуємо AddPlanPage з DI контейнера
            var addPlanPage = Application.Current?.Handler?.MauiContext?.Services.GetService<AddPlanPage>();
            
            if (addPlanPage != null && addPlanPage.BindingContext is AddPlanViewModel addPlanViewModel)
            {
                // Завантажуємо дані для редагування
                addPlanViewModel.LoadEventForEdit(EventId, Title, Description, 
                    StartDate, StartTime, EndDate, EndTime, Priority, IsCompleted);
                
                // Переходимо на сторінку редагування
                await Shell.Current.Navigation.PopAsync(); // Закриваємо ViewPlanPage
                await Shell.Current.Navigation.PushAsync(addPlanPage); // Відкриваємо AddPlanPage
            }
        }

        private async void OnClose()
        {
            await Shell.Current.GoToAsync("..");
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AttachmentModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}
