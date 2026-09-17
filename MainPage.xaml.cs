using Microsoft.AspNetCore.Components.WebView.Maui;

namespace AgileClass;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var routesType = Type.GetType("AgileClass.Components.Routes, AgileClass")
            ?? throw new InvalidOperationException("Unable to resolve Blazor root component type.");

        BlazorHost.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = routesType
        });
    }
}
