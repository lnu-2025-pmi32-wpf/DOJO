using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Presentation.Helpers;

namespace Presentation.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _fullName = string.Empty;
        private string _emailError = string.Empty;
        private string _passwordError = string.Empty;
        private string _confirmPasswordError = string.Empty;
        private string _fullNameError = string.Empty;
        private bool _isLoading;

        public RegisterViewModel()
        {
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
            if (!ValidateEmail() || !ValidatePassword() || !ValidateConfirmPassword() || !ValidateFullName())
                return;

            IsLoading = true;

            try
            {
                // TODO: Implement actual registration logic with UserService
                await Task.Delay(1500); // Simulate API call
                
                // Show success message
                var window = Application.Current?.Windows[0];
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert(
                        "Успіх", 
                        "Реєстрація пройшла успішно! Тепер ви можете увійти.", 
                        "OK");
                }
                
                // Navigate back to login
                OnBackToLogin();
            }
            catch (Exception ex)
            {
                var window = Application.Current?.Windows[0];
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert(
                        "Помилка", 
                        $"Не вдалося зареєструватися: {ex.Message}", 
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnBackToLogin()
        {
            var window = Application.Current?.Windows[0];
            if (window != null)
            {
                window.Page = new Views.LoginPage();
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
                FullNameError = "Ім'я обов'язкове";
                return false;
            }

            if (FullName.Length < 2)
            {
                FullNameError = "Ім'я має містити мінімум 2 символи";
                return false;
            }

            FullNameError = string.Empty;
            return true;
        }
    }
}

