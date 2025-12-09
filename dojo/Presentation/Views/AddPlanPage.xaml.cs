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

        private void OnStartDateTapped(object sender, EventArgs e)
        {
            StartDatePicker.Focus();
        }

        private void OnStartTimeTapped(object sender, EventArgs e)
        {
            StartTimePicker.Focus();
        }

        private void OnEndDateTapped(object sender, EventArgs e)
        {
            EndDatePicker.Focus();
        }

        private void OnEndTimeTapped(object sender, EventArgs e)
        {
            EndTimePicker.Focus();
        }
    }
}

