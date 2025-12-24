using System;
using System.Text;

namespace CSEuchre4
{
    public class EuchrePlayer
    {
        #region "Enums"
        public enum Personalities : int
        {
            Crazy = 0,
            Normal = 1,
            Conservative = 2
        }

        public enum Seats : int
        {
            LeftOpponent = 0,
            Partner = 1,
            RightOpponent = 2,
            Player = 3,
            NoPlayer = -1
        }
        #endregion

        #region "Private Methods"
        private int HandValue(EuchreCard.Suits TrumpSuit)
        {
            int hv = 0;
            for (int i = 0; i <= 4; i++)
            {
                hv += (int)handCardsHeld[i].GetValue(TrumpSuit);
            }
            return hv;
        }

        private int HighestCard()
        {
            int index = -1;
            int value = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue > value)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int HighestCardNotTrump()
        {
            int index = -1;
            int value = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue > value && localvalue < (int)EuchreCard.Values.NineTrump)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int LowestCard()
        {
            int index = -1;
            int value = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue < value)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int LowestCardTrump()
        {
            int index = -1;
            int value = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue < value && localvalue > (int)EuchreCard.Values.AceNoTrump)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int HighestCardLedSuit()
        {
            int index = -1;
            int value = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null) && CardBelongsToLedSuit(handCardsHeld[i]))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue > value)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int LowestCardThatTakesLedSuit(EuchreTable Table, EuchreCard.Values ValueToBeat)
        {
            int index = -1;
            int DifferenceToMinimize = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null) && CardBelongsToLedSuit(handCardsHeld[i]))
                {
                    int localdifference = (int)handCardsHeld[i].GetValue(Table.handTrumpSuit) - (int)ValueToBeat;
                    if (localdifference > 0 && localdifference < DifferenceToMinimize)
                    {
                        index = i;
                        DifferenceToMinimize = localdifference;
                    }
                }
            }
            return index;
        }

        private int LowestCardTrumpThatTakes(EuchreTable Table, EuchreCard.Values ValueToBeat)
        {
            int index = -1;
            int DifferenceToMinimize = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null))
                {
                    int localdifference = (int)handCardsHeld[i].GetValue(Table.handTrumpSuit) - (int)ValueToBeat;
                    if (localdifference > 0 && localdifference < DifferenceToMinimize && handCardsHeld[i].GetValue(Table.handTrumpSuit) > EuchreCard.Values.AceNoTrump)
                    {
                        index = i;
                        DifferenceToMinimize = localdifference;
                    }
                }
            }
            return index;
        }

        private int LowestCardLedSuit()
        {
            int index = -1;
            int value = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                if (!(handCardsHeld[i] == null) && CardBelongsToLedSuit(handCardsHeld[i]))
                {
                    int localvalue = (int)handCardsHeld[i].GetValue(_gameTable.handTrumpSuit);
                    if (localvalue < value)
                    {
                        index = i;
                        value = localvalue;
                    }
                }
            }
            return index;
        }

        private int AutoLeadACard()
        {
            int index = -1;
            if (Seat == _gameTable.handPickedTrump || OppositeSeat() == _gameTable.handPickedTrump)
            {
                // Start off strong, and lead your highest value card:
                index = HighestCard();
            }
            else
            {
                // Lead a high card that isn't trump
                index = HighestCardNotTrump();
                if (index == -1)
                {
                    index = HighestCard();
                }
            }
            return index;
        }

        private int AutoPlayDefendCard()
        {
            EuchreCard.Values CurrentHighestValue = _gameTable.handPlayedCards[(int)_gameTable.trickLeaderIndex].GetValue(_gameTable.handTrumpSuit);
            int index = HighestCardLedSuit();
            if (index == -1)
            {
                // Don't have that suit -- try to trump it
                index = LowestCardTrump();
                if (index == -1)
                {
                    // Don't have trump -- throw junk
                    index = LowestCard();
                }
            }
            else if (handCardsHeld[index].GetValue(_gameTable.handTrumpSuit) < CurrentHighestValue)
            {
                // Can't beat it -- throw lowest possible
                index = LowestCardLedSuit();
            }
            return index;
        }

        private int AutoPlaySupportCard()
        {
            EuchreCard.Values CurrentLeaderValue = _gameTable.handPlayedCards[(int)_gameTable.trickLeaderIndex].GetValue(_gameTable.handTrumpSuit);
            EuchreCard.Values CurrentDefenderValue = EuchreCard.Values.NoValue;
            if (!_gameTable.gamePlayers[(int)NextPlayer(_gameTable.trickLeaderIndex)].handSittingOut)
            {
                CurrentDefenderValue = _gameTable.handPlayedCards[(int)NextPlayer(_gameTable.trickLeaderIndex)].GetCurrentValue(_gameTable.handTrumpSuit, _gameTable.trickSuitLed);
            }

            bool Winning = (CurrentDefenderValue <= CurrentLeaderValue);
            int index = -1;
            if (!Winning)
            {
                index = HighestCardLedSuit();
                if (index == -1)
                {
                    // Don't have that suit -- try to trump it
                    index = LowestCardTrumpThatTakes(_gameTable, CurrentDefenderValue);
                    if (index == -1)
                    {
                        // Don't have trump-- throw junk
                        index = LowestCard();
                    }
                }
                else if (handCardsHeld[index].GetValue(_gameTable.handTrumpSuit) < CurrentDefenderValue)
                {
                    // Can't beat it-- throw lowest possible
                    index = LowestCardLedSuit();
                }
            }
            else
            {
                // Don't overplay my partner
                index = LowestCardLedSuit();
                if (index == -1)
                {
                    // Don't have that suit, just throw junk
                    index = LowestCard();
                }
            }
            return index;
        }

        private int AutoPlayLastDefendCard()
        {
            EuchreCard.Values CurrentLeaderValue = _gameTable.handPlayedCards[(int)_gameTable.trickLeaderIndex].GetValue(_gameTable.handTrumpSuit);
            EuchreCard.Values CurrentDefenderValue = EuchreCard.Values.NoValue;
            if (!_gameTable.gamePlayers[(int)NextPlayer(_gameTable.trickLeaderIndex)].handSittingOut)
            {
                CurrentDefenderValue = _gameTable.handPlayedCards[(int)NextPlayer(_gameTable.trickLeaderIndex)].GetCurrentValue(_gameTable.handTrumpSuit, _gameTable.trickSuitLed);
            }
            EuchreCard.Values CurrentSupporterValue = EuchreCard.Values.NoValue;
            if (!_gameTable.gamePlayers[(int)NextPlayer(NextPlayer(_gameTable.trickLeaderIndex))].handSittingOut)
            {
                CurrentSupporterValue = _gameTable.handPlayedCards[(int)NextPlayer(NextPlayer(_gameTable.trickLeaderIndex))].GetCurrentValue(_gameTable.handTrumpSuit, _gameTable.trickSuitLed);
            }

            bool Winning = (CurrentDefenderValue > CurrentLeaderValue) && (CurrentDefenderValue > CurrentSupporterValue);
            int index;
            if (!Winning)
            {
                EuchreCard.Values ValueToBeat = CurrentLeaderValue;
                if (CurrentSupporterValue > CurrentLeaderValue)
                {
                    ValueToBeat = CurrentSupporterValue;
                }
                index = LowestCardThatTakesLedSuit(_gameTable, ValueToBeat);
                if (index == -1)
                {
                    index = LowestCardLedSuit();
                    if (index == -1)
                    {
                        index = LowestCardTrumpThatTakes(_gameTable, ValueToBeat);
                        if (index == -1)
                        {
                            index = LowestCard();
                        }
                    }
                }
            }
            else
            {
                // Don't overplay, you've already won
                index = LowestCardLedSuit();
                if (index == -1)
                {
                    // Throw junk -- you've already won.
                    index = LowestCard();
                }
            }
            return index;
        }

        private int Makeable()
        {
            switch (gamePersonality)
            {
            case Personalities.Crazy: return EuchreCard.goalMakeable - 15;
            case Personalities.Normal: return EuchreCard.goalMakeable;
            case Personalities.Conservative: return EuchreCard.goalMakeable + 15;
            default: throw new Exception("Unknown personality type in Makeable");
            }
        }

        private int Loner()
        {
            switch (gamePersonality)
            {
            case Personalities.Crazy: return EuchreCard.goalLoner - 15;
            case Personalities.Normal: return EuchreCard.goalLoner;
            case Personalities.Conservative: return EuchreCard.goalLoner + 15;
            default: throw new Exception("Unknown personality type in Loner");
            }
        }

        #endregion

        #region "Static methods"
        public static EuchrePlayer.Seats NextPlayer(EuchrePlayer.Seats CurrentPlayer)
        {
            return (CurrentPlayer == Seats.Player ? Seats.LeftOpponent : CurrentPlayer + 1);
        }

        #endregion

        #region "Public methods"
        public int AutoPlayACard()
        {
            int index = -1;
            if (Seat == _gameTable.trickLeaderIndex)
                index = AutoLeadACard();
            else if (Seat == NextPlayer(_gameTable.trickLeaderIndex))
                index = AutoPlayDefendCard();
            else if (Seat == NextPlayer(NextPlayer(_gameTable.trickLeaderIndex)))
                index = AutoPlaySupportCard();
            else
                index = AutoPlayLastDefendCard();
            return index;
        }

        public bool CardBelongsToLedSuit(EuchreCard card)
        {
            EuchreCard.Suits ThisSuit = card.GetCurrentSuit(_gameTable.handTrumpSuit);
            return (ThisSuit == _gameTable.trickSuitLed);
        }

        public int LowestCardOnReplace(EuchreCard.Suits trump)
        {
            int rv = -1;
            int value = (int)EuchreCard.Values.RightBower + 1;
            for (int i = 0; i <= 4; i++)
            {
                int localvalue = (int)handCardsHeld[i].GetValue(trump);
                if (localvalue < value)
                {
                    value = localvalue;
                    rv = i;
                }
            }
            return rv;
        }

        public Seats OppositeSeat()
        {
            switch (Seat)
            {
            case Seats.LeftOpponent: return Seats.RightOpponent;
            case Seats.RightOpponent: return Seats.LeftOpponent;
            case Seats.Player: return Seats.Partner;
            case Seats.Partner: return Seats.Player;
            default: throw new Exception("Unknown seat type in OppositeSeat");
            }
        }

        public void ClearAllTricks()
        {
            handTricksWon = 0;
            handSittingOut = false;
        }

        public EuchrePlayer(Seats NewSeat, EuchreTable table)
        {
            Seat = NewSeat;
            _gameTable = table;
            gameVoice = new EuchreSpeech();
        }

        public void DisposeVoice()
        {
            if (gameVoice != null)
            {
                gameVoice.DisposeVoice();
            }
        }

        public void SortCards(EuchreCard.Suits Trump)
        {
            EuchreCard card;
            for (int i = 0; i <= 4; i++)
            {
                for (int j = i+1; j <= 4; j++)
                {
                    if (handCardsHeld[i].GetSortValue(Trump) < handCardsHeld[j].GetSortValue(Trump))
                    {
                        card = handCardsHeld[i];
                        handCardsHeld[i] = handCardsHeld[j];
                        handCardsHeld[j] = card;
                    }
                }
            }
        }

        public string GetDisplayName()
        {
            switch (Seat)
            {
            case Seats.LeftOpponent: return _gameTable.gameLeftOpponentName;
            case Seats.RightOpponent: return _gameTable.gameRightOpponentName;
            case Seats.Player: return _gameTable.gamePartnerName;
            case Seats.Partner: return _gameTable.gamePlayerName;
            default: throw new Exception("Unknown seat type in OppositeSeat");
            }
        }

        public void HumanBidFirstRound()
        {
            _gameTable.BidControl.Reset();
            _gameTable.BidControl.GoingAlone.IsEnabled = false;
            _gameTable.BidControl.ForceGoAlone(_gameTable.handDealer == Seats.Partner && _gameTable.ruleUseQuietDealer);
            _gameTable.BidControl.OkButton.IsDefault = true;
            _gameTable.BidControl.Visibility = System.Windows.Visibility.Visible;
            _gameTable.BidControl.IsEnabled = true;
            _gameTable.BidControl.IsHitTestVisible = true;
            _gameTable.BidControl.UpdateLayout();
        }

        public bool AutoBidFirstRound(bool GoingAlone)
        {
            bool bid = false;
            int value = HandValue(_gameTable.handKitty[0].Suit);
            if (_gameTable.handDealer == Seat || _gameTable.handDealer == OppositeSeat())
            {
                int index = LowestCardOnReplace(_gameTable.handTrumpSuit); // Player would drop this one to get the kitty card
                value += _gameTable.handKitty[0].GetValue(_gameTable.handKitty[0].Suit) - handCardsHeld[index].GetValue(_gameTable.handKitty[0].Suit);
            }

            if (value >= Makeable())
            {
                if (!(_gameTable.handDealer == OppositeSeat() && _gameTable.ruleUseQuietDealer))
                {
                    bid = true;
                }
                if (value >= Loner())
                {
                    bid = true;
                    GoingAlone = true;
                }
            }
            return bid;
        }

        public void ProcessBidFirstRoundPassed()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat(Properties.Resources.Notice_Pass, GetDisplayName());
            _gameTable.UpdateStatus(s.ToString());
            _gameTable.SpeakPass(Seat);
        }

        public void ProcessBidFirstRoundCalled(bool GoingAlone)
        {
            StringBuilder s = new StringBuilder();
            _gameTable.handTrumpSuit = _gameTable.handKitty[0].Suit;

            if (GoingAlone)
            {
                _gameTable.gamePlayers[(int)OppositeSeat()].handSittingOut = true;
                if (OppositeSeat() == _gameTable.trickLeaderIndex)
                {
                    _gameTable.trickLeaderIndex = NextPlayer(_gameTable.trickLeaderIndex);
                }
                if (Seat == _gameTable.handDealer)
                {
                    s.AppendFormat(Properties.Resources.Notice_IPickItUpAlone, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                    _gameTable.UpdateStatus(s.ToString());
                    _gameTable.SpeakIPickItUp(Seat);
                }
                else
                {
                    s.AppendFormat(Properties.Resources.Notice_PickItUpAlone, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                    _gameTable.UpdateStatus(s.ToString());
                    _gameTable.SpeakPickItUp(Seat);
                }
                _gameTable.SpeakSuit(Seat);
                _gameTable.SpeakAlone(Seat);
            }
            else
            {
                if (Seat == _gameTable.handDealer)
                {
                    s.AppendFormat(Properties.Resources.Notice_IPickItUp, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                    _gameTable.UpdateStatus(s.ToString());
                    _gameTable.SpeakIPickItUp(Seat);
                }
                else
                {
                    s.AppendFormat(Properties.Resources.Notice_PickItUp, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                    _gameTable.UpdateStatus(s.ToString());
                    _gameTable.SpeakPickItUp(Seat);
                }
                _gameTable.SpeakSuit(Seat);
            }
        }

        public void ProcessBidFirstRound(bool GoingAlone, bool Called)
        {
            if (Called)
                ProcessBidFirstRoundCalled(GoingAlone);
            else
                ProcessBidFirstRoundPassed();
        }

        public void HumanBidSecondRound()
        {
            _gameTable.BidControl2.Reset();
            // Don't forget about the "stick the dealer" choice!
            if (Seat == _gameTable.handDealer && _gameTable.ruleStickTheDealer)
            {
                _gameTable.BidControl2.Pass.IsChecked = false;
                _gameTable.BidControl2.Pass.IsEnabled = false;
                _gameTable.BidControl2.Pass.Opacity = 0.25;
                _gameTable.BidControl2.GoingAlone.IsEnabled = true;
                if (_gameTable.handKitty[0].Suit != EuchreCard.Suits.Hearts)
                {
                    _gameTable.BidControl2.Hearts.IsChecked = true;
                }
                else if (_gameTable.handKitty[0].Suit != EuchreCard.Suits.Diamonds)
                {
                    _gameTable.BidControl2.Diamonds.IsChecked = true;
                }
                else if (_gameTable.handKitty[0].Suit != EuchreCard.Suits.Clubs)
                {
                    _gameTable.BidControl2.Clubs.IsChecked = true;
                }
                else
                {
                    _gameTable.BidControl2.Spades.IsChecked = true;
                }
            }

            switch (_gameTable.handKitty[0].Suit) // Disable the suit of the kitty card; user can't choose it
            {
            case EuchreCard.Suits.Hearts:
                _gameTable.BidControl2.Hearts.IsEnabled = false;
                _gameTable.BidControl2.Hearts.Opacity = 0.25;
                break;
            case EuchreCard.Suits.Diamonds:
                _gameTable.BidControl2.Diamonds.IsEnabled = false;
                _gameTable.BidControl2.Diamonds.Opacity = 0.25;
                break;
            case EuchreCard.Suits.Clubs:
                _gameTable.BidControl2.Clubs.IsEnabled = false;
                _gameTable.BidControl2.Clubs.Opacity = 0.25;
                break;
            case EuchreCard.Suits.Spades:
                _gameTable.BidControl2.Spades.IsEnabled = false;
                _gameTable.BidControl2.Spades.Opacity = 0.25;
                break;
            }

            _gameTable.BidControl2.ForceGoAlone(_gameTable.handDealer == Seats.Partner && _gameTable.ruleUseQuietDealer);

            _gameTable.BidControl2.OkButton.IsDefault = true; ;
            _gameTable.BidControl2.Visibility = System.Windows.Visibility.Visible;
            _gameTable.BidControl2.IsHitTestVisible = true;
            _gameTable.BidControl2.IsEnabled = true;
            _gameTable.BidControl2.UpdateLayout();
        }

        public bool AutoBidSecondRound(bool GoingAlone)
        {
            bool rv = false;
            bool bid = false;
            EuchreCard.Suits TrumpSuit = EuchreCard.Suits.NoSuit;

            int value = 0;
            for (EuchreCard.Suits i = EuchreCard.Suits.Spades; i <= EuchreCard.Suits.Hearts; i++)
            {
                int localvalue = 0;
                if (i != _gameTable.handKitty[0].Suit)
                {
                    localvalue = HandValue(i);
                }
                if (localvalue > value)
                {
                    value = localvalue;
                    TrumpSuit = i;
                }
            }

            if (_gameTable.handDealer == Seat && _gameTable.ruleStickTheDealer)
            {
                bid = true;
            }
            else if (value >= Makeable() && !(_gameTable.handDealer == OppositeSeat() && _gameTable.ruleUseQuietDealer))
            {
                bid = true;
            }
            else if (value >= Loner())
            {
                bid = true;
                GoingAlone = true;
            }

            if (bid)
            {
                rv = true;
                _gameTable.handTrumpSuit = TrumpSuit;
            }

            return rv;
        }

        public void ProcessBidSecondRound(bool GoingAlone, bool Called)
        {
            if (Called)
                ProcessBidSecondRoundCalled(GoingAlone);
            else
                ProcessBidSecondRoundPassed();
        }

        public void ProcessBidSecondRoundPassed()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat(Properties.Resources.Notice_Pass, GetDisplayName());
            _gameTable.UpdateStatus(s.ToString());
            _gameTable.SpeakPass(Seat);
        }

        public void ProcessBidSecondRoundCalled(bool GoingAlone)
        {
            StringBuilder s = new StringBuilder();
            if (GoingAlone)
            {
                _gameTable.gamePlayers[(int)OppositeSeat()].handSittingOut = true;
                _gameTable.EnableCards(OppositeSeat(), false);

                if (OppositeSeat() == _gameTable.trickLeaderIndex)
                {
                    _gameTable.trickLeaderIndex = NextPlayer(_gameTable.trickLeaderIndex);
                }
                s.AppendFormat(Properties.Resources.Notice_ChoseTrumpAlone, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                _gameTable.UpdateStatus(s.ToString());
                _gameTable.SpeakSuit(Seat);
                _gameTable.SpeakAlone(Seat);
            }
            else
            {
                s.AppendFormat(Properties.Resources.Notice_ChoseTrump, GetDisplayName(), Properties.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(_gameTable.handTrumpSuit)));
                _gameTable.UpdateStatus(s.ToString());
                _gameTable.SpeakSuit(Seat);
            }
            _gameTable.handPickedTrump = Seat;

        }

        #endregion

        #region "Public variables"
        public Seats Seat;
        public EuchreCard trickBuriedCard = null!;
        public int handTricksWon;
        public EuchreCard[] handCardsHeld = new EuchreCard[5];
        public bool handSittingOut;
        public Personalities gamePersonality = Personalities.Normal;
        public EuchreSpeech gameVoice;
        #endregion
        #region "Private variables"
        private EuchreTable _gameTable;

        #endregion
    }
}
