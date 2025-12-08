using System.Collections.ObjectModel;

namespace Presentation.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private int _completedTasks;
        private int _totalTasks;
        private double _totalHours;
        private int _currentStreak;
        private double _productivityPercent;

        public StatisticsViewModel()
        {
            LoadStatistics();
        }

        public int CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        public int TotalTasks
        {
            get => _totalTasks;
            set => SetProperty(ref _totalTasks, value);
        }

        public double TotalHours
        {
            get => _totalHours;
            set => SetProperty(ref _totalHours, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public double ProductivityPercent
        {
            get => _productivityPercent;
            set => SetProperty(ref _productivityPercent, value);
        }

        public string ProductivityText => $"{ProductivityPercent:F0}%";
        public string CompletionRate => TotalTasks > 0 ? $"{(CompletedTasks * 100.0 / TotalTasks):F0}%" : "0%";
        
        // Progress values for ProgressBar (0.0 - 1.0)
        public double CompletionProgress => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks : 0;
        public double ProductivityProgress => ProductivityPercent / 100.0;

        private void LoadStatistics()
        {
            // TODO: Load from database
            CompletedTasks = 45;
            TotalTasks = 60;
            TotalHours = 120.5;
            CurrentStreak = 7;
            ProductivityPercent = 85;
        }
    }
}