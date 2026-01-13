using System;
using System.Speech.Synthesis;
namespace CSEuchre4
{
    public class EuchreSpeech
    {
        private SpeechSynthesizer VoiceSynthesizer = new SpeechSynthesizer();
        /// <summary>
        /// Sets up the object with a the desired voice
        /// </summary>
        /// <remarks></remarks>
        public void SetVoice(string Name)
        {
            if (VoiceSynthesizer != null)
            {
                try
                {
                    VoiceSynthesizer.SelectVoice(Name);
                }
                catch
                {

                }
            }
        }

        public void DisposeVoice()
        {
            if (VoiceSynthesizer != null)
            {
                VoiceSynthesizer.Dispose();
                VoiceSynthesizer = null!;
            }
        }

        /// <summary>
        /// Speaks one string if the voice is initialized
        /// </summary>
        /// <param name="s"></param>
        /// <remarks></remarks>
        private void Say(string s)
        {
            if (VoiceSynthesizer != null)
            {
                try
                {
                    VoiceSynthesizer.Speak(s);
                }
                catch
                {
                    // If speech fails, dispose and recreate the synthesizer to recover
                    try
                    {
                        VoiceSynthesizer.Dispose();
                        VoiceSynthesizer = new SpeechSynthesizer();
                    }
                    catch
                    {
                        // If we can't recover, set to null to prevent further attempts
                        VoiceSynthesizer = null!;
                    }
                }
            }
        }


        /// <summary>
        /// Says "I'll pick it up."
        /// </summary>
        /// <remarks></remarks>
        public void SayIPickItUp()
        {
            Say(Properties.Resources.SAY_IPickItUp);
        }
        /// <summary>
        /// Says "Pick it up."
        /// </summary>
        /// <remarks></remarks>
        public void SayPickItUp()
        {
            Say(Properties.Resources.SAY_PickItUp);
        }
        /// <summary>
        /// Says "Pass."
        /// </summary>
        /// <remarks></remarks>
        public void SayPass()
        {
            Say(Properties.Resources.SAY_Pass);
        }
        /// <summary>
        /// Says "Trump is Hearts."
        /// </summary>
        /// <remarks></remarks>
        public void SayHearts()
        {
            Say(Properties.Resources.SAY_Hearts);
        }
        /// <summary>
        /// Says "Trump is Diamonds."
        /// </summary>
        /// <remarks></remarks>
        public void SayDiamonds()
        {
            Say(Properties.Resources.SAY_Diamonds);
        }
        /// <summary>
        /// Says "Trump is Clubs."
        /// </summary>
        /// <remarks></remarks>
        public void SayClubs()
        {
            Say(Properties.Resources.SAY_Clubs);
        }
        /// <summary>
        /// Says "Trump is Spades."
        /// </summary>
        /// <remarks></remarks>
        public void SaySpades()
        {
            Say(Properties.Resources.SAY_Spades);
        }
        /// <summary>
        /// Says "And I'm going alone."
        /// </summary>
        /// <remarks></remarks>
        public void SayAlone()
        {
            Say(Properties.Resources.SAY_Alone);
        }

        /// <summary>
        /// Apologize for getting euchred after calling
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotEuchredMyFault(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotEuchredMyFault, s));
        }

        /// <summary>
        /// Apologize for not having a supporting hand
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotEuchredOurFault(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotEuchredOurFault, s));
        }

        /// <summary>
        /// Apologize for not having a supporting hand
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotEuchredYourFault(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotEuchredYourFault, s));
        }

        /// <summary>
        /// 1 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotOne(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotOne, s));
        }

        /// <summary>
        /// 2 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotTwo(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotTwo, s));
        }

        /// <summary>
        /// 4 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayWeGotFour(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeGotFour, s));
        }
        /// <summary>
        /// 1 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayMeGotOne(string s)
        {
            Say(String.Format(Properties.Resources.SAY_MeGotOne, s));
        }

        /// <summary>
        /// 2 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayMeGotTwo(string s)
        {
            Say(String.Format(Properties.Resources.SAY_MeGotTwo, s));
        }

        /// <summary>
        /// 4 point win
        /// </summary>
        /// <remarks></remarks>
        public void SayMeGotFour(string s)
        {
            Say(String.Format(Properties.Resources.SAY_MeGotFour, s));
        }

        /// <summary>
        /// Congratulations
        /// </summary>
        /// <remarks></remarks>
        public void SayWeWon(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeWon, s));
        }

        /// <summary>
        /// Commiserations
        /// </summary>
        /// <remarks></remarks>
        public void SayTheyWon(string s)
        {
            Say(String.Format(Properties.Resources.SAY_TheyWon, s));
        }

        /// <summary>
        /// We Euchred Them
        /// </summary>
        /// <remarks></remarks>
        public void SayWeEuchredThem(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeEuchredThem, s));
        }

        /// <summary>
        /// We SuperEuchred Them
        /// </summary>
        /// <remarks></remarks>
        public void SayWeSuperEuchredThem(string s)
        {
            Say(String.Format(Properties.Resources.SAY_WeSuperEuchredThem, s));
        }

        /// <summary>
        /// We SuperEuchred Them
        /// </summary>
        /// <remarks></remarks>
        public void SayTheyGotOne()
        {
            Say(Properties.Resources.SAY_TheyGotOne);
        }

        /// <summary>
        /// We SuperEuchred Them
        /// </summary>
        /// <remarks></remarks>
        public void SayTheyGotTwo()
        {
            Say(Properties.Resources.SAY_TheyGotTwo);
        }

        /// <summary>
        /// We SuperEuchred Them
        /// </summary>
        /// <remarks></remarks>
        public void SayTheyGotFour()
        {
            Say(Properties.Resources.SAY_TheyGotFour);
        }
    }
}
