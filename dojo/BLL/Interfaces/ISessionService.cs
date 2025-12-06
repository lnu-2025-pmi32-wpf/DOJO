namespace BLL.Interfaces
{
    public interface ISessionService
    {
        Task SaveUserSessionAsync(string email, int userId);
        Task<(string Email, int UserId)?> GetUserSessionAsync();
        Task ClearSessionAsync();
        Task<bool> IsLoggedInAsync();
    }
}

