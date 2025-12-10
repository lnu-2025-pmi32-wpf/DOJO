using BLL.Services;
using DAL;
using DAL.Models;
using FluentAssertions;
using Xunit;

namespace BLL.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly DojoDbContext context;
        private readonly UserService userService;

        public UserServiceTests()
        {
            this.context = TestDbContextFactory.CreateInMemoryContext();
            this.userService = new UserService(this.context);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateNewUser()
        {
            string email = "newuser@example.com";
            string password = "Password123";
            string username = "NewUser";

            var result = await this.userService.RegisterAsync(email, password, username);

            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            result.Username.Should().Be(username);
            result.Password.Should().NotBeNullOrEmpty();
            result.Password.Should().NotBe(password); 
            result.Level.Should().Be(1);
            result.ExpPoints.Should().Be(0);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenEmailExists()
        {
            string email = "existing@example.com";
            var existingUser = new User
            {
                Username = "ExistingUser",
                Email = email,
                Password = "hash123",
                Level = 1,
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(existingUser);
            await this.context.SaveChangesAsync();

            var result = await this.userService.RegisterAsync(email, "NewPassword", "NewUser");

            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_ShouldHashPassword()
        {
            string password = "MySecretPassword";

            var user1 = await this.userService.RegisterAsync("user1@test.com", password, "User1");
            var user2 = await this.userService.RegisterAsync("user2@test.com", password, "User2");

            user1.Should().NotBeNull();
            user2.Should().NotBeNull();
            user1!.Password.Should().Be(user2!.Password); 
            user1.Password.Should().NotBe(password); 
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenCredentialsCorrect()
        {
            string email = "test@example.com";
            string password = "Password123";
            
            await this.userService.RegisterAsync(email, password, "TestUser");

            var result = await this.userService.LoginAsync(email, password);

            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            result.Username.Should().Be("TestUser");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIncorrect()
        {
            string email = "test@example.com";
            await this.userService.RegisterAsync(email, "CorrectPassword", "TestUser");

            var result = await this.userService.LoginAsync(email, "WrongPassword");

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenEmailNotExists()
        {
            var result = await this.userService.LoginAsync("nonexistent@example.com", "SomePassword");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                Level = 3,
                ExpPoints = 250,
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            var result = await this.userService.GetUserByIdAsync(user.Id);

            result.Should().NotBeNull();
            result!.Username.Should().Be("TestUser");
            result.Level.Should().Be(3);
            result.ExpPoints.Should().Be(250);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await this.userService.GetUserByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenExists()
        {
            string email = "unique@example.com";
            await this.userService.RegisterAsync(email, "Password123", "UniqueUser");

            var result = await this.userService.GetUserByEmailAsync(email);

            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            result.Username.Should().Be("UniqueUser");
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await this.userService.GetUserByEmailAsync("nonexistent@example.com");

            result.Should().BeNull();
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }
    }
}
