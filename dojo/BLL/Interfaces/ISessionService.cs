namespace BLL.Interfaces
{
    public interface ISessionService
    {
        Task SaveUserSessionAsync(string email, int userId, string? username = null);
        Task<(string Email, int UserId, string? Username)?> GetUserSessionAsync();
        Task ClearSessionAsync();
        Task<bool> IsLoggedInAsync();
    }
}

