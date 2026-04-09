namespace MAUIEuchre;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new SplashPage());
		window.Title = "Matt's Euchre";
#if WINDOWS
		window.Width = 1080;
		window.Height = 800;
#endif
		return window;
	}
}