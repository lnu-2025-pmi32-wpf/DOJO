using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class AddPlanPage : ContentPage
    {
        public AddPlanPage()
        {
            InitializeComponent();
            BindingContext = new AddPlanViewModel();
        }
    }
}

