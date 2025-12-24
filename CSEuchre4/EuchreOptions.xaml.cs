using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSEuchre4
{
    /// <summary>
    /// Interaction logic for EuchreOptions.xaml
    /// </summary>
    public partial class EuchreOptions : Window
    {
        #region "Public methods"
        public EuchreOptions()
        {
            InitializeComponent();

            this.Closing += EuchreOptions_Closing;
            this.Loaded += EuchreOptions_Load;
            this.OKBtn.Click += OKBtn_Click;
            this.CancelBtn.Click += CancelBtn_Click;
            this.ResetBtn.Click += CancelBtn_Click;
            this.LeftVoiceCombo.DropDownClosed += LeftVoiceCombo_DropDownClosed;
            this.PartnerVoiceCombo.DropDownClosed += PartnerVoiceCombo_DropDownClosed;
            this.RightVoiceCombo.DropDownClosed += RightVoiceCombo_DropDownClosed;
        }

        public void UpdateSettings()
        {
            SetTooltip(PlayerName, Properties.Resources.OPTION_Name);
            SetTooltip(StickTheDealer, Properties.Resources.OPTION_StickTheDealer);
            SetTooltip(NineOfHearts, Properties.Resources.OPTION_NineOfHearts);
            SetTooltip(SuperEuchre, Properties.Resources.OPTION_SuperEuchre);
            SetTooltip(QuietDealer, Properties.Resources.OPTION_QuietDealer);
            SetTooltip(PeekAtOtherCards, Properties.Resources.OPTION_PeekAtOtherCards);
            SetTooltip(SoundOn, Properties.Resources.OPTION_SoundEffects);
            SetTooltip(LeftOpponentName, Properties.Resources.OPTION_LeftOpponent);
            SetTooltip(PartnerName, Properties.Resources.OPTION_YourPartner);
            SetTooltip(RightOpponentName, Properties.Resources.OPTION_RightOpponent);
            SetTooltip(LeftVoiceCombo, Properties.Resources.OPTION_LeftOpponentVoice);
            SetTooltip(PartnerVoiceCombo, Properties.Resources.OPTION_YourPartnerVoice);
            SetTooltip(RightVoiceCombo, Properties.Resources.OPTION_RightOpponentVoice);
            SetTooltip(LeftOpponentCrazy, Properties.Resources.OPTION_Crazy);
            SetTooltip(LeftOpponentNormal, Properties.Resources.OPTION_Normal);
            SetTooltip(LeftOpponentConservative, Properties.Resources.OPTION_Conservative);
            SetTooltip(PartnerCrazy, Properties.Resources.OPTION_Crazy);
            SetTooltip(PartnerNormal, Properties.Resources.OPTION_Normal);
            SetTooltip(PartnerConservative, Properties.Resources.OPTION_Conservative);
            SetTooltip(RightOpponentCrazy, Properties.Resources.OPTION_Crazy);
            SetTooltip(RightOpponentNormal, Properties.Resources.OPTION_Normal);
            SetTooltip(RightOpponentConservative, Properties.Resources.OPTION_Conservative);

            PlayerName.Text = Properties.Settings.Default.PlayerName;
            PartnerName.Text = Properties.Settings.Default.PartnerName;
            LeftOpponentName.Text = Properties.Settings.Default.LeftOpponentName;
            RightOpponentName.Text = Properties.Settings.Default.RightOpponentName;

            LeftVoiceCombo.SelectedIndex = (Properties.Settings.Default.LeftOpponentVoice < _voiceCount) ? Properties.Settings.Default.LeftOpponentVoice : 0;
            PartnerVoiceCombo.SelectedIndex = (Properties.Settings.Default.PartnerVoice < _voiceCount) ? Properties.Settings.Default.PartnerVoice : 0;
            RightVoiceCombo.SelectedIndex = (Properties.Settings.Default.RightOpponentVoice < _voiceCount) ? Properties.Settings.Default.RightOpponentVoice : 0;

            StickTheDealer.IsChecked = Properties.Settings.Default.StickTheDealer;
            NineOfHearts.IsChecked = Properties.Settings.Default.NineOfHearts;
            SuperEuchre.IsChecked = Properties.Settings.Default.SuperEuchre;
            QuietDealer.IsChecked = Properties.Settings.Default.QuietDealer;
            PeekAtOtherCards.IsChecked = Properties.Settings.Default.PeekAtOtherCards;
            SoundOn.IsChecked = Properties.Settings.Default.SoundOn;

            LeftOpponentCrazy.IsChecked = (Properties.Settings.Default.LeftOpponentPlay == 1);
            LeftOpponentNormal.IsChecked = (Properties.Settings.Default.LeftOpponentPlay != 1 && Properties.Settings.Default.LeftOpponentPlay != 3);
            LeftOpponentConservative.IsChecked = (Properties.Settings.Default.LeftOpponentPlay == 3);

            PartnerCrazy.IsChecked = (Properties.Settings.Default.PartnerPlay == 1);
            PartnerNormal.IsChecked = (Properties.Settings.Default.PartnerPlay != 1 && Properties.Settings.Default.PartnerPlay != 3);
            PartnerConservative.IsChecked = (Properties.Settings.Default.PartnerPlay == 3);

            RightOpponentCrazy.IsChecked = (Properties.Settings.Default.RightOpponentPlay == 1);
            RightOpponentNormal.IsChecked = (Properties.Settings.Default.RightOpponentPlay != 1 && Properties.Settings.Default.RightOpponentPlay != 3);
            RightOpponentConservative.IsChecked = (Properties.Settings.Default.RightOpponentPlay == 3);

        }

        public bool LocalDialogResult { get; set; }

        public void DisposeVoice()
        {
            if (_voiceSynthesizer != null)
            {
                _voiceSynthesizer.Dispose();
                _voiceSynthesizer = null;
            }
        }

        #endregion

        #region "Private methods"
        private void Speak(string Name)
        {
            if (_voiceSynthesizer != null)
            {
                try
                {
                    _voiceSynthesizer.SelectVoice(Name);
                    _voiceSynthesizer.Speak(Properties.Resources.SAY_LetsPlayEuchre);
                }
                catch
                {
                }
            }
        }

        private void SetTooltip(System.Windows.Controls.Control Ctrl, string Tip)
        {
            Ctrl.ToolTip = Tip;
        }

        #endregion

        #region "Event Handlers"

        private void EuchreOptions_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LocalDialogResult)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void EuchreOptions_Load(object sender, RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.NoResize;
            EuchreTable.SetIcon(this, Properties.Resources.Euchre);
            if (_voiceSynthesizer == null)
            { // Haven't already initialized it
                try
                {
                    _voiceSynthesizer = new SpeechSynthesizer();
                }
                catch
                {
                }
                LeftVoiceCombo.IsEnabled = false;
                PartnerVoiceCombo.IsEnabled = false;
                RightVoiceCombo.IsEnabled = false;
                if (_voiceSynthesizer != null)
                {
                    var Voices = _voiceSynthesizer.GetInstalledVoices();
                    _voiceCount = Voices.Count;
                    if (_voiceCount > 0)
                    {
                        for (int i = 0; i < _voiceCount; i++)
                        {
                            // As of Windows 8, it seems that the synthesizer will automatically covert the voice formats.
                            // This is definitely not true for Win7 and earlier

                            OperatingSystem osVersion = System.Environment.OSVersion;
                            bool supportsVoiceConversion = osVersion.Version.Major > 6 || (osVersion.Version.Major == 6 && osVersion.Version.Minor >= 2);
                            if (supportsVoiceConversion || Voices[i].VoiceInfo.SupportedAudioFormats.Count > 0)
                            {
                                string s = Voices[i].VoiceInfo.Name;
                                LeftVoiceCombo.Items.Add(s);
                                PartnerVoiceCombo.Items.Add(s);
                                RightVoiceCombo.Items.Add(s);
                            }
                        }
                        LeftVoiceCombo.IsEnabled = true;
                        PartnerVoiceCombo.IsEnabled = true;
                        RightVoiceCombo.IsEnabled = true;
                    }
                }
            }

            UpdateSettings();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PlayerName = PlayerName.Text;
            Properties.Settings.Default.PartnerName = PartnerName.Text;
            Properties.Settings.Default.LeftOpponentName = LeftOpponentName.Text;
            Properties.Settings.Default.RightOpponentName = RightOpponentName.Text;

            Properties.Settings.Default.LeftOpponentVoice = LeftVoiceCombo.SelectedIndex;
            Properties.Settings.Default.PartnerVoice = PartnerVoiceCombo.SelectedIndex;
            Properties.Settings.Default.RightOpponentVoice = RightVoiceCombo.SelectedIndex;

            Properties.Settings.Default.StickTheDealer = StickTheDealer.IsChecked == true;
            Properties.Settings.Default.NineOfHearts = NineOfHearts.IsChecked == true;
            Properties.Settings.Default.SuperEuchre = SuperEuchre.IsChecked == true;
            Properties.Settings.Default.QuietDealer = QuietDealer.IsChecked == true;
            Properties.Settings.Default.PeekAtOtherCards = PeekAtOtherCards.IsChecked == true;
            Properties.Settings.Default.SoundOn = SoundOn.IsChecked == true;

            if (LeftOpponentCrazy.IsChecked == true)
                Properties.Settings.Default.LeftOpponentPlay = 1;
            else if (LeftOpponentNormal.IsChecked == true)
                Properties.Settings.Default.LeftOpponentPlay = 2;
            else
                Properties.Settings.Default.LeftOpponentPlay = 3;

            if (PartnerCrazy.IsChecked == true)
                Properties.Settings.Default.PartnerPlay = 1;
            else if (PartnerNormal.IsChecked == true)
                Properties.Settings.Default.PartnerPlay = 2;
            else
                Properties.Settings.Default.PartnerPlay = 3;

            if (RightOpponentCrazy.IsChecked == true)
                Properties.Settings.Default.RightOpponentPlay = 1;
            else if (RightOpponentNormal.IsChecked == true)
                Properties.Settings.Default.RightOpponentPlay = 2;
            else
                Properties.Settings.Default.RightOpponentPlay = 3;

            LocalDialogResult = true;
            DialogResult = false;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            LocalDialogResult = false;
            DialogResult = false;
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            UpdateSettings();
            UpdateLayout();
        }

        private void LeftVoiceCombo_DropDownClosed(object? sender, EventArgs e)
        {
            Speak(LeftVoiceCombo.Text);
        }

        private void PartnerVoiceCombo_DropDownClosed(object? sender, EventArgs e)
        {
            Speak(PartnerVoiceCombo.Text);
        }

        private void RightVoiceCombo_DropDownClosed(object? sender, EventArgs e)
        {
            Speak(RightVoiceCombo.Text);
        }

        #endregion

        #region "Private variables"
        private SpeechSynthesizer _voiceSynthesizer = null;
        private int _voiceCount = 0;
        #endregion
    }
}
