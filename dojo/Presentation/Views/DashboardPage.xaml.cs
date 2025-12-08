using Presentation.ViewModels;
using Presentation.Models;
using BLL.Interfaces;

namespace Presentation.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _isInitialized = false;

        public DashboardPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            System.Diagnostics.Debug.WriteLine("DashboardPage: Створено");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            System.Diagnostics.Debug.WriteLine($"DashboardPage: OnAppearing викликано (IsInitialized: {_isInitialized})");
            
            // Ініціалізуємо тільки один раз при першому відображенні
            if (!_isInitialized && _viewModel != null)
            {
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("DashboardPage: Запускаємо ініціалізацію...");
                _viewModel.Initialize();
                System.Diagnostics.Debug.WriteLine("DashboardPage: Ініціалізацію запущено");
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var action = await DisplayActionSheet(
                    $"👤 {_viewModel.UserName}", 
                    "Скасувати", 
                    "Вийти", 
                    $"📧 {_viewModel.UserEmail}");

                if (action == "Вийти")
                {
                    // Викликаємо команду logout
                    if (_viewModel.LogoutCommand.CanExecute(null))
                    {
                        _viewModel.LogoutCommand.Execute(null);
                    }
                }
                // Якщо користувач вибрав email - нічого не робимо, просто показуємо інформацію
            }
        }

        private void OnCalendarDaySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.CurrentSelection.Count > 0)
            {
                var selectedDay = e.CurrentSelection[0] as CalendarDayModel;
                if (selectedDay != null && selectedDay.IsCurrentMonth)
                {
                    _viewModel.SelectedDate = selectedDay.Date;
                }
            }
        }
    }
}
