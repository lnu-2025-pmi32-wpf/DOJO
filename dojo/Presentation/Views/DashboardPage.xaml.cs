using Presentation.ViewModels;
using Presentation.Models;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Перезавантажуємо дані при поверненні на сторінку
            var viewModel = BindingContext as MainViewModel;
            viewModel?.ReloadGoals();
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

        private void OnCalendarDaySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = BindingContext as MainViewModel;
            if (viewModel != null && e.CurrentSelection.Count > 0)
            {
                var selectedDay = e.CurrentSelection[0] as CalendarDayModel;
                if (selectedDay != null && selectedDay.IsCurrentMonth)
                {
                    viewModel.SelectedDate = selectedDay.Date;
                }
            }
        }
    }
}
