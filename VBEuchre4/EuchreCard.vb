Imports System.Text
Imports System.Drawing
''' <summary>
''' The card object, which supports methods for getting rank, suit, and value information,
''' as well as images of the cards.
''' </summary>
''' <remarks></remarks>
Public Class EuchreCard
#Region "Enums"
    ''' <summary>
    ''' The possible suits of a card
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Suits As Integer
        Hearts = 3
        Diamonds = 2
        Clubs = 1
        Spades = 0
        NoSuit = -1
    End Enum
    ''' <summary>
    ''' The possible ranks of the cards (not accounting for trump)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Ranks As Integer
        Nine = 9
        Ten = 10
        Jack = 11
        Queen = 12
        King = 13
        Ace = 14
        NoRank = -1
    End Enum
    ''' <summary>
    '''  The state of the card (face up or face down)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum States As Integer
        FaceDown = 0
        FaceUp = 1
        NoState = -1
    End Enum
    ''' <summary>
    ''' The value of a card with respect to game UI.  Includes trump values.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Values
        NineNoTrump = 1
        TenNoTrump = 2
        JackNoTrump = 3
        QueenNoTrump = 4
        KingNoTrump = 5
        AceNoTrump = 10
        NineTrump = 12
        TenTrump = 15
        QueenTrump = 20
        KingTrump = 25
        AceTrump = 30
        LeftBower = 31
        RightBower = 35
        NoValue = -1
    End Enum
#End Region
#Region "Shared methods"
    ''' <summary>
    ''' Gets a suit name's resource name.
    ''' </summary>
    ''' <param name="TheSuit"></param>
    ''' <returns>The resource name of the suit's name.</returns>
    ''' <remarks></remarks>
    Public Shared Function GetSuitDisplayStringResourceName(ByVal TheSuit As EuchreCard.Suits) As String
        Select Case TheSuit
            Case Suits.NoSuit
                Return "NOSUIT"
            Case Suits.Hearts
                Return "HEARTS"
            Case Suits.Diamonds
                Return "DIAMONDS"
            Case Suits.Clubs
                Return "CLUBS"
            Case Suits.Spades
                Return "SPADES"
            Case Else
                Return "NOSUIT" ' Won't happen, but...
        End Select
    End Function
    ''' <summary>
    ''' Gets a suits image's resource name.
    ''' </summary>
    ''' <param name="TheSuit"></param>
    ''' <returns>The reource name of the suit's image</returns>
    ''' <remarks></remarks>
    Public Shared Function GetSuitImageResourceName(ByVal TheSuit As EuchreCard.Suits) As String
        Select Case TheSuit
            Case Suits.NoSuit
                Return "NOSUITIMAGE"
            Case Suits.Hearts
                Return "HEARTSIMAGE"
            Case Suits.Diamonds
                Return "DIAMONDSIMAGE"
            Case Suits.Clubs
                Return "CLUBSIMAGE"
            Case Suits.Spades
                Return "SPADESIMAGE"
            Case Else
                Return "" ' Won't happen, but...
        End Select
    End Function

    ''' <summary>
    ''' A shared function to return the bower suit for a given trump suit.
    ''' This allows the EuchreCard object to more easily calculate a real value.
    ''' </summary>
    ''' <param name="Trump">The trump suit</param>
    ''' <returns>The suit which contains the left bower of the trump suit</returns>
    ''' <remarks></remarks>
    Public Shared Function GetBowerSuit(ByVal Trump As Suits) As Suits
        Select Case Trump
            Case Suits.Hearts
                Return Suits.Diamonds
            Case Suits.Diamonds
                Return Suits.Hearts
            Case Suits.Clubs
                Return Suits.Spades
            Case Suits.Spades
                Return Suits.Clubs
        End Select
        Return Suits.NoSuit ' Should never get here, but...
    End Function
