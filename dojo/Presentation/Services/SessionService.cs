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
            System.Diagnostics. Debug.WriteLine($"📝 SaveUserSession:  Зберігаємо сесію для {email}.. .");
            
            // ✅ ВИПРАВЛЕНО: Спочатку зберігаємо в Preferences (надійніше на Windows)
            try
            {
                Preferences.Set(EmailKey, email);
                Preferences.Set(UserIdKey, userId.ToString());
                if (!string. IsNullOrEmpty(username))
                {
                    Preferences.Set(UsernameKey, username);
                }
                System.Diagnostics.Debug.WriteLine("✅ Сесія збережена в Preferences");
            }
            catch (Exception prefsEx)
            {
                System. Diagnostics.Debug.WriteLine($"❌ Preferences save error: {prefsEx.Message}");
            }

            // Також пробуємо SecureStorage як backup (може не працювати на Windows)
            try
            {
                await SecureStorage.SetAsync(EmailKey, email);
                await SecureStorage.SetAsync(UserIdKey, userId.ToString());
                if (!string.IsNullOrEmpty(username))
                {
                    await SecureStorage. SetAsync(UsernameKey, username);
                }
                System.Diagnostics.Debug.WriteLine("✅ Сесія збережена в SecureStorage");
            }
            catch (Exception secureEx)
            {
                // SecureStorage може не працювати на Windows - це нормально
                System.Diagnostics.Debug.WriteLine($"⚠️ SecureStorage save error (може бути нормально на Windows): {secureEx.Message}");
            }
        }

        public async Task<(string Email, int UserId, string? Username)?> GetUserSessionAsync()
        {
            System.Diagnostics.Debug.WriteLine("🔍 GetUserSession: Отримуємо сесію.. .");
            
            string?  email = null;
            string? userIdStr = null;
            string? username = null;

            // ✅ ВИПРАВЛЕНО:  Спочатку пробуємо Preferences (надійніше на Windows)
            try
            {
                email = Preferences.Get(EmailKey, string.Empty);
                userIdStr = Preferences. Get(UserIdKey, string.Empty);
                username = Preferences.Get(UsernameKey, string.Empty);
                
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userIdStr))
                {
                    if (int. TryParse(userIdStr, out int userIdFromPrefs))
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Сесія отримана з Preferences: {email}, UserId={userIdFromPrefs}");
                        return (email, userIdFromPrefs, string.IsNullOrEmpty(username) ? null : username);
                    }
                }
            }
            catch (Exception prefsEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Preferences get error: {prefsEx.Message}");
            }

            // Якщо Preferences пусті - пробуємо SecureStorage
            try
            {
                email = await SecureStorage.GetAsync(EmailKey);
                userIdStr = await SecureStorage. GetAsync(UserIdKey);
                username = await SecureStorage.GetAsync(UsernameKey);

                if (! string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userIdStr))
                {
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        System. Diagnostics.Debug.WriteLine($"✅ Сесія отримана з SecureStorage: {email}, UserId={userId}");
                        
                        // ✅ Синхронізуємо з Preferences для надійності
                        try
                        {
                            Preferences. Set(EmailKey, email);
                            Preferences.Set(UserIdKey, userIdStr);
                            if (!string. IsNullOrEmpty(username))
                            {
                                Preferences.Set(UsernameKey, username);
                            }
                        }
                        catch { /* ігноруємо */ }
                        
                        return (email, userId, string.IsNullOrEmpty(username) ? null : username);
                    }
                }
            }
            catch (Exception secureEx)
            {
                System. Diagnostics.Debug.WriteLine($"⚠️ SecureStorage get error:  {secureEx. Message}");
            }

            System.Diagnostics. Debug.WriteLine("⚠️ Сесія не знайдена");
            return null;
        }

        public async Task ClearSessionAsync()
        {
            System. Diagnostics.Debug.WriteLine("🗑️ ClearSession: Очищаємо сесію...");
            
            try
            {
                Preferences. Remove(EmailKey);
                Preferences. Remove(UserIdKey);
                Preferences.Remove(UsernameKey);
                System.Diagnostics.Debug.WriteLine("✅ Preferences очищено");
            }
            catch (Exception prefsEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Preferences clear error:  {prefsEx. Message}");
            }
            
            // Також очищаємо SecureStorage
            try
            {
                SecureStorage.Remove(EmailKey);
                SecureStorage.Remove(UserIdKey);
                SecureStorage.Remove(UsernameKey);
                System.Diagnostics.Debug.WriteLine("✅ SecureStorage очищено");
            }
            catch (Exception secureEx)
            {
                System. Diagnostics.Debug. WriteLine($"⚠️ SecureStorage clear error: {secureEx.Message}");
            }
            
            await Task.CompletedTask;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            try
            {
                System.Diagnostics. Debug.WriteLine("🔐 IsLoggedIn: Перевіряємо статус.. .");
                var session = await GetUserSessionAsync();
                var isLoggedIn = session.HasValue;
                System. Diagnostics.Debug. WriteLine($"🔐 IsLoggedIn: {isLoggedIn}");
                return isLoggedIn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ IsLoggedIn error: {ex.Message}");
                return false;
            }
        }
    }
}