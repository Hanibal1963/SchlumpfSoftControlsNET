' --------------------------------------------------------------------------------------------------------
' Datei: FormFileList.vb
' Author: Andreas Sauer
' Datum: 08.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormFileList

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Private Sub Button_SelectPath_Click(sender As Object, e As EventArgs) Handles Button_SelectPath.Click
        Dim selectedPath As String
        If Me.FolderBrowserDialog.ShowDialog() = DialogResult.OK Then
            selectedPath = Me.FolderBrowserDialog.SelectedPath
        Else
            Exit Sub
        End If
        Me.Label_SelectedPath.Text = selectedPath
        Me.FileList.StartFolder = selectedPath
    End Sub

End Class