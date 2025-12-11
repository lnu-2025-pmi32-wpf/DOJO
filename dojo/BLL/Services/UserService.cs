using System.Security.Cryptography;
using System.Text;
using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

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
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return null;
            }

            var passwordHash = HashPassword(password);

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

        // Compatibility wrapper so older code/tests that call CreateUserAsync will still work
        public async Task<User?> CreateUserAsync(User user)
        {
            // Якщо в user.Password передається plain text, він буде захешований всередині RegisterAsync
            return await RegisterAsync(user.Email, user.Password, user.Username);
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            var passwordHash = HashPassword(password);
            if (user.Password != passwordHash)
            {
                return null;
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
