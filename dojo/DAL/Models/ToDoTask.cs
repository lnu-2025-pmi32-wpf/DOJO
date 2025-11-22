namespace DAL.Models
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? GoalId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public User? User { get; set; }
        public Goal? Goal { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
    }
}


