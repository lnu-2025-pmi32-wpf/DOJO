using Presentation.ViewModels;
using BLL.Interfaces;

namespace Presentation.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ISessionService _sessionService;

        public LoginPage(LoginViewModel viewModel, ISessionService sessionService)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _sessionService = sessionService;
        }

        // TODO: Додамо автологін після того як хоч раз залогінимось
        //protected override async void OnAppearing()
        //{
        //    base.OnAppearing();
        //    var isLoggedIn = await _sessionService.IsLoggedInAsync();
        //    if (isLoggedIn)
        //    {
        //        await Shell.Current.GoToAsync($"//DashboardPage");
        //    }
        //}
    }
}

