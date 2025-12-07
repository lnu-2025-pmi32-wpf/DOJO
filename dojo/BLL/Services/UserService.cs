using DAL;
using DAL.Models;
using BLL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly DojoDbContext _context;

        public UserService(DojoDbContext context)
        {
            _context = context;
        }

        public async Task<User?> RegisterAsync(string email, string password, string? username = null)
        {
            // Перевіряємо, чи користувач з таким email вже існує
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return null; // Користувач вже існує
            }

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

            return newUser;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            // Шукаємо користувача за email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null; // Користувача не знайдено
            }

            // Перевіряємо пароль
            var passwordHash = HashPassword(password);
            if (user.Password != passwordHash)
            {
                return null; // Невірний пароль
            }

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
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

