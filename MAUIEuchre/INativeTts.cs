namespace MAUIEuchre
{
    public interface INativeTts
    {
        Task InitializeAsync();
        List<string> GetVoiceNames();
        void SetVoice(string name);
        void Speak(string text);
        void Stop();
        void Shutdown();
    }
}
