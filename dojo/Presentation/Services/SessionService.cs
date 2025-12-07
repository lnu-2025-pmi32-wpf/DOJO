using BLL.Interfaces;

namespace Presentation.Services
{
    public class SessionService : ISessionService
    {
        private const string EmailKey = "user_email";
        private const string UserIdKey = "user_id";
        private const string UsernameKey = "user_name";

        public async Task SaveUserSessionAsync(string email, int userId, string? username = null)
        {
            try
            {
                await SecureStorage.SetAsync(EmailKey, email);
                await SecureStorage.SetAsync(UserIdKey, userId.ToString());
                if (!string.IsNullOrEmpty(username))
                {
                    await SecureStorage.SetAsync(UsernameKey, username);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
                // Fallback to Preferences if SecureStorage fails
                Preferences.Set(EmailKey, email);
                Preferences.Set(UserIdKey, userId);
                if (!string.IsNullOrEmpty(username))
                {
                    Preferences.Set(UsernameKey, username);
                }
            }
        }

        public async Task<(string Email, int UserId, string? Username)?> GetUserSessionAsync()
        {
            try
            {
                var email = await SecureStorage.GetAsync(EmailKey);
                var userIdStr = await SecureStorage.GetAsync(UserIdKey);
                var username = await SecureStorage.GetAsync(UsernameKey);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userIdStr))
                {
                    // Try fallback to Preferences
                    email = Preferences.Get(EmailKey, string.Empty);
                    userIdStr = Preferences.Get(UserIdKey, string.Empty);
                    username = Preferences.Get(UsernameKey, string.Empty);
                    
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userIdStr))
                        return null;
                }

                if (int.TryParse(userIdStr, out int userId))
                {
                    return (email, userId, username);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting session: {ex.Message}");
                
                // Try fallback to Preferences
                try
                {
                    var email = Preferences.Get(EmailKey, string.Empty);
                    var userIdStr = Preferences.Get(UserIdKey, string.Empty);
                    var username = Preferences.Get(UsernameKey, string.Empty);
                    
                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userIdStr) && 
                        int.TryParse(userIdStr, out int userId))
                    {
                        return (email, userId, username);
                    }
                }
                catch
                {
                    // Ignore fallback errors
                }
                
                return null;
            }
        }

        public async Task ClearSessionAsync()
        {
            try
            {
                SecureStorage.Remove(EmailKey);
                SecureStorage.Remove(UserIdKey);
                SecureStorage.Remove(UsernameKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing session: {ex.Message}");
            }
            
            // Also clear from Preferences
            try
            {
                Preferences.Remove(EmailKey);
                Preferences.Remove(UserIdKey);
                Preferences.Remove(UsernameKey);
            }
            catch
            {
                // Ignore errors
            }
            
            await Task.CompletedTask;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            try
            {
                var session = await GetUserSessionAsync();
                return session.HasValue;
            }
            catch
            {
                return false;
            }
        }
    }
}

