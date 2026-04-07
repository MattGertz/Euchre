namespace MAUIEuchre;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(2000);
        Application.Current!.Windows[0].Page = new AppShell();
    }
}
