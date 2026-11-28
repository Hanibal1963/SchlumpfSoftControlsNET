' --------------------------------------------------------------------------------------------------------
' Datei: FormIniFile.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormIniFile

    Public Sub New()
        Me.InitializeComponent()

#Region "Dateipfad einstellen und neue Datei erzeugen"
        If String.IsNullOrEmpty(My.Settings.IniFile_IniPath) Then
            My.Settings.IniFile_IniPath = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        End If
        Me.IniFile.FilePath = My.Settings.IniFile_IniPath
        Me.IniFile.CreateNewFile()
#End Region

    End Sub

#Region "Behandlung von FormMain Ereignissen"

    Private Sub ToolStripMenuItemOeffnen_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemOeffnen.Click
        Dim ofd As New OpenFileDialog With {
            .Filter = "INI-Dateien (*.ini)|*.ini|Alle Dateien (*.*)|*.*",
            .InitialDirectory = My.Settings.IniFile_IniPath,
            .Title = "INI-Datei öffnen"}
        Dim result As DialogResult = ofd.ShowDialog
        If result = DialogResult.OK Then
            Me.IniFile.FileName = ofd.FileName
            Me.IniFile.LoadFile()
        Else
        End If
    End Sub

    Private Sub ToolStripMenuItemSchliessen_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemSchliessen.Click
        If Me.IniFile.FileSaved Then
            Me.IniFile.CreateNewFile()
        Else
            Dim result As DialogResult = MessageBox.Show(
                "Die aktuelle INI-Datei wurde geändert, aber noch nicht gespeichert. Möchten Sie die Änderungen speichern?",
                "Änderungen speichern?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning)
            If result = DialogResult.Yes Then
                Me.IniFile.SaveFile()
                Me.IniFile.CreateNewFile()
            ElseIf result = DialogResult.No Then
                Me.IniFile.CreateNewFile()
            End If
        End If
    End Sub

    Private Sub BeendenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemBeenden.Click
        If Me.IniFile.FileSaved Then
            Me.IniFile.Dispose()
        Else
            Dim result As DialogResult = MessageBox.Show(
                "Die aktuelle INI-Datei wurde geändert, aber noch nicht gespeichert. Möchten Sie die Änderungen speichern?",
                "Änderungen speichern?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning)
            If result = DialogResult.Yes Then
                Me.IniFile.SaveFile()
                Me.IniFile.Dispose()
            ElseIf result = DialogResult.No Then
                Me.IniFile.Dispose()
            End If
        End If
        Me.Close()
    End Sub

    Private Sub ToolStripMenuItemSpeichern_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemSpeichern.Click
        Me.IniFile.SaveFile()
    End Sub

    Private Sub ToolStripMenuItemSpeichernUnter_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItemSpeichernUnter.Click
        Dim sfd As New SaveFileDialog With {
            .Filter = "INI-Dateien (*.ini)|*.ini|Alle Dateien (*.*)|*.*",
            .InitialDirectory = My.Settings.IniFile_IniPath,
            .Title = "INI-Datei speichern unter"}
        Dim result As DialogResult = sfd.ShowDialog
        If result = DialogResult.OK Then
            Me.IniFile.FileName = sfd.FileName
            Me.IniFile.SaveFile()
        Else
        End If
    End Sub

#End Region

#Region "Behandlung von InFile Ereignissen"

    Private Sub IniFile_FileContentChanged(sender As Object, e As EventArgs) Handles IniFile.FileContentChanged
        Me.ContentView.Lines = Me.IniFile.GetFileContent
        Me.CommentEditFileComment.Comment = Me.IniFile.GetFileComment
        Me.ListEditSections.ListItems = Me.IniFile.GetSectionNames()
    End Sub

    Private Sub IniFile_EntryNameExist(sender As Object, e As EventArgs) Handles IniFile.EntryNameExist
        Dim unused = MessageBox.Show(
            "Der Eintragsname existiert bereits.",
            "Fehlerhafte Eingabe",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)
    End Sub

    Private Sub IniFile_SectionNameExist(sender As Object, e As EventArgs) Handles IniFile.SectionNameExist
        Dim unused = MessageBox.Show(
            "Der Abschnittsname existiert bereits.",
            "Fehlerhafte Eingabe",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)
    End Sub

#End Region

#Region "Behandlung von FileCommentEdit Ereignissen"

    Private Sub CommentEditFileComment_CommentChanged(sender As Object, e As IniFileControl.CommentEditEventArgs) Handles CommentEditFileComment.CommentChanged

        Me.IniFile.SetFileComment(e.Comment)

    End Sub

#End Region

#Region "Behandlung von SectionCommentEdit Ereignissen"

    Private Sub CommentEditSections_CommentChanged(sender As Object, e As IniFileControl.CommentEditEventArgs) Handles CommentEditSections.CommentChanged
        Me.IniFile.SetSectionComment(Me.ListEditSections.SelectedElement, e.Comment)
    End Sub

#End Region

#Region "Behandlung von ListEditSections Ereignissen"

    Private Sub ListEditSections_ItemAdd(sender As Object, e As IniFileControl.ListEditAddEventArgs) Handles ListEditSections.ItemAdd
        Me.IniFile.AddSection(e.ItemToAdd)
    End Sub

    Private Sub ListEditSections_ItemRemove(sender As Object, e As IniFileControl.ListEditRemoveEventArgs) Handles ListEditSections.ItemRemove
        Me.IniFile.DeleteSection(e.ItemToRemove)
    End Sub

    Private Sub ListEditSections_ItemRename(sender As Object, e As IniFileControl.ListEditRenameEventArgs) Handles ListEditSections.ItemRename
        Me.IniFile.RenameSection(e.OldName, e.NewName)
    End Sub

    Private Sub ListEditSections_SelectedItemChanged(sender As Object, e As IniFileControl.ListEditSelectedElementChangedEventArgs) Handles ListEditSections.SelectedItemChanged
        If String.IsNullOrWhiteSpace(e.SelectedElement) Then
            Me.CommentEditSections.TitelText = "kein Abschnitt ausgewählt"
            Me.CommentEditSections.Comment = {""}
            Me.CommentEditSections.Enabled = False
            Me.ListEditEntrys.ListItems = Nothing
            Me.ListEditEntrys.TitelText = "kein Abschnitt ausgewählt"
        Else
            Me.CommentEditSections.TitelText = $"Abschnittskommentar für [{e.SelectedElement}]"
            Me.CommentEditSections.Comment = Me.IniFile.GetSectionComment(e.SelectedElement)
            Me.CommentEditSections.Enabled = True
            Me.ListEditEntrys.TitelText = $"Einträge der Sektion [{e.SelectedElement}] bearbeiten."
            Me.ListEditEntrys.ListItems = Me.IniFile.GetEntryNames(e.SelectedElement)
        End If
    End Sub

#End Region

#Region "Behandlung von ListEditEntrys Ereignissen"

    Private Sub ListEditEntrys_ItemAdd(sender As Object, e As IniFileControl.ListEditAddEventArgs) Handles ListEditEntrys.ItemAdd
        Me.IniFile.AddEntry(Me.ListEditSections.SelectedElement, e.ItemToAdd)
    End Sub

    Private Sub ListEditEntrys_ItemRemove(sender As Object, e As IniFileControl.ListEditRemoveEventArgs) Handles ListEditEntrys.ItemRemove
        Me.IniFile.DeleteEntry(Me.ListEditSections.SelectedElement, e.ItemToRemove)
    End Sub

    Private Sub ListEditEntrys_ItemRename(sender As Object, e As IniFileControl.ListEditRenameEventArgs) Handles ListEditEntrys.ItemRename
        Me.IniFile.RenameEntry(Me.ListEditSections.SelectedElement, e.OldName, e.NewName)
    End Sub

    Private Sub ListEditEntrys_SelectedItemChanged(sender As Object, e As IniFileControl.ListEditSelectedElementChangedEventArgs) Handles ListEditEntrys.SelectedItemChanged
        If String.IsNullOrWhiteSpace(e.SelectedElement) Then
            Me.EntryValueEdit.TitelText = "kein Eintrag ausgewählt"
            Me.EntryValueEdit.Value = String.Empty
            Me.EntryValueEdit.Enabled = False
        Else
            Me.EntryValueEdit.TitelText = $"Wert für Eintrag [{e.SelectedElement}] bearbeiten."
            Me.EntryValueEdit.Value = Me.IniFile.GetEntryValue(Me.ListEditSections.SelectedElement, e.SelectedElement)
            Me.EntryValueEdit.Enabled = True
        End If
    End Sub

#End Region

#Region "Behandlung von EntryValueEdit Ereignissen"

    Private Sub EntryValueEdit_ValueChanged(sender As Object, e As IniFileControl.EntryValueEditEventArgs) Handles EntryValueEdit.ValueChanged
        Me.IniFile.SetEntryValue(Me.ListEditSections.SelectedElement, Me.ListEditEntrys.SelectedElement, e.NewValue)
    End Sub

#End Region

End Class