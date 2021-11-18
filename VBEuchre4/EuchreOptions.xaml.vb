Imports System.Speech.Synthesis

Public Class EuchreOptions

#Region "Public methods"
    ''' <summary>
    ''' Populates the dialog control values from the settings in the application data
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UpdateSettings()
        SetTooltip(Me.PlayerName, My.Resources.OPTION_Name)
        SetTooltip(Me.StickTheDealer, My.Resources.OPTION_StickTheDealer)
        SetTooltip(Me.NineOfHearts, My.Resources.OPTION_NineOfHearts)
        SetTooltip(Me.SuperEuchre, My.Resources.OPTION_SuperEuchre)
        SetTooltip(Me.QuietDealer, My.Resources.OPTION_QuietDealer)
        SetTooltip(Me.PeekAtOtherCards, My.Resources.OPTION_PeekAtOtherCards)
        SetTooltip(Me.SoundOn, My.Resources.OPTION_SoundEffects)
        SetTooltip(Me.LeftOpponentName, My.Resources.OPTION_LeftOpponent)
        SetTooltip(Me.PartnerName, My.Resources.OPTION_YourPartner)
        SetTooltip(Me.RightOpponentName, My.Resources.OPTION_RightOpponent)
        SetTooltip(Me.LeftVoiceCombo, My.Resources.OPTION_LeftOpponentVoice)
        SetTooltip(Me.PartnerVoiceCombo, My.Resources.OPTION_YourPartnerVoice)
        SetTooltip(Me.RightVoiceCombo, My.Resources.OPTION_RightOpponentVoice)
        SetTooltip(Me.LeftOpponentCrazy, My.Resources.OPTION_Crazy)
        SetTooltip(Me.LeftOpponentNormal, My.Resources.OPTION_Normal)
        SetTooltip(Me.LeftOpponentConservative, My.Resources.OPTION_Conservative)
        SetTooltip(Me.PartnerCrazy, My.Resources.OPTION_Crazy)
        SetTooltip(Me.PartnerNormal, My.Resources.OPTION_Normal)
        SetTooltip(Me.PartnerConservative, My.Resources.OPTION_Conservative)
        SetTooltip(Me.RightOpponentCrazy, My.Resources.OPTION_Crazy)
        SetTooltip(Me.RightOpponentNormal, My.Resources.OPTION_Normal)
        SetTooltip(Me.RightOpponentConservative, My.Resources.OPTION_Conservative)

        Me.PlayerName.Text = My.Settings.PlayerName
        Me.PartnerName.Text = My.Settings.PartnerName
        Me.LeftOpponentName.Text = My.Settings.LeftOpponentName
        Me.RightOpponentName.Text = My.Settings.RightOpponentName

        Me.LeftVoiceCombo.SelectedIndex = If(My.Settings.LeftOpponentVoice < VoiceCount, My.Settings.LeftOpponentVoice, 0)
        Me.PartnerVoiceCombo.SelectedIndex = If(My.Settings.PartnerVoice < VoiceCount, My.Settings.PartnerVoice, 0)
        Me.RightVoiceCombo.SelectedIndex = If(My.Settings.RightOpponentVoice < VoiceCount, My.Settings.RightOpponentVoice, 0)

        Me.StickTheDealer.IsChecked = My.Settings.StickTheDealer
        Me.NineOfHearts.IsChecked = My.Settings.NineOfHearts
        Me.SuperEuchre.IsChecked = My.Settings.SuperEuchre
        Me.QuietDealer.IsChecked = My.Settings.QuietDealer
        Me.PeekAtOtherCards.IsChecked = My.Settings.PeekAtOtherCards
        Me.SoundOn.IsChecked = My.Settings.SoundOn

        Me.LeftOpponentCrazy.IsChecked = (My.Settings.LeftOpponentPlay = 1)
        Me.LeftOpponentNormal.IsChecked = (My.Settings.LeftOpponentPlay <> 1 AndAlso My.Settings.LeftOpponentPlay <> 3)
        Me.LeftOpponentConservative.IsChecked = (My.Settings.LeftOpponentPlay = 3)

        Me.PartnerCrazy.IsChecked = (My.Settings.PartnerPlay = 1)
        Me.PartnerNormal.IsChecked = (My.Settings.PartnerPlay <> 1 AndAlso My.Settings.PartnerPlay <> 3)
        Me.PartnerConservative.IsChecked = (My.Settings.PartnerPlay = 3)

        Me.RightOpponentCrazy.IsChecked = (My.Settings.RightOpponentPlay = 1)
        Me.RightOpponentNormal.IsChecked = (My.Settings.RightOpponentPlay <> 1 AndAlso My.Settings.RightOpponentPlay <> 3)
        Me.RightOpponentConservative.IsChecked = (My.Settings.RightOpponentPlay = 3)

    End Sub
    Property LocalDialogResult As Boolean

    Public Sub DisposeVoice()
        If VoiceSynthesizer IsNot Nothing Then
            VoiceSynthesizer.Dispose()
            VoiceSynthesizer = Nothing
        End If
    End Sub

