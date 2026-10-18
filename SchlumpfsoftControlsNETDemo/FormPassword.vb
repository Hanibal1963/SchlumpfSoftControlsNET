' --------------------------------------------------------------------------------------------------------
' Datei: FormPassword.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET
Imports SchlumpfSoft.ControlsNET.PasswordControl

Public Class FormPassword

    Private pwspeicher As String = String.Empty

    Public Sub New()
        Me.InitializeComponent()
        Me.InitializeLabels()
    End Sub

    Private Sub InitializeLabels()
        Me.LabelInfo1.Text = My.Resources.Password_LabelInfoText1
        Me.LabelInfo2.Text = My.Resources.Password_LabelInfoText2
        Me.Label_1_PW.Text = My.Resources.Password_LabelPwInput1Text
        Me.Label_2_PW.Text = My.Resources.Password_LabelPwInput2Text
        Me.Label_3_PW.Text = My.Resources.Password_LabelPwInput1Text
    End Sub

    Private Sub FormPassword_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Size = New Size(470, 335)
        Me.LabelMsg1.Text = String.Empty
        Me.LabelMsg1.BackColor = Me.BackColor
        Me.LabelMsg2.Text = String.Empty
        Me.LabelMsg2.BackColor = Me.BackColor
        Me.ButtonOk1.Enabled = False
        Me.ButtonOk2.Enabled = False
        Me.ButtonPwDelete.Enabled = False
        If String.IsNullOrEmpty(My.Settings.Password_PasswordCode) Then
            Me.SetPanel1Active()
        Else
            Me.SetPanel2Active()
        End If
    End Sub

    Private Sub SetPanel1Active()
        Me.Panel1.Dock = DockStyle.Fill
        Me.Panel1.Visible = True
        Me.Panel2.Visible = False
        Me.Password1.ClearPassword()
        Me.Password2.ClearPassword()
        Me.Password2.Enabled = False
        Me.Label_2_PW.Enabled = False
    End Sub

    Private Sub SetPanel2Active()
        Me.Panel2.Dock = DockStyle.Fill
        Me.Panel1.Visible = False
        Me.Panel2.Visible = True
        Me.Password3.ClearPassword()
        Me.pwspeicher = My.Settings.Password_PasswordCode
    End Sub

    Private Sub Password1_PasswortChanged(sender As Object, e As PasswordChangedEventArgs) Handles Password1.PasswortChanged
        Me.pwspeicher = e.PasswordCode
        Me.Label_2_PW.Enabled = True
        Me.Password2.Enabled = True
        Dim unused = Me.Password2.Focus()
    End Sub

    Private Sub Password2_PasswortChanged(sender As Object, e As PasswordChangedEventArgs) Handles Password2.PasswortChanged
        If Me.Password2.VerifyPasswordCode(Me.pwspeicher) = True Then
            Me.LabelMsg1.Text = $"{My.Resources.Password_LabelMsg_Ok} und wird gespeichert."
            Me.LabelMsg1.BackColor = Color.Green
            My.Settings.Password_PasswordCode = Me.pwspeicher
            My.Settings.Save()
            Me.ButtonOk1.Enabled = True
            Dim unused = Me.ButtonOk1.Focus()
        Else
            Me.LabelMsg1.Text = $"{My.Resources.Password_LabelMsg_Fail}."
            Me.LabelMsg1.BackColor = Color.Red
            Me.ButtonOk1.Enabled = False
        End If
    End Sub

    Private Sub Password3_PasswortChanged(sender As Object, e As PasswordChangedEventArgs) Handles Password3.PasswortChanged
        If Me.Password3.VerifyPasswordCode(Me.pwspeicher) = True Then
            Me.LabelMsg2.Text = $"{My.Resources.Password_LabelMsg_Ok}."
            Me.LabelMsg2.BackColor = Color.Green
            Me.ButtonPwDelete.Enabled = True
            Me.ButtonOk2.Enabled = True
        Else
            Me.LabelMsg2.Text = $"{My.Resources.Password_LabelMsg_Fail}."
            Me.LabelMsg2.BackColor = Color.Red
            Me.ButtonPwDelete.Enabled = False
            Me.ButtonOk2.Enabled = False
        End If
    End Sub

    Private Sub ButtonOk_Click(sender As Object, e As EventArgs) Handles ButtonOk1.Click, ButtonOk2.Click
        Me.Close()
    End Sub

    Private Sub ButtonPwDelete_Click(sender As Object, e As EventArgs) Handles ButtonPwDelete.Click
        My.Settings.Password_PasswordCode = String.Empty
        My.Settings.Save()
        Me.Close()
    End Sub

End Class