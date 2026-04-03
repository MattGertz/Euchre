using System;
using System.Threading;

namespace MAUIEuchre
{
    public class EuchreSpeech
    {
        private CancellationTokenSource? _cts;
        private bool _disposed;

        public void SetVoice(string Name)
        {
            // MAUI TextToSpeech on Android uses the system default voice.
            // Per-voice selection is not supported the same way as System.Speech.
        }

        public void DisposeVoice()
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async void Say(string s)
        {
            if (_disposed || string.IsNullOrEmpty(s)) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                await TextToSpeech.Default.SpeakAsync(s, cancelToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when new speech cancels previous speech
            }
            catch
            {
                // If speech fails, silently continue
            }
        }

        public void SayIPickItUp()
        {
            Say(AppResources.GetString("SAY_IPickItUp"));
        }

        public void SayPickItUp()
        {
            Say(AppResources.GetString("SAY_PickItUp"));
        }

        public void SayPass()
        {
            Say(AppResources.GetString("SAY_Pass"));
        }

        public void SayHearts()
        {
            Say(AppResources.GetString("SAY_Hearts"));
        }

        public void SayDiamonds()
        {
            Say(AppResources.GetString("SAY_Diamonds"));
        }

        public void SayClubs()
        {
            Say(AppResources.GetString("SAY_Clubs"));
        }

        public void SaySpades()
        {
            Say(AppResources.GetString("SAY_Spades"));
        }

        public void SayAlone()
        {
            Say(AppResources.GetString("SAY_Alone"));
        }

        public void SayWeGotEuchredMyFault(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotEuchredMyFault"), s));
        }

        public void SayWeGotEuchredOurFault(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotEuchredOurFault"), s));
        }

        public void SayWeGotEuchredYourFault(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotEuchredYourFault"), s));
        }

        public void SayWeGotOne(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotOne"), s));
        }

        public void SayWeGotTwo(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotTwo"), s));
        }

        public void SayWeGotFour(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeGotFour"), s));
        }

        public void SayMeGotOne(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_MeGotOne"), s));
        }

        public void SayMeGotTwo(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_MeGotTwo"), s));
        }

        public void SayMeGotFour(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_MeGotFour"), s));
        }

        public void SayWeWon(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeWon"), s));
        }

        public void SayTheyWon(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_TheyWon"), s));
        }

        public void SayWeEuchredThem(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeEuchredThem"), s));
        }

        public void SayWeSuperEuchredThem(string s)
        {
            Say(String.Format(AppResources.GetString("SAY_WeSuperEuchredThem"), s));
        }

        public void SayTheyGotOne()
        {
            Say(AppResources.GetString("SAY_TheyGotOne"));
        }

        public void SayTheyGotTwo()
        {
            Say(AppResources.GetString("SAY_TheyGotTwo"));
        }

        public void SayTheyGotFour()
        {
            Say(AppResources.GetString("SAY_TheyGotFour"));
        }
    }
}
