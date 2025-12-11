using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Pomodoro
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime StartTime { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EndTime { get; set; }

        public int? DurationMinutes { get; set; }
        public int WorkCycles { get; set; }

        public User? User { get; set; }
        public ToDoTask? Task { get; set; }
    }
}
