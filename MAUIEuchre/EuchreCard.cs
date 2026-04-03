using System;
using System.Text;

namespace MAUIEuchre
{
    public class EuchreCard
    {
        #region "Enums"
        public enum Suits : int
        {
            Hearts = 3,
            Diamonds = 2,
            Clubs = 1,
            Spades = 0,
            NoSuit = -1
        }
        public enum Ranks : int
        {
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13,
            Ace = 14,
            NoRank = -1
        }
        public enum States : int
        {
            FaceDown = 0,
            FaceUp = 1,
            NoState = -1
        }
        public enum Values : int
        {
            NineNoTrump = 1,
            TenNoTrump = 2,
            JackNoTrump = 3,
            QueenNoTrump = 4,
            KingNoTrump = 5,
            AceNoTrump = 10,
            NineTrump = 12,
            TenTrump = 15,
            QueenTrump = 20,
            KingTrump = 25,
            AceTrump = 30,
            LeftBower = 31,
            RightBower = 35,
            NoValue = -1
        }
        #endregion

        #region "Static Methods"

        public static string GetSuitDisplayStringResourceName(EuchreCard.Suits TheSuit)
        {
            switch (TheSuit)
            {
            case Suits.NoSuit:
                return "NOSUIT";
            case Suits.Hearts:
                return "HEARTS";
            case Suits.Diamonds:
                return "DIAMONDS";
            case Suits.Clubs:
                return "CLUBS";
            case Suits.Spades:
                return "SPADES";
            default:
                return "NOSUIT"; // Won't happen, but...
            }
        }

        public static string GetSuitImageResourceName(EuchreCard.Suits TheSuit)
        {
            switch (TheSuit)
            {
            case Suits.Hearts:
                return "heartsimage.png";
            case Suits.Diamonds:
                return "diamondsimage.png";
            case Suits.Clubs:
                return "clubsimage.png";
            case Suits.Spades:
                return "spadesimage.png";
            default:
                return "";
            }
        }

        public static Suits GetBowerSuit(Suits Trump)
        {
            switch (Trump)
            {
            case Suits.Hearts:
                return Suits.Diamonds;
            case Suits.Diamonds:
                return Suits.Hearts;
            case Suits.Clubs:
                return Suits.Spades;
            case Suits.Spades:
                return Suits.Clubs;
            default:
                return Suits.NoSuit; // Won't happen, but...
            }
        }

        public static int GetSeatRotationAngle(EuchrePlayer.Seats seat)
        {
            return seat switch
            {
                EuchrePlayer.Seats.Player => 0,
                EuchrePlayer.Seats.LeftOpponent => 90,
                EuchrePlayer.Seats.Partner => 180,
                EuchrePlayer.Seats.RightOpponent => 270,
                _ => 0
            };
        }

        #endregion

        #region "Public methods"
        public EuchreCard(Ranks NewRank, Suits NewSuit, IGameTable CardTable)
        {
            Rank = NewRank;
            Suit = NewSuit;
            stateCurrent = States.FaceDown;
            orientationCurrent = EuchrePlayer.Seats.Player;
            _gameTable = CardTable;
        }

        public EuchrePlayer.Seats Perspective
        {
            get { return orientationCurrent; }
            set
            {
                int diff = orientationCurrent - value;
                if (diff != 0)
                {
                    int addedRotation = diff switch
                    {
                        -1 or 3 => 90,
                        -2 or 2 => 180,
                        -3 or 1 => 270,
                        _ => 0
                    };
                    rotationAngle = (rotationAngle + addedRotation) % 360;
                    orientationCurrent = value;
                }
            }
        }

        public string GetImageSource(EuchrePlayer.Seats player)
        {
            if (stateCurrent == States.FaceUp || _gameTable.modePeekAtOtherCards)
                return imageResourceName;
            else
                return CardBackImageName;
        }

        public string GetImageResourceName()
        {
            return $"cardface{Rank.ToString().ToLowerInvariant()}of{Suit.ToString().ToLowerInvariant()}.png";
        }

        public string GetDisplayStringResourceName(Suits TrumpSuit = Suits.NoSuit)
        {
            if (stateCurrent == States.FaceUp || _gameTable.modePeekAtOtherCards || _gameTable.stateSelectingDealer)
            {
                StringBuilder FileName = new StringBuilder();
                if (TrumpSuit != Suits.NoSuit && (GetValue(TrumpSuit) == Values.LeftBower || GetValue(TrumpSuit) == Values.RightBower))
                {
                    if (GetValue(TrumpSuit) == Values.RightBower)
                    {
                        FileName.Append("CARDNAME_RightBower");
                        return FileName.ToString();
                    }
                    else
                    {
                        FileName.Append("CARDNAME_LeftBower");
                        return FileName.ToString();
                    }
                }
                else
                {
                    FileName.AppendFormat("CARDNAME_{0}Of{1}", Rank, Suit);
                    return FileName.ToString();
                }
            }
            else
            {
                return "CARDNAME_BACK";
            }
        }

