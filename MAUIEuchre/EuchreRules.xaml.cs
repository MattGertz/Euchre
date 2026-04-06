namespace MAUIEuchre
{
    public partial class EuchreRules : ContentPage
    {
        public EuchreRules()
        {
            InitializeComponent();

            RulesWebView.Source = new HtmlWebViewSource { Html = AppResources.GetString("Rules_Html") };
        }

        private async void CloseBtn_Click(object? sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
