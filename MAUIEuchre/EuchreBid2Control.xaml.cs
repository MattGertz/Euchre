namespace MAUIEuchre
{
    public partial class EuchreBid2Control : ContentView
    {
        #region "Public methods"
        public EuchreBid2Control()
        {
            InitializeComponent();
            Pass.CheckedChanged += Pass_CheckedChanged;
            Hearts.CheckedChanged += SuitRadio_CheckedChanged;
            Diamonds.CheckedChanged += SuitRadio_CheckedChanged;
            Clubs.CheckedChanged += SuitRadio_CheckedChanged;
            Spades.CheckedChanged += SuitRadio_CheckedChanged;
        }

        public void ForceGoAlone(bool forced)
        {
            _modeQuietDealer = forced;
        }

        public void Reset()
        {
            Pass.IsChecked = true;
            Pass.IsEnabled = true;
            Pass.Opacity = 1.0;
            GoingAlone.IsChecked = false;
            GoingAlone.IsEnabled = false;
            GoingAlone.Opacity = 0.25;
            GoingAloneLabel.Opacity = 0.25;

            Hearts.IsEnabled = true;
            Hearts.Opacity = 1.0;
            Diamonds.IsEnabled = true;
            Diamonds.Opacity = 1.0;
            Clubs.IsEnabled = true;
            Clubs.Opacity = 1.0;
            Spades.IsEnabled = true;
            Spades.Opacity = 1.0;
        }
        #endregion

        #region "Event handlers"
        private void Pass_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                GoingAlone.IsEnabled = false;
                GoingAlone.Opacity = 0.25;
                GoingAloneLabel.Opacity = 0.25;
                GoingAlone.IsChecked = false;
            }
        }

        private void SuitRadio_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                if (_modeQuietDealer)
                {
                    GoingAlone.IsEnabled = false;
                    GoingAlone.Opacity = 0.25;
                    GoingAloneLabel.Opacity = 0.25;
                    GoingAlone.IsChecked = true;
                }
                else
                {
                    GoingAlone.IsEnabled = true;
                    GoingAlone.Opacity = 1.0;
                    GoingAloneLabel.Opacity = 1.0;
                    GoingAlone.IsChecked = false;
                }
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            IsVisible = false;
            IsEnabled = false;
            InputTransparent = true;
            gameTable?.PostHumanBidSecondRound();
        }
        #endregion

        #region "Public members"
        public EuchreTable? gameTable;
        #endregion

        #region "Private members"
        private bool _modeQuietDealer;
        #endregion
    }
}
