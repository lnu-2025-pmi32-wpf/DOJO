using DAL.Models;
using FluentAssertions;
using Xunit;

namespace DAL.Tests.Models
{

    public class UserTests
    {
        [Fact]
        public void User_ShouldInitializeWithDefaultValues()
        {
            var user = new User();

            user.Id.Should().Be(0);
            user.Email.Should().BeEmpty();
            user.Username.Should().BeNull();
            user.Password.Should().BeEmpty();
            user.ExpPoints.Should().Be(0);
            user.Level.Should().Be(0);
            user.CurrentStreak.Should().Be(0);
        }

        [Fact]
        public void User_ShouldSetPropertiesCorrectly()
        {
            var createdAt = DateTime.UtcNow;
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                ExpPoints = 500,
                Level = 5,
                CurrentStreak = 3,
                CreatedAt = createdAt
            };

            user.Username.Should().Be("TestUser");
            user.Email.Should().Be("test@example.com");
            user.Password.Should().Be("hash123");
            user.ExpPoints.Should().Be(500);
            user.Level.Should().Be(5);
            user.CurrentStreak.Should().Be(3);
            user.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void User_ShouldInitializeCollections()
        {
            var user = new User();
            var goal = new Goal { Description = "Test Goal" };
            var task = new ToDoTask { Description = "Test Task" };

            user.Goals = new List<Goal> { goal };
            user.Tasks = new List<ToDoTask> { task };

            user.Goals.Should().HaveCount(1);
            user.Tasks.Should().HaveCount(1);
            user.Goals.First().Description.Should().Be("Test Goal");
            user.Tasks.First().Description.Should().Be("Test Task");
        }

        [Fact]
        public void User_ShouldAllowNullableLastCompletionDate()
        {

            var user = new User();

            user.LastCompletionDate.Should().BeNull();

            var date = DateTime.UtcNow;
            user.LastCompletionDate = date;

            user.LastCompletionDate.Should().Be(date);
        }
    }
}
