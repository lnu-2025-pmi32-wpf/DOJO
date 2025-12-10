using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class AddTodoPopup : ContentPage
    {
        public AddTodoPopup(AddTodoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}