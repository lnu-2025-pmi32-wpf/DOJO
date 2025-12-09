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
            System.Diagnostics.Debug.WriteLine("✅ LoginPage created");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            System.Diagnostics.Debug.WriteLine("🔹 LoginPage OnAppearing");
            
            // АВТОЛОГІН: перевіряємо сесію тільки один раз при першому відображенні
            if (!_hasCheckedSession)
            {
                _hasCheckedSession = true;
                try
                {
                    var isLoggedIn = await _sessionService.IsLoggedInAsync();
                    System.Diagnostics.Debug.WriteLine($"🔹 IsLoggedIn: {isLoggedIn}");
                    if (isLoggedIn)
                    {
                        System.Diagnostics.Debug.WriteLine("🔹 User is logged in, navigating to Dashboard");
                        // Якщо вже залогінений, переходимо на Dashboard
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            var appShell = Application.Current?.Handler?.MauiContext?.Services.GetService<AppShell>();
                            if (appShell != null && Application.Current?.Windows.Count > 0)
                            {
                                Application.Current.Windows[0].Page = appShell;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error checking session: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                    // Якщо помилка - просто залишаємось на LoginPage
                }
            }
            
            // Очищаємо стару сесію
            try
            {
                await _sessionService.ClearSessionAsync();
                System.Diagnostics.Debug.WriteLine("✅ Session cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error clearing session: {ex.Message}");
            }
        }
    }
}