#End Region
#Region "Private methods"
    Private Sub Speak(ByVal Name As String)
        If VoiceSynthesizer IsNot Nothing Then
            Try
                VoiceSynthesizer.SelectVoice(Name)
                VoiceSynthesizer.Speak(My.Resources.SAY_LetsPlayEuchre)
            Catch ex As Exception

            End Try
        End If
    End Sub
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
#Region "Event Handlers"

    ''' <summary>
    ''' Short-circuit the closing handling so that we can reuse the dialog
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub EuchreOptions_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        If LocalDialogResult Then ' Don't do this on quit, or else we can't shut down!  
            e.Cancel = True ' Otherwise, we'll never be able to reuse this window.
            Hide()
        End If
    End Sub

    ''' <summary>
    ''' Load event for the options dialog.  Simply calls through to UpdateSettings.
    ''' </summary>
    ''' <param name="sender">Originator of event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub EuchreOptions_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Loaded
        ResizeMode = ResizeMode.NoResize
        EuchreTable.SetIcon(Me, My.Resources.Euchre)
        If VoiceSynthesizer Is Nothing Then ' Haven't already initialized it
            Try
                VoiceSynthesizer = New SpeechSynthesizer
            Catch ex As Exception
            End Try
            Me.LeftVoiceCombo.IsEnabled = False
            Me.PartnerVoiceCombo.IsEnabled = False
            Me.RightVoiceCombo.IsEnabled = False
            If VoiceSynthesizer IsNot Nothing Then
                Dim Voices = VoiceSynthesizer.GetInstalledVoices()
                VoiceCount = Voices.Count
                If VoiceCount > 0 Then
                    For i As Integer = 0 To VoiceCount - 1
                        ' As of Windows 8, it seems that the synthesizer will automatically covert the voice formats.
                        ' This is definitely not true for Win7 and earlier
                        Dim osVersion As String = My.Computer.Info.OSVersion
                        Dim versionNumbers As String() = osVersion.Split(".")
                        Dim supportsVoiceConversion As Boolean = CInt(versionNumbers(0)) > 6 OrElse (CInt(versionNumbers(0)) = 6 AndAlso CInt(versionNumbers(1)) >= 2)
                        If supportsVoiceConversion OrElse Voices.Item(i).VoiceInfo.SupportedAudioFormats.Count > 0 Then
                            Dim s As String = Voices.Item(i).VoiceInfo.Name
                            Me.LeftVoiceCombo.Items.Add(s)
                            Me.PartnerVoiceCombo.Items.Add(s)
                            Me.RightVoiceCombo.Items.Add(s)
                        End If
                    Next i
                    Me.LeftVoiceCombo.IsEnabled = True
                    Me.PartnerVoiceCombo.IsEnabled = True
                    Me.RightVoiceCombo.IsEnabled = True
                End If
            End If
        End If

        UpdateSettings()
    End Sub

    ''' <summary>
    ''' Persists the settings to application data and dismisses the dialog.
    ''' </summary>
    ''' <param name="sender">Originator of event (ignored)</param>
    ''' <param name="e">Event arguments (ignored)</param>
    ''' <remarks></remarks>
    Private Sub OKBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKBtn.Click
        My.Settings.PlayerName = Me.PlayerName.Text
        My.Settings.PartnerName = Me.PartnerName.Text
        My.Settings.LeftOpponentName = Me.LeftOpponentName.Text
        My.Settings.RightOpponentName = Me.RightOpponentName.Text

        My.Settings.LeftOpponentVoice = Me.LeftVoiceCombo.SelectedIndex
        My.Settings.PartnerVoice = Me.PartnerVoiceCombo.SelectedIndex
        My.Settings.RightOpponentVoice = Me.RightVoiceCombo.SelectedIndex

        My.Settings.StickTheDealer = Me.StickTheDealer.IsChecked
        My.Settings.NineOfHearts = Me.NineOfHearts.IsChecked
        My.Settings.SuperEuchre = Me.SuperEuchre.IsChecked
        My.Settings.QuietDealer = Me.QuietDealer.IsChecked
        My.Settings.PeekAtOtherCards = Me.PeekAtOtherCards.IsChecked
        My.Settings.SoundOn = Me.SoundOn.IsChecked

        If Me.LeftOpponentCrazy.IsChecked Then
            My.Settings.LeftOpponentPlay = 1
        ElseIf LeftOpponentNormal.IsChecked Then
            My.Settings.LeftOpponentPlay = 2
        Else
            My.Settings.LeftOpponentPlay = 3
        End If

        If Me.PartnerCrazy.IsChecked Then
            My.Settings.PartnerPlay = 1
        ElseIf PartnerNormal.IsChecked Then
            My.Settings.PartnerPlay = 2
        Else
            My.Settings.PartnerPlay = 3
        End If

        If Me.RightOpponentCrazy.IsChecked Then
            My.Settings.RightOpponentPlay = 1
        ElseIf RightOpponentNormal.IsChecked Then
            My.Settings.RightOpponentPlay = 2
        Else
            My.Settings.RightOpponentPlay = 3
        End If
        Me.LocalDialogResult = True
        Me.DialogResult = False
    End Sub

    ''' <summary>
    ''' Cancel closing the dialog and clean up
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CancelBtn_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles CancelBtn.Click
        Me.LocalDialogResult = False
        Me.DialogResult = False
    End Sub

    ''' <summary>
    ''' Repopulates the dialog with the original application settings.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ResetBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ResetBtn.Click
        My.Settings.Reset()
        UpdateSettings()
        UpdateLayout()
    End Sub
    ''' <summary>
    ''' Try out the chosen left opponent voice
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub LeftVoiceCombo_DropDownClosed(ByVal sender As Object, ByVal e As System.EventArgs) Handles LeftVoiceCombo.DropDownClosed
        Speak(Me.LeftVoiceCombo.Text)
    End Sub
    ''' <summary>
    ''' Try out the chosen partner voice
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>    
    Private Sub PartnerVoiceCombo_DropDownClosed(ByVal sender As Object, ByVal e As System.EventArgs) Handles PartnerVoiceCombo.DropDownClosed
        Speak(Me.PartnerVoiceCombo.Text)
    End Sub
    ''' <summary>
    ''' Try out the chosen right opponent voice
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub RightVoiceCombo_DropDownClosed(ByVal sender As Object, ByVal e As System.EventArgs) Handles RightVoiceCombo.DropDownClosed
        Speak(Me.RightVoiceCombo.Text)
    End Sub
#End Region
#Region "Private variables"
    Private VoiceSynthesizer As SpeechSynthesizer = Nothing
    Private VoiceCount As Integer = 0
#End Region


End Class
