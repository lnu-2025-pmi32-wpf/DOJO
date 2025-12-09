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
            try
            {
                System.Diagnostics.Debug.WriteLine("DashboardPage: Початок конструктора");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("DashboardPage: InitializeComponent завершено");
                
                _viewModel = viewModel;
                BindingContext = _viewModel;
                System.Diagnostics.Debug.WriteLine("DashboardPage: BindingContext встановлено");
                
                // Підписуємося на події контролів
                if (DaySchedule != null)
                {
                    DaySchedule.EventTapped += OnEventTapped;
                    System.Diagnostics.Debug.WriteLine("DashboardPage: DaySchedule підписано");
                }
                if (WeekSchedule != null)
                {
                    WeekSchedule.DayTapped += OnDayTappedInCalendar;
                    WeekSchedule.EventTapped += OnEventTapped;
                    System.Diagnostics.Debug.WriteLine("DashboardPage: WeekSchedule підписано");
                }
                if (MonthView != null)
                {
                    MonthView.DayTapped += OnDayTappedInCalendar;
                    MonthView.EventTapped += OnEventTapped;
                    System.Diagnostics.Debug.WriteLine("DashboardPage: MonthView підписано");
                }
                
                // Підписуємося на повідомлення про оновлення планів
                MessagingCenter.Subscribe<AddPlanViewModel>(this, "GoalAdded", (sender) =>
                {
                    System.Diagnostics.Debug.WriteLine("DashboardPage: Отримано GoalAdded, оновлюємо дані");
                    try
                    {
                        _viewModel?.RefreshData();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DashboardPage: Помилка при RefreshData після GoalAdded - {ex.Message}");
                    }
                });
                
                MessagingCenter.Subscribe<AddPlanViewModel>(this, "GoalUpdated", (sender) =>
                {
                    System.Diagnostics.Debug.WriteLine("DashboardPage: Отримано GoalUpdated, оновлюємо дані");
                    try
                    {
                        _viewModel?.RefreshData();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DashboardPage: Помилка при RefreshData після GoalUpdated - {ex.Message}");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine("DashboardPage: Конструктор завершено успішно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DashboardPage: КРИТИЧНА ПОМИЛКА в конструкторі - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"DashboardPage: OnAppearing викликано (IsInitialized: {_isInitialized})");
                
                // Ініціалізуємо тільки один раз при першому відображенні
                if (!_isInitialized && _viewModel != null)
                {
                    _isInitialized = true;
                    System.Diagnostics.Debug.WriteLine("DashboardPage: Запускаємо ініціалізацію...");
                    _viewModel.Initialize();
                    System.Diagnostics.Debug.WriteLine("DashboardPage: Ініціалізацію завершено");
                }
                else if (_isInitialized && _viewModel != null)
                {
                    // Оновлюємо дані при поверненні на сторінку
                    System.Diagnostics.Debug.WriteLine("DashboardPage: Оновлюємо дані при поверненні...");
                    _viewModel.RefreshData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DashboardPage: Помилка OnAppearing - {ex.Message}");
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

        private void OnDayTappedInCalendar(object? sender, DateTime selectedDate)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedDate = selectedDate;
                _viewModel.CurrentViewMode = ViewMode.Day;
            }
        }

        private async void OnEventTapped(object? sender, EventModel eventModel)
        {
            if (eventModel == null) return;

            // Створюємо ViewPlanViewModel та завантажуємо дані події
            var viewPlanViewModel = new ViewPlanViewModel();
            viewPlanViewModel.LoadEvent(eventModel);

            // Створюємо сторінку та передаємо ViewModel
            var viewPlanPage = new ViewPlanPage(viewPlanViewModel);

            // Відкриваємо сторінку
            await Navigation.PushAsync(viewPlanPage);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Відписуємося від повідомлень
            MessagingCenter.Unsubscribe<AddPlanViewModel>(this, "GoalAdded");
            MessagingCenter.Unsubscribe<AddPlanViewModel>(this, "GoalUpdated");
        }
    }
}
