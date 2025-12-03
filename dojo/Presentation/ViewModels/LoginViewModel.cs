using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Presentation.Helpers;

namespace Presentation.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _emailError = string.Empty;
        private string _passwordError = string.Empty;
        private bool _isLoading;

        public LoginViewModel()
        {
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
            if (!ValidateEmail() || !ValidatePassword())
                return;

            IsLoading = true;

            try
            {
                // TODO: Implement actual login logic with UserService
                await Task.Delay(1000); // Simulate API call
                
                // Navigate to dashboard page - replace the window content
                var window = Application.Current?.Windows[0];
                if (window != null)
                {
                    window.Page = new AppShell();
                }
            }
            catch (Exception ex)
            {
                var window = Application.Current?.Windows[0];
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert("Помилка", $"Не вдалося увійти: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnRegister()
        {
            var window = Application.Current?.Windows[0];
            if (window != null)
            {
                window.Page = new Views.RegisterPage();
            }
        }

        private void OnForgotPassword()
        {
            var window = Application.Current?.Windows[0];
            if (window?.Page != null)
            {
                window.Page.DisplayAlert("Відновлення паролю", "Функція відновлення паролю буде додана найближчим часом", "OK");
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

            if (Password.Length < 8)
            {
                PasswordError = "Пароль має містити мінімум 8 символів";
                return false;
            }

            if (!Password.Any(char.IsUpper) || !Password.Any(char.IsLower) || 
                !Password.Any(char.IsDigit) || !Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                PasswordError = "Пароль має містити великі та малі літери, цифри та спеціальний символ";
                return false;
            }

            PasswordError = string.Empty;
            return true;
        }
    }
}

