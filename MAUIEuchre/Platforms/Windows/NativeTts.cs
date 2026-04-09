using Windows.Media.SpeechSynthesis;

namespace MAUIEuchre
{
    public class NativeTts : INativeTts
    {
        private SpeechSynthesizer? _synth;
        private readonly List<VoiceInformation> _voices = new();
        private float _pitch = 1.0f;
        private float _rate = 1.0f;
        private readonly SemaphoreSlim _speakLock = new(1, 1);

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

        public void SetPitch(float pitch)
        {
            _pitch = pitch;
            if (_synth != null)
                _synth.Options.AudioPitch = pitch;
        }

        public void SetRate(float rate)
        {
            _rate = rate;
            if (_synth != null)
                _synth.Options.SpeakingRate = rate;
        }

        public void Speak(string text)
        {
            if (_synth == null || string.IsNullOrEmpty(text)) return;
            _ = SpeakAsync(text);
        }

        private async Task SpeakAsync(string text)
        {
            await _speakLock.WaitAsync();
            try
            {
                var stream = await _synth!.SynthesizeTextToStreamAsync(text);
                var tcs = new TaskCompletionSource();
                var player = new Windows.Media.Playback.MediaPlayer();
                player.Source = Windows.Media.Core.MediaSource.CreateFromStream(stream, stream.ContentType);
                player.MediaEnded += (s, e) => { player.Dispose(); tcs.TrySetResult(); };
                player.MediaFailed += (s, e) => { player.Dispose(); tcs.TrySetResult(); };
                player.Play();
                await tcs.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS ERROR: {ex.Message}");
            }
            finally
            {
                _speakLock.Release();
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
