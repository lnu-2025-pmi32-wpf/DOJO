using DAL;
using DAL.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DAL.Tests
{

    public class DojoDbContextTests : IDisposable
    {
        private readonly DojoDbContext context;

        public DojoDbContextTests()
        {
            this.context = TestDbContextFactory.CreateInMemoryContext();
        }

        [Fact]
        public void DbContext_ShouldAddAndRetrieveUser()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                ExpPoints = 100,
                Level = 2,
                CreatedAt = DateTime.UtcNow
            };

            this.context.Users.Add(user);
            this.context.SaveChanges();

            var savedUser = this.context.Users.FirstOrDefault(u => u.Username == "TestUser");
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("test@example.com");
            savedUser.ExpPoints.Should().Be(100);
            savedUser.Level.Should().Be(2);
        }

        [Fact]
        public void DbContext_ShouldAddGoalWithUser()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            var goal = new Goal
            {
                UserId = user.Id,
                Description = "Complete Project\nDetails here",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(7),
                Priority = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.context.Goals.Add(goal);
            this.context.SaveChanges();

            var savedGoal = this.context.Goals
                .Include(g => g.User)
                .FirstOrDefault(g => g.Description.Contains("Complete Project"));
            
            savedGoal.Should().NotBeNull();
            savedGoal!.UserId.Should().Be(user.Id);
            savedGoal.User.Should().NotBeNull();
            savedGoal.User!.Username.Should().Be("TestUser");
        }

        [Fact]
        public void DbContext_ShouldAddTaskWithGoal()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            var goal = new Goal
            {
                UserId = user.Id,
                Description = "Main Goal",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            this.context.SaveChanges();

            var task = new ToDoTask
            {
                UserId = user.Id,
                GoalId = goal.Id,
                Description = "Subtask 1",
                Priority = 1,
                CreatedAt = DateTime.UtcNow
            };

            this.context.ToDoTasks.Add(task);
            this.context.SaveChanges();

            var savedTask = this.context.ToDoTasks
                .Include(t => t.User)
                .Include(t => t.Goal)
                .FirstOrDefault(t => t.Description == "Subtask 1");

            savedTask.Should().NotBeNull();
            savedTask!.GoalId.Should().Be(goal.Id);
            savedTask.Goal.Should().NotBeNull();
            savedTask.Goal!.Description.Should().Be("Main Goal");
            savedTask.User!.Username.Should().Be("TestUser");
        }

        [Fact]
        public void DbContext_ShouldUpdateUser()
        {
            var user = new User
            {
                Username = "OriginalName",
                Email = "original@example.com",
                Password = "hash123",
                ExpPoints = 0,
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            user.Username = "UpdatedName";
            user.ExpPoints = 500;
            this.context.SaveChanges();

            var updatedUser = this.context.Users.Find(user.Id);
            updatedUser.Should().NotBeNull();
            updatedUser!.Username.Should().Be("UpdatedName");
            updatedUser.ExpPoints.Should().Be(500);
        }

        [Fact]
        public void DbContext_ShouldDeleteGoal()
        {

            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            var goal = new Goal
            {
                UserId = user.Id,
                Description = "Goal to Delete",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            this.context.SaveChanges();
            int goalId = goal.Id;

            this.context.Goals.Remove(goal);
            this.context.SaveChanges();

            var deletedGoal = this.context.Goals.Find(goalId);
            deletedGoal.Should().BeNull();
        }

        [Fact]
        public void DbContext_ShouldHandleMultipleGoalsForUser()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            var goal1 = new Goal { UserId = user.Id, Description = "Goal 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var goal2 = new Goal { UserId = user.Id, Description = "Goal 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var goal3 = new Goal { UserId = user.Id, Description = "Goal 3", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            this.context.Goals.AddRange(goal1, goal2, goal3);
            this.context.SaveChanges();

            var userGoals = this.context.Goals.Where(g => g.UserId == user.Id).ToList();
            userGoals.Should().HaveCount(3);
            userGoals.Should().Contain(g => g.Description == "Goal 1");
            userGoals.Should().Contain(g => g.Description == "Goal 2");
            userGoals.Should().Contain(g => g.Description == "Goal 3");
        }

        [Fact]
        public void DbContext_ShouldMarkTaskAsCompleted()
        {
            var user = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user);
            this.context.SaveChanges();

            var task = new ToDoTask
            {
                UserId = user.Id,
                Description = "Task to Complete",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.Add(task);
            this.context.SaveChanges();

            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            this.context.SaveChanges();

            var completedTask = this.context.ToDoTasks.Find(task.Id);
            completedTask.Should().NotBeNull();
            completedTask!.IsCompleted.Should().BeTrue();
            completedTask.CompletedAt.Should().NotBeNull();
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }
    }
}
