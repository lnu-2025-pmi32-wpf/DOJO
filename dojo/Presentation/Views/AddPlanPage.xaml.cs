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

        private void OnStartDateSelected(object sender, DateChangedEventArgs e)
        {
            // Біндинг оновить StartDateText автоматично через ViewModel
        }

        private void OnEndDateTapped(object sender, EventArgs e)
        {
            EndDatePicker.Focus();
        }

        private void OnEndDateSelected(object sender, DateChangedEventArgs e)
        {
            // Біндинг оновить EndDateText автоматично через ViewModel
        }

        private void OnStartTimeTapped(object sender, EventArgs e)
        {
            StartTimePicker.Focus();
        }

        private void OnStartTimeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                // Біндинг оновить StartTimeText автоматично через ViewModel
            }
        }

        private void OnEndTimeTapped(object sender, EventArgs e)
        {
            EndTimePicker.Focus();
        }

        private void OnEndTimeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                // Біндинг оновить EndTimeText автоматично через ViewModel
            }
        }
    }
}