        public Suits GetCurrentSuit(Suits Trump)
        {
            return (Suit == GetBowerSuit(Trump) && Rank == Ranks.Jack) ? Trump : Suit;
        }

        public Values GetCurrentValue(Suits Trump, Suits LedSuit)
        {
            return (GetCurrentSuit(Trump) == LedSuit || GetCurrentSuit(Trump) == Trump) ? GetValue(Trump) : 0;
        }

        public int GetSortValue(Suits Trump)
        {
            return GetCurrentSuit(Trump) == Trump ? (int)GetValue(Trump) + 5000 : (int)GetValue(Trump) + (int)Suit * 1000;
        }

        public Values GetValue(Suits Trump)
        {
            if (Suit == Trump)
            {
                switch (Rank)
                {
                case Ranks.Nine:
                    return Values.NineTrump;
                case Ranks.Ten:
                    return Values.TenTrump;
                case Ranks.Queen:
                    return Values.QueenTrump;
                case Ranks.King:
                    return Values.KingTrump;
                case Ranks.Ace:
                    return Values.AceTrump;
                case Ranks.Jack:
                    return Values.RightBower;
                }
            }
            else if (Suit == GetBowerSuit(Trump) && Rank == Ranks.Jack)
            {
                return Values.LeftBower;
            }
            else
            {
                switch (Rank)
                {
                case Ranks.Nine:
                    return Values.NineNoTrump;
                case Ranks.Ten:
                    return Values.TenNoTrump;
                case Ranks.Jack:
                    return Values.JackNoTrump;
                case Ranks.Queen:
                    return Values.QueenNoTrump;
                case Ranks.King:
                    return Values.KingNoTrump;
                case Ranks.Ace:
                    return Values.AceNoTrump;
                }
            }
            return Values.NoValue; // Should never get here, but...
        }

        #endregion

        #region "Public variables"
        public const int goalLoner = 117;
        public const int goalMakeable = 85;
        public const string CardBackImageName = "cardback.png";

        public States stateCurrent;
        public Suits Suit;
        public Ranks Rank;
        public string imageResourceName = string.Empty;
        public int rotationAngle;
        public EuchrePlayer.Seats orientationCurrent;
        public static string[] suitImageNames = new string[4];
        #endregion

        #region "Private variables"
        private IGameTable _gameTable;
        #endregion
    }

    public class EuchreCardDeck
    {
        #region "Public methods"
        public EuchreCardDeck(bool UseNineOfHeartsVariation, IGameTable CardTable)
        {
            _ruleNineOfHeartsVariation = UseNineOfHeartsVariation;
            _gameMaxCards = _ruleNineOfHeartsVariation ? 21 : 24;
            _gameTable = CardTable;
        }

        public void Initialize()
        {
            if (!_stateInited)
            {
                int n = 0;
                for (EuchreCard.Suits NewSuit = EuchreCard.Suits.Spades; NewSuit <= EuchreCard.Suits.Hearts; NewSuit++)
                {
                    EuchreCard.suitImageNames[(int)NewSuit] = EuchreCard.GetSuitImageResourceName(NewSuit);
                    for (EuchreCard.Ranks NewRank = EuchreCard.Ranks.Nine; NewRank <= EuchreCard.Ranks.Ace; NewRank++)
                    {
                        if (!(_ruleNineOfHeartsVariation && NewRank == EuchreCard.Ranks.Nine && NewSuit != EuchreCard.Suits.Hearts))
                        {
                            _gameCards[n] = new EuchreCard(NewRank, NewSuit, _gameTable);
                            _gameCards[n].imageResourceName = _gameCards[n].GetImageResourceName();
                            n = n + 1;
                        }
                    }
                }
            }
            _stateInited = true;
            _cardTopmost = 0;
        }

        public EuchreCard GetNextCard()
        {
            if (!_stateInited)
                Shuffle();
            EuchreCard rv = _gameCards[_cardTopmost];
            _cardTopmost++;
            return rv;
        }

        public void Shuffle()
        {
            if (!_stateInited)
                Initialize();

            for (int i = 0; i < _gameMaxCards; i++)
            {
                int rand = Random.Shared.Next(_gameMaxCards);
                EuchreCard temp = _gameCards[i];
                _gameCards[i] = _gameCards[rand];
                _gameCards[rand] = temp;
            }

            _cardTopmost = 0;
        }
        #endregion

        #region "Private variables"
        private bool _ruleNineOfHeartsVariation;
        private int _gameMaxCards;
        private EuchreCard[] _gameCards = new EuchreCard[24];
        private IGameTable _gameTable;
        private bool _stateInited;
        private int _cardTopmost;
        #endregion
    }
}
