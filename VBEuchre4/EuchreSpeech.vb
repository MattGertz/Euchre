Imports System.Speech.Synthesis

Public Class EuchreSpeech
    Private VoiceSynthesizer As New SpeechSynthesizer
    ''' <summary>
    ''' Sets up the object with a the desired voice
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetVoice(ByVal Name As String)
        If VoiceSynthesizer IsNot Nothing Then

            Try
                VoiceSynthesizer.SelectVoice(Name)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Public Sub DisposeVoice()
        If VoiceSynthesizer IsNot Nothing Then
            VoiceSynthesizer.Dispose()
            VoiceSynthesizer = Nothing
        End If

    End Sub

    ''' <summary>
    ''' Speaks one string if the voice is initialized
    ''' </summary>
    ''' <param name="s"></param>
    ''' <remarks></remarks>
    Private Sub Say(ByVal s As String)
        If VoiceSynthesizer IsNot Nothing Then
            Try
                VoiceSynthesizer.Speak(s)
            Catch ex As Exception

            End Try
        End If
    End Sub


    ''' <summary>
    ''' Says "I'll pick it up."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayIPickItUp()
        Say(My.Resources.SAY_IPickItUp)
    End Sub
    ''' <summary>
    ''' Says "Pick it up."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayPickItUp()
        Say(My.Resources.SAY_PickItUp)
    End Sub
    ''' <summary>
    ''' Says "Pass."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayPass()
        Say(My.Resources.SAY_Pass)
    End Sub
    ''' <summary>
    ''' Says "Trump is Hearts."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayHearts()
        Say(My.Resources.SAY_Hearts)
    End Sub
    ''' <summary>
    ''' Says "Trump is Diamonds."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayDiamonds()
        Say(My.Resources.SAY_Diamonds)
    End Sub
    ''' <summary>
    ''' Says "Trump is Clubs."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayClubs()
        Say(My.Resources.SAY_Clubs)
    End Sub
    ''' <summary>
    ''' Says "Trump is Spades."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SaySpades()
        Say(My.Resources.SAY_Spades)
    End Sub
    ''' <summary>
    ''' Says "And I'm going alone."
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayAlone()
        Say(My.Resources.SAY_Alone)
    End Sub

    ''' <summary>
    ''' Apologize for getting euchred after calling
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotEuchredMyFault(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotEuchredMyFault, s))
    End Sub

    ''' <summary>
    ''' Apologize for not having a supporting hand
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotEuchredOurFault(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotEuchredOurFault, s))
    End Sub

    ''' <summary>
    ''' Apologize for not having a supporting hand
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotEuchredYourFault(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotEuchredYourFault, s))
    End Sub

    ''' <summary>
    ''' 1 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotOne(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotOne, s))
    End Sub

    ''' <summary>
    ''' 2 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotTwo(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotTwo, s))
    End Sub

    ''' <summary>
    ''' 4 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeGotFour(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeGotFour, s))
    End Sub
    ''' <summary>
    ''' 1 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayMeGotOne(ByVal s As String)
        Say(String.Format(My.Resources.SAY_MeGotOne, s))
    End Sub

    ''' <summary>
    ''' 2 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayMeGotTwo(ByVal s As String)
        Say(String.Format(My.Resources.SAY_MeGotTwo, s))
    End Sub

    ''' <summary>
    ''' 4 point win
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayMeGotFour(ByVal s As String)
        Say(String.Format(My.Resources.SAY_MeGotFour, s))
    End Sub

    ''' <summary>
    ''' Congratulations
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeWon(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeWon, s))
    End Sub

    ''' <summary>
    ''' Commiserations
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayTheyWon(ByVal s As String)
        Say(String.Format(My.Resources.SAY_TheyWon, s))
    End Sub

    ''' <summary>
    ''' We Euchred Them
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeEuchredThem(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeEuchredThem, s))
    End Sub

    ''' <summary>
    ''' We SuperEuchred Them
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayWeSuperEuchredThem(ByVal s As String)
        Say(String.Format(My.Resources.SAY_WeSuperEuchredThem, s))
    End Sub

    ''' <summary>
    ''' We SuperEuchred Them
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayTheyGotOne()
        Say(My.Resources.SAY_TheyGotOne)
    End Sub

    ''' <summary>
    ''' We SuperEuchred Them
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayTheyGotTwo()
        Say(My.Resources.SAY_TheyGotTwo)
    End Sub

    ''' <summary>
    ''' We SuperEuchred Them
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SayTheyGotFour()
        Say(My.Resources.SAY_TheyGotFour)
    End Sub

End Class
