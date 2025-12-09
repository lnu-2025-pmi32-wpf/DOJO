using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class AddPlanPage : ContentPage
    {
        public AddPlanPage(AddPlanViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

