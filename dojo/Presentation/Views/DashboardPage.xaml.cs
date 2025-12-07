using Presentation.ViewModels;
using BLL.Interfaces;

namespace Presentation.Views
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage(ISessionService sessionService)
        {
            InitializeComponent();
            var viewModel = new MainViewModel(sessionService);
            BindingContext = viewModel;
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null)
            {
                var action = await DisplayActionSheet(
                    $"👤 {viewModel.UserName}", 
                    "Скасувати", 
                    "Вийти", 
                    $"📧 {viewModel.UserEmail}");

                if (action == "Вийти")
                {
                    // Викликаємо команду logout
                    if (viewModel.LogoutCommand.CanExecute(null))
                    {
                        viewModel.LogoutCommand.Execute(null);
                    }
                }
            }
        }
    }
}

