using BLL.Interfaces;

namespace Presentation.Services
{
    public class SessionService : ISessionService
    {
        private const string EmailKey = "user_email";
        private const string UserIdKey = "user_id";

        public async Task SaveUserSessionAsync(string email, int userId)
        {
            try
            {
                await SecureStorage.SetAsync(EmailKey, email);
                await SecureStorage.SetAsync(UserIdKey, userId.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
                // Fallback to Preferences if SecureStorage fails
                Preferences.Set(EmailKey, email);
                Preferences.Set(UserIdKey, userId);
            }
        }

        public async Task<(string Email, int UserId)?> GetUserSessionAsync()
        {
            try
            {
                var email = await SecureStorage.GetAsync(EmailKey);
                var userIdStr = await SecureStorage.GetAsync(UserIdKey);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userIdStr))
                {
                    // Try fallback to Preferences
                    email = Preferences.Get(EmailKey, string.Empty);
                    userIdStr = Preferences.Get(UserIdKey, string.Empty);
                    
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userIdStr))
                        return null;
                }

                if (int.TryParse(userIdStr, out int userId))
                {
                    return (email, userId);
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
                    
                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userIdStr) && 
                        int.TryParse(userIdStr, out int userId))
                    {
                        return (email, userId);
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

