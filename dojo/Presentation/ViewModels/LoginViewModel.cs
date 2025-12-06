using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Presentation.Helpers;
using BLL.Interfaces;

namespace Presentation.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _emailError = string.Empty;
        private string _passwordError = string.Empty;
        private bool _isLoading;
        private string _notificationMessage = string.Empty;
        private bool _isNotificationSuccess;

        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
            LoginCommand = new AsyncRelayCommand(OnLogin, CanLogin);
            RegisterCommand = new RelayCommand(OnRegister);
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ValidatePassword();
                    (LoginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string NotificationMessage
        {
            get => _notificationMessage;
            set => SetProperty(ref _notificationMessage, value);
        }

        public bool IsNotificationSuccess
        {
            get => _isNotificationSuccess;
            set => SetProperty(ref _isNotificationSuccess, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   string.IsNullOrEmpty(EmailError) &&
                   string.IsNullOrEmpty(PasswordError) &&
                   !IsLoading;
        }

        private async Task OnLogin()
        {
            // Перевіряємо чи всі поля заповнені
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password))
            {
                NotificationMessage = "Будь ласка, заповніть всі поля";
                IsNotificationSuccess = false;
                return;
            }

            if (!ValidateEmail() || !ValidatePassword())
                return;

            IsLoading = true;
            NotificationMessage = string.Empty;

            try
            {
                // Викликаємо LoginAsync
                var user = await _userService.LoginAsync(Email, Password);
                
                if (user == null)
                {
                    // Невірний email або пароль
                    NotificationMessage = "Невірний email або пароль";
                    IsNotificationSuccess = false;
                    return;
                }
                
                // Успішний вхід
                NotificationMessage = "Вхід виконано успішно!";
                IsNotificationSuccess = true;

                // Зберігаємо email користувача
                Preferences.Set("UserEmail", Email);

                // Невелика затримка щоб показати повідомлення
                await Task.Delay(1000);
                
                // Navigate to dashboard page
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var window = Application.Current?.Windows[0];
                    if (window != null)
                    {
                        window.Page = new AppShell();
                    }
                });
            }
            catch (Exception ex)
            {
                NotificationMessage = $"Помилка: {ex.Message}";
                IsNotificationSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnRegister()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var window = Application.Current?.Windows[0];
                if (window != null)
                {
                    var registerPage = Application.Current?.Handler?.MauiContext?.Services
                        .GetService<Views.RegisterPage>();
                    if (registerPage != null)
                    {
                        window.Page = registerPage;
                    }
                }
            });
        }

        private void OnForgotPassword()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ShowForgotPasswordAlert();
            });
        }

        private async void ShowForgotPasswordAlert()
        {
            var window = Application.Current?.Windows[0];
            if (window?.Page != null)
            {
                await window.Page.DisplayAlert("Відновлення паролю", "Функція відновлення паролю буде додана найближчим часом", "OK");
            }
        }

        private bool ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email обов'язковий";
                return false;
            }

            if (!new EmailAddressAttribute().IsValid(Email))
            {
                EmailError = "Невірний формат email";
                return false;
            }

            EmailError = string.Empty;
            return true;
        }

        private bool ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Пароль обов'язковий";
                return false;
            }

            if (Password.Length < 6)
            {
                PasswordError = "Пароль має містити мінімум 6 символів";
                return false;
            }

            PasswordError = string.Empty;
            return true;
        }
    }
}

