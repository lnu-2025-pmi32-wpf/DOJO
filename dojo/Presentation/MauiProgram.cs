using Microsoft.Extensions.Logging;
using Microsoft. EntityFrameworkCore;
using DAL;
using BLL. Services;
using BLL.Interfaces;
using Presentation. Views;
using Presentation.ViewModels;
using Presentation.Services;

namespace Presentation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                System.Diagnostics. Debug.WriteLine("🔹 MauiProgram:  Початок ініціалізації.. .");
                
                // Важливо! Дозволяємо Npgsql працювати з DateTime без timezone
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                
                var builder = MauiApp.CreateBuilder();
                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular. ttf", "OpenSansRegular");
                        fonts. AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                // ✅ Підключення до бази даних з обробкою помилок
                string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=24062006";
                builder. Services.AddDbContext<DojoDbContext>(options =>
                {
                    options. UseNpgsql(connectionString);
                    // ✅ Вимикаємо відстеження для кращої продуктивності
                    options. UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

                // Реєстрація сервісів
                builder.Services.AddSingleton<ISessionService, SessionService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IToDoTaskService, ToDoTaskService>();
                builder.Services.AddScoped<IGoalService, GoalService>();
                builder.Services.AddScoped<IPomodoroService, PomodoroService>();

                // Реєстрація ViewModels
                builder.Services.AddTransient<LoginViewModel>();
                builder.Services.AddTransient<RegisterViewModel>();
                
                // ✅ MainViewModel отримує IServiceProvider для створення scope
                builder.Services.AddTransient<MainViewModel>(sp => 
                    new MainViewModel(
                        sp.GetRequiredService<ISessionService>(),
                        sp.GetRequiredService<IPomodoroService>(),
                        sp));
                
                builder.Services.AddTransient<AddPlanViewModel>(sp =>
                    new AddPlanViewModel(
                        sp.GetRequiredService<IGoalService>(),
                        sp.GetRequiredService<ISessionService>()));
                
                builder. Services.AddTransient<ViewPlanViewModel>(sp =>
                    new ViewPlanViewModel(sp.GetRequiredService<IGoalService>()));
                
                builder.Services.AddTransient<StatisticsViewModel>();

                // Реєстрація Views
                builder.Services.AddTransient<DashboardPage>();
                builder.Services.AddTransient<LoginPage>();
                builder.Services.AddTransient<RegisterPage>();
                builder. Services.AddTransient<AddPlanPage>();
                builder. Services.AddTransient<ViewPlanPage>();
                builder. Services.AddTransient<StatisticsPage>();
                builder. Services.AddTransient<AppShell>();

#if DEBUG
                builder.Logging.AddDebug();
#endif

                System.Diagnostics. Debug.WriteLine("✅ MauiProgram: Сервіси зареєстровано");
                
                var app = builder.Build();
                
                System.Diagnostics. Debug.WriteLine("✅ MauiProgram: Додаток створено успішно");
                
                return app;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MauiProgram CRITICAL ERROR: {ex.Message}");
                System.Diagnostics. Debug.WriteLine($"Stack:  {ex.StackTrace}");
                throw;
            }
        }
    }
}