Imports System.IO
Imports System.Text

Public Class EuchreRules
    Private Sub CloseBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseBtn.Click
        Me.Close()
    End Sub

    Private Sub EuchreRules_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Loaded
        EuchreTable.SetIcon(Me, My.Resources.Euchre)
        ResizeMode = ResizeMode.CanMinimize

        Dim memStream As MemoryStream = New MemoryStream(ASCIIEncoding.UTF8.GetBytes(My.Resources.VBEuchreRules))

        Dim range As TextRange
        range = New TextRange(Me.RtfRules.Document.ContentStart, Me.RtfRules.Document.ContentEnd)
        range.Load(memStream, DataFormats.Rtf)
    End Sub
End Class
