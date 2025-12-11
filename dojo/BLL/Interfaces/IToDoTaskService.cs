using DAL.Models;

namespace BLL.Interfaces
{
    public interface IToDoTaskService
    {
        Task<IEnumerable<ToDoTask>> GetTasksByUserIdAsync(int userId);
        Task<ToDoTask?> GetTaskByIdAsync(int taskId);
        Task AddTaskAsync(ToDoTask task);
        Task UpdateTaskAsync(ToDoTask task);
        Task DeleteTaskAsync(int taskId);
    }
}

