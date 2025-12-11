using DAL.Models;

namespace BLL.Interfaces
{
    public interface IGoalService
    {
        Task<IEnumerable<Goal>> GetGoalsByUserIdAsync(int userId);
        Task<Goal?> GetGoalByIdAsync(int goalId);
        Task AddGoalAsync(Goal goal);
        Task UpdateGoalAsync(Goal goal);
        Task DeleteGoalAsync(int goalId);
    }
}

