using BLL.Services;
using DAL;
using DAL.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BLL.Tests.Services
{
    public class ToDoTaskServiceTests : IDisposable
    {
        private readonly DojoDbContext context;
        private readonly ToDoTaskService taskService;
        private readonly User testUser;

        public ToDoTaskServiceTests()
        {
            this.context = TestDbContextFactory.CreateInMemoryContext();
            this.taskService = new ToDoTaskService(this.context, NullLogger<ToDoTaskService>.Instance);

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
        public async Task AddTaskAsync_ShouldAddTaskToDatabase()
        {
            // Arrange
            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                Description = "Complete homework",
                Priority = 2,
                DueDate = DateTime.UtcNow.AddDays(3),
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await this.taskService.AddTaskAsync(task);

            // Assert
            var savedTask = await this.context.ToDoTasks
                .FirstOrDefaultAsync(t => t.Description == "Complete homework");
            
            savedTask.Should().NotBeNull();
            savedTask!.UserId.Should().Be(this.testUser.Id);
            savedTask.Priority.Should().Be(2);
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnUserTasks()
        {
            // Arrange
            var task1 = new ToDoTask { UserId = this.testUser.Id, Description = "Task 1", CreatedAt = DateTime.UtcNow };
            var task2 = new ToDoTask { UserId = this.testUser.Id, Description = "Task 2", CreatedAt = DateTime.UtcNow };
            var task3 = new ToDoTask { UserId = this.testUser.Id, Description = "Task 3", CreatedAt = DateTime.UtcNow };
            
            this.context.ToDoTasks.AddRange(task1, task2, task3);
            await this.context.SaveChangesAsync();

            // Act
            var result = await this.taskService.GetTasksByUserIdAsync(this.testUser.Id);

            // Assert
            result.Should().NotBeNull();
            var tasksList = result.ToList();
            tasksList.Should().HaveCount(3);
            tasksList.Should().Contain(t => t.Description == "Task 1");
            tasksList.Should().Contain(t => t.Description == "Task 2");
            tasksList.Should().Contain(t => t.Description == "Task 3");
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnEmptyList_WhenNoTasks()
        {
            // Act
            var result = await this.taskService.GetTasksByUserIdAsync(999);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenExists()
        {
            // Arrange
            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                Description = "Specific Task",
                Priority = 1,
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.Add(task);
            await this.context.SaveChangesAsync();

            // Act
            var result = await this.taskService.GetTaskByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Description.Should().Be("Specific Task");
            result.Priority.Should().Be(1);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await this.taskService.GetTaskByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdateTaskInDatabase()
        {
            // Arrange
            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                Description = "Original Task",
                IsCompleted = false,
                Priority = 1,
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.Add(task);
            await this.context.SaveChangesAsync();

            // Act
            task.Description = "Updated Task";
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            task.Priority = 3;
            await this.taskService.UpdateTaskAsync(task);

            // Assert
            var updatedTask = await this.context.ToDoTasks.FindAsync(task.Id);
            updatedTask.Should().NotBeNull();
            updatedTask!.Description.Should().Be("Updated Task");
            updatedTask.IsCompleted.Should().BeTrue();
            updatedTask.CompletedAt.Should().NotBeNull();
            updatedTask.Priority.Should().Be(3);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldRemoveTaskFromDatabase()
        {
            // Arrange
            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                Description = "Task to Delete",
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.Add(task);
            await this.context.SaveChangesAsync();
            int taskId = task.Id;

            // Act
            await this.taskService.DeleteTaskAsync(taskId);

            // Assert
            var deletedTask = await this.context.ToDoTasks.FindAsync(taskId);
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task AddTaskAsync_ShouldLinkTaskToGoal()
        {
            // Arrange
            var goal = new Goal
            {
                UserId = this.testUser.Id,
                Description = "Main Goal",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            this.context.Goals.Add(goal);
            await this.context.SaveChangesAsync();

            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                GoalId = goal.Id,
                Description = "Linked Task",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await this.taskService.AddTaskAsync(task);

            // Assert
            var savedTask = await this.context.ToDoTasks
                .Include(t => t.Goal)
                .FirstOrDefaultAsync(t => t.Description == "Linked Task");
            
            savedTask.Should().NotBeNull();
            savedTask!.GoalId.Should().Be(goal.Id);
            savedTask.Goal.Should().NotBeNull();
            savedTask.Goal!.Description.Should().Be("Main Goal");
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks()
        {
            // Arrange
            var user2 = new User
            {
                Username = "User2",
                Email = "user2@example.com",
                Password = "hash456",
                CreatedAt = DateTime.UtcNow
            };
            this.context.Users.Add(user2);
            await this.context.SaveChangesAsync();

            var task1 = new ToDoTask { UserId = this.testUser.Id, Description = "Task 1", CreatedAt = DateTime.UtcNow };
            var task2 = new ToDoTask { UserId = user2.Id, Description = "Task 2", CreatedAt = DateTime.UtcNow };
            
            this.context.ToDoTasks.AddRange(task1, task2);
            await this.context.SaveChangesAsync();

            // Act
            var result = await this.taskService.GetAllTasksAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldMarkAsCompleted()
        {
            // Arrange
            var task = new ToDoTask
            {
                UserId = this.testUser.Id,
                Description = "Task to Complete",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
            this.context.ToDoTasks.Add(task);
            await this.context.SaveChangesAsync();

            // Act
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            await this.taskService.UpdateTaskAsync(task);

            // Assert
            var completedTask = await this.context.ToDoTasks.FindAsync(task.Id);
            completedTask.Should().NotBeNull();
            completedTask!.IsCompleted.Should().BeTrue();
            completedTask.CompletedAt.Should().NotBeNull();
            completedTask.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            this.context?.Dispose();
        }
    }
}