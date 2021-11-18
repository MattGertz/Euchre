Imports System.Text

Public Class EuchrePlayer
#Region "Enums"
    ''' <summary>
    ''' The personalities of the AI players, which affects how they bid
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Personalities
        Crazy = 0
        Normal = 1
        Conservative = 2
    End Enum
    ''' <summary>
    ''' The enum which tracks the identity of the players.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Seats
        LeftOpponent = 0
        Partner = 1
        RightOpponent = 2
        Player = 3
        NoPlayer = -1
    End Enum
#End Region
#Region "Private methods"
    ''' <summary>
    ''' Calculate the total value of a hand based on the given trump suit.  Used for AI bidding.
    ''' </summary>
    ''' <param name="TrumpSuit">The trump suit</param>
    ''' <returns>Total value of hand</returns>
    ''' <remarks></remarks>
    Private Function HandValue(ByVal TrumpSuit As EuchreCard.Suits) As Integer
        HandValue = 0
        For i As Integer = 0 To 4
            HandValue = HandValue + Me.CardsHeldThisHand(i).GetValue(TrumpSuit)
        Next
    End Function

    ''' <summary>
    ''' Helper method to return the card of highest value in the player's hand, given the current trump.
    ''' Useful when the AI needs to see if it as a shot at winning the trick.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card with the highest value</returns>
    ''' <remarks></remarks>
    Private Function HighestCard(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = 0
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue > value Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the card of highest value in the player's hand, given the current trump,
    ''' which is not a trump card.  Useful when the AI needs to lead a high non-trump suit which has
    ''' a reasonable change of winning.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card with the highest non-trump value, or -1 if not found</returns>
    ''' <remarks></remarks>
    Private Function HighestCardNotTrump(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = 0
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue > value AndAlso localvalue < EuchreCard.Values.NineTrump Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the card of lowest value in the player's hand, given the current trump.
    ''' Useful when the AI knows it can't win the trick and just wants to dump a low card.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card with the lowest value</returns>
    ''' <remarks></remarks>
    Private Function LowestCard(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue < value Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the trump card of lowest value in the player's hand, given the current trump.
    ''' Useful when the AI wants to trump over a non-trump suit without wasting high trump.
    ''' (Yes, the AI does tend to "send a boy out to do a man's job," but realistically it's the
    ''' smarter play most of the time.)
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the trump card with the lowest value, or -1 if not found</returns>
    ''' <remarks></remarks>
    Private Function LowestCardTrump(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue < value AndAlso localvalue > EuchreCard.Values.AceNoTrump Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the card of highest value in the player's hand, given the current trump,
    ''' of the led suit.
    ''' Useful when the AI needs to see if it as a shot at winning the trick.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card with the highest value of the led suit, or -1 if not found</returns>
    ''' <remarks></remarks>
    Private Function HighestCardLedSuit(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = 0
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing AndAlso CardBelongsToLedSuit(Table, CardsHeldThisHand(i)) Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue > value Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the card of lowest value in the player's hand, given the current trump,
    ''' which could win the hand.
    ''' Useful when the AI is the last player and wants to win economically.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card with the lowest value which will win the trick, or -1 if not found.</returns>
    ''' <remarks></remarks>
    Private Function LowestCardThatTakesLedSuit(ByVal Table As EuchreTable, ByVal ValueToBeat As EuchreCard.Values) As Integer
        Dim index As Integer = -1
        Dim DifferenceToMinimize As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing AndAlso CardBelongsToLedSuit(Table, CardsHeldThisHand(i)) Then
                Dim localdifference As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit) - ValueToBeat
                If localdifference > 0 AndAlso localdifference < DifferenceToMinimize Then
                    index = i
                    DifferenceToMinimize = localdifference
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the trump card of lowest value in the player's hand, given the current trump,
    ''' which will win the hand.
    ''' Useful when the AI is the last player and wants to win economically.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the trump card with the lowest value which will win the trick, or -1 if not found.</returns>
    ''' <remarks></remarks>
    Private Function LowestCardTrumpThatTakes(ByVal Table As EuchreTable, ByVal ValueToBeat As EuchreCard.Values) As Integer
        Dim index As Integer = -1
        Dim DifferenceToMinimize As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing Then
                Dim localdifference As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit) - ValueToBeat
                If localdifference > 0 AndAlso localdifference < DifferenceToMinimize AndAlso CardsHeldThisHand(i).GetValue(Table.TrumpSuit) > EuchreCard.Values.AceNoTrump Then
                    index = i
                    DifferenceToMinimize = localdifference
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Helper method to return the  card of lowest value in the player's hand, given the current trump,
    ''' which will win the hand.
    ''' Useful when the AI is the last player and wants to win economically.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the trump card with the lowest value which will win the trick, or -1 if not found.</returns>
    ''' <remarks></remarks>
    Private Function LowestCardLedSuit(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        Dim value As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            If Not CardsHeldThisHand(i) Is Nothing AndAlso CardBelongsToLedSuit(Table, CardsHeldThisHand(i)) Then
                Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(Table.TrumpSuit)
                If localvalue < value Then
                    index = i
                    value = localvalue
                End If
            End If
        Next
        Return index
    End Function

    ''' <summary>
    ''' Calculates a card for an AI player to lead..
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card that the player will lead</returns>
    ''' <remarks></remarks>
    Private Function AutoLeadACard(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        If Seat = Table.PickedTrumpThisHand OrElse OppositeSeat() = Table.PickedTrumpThisHand Then
            ' Start off strong, and lead your highest value card:
            index = HighestCard(Table)
        Else
            ' Lead a high card that isn't trump
            index = HighestCardNotTrump(Table)
            If index = -1 Then
                index = HighestCard(Table)
            End If
        End If
        Return index
    End Function

    ''' <summary>
    ''' Calculates a card for an AI player to play when it is on the defending team.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card that the player will play</returns>
    ''' <remarks></remarks>
    Private Function AutoPlayDefendCard(ByVal Table As EuchreTable) As Integer
        Dim CurrentHighestValue As EuchreCard.Values = Table.PlayedCards(Table.TrickLeaderIndex).GetValue(Table.TrumpSuit)
        Dim index As Integer = HighestCardLedSuit(Table)
        If index = -1 Then
            ' Don't have that suit -- try to trump it
            index = LowestCardTrump(Table)
            If index = -1 Then
                ' Don't have trump -- throw junk
                index = LowestCard(Table)
            End If
        ElseIf Me.CardsHeldThisHand(index).GetValue(Table.TrumpSuit) < CurrentHighestValue Then
            ' Can't beat it -- throw lowest possible
            index = LowestCardLedSuit(Table)
        End If
        Return index
    End Function

    ''' <summary>
    ''' Calculates a card for an AI player to play when it is on the making team.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>Index of the card that the player will play</returns>
    ''' <remarks></remarks>
    Private Function AutoPlaySupportCard(ByVal Table As EuchreTable) As Integer
        Dim CurrentLeaderValue As EuchreCard.Values = Table.PlayedCards(Table.TrickLeaderIndex).GetValue(Table.TrumpSuit)
        Dim CurrentDefenderValue As EuchreCard.Values = EuchreCard.Values.NoValue
        If Not Table.Players(NextPlayer(Table.TrickLeaderIndex)).SittingOutThisHand Then
            CurrentDefenderValue = Table.PlayedCards(NextPlayer(Table.TrickLeaderIndex)).GetCurrentValue(Table.TrumpSuit, Table.SuitLedThisRound)
        End If

        Dim Winning As Boolean = (CurrentDefenderValue <= CurrentLeaderValue)
        Dim index As Integer = -1
        If Not Winning Then
            index = HighestCardLedSuit(Table)
            If index = -1 Then
                ' Don't have that suit -- try to trump it
                index = LowestCardTrumpThatTakes(Table, CurrentDefenderValue)
                If index = -1 Then
                    ' Don't have trump -- throw junk
                    index = LowestCard(Table)
                End If
            ElseIf Me.CardsHeldThisHand(index).GetValue(Table.TrumpSuit) < CurrentDefenderValue Then
                ' Can't beat it -- throw lowest possible
                index = LowestCardLedSuit(Table)
            End If
        Else
            ' Don't overplay my partner
            index = LowestCardLedSuit(Table)
            If index = -1 Then
                ' Don't have that suit, just throw junk
                index = LowestCard(Table)
            End If
        End If
        Return index
    End Function

    ''' <summary>
    ''' Calculates the card to play when the player is the last player and is on the defending team
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>The index of the card that the player will play</returns>
    ''' <remarks></remarks>
    Private Function AutoPlayLastDefendCard(ByVal Table As EuchreTable) As Integer
        Dim CurrentLeaderValue As EuchreCard.Values = Table.PlayedCards(Table.TrickLeaderIndex).GetValue(Table.TrumpSuit)
        Dim CurrentDefenderValue As EuchreCard.Values = EuchreCard.Values.NoValue
        If Not Table.Players(NextPlayer(Table.TrickLeaderIndex)).SittingOutThisHand Then
            CurrentDefenderValue = Table.PlayedCards(NextPlayer(Table.TrickLeaderIndex)).GetCurrentValue(Table.TrumpSuit, Table.SuitLedThisRound)
        End If
        Dim CurrentSupporterValue As EuchreCard.Values = EuchreCard.Values.NoValue
        If Not Table.Players(NextPlayer(NextPlayer(Table.TrickLeaderIndex))).SittingOutThisHand Then
            CurrentSupporterValue = Table.PlayedCards(NextPlayer(NextPlayer(Table.TrickLeaderIndex))).GetCurrentValue(Table.TrumpSuit, Table.SuitLedThisRound)
        End If

        Dim Winning As Boolean = (CurrentDefenderValue > CurrentLeaderValue) AndAlso (CurrentDefenderValue > CurrentSupporterValue)
        Dim index As Integer = -1
        If Not Winning Then
            Dim ValueToBeat As EuchreCard.Values = CurrentLeaderValue
            If CurrentSupporterValue > CurrentLeaderValue Then
                ValueToBeat = CurrentSupporterValue
            End If
            index = LowestCardThatTakesLedSuit(Table, ValueToBeat)
            If index = -1 Then
                index = LowestCardLedSuit(Table)
                If index = -1 Then
                    index = LowestCardTrumpThatTakes(Table, ValueToBeat)
                    If index = -1 Then
                        index = LowestCard(Table)
                    End If
                End If
            End If
        Else
            ' Don't overplay, you've already won
            index = LowestCardLedSuit(Table)
            If index = -1 Then
                ' Throw junk -- you've already won.
                index = LowestCard(Table)
            End If
        End If
        Return index
    End Function

    ''' <summary>
    ''' Calculates what constitutes a makeable hand for this player, given its AI personality.
    ''' </summary>
    ''' <returns>Hand value which is makeable</returns>
    ''' <remarks></remarks>
    Private Function Makeable() As Integer
        Select Case Personality
            Case Personalities.Crazy
                Return EuchreCard.Makeable - 15
            Case Personalities.Normal
                Return EuchreCard.Makeable
            Case Personalities.Conservative
                Return EuchreCard.Makeable + 15
        End Select
        Return EuchreCard.Makeable ' Should never get here, but...
    End Function

    ''' <summary>
    ''' Calculates what constitutes a loner hand for this player, given its AI personality.
    ''' </summary>
    ''' <returns>Hand value which can be made alone</returns>
    ''' <remarks></remarks>
    Private Function Loner() As Integer
        Select Case Personality
            Case Personalities.Crazy
                Return EuchreCard.Loner - 15
            Case Personalities.Normal
                Return EuchreCard.Loner
            Case Personalities.Conservative
                Return EuchreCard.Loner + 15
        End Select
        Return EuchreCard.Loner ' Should never get here, but...
    End Function
#End Region

#Region "Shared methods"
    ''' <summary>
    ''' Shared method to get the player whose turn follows the current player.
    ''' </summary>
    ''' <param name="CurrentPlayer">The current player</param>
    ''' <returns>The player who follows the current player</returns>
    ''' <remarks></remarks>
    Public Shared Function NextPlayer(ByVal CurrentPlayer As EuchrePlayer.Seats) As EuchrePlayer.Seats
        If CurrentPlayer = Seats.Player Then
            Return Seats.LeftOpponent
        Else
            Return CurrentPlayer + 1
        End If
    End Function
#End Region
#Region "Public methods"
    ''' <summary>
    ''' Calls methods to calculate the card an AI player will play, based on the status of the game.
    ''' </summary>
    ''' <param name="Table">Reference back to the EuchreTable</param>
    ''' <returns>The index of the card that the player will play</returns>
    ''' <remarks></remarks>
    Public Function AutoPlayACard(ByVal Table As EuchreTable) As Integer
        Dim index As Integer = -1
        If Seat = Table.TrickLeaderIndex Then
            index = AutoLeadACard(Table)
        ElseIf Seat = NextPlayer(Table.TrickLeaderIndex) Then
            index = AutoPlayDefendCard(Table)
        ElseIf Seat = NextPlayer(NextPlayer(Table.TrickLeaderIndex)) Then
            index = AutoPlaySupportCard(Table)
        Else
            index = AutoPlayLastDefendCard(Table)
        End If
        If CardsHeldThisHand(index) Is Nothing Then
            Stop ' Really bad error
        End If
        Return index
    End Function

    ''' <summary>
    ''' Helper function to determine if a given card belongs tot he suit which was led.
    ''' </summary>
    ''' <param name="Table">Reference back to the euchre table to get the led suit</param>
    ''' <param name="card">The card</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CardBelongsToLedSuit(ByVal Table As EuchreTable, ByVal card As EuchreCard) As Boolean
        Dim ThisSuit As EuchreCard.Suits = card.GetCurrentSuit(Table.TrumpSuit)
        If ThisSuit = Table.SuitLedThisRound Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Calculates the best card to drop when picking up a card from the kitty for a given trump suit.
    ''' </summary>
    ''' <param name="trump">The trump suit</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LowestCardOnReplace(ByVal trump As EuchreCard.Suits) As Integer
        LowestCardOnReplace = -1
        Dim value As Integer = EuchreCard.Values.RightBower + 1
        For i As Integer = 0 To 4
            Dim localvalue As Integer = CardsHeldThisHand(i).GetValue(trump)
            If localvalue < value Then
                value = localvalue
                LowestCardOnReplace = i
            End If
        Next

    End Function

    ''' <summary>
    ''' Helper function to determine who the player's partner is.
    ''' </summary>
    ''' <returns>Player's partner</returns>
    ''' <remarks></remarks>
    Public Function OppositeSeat() As Seats
        Select Case Seat
            Case Seats.LeftOpponent
                Return Seats.RightOpponent
            Case Seats.Partner
                Return Seats.Player
            Case Seats.RightOpponent
                Return Seats.LeftOpponent
            Case Seats.Player
                Return Seats.Partner
        End Select
        Return Seats.NoPlayer ' Should never get here, but...
    End Function

    ''' <summary>
    ''' Clears the information on how many tricks this player has won, and whether or not the player
    ''' is sitting out the hand.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ClearAllTricks()
        TricksWonThisHand = 0
        SittingOutThisHand = False
    End Sub

    ''' <summary>
    ''' Creates a new player object at the given seat
    ''' </summary>
    ''' <param name="NewSeat">The player's seat identity</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal NewSeat As Seats)
        Seat = NewSeat
        Voice = New EuchreSpeech
    End Sub

    Public Sub DisposeVoice()
        If Voice IsNot Nothing Then
            Voice.DisposeVoice()
        End If
    End Sub

    ''' <summary>
    ''' Select-sorts the player's hand array based on the current trump.  Caller is responsible for updating UI.
    ''' </summary>
    ''' <param name="Trump">The trump suit</param>
    ''' <remarks></remarks>
    Public Sub SortCards(ByVal Trump As EuchreCard.Suits)
        Dim j As Integer
        Dim card As EuchreCard
        For i As Integer = 0 To 4
            For j = i + 1 To 4
                If CardsHeldThisHand(i).GetSortValue(Trump) < CardsHeldThisHand(j).GetSortValue(Trump) Then
                    card = CardsHeldThisHand(i)
                    CardsHeldThisHand(i) = CardsHeldThisHand(j)
                    CardsHeldThisHand(j) = card
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Gets the name of the player -- this is used for UI feedback
    ''' </summary>
    ''' <param name="Table">Reference to the EuchreTable which owns the game</param>
    ''' <returns>The player's name</returns>
    ''' <remarks></remarks>
    Public Function GetDisplayName(ByVal Table As EuchreTable) As String
        Select Case Seat
            Case Seats.LeftOpponent
                Return Table.LeftOpponentName
            Case Seats.RightOpponent
                Return Table.RightOpponentName
            Case Seats.Partner
                Return Table.PartnerName
            Case Seats.Player
                Return Table.PlayerName
            Case Else
                Return Table.PlayerName ' Won't happen, but...
        End Select
    End Function

    ''' <summary>
    ''' For the user, a control is shown which collects the first-round bidding choice
    ''' (forcing a "loner" on a call if player's partner is the dealer and "quiet dealer" rule
    ''' is in effect).
    ''' </summary>
    ''' <param name="Table">The euchre table object calling this method</param>
    ''' <remarks></remarks>
    ''' 
    Public Sub HumanBidFirstRound(ByVal Table As EuchreTable)
        Table.BidControl.Reset()
        Table.BidControl.GoingAlone.IsEnabled = False
        If Table.DealerThisHand = Seats.Partner AndAlso Table.UseQuietDealerRule Then
            Table.BidControl.ForceGoAlone(True)
        Else
            Table.BidControl.ForceGoAlone(False)
        End If

        Table.BidControl.OkButton.IsDefault = True
        Table.BidControl.Visibility = Visibility.Visible
        Table.BidControl.IsEnabled = True
        Table.BidControl.IsHitTestVisible = True
        Table.BidControl.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Calculates whether or not the player wants to call trump on the first round, and if he/she
    ''' is going alone.  Card values are used to calculate whether or not the bid is makeable (alone or otherwise), based on
    ''' the AI personality.  The AI is smart enough to deal with the "Quiet Dealer" rule as well.
    ''' </summary>
    ''' <param name="Table">The euchre table object calling this method</param>
    ''' <remarks></remarks>
    ''' 
    Public Function AutoBidFirstRound(ByVal Table As EuchreTable, ByRef GoingAlone As Boolean) As Boolean
        Dim bid As Boolean = False
        Dim value As Integer = HandValue(Table.Kitty(0).Suit)
        If Table.DealerThisHand = Seat OrElse Table.DealerThisHand = OppositeSeat() Then
            Dim index As Integer = LowestCardOnReplace(Table.TrumpSuit) ' Player would drop this one to get the kitty card
            value = value + Table.Kitty(0).GetValue(Table.Kitty(0).Suit) - CardsHeldThisHand(index).GetValue(Table.Kitty(0).Suit)
        End If
        If value >= Makeable() Then
            If Not (Table.DealerThisHand = OppositeSeat() AndAlso Table.UseQuietDealerRule) Then
                bid = True
            End If
            If value >= Loner() Then
                bid = True
                GoingAlone = True
            End If
        End If
        Return bid
    End Function

    ''' <summary>
    ''' Log that the player passed in the first round of bidding.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidFirstRoundPassed(ByVal Table As EuchreTable)
        Dim s As New StringBuilder()
        s.AppendFormat(My.Resources.Notice_Pass, GetDisplayName(Table))
        Table.UpdateStatus(s.ToString)
        Table.SpeakPass(Seat)
    End Sub

    ''' <summary>
    ''' Log that the player ordered the kitty card up in the first round of biddingand update trump.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="GoingAlone"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidFirstRoundCalled(ByVal Table As EuchreTable, GoingAlone As Boolean)
        Dim s As New StringBuilder()
        Table.TrumpSuit = Table.Kitty(0).Suit

        If GoingAlone Then
            Table.Players(OppositeSeat()).SittingOutThisHand = True
            If OppositeSeat() = Table.TrickLeaderIndex Then
                Table.TrickLeaderIndex = NextPlayer(Table.TrickLeaderIndex)
            End If
            If Seat = Table.DealerThisHand Then
                s.AppendFormat(My.Resources.Notice_IPickItUpAlone, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
                Table.UpdateStatus(s.ToString)
                Table.SpeakIPickItUp(Seat)
            Else
                s.AppendFormat(My.Resources.Notice_PickItUpAlone, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
                Table.UpdateStatus(s.ToString)
                Table.SpeakPickItUp(Seat)
            End If
            Table.SpeakSuit(Seat)
            Table.SpeakAlone(Seat)
        Else
            If Seat = Table.DealerThisHand Then
                s.AppendFormat(My.Resources.Notice_IPickItUp, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
                Table.UpdateStatus(s.ToString)
                Table.SpeakIPickItUp(Seat)
            Else
                s.AppendFormat(My.Resources.Notice_PickItUp, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
                Table.UpdateStatus(s.ToString)
                Table.SpeakPickItUp(Seat)
            End If
            Table.SpeakSuit(Seat)
        End If

    End Sub

    ''' <summary>
    ''' Processes a first-round bidder's response based on whether or not the kitty card was ordered up.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="GoingAlone"></param>
    ''' <param name="Called"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidFirstRound(ByVal Table As EuchreTable, GoingAlone As Boolean, Called As Boolean)
        If Called Then
            ProcessBidFirstRoundCalled(Table, GoingAlone)
        Else
            ProcessBidFirstRoundPassed(Table)
        End If
    End Sub

    ''' <summary>
    ''' Prep the table with the second-round bid tool if its the human's turn to bid,
    ''' enabling the various controls on it as appropriate.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <remarks></remarks>
    Public Sub HumanBidSecondRound(ByVal Table As EuchreTable)
        Table.BidControl2.Reset()
        ' Don't forget about the "stick the dealer" choice!
        If Seat = Table.DealerThisHand AndAlso Table.StickTheDealer Then
            Table.BidControl2.Pass.IsChecked = False
            Table.BidControl2.Pass.IsEnabled = False
            Table.BidControl2.Pass.Opacity = 0.25
            Table.BidControl2.GoingAlone.IsEnabled = True
            If Table.Kitty(0).Suit <> EuchreCard.Suits.Hearts Then
                Table.BidControl2.Hearts.IsChecked = True
            ElseIf Table.Kitty(0).Suit <> EuchreCard.Suits.Diamonds Then
                Table.BidControl2.Diamonds.IsChecked = True
            ElseIf Table.Kitty(0).Suit <> EuchreCard.Suits.Clubs Then
                Table.BidControl2.Clubs.IsChecked = True
            Else
                Table.BidControl2.Spades.IsChecked = True
            End If
        End If

        Select Case Table.Kitty(0).Suit ' Disable the suit of the kitty card; user can't choose it
            Case EuchreCard.Suits.Hearts
                Table.BidControl2.Hearts.IsEnabled = False
                Table.BidControl2.Hearts.Opacity = 0.25
            Case EuchreCard.Suits.Diamonds
                Table.BidControl2.Diamonds.IsEnabled = False
                Table.BidControl2.Diamonds.Opacity = 0.25
            Case EuchreCard.Suits.Clubs
                Table.BidControl2.Clubs.IsEnabled = False
                Table.BidControl2.Clubs.Opacity = 0.25
            Case EuchreCard.Suits.Spades
                Table.BidControl2.Spades.IsEnabled = False
                Table.BidControl2.Spades.Opacity = 0.25
        End Select

        If Table.DealerThisHand = Seats.Partner AndAlso Table.UseQuietDealerRule Then
            Table.BidControl2.ForceGoAlone(True)
        Else
            Table.BidControl2.ForceGoAlone(False)
        End If

        Table.BidControl2.OkButton.IsDefault = True
        Table.BidControl2.Visibility = Visibility.Visible
        Table.BidControl2.IsHitTestVisible = True
        Table.BidControl2.IsEnabled = True
        Table.BidControl2.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Handle second-round bidding for an AI player.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="GoingAlone"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AutoBidSecondRound(ByVal Table As EuchreTable, ByRef GoingAlone As Boolean) As Boolean
        Dim rv As Boolean = False
        Dim bid As Boolean = False
        Dim TrumpSuit As EuchreCard.Suits = EuchreCard.Suits.NoSuit

        Dim value As Integer = 0
        For i As EuchreCard.Suits = EuchreCard.Suits.Spades To EuchreCard.Suits.Hearts
            Dim localvalue As Integer = 0
            If i <> Table.Kitty(0).Suit Then
                localvalue = HandValue(i)
            End If
            If localvalue > value Then
                value = localvalue
                TrumpSuit = i
            End If
        Next

        If Table.DealerThisHand = Seat AndAlso Table.StickTheDealer Then
            bid = True
        ElseIf value >= Makeable() AndAlso Not (Table.DealerThisHand = OppositeSeat() AndAlso Table.UseQuietDealerRule) Then
            bid = True
        ElseIf value >= Loner() Then
            bid = True
            GoingAlone = True
        End If

        If bid Then
            rv = True
            Table.TrumpSuit = TrumpSuit
        End If

        Return rv
    End Function

    ''' <summary>
    ''' Processes a second-round bidder's response based on whether or not the kitty card was ordered up.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="GoingAlone"></param>
    ''' <param name="Called"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidSecondRound(ByVal Table As EuchreTable, GoingAlone As Boolean, Called As Boolean)
        If Called Then
            ProcessBidSecondRoundCalled(Table, GoingAlone)
        Else
            ProcessBidSecondRoundPassed(Table)
        End If
    End Sub

    ''' <summary>
    ''' Log that the bidder passed in the second round.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidSecondRoundPassed(ByVal Table As EuchreTable)
        Dim s As New StringBuilder()
        s.AppendFormat(My.Resources.Notice_Pass, GetDisplayName(Table))
        Table.UpdateStatus(s.ToString)
        Table.SpeakPass(Seat)
    End Sub

    ''' <summary>
    ''' Log that a bidder called trump in the second round.
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="GoingAlone"></param>
    ''' <remarks></remarks>
    Public Sub ProcessBidSecondRoundCalled(ByVal Table As EuchreTable, GoingAlone As Boolean)
        Dim s As New StringBuilder()
        If GoingAlone Then
            Table.Players(OppositeSeat()).SittingOutThisHand = True
            Table.EnableCards(OppositeSeat(), False)

            If OppositeSeat() = Table.TrickLeaderIndex Then
                Table.TrickLeaderIndex = NextPlayer(Table.TrickLeaderIndex)
            End If
            s.AppendFormat(My.Resources.Notice_ChoseTrumpAlone, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
            Table.UpdateStatus(s.ToString)
            Table.SpeakSuit(Seat)
            Table.SpeakAlone(Seat)
        Else
            s.AppendFormat(My.Resources.Notice_ChoseTrump, GetDisplayName(Table), My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(Table.TrumpSuit)))
            Table.UpdateStatus(s.ToString)
            Table.SpeakSuit(Seat)
        End If
        Table.PickedTrumpThisHand = Seat
    End Sub

#End Region
#Region "Public variables"
    Public Seat As Seats
    Public BuriedCard As EuchreCard

    Public TricksWonThisHand As Integer
    Public CardsHeldThisHand(5) As EuchreCard
    Public SittingOutThisHand As Boolean

    Public Personality As Personalities = Personalities.Normal
    Public Voice As EuchreSpeech
#End Region
End Class
