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

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } 
        
        [Column(TypeName = "timestamp without time zone")]
        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
        public ICollection<ToDoTask>? Tasks { get; set; }
    }
}