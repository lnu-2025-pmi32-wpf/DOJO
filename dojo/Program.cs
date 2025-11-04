using System;
using Microsoft.EntityFrameworkCore;
using dojo;

class Program
{
    static void Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DojoDbContext>();
        string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=0673737982vlad";
        optionsBuilder.UseNpgsql(connectionString);
        
        try
        {
            using (var context = new DojoDbContext(optionsBuilder.Options))
            {
                bool canConnect = context.Database.CanConnect();
                
                if (canConnect)
                {
                    Console.WriteLine("Correct connect!");
                    
                    string actualDbName = context.Database.GetDbConnection().Database;
                    Console.WriteLine($"Data Base: {actualDbName}");
                    Console.WriteLine();

                    Console.WriteLine($"Users count: {context.Users.Count()}");
                    Console.WriteLine($"ToDoTask count: {context.ToDoTasks.Count()}");
                    Console.WriteLine($"Goal count: {context.Goals.Count()}");
                    Console.WriteLine($"Attachments count: {context.Attachments.Count()}");
                    Console.WriteLine($"Pomodoro count: {context.Pomodoros.Count()}");
                }
                else
                {
                    Console.WriteLine("✗ No connection to the database.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Details: {ex.InnerException?.Message}");
        }
    }
}
