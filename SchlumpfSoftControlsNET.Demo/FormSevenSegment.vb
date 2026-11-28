' --------------------------------------------------------------------------------------------------------
' Datei: FormSevenSegment.vb
' Author: Andreas Sauer
' Datum: 08.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormSevenSegment

    Private ReadOnly _inaktivecolor As Color = Color.Gray
    Private ReadOnly _aktivecolor As Color = Color.Green

    Public Sub New()
        Me.InitializeComponent()
        Me.SingleDigit.InactiveColor = Me._inaktivecolor
        Me.SingleDigit.ForeColor = Me._aktivecolor
        Me.MultiDigit.InactiveColor = Me._inaktivecolor
        Me.MultiDigit.ForeColor = Me._aktivecolor
    End Sub

    Private Sub TextBox_SingleDigit_TextChanged(sender As Object, e As EventArgs) Handles TextBox_SingleDigit.TextChanged
        Me.SingleDigit.DigitValue = CType(sender, TextBox).Text
    End Sub

    Private Sub TextBox_MultiDigit_TextChanged(sender As Object, e As EventArgs) Handles TextBox_MultiDigit.TextChanged
        Me.MultiDigit.Value = CType(sender, TextBox).Text
    End Sub

    Private Sub Button_InactiveColor_Click(sender As Object, e As EventArgs) Handles Button_InactiveColor.Click
        Me.ColorDialog.Color = Me._inaktivecolor
        If Me.ColorDialog.ShowDialog() = DialogResult.OK Then
            Me.MultiDigit.InactiveColor = Me.ColorDialog.Color
            Me.SingleDigit.InactiveColor = Me.ColorDialog.Color
        End If
    End Sub

    Private Sub Button_ForeColor_Click(sender As Object, e As EventArgs) Handles Button_ForeColor.Click
        Me.ColorDialog.Color = Me._aktivecolor
        If Me.ColorDialog.ShowDialog() = DialogResult.OK Then
            Me.MultiDigit.ForeColor = Me.ColorDialog.Color
            Me.SingleDigit.ForeColor = Me.ColorDialog.Color
        End If
    End Sub

End Class