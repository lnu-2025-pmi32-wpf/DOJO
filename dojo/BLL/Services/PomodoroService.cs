using dojo;
using dojo.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public interface IPomodoroService
    {
        Task<IEnumerable<Pomodoro>> GetAllPomodorosAsync();
        Task<IEnumerable<Pomodoro>> GetPomodorosByTaskIdAsync(int taskId);
        Task<Pomodoro?> GetPomodoroByIdAsync(int id);
        Task AddPomodoroAsync(Pomodoro pomodoro);
        Task UpdatePomodoroAsync(Pomodoro pomodoro);
        Task DeletePomodoroAsync(int id);
    }

    public class PomodoroService : IPomodoroService
    {
        private readonly DojoDbContext _context;

        public PomodoroService(DojoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pomodoro>> GetAllPomodorosAsync()
        {
            return await _context.Pomodoros.ToListAsync();
        }

        public async Task<IEnumerable<Pomodoro>> GetPomodorosByTaskIdAsync(int taskId)
        {
            return await _context.Pomodoros
                .Where(p => p.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<Pomodoro?> GetPomodoroByIdAsync(int id)
        {
            return await _context.Pomodoros.FindAsync(id);
        }

        public async Task AddPomodoroAsync(Pomodoro pomodoro)
        {
            await _context.Pomodoros.AddAsync(pomodoro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePomodoroAsync(Pomodoro pomodoro)
        {
            _context.Pomodoros.Update(pomodoro);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePomodoroAsync(int id)
        {
            var pomodoro = await _context.Pomodoros.FindAsync(id);
            if (pomodoro != null)
            {
                _context.Pomodoros.Remove(pomodoro);
                await _context.SaveChangesAsync();
            }
        }
    }
}

