﻿﻿using Presentation.Views;

namespace Presentation;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = Handler?.MauiContext?.Services.GetService<LoginPage>();
        return new Window(loginPage ?? new LoginPage(null!));
    }
}