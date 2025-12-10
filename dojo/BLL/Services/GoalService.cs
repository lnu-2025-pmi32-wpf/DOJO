using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLL. Interfaces;

namespace BLL.Services
{
    public class GoalService : IGoalService
    {
        private readonly DojoDbContext _context;
        private readonly ILogger<GoalService> _logger;

        public GoalService(DojoDbContext context, ILogger<GoalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Goal>> GetAllGoalsAsync()
        {
            _logger.LogInformation("🎯 Завантаження всіх планів (Goals)");

            try
            {
                var goals = await _context.Goals. Include(g => g.Tasks).ToListAsync();
                _logger.LogInformation("✅ Завантажено {Count} планів", goals.Count);
                return goals;
            }
            catch (Exception ex)
            {
                _logger. LogError(ex, "❌ Помилка завантаження всіх планів");
                throw;
            }
        }

        public async Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId)
        {
            _logger.LogInformation("🎯 Завантаження планів для користувача {UserId}", userId);

            try
            {
                var goals = await _context.Goals
                    .Where(g => g.UserId == userId)
                    .Include(g => g. Tasks)
                    .ToListAsync();

                _logger.LogInformation("✅ Завантажено {Count} планів для користувача {UserId}", goals.Count, userId);
                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка завантаження планів для користувача {UserId}", userId);
                throw;
            }
        }

        public async Task<Goal? > GetGoalByIdAsync(int id)
        {
            _logger.LogDebug("🔍 Пошук плану за ID: {GoalId}", id);

            try
            {
                var goal = await _context.Goals
                    .Include(g => g.Tasks)
                    .FirstOrDefaultAsync(g => g. Id == id);

                if (goal != null)
                {
                    _logger.LogDebug("✅ План знайдено: {GoalId} - '{Description}' ({TaskCount} завдань)", 
                        id, goal.Description, goal.Tasks?. Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("⚠️ План з ID {GoalId} не знайдено", id);
                }

                return goal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка пошуку плану {GoalId}", id);
                throw;
            }
        }

        public async Task AddGoalAsync(Goal goal)
        {
            _logger.LogInformation("📝 Створення нового плану для користувача {UserId}:  '{Description}'", 
                goal. UserId, goal.Description);

            try
            {
                await _context.Goals.AddAsync(goal);
                await _context. SaveChangesAsync();

                _logger.LogInformation("✅ План створено:  ID={GoalId}, Опис='{Description}', UserId={UserId}, Пріоритет={Priority}, Дедлайн={EndTime}", 
                    goal. Id, goal.Description, goal. UserId, goal.Priority, goal.EndTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка створення плану '{Description}' для користувача {UserId}", 
                    goal.Description, goal.UserId);
                throw;
            }
        }

        public async Task UpdateGoalAsync(Goal goal)
        {
            _logger.LogInformation("🔄 Оновлення плану:  ID={GoalId}, '{Description}', Прогрес={Progress}%", 
                goal.Id, goal.Description, goal.Progress);

            try
            {
                _context.Goals.Update(goal);
                await _context. SaveChangesAsync();

                if (goal.IsCompleted)
                {
                    _logger.LogInformation("✅ План виконано: ID={GoalId}, Опис='{Description}'", goal.Id, goal.Description);
                }
                else
                {
                    _logger.LogInformation("✅ План оновлено:  ID={GoalId}, Опис='{Description}', Прогрес={Progress}%", 
                        goal.Id, goal.Description, goal.Progress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка оновлення плану ID={GoalId}", goal. Id);
                throw;
            }
        }

        public async Task DeleteGoalAsync(int id)
        {
            _logger.LogInformation("🗑️ Видалення плану: ID={GoalId}", id);

            try
            {
                var goal = await _context. Goals.FindAsync(id);
                
                if (goal != null)
                {
                    _context.Goals.Remove(goal);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ План видалено: ID={GoalId}, Опис='{Description}'", id, goal. Description);
                }
                else
                {
                    _logger.LogWarning("⚠️ Спроба видалити неіснуючий план:  ID={GoalId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка видалення плану ID={GoalId}", id);
                throw;
            }
        }
    }
}