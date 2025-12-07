using DAL.Models;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterAsync(string email, string password, string? username = null);
        Task<User?> LoginAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
