using Android.Speech.Tts;
using Android.Runtime;
using AndroidTts = Android.Speech.Tts.TextToSpeech;

namespace MAUIEuchre
{
    public class NativeTts : Java.Lang.Object, AndroidTts.IOnInitListener, INativeTts
    {
        private AndroidTts? _tts;
        private TaskCompletionSource<bool>? _initTcs;
        private readonly List<Voice> _voices = new();

        public async Task InitializeAsync()
        {
            _initTcs = new TaskCompletionSource<bool>();
            _tts = new AndroidTts(Android.App.Application.Context, this);
            await _initTcs.Task;
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Success && _tts != null)
            {
                var allVoices = _tts.Voices;
                if (allVoices != null)
                {
                    _voices.Clear();
                    foreach (var v in allVoices)
                    {
                        if (v.Locale?.Language == "en")
                            _voices.Add(v);
                    }
                    _voices.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                }
            }
            _initTcs?.TrySetResult(status == OperationResult.Success);
        }

        public List<string> GetVoiceNames()
        {
            return _voices.Select(v => v.Name ?? "").ToList();
        }

        public void SetVoice(string name)
        {
            if (_tts == null) return;
            var voice = _voices.FirstOrDefault(v => v.Name == name);
            if (voice != null)
                _tts.SetVoice(voice);
        }

        public void SetPitch(float pitch)
        {
            _tts?.SetPitch(pitch);
        }

        public void SetRate(float rate)
        {
            _tts?.SetSpeechRate(rate);
        }

        public void Speak(string text)
        {
            _tts?.Speak(text, QueueMode.Add, null, null);
        }

        public void Stop()
        {
            _tts?.Stop();
        }

        public void Shutdown()
        {
            _tts?.Stop();
            _tts?.Shutdown();
            _tts = null;
        }
    }
}
