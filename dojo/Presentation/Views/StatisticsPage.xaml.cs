using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class StatisticsPage : ContentPage
    {
        public StatisticsPage()
        {
            InitializeComponent();
            BindingContext = new StatisticsViewModel();
        }
    }
}

