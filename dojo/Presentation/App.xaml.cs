using System.Linq;
using BLL.Interfaces;
using Presentation.Views;

namespace Presentation;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();

        // Глобальний обробник винятків
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"❌❌❌ UNHANDLED EXCEPTION ❌❌❌");
            System.Diagnostics.Debug.WriteLine($"Message: {ex?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex?.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner: {ex?.InnerException?.Message}");
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ UNOBSERVED TASK EXCEPTION ❌❌❌");
            System.Diagnostics.Debug.WriteLine($"Message: {e.Exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {e.Exception?.StackTrace}");
            e.SetObserved();
        };

        System.Diagnostics.Debug.WriteLine("🔹 App started");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔹 CreateWindow started");

            var sessionService = _services.GetRequiredService<ISessionService>();

            var hasSession = Task.Run(async () =>
            {
                var session = await sessionService.GetUserSessionAsync();
                return session.HasValue;
            }).Result;

            if (hasSession)
            {
                System.Diagnostics.Debug.WriteLine("✅ Session found, showing AppShell");
                var appShell = _services.GetRequiredService<AppShell>();
                return new Window(appShell);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No session found, showing LoginPage");
                var loginPage = _services.GetRequiredService<LoginPage>();
                return new Window(new NavigationPage(loginPage));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ CreateWindow ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }
}
