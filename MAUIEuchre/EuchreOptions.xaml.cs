namespace MAUIEuchre
{
    public partial class EuchreOptions : ContentPage
    {
        #region "Public properties"
        public bool LocalDialogResult { get; set; }
        #endregion

        #region "Public methods"
        public EuchreOptions()
        {
            InitializeComponent();
            UpdateSettings();
        }

        public void UpdateSettings()
        {
            PlayerName.Text = GameSettings.PlayerName;
            PartnerName.Text = GameSettings.PartnerName;
            LeftOpponentName.Text = GameSettings.LeftOpponentName;
            RightOpponentName.Text = GameSettings.RightOpponentName;

            StickTheDealer.IsChecked = GameSettings.StickTheDealer;
            NineOfHearts.IsChecked = GameSettings.NineOfHearts;
            SuperEuchre.IsChecked = GameSettings.SuperEuchre;
            QuietDealer.IsChecked = GameSettings.QuietDealer;
            PeekAtOtherCards.IsChecked = GameSettings.PeekAtOtherCards;
            SoundOn.IsChecked = GameSettings.SoundOn;

            LeftOpponentCrazy.IsChecked = (GameSettings.LeftOpponentPlay == 1);
            LeftOpponentNormal.IsChecked = (GameSettings.LeftOpponentPlay != 1 && GameSettings.LeftOpponentPlay != 3);
            LeftOpponentConservative.IsChecked = (GameSettings.LeftOpponentPlay == 3);

            PartnerCrazy.IsChecked = (GameSettings.PartnerPlay == 1);
            PartnerNormal.IsChecked = (GameSettings.PartnerPlay != 1 && GameSettings.PartnerPlay != 3);
            PartnerConservative.IsChecked = (GameSettings.PartnerPlay == 3);

            RightOpponentCrazy.IsChecked = (GameSettings.RightOpponentPlay == 1);
            RightOpponentNormal.IsChecked = (GameSettings.RightOpponentPlay != 1 && GameSettings.RightOpponentPlay != 3);
            RightOpponentConservative.IsChecked = (GameSettings.RightOpponentPlay == 3);
        }
        #endregion

        #region "Event handlers"
        private async void OKBtn_Click(object sender, EventArgs e)
        {
            GameSettings.PlayerName = PlayerName.Text;
            GameSettings.PartnerName = PartnerName.Text;
            GameSettings.LeftOpponentName = LeftOpponentName.Text;
            GameSettings.RightOpponentName = RightOpponentName.Text;

            GameSettings.StickTheDealer = StickTheDealer.IsChecked;
            GameSettings.NineOfHearts = NineOfHearts.IsChecked;
            GameSettings.SuperEuchre = SuperEuchre.IsChecked;
            GameSettings.QuietDealer = QuietDealer.IsChecked;
            GameSettings.PeekAtOtherCards = PeekAtOtherCards.IsChecked;
            GameSettings.SoundOn = SoundOn.IsChecked;

            if (LeftOpponentCrazy.IsChecked)
                GameSettings.LeftOpponentPlay = 1;
            else if (LeftOpponentNormal.IsChecked)
                GameSettings.LeftOpponentPlay = 2;
            else
                GameSettings.LeftOpponentPlay = 3;

            if (PartnerCrazy.IsChecked)
                GameSettings.PartnerPlay = 1;
            else if (PartnerNormal.IsChecked)
                GameSettings.PartnerPlay = 2;
            else
                GameSettings.PartnerPlay = 3;

            if (RightOpponentCrazy.IsChecked)
                GameSettings.RightOpponentPlay = 1;
            else if (RightOpponentNormal.IsChecked)
                GameSettings.RightOpponentPlay = 2;
            else
                GameSettings.RightOpponentPlay = 3;

            LocalDialogResult = true;
            await Navigation.PopModalAsync();
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            GameSettings.Reset();
            UpdateSettings();
        }
        #endregion
    }
}
