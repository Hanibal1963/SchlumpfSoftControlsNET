' --------------------------------------------------------------------------------------------------------
' Datei: FormNotifyForm.vb
' Author: Andreas Sauer
' Datum: 06.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormNotifyForm

    Public Sub New()
        Me.InitializeComponent()
        Me.TextBox_Title.Text = Me.NotifyForm.Title
        Me.TextBox_Message.Text = Me.NotifyForm.Message
        Me.NumericUpDown_ShowTime.Value = CDec(Me.NotifyForm.ShowTime / 1000)
        Me.ComboBox_Style.SelectedIndex = CInt(Me.NotifyForm.Style)
        Me.ComboBox_Design.SelectedIndex = CInt(Me.NotifyForm.Design)
    End Sub

    Private Sub Button_ShowWindow_Click(sender As Object, e As EventArgs) Handles Button_ShowWindow.Click
        Me.NotifyForm.Title = Me.TextBox_Title.Text
        Me.NotifyForm.Message = Me.TextBox_Message.Text
        Me.NotifyForm.ShowTime = CInt(Me.NumericUpDown_ShowTime.Value * 1000)
        Me.NotifyForm.Style = CType(Me.ComboBox_Style.SelectedIndex, SchlumpfSoft.ControlsNET.NotifyFormControl.NotifyFormStyle)
        Me.NotifyForm.Design = CType(Me.ComboBox_Design.SelectedIndex, SchlumpfSoft.ControlsNET.NotifyFormControl.NotifyFormDesign)
        Me.NotifyForm.Show()
    End Sub

End Class