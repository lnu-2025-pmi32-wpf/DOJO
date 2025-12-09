using Microsoft.EntityFrameworkCore;
using DAL.Models;
using System.Text.RegularExpressions;

namespace DAL
{
    public class DojoDbContext : DbContext
    {
        public DojoDbContext(DbContextOptions<DojoDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<ToDoTask> ToDoTasks { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Pomodoro> Pomodoros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Goal>().ToTable("goals");
            modelBuilder.Entity<ToDoTask>().ToTable("tasks");
            modelBuilder.Entity<Attachment>().ToTable("attachments");
            modelBuilder.Entity<Pomodoro>().ToTable("pomodoros");
            
            modelBuilder.Entity<User>(eb =>
            {
                eb.Property(u => u.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
                
                eb.Property(u => u.LastCompletionDate)
                  .HasColumnName("last_completion_date")
                  .HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Goal>(eb =>
            {
                eb.Property(g => g.StartTime)
                  .HasColumnName("start_time")
                  .HasColumnType("timestamp without time zone");
                
                eb.Property(g => g.EndTime)
                  .HasColumnName("end_time")
                  .HasColumnType("timestamp without time zone");
                  
                eb.Property(g => g.Priority)
                  .HasColumnName("priority")
                  .HasDefaultValue(1);
                
                eb.Property(g => g.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();

                eb.Property(g => g.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<ToDoTask>(eb =>
            {
                eb.Property(t => t.DueDate)
                  .HasColumnName("due_date")
                  .HasColumnType("timestamp without time zone");

                eb.Property(t => t.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();

                eb.Property(t => t.CompletedAt)
                  .HasColumnName("completed_at")
                  .HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Pomodoro>(eb =>
            {
                eb.Property(p => p.DurationMinutes)
                  .HasColumnName("duration_minutes");

                eb.Property(p => p.StartTime)
                  .HasColumnName("start_time")
                  .HasColumnType("timestamp without time zone");
                  
                eb.Property(p => p.EndTime)
                  .HasColumnName("end_time")
                  .HasColumnType("timestamp without time zone");
                  
                eb.Property(p => p.WorkCycles).HasColumnName("work_cycles");
                
                eb.HasOne(p => p.Task)
                  .WithMany()
                  .HasForeignKey(p => p.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Attachment>(eb =>
            {
                eb.Property(a => a.FilePath).HasColumnName("file_path");
                eb.Property(a => a.FileName).HasColumnName("file_name");
                eb.Property(a => a.TaskId).HasColumnName("task_id");
                eb.Property(a => a.GoalId).HasColumnName("goal_id");
                
                eb.HasOne(a => a.Task)
                  .WithMany()
                  .HasForeignKey(a => a.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
                eb.HasOne(a => a.Goal)
                  .WithMany()
                  .HasForeignKey(a => a.GoalId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
            
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }
        
        private static string ToSnakeCase(string name)
        {
            return Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
        }
    }
}
