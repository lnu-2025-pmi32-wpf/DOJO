using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DAL;
using BLL.Services;
using BLL.Interfaces;
using Presentation.Views;
using Presentation.ViewModels;
using Presentation.Services;
using Serilog;
using Serilog.Events;
using Microsoft.Maui.Storage;

namespace Presentation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Використовуємо крос-платформену AppData папку для логів
            string appData;
            try
            {
                appData = FileSystem.AppDataDirectory;
            }
            catch
            {
                appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(appData))
                    appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }

            var logsFolder = Path.Combine(appData, "DojoLogs");
            if (!Directory.Exists(logsFolder))
                Directory.CreateDirectory(logsFolder);

            var logPath = Path.Combine(logsFolder, $"dojo-{DateTime.Now:yyyy-MM-dd}.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("🚀 Програма Dojo запущена");
            Log.Information("📁 Логи зберігаються в: {LogPath}", logPath);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=A_p131205Kk%";
            builder.Services.AddDbContext<DojoDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Services
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToDoTaskService, ToDoTaskService>();
            builder.Services.AddScoped<IGoalService, GoalService>();
            builder.Services.AddScoped<IPomodoroService, PomodoroService>();
            builder.Services.AddScoped<IExperienceService, ExperienceService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            
            builder.Services.AddTransient<MainViewModel>(sp => 
                new MainViewModel(
                    sp.GetRequiredService<ISessionService>(),
                    sp.GetRequiredService<IPomodoroService>(),
                    sp,
                    sp.GetRequiredService<IToDoTaskService>(),
                    sp.GetRequiredService<IExperienceService>())); 
                    
            builder.Services.AddTransient<AddPlanViewModel>(sp =>
                new AddPlanViewModel(
                    sp.GetRequiredService<IGoalService>(),
                    sp.GetRequiredService<ISessionService>()));
                    
            builder.Services.AddTransient<ViewPlanViewModel>(sp =>
                new ViewPlanViewModel(sp.GetRequiredService<IGoalService>()));
            
            builder.Services.AddTransient<StatisticsViewModel>(sp =>
                new StatisticsViewModel(
                    sp.GetRequiredService<IToDoTaskService>(),
                    sp.GetRequiredService<ISessionService>()));
            
            builder.Services.AddTransient<AddTodoViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<AddPlanPage>();
            builder.Services.AddTransient<ViewPlanPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<AddTodoPopup>();
            builder.Services.AddTransient<AppShell>();

            Log.Information("✅ Всі сервіси та сторінки зареєстровано");

            var app = builder.Build();

            Log.Information("✅ Програма успішно налаштована");

            return app;
        }
    }
}