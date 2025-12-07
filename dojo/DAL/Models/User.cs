using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Column("username")]
        [MaxLength(100)]
        public string? Username { get; set; }
        
        [Required]
        [Column("password")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty; 
        
        [Column("exp_points")]
        public int ExpPoints { get; set; }
        
        [Column("level")]
        public int Level { get; set; }
        
        [Column("current_streak")]
        public int CurrentStreak { get; set; }
        
        [Column("last_completion_date")]
        public DateTime? LastCompletionDate { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public ICollection<Goal>? Goals { get; set; }
        public ICollection<ToDoTask>? Tasks { get; set; }
        public ICollection<Pomodoro>? Pomodoros { get; set; }
    }
}