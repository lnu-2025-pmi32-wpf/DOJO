using DAL.Models;
using FluentAssertions;
using Xunit;

namespace DAL.Tests.Models
{
    public class GoalTests
    {
        [Fact]
        public void Goal_ShouldInitializeWithDefaultValues()
        {
            var goal = new Goal();

            goal.Id.Should().Be(0);
            goal.UserId.Should().Be(0);
            goal.Description.Should().BeEmpty();
            goal.Progress.Should().Be(0);
            goal.Priority.Should().Be(1); 
            goal.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void Goal_ShouldSetPropertiesCorrectly()
        {
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(7);
            
            var goal = new Goal
            {
                UserId = 1,
                Description = "Complete project",
                StartTime = startTime,
                EndTime = endTime,
                Progress = 50.5f,
                Priority = 2,
                IsCompleted = true
            };

            goal.UserId.Should().Be(1);
            goal.Description.Should().Be("Complete project");
            goal.StartTime.Should().Be(startTime);
            goal.EndTime.Should().Be(endTime);
            goal.Progress.Should().Be(50.5f);
            goal.Priority.Should().Be(2);
            goal.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void Goal_ShouldAllowNullableDates()
        {
            var goal = new Goal();

            goal.StartTime.Should().BeNull();
            goal.EndTime.Should().BeNull();
        }

        [Fact]
        public void Goal_ShouldHandleTasksCollection()
        {
            var goal = new Goal();
            var task1 = new ToDoTask { Description = "Task 1" };
            var task2 = new ToDoTask { Description = "Task 2" };

            goal.Tasks = new List<ToDoTask> { task1, task2 };

            goal.Tasks.Should().HaveCount(2);
            goal.Tasks.Should().Contain(t => t.Description == "Task 1");
            goal.Tasks.Should().Contain(t => t.Description == "Task 2");
        }
    }
}
