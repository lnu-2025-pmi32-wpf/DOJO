using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? StartTime { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EndTime { get; set; }

        public float Progress { get; set; }

        // 0 = Low, 1 = Normal, 2 = High
        public int Priority { get; set; } = 1;

        // Чи виконаний план
        public bool IsCompleted { get; set; } = false;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
        public ICollection<ToDoTask>? Tasks { get; set; }
    }
}
