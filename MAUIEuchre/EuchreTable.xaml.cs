using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace MAUIEuchre
{
    public partial class EuchreTable : ContentPage, IGameTable
    {
        #region "Static methods"

        static private readonly string[] NumberWords = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

        static private string GetScoreResourceName(ScorePrefix prefix, int value)
        {
            StringBuilder Score = new StringBuilder();
            switch (prefix)
            {
            case ScorePrefix.ScoreThem:
                Score.Append("scorethem");
                break;
            case ScorePrefix.ScoreUs:
                Score.Append("scoreus");
                break;
            }
            Score.Append(NumberWords[value]);
            Score.Append(".png");
            return Score.ToString();
        }

        static private void SetImage(Image img, string resourceName)
        {
            img.Source = ImageSource.FromFile(resourceName);
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

            ContinueButton.Clicked += ContinueButton_Click;

            BidControl.gameTable = this;
            BidControl2.gameTable = this;

            var playerCardTap1 = new TapGestureRecognizer(); playerCardTap1.Tapped += PlayerCard_Click; PlayerCard1.GestureRecognizers.Add(playerCardTap1);
            var playerCardTap2 = new TapGestureRecognizer(); playerCardTap2.Tapped += PlayerCard_Click; PlayerCard2.GestureRecognizers.Add(playerCardTap2);
            var playerCardTap3 = new TapGestureRecognizer(); playerCardTap3.Tapped += PlayerCard_Click; PlayerCard3.GestureRecognizers.Add(playerCardTap3);
            var playerCardTap4 = new TapGestureRecognizer(); playerCardTap4.Tapped += PlayerCard_Click; PlayerCard4.GestureRecognizers.Add(playerCardTap4);
            var playerCardTap5 = new TapGestureRecognizer(); playerCardTap5.Tapped += PlayerCard_Click; PlayerCard5.GestureRecognizers.Add(playerCardTap5);

            Loaded += EuchreTable_Loaded;
        }

        private void EuchreTable_Loaded(object? sender, EventArgs e)
        {
            Logo.Source = ImageSource.FromFile("logo.png");
            UpdateStatus(AppResources.GetString("Notice_Welcome"));
            UpdateEuchreState(EuchreState.StartNewGameRequested);
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

        public void UpdateEuchreState(EuchreState state)
        {
            _stateLast = _stateCurrent;
            _stateCurrent = state;
            _stateDesiredStateAfterHumanClick = EuchreState.NoState;

            MainThread.BeginInvokeOnMainThread(async () => await MasterStateDirector());
        }

        public void RevertEuchreState()
        {
            UpdateEuchreState(_stateLast);
            _stateLast = EuchreState.NoState;
        }

        public async Task MasterStateDirector()
        {
            switch (_stateCurrent)
            {
            case EuchreState.NoState:
                break;

            case EuchreState.StartNewGameRequested:
                if (_stateGameStarted)
                {
                    if (!await RestartGame())
                    {
                        RevertEuchreState();
                        return;
                    }
                }
                if (await NewGame())
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
                await PreDealerSelection();
                UpdateEuchreState(EuchreState.StillSelectingDealer);
                break;
            case EuchreState.StillSelectingDealer:
                await TrySelectDealer();
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
                await DealCards();
                trickLeaderIndex = EuchrePlayer.NextPlayer(handDealer);
                UpdateEuchreState(EuchreState.Bid1Starts);
                break;

            case EuchreState.Bid1Starts:
                PreBid1();
                UpdateEuchreState(EuchreState.Bid1Player0);
                break;
            case EuchreState.Bid1Player0:
                await Bid1(EuchreState.Bid1Player1);
                break;
            case EuchreState.Bid1Player1:
                await Bid1(EuchreState.Bid1Player2);
                break;
            case EuchreState.Bid1Player2:
                await Bid1(EuchreState.Bid1Player3);
                break;
            case EuchreState.Bid1Player3:
                await Bid1(EuchreState.Bid2Starts);
                break;
            case EuchreState.Bid1PickUp:
                await Bid1PickUp();
                break;
            case EuchreState.Bid1PickedUp:
                await Bid1PickedUp();
                UpdateEuchreState(EuchreState.Bid1Succeeded);
                break;
            case EuchreState.Bid1Succeeded:
                SortAndSetHandImagesAndText();
                ShowAndEnableContinueButton(EuchreState.Bid1SucceededAcknowledged);
                break;
            case EuchreState.Bid1SucceededAcknowledged:
                UpdateEuchreState(EuchreState.Trick0Started);
                break;

            case EuchreState.Bid2Starts:
                PreBid2();
                UpdateEuchreState(EuchreState.Bid2Player0);
                break;
            case EuchreState.Bid2Player0:
                await Bid2(EuchreState.Bid2Player1);
                break;
            case EuchreState.Bid2Player1:
                await Bid2(EuchreState.Bid2Player2);
                break;
            case EuchreState.Bid2Player2:
                await Bid2(EuchreState.Bid2Player3);
                break;
            case EuchreState.Bid2Player3:
                await Bid2(EuchreState.Bid2Failed);
                break;
            case EuchreState.Bid2Succeeded:
                ShowAndEnableContinueButton(EuchreState.Bid2SucceededAcknowledged);
                break;
            case EuchreState.Bid2SucceededAcknowledged:
                SortAndSetHandImagesAndText();
                UpdateEuchreState(EuchreState.Trick0Started);
                break;
            case EuchreState.Bid2Failed:
                UpdateStatusBold(AppResources.GetString("Notice_AllPassedTwice"));
                ShowAndEnableContinueButton(EuchreState.Bid2FailedAcknowledged);
                break;
            case EuchreState.Bid2FailedAcknowledged:
                SetNextDealer();
                UpdateEuchreState(EuchreState.ClearHand);
                break;

            // Tricks 0-4: same pattern
            case EuchreState.Trick0Started:
            case EuchreState.Trick1Started:
            case EuchreState.Trick2Started:
            case EuchreState.Trick3Started:
            case EuchreState.Trick4Started:
                PrepTrick();
                UpdateEuchreState(_stateCurrent + 1); // -> SelectCard0
                break;

            case EuchreState.Trick0_SelectCard0:
            case EuchreState.Trick1_SelectCard0:
            case EuchreState.Trick2_SelectCard0:
            case EuchreState.Trick3_SelectCard0:
            case EuchreState.Trick4_SelectCard0:
                SelectCardForTrick(_stateCurrent + 1);
                break;
            case EuchreState.Trick0_PlayCard0:
            case EuchreState.Trick1_PlayCard0:
            case EuchreState.Trick2_PlayCard0:
            case EuchreState.Trick3_PlayCard0:
            case EuchreState.Trick4_PlayCard0:
                await PlayCardForTrick();
                UpdateEuchreState(_stateCurrent + 1); // -> SelectCard1
                break;
            case EuchreState.Trick0_SelectCard1:
            case EuchreState.Trick1_SelectCard1:
            case EuchreState.Trick2_SelectCard1:
            case EuchreState.Trick3_SelectCard1:
            case EuchreState.Trick4_SelectCard1:
                SelectCardForTrick(_stateCurrent + 1);
                break;
            case EuchreState.Trick0_PlayCard1:
            case EuchreState.Trick1_PlayCard1:
            case EuchreState.Trick2_PlayCard1:
            case EuchreState.Trick3_PlayCard1:
            case EuchreState.Trick4_PlayCard1:
                await PlayCardForTrick();
                UpdateEuchreState(_stateCurrent + 1); // -> SelectCard2
                break;
            case EuchreState.Trick0_SelectCard2:
            case EuchreState.Trick1_SelectCard2:
            case EuchreState.Trick2_SelectCard2:
            case EuchreState.Trick3_SelectCard2:
            case EuchreState.Trick4_SelectCard2:
                SelectCardForTrick(_stateCurrent + 1);
                break;
            case EuchreState.Trick0_PlayCard2:
            case EuchreState.Trick1_PlayCard2:
            case EuchreState.Trick2_PlayCard2:
            case EuchreState.Trick3_PlayCard2:
            case EuchreState.Trick4_PlayCard2:
                await PlayCardForTrick();
                UpdateEuchreState(_stateCurrent + 1); // -> SelectCard3
                break;
            case EuchreState.Trick0_SelectCard3:
            case EuchreState.Trick1_SelectCard3:
            case EuchreState.Trick2_SelectCard3:
            case EuchreState.Trick3_SelectCard3:
            case EuchreState.Trick4_SelectCard3:
                SelectCardForTrick(_stateCurrent + 1);
                break;
            case EuchreState.Trick0_PlayCard3:
            case EuchreState.Trick1_PlayCard3:
            case EuchreState.Trick2_PlayCard3:
            case EuchreState.Trick3_PlayCard3:
                await PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(_stateCurrent + 2); // -> TrickNEndingAcknowledged (skip TrickNEnded)
                break;
            case EuchreState.Trick4_PlayCard3:
                await PlayCardForTrick();
                PostTrick();
                ShowAndEnableContinueButton(EuchreState.Trick4EndingAcknowledged);
                break;

            case EuchreState.Trick0EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick1Started);
                break;
            case EuchreState.Trick1EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick2Started);
                break;
            case EuchreState.Trick2EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick3Started);
                break;
            case EuchreState.Trick3EndingAcknowledged:
                UpdateEuchreState(EuchreState.Trick4Started);
                break;
            case EuchreState.Trick4EndingAcknowledged:
                UpdateEuchreState(EuchreState.HandCompleted);
                break;

            case EuchreState.HandCompleted:
                await UpdateAllScores(EuchreState.HandCompletedAcknowledged);
                break;
            case EuchreState.HandCompletedAcknowledged:
                CleanupAfterHand();
                if (_gameTheirScore >= 10 || _gameYourScore >= 10)
                    UpdateEuchreState(EuchreState.GameOver);
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

        public async Task<bool> RestartGame()
        {
            return await DisplayAlert(AppResources.GetString("Command_NewTitle"), AppResources.GetString("Command_New"), "OK", "Cancel");
        }

        public void UpdateStatus(string s, int WhiteSpace = 1)
        {
            StatusArea.Text += System.Net.WebUtility.HtmlEncode(s);
            if (WhiteSpace > 0)
            {
                for (int i = 1; i <= WhiteSpace; i++)
                {
                    StatusArea.Text += "<br>";
                }
            }
            ScrollStatusToBottom();
        }

        public void UpdateStatusBold(string s, int WhiteSpace = 1)
        {
            StatusArea.Text += "<b>" + System.Net.WebUtility.HtmlEncode(s) + "</b>";
            if (WhiteSpace > 0)
            {
                for (int i = 1; i <= WhiteSpace; i++)
                {
                    StatusArea.Text += "<br>";
                }
            }
            ScrollStatusToBottom();
        }

        public void UpdateStatusSeparator()
        {
            StatusArea.Text += "<br>";
            ScrollStatusToBottom();
        }

        public void UpdateStatusBoldName(string format, string boldArg, params string[] otherArgs)
        {
            string encodedBold = "<b>" + System.Net.WebUtility.HtmlEncode(boldArg) + "</b>";
            object[] allArgs = new object[otherArgs.Length + 1];
            allArgs[0] = encodedBold;
            for (int i = 0; i < otherArgs.Length; i++)
            {
                allArgs[i + 1] = System.Net.WebUtility.HtmlEncode(otherArgs[i]);
            }
            StatusArea.Text += string.Format(format, allArgs) + "<br>";
            ScrollStatusToBottom();
        }

        private void ClearStatus()
        {
            StatusArea.Text = "";
        }

        private void ScrollStatusToBottom()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(150);
                await StatusScrollView.ScrollToAsync(StatusArea, ScrollToPosition.End, false);
            });
        }

        public void EnableCards(EuchrePlayer.Seats player, bool EnableIt)
        {
            for (int i = 0; i <= 4; i++)
            {
                gameTableTopCards[(int)player, i].IsEnabled = EnableIt;
                gameTableTopCards[(int)player, i].Opacity = EnableIt ? 1.0 : 0.25;
            }
        }

        public void ShowBidFirstRound(bool forceGoAlone)
        {
            BidControl.Reset();
            BidControl.GoingAlone.IsEnabled = false;
            BidControl.ForceGoAlone(forceGoAlone);
            BidControl.IsVisible = true;
            BidControl.IsEnabled = true;
            BidControl.InputTransparent = false;
        }

        public void ShowBidSecondRound(EuchreCard.Suits kittyCardSuit, EuchrePlayer.Seats dealer,
            bool stickTheDealer, bool forceGoAlone)
        {
            BidControl2.Reset();

            if (stickTheDealer)
            {
                BidControl2.Pass.IsChecked = false;
                BidControl2.Pass.IsEnabled = false;
                BidControl2.Pass.Opacity = 0.25;
                BidControl2.GoingAlone.IsEnabled = true;
                if (kittyCardSuit != EuchreCard.Suits.Hearts)
                    BidControl2.Hearts.IsChecked = true;
                else if (kittyCardSuit != EuchreCard.Suits.Diamonds)
                    BidControl2.Diamonds.IsChecked = true;
                else if (kittyCardSuit != EuchreCard.Suits.Clubs)
                    BidControl2.Clubs.IsChecked = true;
                else
                    BidControl2.Spades.IsChecked = true;
            }

            switch (kittyCardSuit)
            {
                case EuchreCard.Suits.Hearts:
                    BidControl2.Hearts.IsEnabled = false;
                    BidControl2.Hearts.Opacity = 0.25;
                    break;
                case EuchreCard.Suits.Diamonds:
                    BidControl2.Diamonds.IsEnabled = false;
                    BidControl2.Diamonds.Opacity = 0.25;
                    break;
                case EuchreCard.Suits.Clubs:
                    BidControl2.Clubs.IsEnabled = false;
                    BidControl2.Clubs.Opacity = 0.25;
                    break;
                case EuchreCard.Suits.Spades:
                    BidControl2.Spades.IsEnabled = false;
                    BidControl2.Spades.Opacity = 0.25;
                    break;
            }

            BidControl2.ForceGoAlone(forceGoAlone);
            BidControl2.IsVisible = true;
            BidControl2.IsEnabled = true;
            BidControl2.InputTransparent = false;
        }

        public void PostHumanBidFirstRound()
        {
            bool rv = BidControl.PickItUp.IsChecked;
            _handCurrentBidder.ProcessBidFirstRound(BidControl.GoingAlone.IsChecked, rv);
            if (rv)
            {
                UpdateEuchreState(EuchreState.Bid1PickUp);
            }
            else
            {
                UpdateEuchreState(_stateDesiredBidPass);
            }
        }

        public void PostHumanBidSecondRound()
        {
            bool calledIt = !BidControl2.Pass.IsChecked;
            if (calledIt)
            {
                if (BidControl2.Hearts.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Hearts;
                else if (BidControl2.Diamonds.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Diamonds;
                else if (BidControl2.Clubs.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Clubs;
                else if (BidControl2.Spades.IsChecked)
                    handTrumpSuit = EuchreCard.Suits.Spades;
            }
            _handCurrentBidder.ProcessBidSecondRound(BidControl2.GoingAlone.IsChecked, calledIt);
            if (calledIt)
            {
                UpdateEuchreState(EuchreState.Bid2Succeeded);
            }
            else
            {
                UpdateEuchreState(_stateDesiredBidPass);
            }
        }

        public void MarkCardAsPlayed(EuchreCard card)
        {
            handCardsPlayed[_trickPlayedCardIndex] = card;
            _trickPlayedCardIndex++;
        }

        public void ResetPlayedCards()
        {
            for (int i = 0; i <= 23; i++)
            {
                handCardsPlayed[i] = null!;
            }
            _trickPlayedCardIndex = 0;
        }

        #endregion

        #region "Private methods"

        private void OnCenterContainerSizeChanged(object sender, EventArgs e)
        {
            if (CenterContainer.Width <= 0 || CenterContainer.Height <= 0) return;

            const double designWidth = 855.0;
            const double designHeight = 735.0;

            double scaleX = CenterContainer.Width / designWidth;
            double scaleY = CenterContainer.Height / designHeight;
            double scale = Math.Min(scaleX, scaleY);

            EuchreGrid.Scale = scale;
        }

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

        private void SpeakWeGotEuchredMyFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayWeGotEuchredMyFault(gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].GetDisplayName());
        }

        private void SpeakWeGotEuchredOurFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotEuchredOurFault(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeGotEuchredYourFault(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotEuchredYourFault(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotOne(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotTwo(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeGotFour(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakMeGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayMeGotOne(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
        }

        private void SpeakMeGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayMeGotTwo(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
        }

        private void SpeakMeGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayMeGotFour(gamePlayers[(int)EuchrePlayer.Seats.Player].GetDisplayName());
        }

        private void SpeakWeWon(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeWon(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakTheyWon(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayTheyWon(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeEuchredThem(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeEuchredThem(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakWeSuperEuchredThem(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Player && _modeSoundOn)
                gamePlayers[(int)gamePlayers[(int)seat].OppositeSeat()].gameVoice.SayWeSuperEuchredThem(gamePlayers[(int)seat].GetDisplayName());
        }

        private void SpeakTheyGotOne(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayTheyGotOne();
        }

        private void SpeakTheyGotTwo(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayTheyGotTwo();
        }

        private void SpeakTheyGotFour(EuchrePlayer.Seats seat)
        {
            if (seat != EuchrePlayer.Seats.Partner && _modeSoundOn)
                gamePlayers[(int)seat].gameVoice.SayTheyGotFour();
        }

        private async Task PlayResourceSound(string resourceName)
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(resourceName);
                var player = AudioManager.Current.CreatePlayer(stream);
                player.Play();
                // Wait for playback to complete so sounds don't overlap
                while (player.IsPlaying)
                    await Task.Delay(50);
                player.Dispose();
            }
            catch
            {
                // If audio fails, silently continue
            }
        }

        private async Task PlayCardSound()
        {
            if (_modeSoundOn)
                await PlayResourceSound("playcard.wav");
        }

        private async Task PlayApplause(int level)
        {
            if (_modeSoundOn)
            {
                switch (level)
                {
                case 1: await PlayResourceSound("softapplause.wav"); break;
                case 2: await PlayResourceSound("loudapplause.wav"); break;
                case 3: await PlayResourceSound("wildapplause.wav"); break;
                }
            }
        }

        private async Task PlayShuffleSound()
        {
            UpdateStatusSeparator();
            UpdateStatus(AppResources.GetString("Notice_ShufflingCards"));
            if (_modeSoundOn)
            {
                int numShuffle = Random.Shared.Next(2) + 1;
                for (int i = 0; i <= numShuffle; i++)
                {
                    await PlayResourceSound("shuffle.wav");
                }
            }
            UpdateStatus(AppResources.GetString("Notice_DealingCards"));
        }

        private void UpdateScoreText()
        {
            StringBuilder sTheirScore = new StringBuilder();
            sTheirScore.AppendFormat(AppResources.GetString("Format_TheirScore"), _gameTheirScore);
            TheirScore.Text = sTheirScore.ToString();

            StringBuilder sYourScore = new StringBuilder();
            sYourScore.AppendFormat(AppResources.GetString("Format_YourScore"), _gameYourScore);
            YourScore.Text = sYourScore.ToString();

            if (_gameTheirScore > 10) _gameTheirScore = 10;
            if (_gameYourScore > 10) _gameYourScore = 10;

            SetImage(ThemScore, GetScoreResourceName(ScorePrefix.ScoreThem, _gameTheirScore));
            SetImage(UsScore, GetScoreResourceName(ScorePrefix.ScoreUs, _gameYourScore));
        }

        private void UpdateTricksText()
        {
            StringBuilder sTheirTricks = new StringBuilder();
            sTheirTricks.AppendFormat(AppResources.GetString("Format_TheirTricks"), _gameTheirTricks);
            TheirTricks.Text = sTheirTricks.ToString();

            StringBuilder sYourTricks = new StringBuilder();
            sYourTricks.AppendFormat(AppResources.GetString("Format_YourTricks"), _gameYourTricks);
            YourTricks.Text = sYourTricks.ToString();
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

        private async Task UpdateAllScores(EuchreState nextState)
        {
            int TheirTotalTricks = gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].handTricksWon;
            int YourTotalTricks = gamePlayers[(int)EuchrePlayer.Seats.Player].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.Partner].handTricksWon;
            switch (handPickedTrump)
            {
            case EuchrePlayer.Seats.LeftOpponent:
            case EuchrePlayer.Seats.RightOpponent:
                if (TheirTotalTricks == 0 && ruleUseSuperEuchre)
                {
                    _gameYourScore += 4;
                    UpdateStatusBold(AppResources.GetString("Notice_YouSuperEuchredThem"));
                    await PlayApplause(3);
                    SpeakWeSuperEuchredThem(EuchrePlayer.Seats.Player);
                }
                else if (TheirTotalTricks < 3)
                {
                    _gameYourScore += 2;
                    UpdateStatusBold(AppResources.GetString("Notice_YouEuchredThem"));
                    await PlayApplause(2);
                    SpeakWeEuchredThem(EuchrePlayer.Seats.Player);
                }
                else if (TheirTotalTricks == 5)
                {
                    if (TheirTeamWentAloneThisHand())
                    {
                        _gameTheirScore += 4;
                        UpdateStatusBold(AppResources.GetString("Notice_TheyWonTheHandAllTricksAlone"));
                        SpeakTheyGotFour(EuchrePlayer.Seats.Partner);
                    }
                    else
                    {
                        _gameTheirScore += 2;
                        UpdateStatusBold(AppResources.GetString("Notice_TheyWonTheHandAllTricks"));
                        SpeakTheyGotTwo(EuchrePlayer.Seats.Partner);
                    }
                }
                else
                {
                    _gameTheirScore += 1;
                    UpdateStatusBold(AppResources.GetString("Notice_TheyWonTheHand"));
                    SpeakTheyGotOne(EuchrePlayer.Seats.Partner);
                }
                break;

            case EuchrePlayer.Seats.Player:
            case EuchrePlayer.Seats.Partner:
                if (YourTotalTricks == 0 && ruleUseSuperEuchre)
                {
                    _gameTheirScore += 4;
                    UpdateStatusBold(AppResources.GetString("Notice_TheySuperEuchredYou"));
                    if (handPickedTrump == EuchrePlayer.Seats.Partner)
                        SpeakWeGotEuchredMyFault(handPickedTrump);
                    else if (!YourTeamWentAloneThisHand())
                        SpeakWeGotEuchredOurFault(handPickedTrump);
                    else
                        SpeakWeGotEuchredYourFault(handPickedTrump);
                }
                else if (YourTotalTricks < 3)
                {
                    _gameTheirScore += 2;
                    UpdateStatusBold(AppResources.GetString("Notice_TheyEuchredYou"));
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
                        _gameYourScore += 4;
                        UpdateStatusBold(AppResources.GetString("Notice_YouWonTheHandAllTricksAlone"));
                        await PlayApplause(3);
                        if (handPickedTrump == EuchrePlayer.Seats.Player)
                            SpeakWeGotFour(handPickedTrump);
                        else
                            SpeakMeGotFour(handPickedTrump);
                    }
                    else
                    {
                        _gameYourScore += 2;
                        UpdateStatusBold(AppResources.GetString("Notice_YouWonTheHandAllTricks"));
                        await PlayApplause(2);
                        if (handPickedTrump == EuchrePlayer.Seats.Player)
                            SpeakWeGotTwo(handPickedTrump);
                        else
                            SpeakMeGotTwo(handPickedTrump);
                    }
                }
                else
                {
                    _gameYourScore += 1;
                    UpdateStatusBold(AppResources.GetString("Notice_YouWonTheHand"));
                    await PlayApplause(1);
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

        private void HideAllPlayedCards()
        {
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                gameTableTopCards[(int)i, 5].Source = null;
                gameTableTopCards[(int)i, 5].IsVisible = false;
                handPlayedCards[(int)i] = null!;
            }
        }

        private void ShowAllCards(bool ShowThem)
        {
            bool visibleNoH = ShowThem && !ruleUseNineOfHearts;
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    gameTableTopCards[(int)i, j].IsVisible = ShowThem;
                }
            }
            KittyCard1.IsVisible = ShowThem;
            KittyCard2.IsVisible = visibleNoH;
            KittyCard3.IsVisible = visibleNoH;
            KittyCard4.IsVisible = visibleNoH;
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

        private void ResetUserInputStates()
        {
            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                for (int j = 0; j <= 4; j++)
                {
                    gameTableTopCards[(int)i, j].IsEnabled = true;
                    gameTableTopCards[(int)i, j].Opacity = 1.0;
                }
            }
            ContinueButton.IsVisible = false;
            ContinueButton.IsEnabled = false;

            BidControl.IsVisible = false;
            BidControl.IsEnabled = false;
            BidControl.InputTransparent = true;
            BidControl2.IsVisible = false;
            BidControl2.IsEnabled = false;
            BidControl2.InputTransparent = true;

            statePlayerIsDroppingACard = false;
            statePlayerIsPlayingACard = false;

            SelectLabel.IsVisible = false;
        }

        private void SetAllPlayerCardImages()
        {
            for (int i = 0; i <= 4; i++)
            {
                SetCardImage(
                    gamePlayers[(int)EuchrePlayer.Seats.Player].handCardsHeld[i],
                    EuchrePlayer.Seats.Player,
                    gameTableTopCards[(int)EuchrePlayer.Seats.Player, i]);
            }
        }

        private void SetCardImage(EuchreCard card, EuchrePlayer.Seats perspective, Image img)
        {
            card.Perspective = perspective;
            string source = card.GetImageSource(perspective);
            SetImage(img, source);
            img.Rotation = card.rotationAngle;
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
                        i,
                        gameTableTopCards[(int)i, j]);
                }
            }

            SetCardImage(handKitty[0], EuchrePlayer.Seats.Player, KittyCard1);

            if (!ruleUseNineOfHearts)
            {
                SetCardImage(handKitty[1], EuchrePlayer.Seats.Player, KittyCard2);
                SetCardImage(handKitty[2], EuchrePlayer.Seats.Player, KittyCard3);
                SetCardImage(handKitty[3], EuchrePlayer.Seats.Player, KittyCard4);
            }
        }

        private async Task DealACard(EuchrePlayer.Seats player, int slot)
        {
            gamePlayers[(int)player].handCardsHeld[slot] = _gameDeck.GetNextCard();
            if (player != EuchrePlayer.Seats.Player)
                gamePlayers[(int)player].handCardsHeld[slot].stateCurrent = EuchreCard.States.FaceDown;
            else
                gamePlayers[(int)player].handCardsHeld[slot].stateCurrent = EuchreCard.States.FaceUp;

            int angle = EuchreCard.GetSeatRotationAngle(player);
            SetImage(gameTableTopCards[(int)player, slot], EuchreCard.CardBackImageName);
            gameTableTopCards[(int)player, slot].Rotation = angle;
            gameTableTopCards[(int)player, slot].IsVisible = true;
            await PlayCardSound();
            await Task.Delay(_timerSleepDuration);
        }

        private async Task DealCards()
        {
            ShowAllCards(false);
            HideAllPlayedCards();

            _gameDeck.Shuffle();
            await PlayShuffleSound();

            EuchrePlayer.Seats i;

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
                    await DealACard(i, j);
                if (i == handDealer)
                {
                    k++;
                    m = 2;
                }
            }

            for (int j = 0; j <= 3; j++)
            {
                handKitty[j] = _gameDeck.GetNextCard();
                if (j == 0)
                    handKitty[j].stateCurrent = EuchreCard.States.FaceUp;
                else if (handKitty[j] != null)
                    handKitty[j].stateCurrent = EuchreCard.States.FaceDown;
            }

            for (EuchrePlayer.Seats seat = EuchrePlayer.Seats.LeftOpponent; seat <= EuchrePlayer.Seats.Player; seat++)
            {
                gamePlayers[(int)seat].SortCards(EuchreCard.Suits.NoSuit);
            }

            SetAllCardImages();
            ShowAllCards(true);

            StringBuilder sKitty = new StringBuilder();
            sKitty.AppendFormat(AppResources.GetString("Notice_KittyCard"), AppResources.ResourceManager.GetString(handKitty[0].GetDisplayStringResourceName()));
            UpdateStatus(sKitty.ToString());
        }

        private void UpdateAllTricks()
        {
            _gameTheirTricks = gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].handTricksWon;
            _gameYourTricks = gamePlayers[(int)EuchrePlayer.Seats.Player].handTricksWon + gamePlayers[(int)EuchrePlayer.Seats.Partner].handTricksWon;
            UpdateTricksText();
        }

        private async Task PlaySelectedCard(EuchrePlayer player, int index)
        {
            if (index > 4) throw new Exception("Invalid index");

            player.handCardsHeld[index].stateCurrent = EuchreCard.States.FaceUp;
            SetCardImage(player.handCardsHeld[index], player.Seat, gameTableTopCards[(int)player.Seat, 5]);

            string? s = AppResources.ResourceManager.GetString(player.handCardsHeld[index].GetDisplayStringResourceName(handTrumpSuit));
            if (string.IsNullOrEmpty(s)) throw new Exception("Invalid value");

            UpdateStatusBoldName(AppResources.GetString("Notice_PlayedACard"), player.GetDisplayName(), s);

            handPlayedCards[(int)player.Seat] = player.handCardsHeld[index];

            MarkCardAsPlayed(player.handCardsHeld[index]);
            player.handCardsHeld[index] = null!;
            gameTableTopCards[(int)player.Seat, index].Source = null;
            gameTableTopCards[(int)player.Seat, index].IsVisible = false;
            gameTableTopCards[(int)player.Seat, 5].IsVisible = true;

            await PlayCardSound();
            await Task.Delay(_timerSleepDuration);
        }

        private async Task SwapCardWithKitty(EuchrePlayer player, int index)
        {
            await Task.CompletedTask;

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

            player.trickBuriedCard = handKitty[0];
        }

        private void PrepTrick()
        {
            UpdateStatusSeparator();
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
                    HumanPlayACard();
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
            SelectLabel.Text = AppResources.GetString("Notice_PlayACard");
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

            if (!AnyValid)
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

            SelectLabel.IsVisible = true;
            statePlayerIsPlayingACard = true;
        }

        private async Task PlayCardForTrick()
        {
            if (!trickPlayer.handSittingOut)
            {
                if (trickPlayer.Seat == EuchrePlayer.Seats.Player)
                {
                    SelectLabel.IsVisible = false;
                    for (int i = 0; i <= 4; i++)
                    {
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].IsEnabled = true;
                        gameTableTopCards[(int)EuchrePlayer.Seats.Player, i].Opacity = 1.0;
                    }
                }

                if (trickLeaderIndex == trickPlayer.Seat)
                {
                    trickSuitLed = trickPlayer.handCardsHeld[trickSelectedCardIndex].GetCurrentSuit(handTrumpSuit);
                }
                await PlaySelectedCard(trickPlayer, trickSelectedCardIndex);

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
            gamePlayers[(int)trickPlayerWhoPlayedHighestCardSoFar].handTricksWon++;
            switch (trickPlayerWhoPlayedHighestCardSoFar)
            {
            case EuchrePlayer.Seats.LeftOpponent:
            case EuchrePlayer.Seats.RightOpponent:
                UpdateStatusBold(AppResources.GetString("Notice_TheirTeamWonTrick"));
                break;
            case EuchrePlayer.Seats.Player:
            case EuchrePlayer.Seats.Partner:
                UpdateStatusBold(AppResources.GetString("Notice_YourTeamWonTrick"));
                break;
            }

            UpdateAllTricks();
            trickLeaderIndex = trickPlayerWhoPlayedHighestCardSoFar;
        }

        private void ClearAllTricks()
        {
            _gameTheirTricks = 0;
            _gameYourTricks = 0;

            for (EuchrePlayer.Seats i = EuchrePlayer.Seats.LeftOpponent; i <= EuchrePlayer.Seats.Player; i++)
            {
                gamePlayers[(int)i].ClearAllTricks();
            }

            ResetPlayedCards();
            UpdateTricksText();
        }

        private void PreBid1()
        {
            UpdateStatusSeparator();
            _handCurrentBidder = gamePlayers[(int)handDealer];
        }

        private async Task Bid1(EuchreState passedState)
        {
            _handCurrentBidder = gamePlayers[(int)EuchrePlayer.NextPlayer(_handCurrentBidder.Seat)];
            _handCurrentBidder.trickBuriedCard = null!;

            bool GoingAlone = false;
            if (_handCurrentBidder.Seat == EuchrePlayer.Seats.Player)
            {
                _handCurrentBidder.HumanBidFirstRound();
                _stateDesiredBidPass = passedState;
            }
            else
            {
                bool rv = _handCurrentBidder.AutoBidFirstRound(GoingAlone);
                _handCurrentBidder.ProcessBidFirstRound(GoingAlone, rv);
                await Task.Delay(_timerBidSpeechDuration);
                if (rv)
                    UpdateEuchreState(EuchreState.Bid1PickUp);
                else
                    UpdateEuchreState(passedState);
            }
        }

        private async Task Bid1PickUp()
        {
            if (gamePlayers[(int)handDealer].Seat == EuchrePlayer.Seats.Player)
            {
                HumanReplaceACard();
            }
            else
            {
                int index = gamePlayers[(int)handDealer].LowestCardOnReplace(handTrumpSuit);
                await SwapCardWithKitty(gamePlayers[(int)handDealer], index);
                UpdateEuchreState(EuchreState.Bid1PickedUp);
            }
        }

        private void HumanReplaceACard()
        {
            SelectLabel.Text = AppResources.GetString("Notice_SwapACard");
            SelectLabel.IsVisible = true;
            statePlayerIsDroppingACard = true;
            _stateDesiredStateAfterHumanClick = EuchreState.Bid1PickedUp;
        }

        private async Task Bid1PickedUp()
        {
            if (gamePlayers[(int)handDealer].Seat == EuchrePlayer.Seats.Player)
            {
                SelectLabel.IsVisible = false;
                await SwapCardWithKitty(gamePlayers[(int)handDealer], trickSelectedCardIndex);
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
            SetImage(KittyCard1, handKitty[0].GetImageSource(EuchrePlayer.Seats.NoPlayer));
            MarkCardAsPlayed(handKitty[0]);

            for (EuchrePlayer.Seats seat = EuchrePlayer.Seats.LeftOpponent; seat <= EuchrePlayer.Seats.Player; seat++)
            {
                gamePlayers[(int)seat].SortCards(EuchreCard.Suits.NoSuit);
            }
            SetAllCardImages();
        }

        private async Task Bid2(EuchreState passedState)
        {
            _handCurrentBidder = gamePlayers[(int)EuchrePlayer.NextPlayer(_handCurrentBidder.Seat)];

            bool GoingAlone = false;
            if (_handCurrentBidder.Seat == EuchrePlayer.Seats.Player)
            {
                _handCurrentBidder.HumanBidSecondRound();
                _stateDesiredBidPass = passedState;
            }
            else
            {
                bool rv = _handCurrentBidder.AutoBidSecondRound(GoingAlone);
                _handCurrentBidder.ProcessBidSecondRound(GoingAlone, rv);
                await Task.Delay(_timerBidSpeechDuration);
                if (rv)
                    UpdateEuchreState(EuchreState.Bid2Succeeded);
                else
                    UpdateEuchreState(passedState);
            }
        }

        private void SetForNewHand()
        {
            ClearAllTricks();
            TrumpPartner.IsVisible = false;
            TrumpPlayer.IsVisible = false;
            TrumpLeft.IsVisible = false;
            TrumpRight.IsVisible = false;
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
                SetImage(TrumpPartner, EuchreCard.suitImageNames[(int)handTrumpSuit]);
                TrumpPartner.IsVisible = true;
                break;
            case EuchrePlayer.Seats.Player:
                SetImage(TrumpPlayer, EuchreCard.suitImageNames[(int)handTrumpSuit]);
                TrumpPlayer.IsVisible = true;
                break;
            case EuchrePlayer.Seats.LeftOpponent:
                SetImage(TrumpLeft, EuchreCard.suitImageNames[(int)handTrumpSuit]);
                TrumpLeft.IsVisible = true;
                break;
            case EuchrePlayer.Seats.RightOpponent:
                SetImage(TrumpRight, EuchreCard.suitImageNames[(int)handTrumpSuit]);
                TrumpRight.IsVisible = true;
                break;
            }
        }

        private void DetermineWinnerAndEndGame()
        {
            if (_gameTheirScore > _gameYourScore)
            {
                UpdateStatusBold(AppResources.GetString("Notice_TheyWonTheGame"), 2);
                SpeakTheyWon(EuchrePlayer.Seats.Player);
            }
            else
            {
                UpdateStatusBold(AppResources.GetString("Notice_YouWonTheGame"), 2);
                SpeakWeWon(EuchrePlayer.Seats.Player);
            }
            _stateGameStarted = false;
        }

        private void CleanupAfterHand()
        {
            if (gamePlayers[(int)gamePlayers[(int)handPickedTrump].OppositeSeat()].handSittingOut)
            {
                gamePlayers[(int)gamePlayers[(int)handPickedTrump].OppositeSeat()].handSittingOut = false;
                EnableCards(gamePlayers[(int)handPickedTrump].OppositeSeat(), true);
            }
        }

        private void SetNextDealer()
        {
            _gameDealerBox[(int)handDealer].IsVisible = false;
            handDealer = EuchrePlayer.NextPlayer(handDealer);
            _gameDealerBox[(int)handDealer].IsVisible = true;
        }

        private async Task<EuchreCard> DealACardForDeal(EuchrePlayer.Seats player, int slot)
        {
            EuchreCard card = _gameDeck.GetNextCard();
            card.Perspective = player;
            card.stateCurrent = EuchreCard.States.FaceUp;

            SetCardImage(card, player, gameTableTopCards[(int)player, slot]);
            gameTableTopCards[(int)player, slot].IsVisible = true;

            UpdateStatusBoldName(AppResources.GetString("Notice_DealtACard"), gamePlayers[(int)player].GetDisplayName(), AppResources.ResourceManager.GetString(card.GetDisplayStringResourceName())!);

            await PlayCardSound();
            await Task.Delay(_timerSleepDuration);
            return card;
        }

        private async Task PreDealerSelection()
        {
            stateSelectingDealer = true;
            _gameDeck.Shuffle();
            await PlayShuffleSound();
            UpdateStatusSeparator();
            UpdateStatus(AppResources.GetString("Notice_ChoosingDealer"));
            _handPotentialDealer = EuchrePlayer.Seats.Player;
            potentialDealerCardIndex = 0;
        }

        private async Task TrySelectDealer()
        {
            EuchreCard card = await DealACardForDeal(_handPotentialDealer, potentialDealerCardIndex);
            if (card.Rank == EuchreCard.Ranks.Jack)
            {
                UpdateEuchreState(EuchreState.DealerSelected);
            }
            else
            {
                _handPotentialDealer = EuchrePlayer.NextPlayer(_handPotentialDealer);
                if (_handPotentialDealer == EuchrePlayer.Seats.Player)
                {
                    potentialDealerCardIndex++;
                }
                UpdateEuchreState(EuchreState.StillSelectingDealer);
            }
        }

        private void PostDealerSelection(EuchreState nextState)
        {
            UpdateStatusBoldName(AppResources.GetString("Notice_IAmTheDealer"), gamePlayers[(int)_handPotentialDealer].GetDisplayName());
            ShowAndEnableContinueButton(nextState);
        }

        private void PostDealerCleanup()
        {
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    gameTableTopCards[i, j].IsVisible = false;
                    gameTableTopCards[i, j].Source = null;
                }
            }

            stateSelectingDealer = false;
            handDealer = _handPotentialDealer;
            _gameDealerBox[(int)handDealer].IsVisible = true;
        }

        private void ShowAndEnableContinueButton(EuchreState nextState)
        {
            ContinueButton.IsVisible = true;
            ContinueButton.IsEnabled = true;
            _stateDesiredStateAfterHumanClick = nextState;
        }

        private async Task<bool> NewGame()
        {
            ClearStatus();
            UpdateStatus(AppResources.GetString("Notice_StartingNewGame"));
            ResetScores();
            ResetUserInputStates();
            ShowAllCards(false);
            HideAllPlayedCards();
            ShowAllNameLabels(false);
            HideAllDealerAndTrumpLabels();

            // Launch options dialog modally and wait for result
            var tcs = new TaskCompletionSource<bool>();
            var optionsPage = new EuchreOptions();
            optionsPage.Disappearing += (s, e) =>
            {
                tcs.TrySetResult(optionsPage.LocalDialogResult);
            };
            await Navigation.PushModalAsync(optionsPage);
            bool result = await tcs.Task;

            if (result)
            {
                ruleStickTheDealer = GameSettings.StickTheDealer;
                ruleUseNineOfHearts = GameSettings.NineOfHearts;
                modePeekAtOtherCards = GameSettings.PeekAtOtherCards;
                ruleUseSuperEuchre = GameSettings.SuperEuchre;
                ruleUseQuietDealer = GameSettings.QuietDealer;
                _modeSoundOn = GameSettings.SoundOn;

                gamePlayerName = string.IsNullOrEmpty(GameSettings.PlayerName) ? AppResources.GetString("Player_Player") : GameSettings.PlayerName;
                gameLeftOpponentName = string.IsNullOrEmpty(GameSettings.LeftOpponentName) ? AppResources.GetString("Player_LeftOpponent") : GameSettings.LeftOpponentName;
                gameRightOpponentName = string.IsNullOrEmpty(GameSettings.RightOpponentName) ? AppResources.GetString("Player_RightOpponent") : GameSettings.RightOpponentName;

                gamePartnerName = GameSettings.PartnerName;
                if (string.IsNullOrEmpty(gamePartnerName))
                {
                    StringBuilder s = new StringBuilder();
                    s.AppendFormat(AppResources.GetString("Player_Partner"), gamePlayerName);
                    gamePartnerName = s.ToString();
                }

                if (GameSettings.LeftOpponentPlay == 1)
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if (GameSettings.LeftOpponentPlay == 2)
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.LeftOpponent].gamePersonality = EuchrePlayer.Personalities.Conservative;

                if (GameSettings.RightOpponentPlay == 1)
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if (GameSettings.RightOpponentPlay == 2)
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.RightOpponent].gamePersonality = EuchrePlayer.Personalities.Conservative;

                if (GameSettings.PartnerPlay == 1)
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Crazy;
                else if (GameSettings.PartnerPlay == 2)
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Normal;
                else
                    gamePlayers[(int)EuchrePlayer.Seats.Partner].gamePersonality = EuchrePlayer.Personalities.Conservative;

                PlayerNameLabel.Text = gamePlayerName;
                PartnerNameLabel.Text = gamePartnerName;
                LeftOpponentNameLabel.Text = gameLeftOpponentName;
                RightOpponentNameLabel.Text = gameRightOpponentName;
                ShowAllNameLabels(true);

                _gameDeck = new EuchreCardDeck(ruleUseNineOfHearts, this);
                _gameDeck.Initialize();

                _stateGameStarted = true;
                return true;
            }
            else
            {
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
            PlayerNameLabel.IsVisible = ShowAll;
            PartnerNameLabel.IsVisible = ShowAll;
            LeftOpponentNameLabel.IsVisible = ShowAll;
            RightOpponentNameLabel.IsVisible = ShowAll;
        }

        private void HideAllDealerAndTrumpLabels()
        {
            DealerLeftOpponent.IsVisible = false;
            DealerRightOpponent.IsVisible = false;
            DealerPartner.IsVisible = false;
            DealerPlayer.IsVisible = false;

            TrumpLeft.IsVisible = false;
            TrumpRight.IsVisible = false;
            TrumpPlayer.IsVisible = false;
            TrumpPartner.IsVisible = false;
        }
        #endregion

        #region "Event handlers"

        private void PlayerCard_Click(object? sender, TappedEventArgs e)
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

                UpdateEuchreState(_stateDesiredStateAfterHumanClick);
            }
        }

        private void ContinueButton_Click(object? sender, EventArgs e)
        {
            ContinueButton.IsVisible = false;
            ContinueButton.IsEnabled = false;
            UpdateEuchreState(_stateDesiredStateAfterHumanClick);
        }

        private void NewGameButton_Click(object? sender, EventArgs e)
        {
            UpdateEuchreState(EuchreState.StartNewGameRequested);
        }

        private async void AboutButton_Click(object? sender, EventArgs e)
        {
            await DisplayAlert("Matt's Euchre", AppResources.GetString("About_Text"), "OK");
        }

        private async void RulesButton_Click(object? sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EuchreRules());
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

        private EuchreCardDeck _gameDeck = null!;

        private Border[] _gameDealerBox = new Border[4];

        private int _trickPlayedCardIndex;

        private EuchreState _stateDesiredBidPass;
        private EuchreState _stateDesiredStateAfterHumanClick;
        private EuchreState _stateLast;
        private EuchreState _stateCurrent;
        private EuchrePlayer _handCurrentBidder = null!;
        private EuchrePlayer.Seats _handPotentialDealer;
        private int potentialDealerCardIndex;

        private const int _timerSleepDuration = 250;
        private const int _timerBidSpeechDuration = 1000;

        #endregion

        #region "Public variables"
        public bool ruleStickTheDealer { get; set; }
        public bool ruleUseNineOfHearts;
        public bool ruleUseSuperEuchre;
        public bool ruleUseQuietDealer { get; set; }

        public bool modePeekAtOtherCards { get; set; }

        public bool stateSelectingDealer { get; set; }
        public bool statePlayerIsDroppingACard;
        public bool statePlayerIsPlayingACard;

        public EuchrePlayer[] gamePlayers { get; } = new EuchrePlayer[4];
        public string gamePlayerName { get; set; } = "";
        public string gamePartnerName { get; set; } = "";
        public string gameLeftOpponentName { get; set; } = "";
        public string gameRightOpponentName { get; set; } = "";
        public Image[,] gameTableTopCards = new Image[4, 6];

        public EuchreCard.Suits handTrumpSuit { get; set; }
        public EuchrePlayer.Seats handPickedTrump { get; set; } = EuchrePlayer.Seats.NoPlayer;
        public EuchrePlayer.Seats handDealer { get; set; }
        public EuchreCard[] handCardsPlayed = new EuchreCard[24];
        public EuchreCard[] handPlayedCards { get; } = new EuchreCard[4];
        public EuchreCard[] handKitty { get; } = new EuchreCard[4];

        public EuchrePlayer.Seats trickLeaderIndex { get; set; }
        public EuchreCard.Suits trickSuitLed { get; set; }
        public int trickSelectedCardIndex;
        public EuchrePlayer trickLeader = null!;
        public EuchrePlayer trickPlayer = null!;
        public EuchreCard.Values trickHighestCardSoFar;
        public EuchrePlayer.Seats trickPlayerWhoPlayedHighestCardSoFar;
        #endregion
    }
}
