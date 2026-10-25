' --------------------------------------------------------------------------------------------------------
' Datei: FormWizard.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormWizard

    Private oldindex As Integer
    Private newindex As Integer

    Public Sub New()
        Me.InitializeComponent()
    End Sub
    Private Sub Wizard_Help(sender As Object, e As EventArgs) Handles Wizard.Help
        Dim unused = MessageBox.Show($"Hier wird die Hilfe angezeigt", $"Hilfe", MessageBoxButtons.OK, MessageBoxIcon.Question)
    End Sub

    Private Sub Wizard_AfterSwitchPages(sender As Object, e As WizardControl.AfterSwitchPagesEventArgs) Handles Wizard.AfterSwitchPages
        Me.oldindex = e.OldIndex
        Me.newindex = e.NewIndex
        Dim oldpage As String = Me.Wizard.Pages(e.OldIndex).Title
        Dim newpage As String = Me.Wizard.Pages(e.NewIndex).Title
        Dim unused = MessageBox.Show(
            $"Die vorherige Seite war {oldpage} und die aktuelle Seite ist {newpage}",
            Application.ProductName,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)
    End Sub

    Private Sub Wizard_BeforeSwitchPages(sender As Object, e As WizardControl.BeforeSwitchPagesEventArgs) Handles Wizard.BeforeSwitchPages
        Me.oldindex = e.OldIndex
        Me.newindex = e.NewIndex
        Dim oldpage As String = Me.Wizard.Pages(e.OldIndex).Title
        Dim newpage As String = Me.Wizard.Pages(e.NewIndex).Title
        Dim unused = MessageBox.Show(
            $"Die aktuelle Seite ist {oldpage} und es soll zur Seite {newpage} gewechselt werden.",
            Application.ProductName,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)
    End Sub

End Class