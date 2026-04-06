namespace MAUIEuchre
{
    public partial class EuchreBidControl : ContentView
    {
        #region "Public methods"
        public EuchreBidControl()
        {
            InitializeComponent();
            Pass.CheckedChanged += Pass_CheckedChanged;
            PickItUp.CheckedChanged += PickItUp_CheckedChanged;
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
            PickItUp.IsChecked = false;
            GoingAlone.IsChecked = false;
            GoingAlone.IsEnabled = false;
            GoingAlone.Opacity = 0.25;
            GoingAloneLabel.Opacity = 0.25;
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

        private void PickItUp_CheckedChanged(object? sender, CheckedChangedEventArgs e)
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
            gameTable?.PostHumanBidFirstRound();
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
