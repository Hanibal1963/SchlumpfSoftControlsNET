' --------------------------------------------------------------------------------------------------------
' Datei: FormAniGif.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET
Imports SchlumpfSoft.ControlsNET.AniGifControl

Public Class FormAniGif

    Private _AniGifAnimationNumber As Int32

    Public Sub New()

        Me.InitializeComponent()
        Me.AniGif.AutoPlay = False
        Me.AniGif.CustomDisplaySpeed = False
        Me.CheckBox_AutoPlay.Checked = Me.AniGif.AutoPlay
        Me.CheckBox_CustomDisplaySpeed.Checked = Me.AniGif.CustomDisplaySpeed
        Me.CheckBox_Diagnostics.Checked = Me.AniGif.DiagnosticsEnabled
        Me.CheckBox_PartialFrameInvalidation.Checked = Me.AniGif.UsePartialFrameInvalidation
        Me.Button_Diagnostics.Enabled = Me.AniGif.DiagnosticsEnabled
        Me.ComboBox_ScalingQualityPreset.SelectedIndex = 0
        Me.ComboBox_SizeMode.SelectedIndex = 0
        Me.NumericUpDown_CustomDisplaySpeed.Enabled = False
        Me.NumericUpDown_CustomDisplaySpeed.Value = Me.AniGif.FramesPerSecond
        Me.NumericUpDown_ZoomFactor.Enabled = False
        Me.NumericUpDown_ZoomFactor.Value = Me.AniGif.ZoomFactor
        Me._AniGifAnimationNumber = 0
        Me.Button_Back.Enabled = False
        Me.ChangeAni()

    End Sub

    Private Sub AniGif_NoAnimation(sender As Object, e As EventArgs) Handles AniGif.NoAnimation

        Dim msg As String = $"Das Bild {Me.GetAniResName(Me._AniGifAnimationNumber)} kann nicht animiert werden!"
        Me.Label_EventMessage.Text = msg

    End Sub

    Private Sub AniGif_AutoPlayChanged(sender As Object, e As EventArgs) Handles AniGif.AutoPlayChanged

        Dim autoplay As Boolean = Me.AniGif.AutoPlay
        Dim msg = If(autoplay, $"Die Animation wird automatisch abgespielt.", $"Die Animation wird nicht automatisch abgespielt.")

        Me.Label_EventMessage.Text = msg
        Me.CheckBox_AutoPlay.Checked = autoplay

    End Sub

    Private Sub AniGif_AnimationStarted(sender As Object, e As EventArgs) Handles AniGif.AnimationStarted

        Dim msg As String
        msg = $"Die Animation wird abgespielt."
        Me.Label_EventMessage.Text = msg
        Me.Button_StartAnimation.Enabled = False
        Me.Button_StopAnimation.Enabled = True
        Me.Button_Resume.Enabled = False
        Me.Button_Pause.Enabled = True

    End Sub

    Private Sub AniGif_AnimationStopped(sender As Object, e As EventArgs) Handles AniGif.AnimationStopped

        Dim msg As String = $""

        Select Case Me.AniGif.GetPlaybackState

            Case PlaybackState.Paused
                msg = $"Die Animation wurde pausiert."
                Me.Button_Resume.Enabled = True
                Me.Button_Pause.Enabled = False
                Me.Button_StartAnimation.Enabled = False
                Me.Button_StopAnimation.Enabled = True

            Case PlaybackState.Stopped
                msg = $"Die Animation wurde gestoppt."
                Me.Button_StartAnimation.Enabled = True
                Me.Button_StopAnimation.Enabled = False
                Me.Button_Resume.Enabled = False
                Me.Button_Pause.Enabled = False

        End Select

        Me.Label_EventMessage.Text = msg

    End Sub

    Private Sub CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles _
            CheckBox_AdaptiveTiming.CheckedChanged,
            CheckBox_CustomDisplaySpeed.CheckedChanged,
            CheckBox_AutoPlay.CheckedChanged,
            CheckBox_Diagnostics.CheckedChanged,
            CheckBox_PartialFrameInvalidation.CheckedChanged

        Select Case True

            Case sender Is Me.CheckBox_AdaptiveTiming
                Me.AniGif.AdaptiveTiming = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_CustomDisplaySpeed
                Me.AniGif.CustomDisplaySpeed = CType(sender, CheckBox).Checked
                Me.AniGif.FramesPerSecond = Me.NumericUpDown_CustomDisplaySpeed.Value
                Me.NumericUpDown_CustomDisplaySpeed.Enabled = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_AutoPlay
                Me.AniGif.AutoPlay = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_Diagnostics
                Me.AniGif.DiagnosticsEnabled = CType(sender, CheckBox).Checked
                Me.Button_Diagnostics.Enabled = CType(sender, CheckBox).Checked

            Case sender Is Me.CheckBox_PartialFrameInvalidation
                Me.AniGif.UsePartialFrameInvalidation = CType(sender, CheckBox).Checked

        End Select

    End Sub

    Private Sub NumericUpDown_CustomDisplaySpeed_ValueChanged(sender As Object, e As EventArgs) Handles _
        NumericUpDown_CustomDisplaySpeed.ValueChanged,
        NumericUpDown_ZoomFactor.ValueChanged

        Select Case True

            Case sender Is Me.NumericUpDown_CustomDisplaySpeed
                Me.AniGif.FramesPerSecond = CType(sender, NumericUpDown).Value

            Case sender Is Me.NumericUpDown_ZoomFactor
                Me.AniGif.ZoomFactor = CType(sender, NumericUpDown).Value

        End Select

    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _
        ComboBox_SizeMode.SelectedIndexChanged,
        ComboBox_ScalingQualityPreset.SelectedIndexChanged

        Select Case True
            Case sender Is Me.ComboBox_SizeMode
                Me.NumericUpDown_ZoomFactor.Enabled = False
                Select Case CType(sender, ComboBox).SelectedIndex

                    Case 0
                        Me.AniGif.GifSizeMode = AniGifControl.ImageSizeMode.Normal
                        Me.ComboBox_ScalingQualityPreset.Enabled = False

                    Case 1
                        Me.AniGif.GifSizeMode = AniGifControl.ImageSizeMode.CenterImage
                        Me.ComboBox_ScalingQualityPreset.Enabled = False

                    Case 2
                        Me.AniGif.GifSizeMode = AniGifControl.ImageSizeMode.Zoom
                        Me.ComboBox_ScalingQualityPreset.Enabled = True
                        Me.ComboBox_ScalingQualityPreset.SelectedIndex = 0
                        Me.NumericUpDown_ZoomFactor.Enabled = True

                    Case 3
                        Me.AniGif.GifSizeMode = AniGifControl.ImageSizeMode.Fill
                        Me.ComboBox_ScalingQualityPreset.Enabled = False
                        Me.ComboBox_ScalingQualityPreset.SelectedIndex = 0

                End Select

            Case sender Is Me.ComboBox_ScalingQualityPreset

                Select Case CType(sender, ComboBox).SelectedIndex

                    Case 0
                        Me.AniGif.ScalingQualityPreset = AniGifControl.GifScalingQualityPreset.Photo

                    Case 1
                        Me.AniGif.ScalingQualityPreset = AniGifControl.GifScalingQualityPreset.PixelArt

                End Select

        End Select

    End Sub

    Private Sub Button_Click(sender As Object, e As EventArgs) Handles _
        Button_Back.Click,
        Button_Forward.Click,
        Button_StartAnimation.Click,
        Button_Pause.Click,
        Button_Resume.Click,
        Button_StopAnimation.Click,
        Button_Diagnostics.Click

        Select Case True

            Case sender Is Me.Button_Back
                Me.ShowBackAni(sender)

            Case sender Is Me.Button_Forward
                Me.ShowNextAni(sender)

            Case sender Is Me.Button_Pause
                Me.AniGif.PauseAnimation()

            Case sender Is Me.Button_Resume
                Me.AniGif.ResumeAnimation()

            Case sender Is Me.Button_StartAnimation
                Me.AniGif.StartAnimation()

            Case sender Is Me.Button_StopAnimation
                Me.AniGif.StopAnimation()

            Case sender Is Me.Button_Diagnostics
                Me.ShowDiagnostics()

        End Select

    End Sub

    Private Sub ShowBackAni(sender As Object)

        If Me._AniGifAnimationNumber > 0 Then Me._AniGifAnimationNumber -= 1
        Me.Button_Forward.Enabled = True
        If Me._AniGifAnimationNumber = 0 Then CType(sender, Button).Enabled = False
        Me.ChangeAni()

    End Sub

    Private Sub ShowNextAni(sender As Object)

        If Me._AniGifAnimationNumber < 20 Then Me._AniGifAnimationNumber += 1
        Me.Button_Back.Enabled = True
        If Me._AniGifAnimationNumber = 20 Then CType(sender, Button).Enabled = False
        Me.ChangeAni()

    End Sub

    Private Sub ShowDiagnostics()

        Dim msg As String = $""

        With Me.AniGif.Diagnostics

            msg &= $"Kumulierte aktive und zeichnungsfähige Wiedergabezeit: { .ActiveDuration} {vbCrLf}"
            msg &= $"Mittlere Anzahl gezeichneter Timer-Frames pro aktiver Sekunde:  { .ActualFps}{vbCrLf}"
            msg &= $"Anzahl mindestens einmal gezeichneter Timer-Frames: { .RenderedFrames}{vbCrLf}"
            msg &= $"Anzahl vor dem Zeichnen durch spätere Timer-Frames ersetzter Frames: { .DroppedFrames}{vbCrLf}"
            msg &= $"Anzahl erfolgreicher GIF-Zeichenaufrufe einschließlich Repaints: { .RenderOperations}{vbCrLf}"
            msg &= $"Mittlere Dauer eines Zeichenaufrufs in Millisekunden: { .AverageRenderTimeMs}{vbCrLf}"
            msg &= $"Längste Dauer eines Zeichenaufrufs in Millisekunden: { .MaxRenderTimeMs}{vbCrLf}"

        End With

        Me.Label_EventMessage.Text = msg

    End Sub

    Private Sub ChangeAni()

        Dim aniresname As String = Me.GetAniResName(Me._AniGifAnimationNumber)
        Me.Label_Animation.Text = $"Es wird ""{aniresname}"" angezeigt."
        Me.AniGif.Gif = CType(My.Resources.ResourceManager.GetObject(aniresname), Bitmap)

    End Sub

    Private Function GetAniResName(AniNumber As Int32) As String
        Return $"AniGif_Anim{CStr(100 + AniNumber)}"
    End Function


End Class