using System.Text.Json;

namespace MAUIEuchre
{
    public class VoiceVariation
    {
        public string DisplayName { get; set; } = "";
        public string RealVoiceName { get; set; } = "";
        public float Pitch { get; set; } = 1.0f;
        public float Rate { get; set; } = 1.0f;

        private const string VariationSuffix = " (variation)";
        private const string SettingsKey = "VoiceVariations";

        private const float MinPitch = 0.75f;
        private const float MaxPitch = 1.25f;
        private const float MinRate = 0.85f;
        private const float MaxRate = 1.15f;

        public float ClampedPitch => Math.Clamp(Pitch, MinPitch, MaxPitch);
        public float ClampedRate => Math.Clamp(Rate, MinRate, MaxRate);

        private static readonly Random _rng = new();

        private static float RandomVariation(int minPercent, int maxPercent)
        {
            int pct = _rng.Next(minPercent, maxPercent + 1);
            return _rng.Next(2) == 0 ? 1.0f - pct / 100f : 1.0f + pct / 100f;
        }

        public static List<VoiceVariation> EnsureEnoughVoices(List<string> realVoiceNames, int minimumCount = 3)
        {
            var saved = LoadSaved();

            // Remove saved variations whose base voice no longer exists
            saved.RemoveAll(v => !realVoiceNames.Contains(v.RealVoiceName));

            int totalAvailable = realVoiceNames.Count + saved.Count;
            if (totalAvailable >= minimumCount)
            {
                Save(saved);
                return saved;
            }

            // Need to create more variations
            int needed = minimumCount - totalAvailable;

            foreach (var realName in realVoiceNames)
            {
                if (needed <= 0) break;

                while (needed > 0)
                {
                    string displayName = realName + VariationSuffix;
                    int suffix = 2;
                    while (saved.Any(v => v.DisplayName == displayName))
                    {
                        displayName = $"{realName} (variation {suffix})";
                        suffix++;
                    }

                    saved.Add(new VoiceVariation
                    {
                        DisplayName = displayName,
                        RealVoiceName = realName,
                        Pitch = RandomVariation(15, 25),
                        Rate = RandomVariation(10, 15),
                    });
                    needed--;
                }
            }

            Save(saved);
            return saved;
        }

#if DEBUG
        public static List<VoiceVariation> EnsureDebugVariation(List<string> realVoiceNames, List<VoiceVariation> variations)
        {
            if (realVoiceNames.Count == 0) return variations;

            if (variations.Any())
                return variations;

            string realName = realVoiceNames[0];
            string displayName = realName + VariationSuffix;
            variations.Add(new VoiceVariation
            {
                DisplayName = displayName,
                RealVoiceName = realName,
                Pitch = RandomVariation(15, 25),
                Rate = RandomVariation(10, 15),
            });
            Save(variations);
            return variations;
        }
#endif

        public static VoiceVariation? Find(List<VoiceVariation> variations, string displayName)
        {
            return variations.FirstOrDefault(v => v.DisplayName == displayName);
        }

        private static List<VoiceVariation> LoadSaved()
        {
            string json = Preferences.Default.Get(SettingsKey, "");
            if (string.IsNullOrEmpty(json)) return new List<VoiceVariation>();
            try
            {
                return JsonSerializer.Deserialize<List<VoiceVariation>>(json) ?? new List<VoiceVariation>();
            }
            catch
            {
                return new List<VoiceVariation>();
            }
        }

        private static void Save(List<VoiceVariation> variations)
        {
            string json = JsonSerializer.Serialize(variations);
            Preferences.Default.Set(SettingsKey, json);
        }
    }
}
