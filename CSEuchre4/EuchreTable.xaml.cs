using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CSEuchre4
{
    /// <summary>
    /// Interaction logic for EuchreTable.xaml
    /// </summary>
    public partial class EuchreTable : Window
    {


        #region "Static methods"

        static public int GenRandomNumber(int NumSides)
        {
            byte[] randomNumber = new byte[1];
            System.Security.Cryptography.RNGCryptoServiceProvider Gen = new System.Security.Cryptography.RNGCryptoServiceProvider();
            Gen.GetBytes(randomNumber);
            int rand = Convert.ToInt32(randomNumber[0]);
            return rand % NumSides;
        }

        static private string GetScoreResourceName(ScorePrefix prefix, int value)
        {
            StringBuilder Score = new StringBuilder();
            switch (prefix)
            {
            case ScorePrefix.ScoreThem:
                Score.Append("SCOREThem");
                break;
            case ScorePrefix.ScoreUs:
                Score.Append("SCOREUs");
                break;
            }

            Score.Append(value.ToString());
            return Score.ToString();
        }

        static public void SetImage(System.Windows.Controls.Image Img, System.Drawing.Image res)
        {
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            MemoryStream memStream = new MemoryStream();
            res.Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memStream.Seek(0, SeekOrigin.Begin);
            bmpImage.StreamSource = memStream;
            bmpImage.EndInit();
            Img.Source = bmpImage;
        }

        static public void SetIcon(System.Windows.Window win, System.Drawing.Icon res)
        {
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            MemoryStream memStream = new MemoryStream();
            res.Save(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            bmpImage.StreamSource = memStream;
            bmpImage.EndInit();
            win.Icon = bmpImage;
        }
        #endregion

        #region "Public Methods"
        public EuchreTable()
        {
            InitializeComponent();
            InitializeLabelArray();

            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                gamePlayers[(int)i] = new EuchrePlayer(i, this);
            }

            BidControl.gameTable = this;
            BidControl2.gameTable = this;

            this.Closing += EuchreTable_Closing;
            this.KeyUp += EuchreTable_KeyUp;
            this.Loaded += EuchreTable_Loaded;
            this.ContinueButton.Click += ContinueButton_Click;
            this.PlayerCard1.MouseDown += PlayerCard_Click;
            this.PlayerCard2.MouseDown += PlayerCard_Click;
            this.PlayerCard3.MouseDown += PlayerCard_Click;
            this.PlayerCard4.MouseDown += PlayerCard_Click;
            this.PlayerCard5.MouseDown += PlayerCard_Click;
        }

        public void SpeakSuit(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                switch (handTrumpSuit)
                {
                case EuchreCard.Suits.Clubs:
                    gamePlayers[(int)seat].gameVoice.SayClubs();
                    break;
                case EuchreCard.Suits.Diamonds:
                    gamePlayers[(int)seat].gameVoice.SayDiamonds();
                    break;
                case EuchreCard.Suits.Hearts:
                    gamePlayers[(int)seat].gameVoice.SayHearts();
                    break;
                case EuchreCard.Suits.Spades:
                    gamePlayers[(int)seat].gameVoice.SaySpades();
                    break;
                }
            }
        }

        public void SpeakPass(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayPass();
            }
        }

        public void SpeakAlone(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayAlone();
            }
        }

        public void SpeakPickItUp(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayPickItUp();
            }
        }

        public void SpeakIPickItUp(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayIPickItUp();
            }
        }

        public void SetPlayerCursorToHand(bool SetHand)
        {
            for (int i = 0; i <= 4; i++)
            {
                gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Cursor = SetHand ? Cursors.Hand : _cursorCached;
            }
        }

        public void UpdateEuchreState(EuchreState state)
        {
            _stateLast = _stateCurrent;
            _stateCurrent = state;
            _stateDesiredStateAfterHumanClick = EuchreState.NoState;

            Dispatcher.BeginInvoke(new NextTableAction(MasterStateDirector));
        }

        public void RevertEuchreState()
        {
            UpdateEuchreState(_stateLast);
            _stateLast = EuchreState.NoState;
        }

        public delegate void NextTableAction();

        public void MasterStateDirector()
        {
            switch (_stateCurrent)
            {
            case EuchreState.NoState:
                // Do nothing
                NoOp();
                break;

            case EuchreState.StartNewGameRequested:
                if (_stateGameStarted)
                {
                    if (!RestartGame())
                    {
                        RevertEuchreState();
                        return;
                    }
                }

                if (NewGame())
                {
                    UpdateEuchreState(EuchreState.StartNewGameConfirmed);
                }
                else
                {
                    CleanUpGame();
                    UpdateEuchreState(EuchreState.NoState);
                }
                break;

            case EuchreState.StartNewGameConfirmed:
                PreDealerSelection();
                UpdateEuchreState(EuchreState.StillSelectingDealer);
                break;
            case EuchreState.StillSelectingDealer:
                TrySelectDealer();
                break;
            case EuchreState.DealerSelected:
                PostDealerSelection(EuchreState.DealerAcknowledged);
                break;
            case EuchreState.DealerAcknowledged:
                PostDealerCleanup();
                UpdateEuchreState(EuchreState.ClearHand);

                break;
            case EuchreState.ClearHand:
                SetForNewHand();
                UpdateEuchreState(EuchreState.StartNewHand);
                break;
            case EuchreState.StartNewHand:
                UpdateEuchreState(EuchreState.DealCards);
                break;
            case EuchreState.DealCards:
                DealCards();
                trickLeaderIndex = EuchrePlayer.NextPlayer(handDealer);
                UpdateEuchreState(EuchreState.Bid1Starts);
                break;

            case EuchreState.Bid1Starts:
                PreBid1();
                UpdateEuchreState(EuchreState.Bid1Player0);
                break;
            case EuchreState.Bid1Player0:
                Bid1(EuchreState.Bid1Player1);
                break;
            case EuchreState.Bid1Player1:
                Bid1(EuchreState.Bid1Player2);
                break;
            case EuchreState.Bid1Player2:
                Bid1(EuchreState.Bid1Player3);
                break;
            case EuchreState.Bid1Player3:
                Bid1(EuchreState.Bid2Starts); // We don't current show a continue dialog after the first round of failed bidding
                break;
            case EuchreState.Bid1PickUp:
                Bid1PickUp();
                break;
            case EuchreState.Bid1PickedUp:
                Bid1PickedUp();
                UpdateEuchreState(EuchreState.Bid1Succeeded);
                break;
            case EuchreState.Bid1Succeeded:
                SortAndSetHandImagesAndText();
                ShowAndEnableContinueButton(EuchreState.Bid1SucceededAcknowledged);
                break;
            case EuchreState.Bid1SucceededAcknowledged:
                UpdateEuchreState(EuchreState.Trick0Started);
                break;
            case EuchreState.Bid1Failed: // Currently unused state
                NoOp();
                break;
            case EuchreState.Bid1FailedAcknowledged: // Currently unused state
                NoOp();
                break;

            case EuchreState.Bid2Starts:
                PreBid2();
                UpdateEuchreState(EuchreState.Bid2Player0);
                break;
            case EuchreState.Bid2Player0:
                Bid2(EuchreState.Bid2Player1);
                break;
            case EuchreState.Bid2Player1:
                Bid2(EuchreState.Bid2Player2);
                break;
            case EuchreState.Bid2Player2:
                Bid2(EuchreState.Bid2Player3);
                break;
            case EuchreState.Bid2Player3:
                Bid2(EuchreState.Bid2Failed);
                break;
            case EuchreState.Bid2Succeeded:
                ShowAndEnableContinueButton(EuchreState.Bid2SucceededAcknowledged);
                break;

            case EuchreState.Bid2SucceededAcknowledged:
                SortAndSetHandImagesAndText();
                UpdateEuchreState(EuchreState.Trick0Started);
                break;

            case EuchreState.Bid2Failed:
                this.UpdateStatus(Properties.Resources.Notice_AllPassedTwice);
                ShowAndEnableContinueButton(EuchreState.Bid2FailedAcknowledged);
                break;

            case EuchreState.Bid2FailedAcknowledged:
                SetNextDealer(); // Nobody bid; move to the next dealer
                UpdateEuchreState(EuchreState.ClearHand);
                break;

            case EuchreState.Trick0Started:
                UpdateLayout();
                PrepTrick();
                UpdateEuchreState(EuchreState.Trick0_SelectCard0);
                break;
            case EuchreState.Trick0_SelectCard0:
                SelectCardForTrick(EuchreState.Trick0_PlayCard0);
                break;
            case EuchreState.Trick0_PlayCard0:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick0_SelectCard1);
                break;
            case EuchreState.Trick0_SelectCard1:
                SelectCardForTrick(EuchreState.Trick0_PlayCard1);
                break;
            case EuchreState.Trick0_PlayCard1:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick0_SelectCard2);
                break;
            case EuchreState.Trick0_SelectCard2:
                SelectCardForTrick(EuchreState.Trick0_PlayCard2);
                break;
            case EuchreState.Trick0_PlayCard2:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick0_SelectCard3);
                break;
            case EuchreState.Trick0_SelectCard3:
                SelectCardForTrick(EuchreState.Trick0_PlayCard3);
                break;
            case EuchreState.Trick0_PlayCard3:
                PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick0EndingAcknowledged);
                break;
            case EuchreState.Trick0Ended:
                NoOp(); // Currently unused state
                break;
            case EuchreState.Trick0EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick1Started);
                break;

            case EuchreState.Trick1Started:
                UpdateLayout();
                PrepTrick();
                UpdateEuchreState(EuchreState.Trick1_SelectCard0);
                break;
            case EuchreState.Trick1_SelectCard0:
                SelectCardForTrick(EuchreState.Trick1_PlayCard0);
                break;
            case EuchreState.Trick1_PlayCard0:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick1_SelectCard1);
                break;
            case EuchreState.Trick1_SelectCard1:
                SelectCardForTrick(EuchreState.Trick1_PlayCard1);
                break;
            case EuchreState.Trick1_PlayCard1:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick1_SelectCard2);
                break;
            case EuchreState.Trick1_SelectCard2:
                SelectCardForTrick(EuchreState.Trick1_PlayCard2);
                break;
            case EuchreState.Trick1_PlayCard2:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick1_SelectCard3);
                break;
            case EuchreState.Trick1_SelectCard3:
                SelectCardForTrick(EuchreState.Trick1_PlayCard3);
                break;
            case EuchreState.Trick1_PlayCard3:
                PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick1EndingAcknowledged);
                break;
            case EuchreState.Trick1Ended:
                NoOp(); // Currently unused state
                break;
            case EuchreState.Trick1EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick2Started);
                break;

            case EuchreState.Trick2Started:
                UpdateLayout();
                PrepTrick();
                UpdateEuchreState(EuchreState.Trick2_SelectCard0);
                break;
            case EuchreState.Trick2_SelectCard0:
                SelectCardForTrick(EuchreState.Trick2_PlayCard0);
                break;
            case EuchreState.Trick2_PlayCard0:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick2_SelectCard1);
                break;
            case EuchreState.Trick2_SelectCard1:
                SelectCardForTrick(EuchreState.Trick2_PlayCard1);
                break;
            case EuchreState.Trick2_PlayCard1:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick2_SelectCard2);
                break;
            case EuchreState.Trick2_SelectCard2:
                SelectCardForTrick(EuchreState.Trick2_PlayCard2);
                break;
            case EuchreState.Trick2_PlayCard2:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick2_SelectCard3);
                break;
            case EuchreState.Trick2_SelectCard3:
                SelectCardForTrick(EuchreState.Trick2_PlayCard3);
                break;
            case EuchreState.Trick2_PlayCard3:
                PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick2EndingAcknowledged);
                break;
            case EuchreState.Trick2Ended:
                NoOp(); // Currently unused state
                break;
            case EuchreState.Trick2EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick3Started);
                break;

            case EuchreState.Trick3Started:
                UpdateLayout();
                PrepTrick();
                UpdateEuchreState(EuchreState.Trick3_SelectCard0);
                break;
            case EuchreState.Trick3_SelectCard0:
                SelectCardForTrick(EuchreState.Trick3_PlayCard0);
                break;
            case EuchreState.Trick3_PlayCard0:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick3_SelectCard1);
                break;
            case EuchreState.Trick3_SelectCard1:
                SelectCardForTrick(EuchreState.Trick3_PlayCard1);
                break;
            case EuchreState.Trick3_PlayCard1:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick3_SelectCard2);
                break;
            case EuchreState.Trick3_SelectCard2:
                SelectCardForTrick(EuchreState.Trick3_PlayCard2);
                break;
            case EuchreState.Trick3_PlayCard2:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick3_SelectCard3);
                break;
            case EuchreState.Trick3_SelectCard3:
                SelectCardForTrick(EuchreState.Trick3_PlayCard3);
                break;
            case EuchreState.Trick3_PlayCard3:
                PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick3EndingAcknowledged);
                break;
            case EuchreState.Trick3Ended:
                NoOp(); // Currently unused state
                break;
            case EuchreState.Trick3EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick4Started);
                break;

            case EuchreState.Trick4Started:
                UpdateLayout();
                PrepTrick();
                UpdateEuchreState(EuchreState.Trick4_SelectCard0);
                break;
            case EuchreState.Trick4_SelectCard0:
                SelectCardForTrick(EuchreState.Trick4_PlayCard0);
                break;
            case EuchreState.Trick4_PlayCard0:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick4_SelectCard1);
                break;
            case EuchreState.Trick4_SelectCard1:
                SelectCardForTrick(EuchreState.Trick4_PlayCard1);
                break;
            case EuchreState.Trick4_PlayCard1:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick4_SelectCard2);
                break;
            case EuchreState.Trick4_SelectCard2:
                SelectCardForTrick(EuchreState.Trick4_PlayCard2);
                break;
            case EuchreState.Trick4_PlayCard2:
                PlayCardForTrick();
                UpdateEuchreState(EuchreState.Trick4_SelectCard3);
                break;
            case EuchreState.Trick4_SelectCard3:
                SelectCardForTrick(EuchreState.Trick4_PlayCard3);
                break;
            case EuchreState.Trick4_PlayCard3:
                PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick4EndingAcknowledged);
                break;
            case EuchreState.Trick4Ended:
                NoOp(); // Currently unused state
                break;
            case EuchreState.Trick4EndingAcknowledged:
                UpdateEuchreState(EuchreState.HandCompleted);
                break;

            case EuchreState.HandCompleted:
                UpdateAllScores(EuchreState.HandCompletedAcknowledged);
                break;

            case EuchreState.HandCompletedAcknowledged:
                CleanupAfterHand();
                if (_gameTheirScore >= 10 || _gameYourScore >= 10)
                {
                    UpdateEuchreState(EuchreState.GameOver);
                }
                else
                {
                    SetNextDealer();
                    UpdateEuchreState(EuchreState.ClearHand);
                }
                break;

            case EuchreState.GameOver:
                DetermineWinnerAndEndGame();
                UpdateEuchreState(EuchreState.NoState);
                break;
            }
        }

        public bool RestartGame()
        {
            return (MessageBox.Show(Properties.Resources.Command_New, Properties.Resources.Command_NewTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK);
        }

        public void UpdateStatus(string s, int WhiteSpace = 1)
        {
            StatusArea.AppendText(s);
            if (WhiteSpace > 0)
            {
                for (int i = 1; i <= WhiteSpace; i++)
                {
                    StatusArea.AppendText("\r\n");
                    StatusArea.ScrollToEnd();
                }
            }
        }

        public void EnableCards(EuchrePlayer.Seats player, bool EnableIt)
        {
            for (int i = 0; i <= 4; i++)
            {
                gameTableTopCards[(int)player, i].IsEnabled = EnableIt;
                gameTableTopCards[(int)player, i].Opacity = EnableIt ? 1.0 : 0.25;
            }
        }

        public void MarkCardAsPlayed(EuchreCard card)
        {
            handCardsPlayed[_trickPlayedCardIndex] = card;
            _trickPlayedCardIndex = _trickPlayedCardIndex + 1;
        }

        public void ResetPlayedCards()
        {
            for (int i = 0; i <= 23; i++)
            {
                handCardsPlayed[i] = null;
            }
            _trickPlayedCardIndex = 0;
        }

        public void PostHumanBidFirstRound()
        {
            bool rv = (bool)BidControl.PickItUp.IsChecked;
            _handCurrentBidder.ProcessBidFirstRound((bool)BidControl.GoingAlone.IsChecked, rv);
            if (rv)
                UpdateEuchreState(EuchreState.Bid1PickUp);
            else
                UpdateEuchreState(_stateDesiredBidPass);

            UpdateLayout();
        }

        public void PostHumanBidSecondRound()
        {
            bool calledIt = !(bool)BidControl2.Pass.IsChecked;
            if (calledIt)
            {
                if ((bool)BidControl2.Hearts.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Hearts;
                else if ((bool)BidControl2.Diamonds.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Diamonds;
                else if ((bool)BidControl2.Clubs.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Clubs;
                else if ((bool)BidControl2.Spades.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Spades;
            }

            _handCurrentBidder.ProcessBidSecondRound((bool)BidControl2.GoingAlone.IsChecked, calledIt);
            if (calledIt)
                UpdateEuchreState(EuchreState.Bid2Succeeded);
            else
                UpdateEuchreState(_stateDesiredBidPass);

            UpdateLayout();
        }

        #endregion

        #region "Private methods"

        private void InitializeLabelArray()
        {
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 0] = LeftOpponentCard1;
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 1] = LeftOpponentCard2;
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 2] = LeftOpponentCard3;
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 3] = LeftOpponentCard4;
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 4] = LeftOpponentCard5;
            gameTableTopCards[(int)EuchrePlayer.Seats.LeftOpponent, 5] = LeftOpponentCard;

            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 0] = RightOpponentCard1;
            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 1] = RightOpponentCard2;
            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 2] = RightOpponentCard3;
            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 3] = RightOpponentCard4;
            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 4] = RightOpponentCard5;
            gameTableTopCards[(int)EuchrePlayer.Seats.RightOpponent, 5] = RightOpponentCard;

            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 0] = PlayerCard1;
            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 1] = PlayerCard2;
            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 2] = PlayerCard3;
            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 3] = PlayerCard4;
            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 4] = PlayerCard5;
            gameTableTopCards[(int)EuchrePlayer.Seats.Player, 5] = PlayerCard;

            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 0] = PartnerCard1;
            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 1] = PartnerCard2;
            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 2] = PartnerCard3;
            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 3] = PartnerCard4;
            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 4] = PartnerCard5;
            gameTableTopCards[(int)EuchrePlayer.Seats.Partner, 5] = PartnerCard;

            _gameDealerBox[(int)EuchrePlayer.Seats.LeftOpponent] = DealerLeftOpponent;
            _gameDealerBox[(int)EuchrePlayer.Seats.RightOpponent] = DealerRightOpponent;
            _gameDealerBox[(int)EuchrePlayer.Seats.Partner] = DealerPartner;
            _gameDealerBox[(int)EuchrePlayer.Seats.Player] = DealerPlayer;
        }

        private void AnimateACard(System.Drawing.Image imageToAnimate, UIElement startingControl, UIElement endingControl, EuchrePlayer.Seats perspective)
        {
            // Cards always animate from a position to a position, where each position is marked by another control.
            // The calling method is responsible for hiding the original card image at the original position (if such
            // exists -- it won't when dealing cards, for example) and showing the image at the new position.  This
            // method simply gets a card image, shows it at the original site, animates motion to the final site, and
            // disposes of the image.

            /*
             * This works:
            foo.Visibility = Visibility.Visible;
            TranslateTransform moveTransform = new TranslateTransform();
            this.foo.RenderTransform = moveTransform;

            DoubleAnimation anX = new DoubleAnimation(500, 100, TimeSpan.FromMilliseconds(1000));
            DoubleAnimation anY = new DoubleAnimation(500, 100, TimeSpan.FromMilliseconds(1000));

            anX.Completed += (sd, t) =>
            {
                foo.Visibility = Visibility.Hidden;
            };

            moveTransform.BeginAnimation(TranslateTransform.XProperty, anX);
            moveTransform.BeginAnimation(TranslateTransform.YProperty, anY);
             */

            // Which one to animate -- horizontal or vertical?
            System.Windows.Controls.Image animatedCard = (perspective == EuchrePlayer.Seats.LeftOpponent || perspective == EuchrePlayer.Seats.RightOpponent) ? AnimatedCardHorizontal : AnimatedCardVertical;

            FrameworkElement animatedCardFrame = (FrameworkElement)animatedCard;
            FrameworkElement startingControlFrame = (FrameworkElement)startingControl;
            FrameworkElement endingControlFrame = (FrameworkElement)endingControl;

            // First, invisibly move the animated card to the starting control's margins
            animatedCardFrame.Margin = new Thickness(startingControlFrame.Margin.Left, startingControlFrame.Margin.Top, startingControlFrame.Margin.Right, startingControlFrame.Margin.Bottom);

            // Second, set the image of the animated card
            SetImage(animatedCard, imageToAnimate);
            animatedCard.Visibility = Visibility.Visible;
            UpdateLayout();

            // Third, visibly move the animated card to the ending control's margins
            TranslateTransform moveTransform = new TranslateTransform();
            moveTransform.X = endingControlFrame.Margin.Left - startingControlFrame.Margin.Left;
            moveTransform.Y = endingControlFrame.Margin.Top - startingControlFrame.Margin.Top;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(500));
            DoubleAnimation animationX = new DoubleAnimation();
            DoubleAnimation animationY = new DoubleAnimation();
            animationX.Duration = duration;
            animationY.Duration = duration;

            Storyboard storyboard = new Storyboard();
            storyboard.Duration = duration;
            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);
            Storyboard.SetTarget(animationX, moveTransform);
            Storyboard.SetTarget(animationY, moveTransform);

            Storyboard.SetTargetProperty(animationX, new PropertyPath("X"));
            Storyboard.SetTargetProperty(animationY, new PropertyPath("Y"));
            animationX.To = moveTransform.X;
            animationY.To = moveTransform.Y;
            EuchreGrid.Resources.Add("storyboard", storyboard);

            storyboard.Begin();
            Thread.Sleep(700); // TODO:  remove hack
            EuchreGrid.Resources.Remove("storyboard");


            // Fourth, free the image of the animated card
            animatedCard.Visibility = Visibility.Hidden;
            animatedCard.Source = null;

            // Fifth, invisibly move the animated card back to 0,0 margin
            animatedCardFrame.Margin = new Thickness(0, 0, 0, 0);
        }

        private void SpeakWeGotEuchredMyFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayWeGotEuchredMyFault(gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].GetDisplayName());
            }
        }

        private void SpeakWeGotEuchredOurFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotEuchredOurFault(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeGotEuchredYourFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotEuchredYourFault(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotOne(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotTwo(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotFour(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakMeGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayMeGotOne(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
            }
        }

        private void SpeakMeGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayMeGotTwo(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
            }
        }

        private void SpeakMeGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayMeGotFour(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
            }
        }

        private void SpeakWeWon(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeWon(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakTheyWon(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayTheyWon(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeEuchredThem(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeEuchredThem(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakWeSuperEuchredThem(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
            {
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeSuperEuchredThem(gamePlayers[(int)seat].GetDisplayName());
            }
        }

        private void SpeakTheyGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayTheyGotOne();
            }
        }

        private void SpeakTheyGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayTheyGotTwo();
            }
        }

        private void SpeakTheyGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
            {
                gamePlayers[(int)seat].gameVoice.SayTheyGotFour();
            }
        }

        private void PlayResourceSound(System.IO.UnmanagedMemoryStream res)
        {
            using (var player = new System.Media.SoundPlayer(res))
            {
                player.PlaySync();
            }
        }

        private void PlayCardSound()
        {
            if (_modeSoundOn)
                PlayResourceSound(Properties.Resources.SoundPlayCard);
        }

        private void PlayApplause(int level)
        {
            if (_modeSoundOn)
            {
                switch (level)
                {
                case 1: PlayResourceSound(Properties.Resources.SoundApplauseSoft); break;
                case 2: PlayResourceSound(Properties.Resources.SoundApplauseLoud); break;
                case 3: PlayResourceSound(Properties.Resources.SoundApplauseWild); break;
                }
            }
        }

        private void PlayShuffleSound()
        {
            UpdateLayout();
            UpdateStatus(Properties.Resources.Notice_ShufflingCards);
            if (_modeSoundOn)
            {
                int numShuffle = EuchreTable.GenRandomNumber(2) + 1;
                for (int i = 0; i <= numShuffle; i++)
                {
                    PlayResourceSound(Properties.Resources.SoundShuffleDeck);
                }
            }
            UpdateStatus(Properties.Resources.Notice_DealingCards);
        }

        private void UpdateScoreText()
        {
            StringBuilder sTheirScore = new StringBuilder();
            sTheirScore.AppendFormat(Properties.Resources.Format_TheirScore, _gameTheirScore);
            TheirScore.Content = sTheirScore.ToString();
            TheirScore.UpdateLayout();

            StringBuilder sYourScore = new StringBuilder();
            sYourScore.AppendFormat(Properties.Resources.Format_YourScore, _gameYourScore);
            YourScore.Content = sYourScore.ToString();
            YourScore.UpdateLayout();

            if (_gameTheirScore > 10) _gameTheirScore = 10;

            if (_gameYourScore > 10) _gameYourScore = 10;

            SetImage(ThemScore, (System.Drawing.Image)Properties.Resources.ResourceManager.GetObject(GetScoreResourceName(ScorePrefix.ScoreThem, _gameTheirScore)));
            SetImage(UsScore, (System.Drawing.Image)Properties.Resources.ResourceManager.GetObject(GetScoreResourceName(ScorePrefix.ScoreUs, _gameYourScore)));
            UsScore.UpdateLayout();
            ThemScore.UpdateLayout();
        }

        private void UpdateTricksText()
        {
            StringBuilder sTheirTricks = new StringBuilder();
            sTheirTricks.AppendFormat(Properties.Resources.Format_TheirTricks, _gameTheirTricks);
            TheirTricks.Content = sTheirTricks.ToString();
            TheirTricks.UpdateLayout();

            StringBuilder sYourTricks = new StringBuilder();
            sYourTricks.AppendFormat(Properties.Resources.Format_YourTricks, _gameYourTricks);
            YourTricks.Content = sYourTricks.ToString();
            YourTricks.UpdateLayout();
        }

        private bool TheirTeamPickedTrumpThisHand()
        {
            return (handPickedTrump == EuchrePlayer.Seats.LeftOpponent || handPickedTrump == EuchrePlayer.Seats.RightOpponent);
        }

        private bool YourTeamPickedTrumpThisHand()
        {
            return (handPickedTrump == EuchrePlayer.Seats.Player || handPickedTrump == EuchrePlayer.Seats.Partner);
        }

        private bool TheirTeamWentAloneThisHand()
        {
            return (gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].handSittingOut || gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].handSittingOut);
        }

        private bool YourTeamWentAloneThisHand()
        {
            return (gamePlayers[(int)EuchrePlayer.Seats.Player].handSittingOut || gamePlayers[(int)EuchrePlayer.Seats.Partner].handSittingOut);
        }

        private void UpdateAllScores(EuchreState nextState)
        {
            int TheirTotalTricks = gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].handTricksWon;
            int YourTotalTricks = gamePlayers[(int)EuchrePlayer.Seats.Player].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.Partner].handTricksWon;
            switch (handPickedTrump)
            {
            case EuchrePlayer.Seats.LeftOpponent:
            case EuchrePlayer.Seats.RightOpponent:
                if (TheirTotalTricks == 0 && ruleUseSuperEuchre)
                {
                    _gameYourScore = _gameYourScore + 4; // SuperEuchred!
                    UpdateStatus(Properties.Resources.Notice_YouSuperEuchredThem);
                    PlayApplause(3);
                    SpeakWeSuperEuchredThem(EuchrePlayer.Seats.Player);
                }
                else if (TheirTotalTricks < 3)
                {
                    _gameYourScore = _gameYourScore + 2; // Euchred!
                    UpdateStatus(Properties.Resources.Notice_YouEuchredThem);
                    PlayApplause(2);
                    SpeakWeEuchredThem(EuchrePlayer.Seats.Player);
                }
                else if (TheirTotalTricks == 5)
                {
                    if (TheirTeamWentAloneThisHand())
                    {
                        _gameTheirScore = _gameTheirScore + 4;
                        UpdateStatus(Properties.Resources.Notice_TheyWonTheHandAllTricksAlone);
                        SpeakTheyGotFour(EuchrePlayer.Seats.Partner);
                    }
                    else
                    {
                        _gameTheirScore = _gameTheirScore + 2;
                        UpdateStatus(Properties.Resources.Notice_TheyWonTheHandAllTricks);
                        SpeakTheyGotTwo(EuchrePlayer.Seats.Partner);
                    }
                }
                else
                {
                    _gameTheirScore = _gameTheirScore + 1;
                    UpdateStatus(Properties.Resources.Notice_TheyWonTheHand);
                    SpeakTheyGotOne(EuchrePlayer.Seats.Partner);
                }

                break;

            case EuchrePlayer.Seats.Player:
            case EuchrePlayer.Seats.Partner:
                if (YourTotalTricks == 0 && ruleUseSuperEuchre)
                {
                    _gameTheirScore = _gameTheirScore + 4; // SuperEuchred!
                    UpdateStatus(Properties.Resources.Notice_TheySuperEuchredYou);
                    if (handPickedTrump == EuchrePlayer.Seats.Partner)
                        SpeakWeGotEuchredMyFault(handPickedTrump);
                    else if (!YourTeamWentAloneThisHand())
                        SpeakWeGotEuchredOurFault(handPickedTrump);
                    else
                        SpeakWeGotEuchredYourFault(handPickedTrump);
                }
                else if (YourTotalTricks < 3)
                {
                    _gameTheirScore = _gameTheirScore + 2; // Euchred!
                    UpdateStatus(Properties.Resources.Notice_TheyEuchredYou);
                    if (handPickedTrump == EuchrePlayer.Seats.Partner)
                        SpeakWeGotEuchredMyFault(handPickedTrump);
                    else if (!YourTeamWentAloneThisHand())
                        SpeakWeGotEuchredOurFault(handPickedTrump);
                    else
                        SpeakWeGotEuchredYourFault(handPickedTrump);
                }
                else if (YourTotalTricks == 5)
                {
                    if (YourTeamWentAloneThisHand())
                    {
                        _gameYourScore = _gameYourScore + 4;
                        UpdateStatus(Properties.Resources.Notice_YouWonTheHandAllTricksAlone);
                        PlayApplause(3);
                        if (handPickedTrump == EuchrePlayer.Seats.Player)
                            SpeakWeGotFour(handPickedTrump);
                        else
                            SpeakMeGotFour(handPickedTrump);
                    }
                    else
                    {
                        _gameYourScore = _gameYourScore + 2;
                        UpdateStatus(Properties.Resources.Notice_YouWonTheHandAllTricks);
                        PlayApplause(2);
                        if (handPickedTrump == EuchrePlayer.Seats.Player)
                            SpeakWeGotTwo(handPickedTrump);
                        else
                            SpeakMeGotTwo(handPickedTrump);
                    }
                }
                else
                {
                    _gameYourScore = _gameYourScore + 1;
                    UpdateStatus(Properties.Resources.Notice_YouWonTheHand);
                    PlayApplause(1);
                    if (handPickedTrump == EuchrePlayer.Seats.Player)
                        SpeakWeGotOne(handPickedTrump);
                    else
                        SpeakMeGotOne(handPickedTrump);
                }
                break;
            }
            UpdateScoreText();
            ShowAndEnableContinueButton(nextState);
        }

        private void SetUIElementVisibility(UIElement uie, Visibility visible)
        {
            uie.Visibility = visible;
            uie.UpdateLayout();
        }

        private void HideAllPlayedCards()
        {
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                gameTableTopCards[(int)i, 5].Source = null;
                SetUIElementVisibility(gameTableTopCards[(int)i, 5], Visibility.Hidden);
                SetTooltip(gameTableTopCards[(int)i, 5], null);
                handPlayedCards[(int)i] = null;
            }
        }

        private void ShowAllCards(bool ShowThem)
        {
            Visibility visible = ShowThem ? Visibility.Visible : Visibility.Hidden;
            Visibility visibleNoH = (ShowThem && !ruleUseNineOfHearts) ? Visibility.Visible : Visibility.Hidden;
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    SetUIElementVisibility(gameTableTopCards[(int)i, j], visible);
                }
            }
            SetUIElementVisibility(KittyCard1, visible);
            SetUIElementVisibility(KittyCard2, visibleNoH);
            SetUIElementVisibility(KittyCard3, visibleNoH);
            SetUIElementVisibility(KittyCard4, visibleNoH);
        }

        private void ResetScores()
        {
            _gameTheirScore = 0;
            _gameYourScore = 0;
            UpdateScoreText();
            _gameTheirTricks = 0;
            _gameYourTricks = 0;
            UpdateTricksText();
        }

        private void HideAndDisableUIElement(UIElement uie)
        {
            uie.IsEnabled = false;
            uie.IsHitTestVisible = false;
            SetUIElementVisibility(uie, Visibility.Hidden);
            uie.UpdateLayout();
            Refresh(uie);
        }

        private void ResetUserInputStates()
        {
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    gameTableTopCards[(int)i, j].IsEnabled = true;
                    gameTableTopCards[(int)i, j].Opacity = 1.0;
                    gameTableTopCards[(int)i, j].UpdateLayout();
                }
            }
            HideAndDisableUIElement(BidControl);
            HideAndDisableUIElement(BidControl2);
            HideAndDisableUIElement(ContinueButton);

            statePlayerIsDroppingACard = false;
            statePlayerIsPlayingACard = false;

            SetUIElementVisibility(SelectLabel, Visibility.Hidden);
            SetPlayerCursorToHand(false);
        }

        private void SetAllPlayerCardImages()
        {
            for (int i = 0; i <= 4; i++)
            {
                SetCardImage(
                    gamePlayers[(int)EuchrePlayer.Seats.Player].handCardsHeld[i],
                    Properties.Resources.ResourceManager.GetString(gamePlayers[(int)EuchrePlayer.Seats.Player].handCardsHeld[i].GetDisplayStringResourceName()),
                    EuchrePlayer.Seats.Player,
                    gameTableTopCards[(int)EuchrePlayer.Seats.Player, i],
                    gamePlayers[(int)EuchrePlayer.Seats.Player].handCardsHeld[i].GetImage(EuchrePlayer.Seats.Player));
            }
        }

        private void SetTooltip(Image img, string tip)
        {
            img.ToolTip = tip;
        }

        private void SetCardImage(EuchreCard card, string tooltip, EuchrePlayer.Seats perspective, Image img, System.Drawing.Image dimg)
        {
            SetTooltip(img, tooltip);
            card.Perspective = perspective;
            SetImage(img, dimg);
            img.UpdateLayout();
        }

        private void SetAllCardImages()
        {
            HideAllPlayedCards();
            SetAllPlayerCardImages();

            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.RightOpponent; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    SetCardImage(
                        gamePlayers[(int)i].handCardsHeld[j],
                        Properties.Resources.ResourceManager.GetString(gamePlayers[(int)i].handCardsHeld[j].GetDisplayStringResourceName(handTrumpSuit)),
                        i,
                        gameTableTopCards[(int)i, j],
                        gamePlayers[(int)i].handCardsHeld[j].GetImage(i));
                }
            }

            SetCardImage(
                handKitty[0],
                Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()),
                EuchrePlayer.Seats.Player,
                KittyCard1,
                handKitty[0].GetImage(EuchrePlayer.Seats.NoPlayer)
                );

            if (!ruleUseNineOfHearts)
            {
                SetCardImage(
                    handKitty[1],
                    Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()),
                    EuchrePlayer.Seats.Player,
                    KittyCard2,
                    handKitty[1].GetImage(EuchrePlayer.Seats.NoPlayer)
                    );
                SetCardImage(
                    handKitty[2],
                    Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()),
                    EuchrePlayer.Seats.Player,
                    KittyCard3,
                    handKitty[2].GetImage(EuchrePlayer.Seats.NoPlayer)
                    );
                SetCardImage(
                    handKitty[3],
                    Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()),
                    EuchrePlayer.Seats.Player,
                    KittyCard4,
                    handKitty[3].GetImage(EuchrePlayer.Seats.NoPlayer)
                    );
            }
        }

        private void DealACard(EuchrePlayer.Seats player, int slot)
        {
            gamePlayers[(int)player].handCardsHeld[slot] = _gameDeck.GetNextCard();
            if (player != EuchrePlayer.Seats.Player)
            {
                gamePlayers[(int)player].handCardsHeld[slot].stateCurrent = EuchreCard.States.FaceDown;
            }
            else
            {
                gamePlayers[(int)player].handCardsHeld[slot].stateCurrent = EuchreCard.States.FaceUp;
            }
            // TODO: AnimateACard call goes here.

            SetImage(gameTableTopCards[(int)player, slot], EuchreCard.imagesCardBack[(int)player]);
            SetUIElementVisibility(gameTableTopCards[(int)player, slot], Visibility.Visible);
            SetTooltip(gameTableTopCards[(int)player, slot], Properties.Resources.CARDNAME_BACK);
            PlayCardSound();
            RefreshAndSleep(gameTableTopCards[(int)player, slot]);
        }

        private void DealCards()
        {
            ShowAllCards(false);
            HideAllPlayedCards();

            _gameDeck.Shuffle();
            PlayShuffleSound();

            EuchrePlayer.Seats i;

            // Deal the cards 3-2-3-2, 2-3-2-3:
            i = handDealer;
            int k = 0;
            int m = 0;
            int n = 1;
            while (k < 2)
            {
                if (k == 0)
                {
                    if (n == 1)
                        n = 2;
                    else
                        n = 1;
                }
                else
                    n = 4;

                if (k == 0)
                    m = 0;
                else
                {
                    if (m == 2)
                        m = 3;
                    else
                        m = 2;
                }

                i = EuchrePlayer.NextPlayer(i);
                for (int j = m; j <= n; j++)
                    DealACard(i, j);
                if (i == handDealer)
                {
                    k = k + 1;
                    m = 2;
                }
            }

            // TODO:  AnimateACard needs to work for the kitty as well
            // TODO:  What about flipping the cards?
            for (int j = 0; j <= 3; j++)
            {
                handKitty[j] = _gameDeck.GetNextCard();
                if (j == 0)
                    handKitty[j].stateCurrent = EuchreCard.States.FaceUp;
                else if (handKitty[j] != null)
                    handKitty[j].stateCurrent = EuchreCard.States.FaceDown;
            }

            // Sort the players' cards according to the possible trump
            for (EuchrePlayer.Seats seat = EuchrePlayer.Seats.LeftOpponent; seat <= EuchrePlayer.Seats.Player; seat++)
            {
                gamePlayers[(int)seat].SortCards(EuchreCard.Suits.NoSuit);
            }

            SetAllCardImages();
            ShowAllCards(true);

            StringBuilder sKitty = new StringBuilder();
            sKitty.AppendFormat(Properties.Resources.Notice_KittyCard, Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()));
            UpdateStatus(sKitty.ToString());
        }

        private void UpdateAllTricks()
        {
            _gameTheirTricks = gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].handTricksWon;
            _gameYourTricks = gamePlayers[(int)EuchrePlayer.Seats.Player].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.Partner].handTricksWon;
            UpdateTricksText();
        }

        private void PlaySelectedCard(EuchrePlayer player, int index)
        {
            // TODO:  AnimateACard call goes here.

            if (index > 4) throw new System.Exception("Invalid index");

            player.handCardsHeld[index].stateCurrent = EuchreCard.States.FaceUp;
            System.Drawing.Image faceImage = player.handCardsHeld[index].GetImage(player.Seat);
            SetImage(gameTableTopCards[(int)player.Seat, 5], faceImage);

            string s = Properties.Resources.ResourceManager.GetString(player.handCardsHeld[index].GetDisplayStringResourceName(handTrumpSuit));
            if (string.IsNullOrEmpty(s)) throw new System.Exception("Invalid value");

            SetTooltip(gameTableTopCards[(int)player.Seat, 5], s);

            StringBuilder sPlayed = new StringBuilder();
            sPlayed.AppendFormat(Properties.Resources.Notice_PlayedACard, player.GetDisplayName(), s);
            UpdateStatus(sPlayed.ToString());

            handPlayedCards[(int)player.Seat] = player.handCardsHeld[index];

            MarkCardAsPlayed(player.handCardsHeld[index]);
            player.handCardsHeld[index] = null;
            SetTooltip(gameTableTopCards[(int)player.Seat, index], null);
            gameTableTopCards[(int)player.Seat, index].Source = null;

            SetUIElementVisibility(gameTableTopCards[(int)player.Seat, index], Visibility.Hidden);
            SetUIElementVisibility(gameTableTopCards[(int)player.Seat, 5], Visibility.Visible);

            UpdateLayout();

            PlayCardSound();
            Refresh(gameTableTopCards[(int)player.Seat, index]);
            RefreshAndSleep(gameTableTopCards[(int)player.Seat, 5]);
        }

        private void SwapCardWithKitty(EuchrePlayer player, int index)
        {
            EuchreCard card = handKitty[0];
            handKitty[0] = player.handCardsHeld[index];
            player.handCardsHeld[index] = card;
            handKitty[0].stateCurrent = EuchreCard.States.FaceDown;
            handKitty[0].Perspective = EuchrePlayer.Seats.Player;
            player.handCardsHeld[index].Perspective = EuchrePlayer.Seats.Player;

            if (player.Seat == EuchrePlayer.Seats.Player)
                player.handCardsHeld[index].stateCurrent = EuchreCard.States.FaceUp;
            else
                player.handCardsHeld[index].stateCurrent = EuchreCard.States.FaceDown;

            // Only this player knows that this card is buried -- don't add it to the "cards played" list
            player.trickBuriedCard = handKitty[0];
        }

        private void PrepTrick()
        {
            HideAllPlayedCards();

            trickLeader = gamePlayers[(int)trickLeaderIndex];
            trickPlayer = trickLeader;
            trickHighestCardSoFar = EuchreCard.Values.NoValue;
            trickPlayerWhoPlayedHighestCardSoFar = EuchrePlayer.Seats.NoPlayer;
        }

        private void SelectCardForTrick(EuchreState nextState)
        {
            if (!trickPlayer.handSittingOut)
            {
                if (trickPlayer.Seat == EuchrePlayer.Seats.Player)
                {
                    HumanPlayACard(); // Selected card will be set by side-effect when a card is clicked
                    _stateDesiredStateAfterHumanClick = nextState;
                }
                else
                {
                    trickSelectedCardIndex = trickPlayer.AutoPlayACard();
                    UpdateEuchreState(nextState);
                }
            }
            else
            {
                UpdateEuchreState(nextState);
            }
        }

        private void HumanPlayACard()
        {
            SelectLabel.Content = Properties.Resources.Notice_PlayACard;
            bool AnyValid = false;
            for (int i = 0; i <= 4; i++)
            {
                if (trickPlayer.handCardsHeld[i] != null)
                {
                    if (trickLeaderIndex != trickPlayer.Seat)
                    {
                        if (!trickPlayer.CardBelongsToLedSuit(trickPlayer.handCardsHeld[i]))
                        {
                            gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].IsEnabled = false;
                            gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 0.25;
                        }
                        else
                        {
                            AnyValid = true;
                            gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 1.0;
                        }
                    }
                }
                else
                {
                    gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].IsEnabled = false;
                    gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 0.25;
                }
            }

            if (!AnyValid) // Nothing of suit -- play what you want
            {
                for (int i = 0; i <= 4; i++)
                {
                    if (trickPlayer.handCardsHeld[i] != null)
                    {
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].IsEnabled = true;
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 1.0;
                    }
                }
            }

            SelectLabel.Visibility = Visibility.Visible;
            SelectLabel.UpdateLayout();
            statePlayerIsPlayingACard = true;
            SetPlayerCursorToHand(true);

        }

        private void PlayCardForTrick()
        {
            if (!trickPlayer.handSittingOut)
            {
                if (trickPlayer.Seat == EuchrePlayer.Seats.Player)
                {
                    SelectLabel.Visibility = Visibility.Hidden;
                    for (int i = 0; i <= 4; i++)
                    {
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].IsEnabled = true;
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 1.0;
                    }
                    UpdateLayout();
                }

                if (trickLeaderIndex == trickPlayer.Seat)
                {
                    trickSuitLed = trickPlayer.handCardsHeld[trickSelectedCardIndex].GetCurrentSuit(handTrumpSuit);
                }
                PlaySelectedCard(trickPlayer, trickSelectedCardIndex);

                EuchreCard.Values thisValue = handPlayedCards[(int)trickPlayer.Seat].GetCurrentValue(handTrumpSuit, handPlayedCards[(int)trickLeaderIndex].GetCurrentSuit(handTrumpSuit));
                if (thisValue > trickHighestCardSoFar)
                {
                    trickPlayerWhoPlayedHighestCardSoFar = trickPlayer.Seat;
                    trickHighestCardSoFar = handPlayedCards[(int)trickPlayer.Seat].GetCurrentValue(handTrumpSuit, handPlayedCards[(int)trickLeaderIndex].GetCurrentSuit(handTrumpSuit));
                }
            }
            trickPlayer = gamePlayers[(int)EuchrePlayer.NextPlayer(trickPlayer.Seat)];
        }

        private void PostTrick()
        {
            gamePlayers[(int)trickPlayerWhoPlayedHighestCardSoFar].handTricksWon = gamePlayers[(int)trickPlayerWhoPlayedHighestCardSoFar].handTricksWon + 1;
            switch (trickPlayerWhoPlayedHighestCardSoFar)
            {
            case EuchrePlayer.Seats.LeftOpponent:
            case EuchrePlayer.Seats.RightOpponent:
                UpdateStatus(Properties.Resources.Notice_TheirTeamWonTrick);
                break;
            case EuchrePlayer.Seats.Player:
            case EuchrePlayer.Seats.Partner:
                UpdateStatus(Properties.Resources.Notice_YourTeamWonTrick);
                break;
            }

            UpdateAllTricks();

            trickLeaderIndex = trickPlayerWhoPlayedHighestCardSoFar;
        }

        private void ClearAllTricks()
        {
            _gameTheirTricks = 0;
            _gameYourTricks = 0;

            // Clear individual information
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                gamePlayers[(int)i].ClearAllTricks();
            }

            // Clear general information
            ResetPlayedCards();
            UpdateTricksText();
        }

        private void PreBid1()
        {
            _handCurrentBidder = gamePlayers[(int)handDealer];
        }


        private void Bid1(EuchreState passedState)
        {
            _handCurrentBidder = gamePlayers[(int)EuchrePlayer.NextPlayer(_handCurrentBidder.Seat)];
            _handCurrentBidder.trickBuriedCard = null;

            bool GoingAlone = false;
            if (_handCurrentBidder.Seat == EuchrePlayer.Seats.Player)
            {
                _handCurrentBidder.HumanBidFirstRound();
                _stateDesiredBidPass = passedState;
            }
            else
            {
                bool rv;
                rv = _handCurrentBidder.AutoBidFirstRound(GoingAlone);
                _handCurrentBidder.ProcessBidFirstRound(GoingAlone, rv);
                RefreshAndSleep(StatusArea);
                if (rv)
                {
                    UpdateEuchreState(EuchreState.Bid1PickUp);
                }
                else
                {
                    UpdateEuchreState(passedState);
                    UpdateLayout();
                }
            }
        }

        private void Bid1PickUp()
        {
            if (gamePlayers[(int)handDealer].Seat == EuchrePlayer.Seats.Player)
            {
                HumanReplaceACard();
            }
            else
            {
                int index = gamePlayers[(int)handDealer].LowestCardOnReplace(handTrumpSuit);
                SwapCardWithKitty(gamePlayers[(int)handDealer], index);
                UpdateEuchreState(EuchreState.Bid1PickedUp);
            }
        }

        private void HumanReplaceACard()
        {
            SelectLabel.Content = Properties.Resources.Notice_SwapACard;
            SetUIElementVisibility(SelectLabel, Visibility.Visible);
            statePlayerIsDroppingACard = true;
            SetPlayerCursorToHand(true);
            _stateDesiredStateAfterHumanClick = EuchreState.Bid1PickedUp;
        }

        private void Bid1PickedUp()
        {
            if (gamePlayers[(int)handDealer].Seat == EuchrePlayer.Seats.Player)
            {
                SetUIElementVisibility(SelectLabel, Visibility.Hidden);
                SwapCardWithKitty(gamePlayers[(int)handDealer], trickSelectedCardIndex);
            }

            handPickedTrump = _handCurrentBidder.Seat;

            if (gamePlayers[(int)_handCurrentBidder.OppositeSeat()].handSittingOut)
            {
                EnableCards(_handCurrentBidder.OppositeSeat(), false);
            }
        }

        private void PreBid2()
        {
            handKitty[0].stateCurrent = EuchreCard.States.FaceDown;
            SetImage(KittyCard1, handKitty[0].GetImage(EuchrePlayer.Seats.NoPlayer));
            SetTooltip(KittyCard1, Properties.Resources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()));
            KittyCard1.UpdateLayout();
            MarkCardAsPlayed(handKitty[0]);

            for (EuchrePlayer.Seats seat = EuchrePlayer.Seats.LeftOpponent; seat <= EuchrePlayer.Seats.Player; seat++)
            {
                gamePlayers[(int)seat].SortCards(EuchreCard.Suits.NoSuit);
            }
            SetAllCardImages();
        }

        private void Bid2(EuchreState passedState)
        {
            _handCurrentBidder = gamePlayers[(int)EuchrePlayer.NextPlayer(_handCurrentBidder.Seat)];

            bool rv = false;
            bool GoingAlone = false;
            if (_handCurrentBidder.Seat == EuchrePlayer.Seats.Player)
            {
                _handCurrentBidder.HumanBidSecondRound();
                _stateDesiredBidPass = passedState;
            }
            else
            {
                rv = _handCurrentBidder.AutoBidSecondRound(GoingAlone);
                _handCurrentBidder.ProcessBidSecondRound(GoingAlone, rv);
                RefreshAndSleep(this.StatusArea);
                if (rv)
                {
                    UpdateEuchreState(EuchreState.Bid2Succeeded);
                }
                else
                {
                    UpdateEuchreState(passedState);
                    UpdateLayout();
                }
            }
        }

        private void SetForNewHand()
        {
            ClearAllTricks();
            TrumpPartner.Visibility = Visibility.Hidden;
            TrumpPlayer.Visibility = Visibility.Hidden;
            TrumpLeft.Visibility = Visibility.Hidden;
            TrumpRight.Visibility = Visibility.Hidden;
            SetTooltip(TrumpPartner, null);
            SetTooltip(TrumpPlayer, null);
            SetTooltip(TrumpLeft, null);
            SetTooltip(TrumpRight, null);
            handTrumpSuit = EuchreCard.Suits.NoSuit;
            ResetPlayedCards();
        }

        private void SortAndSetHandImagesAndText()
        {
            for (EuchrePlayer.Seats seat = EuchrePlayer.Seats.LeftOpponent; seat <= EuchrePlayer.Seats.Player; seat++)
            {
                gamePlayers[(int)seat].SortCards(handTrumpSuit);
            }

            SetAllCardImages();

            switch (handPickedTrump)
            {
            case EuchrePlayer.Seats.Partner:
                SetImage(TrumpPartner, EuchreCard.imagesSuit[(int)handTrumpSuit]);
                SetTooltip(TrumpPartner, Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(handTrumpSuit)));
                SetUIElementVisibility(TrumpPartner, Visibility.Visible);
                break;
            case EuchrePlayer.Seats.Player:
                SetImage(TrumpPlayer, EuchreCard.imagesSuit[(int)handTrumpSuit]);
                SetTooltip(TrumpPlayer, Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(handTrumpSuit)));
                SetUIElementVisibility(TrumpPlayer, Visibility.Visible);
                break;
            case EuchrePlayer.Seats.LeftOpponent:
                SetImage(TrumpLeft, EuchreCard.imagesSuit[(int)handTrumpSuit]);
                SetTooltip(TrumpLeft, Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(handTrumpSuit)));
                SetUIElementVisibility(TrumpLeft, Visibility.Visible);
                break;
            case EuchrePlayer.Seats.RightOpponent:
                SetImage(TrumpRight, EuchreCard.imagesSuit[(int)handTrumpSuit]);
                SetTooltip(TrumpRight, Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(handTrumpSuit)));
                SetUIElementVisibility(TrumpRight, Visibility.Visible);
                break;
            }
        }

        private void DetermineWinnerAndEndGame()
        {
            if (_gameTheirScore > _gameYourScore)
            {
                UpdateStatus(Properties.Resources.Notice_TheyWonTheGame, 2);
                SpeakTheyWon(EuchrePlayer.Seats.Player);
            }
            else
            {
                UpdateStatus(Properties.Resources.Notice_YouWonTheGame, 2);
                SpeakWeWon(EuchrePlayer.Seats.Player);
            }
            _stateGameStarted = false;
        }

        private void CleanupAfterHand()
        {
            UpdateLayout();

            if (gamePlayers[(int)gamePlayers[(int)handPickedTrump].OppositeSeat()].handSittingOut)
            {
                gamePlayers[(int)gamePlayers[(int)handPickedTrump].OppositeSeat()].handSittingOut = false;
                EnableCards(gamePlayers[(int)handPickedTrump].OppositeSeat(), true);
            }
        }

        private void NoOp()
        {
        }

        private void SetNextDealer()
        {
            _gameDealerBox[(int)handDealer].Visibility = Visibility.Hidden;
            handDealer = EuchrePlayer.NextPlayer(handDealer);
            _gameDealerBox[(int)handDealer].Visibility = Visibility.Visible;
        }

        private EuchreCard DealACardForDeal(EuchrePlayer.Seats player, int slot)
        {
            EuchreCard card = _gameDeck.GetNextCard();
            card.Perspective = player;

            // TODO:  Animate a card goes here
            AnimateACard(EuchreCard.imagesCardBack[(int)player], ContinueButton, gameTableTopCards[(int)player, slot],card.Perspective);

            SetImage(gameTableTopCards[(int)player, slot], card.imageCurrent);
            SetUIElementVisibility(gameTableTopCards[(int)player, slot], Visibility.Visible);
            SetTooltip(gameTableTopCards[(int)player, slot], Properties.Resources.ResourceManager.GetString(card.GetDisplayStringResourceName()));

            StringBuilder sDealt = new StringBuilder();
            sDealt.AppendFormat(Properties.Resources.Notice_DealtACard, gamePlayers[(int)player].GetDisplayName(), Properties.Resources.ResourceManager.GetString(card.GetDisplayStringResourceName()));
            UpdateStatus(sDealt.ToString());

            PlayCardSound();

            RefreshAndSleep(gameTableTopCards[(int)player, slot]);
            return card;
        }

        private void PreDealerSelection()
        {
            stateSelectingDealer = true;
            _gameDeck.Shuffle();
            PlayShuffleSound();
            UpdateStatus(Properties.Resources.Notice_ChoosingDealer);
            _handPotentialDealer = EuchrePlayer.Seats.Player;
            potentialDealerCardIndex = 0;
        }

        private void TrySelectDealer()
        {
            EuchreCard card = DealACardForDeal(_handPotentialDealer, potentialDealerCardIndex);
            if (card.Rank == EuchreCard.Ranks.Jack)
            {
                UpdateEuchreState(EuchreState.DealerSelected);
            }
            else
            {
                _handPotentialDealer = EuchrePlayer.NextPlayer(_handPotentialDealer);
                if (_handPotentialDealer == EuchrePlayer.Seats.Player)
                {
                    potentialDealerCardIndex += 1;
                }
                UpdateEuchreState(EuchreState.StillSelectingDealer);
            }
        }

        private void PostDealerSelection(EuchreState nextState)
        {
            StringBuilder sDealer = new StringBuilder();
            sDealer.AppendFormat(Properties.Resources.Notice_IAmTheDealer, gamePlayers[(int)_handPotentialDealer].GetDisplayName());
            UpdateStatus(sDealer.ToString());
            ShowAndEnableContinueButton(nextState);
        }

        private void PostDealerCleanup()
        {
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    SetUIElementVisibility(gameTableTopCards[i, j], Visibility.Hidden);
                    gameTableTopCards[i, j].Source = null;
                    SetTooltip(gameTableTopCards[i, j], null);
                }
            }

            stateSelectingDealer = false;
            handDealer = _handPotentialDealer;
            _gameDealerBox[(int)handDealer].Visibility = Visibility.Visible;
        }

        private void ShowAndEnableContinueButton(EuchreState nextState)
        {
            ContinueButton.Visibility = Visibility.Visible;
            ContinueButton.IsEnabled = true;
            ContinueButton.IsDefault = true;
            ContinueButton.IsHitTestVisible = true;
            UpdateLayout();
            _stateDesiredStateAfterHumanClick = nextState;
        }

        private bool StartItUp()
        {
            if (_gameOptionsDialog == null)
            {
                _gameOptionsDialog = new EuchreOptions();
            }

            _gameOptionsDialog.Left = Left + (Width - _gameOptionsDialog.Width) / 2;
            _gameOptionsDialog.Top = Top + (Height - _gameOptionsDialog.Height) / 2;

            _gameOptionsDialog.ShowDialog();
            if (_gameOptionsDialog.LocalDialogResult)
            {
                ruleStickTheDealer = (bool)_gameOptionsDialog.StickTheDealer.IsChecked;
                ruleUseNineOfHearts = (bool)_gameOptionsDialog.NineOfHearts.IsChecked;
                modePeekAtOtherCards = (bool)_gameOptionsDialog.PeekAtOtherCards.IsChecked;
                ruleUseSuperEuchre = (bool)_gameOptionsDialog.SuperEuchre.IsChecked;
                ruleUseQuietDealer = (bool)_gameOptionsDialog.QuietDealer.IsChecked;
                _modeSoundOn = (bool)_gameOptionsDialog.SoundOn.IsChecked;

                gamePlayerName = string.IsNullOrEmpty(_gameOptionsDialog.PlayerName.Text) ? Properties.Resources.Player_Player : _gameOptionsDialog.PlayerName.Text;
                gameLeftOpponentName = string.IsNullOrEmpty(_gameOptionsDialog.LeftOpponentName.Text) ? Properties.Resources.Player_LeftOpponent : _gameOptionsDialog.LeftOpponentName.Text;
                gameRightOpponentName = string.IsNullOrEmpty(_gameOptionsDialog.RightOpponentName.Text) ? Properties.Resources.Player_RightOpponent : _gameOptionsDialog.RightOpponentName.Text;

                gamePartnerName = _gameOptionsDialog.PartnerName.Text;
                if (string.IsNullOrEmpty(gamePartnerName))
                {
                    StringBuilder s = new StringBuilder();
                    s.AppendFormat(Properties.Resources.Player_Partner, gamePlayerName);
                    gamePartnerName = s.ToString();
                }

                if ((bool)_gameOptionsDialog.LeftOpponentCrazy.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if ((bool)_gameOptionsDialog.LeftOpponentNormal.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Conservative;


                if ((bool)_gameOptionsDialog.RightOpponentCrazy.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if ((bool)_gameOptionsDialog.RightOpponentNormal.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Conservative;


                if ((bool)_gameOptionsDialog.PartnerCrazy.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if ((bool)_gameOptionsDialog.PartnerNormal.IsChecked)
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Conservative;

            }
            else
            {
                return false;
            }


            PlayerNameLabel.Content = gamePlayerName;
            PartnerNameLabel.Content = gamePartnerName;
            LeftOpponentNameLabel.Content = gameLeftOpponentName;
            RightOpponentNameLabel.Content = gameRightOpponentName;
            ShowAllNameLabels(true);

            gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gameVoice.SetVoice(_gameOptionsDialog.LeftVoiceCombo.Text);
            gamePlayers[(int)EuchrePlayer.Seats.Partner].gameVoice.SetVoice(_gameOptionsDialog.PartnerVoiceCombo.Text);
            gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gameVoice.SetVoice(_gameOptionsDialog.RightVoiceCombo.Text);

            PlayerNameLabel.UpdateLayout();
            PartnerNameLabel.UpdateLayout();
            LeftOpponentNameLabel.UpdateLayout();
            RightOpponentNameLabel.UpdateLayout();

            // Generate the deck
            _gameDeck = new EuchreCardDeck(ruleUseNineOfHearts, this);
            _gameDeck.Initialize();

            return true;
        }

        private bool QueryCancelClose()
        {
            if (_stateGameStarted)
            {
                if (MessageBox.Show(Properties.Resources.Command_Exit, Properties.Resources.Command_ExitTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK)
                {
                    return true;
                }
            }
            return false;
        }

        private bool NewGame()
        {
            UpdateStatus(Properties.Resources.Notice_StartingNewGame);
            ResetScores();
            ResetUserInputStates();
            ShowAllCards(false);
            HideAllPlayedCards();
            ShowAllNameLabels(false);
            HideAllDealerAndTrumpLabels();
            if (StartItUp()) // True if we started a game
            {
                _stateGameStarted = true;
                return true;
            }
            else
            {
                Close(); // User cancelled out of the options dialog, so we'll quit.
                _gameOptionsDialog.DisposeVoice();
                _gameOptionsDialog = null; // Dialog has closed and is not useful anymore.
                return false;
            }
        }

        private void CleanUpGame()
        {
            ShowAllCards(false);
            HideAllPlayedCards();
            HideAllDealerAndTrumpLabels();
            _stateGameStarted = false;
        }

        private void ShowAllNameLabels(bool ShowAll)
        {
            Visibility visible = ShowAll ? Visibility.Visible : Visibility.Hidden;
            PlayerNameLabel.Visibility = visible;
            PartnerNameLabel.Visibility = visible;
            LeftOpponentNameLabel.Visibility = visible;
            RightOpponentNameLabel.Visibility = visible;
        }

        private void HideAllDealerAndTrumpLabels()
        {
            DealerLeftOpponent.Visibility = Visibility.Hidden;
            DealerRightOpponent.Visibility = Visibility.Hidden;
            DealerPartner.Visibility = Visibility.Hidden;
            DealerPlayer.Visibility = Visibility.Hidden;

            TrumpLeft.Visibility = Visibility.Hidden;
            TrumpRight.Visibility = Visibility.Hidden;
            TrumpPlayer.Visibility = Visibility.Hidden;
            TrumpPartner.Visibility = Visibility.Hidden;

            SetTooltip(TrumpPartner, null);
            SetTooltip(TrumpPlayer, null);
            SetTooltip(TrumpLeft, null);
            SetTooltip(TrumpRight, null);
        }
        #endregion

        #region "Event handlers"

        private void PlayerCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (statePlayerIsDroppingACard || statePlayerIsPlayingACard)
            {
                if (sender == PlayerCard1)
                    trickSelectedCardIndex = 0;
                else if (sender == PlayerCard2)
                    trickSelectedCardIndex = 1;
                else if (sender == PlayerCard3)
                    trickSelectedCardIndex = 2;
                else if (sender == PlayerCard4)
                    trickSelectedCardIndex = 3;
                else if (sender == PlayerCard5)
                    trickSelectedCardIndex = 4;

                statePlayerIsDroppingACard = false;
                statePlayerIsPlayingACard = false;
                SetPlayerCursorToHand(false);

                // Signal that we're ready to commence again
                UpdateEuchreState(_stateDesiredStateAfterHumanClick);
            }
        }

        private void EuchreTable_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((bool)!(e.Cancel = QueryCancelClose()))
            {
                // We are closing for cure -- dispose the voices
                for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
                {
                    gamePlayers[(int)i].DisposeVoice();
                }
                if (_gameOptionsDialog != null)
                {
                    _gameOptionsDialog.DisposeVoice();
                }
                Dispatcher.InvokeShutdown(); // Clear out any remaining commands
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateEuchreState(EuchreState.StartNewGameRequested);
        }

        private void RulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Window rulesWindow = null;
            foreach (Window win in System.Windows.Application.Current.Windows) // Man, I really miss the "My" namespace...
            {
                if (win.GetType().ToString() == "CSEuchre4.EuchreRules")
                {
                    rulesWindow = win;
                    break;
                }
            }
            if (rulesWindow == null)
            {
                EuchreRules x = new EuchreRules();
                x.Show();
            }
            else
            {
                rulesWindow.Activate();
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EuchreAboutBox dlg = new EuchreAboutBox(this);
            dlg.ShowDialog();
        }

        private void EuchreTable_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                UpdateEuchreState(EuchreState.StartNewGameRequested);
            }
        }

        private void EuchreTable_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.CanMinimize;
            SetImage(Logo, Properties.Resources.logo);
            SetIcon(this, Properties.Resources.Euchre);
            UpdateStatus(Properties.Resources.Notice_Welcome);
            _cursorCached = Cursor;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            SetUIElementVisibility(ContinueButton, Visibility.Hidden);
            ContinueButton.IsEnabled = false;
            ContinueButton.IsHitTestVisible = false;
            UpdateEuchreState(_stateDesiredStateAfterHumanClick);
        }

        private static Action EmptyDelegate = delegate () { };
        private static void RefreshAndSleep(UIElement uie)
        {
            Refresh(uie);
            Thread.Sleep(_timerSleepDuration);
        }
        private static void Refresh(UIElement uie)
        {
            uie.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        #endregion

        #region "Enums"
        public enum EuchreState
        {
            NoState,
            StartNewGameRequested,
            StartNewGameConfirmed,
            StillSelectingDealer,
            DealerSelected,
            DealerAcknowledged,
            ClearHand,
            StartNewHand,
            DealCards,
            Bid1Starts,
            Bid1Player0,
            Bid1Player1,
            Bid1Player2,
            Bid1Player3,
            Bid1PickUp,
            Bid1PickedUp,
            Bid1Failed,
            Bid1FailedAcknowledged,
            Bid1Succeeded,
            Bid1SucceededAcknowledged,
            Bid2Starts,
            Bid2Player0,
            Bid2Player1,
            Bid2Player2,
            Bid2Player3,
            Bid2Failed,
            Bid2FailedAcknowledged,
            Bid2Succeeded,
            Bid2SucceededAcknowledged,
            Trick0Started,
            Trick0_SelectCard0,
            Trick0_PlayCard0,
            Trick0_SelectCard1,
            Trick0_PlayCard1,
            Trick0_SelectCard2,
            Trick0_PlayCard2,
            Trick0_SelectCard3,
            Trick0_PlayCard3,
            Trick0Ended,
            Trick0EndingAcknowledged,
            Trick1Started,
            Trick1_SelectCard0,
            Trick1_PlayCard0,
            Trick1_SelectCard1,
            Trick1_PlayCard1,
            Trick1_SelectCard2,
            Trick1_PlayCard2,
            Trick1_SelectCard3,
            Trick1_PlayCard3,
            Trick1Ended,
            Trick1EndingAcknowledged,
            Trick2Started,
            Trick2_SelectCard0,
            Trick2_PlayCard0,
            Trick2_SelectCard1,
            Trick2_PlayCard1,
            Trick2_SelectCard2,
            Trick2_PlayCard2,
            Trick2_SelectCard3,
            Trick2_PlayCard3,
            Trick2Ended,
            Trick2EndingAcknowledged,
            Trick3Started,
            Trick3_SelectCard0,
            Trick3_PlayCard0,
            Trick3_SelectCard1,
            Trick3_PlayCard1,
            Trick3_SelectCard2,
            Trick3_PlayCard2,
            Trick3_SelectCard3,
            Trick3_PlayCard3,
            Trick3Ended,
            Trick3EndingAcknowledged,
            Trick4Started,
            Trick4_SelectCard0,
            Trick4_PlayCard0,
            Trick4_SelectCard1,
            Trick4_PlayCard1,
            Trick4_SelectCard2,
            Trick4_PlayCard2,
            Trick4_SelectCard3,
            Trick4_PlayCard3,
            Trick4Ended,
            Trick4EndingAcknowledged,
            HandCompleted,
            HandCompletedAcknowledged,
            GameOver
        }

        private enum ScorePrefix
        {
            ScoreThem = 1,
            ScoreUs = 2
        }

        #endregion

        #region "Private variables"
        private int _gameTheirScore;
        private int _gameYourScore;
        private int _gameTheirTricks;
        private int _gameYourTricks;
        private bool _modeSoundOn = true;
        private bool _stateGameStarted = false;

        private EuchreCardDeck _gameDeck;
        private EuchreOptions _gameOptionsDialog = null;

        private GroupBox[] _gameDealerBox = new GroupBox[4];

        private int _trickPlayedCardIndex;

        private EuchreState _stateDesiredBidPass;
        private EuchreState _stateDesiredStateAfterHumanClick;
        private EuchreState _stateLast;
        private EuchreState _stateCurrent;
        private EuchrePlayer _handCurrentBidder;
        private EuchrePlayer.Seats _handPotentialDealer;
        private int potentialDealerCardIndex;

        private const int _timerSleepDuration = 250;
        private System.Windows.Input.Cursor _cursorCached;

        #endregion

        #region "Public variables"
        public bool ruleStickTheDealer = false;
        public bool ruleUseNineOfHearts = false;
        public bool ruleUseSuperEuchre = false;
        public bool ruleUseQuietDealer = false;

        public bool modePeekAtOtherCards = false;

        public bool stateSelectingDealer = false;
        public bool statePlayerIsDroppingACard = false;
        public bool statePlayerIsPlayingACard = false;

        public EuchrePlayer[] gamePlayers = new EuchrePlayer[4];
        public string gamePlayerName = "";
        public string gamePartnerName = "";
        public string gameLeftOpponentName = "";
        public string gameRightOpponentName = "";
        public Image[,] gameTableTopCards = new Image[4, 6];// (Player index, card index -- final value is played card)

        public EuchreCard.Suits handTrumpSuit;
        public EuchrePlayer.Seats handPickedTrump = EuchrePlayer.Seats.NoPlayer;
        public EuchrePlayer.Seats handDealer;
        public EuchreCard[] handCardsPlayed = new EuchreCard[24];
        public EuchreCard[] handPlayedCards = new EuchreCard[4];
        public EuchreCard[] handKitty = new EuchreCard[4];

        public EuchrePlayer.Seats trickLeaderIndex;
        public EuchreCard.Suits trickSuitLed;
        public int trickSelectedCardIndex = 0;
        public EuchrePlayer trickLeader;
        public EuchrePlayer trickPlayer;
        public EuchreCard.Values trickHighestCardSoFar;
        public EuchrePlayer.Seats trickPlayerWhoPlayedHighestCardSoFar;
        #endregion
    }
}
