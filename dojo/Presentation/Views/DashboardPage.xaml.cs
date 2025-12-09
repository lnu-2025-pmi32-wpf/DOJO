using Presentation.ViewModels;
using Presentation. Models;
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
                System.Diagnostics. Debug.WriteLine("🔹 DashboardPage:  Початок конструктора");
                InitializeComponent();
                System.Diagnostics. Debug.WriteLine("✅ DashboardPage: InitializeComponent завершено");
                
                _viewModel = viewModel ??  throw new ArgumentNullException(nameof(viewModel));
                BindingContext = _viewModel;
                System.Diagnostics. Debug.WriteLine("✅ DashboardPage: BindingContext встановлено");
                
                // ✅ ВИПРАВЛЕНО: Підписуємося на події контролів з перевіркою null
                SubscribeToControlEvents();
                
                System.Diagnostics.Debug.WriteLine("✅ DashboardPage:  Конструктор завершено успішно");
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug. WriteLine($"❌ DashboardPage:  КРИТИЧНА ПОМИЛКА в конструкторі - {ex.Message}");
                System.Diagnostics. Debug.WriteLine($"Stack:  {ex.StackTrace}");
                throw;
            }
        }

        // ✅ НОВИЙ МЕТОД:  Винесено підписки в окремий метод
        private void SubscribeToControlEvents()
        {
            try
            {
                if (DaySchedule != null)
                {
                    DaySchedule.EventTapped += OnEventTapped;
                    System.Diagnostics. Debug.WriteLine("✅ DashboardPage: DaySchedule підписано");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ DashboardPage:  DaySchedule is null");
                }
                
                if (WeekSchedule != null)
                {
                    WeekSchedule.DayTapped += OnDayTappedInCalendar;
                    WeekSchedule.EventTapped += OnEventTapped;
                    System.Diagnostics.Debug.WriteLine("✅ DashboardPage: WeekSchedule підписано");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ DashboardPage:  WeekSchedule is null");
                }
                
                if (MonthView != null)
                {
                    MonthView.DayTapped += OnDayTappedInCalendar;
                    MonthView.EventTapped += OnEventTapped;
                    System. Diagnostics.Debug.WriteLine("✅ DashboardPage: MonthView підписано");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ DashboardPage:  MonthView is null");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics. Debug.WriteLine($"❌ SubscribeToControlEvents error: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔹 DashboardPage:  OnAppearing (IsInitialized:  {_isInitialized})");
                
                if (_viewModel == null)
                {
                    System. Diagnostics.Debug.WriteLine("❌ DashboardPage: ViewModel is null!");
                    return;
                }
                
                // Ініціалізуємо тільки один раз при першому відображенні
                if (! _isInitialized)
                {
                    _isInitialized = true;
                    System. Diagnostics.Debug. WriteLine("🔹 DashboardPage: Запускаємо ініціалізацію.. .");
                    
                    // ✅ ВИПРАВЛЕНО:  Запускаємо з невеликою затримкою для стабільності UI
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await Task.Delay(50); // Невелика затримка для завершення рендерингу
                            _viewModel.Initialize();
                            System. Diagnostics.Debug.WriteLine("✅ DashboardPage: Ініціалізацію завершено");
                        }
                        catch (Exception initEx)
                        {
                            System.Diagnostics. Debug.WriteLine($"❌ DashboardPage Initialize error: {initEx.Message}");
                        }
                    });
                }
                else
                {
                    // Оновлюємо дані при поверненні на сторінку
                    System. Diagnostics.Debug.WriteLine("🔹 DashboardPage: Оновлюємо дані при поверненні...");
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            _viewModel.RefreshData();
                        }
                        catch (Exception refreshEx)
                        {
                            System. Diagnostics.Debug. WriteLine($"❌ DashboardPage RefreshData error:  {refreshEx.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ DashboardPage OnAppearing error: {ex.Message}");
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel == null) return;
                
                var action = await DisplayActionSheet(
                    $"👤 {_viewModel.UserName}", 
                    "Скасувати", 
                    "Вийти", 
                    $"📧 {_viewModel.UserEmail}");

                if (action == "Вийти")
                {
                    if (_viewModel.LogoutCommand.CanExecute(null))
                    {
                        _viewModel.LogoutCommand. Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ OnProfileTapped error:  {ex.Message}");
            }
        }

        private void OnCalendarDaySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null && e. CurrentSelection?. Count > 0)
                {
                    var selectedDay = e.CurrentSelection[0] as CalendarDayModel;
                    if (selectedDay != null && selectedDay.IsCurrentMonth)
                    {
                        _viewModel.SelectedDate = selectedDay.Date;
                    }
                }
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ OnCalendarDaySelectionChanged error: {ex. Message}");
            }
        }

        private void OnDayTappedInCalendar(object? sender, DateTime selectedDate)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel. SelectedDate = selectedDate;
                    _viewModel.CurrentViewMode = ViewMode.Day;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnDayTappedInCalendar error: {ex. Message}");
            }
        }

        private async void OnEventTapped(object? sender, EventModel eventModel)
        {
            try
            {
                if (eventModel == null) 
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ OnEventTapped: eventModel is null");
                    return;
                }

                System.Diagnostics. Debug.WriteLine($"🔹 OnEventTapped:  {eventModel.Title}");

                // ✅ ВИПРАВЛЕНО:  Безпечне отримання сервісу
                ViewPlanViewModel? viewPlanViewModel = null;
                
                try
                {
                    viewPlanViewModel = Application. Current?.Handler?.MauiContext?.Services?. GetService<ViewPlanViewModel>();
                }
                catch (Exception serviceEx)
                {
                    System. Diagnostics.Debug.WriteLine($"❌ GetService error: {serviceEx.Message}");
                }
                
                if (viewPlanViewModel == null)
                {
                    await DisplayAlert("Помилка", "Не вдалося відкрити деталі плану", "OK");
                    return;
                }
                
                viewPlanViewModel.LoadEvent(eventModel);

                var viewPlanPage = new ViewPlanPage(viewPlanViewModel);

                await Navigation.PushAsync(viewPlanPage);
                
                System.Diagnostics. Debug.WriteLine("✅ OnEventTapped: Navigation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnEventTapped error: {ex.Message}");
                
                try
                {
                    await DisplayAlert("Помилка", "Не вдалося відкрити деталі плану", "OK");
                }
                catch
                {
                    // Ігноруємо помилку показу alert
                }
            }
        }

        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                System. Diagnostics.Debug.WriteLine("🔹 DashboardPage: OnDisappearing");
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ OnDisappearing error:  {ex.Message}");
            }
        }
    }
}