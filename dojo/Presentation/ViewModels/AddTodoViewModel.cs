using System.Windows.Input;
using Presentation.Helpers;
using BLL.Interfaces;
using DAL.Models;

namespace Presentation.ViewModels
{
    public class AddTodoViewModel : BaseViewModel
    {
        private readonly IToDoTaskService _todoTaskService;
        private readonly ISessionService _sessionService;
        private string _title = string.Empty;
        private string _titleError = string.Empty;
        private string _description = string.Empty;
        private string _descriptionError = string.Empty;
        private int _priority = 1; 

        public AddTodoViewModel(IToDoTaskService todoTaskService, ISessionService sessionService)
        {
            _todoTaskService = todoTaskService;
            _sessionService = sessionService;
            
            SaveCommand = new AsyncRelayCommand(OnSave, CanSave);
            CloseCommand = new RelayCommand(OnClose);
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

        public string TitleError
        {
            get => _titleError;
            set => SetProperty(ref _titleError, value);
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    ValidateDescription();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Title) && 
                   ! string.IsNullOrWhiteSpace(Description) && 
                   string.IsNullOrEmpty(TitleError) &&
                   string.IsNullOrEmpty(DescriptionError);
        }

        private void ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleError = "Назва завдання не може бути порожньою";
            }
            else if (Title.Length < 3)
            {
                TitleError = "Назва має містити принаймні 3 символи";
            }
            else
            {
                TitleError = string.Empty;
            }
        }

        private void ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                DescriptionError = "Опис завдання не може бути порожнім";
            }
            else if (Description.Length < 3)
            {
                DescriptionError = "Опис має містити принаймні 3 символи";
            }
            else
            {
                DescriptionError = string.Empty;
            }
        }

        private async Task OnSave()
        {
            try
            {
                var session = await _sessionService.GetUserSessionAsync();
                if (session == null)
                {
                    await Application.Current! .MainPage!.DisplayAlert("Помилка", "Сесія користувача не знайдена", "OK");
                    return;
                }

                var newTask = new ToDoTask
                {
                    UserId = session.Value.UserId,
                    Description = $"{Title. Trim()}\n{Description.Trim()}",
                    Priority = Priority,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _todoTaskService.AddTaskAsync(newTask);
                
               
                MessagingCenter.Send(this, "TodoAdded");
                await Application.Current!.MainPage!. Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await Application. Current!.MainPage!.DisplayAlert("Помилка", $"Не вдалося додати завдання: {ex. Message}", "OK");
            }
        }

        private async void OnClose()
        {
            await Application.Current!.MainPage!.Navigation.PopModalAsync();
        }
    }
}