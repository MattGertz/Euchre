Public Class EuchreBid2Control

#Region "Public methods"
    ''' <summary>
    ''' Informs the dialog on the state of the QuietDealer rule
    ''' </summary>
    ''' <param name="forced">True if the QuietDealer rule is on; False otherwise</param>
    ''' <remarks></remarks>
    Public Sub ForceGoAlone(ByVal forced As Boolean)
        QuietDealer = forced
    End Sub

    ''' <summary>
    ''' Resets the dialog so that the default is to pass
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Reset()
        Me.Pass.IsChecked = True
        Me.Pass.IsEnabled = True
        Me.Pass.Opacity = 1.0
        Me.GoingAlone.IsChecked = False
        Me.GoingAlone.IsEnabled = False
        Me.GoingAlone.Opacity = 0.25

        Me.Hearts.IsEnabled = True
        Me.Hearts.Opacity = 1.0
        Me.Diamonds.IsEnabled = True
        Me.Diamonds.Opacity = 1.0
        Me.Clubs.IsEnabled = True
        Me.Clubs.Opacity = 1.0
        Me.Spades.IsEnabled = True
        Me.Spades.Opacity = 1.0
    End Sub
#End Region
#Region "Private methods"
    ''' <summary>
    ''' Sets a tooltip to a particular string for a particular Image
    ''' </summary>
    ''' <param name="Ctrl">The image to set the tooltip on</param>
    ''' <param name="Tip"></param>
    ''' <remarks></remarks>
    Private Sub SetTooltip(ByVal Ctrl As System.Windows.Controls.Control, ByVal Tip As String)
        Ctrl.ToolTip = Tip
    End Sub
#End Region
#Region "Event handlers"
    ''' <summary>
    ''' Handles the pass change event, to control whether or not the player who doesn't pass
    ''' (stick-the-dealer) is is forced to play alone (quiet dealer).
    ''' </summary>
    ''' <param name="sender">Event originator (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub Pass_Checked(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Pass.Checked, Pass.Unchecked
        If QuietDealer AndAlso Not Pass.IsChecked Then
            GoingAlone.IsEnabled = False
            GoingAlone.Opacity = 0.25
            GoingAlone.IsChecked = True
        Else
            GoingAlone.IsEnabled = Not Pass.IsChecked
            GoingAlone.Opacity = If(Pass.IsChecked, 0.25, 1.0)
            GoingAlone.IsChecked = False
        End If
    End Sub

    ''' <summary>
    ''' Handles the OK event for the BidControl.  Hides the control and resets the bidding flag.
    ''' </summary>
    ''' <param name="sender">Event originator (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OkButton.Click
        Me.Visibility = Visibility.Hidden
        Me.IsEnabled = False
        Me.IsHitTestVisible = False
        Me.UpdateLayout()
        Table.PostHumanBidSecondRound()
    End Sub

    ''' <summary>
    ''' Calls NewGame() if F2 is pressed while focus is on the control.
    ''' </summary>
    ''' <param name="sender">Object sending the event (ignored)</param>
    ''' <param name="e">Event arguments (use to check which key was pressed)</param>
    ''' <remarks></remarks>
    Private Sub EuchreBid2Control_KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.F2 Then
            Table.UpdateEuchreState(EuchreTable.EuchreState.StartNewGameRequested)
        End If
    End Sub

    ''' <summary>
    ''' Handles the Load event for the BidControl.  The tooltips get set up here.
    ''' </summary>
    ''' <param name="sender">Event originator (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub EuchreBid2Control_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Loaded
        SetTooltip(Me.Pass, My.Resources.BID_Pass1)
        SetTooltip(Me.GoingAlone, My.Resources.BID_GoingAlone)
        SetTooltip(Me.Hearts, My.Resources.BID_Hearts)
        SetTooltip(Me.Diamonds, My.Resources.BID_Diamonds)
        SetTooltip(Me.Clubs, My.Resources.BID_Clubs)
        SetTooltip(Me.Spades, My.Resources.BID_Spades)
    End Sub

#End Region
#Region "Public members"
    Public Table As EuchreTable
#End Region
#Region "Private members"
    Private QuietDealer As Boolean
#End Region
End Class
