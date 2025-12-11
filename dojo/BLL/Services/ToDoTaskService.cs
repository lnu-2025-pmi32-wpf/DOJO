using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ToDoTaskService : IToDoTaskService
    {
        private readonly DojoDbContext _context;
        private readonly ILogger<ToDoTaskService> _logger;

        public ToDoTaskService(DojoDbContext context, ILogger<ToDoTaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ToDoTask>> GetAllTasksAsync()
        {
            _logger.LogInformation("📋 Завантаження всіх завдань");

            try
            {
                var tasks = await _context.ToDoTasks.Include(t => t.Goal).ToListAsync();
                _logger.LogInformation("✅ Завантажено {Count} завдань", tasks.Count);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка завантаження всіх завдань");
                throw;
            }
        }

        public async Task<IEnumerable<ToDoTask>> GetTasksByUserIdAsync(int userId)
        {
            _logger.LogInformation("📋 Завантаження завдань для користувача {UserId}", userId);

            try
            {
                var tasks = await _context.ToDoTasks
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Goal)
                    .ToListAsync();

                _logger.LogInformation("✅ Завантажено {Count} завдань для користувача {UserId}", tasks.Count, userId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка завантаження завдань для користувача {UserId}", userId);
                throw;
            }
        }

        public async Task<ToDoTask?> GetTaskByIdAsync(int id)
        {
            _logger.LogDebug("🔍 Пошук завдання за ID:  {TaskId}", id);

            try
            {
                var task = await _context.ToDoTasks.Include(t => t.Goal).FirstOrDefaultAsync(t => t.Id == id);

                if (task != null)
                {
                    _logger.LogDebug("✅ Завдання знайдено: {TaskId} - '{Description}'", id, task.Description);
                }
                else
                {
                    _logger.LogWarning("⚠️ Завдання з ID {TaskId} не знайдено", id);
                }

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка пошуку завдання {TaskId}", id);
                throw;
            }
        }

        public async Task AddTaskAsync(ToDoTask task)
        {
            _logger.LogInformation("📝 Створення нового завдання для користувача {UserId}:  '{Description}'",
                task.UserId, task.Description);

            try
            {
                await _context.ToDoTasks.AddAsync(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Завдання створено: ID={TaskId}, Опис='{Description}', UserId={UserId}, Пріоритет={Priority}",
                    task.Id, task.Description, task.UserId, task.Priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка створення завдання '{Description}' для користувача {UserId}",
                    task.Description, task.UserId);
                throw;
            }
        }

        public async Task UpdateTaskAsync(ToDoTask task)
        {
            _logger.LogInformation("🔄 Оновлення завдання:  ID={TaskId}, IsCompleted={IsCompleted}",
                task.Id, task.IsCompleted);

            try
            {
                _context.ToDoTasks.Update(task);
                await _context.SaveChangesAsync();

                if (task.IsCompleted)
                {
                    _logger.LogInformation("✅ Завдання виконано: ID={TaskId}, '{Description}', CompletedAt={CompletedAt}",
                        task.Id, task.Description, task.CompletedAt);
                }
                else
                {
                    _logger.LogInformation("✅ Завдання оновлено: ID={TaskId}, '{Description}'", task.Id, task.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка оновлення завдання ID={TaskId}", task.Id);
                throw;
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            _logger.LogInformation("🗑️ Видалення завдання:  ID={TaskId}", id);

            try
            {
                var task = await _context.ToDoTasks.FindAsync(id);

                if (task != null)
                {
                    _context.ToDoTasks.Remove(task);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Завдання видалено: ID={TaskId}, Опис='{Description}'", id, task.Description);
                }
                else
                {
                    _logger.LogWarning("⚠️ Спроба видалити неіснуюче завдання:  ID={TaskId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка видалення завдання ID={TaskId}", id);
                throw;
            }
        }
    }
}
