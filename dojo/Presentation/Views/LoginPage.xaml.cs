using Presentation.ViewModels;
using BLL.Interfaces;

namespace Presentation.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ISessionService _sessionService;
        private bool _hasCheckedSession = false;

        public LoginPage(LoginViewModel viewModel, ISessionService sessionService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _sessionService = sessionService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Перевіряємо сесію тільки один раз при першому відображенні
            if (!_hasCheckedSession)
            {
                _hasCheckedSession = true;
                
                try
                {
                    var isLoggedIn = await _sessionService.IsLoggedInAsync();
                    
                    if (isLoggedIn)
                    {
                        // Якщо вже залогінений, переходимо на Dashboard
                        await Shell.Current.GoToAsync($"/{nameof(DashboardPage)}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error checking session: {ex.Message}");
                    // Якщо помилка - просто залишаємось на LoginPage
                }
            }
        }
    }
}

