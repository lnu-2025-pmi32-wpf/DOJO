using Microsoft.Maui.Controls;
using Presentation.Views;

namespace Presentation;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(AddPlanPage), typeof(AddPlanPage));
        Routing.RegisterRoute(nameof(ViewPlanPage), typeof(ViewPlanPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
    }

    // TODO: Увімкнемо автологін після першого успішного логіну
}