#End Region
#Region "Public methods"

    ''' <summary>
    ''' Creates the card with a specific identity
    ''' </summary>
    ''' <param name="NewRank">Rank of the card</param>
    ''' <param name="NewSuit">Suit of the card</param>
    ''' <param name="CardTable">Reference back to the card table to get information about trump, etc.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal NewRank As Ranks, ByVal NewSuit As Suits, ByVal CardTable As EuchreTable)
        Rank = NewRank
        Suit = NewSuit
        State = States.FaceDown
        ori = EuchrePlayer.Seats.Player
        Table = CardTable
    End Sub
    ''' <summary>
    ''' Sets/gets the perspective of a card (the orientation in which it should be displayed on the table).
    ''' Rotates the image to match the orientation of the card owner.
    ''' </summary>
    ''' <value>The player who owns the card</value>
    ''' <returns>The player who owns the card</returns>
    ''' <remarks></remarks>
    Public Property Perspective() As EuchrePlayer.Seats
        Get
            Return ori
        End Get
        Set(ByVal value As EuchrePlayer.Seats)
            If ori <> value Then
                Dim diff As Integer
                diff = ori - value
                Dim rft As RotateFlipType
                If diff = -1 OrElse diff = 3 Then
                    rft = RotateFlipType.Rotate90FlipNone
                ElseIf diff = 2 OrElse diff = -2 Then
                    rft = RotateFlipType.Rotate180FlipNone
                ElseIf diff = 1 OrElse diff = -3 Then
                    rft = RotateFlipType.Rotate270FlipNone
                End If
                im.RotateFlip(rft)
                ori = value
            End If
        End Set
    End Property
    ''' <summary>
    ''' Returns the image associated with the card for a given player.  Shows the cardback instead if
    ''' a non-user player or the kitty owns the card and "peek" is not turned on
    ''' </summary>
    ''' <param name="player"></param>
    ''' <returns>The appropriate image</returns>
    ''' <remarks></remarks>
    Public Function GetImage(ByVal player As EuchrePlayer.Seats) As Image
        If State = States.FaceUp OrElse Table.PeekAtOtherCards Then
            Return im
        ElseIf player <> EuchrePlayer.Seats.NoPlayer Then
            Return CardBackImages(player)
        Else ' The kitty
            Return CardBackImages(EuchrePlayer.Seats.Player)
        End If

    End Function

    ''' <summary>
    ''' Calculates the resource name of the card's face image.  
    ''' </summary>
    ''' <returns>The resource name of the card image</returns>
    ''' <remarks></remarks>
    Public Function GetImageResourceName() As String
        Dim FileName As New StringBuilder()
        FileName.AppendFormat("CARDFACE{0}Of{1}", Rank, Suit)
        Return FileName.ToString
    End Function

    ''' <summary>
    ''' Calculates the name of the tooltip for the card.  If trump is specified and the card is bower in that
    ''' trump suit, the tooltip is stated accordingly.  Face-down cards always return a generic tooltip,
    ''' and cards used while choosing dealer are always treated as face-up.
    ''' </summary>
    ''' <param name="TrumpSuit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDisplayStringResourceName(Optional ByVal TrumpSuit As Suits = Suits.NoSuit) As String
        If State = States.FaceUp OrElse Table.PeekAtOtherCards OrElse Table.SelectingDealer Then
            Dim FileName As New StringBuilder()
            If TrumpSuit <> Suits.NoSuit AndAlso (Me.GetValue(TrumpSuit) = Values.LeftBower OrElse Me.GetValue(TrumpSuit) = Values.RightBower) Then
                If Me.GetValue(TrumpSuit) = Values.RightBower Then
                    FileName.Append("CARDNAME_RightBower")
                    Return FileName.ToString
                Else
                    FileName.Append("CARDNAME_LeftBower")
                    Return FileName.ToString
                End If
            Else
                FileName.AppendFormat("CARDNAME_{0}Of{1}", Rank, Suit)
                Return FileName.ToString
            End If
        Else
            Return "CARDNAME_BACK"
        End If
    End Function


    ''' <summary>
    ''' Gets the actual suit of a card, given a certain trump suit.  (Basically, this maps the
    ''' left bower into the correct suit.)
    ''' </summary>
    ''' <param name="Trump">The suit which is trump</param>
    ''' <returns>The suit that this card is for the given trump suit</returns>
    ''' <remarks></remarks>
    Public Function GetCurrentSuit(ByVal Trump As Suits) As Suits
        If Suit = GetBowerSuit(Trump) AndAlso Rank = Ranks.Jack Then
            Return Trump
        Else
            Return Suit
        End If
    End Function
    ''' <summary>
    ''' Gets the value of the card given a certain trump suit and the suit led.  Non-trump cards which 
    ''' don't match the led suit have zero value.  This method is used for game AI.  If this card is
    ''' going to be led, then the value of LedSuit should be equal to this card in order for game logic
    ''' to work.  Calls GetValue for the trump/"matches led" case.
    ''' </summary>
    ''' <param name="Trump">The trump suit</param>
    ''' <param name="LedSuit">The suit led in this hand</param>
    ''' <returns>The value of the card</returns>
    ''' <remarks></remarks>
    Public Function GetCurrentValue(ByVal Trump As Suits, ByVal LedSuit As Suits) As EuchreCard.Values
        If GetCurrentSuit(Trump) = LedSuit OrElse GetCurrentSuit(Trump) = Trump Then
            Return GetValue(Trump)
        Else
            Return 0
        End If
    End Function
    ''' <summary>
    ''' Helper function to sort cards.  Basically uses the value of the card, and applies a multiplier
    ''' for each suit (trump being highest) to return a sorted value. Not essential for game AI; strictly
    ''' used for UI visualization.
    ''' </summary>
    ''' <param name="Trump"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetSortValue(ByVal Trump As Suits) As Integer
        If GetCurrentSuit(Trump) = Trump Then
            GetSortValue = GetValue(Trump) + 5000
        Else
            GetSortValue = GetValue(Trump) + Suit * 1000
        End If
    End Function

    ''' <summary>
    ''' Gets the value of a card given a certain trump suit.  Used by game UI in both bidding and tricks.
    ''' </summary>
    ''' <param name="Trump">The trump suit</param>
    ''' <returns>The card value</returns>
    ''' <remarks></remarks>
    Public Function GetValue(ByVal Trump As Suits) As Values
        If Suit = Trump Then
            Select Case Rank
                Case Ranks.Nine
                    Return Values.NineTrump
                Case Ranks.Ten
                    Return Values.TenTrump
                Case Ranks.Queen
                    Return Values.QueenTrump
                Case Ranks.King
                    Return Values.KingTrump
                Case Ranks.Ace
                    Return Values.AceTrump
                Case Ranks.Jack
                    Return Values.RightBower
            End Select
        ElseIf Suit = GetBowerSuit(Trump) AndAlso Rank = Ranks.Jack Then
            Return Values.LeftBower
        Else
            Select Case Rank
                Case Ranks.Nine
                    Return Values.NineNoTrump
                Case Ranks.Ten
                    Return Values.TenNoTrump
                Case Ranks.Jack
                    Return Values.JackNoTrump
                Case Ranks.Queen
                    Return Values.QueenNoTrump
                Case Ranks.King
                    Return Values.KingNoTrump
                Case Ranks.Ace
                    Return Values.AceNoTrump
            End Select
        End If
        Return Values.NoValue ' Should never get here, but...
    End Function
