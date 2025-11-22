namespace DAL.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public float Progress { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
        public ICollection<ToDoTask>? Tasks { get; set; }
    }
}