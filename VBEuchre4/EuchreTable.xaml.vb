Imports System
Imports System.Threading
Imports System.Windows.Threading
Imports System.Text
Imports System.IO


Class EuchreTable
#Region "Shared Methods"

    ''' <summary>
    ''' This method simulates returns a value from 0 to n-1. The input parameter is the 
    ''' number of sides of the dice.
    ''' This code is taken straight from the MSDN topic on RNGCryptoServiceProvider()
    ''' </summary>
    ''' <param name="NumSides">Number of possible values which can be chosen</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GenRandomNumber(ByVal NumSides As Integer) As Integer
        ' Create a byte array to hold the random value.
        Dim randomNumber(0) As Byte

        ' Create a new instance of the RNGCryptoServiceProvider. 
        Dim Gen As New Security.Cryptography.RNGCryptoServiceProvider()

        ' Fill the array with a random value.
        Gen.GetBytes(randomNumber)

        ' Convert the byte to an integer value to make the modulus operation easier.
        Dim rand As Integer = Convert.ToInt32(randomNumber(0))

        ' Return the random number mod the number
        ' of sides.  The possible values are zero-
        ' based.
        Return rand Mod NumSides
    End Function

    ''' <summary>
    ''' Returns the resource name of the image which displays a given score value.
    ''' </summary>
    ''' <param name="prefix">Which team's resource to use</param>
    ''' <param name="value">The team's score</param>
    ''' <returns>Resource name of the image</returns>
    ''' <remarks>No error checking; private method with known trusted inputs</remarks>
    Private Shared Function GetScoreResourceName(ByVal prefix As ScorePrefix, ByVal value As Integer) As String
        Dim Score As New StringBuilder()
        Select Case prefix
            Case ScorePrefix.ScoreThem
                Score.Append("SCOREThem")
            Case ScorePrefix.ScoreUs
                Score.Append("SCOREUs")
        End Select

        Score.Append(value.ToString())
        Return Score.ToString
    End Function
    ''' <summary>
    ''' Sets an image control to an in-memory resource, using streams
    ''' </summary>
    ''' <param name="Img">The image to set the image on</param>
    ''' <param name="Res">The bitmap resource</param>
    ''' <remarks></remarks>
    Public Shared Sub SetImage(ByVal Img As System.Windows.Controls.Image, ByVal res As System.Drawing.Image)
        ' Found this trick at http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/833ca60f-6a11-4836-bb2b-ef779dfe3ff0/
        Dim bmpImage As New BitmapImage
        bmpImage.BeginInit()
        Dim memStream As New MemoryStream
        res.Save(memStream, System.Drawing.Imaging.ImageFormat.Bmp)
        memStream.Seek(0, SeekOrigin.Begin)
        bmpImage.StreamSource = memStream
        bmpImage.EndInit()
        Img.Source = bmpImage
    End Sub
    ''' <summary>
    ''' Sets an icon to an in-memory resource, using streams
    ''' </summary>
    ''' <param name="win">The window to set the icon on</param>
    ''' <param name="Res">The icon resource</param>
    ''' <remarks></remarks>
    Public Shared Sub SetIcon(ByVal win As System.Windows.Window, ByVal res As System.Drawing.Icon)
        ' Found this trick at http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/833ca60f-6a11-4836-bb2b-ef779dfe3ff0/
        Dim bmpImage As New BitmapImage
        bmpImage.BeginInit()
        Dim memStream As New MemoryStream
        res.Save(memStream)
        memStream.Seek(0, SeekOrigin.Begin)
        bmpImage.StreamSource = memStream
        bmpImage.EndInit()
        win.Icon = bmpImage
    End Sub
#End Region
#Region "Public Methods"
    ''' <summary>
    ''' Initialize the players and various user controls.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyBase.New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

        ' Initialize label array
        InitializeLabelArray()


        ' Generate the players
        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Players(i) = New EuchrePlayer(i)
        Next i

        ' Update the bid controls:
        BidControl.Table = Me
        BidControl2.Table = Me

        AddHandler Me.Closing, AddressOf Me.EuchreTable_Closing


    End Sub
    ''' <summary>
    ''' Speak the name of the suit selected
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Public Sub SpeakSuit(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Select Case TrumpSuit
                Case EuchreCard.Suits.Clubs
                    Players(seat).Voice.SayClubs()
                Case EuchreCard.Suits.Diamonds
                    Players(seat).Voice.SayDiamonds()
                Case EuchreCard.Suits.Hearts
                    Players(seat).Voice.SayHearts()
                Case EuchreCard.Suits.Spades
                    Players(seat).Voice.SaySpades()
            End Select
        End If
    End Sub
    ''' <summary>
    ''' Say "I pass!"
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Public Sub SpeakPass(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(seat).Voice.SayPass()
        End If
    End Sub
    ''' <summary>
    ''' Say "I'm going alone!"
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Public Sub SpeakAlone(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(seat).Voice.SayAlone()
        End If
    End Sub
    ''' <summary>
    ''' Say "Pick it up!"
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Public Sub SpeakPickItUp(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(seat).Voice.SayPickItUp()
        End If
    End Sub
    ''' <summary>
    ''' Say "I'll pick it up!"
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Public Sub SpeakIPickItUp(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(seat).Voice.SayIPickItUp()
        End If
    End Sub

    ''' <summary>
    ''' Changes the cursor for the player's hand
    ''' </summary>
    ''' <param name="SetHand">True if changing the cursor to a hand; False otherwise</param>
    ''' <remarks></remarks>
    Public Sub SetPlayerCursorToHand(ByVal SetHand As Boolean)
        For i As Integer = 0 To 4
            If SetHand Then
                TableTopCards(EuchrePlayer.Seats.Player, i).Cursor = Cursors.Hand
            Else
                TableTopCards(EuchrePlayer.Seats.Player, i).Cursor = CachedCursor
            End If
        Next
    End Sub

    ''' <summary>
    ''' Tells the game to proceed to a new state, while caching the old state.
    ''' It then invokes the MasterStateDirector to process the new state.
    ''' It also clears the "desiredStateAfterHumanClick," which should not be
    ''' relied upon after this call.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <remarks></remarks>
    Public Sub UpdateEuchreState(state As EuchreState)
        lastState = currentState
        currentState = state
        desiredStateAfterHumanClick = EuchreState.NoState

        ' Asynchronously push an event to the loop via Dispatcher BeginInvoke
        Dispatcher.BeginInvoke(New NextTableAction(AddressOf MasterStateDirector))
    End Sub

    ''' <summary>
    ''' Reverts to the previous state.  This is only used if the game is interrupted
    ''' by a request to state the new game.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RevertEuchreState()
        UpdateEuchreState(lastState)
        lastState = EuchreState.NoState
        ' Don't really need to change lastState; we're safe from infinite reversions because only a decision to not
        ' restart a game after all can do this, but do this anyway since it's meaningless
    End Sub

    Public Delegate Sub NextTableAction()

    ''' <summary>
    ''' This is the master delegate handler which chooses next action based on state
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub MasterStateDirector()
        Select Case currentState
            Case EuchreState.NoState
                ' Do nothing
                NoOp()

            Case EuchreState.StartNewGameRequested
                If GameStarted Then
                    If Not RestartGame() Then
                        RevertEuchreState()
                        Return
                    End If
                End If

                If NewGame() Then
                    UpdateEuchreState(EuchreState.StartNewGameConfirmed)
                Else
                    CleanUpGame()
                    UpdateEuchreState(EuchreState.NoState)
                End If

            Case EuchreState.StartNewGameConfirmed
                PreDealerSelection()
                UpdateEuchreState(EuchreState.StillSelectingDealer)
            Case EuchreState.StillSelectingDealer
                TrySelectDealer()
            Case EuchreState.DealerSelected
                PostDealerSelection(EuchreState.DealerAcknowledged)
            Case EuchreState.DealerAcknowledged
                PostDealerCleanup()
                UpdateEuchreState(EuchreState.ClearHand)

            Case EuchreState.ClearHand
                SetForNewHand()
                UpdateEuchreState(EuchreState.StartNewHand)
            Case EuchreState.StartNewHand
                UpdateEuchreState(EuchreState.DealCards)
            Case EuchreState.DealCards
                DealCards()
                TrickLeaderIndex = EuchrePlayer.NextPlayer(DealerThisHand)
                UpdateEuchreState(EuchreState.Bid1Starts)

            Case EuchreState.Bid1Starts
                PreBid1()
                UpdateEuchreState(EuchreState.Bid1Player0)
            Case EuchreState.Bid1Player0
                Bid1(EuchreState.Bid1Player1)
            Case EuchreState.Bid1Player1
                Bid1(EuchreState.Bid1Player2)
            Case EuchreState.Bid1Player2
                Bid1(EuchreState.Bid1Player3)
            Case EuchreState.Bid1Player3
                Bid1(EuchreState.Bid2Starts) ' We don't current show a continue dialog after the first round of failed bidding
            Case EuchreState.Bid1PickUp
                Bid1PickUp()
            Case EuchreState.Bid1PickedUp
                Bid1PickedUp()
                UpdateEuchreState(EuchreState.Bid1Succeeded)
            Case EuchreState.Bid1Succeeded
                SortAndSetHandImagesAndText()
                ShowAndEnableContinueButton(EuchreState.Bid1SucceededAcknowledged)
            Case EuchreState.Bid1SucceededAcknowledged
                UpdateEuchreState(EuchreState.Trick0Started)
            Case EuchreState.Bid1Failed ' Currently unused state
                NoOp()
            Case EuchreState.Bid1FailedAcknowledged ' Currently unused state
                NoOp()

            Case EuchreState.Bid2Starts
                PreBid2()
                UpdateEuchreState(EuchreState.Bid2Player0)
            Case EuchreState.Bid2Player0
                Bid2(EuchreState.Bid2Player1)
            Case EuchreState.Bid2Player1
                Bid2(EuchreState.Bid2Player2)
            Case EuchreState.Bid2Player2
                Bid2(EuchreState.Bid2Player3)
            Case EuchreState.Bid2Player3
                Bid2(EuchreState.Bid2Failed)
            Case EuchreState.Bid2Succeeded
                ShowAndEnableContinueButton(EuchreState.Bid2SucceededAcknowledged)

            Case EuchreState.Bid2SucceededAcknowledged
                SortAndSetHandImagesAndText()
                UpdateEuchreState(EuchreState.Trick0Started)

            Case EuchreState.Bid2Failed
                Me.UpdateStatus(My.Resources.Notice_AllPassedTwice)
                ShowAndEnableContinueButton(EuchreState.Bid2FailedAcknowledged)

            Case EuchreState.Bid2FailedAcknowledged
                SetNextDealer() ' Nobody bid; move to the next dealer
                UpdateEuchreState(EuchreState.ClearHand)

            Case EuchreState.Trick0Started
                UpdateLayout()
                PrepTrick()
                UpdateEuchreState(EuchreState.Trick0_SelectCard0)
            Case EuchreState.Trick0_SelectCard0
                SelectCardForTrick(EuchreState.Trick0_PlayCard0)
            Case EuchreState.Trick0_PlayCard0
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick0_SelectCard1)
            Case EuchreState.Trick0_SelectCard1
                SelectCardForTrick(EuchreState.Trick0_PlayCard1)
            Case EuchreState.Trick0_PlayCard1
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick0_SelectCard2)
            Case EuchreState.Trick0_SelectCard2
                SelectCardForTrick(EuchreState.Trick0_PlayCard2)
            Case EuchreState.Trick0_PlayCard2
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick0_SelectCard3)
            Case EuchreState.Trick0_SelectCard3
                SelectCardForTrick(EuchreState.Trick0_PlayCard3)
            Case EuchreState.Trick0_PlayCard3
                PlayCardForTrick()
                PostTrick()
                ShowAndEnableContinueButton(EuchreState.Trick0EndingAcknowledged)
            Case EuchreState.Trick0Ended
                NoOp() ' Currently unused state
            Case EuchreState.Trick0EndingAcknowledged
                UpdateEuchreState(EuchreState.Trick1Started)

            Case EuchreState.Trick1Started
                UpdateLayout()
                PrepTrick()
                UpdateEuchreState(EuchreState.Trick1_SelectCard0)
            Case EuchreState.Trick1_SelectCard0
                SelectCardForTrick(EuchreState.Trick1_PlayCard0)
            Case EuchreState.Trick1_PlayCard0
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick1_SelectCard1)
            Case EuchreState.Trick1_SelectCard1
                SelectCardForTrick(EuchreState.Trick1_PlayCard1)
            Case EuchreState.Trick1_PlayCard1
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick1_SelectCard2)
            Case EuchreState.Trick1_SelectCard2
                SelectCardForTrick(EuchreState.Trick1_PlayCard2)
            Case EuchreState.Trick1_PlayCard2
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick1_SelectCard3)
            Case EuchreState.Trick1_SelectCard3
                SelectCardForTrick(EuchreState.Trick1_PlayCard3)
            Case EuchreState.Trick1_PlayCard3
                PlayCardForTrick()
                PostTrick()
                ShowAndEnableContinueButton(EuchreState.Trick1EndingAcknowledged)
            Case EuchreState.Trick1Ended
                NoOp() ' Currently unused state
            Case EuchreState.Trick1EndingAcknowledged
                UpdateEuchreState(EuchreState.Trick2Started)

            Case EuchreState.Trick2Started
                UpdateLayout()
                PrepTrick()
                UpdateEuchreState(EuchreState.Trick2_SelectCard0)
            Case EuchreState.Trick2_SelectCard0
                SelectCardForTrick(EuchreState.Trick2_PlayCard0)
            Case EuchreState.Trick2_PlayCard0
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick2_SelectCard1)
            Case EuchreState.Trick2_SelectCard1
                SelectCardForTrick(EuchreState.Trick2_PlayCard1)
            Case EuchreState.Trick2_PlayCard1
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick2_SelectCard2)
            Case EuchreState.Trick2_SelectCard2
                SelectCardForTrick(EuchreState.Trick2_PlayCard2)
            Case EuchreState.Trick2_PlayCard2
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick2_SelectCard3)
            Case EuchreState.Trick2_SelectCard3
                SelectCardForTrick(EuchreState.Trick2_PlayCard3)
            Case EuchreState.Trick2_PlayCard3
                PlayCardForTrick()
                PostTrick()
                ShowAndEnableContinueButton(EuchreState.Trick2EndingAcknowledged)
            Case EuchreState.Trick2Ended
                NoOp() ' Currently unused state
            Case EuchreState.Trick2EndingAcknowledged
                UpdateEuchreState(EuchreState.Trick3Started)

            Case EuchreState.Trick3Started
                UpdateLayout()
                PrepTrick()
                UpdateEuchreState(EuchreState.Trick3_SelectCard0)
            Case EuchreState.Trick3_SelectCard0
                SelectCardForTrick(EuchreState.Trick3_PlayCard0)
            Case EuchreState.Trick3_PlayCard0
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick3_SelectCard1)
            Case EuchreState.Trick3_SelectCard1
                SelectCardForTrick(EuchreState.Trick3_PlayCard1)
            Case EuchreState.Trick3_PlayCard1
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick3_SelectCard2)
            Case EuchreState.Trick3_SelectCard2
                SelectCardForTrick(EuchreState.Trick3_PlayCard2)
            Case EuchreState.Trick3_PlayCard2
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick3_SelectCard3)
            Case EuchreState.Trick3_SelectCard3
                SelectCardForTrick(EuchreState.Trick3_PlayCard3)
            Case EuchreState.Trick3_PlayCard3
                PlayCardForTrick()
                PostTrick()
                ShowAndEnableContinueButton(EuchreState.Trick3EndingAcknowledged)
            Case EuchreState.Trick3Ended
                NoOp() ' Currently unused state
            Case EuchreState.Trick3EndingAcknowledged
                UpdateEuchreState(EuchreState.Trick4Started)

            Case EuchreState.Trick4Started
                UpdateLayout()
                PrepTrick()
                UpdateEuchreState(EuchreState.Trick4_SelectCard0)
            Case EuchreState.Trick4_SelectCard0
                SelectCardForTrick(EuchreState.Trick4_PlayCard0)
            Case EuchreState.Trick4_PlayCard0
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick4_SelectCard1)
            Case EuchreState.Trick4_SelectCard1
                SelectCardForTrick(EuchreState.Trick4_PlayCard1)
            Case EuchreState.Trick4_PlayCard1
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick4_SelectCard2)
            Case EuchreState.Trick4_SelectCard2
                SelectCardForTrick(EuchreState.Trick4_PlayCard2)
            Case EuchreState.Trick4_PlayCard2
                PlayCardForTrick()
                UpdateEuchreState(EuchreState.Trick4_SelectCard3)
            Case EuchreState.Trick4_SelectCard3
                SelectCardForTrick(EuchreState.Trick4_PlayCard3)
            Case EuchreState.Trick4_PlayCard3
                PlayCardForTrick()
                PostTrick()
                ShowAndEnableContinueButton(EuchreState.Trick4EndingAcknowledged)
            Case EuchreState.Trick4Ended
                NoOp() ' Currently unused state
            Case EuchreState.Trick4EndingAcknowledged
                UpdateEuchreState(EuchreState.HandCompleted)

            Case EuchreState.HandCompleted
                UpdateAllScores(EuchreState.HandCompletedAcknowledged)

            Case EuchreState.HandCompletedAcknowledged
                CleanupAfterHand()
                If i_TheirScore >= 10 OrElse i_YourScore >= 10 Then
                    UpdateEuchreState(EuchreState.GameOver)
                Else
                    SetNextDealer()
                    UpdateEuchreState(EuchreState.ClearHand)
                End If

            Case EuchreState.GameOver
                DetermineWinnerAndEndGame()
                UpdateEuchreState(EuchreState.NoState)

        End Select

    End Sub


    ''' <summary>
    ''' If a game is underway, prompts the user to see if he/she really wants to start a new game. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Function RestartGame() As Boolean
        If MessageBox.Show(My.Resources.Command_New, My.Resources.Command_NewTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) <> MessageBoxResult.OK Then
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Updates the status area with the given message.
    ''' </summary>
    ''' <param name="s">The string to be displayed</param>
    ''' <param name="WhiteSpace">The number of carriage returns to insert after the text</param>
    ''' <remarks></remarks>
    Public Sub UpdateStatus(ByVal s As String, Optional ByVal WhiteSpace As Integer = 1)
        Me.StatusArea.AppendText(s)
        If WhiteSpace > 0 Then
            For i As Integer = 1 To WhiteSpace
                Me.StatusArea.AppendText(vbCrLf)
                Me.StatusArea.ScrollToEnd()
            Next
        End If
        Me.StatusArea.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Helper method to enable or disable a player's cards
    ''' </summary>
    ''' <param name="player">The player whose cards should be enabled or disabled</param>
    ''' <param name="EnableIt">True if enabling the cards; False if disabling them</param>
    ''' <remarks></remarks>
    Public Sub EnableCards(ByVal player As EuchrePlayer.Seats, ByVal EnableIt As Boolean)
        For i As Integer = 0 To 4
            TableTopCards(player, i).IsEnabled = EnableIt
            TableTopCards(player, i).Opacity = If(EnableIt, 1.0, 0.25)
        Next
    End Sub

    ''' <summary>
    ''' Helper method to mark a card as played.
    ''' </summary>
    ''' <param name="card"></param>
    ''' <remarks></remarks>
    Public Sub MarkCardAsPlayed(ByVal card As EuchreCard)
        CardsPlayedThisHand(PlayedCardIndex) = card
        PlayedCardIndex = PlayedCardIndex + 1
    End Sub

    ''' <summary>
    ''' Helper method to clear all of the played cards.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ResetPlayedCards()
        For i As Integer = 0 To 23
            CardsPlayedThisHand(i) = Nothing
        Next
        PlayedCardIndex = 0
    End Sub

#End Region
#Region "Private methods"
    ''' <summary>
    ''' Initializes all of the arrays that we use to keep track of various objects on the tables.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitializeLabelArray()
        ' Yuck.

        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 0) = Me.LeftOpponentCard1
        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 1) = Me.LeftOpponentCard2
        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 2) = Me.LeftOpponentCard3
        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 3) = Me.LeftOpponentCard4
        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 4) = Me.LeftOpponentCard5
        Me.TableTopCards(EuchrePlayer.Seats.LeftOpponent, 5) = Me.LeftOpponentCard

        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 0) = Me.RightOpponentCard1
        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 1) = Me.RightOpponentCard2
        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 2) = Me.RightOpponentCard3
        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 3) = Me.RightOpponentCard4
        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 4) = Me.RightOpponentCard5
        Me.TableTopCards(EuchrePlayer.Seats.RightOpponent, 5) = Me.RightOpponentCard

        Me.TableTopCards(EuchrePlayer.Seats.Player, 0) = Me.PlayerCard1
        Me.TableTopCards(EuchrePlayer.Seats.Player, 1) = Me.PlayerCard2
        Me.TableTopCards(EuchrePlayer.Seats.Player, 2) = Me.PlayerCard3
        Me.TableTopCards(EuchrePlayer.Seats.Player, 3) = Me.PlayerCard4
        Me.TableTopCards(EuchrePlayer.Seats.Player, 4) = Me.PlayerCard5
        Me.TableTopCards(EuchrePlayer.Seats.Player, 5) = Me.PlayerCard

        Me.TableTopCards(EuchrePlayer.Seats.Partner, 0) = Me.PartnerCard1
        Me.TableTopCards(EuchrePlayer.Seats.Partner, 1) = Me.PartnerCard2
        Me.TableTopCards(EuchrePlayer.Seats.Partner, 2) = Me.PartnerCard3
        Me.TableTopCards(EuchrePlayer.Seats.Partner, 3) = Me.PartnerCard4
        Me.TableTopCards(EuchrePlayer.Seats.Partner, 4) = Me.PartnerCard5
        Me.TableTopCards(EuchrePlayer.Seats.Partner, 5) = Me.PartnerCard

        Me.DealerBox(EuchrePlayer.Seats.LeftOpponent) = Me.DealerLeftOpponent
        Me.DealerBox(EuchrePlayer.Seats.RightOpponent) = Me.DealerRightOpponent
        Me.DealerBox(EuchrePlayer.Seats.Partner) = Me.DealerPartner
        Me.DealerBox(EuchrePlayer.Seats.Player) = Me.DealerPlayer
    End Sub

    ''' <summary>
    ''' Apologize for getting Euchred
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotEuchredMyFault(ByVal seat As EuchrePlayer.Seats)
        If seat <> EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(seat).Voice.SayWeGotEuchredMyFault(Players(Players(seat).OppositeSeat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' Apologize for not helping to stop getting Euchred.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotEuchredOurFault(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeGotEuchredOurFault(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' Chide player for going alone and losing.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotEuchredYourFault(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeGotEuchredYourFault(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 1 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotOne(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeGotOne(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 2 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotTwo(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeGotTwo(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 4 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeGotFour(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeGotFour(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 1 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakMeGotOne(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayMeGotOne(Players(EuchrePlayer.Seats.Player).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 2 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakMeGotTwo(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayMeGotTwo(Players(EuchrePlayer.Seats.Player).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' 4 point win.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakMeGotFour(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayMeGotFour(Players(EuchrePlayer.Seats.Player).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' Congratulations.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeWon(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeWon(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' Commiserations.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakTheyWon(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayTheyWon(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' We euchred them.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeEuchredThem(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeEuchredThem(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' We super euchred.  Note that this uses the seat information
    ''' in reverse from the other "speak" methods
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakWeSuperEuchredThem(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Player AndAlso SoundOn Then
            Players(Players(seat).OppositeSeat).Voice.SayWeSuperEuchredThem(Players(seat).GetDisplayName(Me))
        End If
    End Sub

    ''' <summary>
    ''' They got one point.
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakTheyGotOne(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayTheyGotOne()
        End If
    End Sub

    ''' <summary>
    ''' They got two points
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakTheyGotTwo(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayTheyGotTwo()
        End If
    End Sub

    ''' <summary>
    ''' They got four points
    ''' </summary>
    ''' <param name="seat"></param>
    ''' <remarks></remarks>
    Private Sub SpeakTheyGotFour(ByVal seat As EuchrePlayer.Seats)
        If seat = EuchrePlayer.Seats.Partner AndAlso SoundOn Then
            Players(seat).Voice.SayTheyGotFour()
        End If
    End Sub

    ''' <summary>
    ''' Synchronously plays the sound of a card being placed on the table.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PlayCardSound()
        If SoundOn Then
            My.Computer.Audio.Play(My.Resources.SoundPlayCard, AudioPlayMode.WaitToComplete)
        End If
    End Sub

    ''' <summary>
    ''' Plays a blocking applause sound.  (Non-blocking sounds appear not to mix well with text-to-speech, creating static.)
    ''' </summary>
    ''' <param name="level">Pass 1 for soft applause, 2 for loud applause, and 3 for wild applause.
    ''' All other values are ignored.</param>
    ''' <remarks></remarks>
    Private Sub PlayApplause(ByVal level As Integer)
        If SoundOn Then
            Select Case level
                Case 1
                    My.Computer.Audio.Play(My.Resources.SoundApplauseSoft, AudioPlayMode.WaitToComplete)
                Case 2
                    My.Computer.Audio.Play(My.Resources.SoundApplauseLoud, AudioPlayMode.WaitToComplete)
                Case 3
                    My.Computer.Audio.Play(My.Resources.SoundApplauseWild, AudioPlayMode.WaitToComplete)
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Plays 2 - 5 card-shuffling sounds.  The sounds are played synchronously.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PlayShuffleSound()
        Me.UpdateLayout()
        Me.UpdateStatus(My.Resources.Notice_ShufflingCards)
        If SoundOn Then
            Dim numShuffle As Integer = EuchreTable.GenRandomNumber(2) + 1
            For i As Integer = 0 To numShuffle
                My.Computer.Audio.Play(My.Resources.SoundShuffleDeck, AudioPlayMode.WaitToComplete)
            Next i
        End If
        Me.UpdateStatus(My.Resources.Notice_DealingCards)
    End Sub

    ''' <summary>
    ''' Updates the score text and images for the teams.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateScoreText()
        ' Update the textual score.
        Dim sTheirScore As New StringBuilder()
        sTheirScore.AppendFormat(My.Resources.Format_TheirScore, i_TheirScore)
        TheirScore.Content = sTheirScore.ToString
        TheirScore.UpdateLayout()

        Dim sYourScore As New StringBuilder()
        sYourScore.AppendFormat(My.Resources.Format_YourScore, i_YourScore)
        YourScore.Content = sYourScore.ToString
        YourScore.UpdateLayout()

        ' Now, update the score card images. (They only go up to 10, so normalize the score.)
        If i_TheirScore > 10 Then
            i_TheirScore = 10
        End If
        If i_YourScore > 10 Then
            i_YourScore = 10
        End If

        SetImage(Me.ThemScore, My.Resources.ResourceManager.GetObject(GetScoreResourceName(ScorePrefix.ScoreThem, i_TheirScore)))
        SetImage(Me.UsScore, My.Resources.ResourceManager.GetObject(GetScoreResourceName(ScorePrefix.ScoreUs, i_YourScore)))
        Me.UsScore.UpdateLayout()
        Me.ThemScore.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Updates the test indicating how many tracks have been taken by the teams.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateTricksText()
        Dim sTheirTricks As New StringBuilder()
        sTheirTricks.AppendFormat(My.Resources.Format_TheirTricks, i_TheirTricks)
        TheirTricks.Content = sTheirTricks.ToString
        TheirTricks.UpdateLayout()

        Dim sYourTricks As New StringBuilder()
        sYourTricks.AppendFormat(My.Resources.Format_YourTricks, i_YourTricks)
        YourTricks.Content = sYourTricks.ToString
        YourTricks.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Determines if the non-user team picked trump this hand.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function TheirTeamPickedTrumpThisHand() As Boolean
        If PickedTrumpThisHand = EuchrePlayer.Seats.LeftOpponent OrElse PickedTrumpThisHand = EuchrePlayer.Seats.RightOpponent Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Determines if the user's team picked trump this hand.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function YourTeamPickedTrumpThisHand() As Boolean
        If PickedTrumpThisHand = EuchrePlayer.Seats.Player OrElse PickedTrumpThisHand = EuchrePlayer.Seats.Partner Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Determines if the non-user team went alone this hand.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function TheirTeamWentAloneThisHand() As Boolean
        If Players(EuchrePlayer.Seats.LeftOpponent).SittingOutThisHand OrElse Players(EuchrePlayer.Seats.RightOpponent).SittingOutThisHand Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Determines if the user's team went alone this hand.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function YourTeamWentAloneThisHand() As Boolean
        If Players(EuchrePlayer.Seats.Player).SittingOutThisHand OrElse Players(EuchrePlayer.Seats.Partner).SittingOutThisHand Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Updates the scores for both teams, calls methods to update the appropriate text and images
    ''' on the table, and pops up a "continue" box so that the user can look at the table before it
    ''' clears.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateAllScores(nextState As EuchreState)
        ' Called after each hand
        Dim TheirTotalTricks As Integer = Players(EuchrePlayer.Seats.LeftOpponent).TricksWonThisHand + Players(EuchrePlayer.Seats.RightOpponent).TricksWonThisHand
        Dim YourTotalTricks As Integer = Players(EuchrePlayer.Seats.Player).TricksWonThisHand + Players(EuchrePlayer.Seats.Partner).TricksWonThisHand
        Select Case PickedTrumpThisHand
            Case EuchrePlayer.Seats.LeftOpponent, EuchrePlayer.Seats.RightOpponent
                If TheirTotalTricks = 0 AndAlso Me.UseSuperEuchreRule Then
                    i_YourScore = i_YourScore + 4 ' SuperEuchred!
                    Me.UpdateStatus(My.Resources.Notice_YouSuperEuchredThem)
                    Me.PlayApplause(3)
                    Me.SpeakWeSuperEuchredThem(EuchrePlayer.Seats.Player)
                ElseIf TheirTotalTricks < 3 Then
                    i_YourScore = i_YourScore + 2 ' Euchred!
                    Me.UpdateStatus(My.Resources.Notice_YouEuchredThem)
                    Me.PlayApplause(2)
                    Me.SpeakWeEuchredThem(EuchrePlayer.Seats.Player)
                ElseIf TheirTotalTricks = 5 Then
                    If TheirTeamWentAloneThisHand() Then
                        i_TheirScore = i_TheirScore + 4
                        Me.UpdateStatus(My.Resources.Notice_TheyWonTheHandAllTricksAlone)
                        Me.SpeakTheyGotFour(EuchrePlayer.Seats.Partner)
                    Else
                        i_TheirScore = i_TheirScore + 2
                        Me.UpdateStatus(My.Resources.Notice_TheyWonTheHandAllTricks)
                        Me.SpeakTheyGotTwo(EuchrePlayer.Seats.Partner)
                    End If
                Else
                    i_TheirScore = i_TheirScore + 1
                    Me.UpdateStatus(My.Resources.Notice_TheyWonTheHand)
                    Me.SpeakTheyGotOne(EuchrePlayer.Seats.Partner)
                End If
            Case EuchrePlayer.Seats.Player, EuchrePlayer.Seats.Partner
                If YourTotalTricks = 0 AndAlso Me.UseSuperEuchreRule Then
                    i_TheirScore = i_TheirScore + 4 ' SuperEuchred!
                    Me.UpdateStatus(My.Resources.Notice_TheySuperEuchredYou)
                    If PickedTrumpThisHand = EuchrePlayer.Seats.Partner Then
                        Me.SpeakWeGotEuchredMyFault(PickedTrumpThisHand)
                    ElseIf Not YourTeamWentAloneThisHand() Then
                        Me.SpeakWeGotEuchredOurFault(PickedTrumpThisHand)
                    Else
                        Me.SpeakWeGotEuchredYourFault(PickedTrumpThisHand)
                    End If
                ElseIf YourTotalTricks < 3 Then
                    i_TheirScore = i_TheirScore + 2 ' Euchred!
                    Me.UpdateStatus(My.Resources.Notice_TheyEuchredYou)
                    If PickedTrumpThisHand = EuchrePlayer.Seats.Partner Then
                        Me.SpeakWeGotEuchredMyFault(PickedTrumpThisHand)
                    ElseIf Not YourTeamWentAloneThisHand() Then
                        Me.SpeakWeGotEuchredOurFault(PickedTrumpThisHand)
                    Else
                        Me.SpeakWeGotEuchredYourFault(PickedTrumpThisHand)
                    End If
                ElseIf YourTotalTricks = 5 Then
                    If YourTeamWentAloneThisHand() Then
                        i_YourScore = i_YourScore + 4
                        Me.UpdateStatus(My.Resources.Notice_YouWonTheHandAllTricksAlone)
                        Me.PlayApplause(3)
                        If PickedTrumpThisHand = EuchrePlayer.Seats.Player Then
                            Me.SpeakWeGotFour(PickedTrumpThisHand)
                        Else
                            Me.SpeakMeGotFour(PickedTrumpThisHand)
                        End If
                    Else
                        i_YourScore = i_YourScore + 2
                        Me.UpdateStatus(My.Resources.Notice_YouWonTheHandAllTricks)
                        Me.PlayApplause(2)
                        If PickedTrumpThisHand = EuchrePlayer.Seats.Player Then
                            Me.SpeakWeGotTwo(PickedTrumpThisHand)
                        Else
                            Me.SpeakMeGotTwo(PickedTrumpThisHand)
                        End If
                    End If
                Else
                    i_YourScore = i_YourScore + 1
                    Me.UpdateStatus(My.Resources.Notice_YouWonTheHand)
                    Me.PlayApplause(1)
                    If PickedTrumpThisHand = EuchrePlayer.Seats.Player Then
                        Me.SpeakWeGotOne(PickedTrumpThisHand)
                    Else
                        Me.SpeakMeGotOne(PickedTrumpThisHand)
                    End If
                End If
        End Select
        UpdateScoreText()
        ShowAndEnableContinueButton(nextState)
    End Sub

    ''' <summary>
    ''' Helper function to erase all played cards from the table, clear the played card array,
    ''' and remove all tooltips associated with the corresponding labels.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub HideAllPlayedCards()
        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Me.TableTopCards(i, 5).Source = Nothing
            SetUIElementVisibility(Me.TableTopCards(i, 5), Visibility.Hidden)
            SetTooltip(Me.TableTopCards(i, 5), Nothing)
            PlayedCards(i) = Nothing
        Next
    End Sub

    ''' <summary>
    ''' Helper function to show or hide all cards on the table (except the fake ones used to keep score).
    ''' Does not affect any data structures.
    ''' </summary>
    ''' <param name="ShowThem">True to show cards, False to hide them</param>
    ''' <remarks></remarks>
    Private Sub ShowAllCards(ByVal ShowThem As Boolean)
        Dim visible As Visibility = If(ShowThem, Visibility.Visible, Visibility.Hidden)
        Dim visibleNoH As Visibility = If(ShowThem AndAlso Not Me.UseNineOfHeartsRule, Visibility.Visible, Visibility.Hidden)
        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            For j As Integer = 0 To 4
                SetUIElementVisibility(Me.TableTopCards(i, j), visible)
            Next j
        Next i

        SetUIElementVisibility(KittyCard1, visible)
        SetUIElementVisibility(KittyCard2, visibleNoH)
        SetUIElementVisibility(KittyCard3, visibleNoH)
        SetUIElementVisibility(KittyCard4, visibleNoH)

    End Sub

    ''' <summary>
    ''' Helper function to reset all elements on the table to their initial state.  
    ''' Useful when starting a new game in the middle of a prompt.
    ''' Does not affect any data structures.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ResetScores()
        ' Clear the text.
        i_TheirScore = 0
        i_YourScore = 0
        UpdateScoreText()
        i_TheirTricks = 0
        i_YourTricks = 0
        UpdateTricksText()
    End Sub

    ''' <summary>
    ''' Helper function to set a UIElement's visibility
    ''' </summary>
    ''' <param name="uie"></param>
    ''' <param name="visible"></param>
    Private Sub SetUIElementVisibility(ByVal uie As UIElement, ByVal visible As Visibility)
        uie.Visibility = visible
        uie.UpdateLayout()
    End Sub

    ''' <summary>
    ''' Helper function to completely hide a UI element synchronously
    ''' </summary>
    ''' <param name="uie"></param>
    Private Sub HideAndDisableUIElement(ByVal uie As UIElement)
        uie.IsEnabled = False
        uie.IsHitTestVisible = False
        SetUIElementVisibility(uie, Visibility.Hidden)
        uie.UpdateLayout()
        Refresh(uie)
    End Sub

    ''' <summary>
    ''' Helper function to reset all elements on the table to their initial state.  
    ''' Useful when starting a new game in the middle of a prompt.
    ''' Does not affect any data structures.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ResetUserInputStates()
        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            For j As Integer = 0 To 4
                Me.TableTopCards(i, j).IsEnabled = True
                ' Undim the cards
                Me.TableTopCards(i, j).Opacity = 1.0
                Me.TableTopCards(i, j).UpdateLayout()
            Next j
        Next i

        HideAndDisableUIElement(BidControl)
        HideAndDisableUIElement(BidControl2)
        HideAndDisableUIElement(ContinueButton)

        Me.PlayerIsDroppingACard = False
        Me.PlayerIsPlayingACard = False

        SetUIElementVisibility(SelectLabel, Visibility.Hidden)
        Me.SetPlayerCursorToHand(False)

    End Sub

    ''' <summary>
    ''' Helper function which updates the required perspective (rotation) state of each card in the 
    ''' user's hand, points the array to the appropriate card images, and sets the tooltip for
    ''' the cards.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetAllPlayerCardImages()
        For i As Integer = 0 To 4
            Players(EuchrePlayer.Seats.Player).CardsHeldThisHand(i).Perspective() = EuchrePlayer.Seats.Player
            SetImage(Me.TableTopCards(EuchrePlayer.Seats.Player, i), Players(EuchrePlayer.Seats.Player).CardsHeldThisHand(i).GetImage(EuchrePlayer.Seats.Player))
            SetTooltip(Me.TableTopCards(EuchrePlayer.Seats.Player, i), My.Resources.ResourceManager.GetString(Players(EuchrePlayer.Seats.Player).CardsHeldThisHand(i).GetDisplayStringResourceName))
            Me.TableTopCards(EuchrePlayer.Seats.Player, i).UpdateLayout()
        Next
    End Sub

    ''' <summary>
    ''' Sets a tooltip to a particular string for a particular Image
    ''' </summary>
    ''' <param name="Img">The image to set the tooltip on</param>
    ''' <param name="Tip"></param>
    ''' <remarks></remarks>
    Private Sub SetTooltip(ByVal Img As System.Windows.Controls.Image, ByVal Tip As String)
        Img.ToolTip = Tip
    End Sub

    ''' <summary>
    ''' Calls a method to clear any played cards from the table, and then updates the images/perspectives/tooltips for all
    ''' cards in the hands and in the kitty.  This is typically called after sorting hands, to make sure that all cards
    ''' are in the correct place, with the correct perspective.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetAllCardImages()
        HideAllPlayedCards()
        SetAllPlayerCardImages()

        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.RightOpponent
            For j As Integer = 0 To 4
                Players(i).CardsHeldThisHand(j).Perspective() = i
                SetImage(Me.TableTopCards(i, j), Players(i).CardsHeldThisHand(j).GetImage(i))
                Me.TableTopCards(i, j).UpdateLayout()
                SetTooltip(Me.TableTopCards(i, j), My.Resources.ResourceManager.GetString(Players(i).CardsHeldThisHand(j).GetDisplayStringResourceName(TrumpSuit)))
            Next j
        Next i

        SetTooltip(Me.KittyCard1, My.Resources.ResourceManager.GetString(Kitty(0).GetDisplayStringResourceName))
        Kitty(0).Perspective = EuchrePlayer.Seats.Player
        SetImage(Me.KittyCard1, Kitty(0).GetImage(EuchrePlayer.Seats.NoPlayer))
        Me.KittyCard1.UpdateLayout()
        If UseNineOfHeartsRule = False Then
            SetTooltip(Me.KittyCard2, My.Resources.ResourceManager.GetString(Kitty(1).GetDisplayStringResourceName))
            SetTooltip(Me.KittyCard3, My.Resources.ResourceManager.GetString(Kitty(2).GetDisplayStringResourceName))
            SetTooltip(Me.KittyCard4, My.Resources.ResourceManager.GetString(Kitty(3).GetDisplayStringResourceName))
            Kitty(1).Perspective = EuchrePlayer.Seats.Player
            Kitty(2).Perspective = EuchrePlayer.Seats.Player
            Kitty(3).Perspective = EuchrePlayer.Seats.Player
            SetImage(Me.KittyCard2, Kitty(1).GetImage(EuchrePlayer.Seats.NoPlayer))
            SetImage(Me.KittyCard3, Kitty(2).GetImage(EuchrePlayer.Seats.NoPlayer))
            SetImage(Me.KittyCard4, Kitty(3).GetImage(EuchrePlayer.Seats.NoPlayer))
            Me.KittyCard2.UpdateLayout()
            Me.KittyCard3.UpdateLayout()
            Me.KittyCard4.UpdateLayout()
        End If
    End Sub

    ''' <summary>
    ''' Deals one card to a player.  The tooltip and image are also set, a sound is played,
    ''' and a brief pause is generated.
    ''' </summary>
    ''' <param name="player">The player to give the card to</param>
    ''' <param name="slot">The slot in the player's hand where the card should go</param>
    ''' <remarks></remarks>
    Private Sub DealACard(ByVal player As EuchrePlayer.Seats, ByVal slot As Integer)
        Players(player).CardsHeldThisHand(slot) = Deck.GetNextCard()
        If player <> EuchrePlayer.Seats.Player Then
            Players(player).CardsHeldThisHand(slot).State = EuchreCard.States.FaceDown
        Else
            Players(player).CardsHeldThisHand(slot).State = EuchreCard.States.FaceUp
        End If
        SetImage(Me.TableTopCards(player, slot), EuchreCard.CardBackImages(player))
        SetUIElementVisibility(Me.TableTopCards(player, slot), Visibility.Visible)
        SetTooltip(Me.TableTopCards(player, slot), My.Resources.CARDNAME_BACK)
        Me.PlayCardSound()
        RefreshAndSleep(Me.TableTopCards(player, slot))
    End Sub

    ''' <summary>
    ''' Hides any card images, calls to shuffle the cards, deals all of the cards to the players and the kitty,
    ''' calls to sort the cards according to the possible trump, and calls to show the cards.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DealCards()
        ShowAllCards(False)
        HideAllPlayedCards()

        Deck.Shuffle()
        PlayShuffleSound()

        Dim i As EuchrePlayer.Seats
        Dim j As Integer

        ' Deal the cards 3-2-3-2, 2-3-2-3:
        i = DealerThisHand
        Dim k As Integer = 0
        Dim m As Integer
        Dim n As Integer = 1
        Do While k < 2
            If k = 0 Then
                If n = 1 Then
                    n = 2
                Else
                    n = 1
                End If
            Else
                n = 4
            End If

            If k = 0 Then
                m = 0
            Else
                If m = 2 Then
                    m = 3
                Else
                    m = 2
                End If
            End If

            i = EuchrePlayer.NextPlayer(i)
            For j = m To n
                DealACard(i, j)
            Next
            If i = DealerThisHand Then
                k = k + 1
                m = 2
            End If
        Loop
        ' End of new way to deal

        For j = 0 To 3
            Kitty(j) = Deck.GetNextCard()
            If j = 0 Then
                Kitty(j).State = EuchreCard.States.FaceUp
            ElseIf Not Kitty(j) Is Nothing Then
                Kitty(j).State = EuchreCard.States.FaceDown
            End If
        Next j

        ' Sort the players' cards according to the possible trump
        Dim seat As EuchrePlayer.Seats
        For seat = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Players(seat).SortCards(EuchreCard.Suits.NoSuit)
        Next

        SetAllCardImages()
        ShowAllCards(True)

        Dim sKitty As New StringBuilder
        sKitty.AppendFormat(My.Resources.Notice_KittyCard, My.Resources.ResourceManager.GetString(Kitty(0).GetDisplayStringResourceName()))
        Me.UpdateStatus(sKitty.ToString)
    End Sub

    ''' <summary>
    ''' Helper function to update the number of tricks taken by the teams this hand.  Does not affect UI.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateAllTricks()
        i_TheirTricks = Players(EuchrePlayer.Seats.LeftOpponent).TricksWonThisHand + Players(EuchrePlayer.Seats.RightOpponent).TricksWonThisHand
        i_YourTricks = Players(EuchrePlayer.Seats.Player).TricksWonThisHand + Players(EuchrePlayer.Seats.Partner).TricksWonThisHand
        UpdateTricksText()
    End Sub

    ''' <summary>
    ''' Moves a known card from a hand onto the table.  Makes the card face up (if needed), updates the
    ''' various arrays of held cards and played cards, updates tooltips and images, and notifies the user what's been played.
    ''' </summary>
    ''' <param name="player">The player who holds the card</param>
    ''' <param name="index">The index of the card to play</param>
    ''' <remarks></remarks>
    Private Sub PlaySelectedCard(ByVal player As EuchrePlayer, ByVal index As Integer)
        ' TODO:  Make this graphically interesting
        If index > 4 Then
            Throw New System.Exception("Invalid index")
        End If

        player.CardsHeldThisHand(index).State = EuchreCard.States.FaceUp
        Dim faceImage As System.Drawing.Image = player.CardsHeldThisHand(index).GetImage(player.Seat)
        SetImage(Me.TableTopCards(player.Seat, 5), faceImage)

        Dim s As String = My.Resources.ResourceManager.GetString(player.CardsHeldThisHand(index).GetDisplayStringResourceName(TrumpSuit))
        If s = "" Then
            Throw New System.Exception("Invalid value")
        End If

        SetTooltip(Me.TableTopCards(player.Seat, 5), s)

        Dim sPlayed As New StringBuilder
        sPlayed.AppendFormat(My.Resources.Notice_PlayedACard, player.GetDisplayName(Me), s)
        Me.UpdateStatus(sPlayed.ToString)

        PlayedCards(player.Seat) = player.CardsHeldThisHand(index)

        MarkCardAsPlayed(player.CardsHeldThisHand(index))
        player.CardsHeldThisHand(index) = Nothing
        SetTooltip(Me.TableTopCards(player.Seat, index), Nothing)
        Me.TableTopCards(player.Seat, index).Source = Nothing

        SetUIElementVisibility(Me.TableTopCards(player.Seat, index), Visibility.Hidden)

        SetUIElementVisibility(Me.TableTopCards(player.Seat, 5), Visibility.Visible)

        UpdateLayout()

        Me.PlayCardSound()
        Refresh(Me.TableTopCards(player.Seat, index))
        RefreshAndSleep(Me.TableTopCards(player.Seat, 5))
    End Sub

    ''' <summary>
    ''' Swaps a known card with the top card in the kitty.  Adjusts for face up/face down and perspective
    ''' as appropriate.  Updates "known card played" array for the player who swapped the card.  Does
    ''' not update images as this will be done later when play begins.
    ''' </summary>
    ''' <param name="player">The player who holds the card</param>
    ''' <param name="index">The index of the card to swap</param>
    ''' <remarks></remarks>
    Private Sub SwapCardWithKitty(ByVal player As EuchrePlayer, ByVal index As Integer)
        Dim card As EuchreCard = Kitty(0)
        Kitty(0) = player.CardsHeldThisHand(index)
        player.CardsHeldThisHand(index) = card
        Kitty(0).State = EuchreCard.States.FaceDown
        Kitty(0).Perspective = EuchrePlayer.Seats.Player
        player.CardsHeldThisHand(index).Perspective = EuchrePlayer.Seats.Player

        If player.Seat = EuchrePlayer.Seats.Player Then
            player.CardsHeldThisHand(index).State = EuchreCard.States.FaceUp
        Else
            player.CardsHeldThisHand(index).State = EuchreCard.States.FaceDown
        End If

        ' Only this player knows that this card is buried -- don't add it to the "cards played" list
        player.BuriedCard = Kitty(0)
    End Sub

    ''' <summary>
    ''' Helper function to reset all trick values before the next trick
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PrepTrick()
        HideAllPlayedCards()

        TrickLeader = Players(TrickLeaderIndex)
        TrickPlayer = TrickLeader
        TrickHighestCardSoFar = EuchreCard.Values.NoValue
        TrickPlayerWhoPlayedHighestCardSoFar = EuchrePlayer.Seats.NoPlayer
    End Sub

    ''' <summary>
    ''' Gets the current player to select a card to play
    ''' </summary>
    ''' <param name="nextState"></param>
    ''' <remarks></remarks>
    Private Sub SelectCardForTrick(nextState As EuchreState)
        If TrickPlayer.SittingOutThisHand = False Then
            If TrickPlayer.Seat = EuchrePlayer.Seats.Player Then
                HumanPlayACard() ' Selected card will be set by side-effect when a card is clicked
                desiredStateAfterHumanClick = nextState
            Else
                SelectedCard = TrickPlayer.AutoPlayACard(Me)
                UpdateEuchreState(nextState)
            End If
        Else
            UpdateEuchreState(nextState)
        End If
    End Sub

    ''' <summary>
    ''' Enables the playable cards in the human's hand as "clickable," and disables the non-playable
    ''' cards.  (The AI version is a member of the EuchrePlayer object.)
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub HumanPlayACard()
        SelectLabel.Content = My.Resources.Notice_PlayACard
        Dim AnyValid As Boolean = False

        Dim i As Integer
        For i = 0 To 4
            If TrickPlayer.CardsHeldThisHand(i) IsNot Nothing Then
                If TrickLeaderIndex <> TrickPlayer.Seat Then
                    If Not TrickPlayer.CardBelongsToLedSuit(Me, TrickPlayer.CardsHeldThisHand(i)) Then
                        TableTopCards(EuchrePlayer.Seats.Player, i).IsEnabled = False
                        TableTopCards(EuchrePlayer.Seats.Player, i).Opacity = 0.25
                    Else
                        AnyValid = True
                        TableTopCards(EuchrePlayer.Seats.Player, i).Opacity = 1.0
                    End If
                End If
            Else
                TableTopCards(EuchrePlayer.Seats.Player, i).IsEnabled = False
                TableTopCards(EuchrePlayer.Seats.Player, i).Opacity = 0.25
            End If
        Next

        If AnyValid = False Then ' Nothing of suit -- play whatever you want
            For i = 0 To 4
                If TrickPlayer.CardsHeldThisHand(i) IsNot Nothing Then
                    TableTopCards(EuchrePlayer.Seats.Player, i).IsEnabled = True
                    TableTopCards(EuchrePlayer.Seats.Player, i).Opacity = 1.0
                End If
            Next
        End If

        SelectLabel.Visibility = Visibility.Visible
        SelectLabel.UpdateLayout()
        PlayerIsPlayingACard = True
        SetPlayerCursorToHand(True)
    End Sub

    ''' <summary>
    ''' Given that a card has been selected (by either a human or an AI), play the card and update
    ''' the status of the trick.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PlayCardForTrick()
        If TrickPlayer.SittingOutThisHand = False Then
            If TrickPlayer.Seat = EuchrePlayer.Seats.Player Then
                SelectLabel.Visibility = Visibility.Hidden
                For i As Integer = 0 To 4
                    TableTopCards(EuchrePlayer.Seats.Player, i).IsEnabled = True
                    TableTopCards(EuchrePlayer.Seats.Player, i).Opacity = 1.0
                Next i
                UpdateLayout()
            End If

            If TrickLeaderIndex = TrickPlayer.Seat Then
                SuitLedThisRound = TrickPlayer.CardsHeldThisHand(SelectedCard).GetCurrentSuit(TrumpSuit)
            End If
            PlaySelectedCard(TrickPlayer, SelectedCard)

            Dim thisvalue As EuchreCard.Values = PlayedCards(TrickPlayer.Seat).GetCurrentValue(TrumpSuit, PlayedCards(TrickLeaderIndex).GetCurrentSuit(TrumpSuit))
            If thisvalue > TrickHighestCardSoFar Then
                TrickPlayerWhoPlayedHighestCardSoFar = TrickPlayer.Seat
                TrickHighestCardSoFar = PlayedCards(TrickPlayer.Seat).GetCurrentValue(TrumpSuit, PlayedCards(TrickLeaderIndex).GetCurrentSuit(TrumpSuit))
            End If
        End If
        TrickPlayer = Players(EuchrePlayer.NextPlayer(TrickPlayer.Seat))
    End Sub

    ''' <summary>
    ''' After the cards have been played in the trick, determine the winner of the trick and the leader
    ''' for the next trick.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PostTrick()
        Players(TrickPlayerWhoPlayedHighestCardSoFar).TricksWonThisHand = Players(TrickPlayerWhoPlayedHighestCardSoFar).TricksWonThisHand + 1
        Select Case TrickPlayerWhoPlayedHighestCardSoFar
            Case EuchrePlayer.Seats.LeftOpponent, EuchrePlayer.Seats.RightOpponent
                Me.UpdateStatus(My.Resources.Notice_TheirTeamWonTrick)
            Case EuchrePlayer.Seats.Player, EuchrePlayer.Seats.Partner
                Me.UpdateStatus(My.Resources.Notice_YourTeamWonTrick)
        End Select

        UpdateAllTricks()

        TrickLeaderIndex = TrickPlayerWhoPlayedHighestCardSoFar
    End Sub

    ''' <summary>
    ''' Calls to clear the hand information for each player (sitting out, tricks taken, cards played)
    ''' used for gameplay AI.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ClearAllTricks()
        i_TheirTricks = 0
        i_YourTricks = 0

        ' Clear individual information
        For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Players(i).ClearAllTricks()
        Next i

        ' Clear general information
        Dim card As EuchreCard
        For Each card In CardsPlayedThisHand
            card = Nothing
        Next
        UpdateTricksText()
    End Sub

    ''' <summary>
    ''' Figure out who the first bidder is in round 1.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub PreBid1()
        currentBidder = Players(DealerThisHand)
    End Sub

    ''' <summary>
    ''' Cause the current bidder to bid for round 1.  If an AI, then post-process the bid
    ''' as well -- no need to wait -- and go to the next state.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub Bid1(passedState As EuchreState)
        currentBidder = Players(EuchrePlayer.NextPlayer(currentBidder.Seat))
        currentBidder.BuriedCard = Nothing

        Dim GoingAlone As Boolean = False
        If currentBidder.Seat = EuchrePlayer.Seats.Player Then
            currentBidder.HumanBidFirstRound(Me)
            desiredBidPassState = passedState
        Else
            Dim rv As Boolean
            rv = currentBidder.AutoBidFirstRound(Me, GoingAlone)
            currentBidder.ProcessBidFirstRound(Me, GoingAlone, rv)
            RefreshAndSleep(Me.StatusArea)
            If rv Then
                UpdateEuchreState(EuchreState.Bid1PickUp)
            Else
                UpdateEuchreState(passedState)
                UpdateLayout()
            End If
        End If
    End Sub

    ''' <summary>
    ''' The human bidder post-processing for bid 1 happens here, after the bid control is dismissed.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub PostHumanBidFirstRound()
        Dim rv As Boolean = BidControl.PickItUp.IsChecked
        currentBidder.ProcessBidFirstRound(Me, BidControl.GoingAlone.IsChecked, rv)
        If rv Then
            UpdateEuchreState(EuchreState.Bid1PickUp)
        Else
            UpdateEuchreState(desiredBidPassState)
        End If
        UpdateLayout()
    End Sub

    ''' <summary>
    ''' Cause the trade for the kitty card.  If an AI, do the swap and go to the next state.
    ''' If human, call the routine which enables a card to be chosen to drop.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub Bid1PickUp()
        If Players(DealerThisHand).Seat = EuchrePlayer.Seats.Player Then
            HumanReplaceACard()
        Else
            Dim index As Integer = Players(DealerThisHand).LowestCardOnReplace(TrumpSuit)
            SwapCardWithKitty(Players(DealerThisHand), index)
            UpdateEuchreState(EuchreState.Bid1PickedUp)
        End If
    End Sub

    ''' <summary>
    ''' A human dealer needs to trade a card for the kitty card, so go ahead
    ''' and enable the cards for that.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub HumanReplaceACard()
        SelectLabel.Content = My.Resources.Notice_SwapACard
        SetUIElementVisibility(SelectLabel, Visibility.Visible)
        PlayerIsDroppingACard = True
        SetPlayerCursorToHand(True)
        desiredStateAfterHumanClick = EuchreState.Bid1PickedUp
    End Sub

    ''' <summary>
    ''' Bidding round 1 is over and the kitty card picked up, so set the variables
    ''' appropriately.  If a human dealer, do the kitty swap that was postponed earlier.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub Bid1PickedUp()
        If Players(DealerThisHand).Seat = EuchrePlayer.Seats.Player Then
            SetUIElementVisibility(SelectLabel, Visibility.Hidden)
            SwapCardWithKitty(Players(DealerThisHand), SelectedCard)
        End If

        PickedTrumpThisHand = currentBidder.Seat

        If Players(currentBidder.OppositeSeat).SittingOutThisHand Then
            EnableCards(currentBidder.OppositeSeat(), False)
        End If

    End Sub

    ''' <summary>
    ''' For the second bidding round, hide the kitty card and reset the cards so that they
    ''' sort order indicates no specific trump.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PreBid2()
        Kitty(0).State = EuchreCard.States.FaceDown
        SetImage(KittyCard1, Kitty(0).GetImage(EuchrePlayer.Seats.NoPlayer))
        SetTooltip(KittyCard1, My.Resources.ResourceManager.GetString(Kitty(0).GetDisplayStringResourceName()))
        KittyCard1.UpdateLayout()
        MarkCardAsPlayed(Kitty(0))

        Dim seat As EuchrePlayer.Seats
        For seat = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Players(seat).SortCards(EuchreCard.Suits.NoSuit)
        Next
        SetAllCardImages()
    End Sub

    ''' <summary>
    ''' Cause the current bidder to bid for round 2.  If an AI, then post-process the bid
    ''' as well -- no need to wait -- and go to the next state.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub Bid2(passedState As EuchreState)
        currentBidder = Players(EuchrePlayer.NextPlayer(currentBidder.Seat))

        Dim rv As Boolean = False
        Dim GoingAlone As Boolean = False
        If currentBidder.Seat = EuchrePlayer.Seats.Player Then
            currentBidder.HumanBidSecondRound(Me)
            desiredBidPassState = passedState
        Else
            rv = currentBidder.AutoBidSecondRound(Me, GoingAlone)
            currentBidder.ProcessBidSecondRound(Me, GoingAlone, rv)
            RefreshAndSleep(Me.StatusArea)
            If rv Then
                UpdateEuchreState(EuchreState.Bid2Succeeded)
            Else
                UpdateEuchreState(passedState)
                UpdateLayout()
            End If
        End If
    End Sub

    ''' <summary>
    ''' The post-processing for a human bidder on round 1 happens here
    ''' after the bid control is dismissed.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub PostHumanBidSecondRound()
        Dim calledIt As Boolean = Not BidControl2.Pass.IsChecked
        If calledIt Then
            If BidControl2.Hearts.IsChecked Then
                TrumpSuit = EuchreCard.Suits.Hearts
            ElseIf BidControl2.Diamonds.IsChecked Then
                TrumpSuit = EuchreCard.Suits.Diamonds
            ElseIf BidControl2.Clubs.IsChecked Then
                TrumpSuit = EuchreCard.Suits.Clubs
            ElseIf BidControl2.Spades.IsChecked Then
                TrumpSuit = EuchreCard.Suits.Spades
            End If
        End If

        currentBidder.ProcessBidSecondRound(Me, BidControl2.GoingAlone.IsChecked, calledIt)
        If calledIt Then
            UpdateEuchreState(EuchreState.Bid2Succeeded)
        Else
            UpdateEuchreState(desiredBidPassState)
        End If
        UpdateLayout()
    End Sub

    ''' <summary>
    ''' Plays a hand (five tricks).  Calls to clear all previous trick information, trump images and tooltips;
    ''' resets trump suit to "NoSuit"; clears the played card list; deals the cards; calls the bidding round;
    ''' calls to sort and refresh cards based on trump suit; updates the trump image for the user;
    ''' calls to play tricks; updates the scores and clears any "sitting out" information.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Private Sub SetForNewHand()
        ' Clear the tricks text
        ClearAllTricks()
        Me.TrumpPartner.Visibility = Visibility.Hidden
        Me.TrumpPlayer.Visibility = Visibility.Hidden
        Me.TrumpLeft.Visibility = Visibility.Hidden
        Me.TrumpRight.Visibility = Visibility.Hidden
        SetTooltip(Me.TrumpPartner, Nothing)
        SetTooltip(Me.TrumpPlayer, Nothing)
        SetTooltip(Me.TrumpLeft, Nothing)
        SetTooltip(Me.TrumpRight, Nothing)
        TrumpSuit = EuchreCard.Suits.NoSuit
        ResetPlayedCards()
    End Sub

    ''' <summary>
    ''' Sorts the hands according to the current trump, and shows the trump indicator.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SortAndSetHandImagesAndText()
        Dim seat As EuchrePlayer.Seats
        For seat = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
            Players(seat).SortCards(TrumpSuit)
        Next
        SetAllCardImages()

        Select Case PickedTrumpThisHand
            Case EuchrePlayer.Seats.Partner
                SetImage(Me.TrumpPartner, EuchreCard.SuitIm(TrumpSuit))
                SetTooltip(Me.TrumpPartner, My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(TrumpSuit)))
                SetUIElementVisibility(Me.TrumpPartner, Visibility.Visible)
            Case EuchrePlayer.Seats.Player
                SetImage(Me.TrumpPlayer, EuchreCard.SuitIm(TrumpSuit))
                SetTooltip(Me.TrumpPlayer, My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(TrumpSuit)))
                SetUIElementVisibility(Me.TrumpPlayer, Visibility.Visible)
            Case EuchrePlayer.Seats.LeftOpponent
                SetImage(Me.TrumpLeft, EuchreCard.SuitIm(TrumpSuit))
                SetTooltip(Me.TrumpLeft, My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(TrumpSuit)))
                SetUIElementVisibility(Me.TrumpLeft, Visibility.Visible)
            Case EuchrePlayer.Seats.RightOpponent
                SetImage(Me.TrumpRight, EuchreCard.SuitIm(TrumpSuit))
                SetTooltip(Me.TrumpRight, My.Resources.ResourceManager.GetString(EuchreCard.GetSuitDisplayStringResourceName(TrumpSuit)))
                SetUIElementVisibility(Me.TrumpRight, Visibility.Visible)
        End Select
    End Sub

    ''' <summary>
    ''' Determines who won a game.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DetermineWinnerAndEndGame()
        ' Figure out who won.
        If i_TheirScore > i_YourScore Then
            Me.UpdateStatus(My.Resources.Notice_TheyWonTheGame, 2)
            Me.SpeakTheyWon(EuchrePlayer.Seats.Player)
        Else
            Me.UpdateStatus(My.Resources.Notice_YouWonTheGame, 2)
            Me.SpeakWeWon(EuchrePlayer.Seats.Player)
        End If
        GameStarted = False
    End Sub

    ''' <summary>
    ''' Ensures that all players are active again after a hand, even if they were sitting out.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CleanupAfterHand()
        UpdateLayout()

        If Players(Players(PickedTrumpThisHand).OppositeSeat()).SittingOutThisHand Then
            Players(Players(PickedTrumpThisHand).OppositeSeat()).SittingOutThisHand = False
            Me.EnableCards(Players(PickedTrumpThisHand).OppositeSeat(), True)
        End If
    End Sub

    ''' <summary>
    ''' Does nothing.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub NoOp()

    End Sub


    ''' <summary>
    ''' Helper function to increment the dealer.  Updates the dealer UI to indicate who the dealer is as well.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetNextDealer()
        Me.DealerBox(DealerThisHand).Visibility = Visibility.Hidden
        DealerThisHand = EuchrePlayer.NextPlayer(DealerThisHand)
        Me.DealerBox(DealerThisHand).Visibility = Visibility.Visible
    End Sub

    ''' <summary>
    ''' Deals one card to a player during the "determine dealer" phase at the beginning of the game.
    ''' Updates the image and perspective of the card on the table.  Does not update any arrays,
    ''' so except for images we don't need to clean up after the card.  An appropriate sound is played,
    ''' notice generated, and a pause generated as well.
    ''' </summary>
    ''' <param name="player">The player to deal the card to</param>
    ''' <param name="slot">The slot in the player's hand to deal the card to</param>
    ''' <returns>The card played</returns>
    ''' <remarks></remarks>
    Private Function DealACardForDeal(ByVal player As EuchrePlayer.Seats, ByVal slot As Integer) As EuchreCard
        Dim card As EuchreCard = Deck.GetNextCard()
        card.Perspective = player
        SetImage(Me.TableTopCards(player, slot), card.im)
        SetUIElementVisibility(Me.TableTopCards(player, slot), Visibility.Visible)
        SetTooltip(Me.TableTopCards(player, slot), My.Resources.ResourceManager.GetString(card.GetDisplayStringResourceName))

        Dim sDealt As New StringBuilder
        sDealt.AppendFormat(My.Resources.Notice_DealtACard, Players(player).GetDisplayName(Me), My.Resources.ResourceManager.GetString(card.GetDisplayStringResourceName))
        Me.UpdateStatus(sDealt.ToString)

        Me.PlayCardSound()

        RefreshAndSleep(Me.TableTopCards(player, slot))
        Return card
    End Function

    ''' <summary>
    ''' Shuffles the deck and sets the player as the first potential dealer.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PreDealerSelection()
        SelectingDealer = True

        Deck.Shuffle()
        PlayShuffleSound()
        Me.UpdateStatus(My.Resources.Notice_ChoosingDealer)
        potentialDealer = EuchrePlayer.Seats.Player
        potentialDealerCardIndex = 0
    End Sub

    ''' <summary>
    ''' Deals a card and determines if the card is a Jack.  If it is,
    ''' signals that a dealer has been selected; otherwise, increments
    ''' the next potential dealer and index of the next card in the deck.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub TrySelectDealer()
        Dim card As EuchreCard
        card = DealACardForDeal(potentialDealer, potentialDealerCardIndex)
        If card.Rank = EuchreCard.Ranks.Jack Then
            UpdateEuchreState(EuchreState.DealerSelected)
        Else
            potentialDealer = EuchrePlayer.NextPlayer(potentialDealer)
            If potentialDealer = EuchrePlayer.Seats.Player Then
                potentialDealerCardIndex += 1
            End If
            UpdateEuchreState(EuchreState.StillSelectingDealer)
        End If

    End Sub

    ''' <summary>
    ''' Updates the UI and gets confirmation after a dealer is selected.
    ''' </summary>
    ''' <param name="nextState"></param>
    ''' <remarks></remarks>
    Private Sub PostDealerSelection(nextState As EuchreState)
        Dim sDealer As New StringBuilder
        sDealer.AppendFormat(My.Resources.Notice_IAmTheDealer, Players(potentialDealer).GetDisplayName(Me))
        Me.UpdateStatus(sDealer.ToString)
        ShowAndEnableContinueButton(nextState)
    End Sub

    ''' <summary>
    ''' Cleans up the dealer-choice cards after teh dealer selection has been acknowledged.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PostDealerCleanup()
        ' Hide the fake cards we just played (no need to update any player arrays)
        For i As Integer = 0 To 3
            For j As Integer = 0 To 4
                SetUIElementVisibility(Me.TableTopCards(i, j), Visibility.Hidden)
                Me.TableTopCards(i, j).Source = Nothing
                SetTooltip(Me.TableTopCards(i, j), Nothing)
            Next j
        Next i

        SelectingDealer = False
        DealerThisHand = potentialDealer
        Me.DealerBox(DealerThisHand).Visibility = Visibility.Visible

    End Sub

    ''' <summary>
    ''' Helper method to display a modal "continue" button.  
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ShowAndEnableContinueButton(nextState As EuchreState)
        Me.ContinueButton.Visibility = Visibility.Visible
        Me.ContinueButton.IsEnabled = True
        Me.ContinueButton.IsDefault = True
        Me.ContinueButton.IsHitTestVisible = True
        UpdateLayout()
        desiredStateAfterHumanClick = nextState
    End Sub

    ''' <summary>
    ''' Starts the gaming.  Calls to collect the options from the user and persists the results;
    ''' creates a new card deck; and calls to play the game.
    ''' </summary>
    ''' <returns>False if user cancels out of option dialog; True otherwise</returns>
    ''' <remarks></remarks>
    Private Function StartItUp() As Boolean
        ' Start the game...
        If OptionsDialog Is Nothing Then
            OptionsDialog = New EuchreOptions()
        End If

        ' Set the window location:
        OptionsDialog.Left = Me.Left + (Me.Width - OptionsDialog.Width) / 2
        OptionsDialog.Top = Me.Top + (Me.Height - OptionsDialog.Height) / 2

        OptionsDialog.ShowDialog()
        If OptionsDialog.LocalDialogResult Then
            Me.StickTheDealer = OptionsDialog.StickTheDealer.IsChecked
            Me.UseNineOfHeartsRule = OptionsDialog.NineOfHearts.IsChecked
            Me.PeekAtOtherCards = OptionsDialog.PeekAtOtherCards.IsChecked
            Me.UseSuperEuchreRule = OptionsDialog.SuperEuchre.IsChecked
            Me.UseQuietDealerRule = OptionsDialog.QuietDealer.IsChecked
            Me.SoundOn = OptionsDialog.SoundOn.IsChecked

            Me.PlayerName = OptionsDialog.PlayerName.Text

            If Me.PlayerName = "" Then
                PlayerName = My.Resources.Player_Player
            End If
            Me.PartnerName = OptionsDialog.PartnerName.Text
            If Me.PartnerName = "" Then
                Dim s As New StringBuilder()
                s.AppendFormat(My.Resources.Player_Partner, PlayerName)
                PartnerName = s.ToString
            End If
            Me.LeftOpponentName = OptionsDialog.LeftOpponentName.Text
            If Me.LeftOpponentName = "" Then
                LeftOpponentName = My.Resources.Player_LeftOpponent
            End If
            Me.RightOpponentName = OptionsDialog.RightOpponentName.Text
            If Me.RightOpponentName = "" Then
                RightOpponentName = My.Resources.Player_RightOpponent
            End If
            If OptionsDialog.LeftOpponentCrazy.IsChecked Then
                Players(EuchrePlayer.Seats.LeftOpponent).Personality = EuchrePlayer.Personalities.Crazy
            ElseIf OptionsDialog.LeftOpponentNormal.IsChecked Then
                Players(EuchrePlayer.Seats.LeftOpponent).Personality = EuchrePlayer.Personalities.Normal
            Else
                Players(EuchrePlayer.Seats.LeftOpponent).Personality = EuchrePlayer.Personalities.Conservative
            End If
            If OptionsDialog.RightOpponentCrazy.IsChecked Then
                Players(EuchrePlayer.Seats.RightOpponent).Personality = EuchrePlayer.Personalities.Crazy
            ElseIf OptionsDialog.RightOpponentNormal.IsChecked Then
                Players(EuchrePlayer.Seats.RightOpponent).Personality = EuchrePlayer.Personalities.Normal
            Else
                Players(EuchrePlayer.Seats.RightOpponent).Personality = EuchrePlayer.Personalities.Conservative
            End If
            If OptionsDialog.PartnerCrazy.IsChecked Then
                Players(EuchrePlayer.Seats.Partner).Personality = EuchrePlayer.Personalities.Crazy
            ElseIf OptionsDialog.PartnerNormal.IsChecked Then
                Players(EuchrePlayer.Seats.Partner).Personality = EuchrePlayer.Personalities.Normal
            Else
                Players(EuchrePlayer.Seats.Partner).Personality = EuchrePlayer.Personalities.Conservative
            End If
        Else
            Return False
        End If

        Me.PlayerNameLabel.Content = PlayerName
        Me.PartnerNameLabel.Content = PartnerName
        Me.LeftOpponentNameLabel.Content = LeftOpponentName
        Me.RightOpponentNameLabel.Content = RightOpponentName
        Me.ShowAllNameLabels(True)

        Players(EuchrePlayer.Seats.LeftOpponent).Voice.SetVoice(OptionsDialog.LeftVoiceCombo.Text)
        Players(EuchrePlayer.Seats.Partner).Voice.SetVoice(OptionsDialog.PartnerVoiceCombo.Text)
        Players(EuchrePlayer.Seats.RightOpponent).Voice.SetVoice(OptionsDialog.RightVoiceCombo.Text)

        Me.PlayerNameLabel.UpdateLayout()
        Me.PartnerNameLabel.UpdateLayout()
        Me.LeftOpponentNameLabel.UpdateLayout()
        Me.RightOpponentNameLabel.UpdateLayout()

        ' Generate the deck
        Deck = New EuchreCardDeck(Me.UseNineOfHeartsRule, Me)
        Deck.Initialize()

        Return True
    End Function

    ''' <summary>
    ''' If a game is under way (GameStarted = true), prompts the user if he/she really wants to quit.
    ''' </summary>
    ''' <returns>True if users cancels the quit; False is quit should continue.</returns>
    ''' <remarks></remarks>
    Private Function QueryCancelClose() As Boolean
        If GameStarted Then
            If MessageBox.Show(My.Resources.Command_Exit, My.Resources.Command_ExitTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) <> MessageBoxResult.OK Then
                Return True ' Cancel the exit
            End If
        End If
        Return False
    End Function

    ''' <summary>
    ''' Starts a new game.  Sets the GameStarted flag if we are
    ''' going to start a new game.  Cleans up the table and calls to start the game.
    ''' Provides various feedback to the user, clears the GameStarted flag, and cleans
    ''' the table when the game is over.  Closes the application if StartItUp returned
    ''' False (i.e., cancelled out of the options dialog).  
    ''' </summary>
    ''' <remarks></remarks>
    Private Function NewGame() As Boolean
        Me.UpdateStatus(My.Resources.Notice_StartingNewGame)
        Me.ResetScores()
        Me.ResetUserInputStates()
        Me.ShowAllCards(False)
        Me.HideAllPlayedCards()
        Me.ShowAllNameLabels(False)
        Me.HideAllDealerAndTrumpLabels()
        If StartItUp() Then ' True if we started a game
            GameStarted = True
            Return True
        Else
            Me.Close() ' User cancelled out of the options dialog, so we'll quit.
            OptionsDialog.DisposeVoice()
            OptionsDialog = Nothing ' Dialog has closed and is not useful anymore.
            Return False
        End If
    End Function

    ''' <summary>
    ''' Cleans up the table after a game, hiding everything that is transitory.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CleanUpGame()
        Me.ShowAllCards(False)
        Me.HideAllPlayedCards()
        Me.HideAllDealerAndTrumpLabels()
        GameStarted = False
    End Sub


    ''' <summary>
    ''' Helper function to hide/show all player name labels.
    ''' </summary>
    ''' <param name="ShowAll">True to show the labels; False to hide them.</param>
    ''' <remarks></remarks>
    Private Sub ShowAllNameLabels(ByVal ShowAll As Boolean)
        Dim visible As Visibility = If(ShowAll, Visibility.Visible, Visibility.Hidden)
        Me.PlayerNameLabel.Visibility = visible
        Me.PartnerNameLabel.Visibility = visible
        Me.LeftOpponentNameLabel.Visibility = visible
        Me.RightOpponentNameLabel.Visibility = visible
    End Sub

    ''' <summary>
    ''' Helper function to hide all of the dealer and trump labels, plus clear any trump tooltips.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub HideAllDealerAndTrumpLabels()
        Me.DealerLeftOpponent.Visibility = Visibility.Hidden
        Me.DealerRightOpponent.Visibility = Visibility.Hidden
        Me.DealerPartner.Visibility = Visibility.Hidden
        Me.DealerPlayer.Visibility = Visibility.Hidden

        Me.TrumpLeft.Visibility = Visibility.Hidden
        Me.TrumpRight.Visibility = Visibility.Hidden
        Me.TrumpPlayer.Visibility = Visibility.Hidden
        Me.TrumpPartner.Visibility = Visibility.Hidden

        SetTooltip(Me.TrumpPartner, Nothing)
        SetTooltip(Me.TrumpPlayer, Nothing)
        SetTooltip(Me.TrumpLeft, Nothing)
        SetTooltip(Me.TrumpRight, Nothing)
    End Sub
#End Region
#Region "Event handlers"

    ''' <summary>
    ''' Reacts to a user's  card being clicked if the user is dropping or playing a card.
    ''' Updates state based on global desiredStateAfterHumanClick.
    ''' </summary>
    ''' <param name="sender">Originating object (ignored)</param>
    ''' <param name="e">Events arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub PlayerCard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
        Handles PlayerCard1.MouseDown, PlayerCard2.MouseDown, PlayerCard3.MouseDown, PlayerCard4.MouseDown, PlayerCard5.MouseDown
        If PlayerIsDroppingACard OrElse PlayerIsPlayingACard Then
            If sender Is PlayerCard1 Then
                SelectedCard = 0
            ElseIf sender Is PlayerCard2 Then
                SelectedCard = 1
            ElseIf sender Is PlayerCard3 Then
                SelectedCard = 2
            ElseIf sender Is PlayerCard4 Then
                SelectedCard = 3
            ElseIf sender Is PlayerCard5 Then
                SelectedCard = 4
            End If

            PlayerIsDroppingACard = False
            PlayerIsPlayingACard = False
            SetPlayerCursorToHand(False)

            ' Signal that we're ready to commence again
            UpdateEuchreState(desiredStateAfterHumanClick)
        End If
    End Sub

    ''' <summary>
    ''' Catches the Closing event and calls to see if the user really wants to quit.
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub EuchreTable_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        e.Cancel = QueryCancelClose()
        If Not e.Cancel Then
            For i As EuchrePlayer.Seats = EuchrePlayer.Seats.LeftOpponent To EuchrePlayer.Seats.Player
                Players(i).DisposeVoice()
            Next
            If (OptionsDialog IsNot Nothing) Then
                OptionsDialog.DisposeVoice()
            End If
            Dispatcher.InvokeShutdown()
        End If
    End Sub

    ''' <summary>
    ''' Catches the "Exit" menu item event and calls Close().
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    ''' <summary>
    ''' Catches the "New" menu item event and calls NewGame(). 
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub NewGameToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UpdateEuchreState(EuchreState.StartNewGameRequested)
    End Sub

    ''' <summary>
    ''' Handler for "Rules" menu item click.  Activates the EuchreRules dialog, creating it if it
    ''' doesn't exist.  The dialog is modeless.
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub RulesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim rulesWindow As Window = Nothing
        For Each win As Window In My.Application.Windows
            If win.GetType().ToString = "EuchreRules" Then
                rulesWindow = win
                Exit For
            End If
        Next
        If rulesWindow Is Nothing Then
            Dim x As New EuchreRules
            x.Show()
        Else
            rulesWindow.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Handler for the "About" menu item click.  Shows the modal "About" box.
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim dlg As New EuchreAboutBox(Me)
        dlg.ShowDialog()
    End Sub

    Private Sub EuchreTable_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.F2 Then
            UpdateEuchreState(EuchreState.StartNewGameRequested)
        End If
    End Sub

    ''' <summary>
    ''' Handler for the EuchreTable form load.  Updates the "Welcome" message in the status area.
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub EuchreTable_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        ResizeMode = ResizeMode.CanMinimize
        SetImage(Logo, My.Resources.logo)
        SetIcon(Me, My.Resources.Euchre)
        Me.UpdateStatus(My.Resources.Notice_Welcome)
        CachedCursor = Me.Cursor
    End Sub

    ''' <summary>
    ''' Handles the click event on the continue button, updating state based on global desiredStateAfterHumanClick
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub ContinueButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ContinueButton.Click
        SetUIElementVisibility(Me.ContinueButton, Visibility.Hidden)
        Me.ContinueButton.IsEnabled = False
        Me.ContinueButton.IsHitTestVisible = False
        UpdateEuchreState(desiredStateAfterHumanClick)
    End Sub

    ''' <summary>
    '''  This gets around a problem in WPF where you can't force an update
    ''' </summary>
    Private Sub EmptyMethod()

    End Sub

    Private Sub RefreshAndSleep(uie As UIElement)
        Refresh(uie)
        Thread.Sleep(sleepDuration)
    End Sub

    Private Sub Refresh(uie As UIElement)
        uie.Dispatcher.Invoke(DispatcherPriority.Render, New Action(AddressOf EmptyMethod))
    End Sub

#End Region
#Region "Enums"
    ' All possible game states
    Public Enum EuchreState
        NoState
        StartNewGameRequested
        StartNewGameConfirmed
        StillSelectingDealer
        DealerSelected
        DealerAcknowledged
        ClearHand
        StartNewHand
        DealCards
        Bid1Starts
        Bid1Player0
        Bid1Player1
        Bid1Player2
        Bid1Player3
        Bid1PickUp
        Bid1PickedUp
        Bid1Failed
        Bid1FailedAcknowledged
        Bid1Succeeded
        Bid1SucceededAcknowledged
        Bid2Starts
        Bid2Player0
        Bid2Player1
        Bid2Player2
        Bid2Player3
        Bid2Failed
        Bid2FailedAcknowledged
        Bid2Succeeded
        Bid2SucceededAcknowledged
        Trick0Started
        Trick0_SelectCard0
        Trick0_PlayCard0
        Trick0_SelectCard1
        Trick0_PlayCard1
        Trick0_SelectCard2
        Trick0_PlayCard2
        Trick0_SelectCard3
        Trick0_PlayCard3
        Trick0Ended
        Trick0EndingAcknowledged
        Trick1Started
        Trick1_SelectCard0
        Trick1_PlayCard0
        Trick1_SelectCard1
        Trick1_PlayCard1
        Trick1_SelectCard2
        Trick1_PlayCard2
        Trick1_SelectCard3
        Trick1_PlayCard3
        Trick1Ended
        Trick1EndingAcknowledged
        Trick2Started
        Trick2_SelectCard0
        Trick2_PlayCard0
        Trick2_SelectCard1
        Trick2_PlayCard1
        Trick2_SelectCard2
        Trick2_PlayCard2
        Trick2_SelectCard3
        Trick2_PlayCard3
        Trick2Ended
        Trick2EndingAcknowledged
        Trick3Started
        Trick3_SelectCard0
        Trick3_PlayCard0
        Trick3_SelectCard1
        Trick3_PlayCard1
        Trick3_SelectCard2
        Trick3_PlayCard2
        Trick3_SelectCard3
        Trick3_PlayCard3
        Trick3Ended
        Trick3EndingAcknowledged
        Trick4Started
        Trick4_SelectCard0
        Trick4_PlayCard0
        Trick4_SelectCard1
        Trick4_PlayCard1
        Trick4_SelectCard2
        Trick4_PlayCard2
        Trick4_SelectCard3
        Trick4_PlayCard3
        Trick4Ended
        Trick4EndingAcknowledged
        HandCompleted
        HandCompletedAcknowledged
        GameOver
    End Enum

    Private Enum ScorePrefix
        ScoreThem = 1
        ScoreUs = 2
    End Enum
#End Region
#Region "Private variables"
    ' Private variables
    Private i_TheirScore As Integer
    Private i_YourScore As Integer
    Private i_TheirTricks As Integer
    Private i_YourTricks As Integer
    Private SoundOn As Boolean = True
    Private GameStarted As Boolean = False

    Private Deck As EuchreCardDeck
    Private OptionsDialog As EuchreOptions = Nothing

    Private DealerBox(4) As GroupBox

    Private PlayedCardIndex As Integer

    Private desiredBidPassState As EuchreState
    Private desiredStateAfterHumanClick As EuchreState
    Private lastState As EuchreState
    Private currentState As EuchreState
    Private currentBidder As EuchrePlayer
    Private potentialDealer As EuchrePlayer.Seats
    Private potentialDealerCardIndex As Integer

    Private sleepDuration As Integer = 250 ' milliseconds
    Private CachedCursor As System.Windows.Input.Cursor

#End Region
#Region "Public variables"

    ' Public variables
    Public StickTheDealer As Boolean = False
    Public UseNineOfHeartsRule As Boolean = False
    Public UseSuperEuchreRule As Boolean = False
    Public UseQuietDealerRule As Boolean = False
    Public PeekAtOtherCards As Boolean = False
    Public SelectingDealer As Boolean = False
    Public PlayerName As String = ""
    Public PartnerName As String = ""
    Public LeftOpponentName As String = ""
    Public RightOpponentName As String = ""
    Public TrumpSuit As EuchreCard.Suits
    Public PickedTrumpThisHand As EuchrePlayer.Seats
    Public DealerThisHand As EuchrePlayer.Seats
    Public TrickLeaderIndex As EuchrePlayer.Seats
    Public SuitLedThisRound As EuchreCard.Suits

    Public CardsPlayedThisHand(24) As EuchreCard
    Public Players(4) As EuchrePlayer
    Public PlayedCards(4) As EuchreCard
    Public Kitty(4) As EuchreCard
    Public TableTopCards(4, 6) As Image ' (Player index, card index -- final value is played card)

    Public SelectedCard As Integer = 0
    Public PlayerIsDroppingACard As Boolean = False
    Public PlayerIsPlayingACard As Boolean = False

    Public TrickLeader As EuchrePlayer
    Public TrickPlayer As EuchrePlayer
    Public TrickHighestCardSoFar As EuchreCard.Values
    Public TrickPlayerWhoPlayedHighestCardSoFar As EuchrePlayer.Seats

#End Region
#Region "Helper classes"

#End Region

End Class

''' <summary>
''' An exception class specifically used for VBEuchre
''' </summary>
''' <remarks></remarks>
Class EuchreException
    Inherits SystemException
    ''' <summary>
    ''' Overridden "New" method which takes a message string and calls the base constructor
    ''' </summary>
    ''' <param name="message"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
End Class

