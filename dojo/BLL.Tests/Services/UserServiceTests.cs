using System.Security.Cryptography;
using System.Text;
using BLL.Services;
using DAL;
using DAL.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BLL.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly DojoDbContext context;
        private readonly UserService userService;
        private readonly User testUser;

        public UserServiceTests()
        {
            this.context = TestDbContextFactory.CreateInMemoryContext();
            this.userService = new UserService(this.context);

            // Створюємо тестового користувача, зберігаємо пароль у хешованому вигляді,
            // бо сервіс хешує пароль перед перевіркою.
            var passwordPlain = "hash123";
            var passwordHash = HashPassword(passwordPlain);

            this.testUser = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = passwordHash, // збережено як хеш
                Level = 1,
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(this.testUser);
            this.context.SaveChanges();
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddUser()
        {
            var user = new User
            {
                Username = "NewUser",
                Email = "new@example.com",
                Password = "pwd",
                CreatedAt = DateTime.UtcNow
            };

            var created = await this.userService.RegisterAsync(user.Email, "pwd", user.Username);

            created.Should().NotBeNull();
            var saved = await this.context.Users.FirstOrDefaultAsync(u => u.Username == "NewUser");
            saved.Should().NotBeNull();
            saved!.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenCredentialsCorrect()
        {
            // Використовуємо plain password, сервіс його захешує і порівняє з тим, що збережено
            var result = await this.userService.LoginAsync(this.testUser.Email, "hash123");

            result.Should().NotBeNull();
            result!.Email.Should().Be(this.testUser.Email);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenExists()
        {
            var result = await this.userService.GetUserByIdAsync(this.testUser.Id);
            result.Should().NotBeNull();
            result!.Username.Should().Be("TestUser");
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenNotExists()
        {
            var result = await this.userService.GetUserByIdAsync(999);
            result.Should().BeNull();
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }

        // Допоміжний метод для хешування паролю (повинен збігатися з тим, що в UserService)
        private static string HashPassword(string password)
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
