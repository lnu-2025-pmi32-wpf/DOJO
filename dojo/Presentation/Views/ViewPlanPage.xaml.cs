using Presentation.ViewModels;

namespace Presentation.Views;

public partial class ViewPlanPage : ContentPage
{
    public ViewPlanPage(ViewPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
