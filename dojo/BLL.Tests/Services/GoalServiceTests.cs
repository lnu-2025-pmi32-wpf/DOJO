using BLL.Services;
using DAL;
using DAL.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BLL.Tests.Services
{
    public class GoalServiceTests : IDisposable
    {
        private readonly DojoDbContext context;
        private readonly GoalService goalService;
        private readonly User testUser;

        public GoalServiceTests()
        {
            this.context = TestDbContextFactory.CreateInMemoryContext();
            this.goalService = new GoalService(this.context, NullLogger<GoalService>.Instance);

            this.testUser = new User
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "hash123",
                Level = 1,
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(this.testUser);
            this.context.SaveChanges();
        }

        [Fact]
        public async Task AddGoalAsync_ShouldAddGoalToDatabase()
        {
            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Test Goal\nWith description",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(7),
                Priority = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.goalService.AddGoalAsync(goal);

            var savedGoal = await this.context.Goals
                .FirstOrDefaultAsync(g => g.Description.Contains("Test Goal"));

            savedGoal.Should().NotBeNull();
            savedGoal!.UserId.Should().Be(this.testUser.Id);
            savedGoal.Priority.Should().Be(2);
        }

        [Fact]
        public async Task GetGoalsByUserIdAsync_ShouldReturnUserGoals()
        {
            var goal1 = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Goal 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var goal2 = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Goal 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var goal3 = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Goal 3",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.context.Goals.AddRange(goal1, goal2, goal3);
            await this.context.SaveChangesAsync();

            var result = await this.goalService.GetGoalsByUserIdAsync(this.testUser.Id);

            result.Should().NotBeNull();
            var goalsList = result.ToList();
            goalsList.Should().HaveCount(3);
            goalsList.Should().Contain(g => g.Description == "Goal 1");
            goalsList.Should().Contain(g => g.Description == "Goal 2");
            goalsList.Should().Contain(g => g.Description == "Goal 3");
        }

        [Fact]
        public async Task GetGoalsByUserIdAsync_ShouldReturnEmptyList_WhenNoGoals()
        {
            var result = await this.goalService.GetGoalsByUserIdAsync(999);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGoalByIdAsync_ShouldReturnGoal_WhenExists()
        {
            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Specific Goal",
                Priority = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            await this.context.SaveChangesAsync();

            var result = await this.goalService.GetGoalByIdAsync(goal.Id);

            result.Should().NotBeNull();
            result!.Description.Should().Be("Specific Goal");
            result.Priority.Should().Be(1);
        }

        [Fact]
        public async Task GetGoalByIdAsync_ShouldReturnNull_WhenNotExists()
        {

            var result = await this.goalService.GetGoalByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateGoalAsync_ShouldUpdateGoalInDatabase()
        {

            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Original Description",
                Priority = 1,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            await this.context.SaveChangesAsync();

            goal.Description = "Updated Description";
            goal.Priority = 3;
            goal.IsCompleted = true;
            goal.Progress = 100;
            await this.goalService.UpdateGoalAsync(goal);

            var updatedGoal = await this.context.Goals.FindAsync(goal.Id);
            updatedGoal.Should().NotBeNull();
            updatedGoal!.Description.Should().Be("Updated Description");
            updatedGoal.Priority.Should().Be(3);
            updatedGoal.IsCompleted.Should().BeTrue();
            updatedGoal.Progress.Should().Be(100);
        }

        [Fact]
        public async Task DeleteGoalAsync_ShouldRemoveGoalFromDatabase()
        {

            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Goal to Delete",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            await this.context.SaveChangesAsync();
            int goalId = goal.Id;

            await this.goalService.DeleteGoalAsync(goalId);

            var deletedGoal = await this.context.Goals.FindAsync(goalId);
            deletedGoal.Should().BeNull();
        }

        [Fact]
        public async Task DeleteGoalAsync_ShouldNotThrow_WhenGoalNotExists()
        {

            Func<Task> act = async () => await this.goalService.DeleteGoalAsync(999);


            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetGoalsByUserIdAsync_ShouldIncludeTasks()
        {

            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Goal with Tasks",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            await this.context.SaveChangesAsync();

            var task1 = new ToDoTask
            {
                UserId = this.testUser.Id,
                GoalId = goal.Id,
                Description = "Task 1",
                CreatedAt = DateTime.UtcNow
            };
            var task2 = new ToDoTask
            {
                UserId = this.testUser.Id,
                GoalId = goal.Id,
                Description = "Task 2",
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.AddRange(task1, task2);
            await this.context.SaveChangesAsync();


            var result = await this.goalService.GetGoalsByUserIdAsync(this.testUser.Id);

            var goalsList = result.ToList();
            goalsList.Should().HaveCount(1);
            var goalWithTasks = goalsList.First();
            goalWithTasks.Tasks.Should().NotBeNull();
            goalWithTasks.Tasks.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllGoalsAsync_ShouldReturnAllGoals()
        {

            var user2 = new User
            {
                Username = "User2",
                Email = "user2@example.com",
                Password = "hash456",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user2);
            await this.context.SaveChangesAsync();

            var goal1 = new Goal { UserId = this.testUser.Id, Description = "Goal 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var goal2 = new Goal { UserId = user2.Id, Description = "Goal 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            this.context.Goals.AddRange(goal1, goal2);
            await this.context.SaveChangesAsync();


            var result = await this.goalService.GetAllGoalsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }
    }
}
