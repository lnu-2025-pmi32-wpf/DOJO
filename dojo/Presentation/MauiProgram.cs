using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DAL;
using BLL.Services;


namespace Presentation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Підключення до бази даних
            string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=postgre2006";
            builder.Services.AddDbContext<DojoDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Реєстрація сервісів
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToDoTaskService, ToDoTaskService>();
            builder.Services.AddScoped<IGoalService, GoalService>();
            builder.Services.AddScoped<IPomodoroService, PomodoroService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}