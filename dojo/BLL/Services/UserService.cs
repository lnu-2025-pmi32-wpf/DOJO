using DAL;
using DAL.Models;
using BLL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly DojoDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(DojoDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> RegisterAsync(string email, string password, string? username = null)
        {
            _logger.LogInformation("📝 Спроба реєстрації користувача:  {Email}", email);

            // Перевіряємо, чи користувач з таким email вже існує
            var existingUser = await _context.Users
                . FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                _logger.LogWarning("⚠️ Користувач з email {Email} вже існує", email);
                return null; // Користувач вже існує
            }

            try
            {
                // Хешуємо пароль
                var passwordHash = HashPassword(password);

                // Створюємо нового користувача
                var newUser = new User
                {
                    Email = email,
                    Username = username,
                    Password = passwordHash,
                    ExpPoints = 0,
                    Level = 1,
                    CurrentStreak = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Користувач успішно зареєстрований: {Email} (ID: {UserId})", email, newUser.Id);

                return newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка при реєстрації користувача {Email}", email);
                throw;
            }
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            _logger. LogInformation("🔑 Спроба входу користувача: {Email}", email);

            // Шукаємо користувача за email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger. LogWarning("⚠️ Користувача з email {Email} не знайдено", email);
                return null; // Користувача не знайдено
            }

            // Перевіряємо пароль
            var passwordHash = HashPassword(password);
            if (user.Password != passwordHash)
            {
                _logger. LogWarning("❌ Невірний пароль для користувача {Email}", email);
                return null; // Невірний пароль
            }

            _logger.LogInformation("✅ Успішний вхід:  {Email} (ID: {UserId}, Level: {Level})", email, user.Id, user.Level);

            return user;
        }

        public async Task<User? > GetUserByIdAsync(int userId)
        {
            _logger.LogDebug("🔍 Пошук користувача за ID:  {UserId}", userId);

            try
            {
                var user = await _context.Users. FindAsync(userId);

                if (user != null)
                {
                    _logger.LogDebug("✅ Користувача знайдено: {Email} (ID: {UserId})", user.Email, userId);
                }
                else
                {
                    _logger.LogWarning("⚠️ Користувача з ID {UserId} не знайдено", userId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка при пошуку користувача за ID {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogDebug("🔍 Пошук користувача за email: {Email}", email);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    _logger.LogDebug("✅ Користувача знайдено: {Email} (ID:  {UserId})", email, user.Id);
                }
                else
                {
                    _logger.LogWarning("⚠️ Користувача з email {Email} не знайдено", email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger. LogError(ex, "❌ Помилка при пошуку користувача за email {Email}", email);
                throw;
            }
        }

        // Метод для хешування паролю (простий SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}