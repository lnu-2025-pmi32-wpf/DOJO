using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            
            // Отримуємо email користувача з Preferences
            var userEmail = Preferences.Get("UserEmail", string.Empty);
            if (!string.IsNullOrEmpty(userEmail))
            {
                viewModel.SetUserEmail(userEmail);
            }
            
            BindingContext = viewModel;
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null)
            {
                var action = await DisplayActionSheet(
                    "Профіль користувача", 
                    "Скасувати", 
                    "Вийти", 
                    $"Email: {viewModel.UserEmail}");

                if (action == "Вийти")
                {
                    // Очищаємо збережений email
                    Preferences.Remove("UserEmail");
                    
                    // Logout logic
                    var window = Application.Current?.Windows[0];
                    if (window != null)
                    {
                        var loginPage = Application.Current?.Handler?.MauiContext?.Services
                            .GetService<LoginPage>();
                        if (loginPage != null)
                        {
                            window.Page = loginPage;
                        }
                    }
                }
            }
        }
    }
}

