namespace MAUIEuchre
{
    public partial class EuchreOptions : ContentPage
    {
        #region "Public properties"
        public bool LocalDialogResult { get; set; }
        private List<string> _voiceNames = new();
        private List<VoiceVariation> _variations = new();
        private INativeTts? _previewTts;
        private bool _voicesInitializing = true;
        #endregion

        #region "Public methods"
        public EuchreOptions()
        {
            InitializeComponent();
            SpeechOn.CheckedChanged += SpeechOn_CheckedChanged;
            LeftOpponentVoice.SelectedIndexChanged += VoicePicker_SelectedIndexChanged;
            PartnerVoice.SelectedIndexChanged += VoicePicker_SelectedIndexChanged;
            RightOpponentVoice.SelectedIndexChanged += VoicePicker_SelectedIndexChanged;
            _ = InitializeVoicesAsync();
            UpdateSettings();
            ApplyTooltips();
        }

        private void ApplyTooltips()
        {
            SetTip(PlayerName, "OPTION_Name");
            SetTip(LeftOpponentName, "OPTION_LeftOpponent");
            SetTip(PartnerName, "OPTION_YourPartner");
            SetTip(RightOpponentName, "OPTION_RightOpponent");
            SetTip(LeftOpponentVoice, "OPTION_LeftOpponentVoice");
            SetTip(PartnerVoice, "OPTION_YourPartnerVoice");
            SetTip(RightOpponentVoice, "OPTION_RightOpponentVoice");
            SetTip(StickTheDealer, "OPTION_StickTheDealer");
            SetTip(NineOfHearts, "OPTION_NineOfHearts");
            SetTip(SuperEuchre, "OPTION_SuperEuchre");
            SetTip(QuietDealer, "OPTION_QuietDealer");
            SetTip(PeekAtOtherCards, "OPTION_PeekAtOtherCards");
            SetTip(SoundOn, "OPTION_SoundEffects");
            SetTip(SpeechOn, "OPTION_Speech");
            SetTip(ShowAnimations, "OPTION_ShowAnimations");
            SetTip(LeftOpponentCrazy, "OPTION_Crazy");
            SetTip(LeftOpponentNormal, "OPTION_Normal");
            SetTip(LeftOpponentConservative, "OPTION_Conservative");
            SetTip(PartnerCrazy, "OPTION_Crazy");
            SetTip(PartnerNormal, "OPTION_Normal");
            SetTip(PartnerConservative, "OPTION_Conservative");
            SetTip(RightOpponentCrazy, "OPTION_Crazy");
            SetTip(RightOpponentNormal, "OPTION_Normal");
            SetTip(RightOpponentConservative, "OPTION_Conservative");
        }

        private static void SetTip(View control, string resourceKey)
        {
            string text = AppResources.GetString(resourceKey);
            if (!string.IsNullOrEmpty(text))
            {
                ToolTipProperties.SetText(control, text);
                SemanticProperties.SetDescription(control, text);
            }
        }

        private async Task InitializeVoicesAsync()
        {
            try
            {
                _previewTts = new NativeTts();
                await _previewTts.InitializeAsync();
                _voiceNames = _previewTts.GetVoiceNames();
                _variations = VoiceVariation.EnsureEnoughVoices(_voiceNames);
#if DEBUG
                _variations = VoiceVariation.EnsureDebugVariation(_voiceNames, _variations);
                foreach (var v in _variations)
                    System.Diagnostics.Debug.WriteLine($"VARIATION: '{v.DisplayName}' -> real='{v.RealVoiceName}' pitch={v.Pitch} rate={v.Rate}");
#endif

                var allNames = new List<string>(_voiceNames);
                foreach (var v in _variations)
                    allNames.Add(v.DisplayName);

                foreach (var name in allNames)
                {
                    LeftOpponentVoice.Items.Add(name);
                    PartnerVoice.Items.Add(name);
                    RightOpponentVoice.Items.Add(name);
                }

                SelectVoice(LeftOpponentVoice, GameSettings.LeftOpponentVoice);
                SelectVoice(PartnerVoice, GameSettings.PartnerVoice);
                SelectVoice(RightOpponentVoice, GameSettings.RightOpponentVoice);
                _voicesInitializing = false;
            }
            catch
            {
                // If TTS init fails, voice pickers stay empty
            }
        }

        private void SelectVoice(Picker picker, string savedName)
        {
            if (string.IsNullOrEmpty(savedName) || picker.Items.Count == 0)
            {
                picker.SelectedIndex = picker.Items.Count > 0 ? 0 : -1;
                return;
            }
            int idx = picker.Items.IndexOf(savedName);
            picker.SelectedIndex = idx >= 0 ? idx : 0;
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
            SpeechOn.IsChecked = GameSettings.SpeechOn;
            ShowAnimations.IsChecked = GameSettings.ShowAnimations;

            VoiceRow.IsVisible = GameSettings.SpeechOn;

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
        private void SpeechOn_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            VoiceRow.IsVisible = e.Value;
        }

        private void VoicePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_voicesInitializing || _previewTts == null || sender is not Picker picker) return;
            if (picker.SelectedIndex < 0 || picker.SelectedIndex >= picker.Items.Count) return;
            string voiceName = picker.Items[picker.SelectedIndex];
            var variation = VoiceVariation.Find(_variations, voiceName);
            if (variation != null)
            {
                System.Diagnostics.Debug.WriteLine($"PREVIEW VARIATION: '{variation.DisplayName}' pitch={variation.ClampedPitch} rate={variation.ClampedRate}");
                _previewTts.SetVoice(variation.RealVoiceName);
                _previewTts.SetPitch(variation.ClampedPitch);
                _previewTts.SetRate(variation.ClampedRate);
            }
            else
            {
                _previewTts.SetVoice(voiceName);
                _previewTts.SetPitch(1.0f);
                _previewTts.SetRate(1.0f);
            }
            _previewTts.Speak(AppResources.GetString("SAY_LetsPlayEuchre"));
        }

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
            GameSettings.SpeechOn = SpeechOn.IsChecked;
            GameSettings.ShowAnimations = ShowAnimations.IsChecked;

            if (LeftOpponentVoice.SelectedIndex >= 0)
                GameSettings.LeftOpponentVoice = LeftOpponentVoice.Items[LeftOpponentVoice.SelectedIndex];
            if (PartnerVoice.SelectedIndex >= 0)
                GameSettings.PartnerVoice = PartnerVoice.Items[PartnerVoice.SelectedIndex];
            if (RightOpponentVoice.SelectedIndex >= 0)
                GameSettings.RightOpponentVoice = RightOpponentVoice.Items[RightOpponentVoice.SelectedIndex];

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
            _previewTts?.Shutdown();
            _previewTts = null;
            await Navigation.PopModalAsync();
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            _previewTts?.Shutdown();
            _previewTts = null;
            GameSettings.Reset();
            UpdateSettings();
        }
        #endregion
    }
}
