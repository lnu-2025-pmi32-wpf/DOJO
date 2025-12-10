namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<DAL.Models.User?> RegisterAsync(string email, string password, string? username = null);
        Task<DAL.Models.User?> CreateUserAsync(DAL.Models.User user); // сумісність з кодом/тестами
        Task<DAL.Models.User?> LoginAsync(string email, string password);
        Task<DAL.Models.User?> GetUserByIdAsync(int userId);
        Task<DAL.Models.User?> GetUserByEmailAsync(string email);
    }
}