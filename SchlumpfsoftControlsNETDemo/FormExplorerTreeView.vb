' --------------------------------------------------------------------------------------------------------
' Datei: FormExplorerTreeView.vb
' Author: Andreas Sauer
' Datum: 08.09.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Imports SchlumpfSoft.ControlsNET

Public Class FormExplorerTreeView

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Private Sub ExplorerTreeView_SelectedPathChanged(sender As Object, e As ExplorerTreeViewControl.SelectedPathChangedEventArgs) Handles ExplorerTreeView.SelectedPathChanged, ExplorerTreeView.SelectedPathChanged
        Dim selpath As String = e.SelectedPath ' aktuell ausgewählten Pfad anzeigen
        Dim text As String = $"{Me.TextBox.Text}{vbCrLf}Aktuell ausgewählter Pfad: {selpath}"
        Me.TextBox.Text = text
        Me.ListView.Items.Clear() ' Liste leeren
        Try
            For Each dir As String In Directory.GetDirectories(selpath)  ' Unterverzeichnisse auslesen
                Dim unused1 = Me.ListView.Items.Add(Me.GetName(dir))
            Next
            For Each file As String In Directory.GetFiles(selpath) ' Dateien auslesen
                Dim unused2 = Me.ListView.Items.Add(Me.GetName(file))
            Next
        Catch ex As UnauthorizedAccessException
            ' geschützter Ordner: ignorieren oder Hinweis anzeigen
        Catch ex As IOException
        End Try
    End Sub

    Private Sub ButtonSearchPath_Click(sender As Object, e As EventArgs) Handles ButtonSearchPath.Click
        Dim result As DialogResult = Me.FolderBrowserDialog.ShowDialog(Me)
        If result = DialogResult.OK Then
            Me.LabelPath.Text = Me.FolderBrowserDialog.SelectedPath
            Dim unused = Me.ExplorerTreeView.ExpandPath(Me.FolderBrowserDialog.SelectedPath)
        End If
    End Sub

    Private Function GetName(FileOrDirectory As String) As String
        Return Path.GetFileName(FileOrDirectory)
    End Function

End Class