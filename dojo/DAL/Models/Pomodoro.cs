namespace dojo.Models
{
    public class Pomodoro
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMinutes { get; set; }
        public int WorkCycles { get; set; }

        public User? User { get; set; }
        public ToDoTask? Task { get; set; }
    }
}
