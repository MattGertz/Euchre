namespace MAUIEuchre
{
    public static class GameSettings
    {
        public static string PlayerName
        {
            get => Preferences.Default.Get("PlayerName", "Player");
            set => Preferences.Default.Set("PlayerName", value);
        }

        public static string PartnerName
        {
            get => Preferences.Default.Get("PartnerName", "Partner");
            set => Preferences.Default.Set("PartnerName", value);
        }

        public static string LeftOpponentName
        {
            get => Preferences.Default.Get("LeftOpponentName", "Left Opponent");
            set => Preferences.Default.Set("LeftOpponentName", value);
        }

        public static string RightOpponentName
        {
            get => Preferences.Default.Get("RightOpponentName", "Right Opponent");
            set => Preferences.Default.Set("RightOpponentName", value);
        }

        public static bool StickTheDealer
        {
            get => Preferences.Default.Get("StickTheDealer", false);
            set => Preferences.Default.Set("StickTheDealer", value);
        }

        public static bool NineOfHearts
        {
            get => Preferences.Default.Get("NineOfHearts", false);
            set => Preferences.Default.Set("NineOfHearts", value);
        }

        public static bool SuperEuchre
        {
            get => Preferences.Default.Get("SuperEuchre", false);
            set => Preferences.Default.Set("SuperEuchre", value);
        }

        public static bool QuietDealer
        {
            get => Preferences.Default.Get("QuietDealer", false);
            set => Preferences.Default.Set("QuietDealer", value);
        }

        public static bool PeekAtOtherCards
        {
            get => Preferences.Default.Get("PeekAtOtherCards", false);
            set => Preferences.Default.Set("PeekAtOtherCards", value);
        }

        public static bool SoundOn
        {
            get => Preferences.Default.Get("SoundOn", false);
            set => Preferences.Default.Set("SoundOn", value);
        }

        public static bool SpeechOn
        {
            get => Preferences.Default.Get("SpeechOn", false);
            set => Preferences.Default.Set("SpeechOn", value);
        }

        public static bool ShowAnimations
        {
            get => Preferences.Default.Get("ShowAnimations", false);
            set => Preferences.Default.Set("ShowAnimations", value);
        }

        public static int LeftOpponentPlay
        {
            get => Preferences.Default.Get("LeftOpponentPlay", 2);
            set => Preferences.Default.Set("LeftOpponentPlay", value);
        }

        public static int PartnerPlay
        {
            get => Preferences.Default.Get("PartnerPlay", 2);
            set => Preferences.Default.Set("PartnerPlay", value);
        }

        public static int RightOpponentPlay
        {
            get => Preferences.Default.Get("RightOpponentPlay", 2);
            set => Preferences.Default.Set("RightOpponentPlay", value);
        }

        public static string LeftOpponentVoice
        {
            get => Preferences.Default.Get("LeftOpponentVoice", "");
            set => Preferences.Default.Set("LeftOpponentVoice", value);
        }

        public static string PartnerVoice
        {
            get => Preferences.Default.Get("PartnerVoice", "");
            set => Preferences.Default.Set("PartnerVoice", value);
        }

        public static string RightOpponentVoice
        {
            get => Preferences.Default.Get("RightOpponentVoice", "");
            set => Preferences.Default.Set("RightOpponentVoice", value);
        }

        public static void Reset()
        {
            Preferences.Default.Clear();
        }
    }
}
