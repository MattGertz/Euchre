namespace MAUIEuchre
{
    public interface INativeTts
    {
        Task InitializeAsync();
        List<string> GetVoiceNames();
        void SetVoice(string name);
        void SetPitch(float pitch);
        void SetRate(float rate);
        void Speak(string text);
        void Stop();
        void Shutdown();
    }
}
