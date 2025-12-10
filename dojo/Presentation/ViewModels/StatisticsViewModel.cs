using System. Windows.Input;
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
        
        private double _day1Height;
        private double _day2Height;
        private double _day3Height;
        private double _day4Height;
        private double _day5Height;
        private double _day6Height;
        private double _day7Height;
        
        private string _motivationQuote;

        private readonly List<string> _motivationQuotes = new()
        {
            "🌟 Кожне завдання - це крок до успіху!",
            "💪 Ти можеш більше, ніж думаєш! ",
            "🚀 Продуктивність - це звичка, а не талант!",
            "✨ Маленькі кроки ведуть до великих результатів!",
            "🎯 Сфокусуйся на процесі, а не на результаті!",
            "🔥 Твоя мотивація сильніша за будь-які перешкоди!",
            "🌈 Кожен новий день - нова можливість!",
            "⚡ Дія - ось що відрізняє мрії від реальності!",
            "🏆 Успіх складається з маленьких перемог!",
            "💎 Ти вже на шляху до своєї мети!",
            "🌱 Зростання відбувається поза зоною комфорту!",
            "🎨 Твори своє майбутнє прямо зараз!",
            "⭐ Ти сильніший за вчорашнього себе!",
            "🌟 Прогрес - це прогрес, навіть якщо він маленький!",
            "🔑 Дисципліна - це ключ до свободи!",
            "🎯 Зроби сьогодні краще за вчора!",
            "💫 Твої зусилля ніколи не марні!",
            "🌸 Вір у себе і все вийде!",
            "⚡ Почни зараз - не чекай ідеального моменту!",
            "🏅 Ти вже молодець, що намагаєшся! ",
            "🌊 Постійність перемагає талант!",
            "🎪 Насолоджуйся процесом, а не тільки результатом!",
            "🌞 Сьогодні - твій день! ",
            "🦋 Зміни починаються з тебе!",
            "🎁 Кожна хвилина - це подарунок!",
            "🌺 Будь кращою версією себе!",
            "⚡ Енергія йде туди, куди спрямована увага!",
            "🎯 Чітка мета - половина успіху!",
            "💪 Не здавайся - ти вже на півдорозі!",
            "🌟 Твоя наполегливість надихає інших!"
        };

        public StatisticsViewModel(IToDoTaskService? todoTaskService = null, ISessionService? sessionService = null)
        {
            _todoTaskService = todoTaskService;
            _sessionService = sessionService;
            
            RefreshCommand = new AsyncRelayCommand(LoadStatistics);
            BackCommand = new AsyncRelayCommand(OnBack);
            
            GenerateRandomMotivation();
            
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

        public string MotivationQuote
        {
            get => _motivationQuote;
            set => SetProperty(ref _motivationQuote, value);
        }

        public double Day1Height { get => _day1Height; set => SetProperty(ref _day1Height, value); }
        public double Day2Height { get => _day2Height; set => SetProperty(ref _day2Height, value); }
        public double Day3Height { get => _day3Height; set => SetProperty(ref _day3Height, value); }
        public double Day4Height { get => _day4Height; set => SetProperty(ref _day4Height, value); }
        public double Day5Height { get => _day5Height; set => SetProperty(ref _day5Height, value); }
        public double Day6Height { get => _day6Height; set => SetProperty(ref _day6Height, value); }
        public double Day7Height { get => _day7Height; set => SetProperty(ref _day7Height, value); }

        public string CompletionRate => TotalTasks > 0 ? $"{(CompletedTasks * 100.0 / TotalTasks):F0}%" : "0%";
        public double CompletionProgress => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks :  0;

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
                    _userId = session.Value. UserId;
                    await LoadStatistics();
                }
            }
            catch (Exception ex)
            {
                System. Diagnostics.Debug.WriteLine($"❌ StatisticsViewModel Init Error: {ex.Message}");
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

                TotalTasks = tasksList.Count;
                CompletedTasks = tasksList.Count(t => t.IsCompleted);
                InProgressTasks = TotalTasks - CompletedTasks;

                LoadChartData(tasksList);
                
                GenerateRandomMotivation();

                System. Diagnostics.Debug.WriteLine($"✅ Статистика:  Всього={TotalTasks}, Виконано={CompletedTasks}, В процесі={InProgressTasks}");
                System.Diagnostics.Debug.WriteLine($"💬 Мотивація: {MotivationQuote}");
                
                OnPropertyChanged(nameof(CompletionRate));
                OnPropertyChanged(nameof(CompletionProgress));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LoadStatistics Error: {ex.Message}");
            }
        }

        private void LoadChartData(List<DAL.Models.ToDoTask> tasks)
        {
            var today = DateTime.Today;
            var heights = new List<double>();

            for (int i = 6; i >= 0; i--)
            {
                var targetDate = today. AddDays(-i);
                var completedOnDay = tasks. Count(t => 
                    t.IsCompleted && 
                    t.CompletedAt.HasValue && 
                    t. CompletedAt.Value.Date == targetDate);
                
                var height = Math.Min(completedOnDay * 30, 200);
                heights. Add(height);
            }

            Day1Height = heights[0];
            Day2Height = heights[1];
            Day3Height = heights[2];
            Day4Height = heights[3];
            Day5Height = heights[4];
            Day6Height = heights[5];
            Day7Height = heights[6];

            System.Diagnostics.Debug.WriteLine($"📊 Графік:  {string.Join(", ", heights)}");
        }

        private void GenerateRandomMotivation()
        {
            var random = new Random();
            var index = random.Next(_motivationQuotes.Count);
            MotivationQuote = _motivationQuotes[index];
        }

        private async Task OnBack()
        {
            await Shell.Current.GoToAsync(". .");
        }
    }
}