using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class StatisticsPage : ContentPage
    {
        public StatisticsPage(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}