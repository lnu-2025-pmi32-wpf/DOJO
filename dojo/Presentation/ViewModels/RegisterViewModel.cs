using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Presentation.Helpers;
using BLL.Interfaces;
using Presentation.Views;

namespace Presentation.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _fullName = string.Empty;
        private string _emailError = string.Empty;
        private string _passwordError = string.Empty;
        private string _confirmPasswordError = string.Empty;
        private string _fullNameError = string.Empty;
        private bool _isLoading;
        private string _notificationMessage = string.Empty;
        private bool _isNotificationSuccess;

        public RegisterViewModel(IUserService userService, ISessionService sessionService)
        {
            _userService = userService;
            _sessionService = sessionService;
            RegisterCommand = new AsyncRelayCommand(OnRegister, CanRegister);
            BackToLoginCommand = new RelayCommand(OnBackToLogin);
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    (RegisterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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
                    ValidateConfirmPassword();
                    (RegisterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ValidateConfirmPassword();
                    (RegisterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ValidateFullName();
                    (RegisterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
        }

        public string FullNameError
        {
            get => _fullNameError;
            set => SetProperty(ref _fullNameError, value);
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

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   string.IsNullOrEmpty(EmailError) &&
                   string.IsNullOrEmpty(PasswordError) &&
                   string.IsNullOrEmpty(ConfirmPasswordError) &&
                   string.IsNullOrEmpty(FullNameError) &&
                   !IsLoading;
        }

        private async Task OnRegister()
        {
            // Перевіряємо чи всі поля заповнені
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || 
                string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(FullName))
            {
                NotificationMessage = "Будь ласка, заповніть всі поля";
                IsNotificationSuccess = false;
                return;
            }

            if (!ValidateEmail() || !ValidatePassword() || !ValidateConfirmPassword() || !ValidateFullName())
                return;

            IsLoading = true;
            NotificationMessage = string.Empty;

            try
            {
                // Викликаємо RegisterAsync з username
                var user = await _userService.RegisterAsync(Email, Password, FullName);
                
                if (user == null)
                {
                    // Користувач з таким email вже існує
                    NotificationMessage = "Користувач з таким email вже зареєстрований";
                    IsNotificationSuccess = false;
                    return;
                }
                
                // Успішна реєстрація
                NotificationMessage = "Реєстрація пройшла успішно! Вхід в систему...";
                IsNotificationSuccess = true;

                // Зберігаємо сесію користувача
                await _sessionService.SaveUserSessionAsync(user.Email, user.Id, user.Username);

                // Затримка щоб показати повідомлення
                await Task.Delay(500);
                
                // Встановлюємо IsLoading = false перед навігацією
                IsLoading = false;
                
                // Перехід на Dashboard - простий підхід як в LoginPage
                var window = Application.Current?.Windows[0];
                if (window != null)
                {
                    window.Page = new AppShell();
                    // Даємо час Shell ініціалізуватися
                    await Task.Delay(100);
                    await Shell.Current.GoToAsync($"///{nameof(DashboardPage)}");
                }
                return; // Виходимо щоб не виконувати finally IsLoading = false
            }
            catch (Exception ex)
            {
                NotificationMessage = $"Помилка: {ex.Message}";
                IsNotificationSuccess = false;
                System.Diagnostics.Debug.WriteLine($"RegisterViewModel Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnBackToLogin()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var window = Application.Current?.Windows[0];
                if (window != null)
                {
                    var loginPage = Application.Current?.Handler?.MauiContext?.Services
                        .GetService<Views.LoginPage>();
                    if (loginPage != null)
                    {
                        window.Page = loginPage;
                    }
                }
            });
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


        private bool ValidateConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Підтвердження паролю обов'язкове";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Паролі не співпадають";
                return false;
            }

            ConfirmPasswordError = string.Empty;
            return true;
        }

        private bool ValidateFullName()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                FullNameError = "Username обов'язковий";
                return false;
            }

            if (FullName.Length < 2)
            {
                FullNameError = "Username має містити мінімум 2 символи";
                return false;
            }

            FullNameError = string.Empty;
            return true;
        }
    }
}

