namespace DAL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
        public int ExpPoints { get; set; }
        public int Level { get; set; }
        public int CurrentStreak { get; set; }
        public DateTime? LastCompletionDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Goal>? Goals { get; set; }
        public ICollection<ToDoTask>? Tasks { get; set; }
        public ICollection<Pomodoro>? Pomodoros { get; set; }
    }
}