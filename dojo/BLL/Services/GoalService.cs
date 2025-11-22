using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public interface IGoalService
    {
        Task<IEnumerable<Goal>> GetAllGoalsAsync();
        Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId);
        Task<Goal?> GetGoalByIdAsync(int id);
        Task AddGoalAsync(Goal goal);
        Task UpdateGoalAsync(Goal goal);
        Task DeleteGoalAsync(int id);
    }

    public class GoalService : IGoalService
    {
        private readonly DojoDbContext _context;

        public GoalService(DojoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Goal>> GetAllGoalsAsync()
        {
            return await _context.Goals.Include(g => g.Tasks).ToListAsync();
        }

        public async Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId)
        {
            return await _context.Goals
                .Where(g => g.UserId == userId)
                .Include(g => g.Tasks)
                .ToListAsync();
        }

        public async Task<Goal?> GetGoalByIdAsync(int id)
        {
            return await _context.Goals
                .Include(g => g.Tasks)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddGoalAsync(Goal goal)
        {
            await _context.Goals.AddAsync(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGoalAsync(Goal goal)
        {
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(int id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
            }
        }
    }
}

