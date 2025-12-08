using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DAL;
using BLL.Services;
using BLL.Interfaces;
using Presentation.Views;
using Presentation.ViewModels;
using Presentation.Services;

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
            string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=";
            builder.Services.AddDbContext<DojoDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Реєстрація сервісів
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToDoTaskService, ToDoTaskService>();
            builder.Services.AddScoped<IGoalService, GoalService>();
            builder.Services.AddScoped<IPomodoroService, PomodoroService>();

            // Реєстрація ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<MainViewModel>(sp => 
                new MainViewModel(
                    sp.GetService<ISessionService>(),
                    sp.GetService<IPomodoroService>(),
                    sp.GetService<IGoalService>()));
            builder.Services.AddTransient<AddPlanViewModel>(sp =>
                new AddPlanViewModel(
                    sp.GetRequiredService<IGoalService>(),
                    sp.GetRequiredService<ISessionService>()));
            builder.Services.AddTransient<StatisticsViewModel>();

            // Реєстрація Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<AddPlanPage>();
            builder.Services.AddTransient<StatisticsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}