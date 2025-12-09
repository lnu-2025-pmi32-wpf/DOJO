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
            // Важливо! Дозволяємо Npgsql працювати з DateTime без timezone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=14122005Ad";
            builder.Services.AddDbContext<DojoDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IToDoTaskService, ToDoTaskService>();
            builder.Services.AddTransient<IGoalService, GoalService>();
            builder.Services.AddTransient<IPomodoroService, PomodoroService>();

            builder.Services.AddSingleton<MainViewModel>(sp => 
                new MainViewModel(
                    sp.GetService<ISessionService>(),
                    sp.GetService<IPomodoroService>(),
                    sp));
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<AddPlanViewModel>(sp =>
                new AddPlanViewModel(
                    sp.GetRequiredService<IGoalService>(),
                    sp.GetRequiredService<ISessionService>()));
            builder.Services.AddTransient<ViewPlanViewModel>(sp =>
                new ViewPlanViewModel(sp.GetRequiredService<IGoalService>()));
            builder.Services.AddTransient<StatisticsViewModel>();

            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<AddPlanPage>();
            builder.Services.AddTransient<ViewPlanPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            
            return app;
        }
    }
}