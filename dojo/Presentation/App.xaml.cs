﻿﻿using Presentation.Views;
using BLL.Interfaces;

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
            var loginPage = _services.GetRequiredService<LoginPage>();
            System.Diagnostics.Debug.WriteLine("✅ LoginPage created, showing login screen");
            return new Window(new NavigationPage(loginPage));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ CreateWindow ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }
}