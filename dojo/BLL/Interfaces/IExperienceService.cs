namespace BLL.Interfaces
{
    public interface IExperienceService
    {
        Task<int> AwardExperienceForTodoAsync(int userId, int priority);
        Task<int> AwardExperienceForPlanAsync(int userId, int priority);
        Task<(int ExpPoints, int Level, int ExpInCurrentLevel, int ExpToNextLevel)> GetUserProgressAsync(int userId);
    }
}