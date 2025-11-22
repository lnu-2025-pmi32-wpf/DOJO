using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public interface IToDoTaskService
    {
        Task<IEnumerable<ToDoTask>> GetAllTasksAsync();
        Task<IEnumerable<ToDoTask>> GetTasksByUserIdAsync(int userId);
        Task<ToDoTask?> GetTaskByIdAsync(int id);
        Task AddTaskAsync(ToDoTask task);
        Task UpdateTaskAsync(ToDoTask task);
        Task DeleteTaskAsync(int id);
    }

    public class ToDoTaskService : IToDoTaskService
    {
        private readonly DojoDbContext _context;

        public ToDoTaskService(DojoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ToDoTask>> GetAllTasksAsync()
        {
            return await _context.ToDoTasks.Include(t => t.Goal).ToListAsync();
        }

        public async Task<IEnumerable<ToDoTask>> GetTasksByUserIdAsync(int userId)
        {
            return await _context.ToDoTasks
                .Where(t => t.UserId == userId)
                .Include(t => t.Goal)
                .ToListAsync();
        }

        public async Task<ToDoTask?> GetTaskByIdAsync(int id)
        {
            return await _context.ToDoTasks.Include(t => t.Goal).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTaskAsync(ToDoTask task)
        {
            await _context.ToDoTasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(ToDoTask task)
        {
            _context.ToDoTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.ToDoTasks.FindAsync(id);
            if (task != null)
            {
                _context.ToDoTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}

