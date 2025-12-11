using DAL.Models;

namespace BLL.Interfaces
{
    public interface IPomodoroService
    {
        Task<IEnumerable<Pomodoro>> GetPomodorosByUserIdAsync(int userId);
        Task<Pomodoro?> GetPomodoroByIdAsync(int pomodoroId);
        Task AddPomodoroAsync(Pomodoro pomodoro);
        Task UpdatePomodoroAsync(Pomodoro pomodoro);
        Task DeletePomodoroAsync(int pomodoroId);
    }
}

