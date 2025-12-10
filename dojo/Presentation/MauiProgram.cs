using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DAL;
using BLL.Services;
using BLL. Interfaces;
using Presentation. Views;
using Presentation.ViewModels;
using Presentation. Services;

namespace Presentation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string connectionString = "Host=localhost;Database=dojo;Username=postgres;Password=A_p131205Kk%";
            builder.Services.AddDbContext<DojoDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Services
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services. AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IToDoTaskService, ToDoTaskService>();
            builder.Services.AddScoped<IGoalService, GoalService>();
            builder.Services.AddScoped<IPomodoroService, PomodoroService>();
            builder.Services.AddScoped<IExperienceService, ExperienceService>();

            // ViewModels
            builder.Services. AddTransient<LoginViewModel>();
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
                    
            builder. Services.AddTransient<ViewPlanViewModel>(sp =>
                new ViewPlanViewModel(sp.GetRequiredService<IGoalService>()));
            
            // ✅ ВИПРАВЛЕНО: StatisticsViewModel з DI
            builder.Services. AddTransient<StatisticsViewModel>(sp =>
                new StatisticsViewModel(
                    sp.GetRequiredService<IToDoTaskService>(),
                    sp.GetRequiredService<ISessionService>()));
            
            builder.Services.AddTransient<AddTodoViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<LoginPage>();
            builder. Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<AddPlanPage>();
            builder.Services.AddTransient<ViewPlanPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<AddTodoPopup>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            
            return app;
        }
    }
}