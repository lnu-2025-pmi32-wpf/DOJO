using Microsoft.Maui.Controls;
using Presentation.Views;

namespace Presentation;

public partial class AppShell : Shell
{
    public AppShell(DashboardPage dashboardPage)
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(AddPlanPage), typeof(AddPlanPage));
        Routing.RegisterRoute(nameof(ViewPlanPage), typeof(ViewPlanPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
        
        // Set the DashboardPage content directly
        DashboardContent.Content = dashboardPage;
    }
}