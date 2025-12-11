using DAL.Models;
using FluentAssertions;
using Xunit;

namespace DAL.Tests.Models
{
    public class ToDoTaskTests
    {
        [Fact]
        public void ToDoTask_ShouldInitializeWithDefaultValues()
        {
            var task = new ToDoTask();

            task.Id.Should().Be(0);
            task.UserId.Should().Be(0);
            task.GoalId.Should().BeNull();
            task.Description.Should().BeEmpty();
            task.IsCompleted.Should().BeFalse();
            task.Priority.Should().Be(0);
        }

        [Fact]
        public void ToDoTask_ShouldSetPropertiesCorrectly()
        {
            var dueDate = DateTime.UtcNow.AddDays(3);
            var createdAt = DateTime.UtcNow;

            var task = new ToDoTask
            {
                UserId = 1,
                GoalId = 5,
                Description = "Finish homework",
                IsCompleted = true,
                DueDate = dueDate,
                Priority = 2,
                CreatedAt = createdAt
            };

            task.UserId.Should().Be(1);
            task.GoalId.Should().Be(5);
            task.Description.Should().Be("Finish homework");
            task.IsCompleted.Should().BeTrue();
            task.DueDate.Should().Be(dueDate);
            task.Priority.Should().Be(2);
            task.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void ToDoTask_ShouldAllowNullableGoalId()
        {
            var task = new ToDoTask { UserId = 1 };

            task.GoalId.Should().BeNull();

            task.GoalId = 10;

            task.GoalId.Should().Be(10);
        }

        [Fact]
        public void ToDoTask_ShouldHandleCompletedAt()
        {
            var task = new ToDoTask();

            task.CompletedAt.Should().BeNull();

            var completionTime = DateTime.UtcNow;
            task.IsCompleted = true;
            task.CompletedAt = completionTime;


            task.CompletedAt.Should().Be(completionTime);
            task.IsCompleted.Should().BeTrue();
        }
    }
}
