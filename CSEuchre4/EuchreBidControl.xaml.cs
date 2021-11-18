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
    /// Interaction logic for EuchreBidControl.xaml
    /// </summary>
    public partial class EuchreBidControl : UserControl
    {
        #region "Public methods"
        public EuchreBidControl()
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
            SetTooltip(PickItUp, Properties.Resources.BID_PickItUp);
            SetTooltip(GoingAlone, Properties.Resources.BID_GoingAlone);
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
            gameTable.PostHumanBidFirstRound();
        }        
        #endregion

        #region "Public members"
        public EuchreTable gameTable;
        #endregion

        #region "Private members"
        private bool _modeQuietDealer;
        #endregion
    }
}
