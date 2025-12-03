namespace Presentation.Models
{
    public class TodoItemModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

