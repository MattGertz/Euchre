using Windows.Media.SpeechSynthesis;

namespace MAUIEuchre
{
    public class NativeTts : INativeTts
    {
        private SpeechSynthesizer? _synth;
        private readonly List<VoiceInformation> _voices = new();

        public Task InitializeAsync()
        {
            _synth = new SpeechSynthesizer();
            _voices.Clear();
            foreach (var v in SpeechSynthesizer.AllVoices)
            {
                if (v.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    _voices.Add(v);
            }
            _voices.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        }

        public List<string> GetVoiceNames()
        {
            return _voices.Select(v => v.DisplayName).ToList();
        }

        public void SetVoice(string name)
        {
            if (_synth == null) return;
            var voice = _voices.FirstOrDefault(v => v.DisplayName == name);
            if (voice != null)
                _synth.Voice = voice;
        }

        public void Speak(string text)
        {
            if (_synth == null || string.IsNullOrEmpty(text)) return;
            _ = SpeakAsync(text);
        }

        private async Task SpeakAsync(string text)
        {
            try
            {
                var stream = await _synth!.SynthesizeTextToStreamAsync(text);
                var player = new Windows.Media.Playback.MediaPlayer();
                player.Source = Windows.Media.Core.MediaSource.CreateFromStream(stream, stream.ContentType);
                player.MediaEnded += (s, e) => player.Dispose();
                player.Play();
            }
            catch
            {
                // If speech fails, silently continue
            }
        }

        public void Stop()
        {
            // WinRT SpeechSynthesizer doesn't have a stop mid-utterance;
            // the MediaPlayer disposes on end.
        }

        public void Shutdown()
        {
            _synth?.Dispose();
            _synth = null;
        }
    }
}
