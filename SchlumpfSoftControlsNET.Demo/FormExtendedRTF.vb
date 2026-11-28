' --------------------------------------------------------------------------------------------------------
' Datei: FormExtendedRTF.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormExtendedRTF

    Public Sub New()
        Me.InitializeComponent()
        Me.ExtendedRTF.Rtf = My.Resources.ExtendedRTF_Testdatei.ToString
    End Sub

    Private Sub ToolStripButton_Bold_Click(sender As Object, e As EventArgs) Handles ToolStripButton_Bold.Click
        ' markierten Text fett oder nicht fett
        Me.ExtendedRTF.ToggleBold()
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetBoldCheckedState()
    End Sub

    Private Sub ToolStripButton_Italic_Click(sender As Object, e As EventArgs) Handles ToolStripButton_Italic.Click
        ' markierten Text kursiv oder nicht kursiv
        Me.ExtendedRTF.ToggleItalic()
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetItalicCheckedState()
    End Sub

    Private Sub ToolStripButton_UnderLine_Click(sender As Object, e As EventArgs) Handles ToolStripButton_UnderLine.Click
        ' markierten Text unterstrichen oder nicht unterstrichen
        Me.ExtendedRTF.ToggleUnderline()
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetUnderlineCheckedState()
    End Sub

    Private Sub ToolStripButton_Strikeout_Click(sender As Object, e As EventArgs) Handles ToolStripButton_Strikeout.Click
        ' markierten Text durchgestrichen oder nicht durchgestrichen
        Me.ExtendedRTF.ToggleStrikeout()
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetStrikeoutCheckedState()
    End Sub

    Private Sub ToolStripButton_ToggleBullets_Click(sender As Object, e As EventArgs) Handles ToolStripButton_ToggleBullets.Click
        ' Aufzählungszeichen ein- oder ausschalten
        Me.ExtendedRTF.ToggleBullet()
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetBulletCheckedState()
    End Sub

    Private Sub ToolStripButton_FontSizeUp_Click(sender As Object, e As EventArgs) Handles ToolStripButton_FontSizeUp.Click
        ' markierte Schriftgröße um 1 Punkt vergrößern, max. 72
        ' Begrenzung schützt vor zu großen Schriftgraden
        If Me.ExtendedRTF.SelectionFontSize < 72 Then
            Dim newSize As Single = CSng(Me.ExtendedRTF.SelectionFontSize + 1)
            If newSize > 72 Then newSize = 72
            Me.ExtendedRTF.SelectionFontSize = newSize
        End If
    End Sub

    Private Sub ToolStripButton_FontSizeDown_Click(sender As Object, e As EventArgs) Handles ToolStripButton_FontSizeDown.Click
        ' markierte Schriftgröße um 1 Punkt verkleinern, min. 8
        ' Begrenzung schützt vor zu kleinen Schriftgraden
        If Me.ExtendedRTF.SelectionFontSize > 8 Then
            Dim newSize As Single = CSng(Me.ExtendedRTF.SelectionFontSize - 1)
            If newSize < 8 Then newSize = 8
            Me.ExtendedRTF.SelectionFontSize = newSize
        End If
    End Sub

    Private Sub ToolStripButton_ForeColor_Click(sender As Object, e As EventArgs) Handles ToolStripButton_ForeColor.Click
        ' Farbe für markierten Text auswählen
        ' Der Dialog wird mit der aktuellen Vordergrundfarbe der Auswahl vorbelegt.
        Using colorDialog As New ColorDialog()
            colorDialog.Color = Me.ExtendedRTF.SelectionForeColor
            If colorDialog.ShowDialog(Me) = DialogResult.OK Then
                Me.ExtendedRTF.SelectionColor = colorDialog.Color
            End If
        End Using
    End Sub

    Private Sub ToolStripButton_BackColor_Click(sender As Object, e As EventArgs) Handles ToolStripButton_BackColor.Click
        ' Hintergrundfarbe für markierten Text auswählen
        ' Der Dialog wird mit der aktuellen Hintergrundfarbe der Auswahl vorbelegt.
        Using colorDialog As New ColorDialog()
            colorDialog.Color = Me.ExtendedRTF.SelectionBackColor
            If colorDialog.ShowDialog(Me) = DialogResult.OK Then
                Me.ExtendedRTF.SelectionBackColor = colorDialog.Color
            End If
        End Using
    End Sub

    Private Sub ToolStripButton_FontFormat_Click(sender As Object, e As EventArgs) Handles ToolStripButton_FontFormat.Click
        ' Schriftart für markierten Text auswählen
        ' Der Dialog wird mit der aktuellen Schriftart der Auswahl vorbelegt.
        Using fontDialog As New FontDialog()
            fontDialog.Font = Me.ExtendedRTF.SelectionFont
            If fontDialog.ShowDialog(Me) = DialogResult.OK Then
                Me.ExtendedRTF.SelectionFont = fontDialog.Font
            End If
        End Using
    End Sub

    Private Sub ToolStripButton_DelFormat_Click(sender As Object, e As EventArgs) Handles ToolStripButton_DelFormat.Click
        ' alle Formatierungen des markierten Textes entfernen
        Me.ExtendedRTF.ClearFormatting()
        ' Nach dem Entfernen alle UI-Checkboxen anhand der Selektion aktualisieren
        Me.SetBoldCheckedState()
        Me.SetItalicCheckedState()
        Me.SetUnderlineCheckedState()
        Me.SetStrikeoutCheckedState()
        Me.SetBulletCheckedState()
        Me.SetAlignmentCheckedState()
    End Sub

    Private Sub ToolStripButton_LeftIndentUp_Click(sender As Object, e As EventArgs) Handles ToolStripButton_LeftIndentUp.Click
        ' linken Einzug um 10 Pixel vergrößern (RichTextBox-Indents sind pixelbasiert)
        Me.ExtendedRTF.SelectionIndent += 10
    End Sub

    Private Sub ToolStripButton_LeftIndentDown_Click(sender As Object, e As EventArgs) Handles ToolStripButton_LeftIndentDown.Click
        ' linken Einzug um 10 Pixel verkleinern, min. 0 (kein negativer Einzug)
        Me.ExtendedRTF.SelectionIndent = Math.Max(0, Me.ExtendedRTF.SelectionIndent - 10)
    End Sub

    Private Sub ToolStripButton_TextLeft_Click(sender As Object, e As EventArgs) Handles ToolStripButton_TextLeft.Click
        ' Text linksbündig
        Me.ExtendedRTF.SelectionAlignment = HorizontalAlignment.Left
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetAlignmentCheckedState()
    End Sub

    Private Sub ToolStripButton_TextCenter_Click(sender As Object, e As EventArgs) Handles ToolStripButton_TextCenter.Click
        ' Text zentriert
        Me.ExtendedRTF.SelectionAlignment = HorizontalAlignment.Center
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetAlignmentCheckedState()
    End Sub

    Private Sub ToolStripButton_TextRight_Click(sender As Object, e As EventArgs) Handles ToolStripButton_TextRight.Click
        ' Text rechtsbündig
        Me.ExtendedRTF.SelectionAlignment = HorizontalAlignment.Right
        ' UI-Status (Häkchen) nach der Aktion synchronisieren
        Me.SetAlignmentCheckedState()
    End Sub

    Private Sub ExtendedRTF_SelectionChanged(sender As Object, e As EventArgs) Handles ExtendedRTF.SelectionChanged
        ' Wenn sich die Textauswahl ändert, müssen die UI-Checkboxen den neuen Status der Auswahl widerspiegeln.
        Me.SetBoldCheckedState()
        Me.SetItalicCheckedState()
        Me.SetUnderlineCheckedState()
        Me.SetStrikeoutCheckedState()
        Me.SetBulletCheckedState()
        Me.SetAlignmentCheckedState()
    End Sub

    Private Sub SetAlignmentCheckedState()
        ' Synchronisiert den Checked-Status  für Textausrichtung (Alignment).
        Select Case Me.ExtendedRTF.SelectionAlignment
            Case HorizontalAlignment.Left
                Me.SetLeftAlignmentCheckedState()
            Case HorizontalAlignment.Center
                Me.SetCenterAlignmentCheckedState()
            Case HorizontalAlignment.Right
                Me.SetRightAlignmentCheckedState()
        End Select
    End Sub

    Private Sub SetRightAlignmentCheckedState()
        ' Synchronisiert den Checked-Status für rechtsbündige Textausrichtung (Right).
        Me.ToolStripButton_TextLeft.Checked = False
        Me.ToolStripButton_TextCenter.Checked = False
        Me.ToolStripButton_TextRight.Checked = True
    End Sub

    Private Sub SetCenterAlignmentCheckedState()
        ' Synchronisiert den Checked-Status für zentrierte Textausrichtung (Center).
        Me.ToolStripButton_TextLeft.Checked = False
        Me.ToolStripButton_TextCenter.Checked = True
        Me.ToolStripButton_TextRight.Checked = False
    End Sub

    Private Sub SetLeftAlignmentCheckedState()
        ' Synchronisiert den Checked-Status für linksbündige Textausrichtung (Left).
        Me.ToolStripButton_TextLeft.Checked = True
        Me.ToolStripButton_TextCenter.Checked = False
        Me.ToolStripButton_TextRight.Checked = False
    End Sub

    Private Sub SetBulletCheckedState()
        ' Synchronisiert den Checked-Status  für Aufzählungszeichen (Bullet).
        Me.ToolStripButton_ToggleBullets.Checked = Me.ExtendedRTF.SelectionBullet
    End Sub

    Private Sub SetStrikeoutCheckedState()
        ' Synchronisiert den Checked-Status für Durchstreichung (Strikeout).
        Me.ToolStripButton_Strikeout.Checked = Me.ExtendedRTF.SelectionStrikeout.GetValueOrDefault(False)
    End Sub

    Private Sub SetUnderlineCheckedState()
        ' Synchronisiert den Checked-Status für Unterstreichung (Underline).
        Me.ToolStripButton_UnderLine.Checked = Me.ExtendedRTF.SelectionUnderline.GetValueOrDefault(False)
    End Sub

    Private Sub SetItalicCheckedState()
        ' Synchronisiert den Checked-Status für Kursivschrift (Italic).
        Me.ToolStripButton_Italic.Checked = Me.ExtendedRTF.SelectionItalic.GetValueOrDefault(False)
    End Sub

    Private Sub SetBoldCheckedState()
        ' Synchronisiert den Checked-Status für Fettschrift (Bold).
        Me.ToolStripButton_Bold.Checked = Me.ExtendedRTF.SelectionBold.GetValueOrDefault(False)
    End Sub

End Class