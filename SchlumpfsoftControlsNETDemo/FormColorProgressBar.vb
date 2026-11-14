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
        Me.CheckBox_ShowGliss.Checked = Me.ColorProgressBar.IsGlossy
        Me.CheckBox_ShowBorder.Checked = Me.ColorProgressBar.ShowBorder

    End Sub

    Private Sub CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _
            CheckBox_ShowGliss.CheckedChanged,
            CheckBox_ShowBorder.CheckedChanged

        Select Case True

            Case sender Is Me.CheckBox_ShowGliss
                Me.ColorProgressBar.IsGlossy = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_ShowBorder
                Me.ColorProgressBar.ShowBorder = CType(sender, CheckBox).Checked

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

        Select Case True

            Case sender Is Me.Button_BarColorChange
                Me.ColorDialog.Color = Me.ColorProgressBar.BarColor
                If Me.ColorDialog.ShowDialog(Me) = DialogResult.OK Then
                    Me.ColorProgressBar.BarColor = Me.ColorDialog.Color
                End If

            Case sender Is Me.Button_BorderColorChange
                Me.ColorDialog.Color = Me.ColorProgressBar.BorderColor
                If Me.ColorDialog.ShowDialog(Me) = DialogResult.OK Then
                    Me.ColorProgressBar.BorderColor = Me.ColorDialog.Color
                End If


            Case sender Is Me.Button_EmptyColorChange
                Me.ColorDialog.Color = Me.ColorProgressBar.EmptyColor
                If Me.ColorDialog.ShowDialog(Me) = DialogResult.OK Then
                    Me.ColorProgressBar.EmptyColor = Me.ColorDialog.Color
                End If

        End Select

    End Sub

    Private Sub ColorProgressBar_Click(sender As Object, e As EventArgs) Handles _
        ColorProgressBar.Click

        Dim msg As String = $"Es wurde auf {CType(sender, ColorProgressBarControl.ColorProgressBar).Name} geklickt."
        Dim unused = MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub

End Class