using System. Windows.Input;
using BLL. Interfaces;
using Presentation. Helpers;

namespace Presentation.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IToDoTaskService?   _todoTaskService;
        private readonly ISessionService?  _sessionService;
        
        private int _totalTasks;
        private int _completedTasks;
        private int _inProgressTasks;
        private double _productivityPercent;
        private int _userId;

        public StatisticsViewModel(IToDoTaskService?   todoTaskService = null, ISessionService?  sessionService = null)
        {
            _todoTaskService = todoTaskService;
            _sessionService = sessionService;
            
            RefreshCommand = new AsyncRelayCommand(LoadStatistics);
            BackCommand = new AsyncRelayCommand(OnBack);
            
            _ = InitializeAsync();
        }

        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }

        public int CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        public int InProgressTasks
        {
            get => _inProgressTasks;
            set => SetProperty(ref _inProgressTasks, value);
        }

        public double ProductivityPercent
        {
            get => _productivityPercent;
            set => SetProperty(ref _productivityPercent, value);
        }

        // Computed properties
        public string ProductivityText => $"{ProductivityPercent: F0}%";
        public string CompletionRate => TotalTasks > 0 ?  $"{(CompletedTasks * 100.0 / TotalTasks):F0}%" : "0%";
        public double CompletionProgress => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks : 0;
        public double ProductivityProgress => ProductivityPercent / 100.0;

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                if (_sessionService == null) return;

                var session = await _sessionService. GetUserSessionAsync();
                if (session.HasValue)
                {
                    _userId = session.Value.UserId;
                    await LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug. WriteLine($"❌ StatisticsViewModel Init Error: {ex. Message}");
            }
        }

        private async Task LoadStatistics()
        {
            if (_todoTaskService == null || _userId == 0)
            {
                System.Diagnostics. Debug.WriteLine("⚠️ LoadStatistics: Сервіс або UserId не доступні");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"📊 Завантаження статистики для користувача {_userId}.. .");

                var tasks = await _todoTaskService.GetTasksByUserIdAsync(_userId);
                var tasksList = tasks.ToList();

                // ✅ Реальні дані з БД
                TotalTasks = tasksList.Count;
                CompletedTasks = tasksList.Count(t => t.IsCompleted);
                InProgressTasks = TotalTasks - CompletedTasks;

                ProductivityPercent = TotalTasks > 0 
                    ? (double)CompletedTasks / TotalTasks * 100 
                    :  0;

                System.Diagnostics.Debug.WriteLine($"✅ Статистика:  Всього={TotalTasks}, Виконано={CompletedTasks}, В процесі={InProgressTasks}");
                
                OnPropertyChanged(nameof(ProductivityText));
                OnPropertyChanged(nameof(CompletionRate));
                OnPropertyChanged(nameof(CompletionProgress));
                OnPropertyChanged(nameof(ProductivityProgress));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LoadStatistics Error: {ex.Message}");
            }
        }

        private async Task OnBack()
        {
            await Shell.Current.GoToAsync(". .");
        }
    }
}