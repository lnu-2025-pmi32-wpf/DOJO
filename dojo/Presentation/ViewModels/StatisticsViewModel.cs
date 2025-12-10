using System.Windows.Input;
using BLL. Interfaces;
using Presentation. Helpers;

namespace Presentation.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IToDoTaskService?  _todoTaskService;
        private readonly ISessionService? _sessionService;
        
        private int _totalTasks;
        private int _completedTasks;
        private int _inProgressTasks;
        private int _userId;
        
        // Висота стовпчиків графіка (0-200)
        private double _day1Height;
        private double _day2Height;
        private double _day3Height;
        private double _day4Height;
        private double _day5Height;
        private double _day6Height;
        private double _day7Height;

        public StatisticsViewModel(IToDoTaskService? todoTaskService = null, ISessionService? sessionService = null)
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

        // Графік - висота стовпчиків
        public double Day1Height { get => _day1Height; set => SetProperty(ref _day1Height, value); }
        public double Day2Height { get => _day2Height; set => SetProperty(ref _day2Height, value); }
        public double Day3Height { get => _day3Height; set => SetProperty(ref _day3Height, value); }
        public double Day4Height { get => _day4Height; set => SetProperty(ref _day4Height, value); }
        public double Day5Height { get => _day5Height; set => SetProperty(ref _day5Height, value); }
        public double Day6Height { get => _day6Height; set => SetProperty(ref _day6Height, value); }
        public double Day7Height { get => _day7Height; set => SetProperty(ref _day7Height, value); }

        // Computed properties
        public string CompletionRate => TotalTasks > 0 ? $"{(CompletedTasks * 100.0 / TotalTasks):F0}%" : "0%";
        public double CompletionProgress => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks : 0;

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                if (_sessionService == null) return;

                var session = await _sessionService.GetUserSessionAsync();
                if (session. HasValue)
                {
                    _userId = session.Value.UserId;
                    await LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ StatisticsViewModel Init Error: {ex.Message}");
            }
        }

        private async Task LoadStatistics()
        {
            if (_todoTaskService == null || _userId == 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ LoadStatistics:  Сервіс або UserId не доступні");
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

                // ✅ Генеруємо графік за останні 7 днів
                LoadChartData(tasksList);

                System. Diagnostics.Debug.WriteLine($"✅ Статистика:  Всього={TotalTasks}, Виконано={CompletedTasks}, В процесі={InProgressTasks}");
                
                OnPropertyChanged(nameof(CompletionRate));
                OnPropertyChanged(nameof(CompletionProgress));
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ LoadStatistics Error: {ex.Message}");
            }
        }

        private void LoadChartData(List<DAL.Models.ToDoTask> tasks)
        {
            var today = DateTime.Today;
            var heights = new List<double>();

            // Рахуємо виконані таски за кожен день (останні 7 днів)
            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today.AddDays(-i);
                var completedOnDay = tasks.Count(t => 
                    t.IsCompleted && 
                    t.CompletedAt.HasValue && 
                    t. CompletedAt.Value.Date == targetDate);

                // Висота = кількість * 30 (максимум 200)
                var height = Math.Min(completedOnDay * 30, 200);
                heights. Add(height);
            }

            // Присвоюємо висоти
            Day1Height = heights[0];
            Day2Height = heights[1];
            Day3Height = heights[2];
            Day4Height = heights[3];
            Day5Height = heights[4];
            Day6Height = heights[5];
            Day7Height = heights[6];

            System.Diagnostics.Debug.WriteLine($"📊 Графік:  {string.Join(", ", heights)}");
        }

        private async Task OnBack()
        {
            await Shell.Current.GoToAsync(". .");
        }
    }
}