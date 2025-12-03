using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }
    }
}

