using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLL.Interfaces;

namespace BLL.Services
{
    public class PomodoroService : IPomodoroService
    {
        private readonly DojoDbContext _context;
        private readonly ILogger<PomodoroService> _logger;

        public PomodoroService(DojoDbContext context, ILogger<PomodoroService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Pomodoro>> GetAllPomodorosAsync()
        {
            _logger. LogInformation("⏰ Завантаження всіх Pomodoro сесій");

            try
            {
                var pomodoros = await _context. Pomodoros.ToListAsync();
                _logger.LogInformation("✅ Завантажено {Count} Pomodoro сесій", pomodoros.Count);
                return pomodoros;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка завантаження всіх Pomodoro сесій");
                throw;
            }
        }

        public async Task<IEnumerable<Pomodoro>> GetPomodorosByTaskIdAsync(int taskId)
        {
            _logger.LogInformation("⏰ Завантаження Pomodoro сесій для завдання {TaskId}", taskId);

            try
            {
                var pomodoros = await _context. Pomodoros
                    .Where(p => p.TaskId == taskId)
                    .ToListAsync();

                _logger.LogInformation("✅ Завантажено {Count} Pomodoro сесій для завдання {TaskId}", 
                    pomodoros.Count, taskId);
                return pomodoros;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка завантаження Pomodoro сесій для завдання {TaskId}", taskId);
                throw;
            }
        }

        public async Task<IEnumerable<Pomodoro>> GetPomodorosByUserIdAsync(int userId)
        {
            _logger.LogInformation("⏰ Завантаження Pomodoro сесій для користувача {UserId}", userId);

            try
            {
                var pomodoros = await _context.Pomodoros
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                _logger.LogInformation("✅ Завантажено {Count} Pomodoro сесій для користувача {UserId}", 
                    pomodoros.Count, userId);
                return pomodoros;
            }
            catch (Exception ex)
            {
                _logger. LogError(ex, "❌ Помилка завантаження Pomodoro сесій для користувача {UserId}", userId);
                throw;
            }
        }

        public async Task<Pomodoro? > GetPomodoroByIdAsync(int id)
        {
            _logger.LogDebug("🔍 Пошук Pomodoro сесії за ID: {PomodoroId}", id);

            try
            {
                var pomodoro = await _context. Pomodoros. FindAsync(id);

                if (pomodoro != null)
                {
                    _logger.LogDebug("✅ Pomodoro сесію знайдено: {PomodoroId}, UserId={UserId}, Duration={DurationMinutes} хв", 
                        id, pomodoro.UserId, pomodoro.DurationMinutes);
                }
                else
                {
                    _logger.LogWarning("⚠️ Pomodoro сесію з ID {PomodoroId} не знайдено", id);
                }

                return pomodoro;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка пошуку Pomodoro сесії {PomodoroId}", id);
                throw;
            }
        }

        public async Task AddPomodoroAsync(Pomodoro pomodoro)
        {
            _logger.LogInformation("▶️ Запуск Pomodoro сесії: UserId={UserId}, TaskId={TaskId}, Duration={DurationMinutes} хв", 
                pomodoro. UserId, pomodoro.TaskId, pomodoro.DurationMinutes);

            try
            {
                await _context.Pomodoros. AddAsync(pomodoro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Pomodoro сесію створено: ID={PomodoroId}, StartTime={StartTime}, Duration={DurationMinutes} хв", 
                    pomodoro.Id, pomodoro.StartTime, pomodoro.DurationMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка створення Pomodoro сесії для користувача {UserId}", pomodoro.UserId);
                throw;
            }
        }

        public async Task UpdatePomodoroAsync(Pomodoro pomodoro)
        {
            _logger.LogInformation("🔄 Оновлення Pomodoro сесії:  ID={PomodoroId}", pomodoro.Id);

            try
            {
                _context.Pomodoros.Update(pomodoro);
                await _context.SaveChangesAsync();

                if (pomodoro.EndTime.HasValue)
                {
                    var duration = (pomodoro.EndTime.Value - pomodoro.StartTime).TotalMinutes;
                    _logger.LogInformation("⏹️ Pomodoro сесію завершено: ID={PomodoroId}, Тривалість={Duration: F1} хв, Цикли={WorkCycles}", 
                        pomodoro.Id, duration, pomodoro.WorkCycles);
                }
                else
                {
                    _logger.LogInformation("✅ Pomodoro сесію оновлено: ID={PomodoroId}", pomodoro.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка оновлення Pomodoro сесії ID={PomodoroId}", pomodoro.Id);
                throw;
            }
        }

        public async Task DeletePomodoroAsync(int id)
        {
            _logger.LogInformation("🗑️ Видалення Pomodoro сесії:  ID={PomodoroId}", id);

            try
            {
                var pomodoro = await _context.Pomodoros. FindAsync(id);
                
                if (pomodoro != null)
                {
                    _context.Pomodoros.Remove(pomodoro);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Pomodoro сесію видалено: ID={PomodoroId}", id);
                }
                else
                {
                    _logger.LogWarning("⚠️ Спроба видалити неіснуючу Pomodoro сесію: ID={PomodoroId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger. LogError(ex, "❌ Помилка видалення Pomodoro сесії ID={PomodoroId}", id);
                throw;
            }
        }
    }
}