' --------------------------------------------------------------------------------------------------------
' Datei: FormImageComboBox.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormImageComboBox

    Public Sub New()
        Me.InitializeComponent()
        Me.LoadImageComboboxElements
        Me.ImageComboBox1.SelectedIndex = 0
    End Sub

    Private Sub LoadImageComboboxElements()
        Dim unused3 = Me.ImageComboBox1.Items.Add(New ImageComboBoxControl.ImageComboBoxItem("Information", My.Resources.ImagecomboBox_Info))
        Dim unused2 = Me.ImageComboBox1.Items.Add(New ImageComboBoxControl.ImageComboBoxItem("Hilfe", My.Resources.ImagecomboBox_Help))
        Dim unused1 = Me.ImageComboBox1.Items.Add(New ImageComboBoxControl.ImageComboBoxItem("Warnung", My.Resources.ImagecomboBox_Warning))
        Dim unused = Me.ImageComboBox1.Items.Add(New ImageComboBoxControl.ImageComboBoxItem("Fehler", My.Resources.ImagecomboBox_Error))
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.TextBox1.Clear()
    End Sub

    Private Sub ImageComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ImageComboBox1.SelectedIndexChanged
        Dim index As Int32 = CType(sender, SchlumpfSoft.ControlsNET.ImageComboBoxControl.ImageComboBox).SelectedIndex
        Dim message As String = $"SelectedIndexChanged meldet: der Index hat sich zu ""{index}"" geändert."
        Me.PrintMessage(message)
    End Sub

    Private Sub ImageComboBox1_TextChanged(sender As Object, e As EventArgs) Handles ImageComboBox1.TextChanged
        Dim text As String = CType(sender, SchlumpfSoft.ControlsNET.ImageComboBoxControl.ImageComboBox).Text
        Dim message As String = $"TextChanged meldet: Der Text hat sich zu ""{text}"" geändert."
        Me.PrintMessage(message)
    End Sub

    Private Sub PrintMessage(message As String)
        Me.TextBox1.AppendText($"{message}{Environment.NewLine}")
    End Sub

End Class