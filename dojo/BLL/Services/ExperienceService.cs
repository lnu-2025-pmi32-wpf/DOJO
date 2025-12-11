using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class ExperienceService : IExperienceService
    {
        private readonly DojoDbContext _context;

        // Базові значення досвіду
        private const int BASE_TODO_EXP = 100;
        private const int BASE_PLAN_EXP = 75;

        // Множники залежно від пріоритету
        private const double LOW_PRIORITY_MULTIPLIER = 0.75;      // Priority = 0 (x0. 75)
        private const double NORMAL_PRIORITY_MULTIPLIER = 1.0;    // Priority = 1 (x1.0)
        private const double HIGH_PRIORITY_MULTIPLIER = 1.5;      // Priority = 2 (x1.5)

        // Досвід для переходу на наступний рівень
        private const int EXP_PER_LEVEL = 600;

        public ExperienceService(DojoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Нараховує досвід за виконане TODO завдання
        /// </summary>
        public async Task<int> AwardExperienceForTodoAsync(int userId, int priority)
        {
            var expGained = CalculateExperience(BASE_TODO_EXP, priority);
            await AddExperienceToUserAsync(userId, expGained);
            return expGained;
        }

        /// <summary>
        /// Нараховує досвід за виконаний план (Goal)
        /// </summary>
        public async Task<int> AwardExperienceForPlanAsync(int userId, int priority)
        {
            var expGained = CalculateExperience(BASE_PLAN_EXP, priority);
            await AddExperienceToUserAsync(userId, expGained);
            return expGained;
        }

        /// <summary>
        /// Розраховує досвід з урахуванням пріоритету
        /// </summary>
        private int CalculateExperience(int baseExp, int priority)
        {
            double multiplier = priority switch
            {
                0 => LOW_PRIORITY_MULTIPLIER,      // Low = 75% досвіду
                1 => NORMAL_PRIORITY_MULTIPLIER,   // Normal = 100% досвіду
                2 => HIGH_PRIORITY_MULTIPLIER,     // High = 150% досвіду
                _ => NORMAL_PRIORITY_MULTIPLIER
            };

            return (int)(baseExp * multiplier);
        }

        /// <summary>
        /// Додає досвід користувачу і перевіряє підвищення рівня
        /// </summary>
        private async Task AddExperienceToUserAsync(int userId, int expAmount)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Користувача з ID {userId} не знайдено!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== ДОДАВАННЯ ДОСВІДУ ===");
            System.Diagnostics.Debug.WriteLine($"UserId: {userId}");
            System.Diagnostics.Debug.WriteLine($"ДО: ExpPoints={user.ExpPoints}, Level={user.Level}");

            // Додаємо досвід
            user.ExpPoints += expAmount;

            // 🔥 ЗАВЖДИ ПЕРЕРАХОВУЄМО РІВЕНЬ (не тільки при підвищенні!)
            int correctLevel = (user.ExpPoints / EXP_PER_LEVEL) + 1;

            System.Diagnostics.Debug.WriteLine($"Додано: +{expAmount} XP");
            System.Diagnostics.Debug.WriteLine($"ПІСЛЯ: ExpPoints={user.ExpPoints}");
            System.Diagnostics.Debug.WriteLine($"EXP_PER_LEVEL={EXP_PER_LEVEL}");
            System.Diagnostics.Debug.WriteLine($"Розрахунок:   ({user.ExpPoints} / {EXP_PER_LEVEL}) + 1 = {correctLevel}");

            // 🔥 ПЕРЕВІРЯЄМО ЧИ ЗМІНИВСЯ РІВЕНЬ
            if (correctLevel != user.Level)
            {
                int oldLevel = user.Level;
                user.Level = correctLevel;  // 🔥 ЗАВЖДИ ОНОВЛЮЄМО! 

                if (correctLevel > oldLevel)
                {
                    System.Diagnostics.Debug.WriteLine($"🎉 РІВЕНЬ ПІДВИЩЕНО! {oldLevel} → {correctLevel}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ РІВЕНЬ ВИПРАВЛЕНО! {oldLevel} → {correctLevel}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"📊 Рівень залишився:   {user.Level}");
            }

            // Оновлюємо дату
            user.LastCompletionDate = DateTime.UtcNow;

            // 🔥 ЯВНО ПОЗНАЧАЄМО ЩО Level ЗМІНЕНО
            _context.Entry(user).Property(u => u.Level).IsModified = true;
            _context.Entry(user).Property(u => u.ExpPoints).IsModified = true;

            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"✅ Зміни збережено в БД (Level={user.Level})");
            System.Diagnostics.Debug.WriteLine($"=== КІНЕЦЬ ДОДАВАННЯ ===\n");
        }

        /// <summary>
        /// Отримує поточний досвід і рівень користувача
        /// </summary>
        public async Task<(int ExpPoints, int Level, int ExpInCurrentLevel, int ExpToNextLevel)> GetUserProgressAsync(int userId)
        {
            // 🔥 ПЕРЕЗАВАНТАЖУЄМО КОРИСТУВАЧА З БД (БЕЗ КЕШУ)
            var user = await _context.Users
                .AsNoTracking()  // Не використовуємо кеш
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return (0, 1, 0, EXP_PER_LEVEL);

            // Розраховуємо досвід В ПОТОЧНОМУ рівні (0-599)
            int expInCurrentLevel = user.ExpPoints % EXP_PER_LEVEL;

            // Скільки треба ДО наступного рівня
            int expToNextLevel = EXP_PER_LEVEL - expInCurrentLevel;

            System.Diagnostics.Debug.WriteLine($"GetUserProgress: ExpPoints={user.ExpPoints}, Level={user.Level}, ExpInLevel={expInCurrentLevel}");

            return (user.ExpPoints, user.Level, expInCurrentLevel, expToNextLevel);
        }
    }
}
