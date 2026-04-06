namespace MAUIEuchre
{
    public interface IGameTable
    {
        // Used by EuchreCard
        bool modePeekAtOtherCards { get; }
        bool stateSelectingDealer { get; }

        // Used by EuchrePlayer
        EuchreCard.Suits handTrumpSuit { get; set; }
        EuchreCard[] handPlayedCards { get; }
        EuchrePlayer.Seats trickLeaderIndex { get; set; }
        EuchrePlayer[] gamePlayers { get; }
        EuchrePlayer.Seats handPickedTrump { get; set; }
        EuchreCard.Suits trickSuitLed { get; }
        EuchreCard[] handKitty { get; }
        EuchrePlayer.Seats handDealer { get; }
        bool ruleStickTheDealer { get; }
        bool ruleUseQuietDealer { get; }
        string gameLeftOpponentName { get; }
        string gameRightOpponentName { get; }
        string gamePlayerName { get; }
        string gamePartnerName { get; }

        // Methods used by EuchrePlayer
        void UpdateStatus(string message, int whiteSpace = 1);
        void UpdateStatusBoldName(string format, string boldArg, params string[] otherArgs);
        void SpeakPass(EuchrePlayer.Seats seat);
        void SpeakIPickItUp(EuchrePlayer.Seats seat);
        void SpeakPickItUp(EuchrePlayer.Seats seat);
        void SpeakSuit(EuchrePlayer.Seats seat);
        void SpeakAlone(EuchrePlayer.Seats seat);
        void EnableCards(EuchrePlayer.Seats seat, bool enable);
        void ShowBidFirstRound(bool forceGoAlone);
        void ShowBidSecondRound(EuchreCard.Suits kittyCardSuit, EuchrePlayer.Seats dealer,
            bool stickTheDealer, bool forceGoAlone);
    }
}
