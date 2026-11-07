' --------------------------------------------------------------------------------------------------------
' Datei: FormAniGif.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormAniGif

    Private _AniGifAnimationNumber As Int32

    Public Sub New()

        Me.InitializeComponent()
        Me.AniGif.AutoPlay = False
        Me.CheckBox_AutoPlay.Checked = Me.AniGif.AutoPlay
        Me.AniGif.CustomDisplaySpeed = False
        Me.CheckBox_CustomDisplaySpeed.Checked = Me.AniGif.CustomDisplaySpeed
        Me.NumericUpDown_CustomDisplaySpeed.Enabled = False
        Me.NumericUpDown_CustomDisplaySpeed.Value = Me.AniGif.FramesPerSecond

#If DEBUG Then
        Debug.Print($"Benutzerdefinierte Anzeigegeschwindigkeit wurde deaktiviert.")
#End If

        Me.ComboBox_SizeMode.SelectedIndex = 0
        Me.NumericUpDown_ZoomFactor.Enabled = False
        Me.NumericUpDown_ZoomFactor.Value = Me.AniGif.ZoomFactor

#If DEBUG Then
        Debug.Print($"Der Anzeigemodus wurde auf normale Anzeige festgelegt.")
#End If

        Me._AniGifAnimationNumber = 0
        Me.Button_Back.Enabled = False
        Me.ChangeAni()

    End Sub

    Private Sub AniGif_NoAnimation(sender As Object, e As EventArgs) Handles AniGif.NoAnimation
        Dim unused = MsgBox($"Das Bild {Me.GetAniResName(Me._AniGifAnimationNumber)} kann nicht animiert werden!", MsgBoxStyle.Information, "AniGif Control")
    End Sub

    Private Sub AniGif_AutoPlayChanged(sender As Object, e As EventArgs) Handles AniGif.AutoPlayChanged
        Me.CheckBox_AutoPlay.Checked = Me.AniGif.AutoPlay
    End Sub

    Private Sub AniGif_AnimationStarted(sender As Object, e As EventArgs) Handles AniGif.AnimationStarted

#If DEBUG Then
        Debug.Print($"Die Animation wurde gestartet.")
#End If

        Me.Button_StartAnimation.Enabled = False
        Me.Button_StopAnimation.Enabled = True
    End Sub

    Private Sub AniGif_AnimationStopped(sender As Object, e As EventArgs) Handles AniGif.AnimationStopped

#If DEBUG Then
        Debug.Print($"Die Animation wurde gestoppt.")
#End If

        Me.Button_StartAnimation.Enabled = True
        Me.Button_StopAnimation.Enabled = False
    End Sub

    Private Sub CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _
            CheckBox_AdaptiveTiming.CheckedChanged, CheckBox_CustomDisplaySpeed.CheckedChanged, CheckBox_AutoPlay.CheckedChanged

        Select Case True

            Case sender Is Me.CheckBox_AdaptiveTiming
                Me.AniGif.AdaptiveTiming = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_AutoPlay
                Me.AniGif.AutoPlay = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_CustomDisplaySpeed
                Me.AniGif.CustomDisplaySpeed = CType(sender, CheckBox).Checked
                Me.AniGif.FramesPerSecond = Me.NumericUpDown_CustomDisplaySpeed.Value
                Me.NumericUpDown_CustomDisplaySpeed.Enabled = CType(sender, CheckBox).Checked

        End Select

    End Sub

    Private Sub NumericUpDown_CustomDisplaySpeed_ValueChanged(sender As Object, e As EventArgs) Handles _
        NumericUpDown_CustomDisplaySpeed.ValueChanged, NumericUpDown_ZoomFactor.ValueChanged

        Select Case True
            Case sender Is Me.NumericUpDown_CustomDisplaySpeed
                Me.AniGif.FramesPerSecond = CType(sender, NumericUpDown).Value

            Case sender Is Me.NumericUpDown_ZoomFactor
                Me.AniGif.ZoomFactor = CType(sender, NumericUpDown).Value

        End Select

    End Sub

    Private Sub ComboBox_SizeMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _
        ComboBox_SizeMode.SelectedIndexChanged

        Me.NumericUpDown_ZoomFactor.Enabled = False
        Select Case CType(sender, ComboBox).SelectedIndex

            Case 0
                Me.AniGif.GifSizeMode = SchlumpfSoft.ControlsNET.AniGifControl.ImageSizeMode.Normal

            Case 1
                Me.AniGif.GifSizeMode = SchlumpfSoft.ControlsNET.AniGifControl.ImageSizeMode.CenterImage

            Case 2
                Me.AniGif.GifSizeMode = SchlumpfSoft.ControlsNET.AniGifControl.ImageSizeMode.Zoom

                Me.NumericUpDown_ZoomFactor.Enabled = True
            Case 3
                Me.AniGif.GifSizeMode = SchlumpfSoft.ControlsNET.AniGifControl.ImageSizeMode.Fill

        End Select
    End Sub

    Private Sub Button_Click(sender As Object, e As EventArgs) Handles _
        Button_Back.Click, Button_Forward.Click, Button_StartAnimation.Click, Button_StopAnimation.Click

        Select Case True

            Case sender Is Me.Button_Back
                If Me._AniGifAnimationNumber > 0 Then Me._AniGifAnimationNumber -= 1
                Me.Button_Forward.Enabled = True
                If Me._AniGifAnimationNumber = 0 Then CType(sender, Button).Enabled = False
                Me.ChangeAni()

            Case sender Is Me.Button_Forward
                If Me._AniGifAnimationNumber < 20 Then Me._AniGifAnimationNumber += 1
                Me.Button_Back.Enabled = True
                If Me._AniGifAnimationNumber = 20 Then CType(sender, Button).Enabled = False
                Me.ChangeAni()

            Case sender Is Me.Button_StartAnimation
                Me.AniGif.StartAnimation()

            Case sender Is Me.Button_StopAnimation
                Me.AniGif.StopAnimation()

        End Select

    End Sub

    Private Sub ChangeAni()
        Me.Label_Animation.Text = Me.GetAniResName(Me._AniGifAnimationNumber)
        Me.AniGif.Gif = CType(My.Resources.ResourceManager.GetObject(Me.GetAniResName(Me._AniGifAnimationNumber)), Bitmap)
    End Sub

    Private Function GetAniResName(AniNumber As Int32) As String
        Return $"AniGif_Anim{CStr(100 + AniNumber)}"
    End Function

End Class