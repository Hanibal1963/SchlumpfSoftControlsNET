' --------------------------------------------------------------------------------------------------------
' Datei: FormColorProgressBar.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormColorProgressBar

    Public Sub New()

        Me.InitializeComponent()
        Me.NumericUpDown_ProgressValue.Value = 10
        Me.ColorProgressBar.Value = CInt(Me.NumericUpDown_ProgressValue.Value)
        Me.ColorProgressBar.ProgressMaximumValue = CInt(Me.NumericUpDown_ProgressValue.Maximum)
        Me.InitializeCheckBoxes()
        Me.InitializeComboBoxes()
    End Sub

    Private Sub InitializeCheckBoxes()
        Me.CheckBox_ShowGliss.Checked = Me.ColorProgressBar.IsGlossy
        Me.CheckBox_ShowBorder.Checked = Me.ColorProgressBar.ShowBorder
        Me.CheckBox_ShowText.Checked = Me.ColorProgressBar.ShowText
        Me.CheckBox_TextFormat.Checked = Me.ColorProgressBar.TextFormat = ColorProgressBarControl.ColorProgressBarTextFormat.Percentage
    End Sub

    Private Sub InitializeComboBoxes()
        Dim unused = Me.ComboBox_Style.Items.Add(ColorProgressBarControl.ColorProgressBarStyle.Solid.ToString)
        Dim unused1 = Me.ComboBox_Style.Items.Add(ColorProgressBarControl.ColorProgressBarStyle.Gradient.ToString)
        Dim unused2 = Me.ComboBox_Style.Items.Add(ColorProgressBarControl.ColorProgressBarStyle.Striped.ToString)
        Dim unused3 = Me.ComboBox_Style.Items.Add(ColorProgressBarControl.ColorProgressBarStyle.Marquee.ToString)
        Me.ComboBox_Style.SelectedIndex = 0
    End Sub

    Private Sub CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _
            CheckBox_ShowGliss.CheckedChanged,
            CheckBox_ShowBorder.CheckedChanged,
            CheckBox_ShowText.CheckedChanged,
            CheckBox_TextFormat.CheckedChanged

        Select Case True

            Case sender Is Me.CheckBox_ShowGliss
                Me.ColorProgressBar.IsGlossy = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_ShowBorder
                Me.ColorProgressBar.ShowBorder = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_ShowText
                Me.ColorProgressBar.ShowText = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_TextFormat
                Me.ColorProgressBar.TextFormat = If(CType(sender, CheckBox).Checked,
                    ColorProgressBarControl.ColorProgressBarTextFormat.Percentage,
                    ColorProgressBarControl.ColorProgressBarTextFormat.Value)

        End Select

    End Sub

    Private Sub NumericUpDown_ProgressValue_ValueChanged(sender As Object, e As EventArgs) Handles _
        NumericUpDown_ProgressValue.ValueChanged

        Me.ColorProgressBar.Value = CInt(CType(sender, NumericUpDown).Value)

    End Sub
    Private Sub Button_BarColorChange_Click(sender As Object, e As EventArgs) Handles _
        Button_BarColorChange.Click,
        Button_BorderColorChange.Click,
        Button_EmptyColorChange.Click

        Dim colordialog As ColorDialog
        Dim dialogresult As DialogResult

        Select Case True

            Case sender Is Me.Button_BarColorChange
                colordialog = New ColorDialog With {.Color = Me.ColorProgressBar.BarColor}
                dialogresult = colordialog.ShowDialog(Me)
                If dialogresult = DialogResult.OK Then
                    Me.ColorProgressBar.BarColor = colordialog.Color
                End If

            Case sender Is Me.Button_BorderColorChange
                colordialog = New ColorDialog With {.Color = Me.ColorProgressBar.BorderColor}
                dialogresult = colordialog.ShowDialog(Me)
                If dialogresult = DialogResult.OK Then
                    Me.ColorProgressBar.BorderColor = colordialog.Color
                End If

            Case sender Is Me.Button_EmptyColorChange
                colordialog = New ColorDialog With {.Color = Me.ColorProgressBar.EmptyColor}
                dialogresult = colordialog.ShowDialog(Me)
                If dialogresult = DialogResult.OK Then
                    Me.ColorProgressBar.EmptyColor = colordialog.Color
                End If

        End Select

    End Sub

    Private Sub ColorProgressBar_Click(sender As Object, e As EventArgs) Handles _
        ColorProgressBar.Click

        Dim msg As String = $"Es wurde auf {CType(sender, ColorProgressBarControl.ColorProgressBar).Name} geklickt."
        Dim unused = MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub

    Private Sub ComboBox_Style_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _
        ComboBox_Style.SelectedIndexChanged

        Select Case CType(sender, ComboBox).SelectedIndex

            Case 0 : Me.ColorProgressBar.ProgressStyle = ColorProgressBarControl.ColorProgressBarStyle.Solid
            Case 1 : Me.ColorProgressBar.ProgressStyle = ColorProgressBarControl.ColorProgressBarStyle.Gradient
            Case 2 : Me.ColorProgressBar.ProgressStyle = ColorProgressBarControl.ColorProgressBarStyle.Striped
            Case 3 : Me.ColorProgressBar.ProgressStyle = ColorProgressBarControl.ColorProgressBarStyle.Marquee

        End Select

    End Sub

End Class