#End Region
#Region "Public variables"
    ' Variable declarations.
    Public Const Loner As Integer = 117
    Public Const Makeable As Integer = 85

    Public State As States
    Public Suit As Suits
    Public Rank As Ranks
    Public im As Image
    Public ori As EuchrePlayer.Seats
    Public Shared CardBackImages(4) As Image
    Public Shared SuitIm(4) As Image
#End Region
#Region "Private variables"
    Private Table As EuchreTable
#End Region
End Class

Public Class EuchreCardDeck
#Region "Public methods"
    ''' <summary>
    ''' Create a new deck of cards for a game.
    ''' </summary>
    ''' <param name="UseNineOfHeartsVariation">Whether or not to include non-Heart card of rank nine</param>
    ''' <param name="CardTable">Reference back to the table to get information like trump, etc.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal UseNineOfHeartsVariation As Boolean, ByVal CardTable As EuchreTable)
        NineOfHeartsVariation = UseNineOfHeartsVariation
        If NineOfHeartsVariation Then
            MaxCards = 21
        Else
            MaxCards = 24
        End If
        Table = CardTable
    End Sub
    ''' <summary>
    ''' Set up the deck so that the proper images are used (including card-back images).  Takes "nine of hearts"
    ''' rule into account when generating images (so we don't generate images for unused cards).  Sets
    ''' the current card to be the first card.  No-op if we've already initialized the deck.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
        If Inited = False Then
            ' Much easier to allocate four cardback images once than to allocate & rotate as-needed.
            EuchreCard.CardBackImages(EuchrePlayer.Seats.Player) = My.Resources.CARDBACK
            EuchreCard.CardBackImages(EuchrePlayer.Seats.LeftOpponent) = EuchreCard.CardBackImages(EuchrePlayer.Seats.Player).Clone
            EuchreCard.CardBackImages(EuchrePlayer.Seats.LeftOpponent).RotateFlip(RotateFlipType.Rotate90FlipNone)
            EuchreCard.CardBackImages(EuchrePlayer.Seats.RightOpponent) = EuchreCard.CardBackImages(EuchrePlayer.Seats.Player).Clone
            EuchreCard.CardBackImages(EuchrePlayer.Seats.RightOpponent).RotateFlip(RotateFlipType.Rotate270FlipNone)
            EuchreCard.CardBackImages(EuchrePlayer.Seats.Partner) = EuchreCard.CardBackImages(EuchrePlayer.Seats.Player).Clone
            EuchreCard.CardBackImages(EuchrePlayer.Seats.Partner).RotateFlip(RotateFlipType.Rotate180FlipNone)

            Dim NewSuit As EuchreCard.Suits
            Dim NewRank As EuchreCard.Ranks
            Dim n As Integer = 0
            For NewSuit = EuchreCard.Suits.Spades To EuchreCard.Suits.Hearts
                EuchreCard.SuitIm(NewSuit) = My.Resources.ResourceManager.GetObject(EuchreCard.GetSuitImageResourceName(NewSuit))
                For NewRank = EuchreCard.Ranks.Nine To EuchreCard.Ranks.Ace
                    If Not (NineOfHeartsVariation AndAlso NewRank = EuchreCard.Ranks.Nine AndAlso NewSuit <> EuchreCard.Suits.Hearts) Then
                        Cards(n) = New EuchreCard(NewRank, NewSuit, Table)
                        Cards(n).im = My.Resources.ResourceManager.GetObject(Cards(n).GetImageResourceName())
                        n = n + 1
                    End If
                Next NewRank
            Next NewSuit
            Cards(n) = Nothing
        End If
        Inited = True
        TopCard = 0
    End Sub

    ''' <summary>
    ''' Gets the next card in the deck by updating the appropriate index.  If deck is not initialized, we
    ''' initialize and shuffle it via calls.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetNextCard() As EuchreCard
        If Inited = False Then
            Shuffle()
        End If
        GetNextCard = Cards(TopCard)
        TopCard = TopCard + 1
    End Function

    ''' <summary>
    ''' A simple shuffling routine which trades the hole with a card, moving each card along.  If the
    ''' deck is not initialized, we do that first via a call.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Shuffle()
        If Inited = False Then
            Initialize()
        End If

        Dim Hole As Integer = MaxCards
        Dim i As Integer
        For i = 0 To MaxCards
            Dim Target As Integer
            If i < MaxCards Then
                Do
                    Target = EuchreTable.GenRandomNumber(MaxCards + 1)
                Loop While Target = Hole
            Else
                Target = MaxCards
            End If
            Cards(Hole) = Cards(Target)
            Cards(Target) = Cards(i)
            Hole = i
        Next i
        Cards(Hole) = Nothing
        TopCard = 0
    End Sub
#End Region
#Region "Private variables"
    Private NineOfHeartsVariation As Boolean
    Private MaxCards As Integer
    Private Cards(25) As EuchreCard ' The 24 (max) cards, plus a hole used for shuffling.
    Private Inited As Boolean
    Private TopCard As Integer
    Private Table As EuchreTable
#End Region
End Class