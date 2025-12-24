using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSEuchre4
{
    /// <summary>
    /// Interaction logic for EuchreBid2Control.xaml
    /// </summary>
    public partial class EuchreBid2Control : UserControl
    {
        #region "Public methods"
        public EuchreBid2Control()
        {
            InitializeComponent();
            this.Pass.Checked += Pass_Checked;
            this.Pass.Unchecked += Pass_Checked;
            this.OkButton.Click += OkButton_Click;
            this.KeyUp += EuchreBidControl_KeyUp;
            this.Loaded += EuchreBidControl_Loaded;
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

        #region "Private methods"
        private void SetTooltip(System.Windows.Controls.Control Ctrl, string Tip)
        {
            Ctrl.ToolTip = Tip;
        }
        #endregion

        #region "Event handlers"
        private void Pass_Checked(object sender, RoutedEventArgs e)
        {
            if (_modeQuietDealer && !(bool)Pass.IsChecked)
            {
                GoingAlone.IsEnabled = false;
                GoingAlone.Opacity = 0.25;
                GoingAlone.IsChecked = true;
            }
            else
            {
                GoingAlone.IsEnabled = !(bool)Pass.IsChecked;
                GoingAlone.Opacity = (bool)Pass.IsChecked ? 0.25 : 1.0;
                GoingAlone.IsChecked = false;
            }
        }

        private void EuchreBidControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetTooltip(Pass, Properties.Resources.BID_Pass1);
            SetTooltip(GoingAlone, Properties.Resources.BID_GoingAlone);
            SetTooltip(Hearts, Properties.Resources.BID_Hearts);
            SetTooltip(Diamonds, Properties.Resources.BID_Diamonds);
            SetTooltip(Clubs, Properties.Resources.BID_Clubs);
            SetTooltip(Spades, Properties.Resources.BID_Spades);
        }

        private void EuchreBidControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                gameTable.UpdateEuchreState(EuchreTable.EuchreState.StartNewGameRequested);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            IsEnabled = false;
            IsHitTestVisible = false;
            UpdateLayout();
            gameTable.PostHumanBidSecondRound();
        }
        #endregion

        #region "Public members"
        public EuchreTable gameTable = null!;
        #endregion

        #region "Private members"
        private bool _modeQuietDealer;
        #endregion
    }
}
