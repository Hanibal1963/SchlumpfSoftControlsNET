<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormAniGif
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormAniGif))
        Me.AniGif = New SchlumpfSoft.ControlsNET.AniGifControl.AniGif()
        Me.CheckBox_AutoPlay = New System.Windows.Forms.CheckBox()
        Me.CheckBox_CustomDisplaySpeed = New System.Windows.Forms.CheckBox()
        Me.NumericUpDown_CustomDisplaySpeed = New System.Windows.Forms.NumericUpDown()
        Me.Label_Animation = New System.Windows.Forms.Label()
        Me.Button_Back = New System.Windows.Forms.Button()
        Me.Button_Forward = New System.Windows.Forms.Button()
        Me.NumericUpDown_ZoomFactor = New System.Windows.Forms.NumericUpDown()
        Me.ComboBox_SizeMode = New System.Windows.Forms.ComboBox()
        Me.Label_FramesPerSecound = New System.Windows.Forms.Label()
        Me.Label_Zoomfaktor = New System.Windows.Forms.Label()
        Me.Button_StartAnimation = New System.Windows.Forms.Button()
        Me.Button_StopAnimation = New System.Windows.Forms.Button()
        Me.CheckBox_AdaptiveTiming = New System.Windows.Forms.CheckBox()
        CType(Me.NumericUpDown_CustomDisplaySpeed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_ZoomFactor, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'AniGif
        '
        Me.AniGif.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.AniGif.FramesPerSecond = New Decimal(New Integer() {10, 0, 0, 0})
        Me.AniGif.Gif = CType(resources.GetObject("AniGif.Gif"), System.Drawing.Bitmap)
        Me.AniGif.Location = New System.Drawing.Point(12, 12)
        Me.AniGif.Name = "AniGif"
        Me.AniGif.Padding = New System.Windows.Forms.Padding(0)
        Me.AniGif.Size = New System.Drawing.Size(247, 232)
        Me.AniGif.TabIndex = 0
        Me.AniGif.ZoomFactor = New Decimal(New Integer() {50, 0, 0, 0})
        '
        'CheckBox_AutoPlay
        '
        Me.CheckBox_AutoPlay.AutoSize = True
        Me.CheckBox_AutoPlay.Location = New System.Drawing.Point(280, 15)
        Me.CheckBox_AutoPlay.Name = "CheckBox_AutoPlay"
        Me.CheckBox_AutoPlay.Size = New System.Drawing.Size(67, 17)
        Me.CheckBox_AutoPlay.TabIndex = 1
        Me.CheckBox_AutoPlay.Text = "Autoplay"
        Me.CheckBox_AutoPlay.UseVisualStyleBackColor = True
        '
        'CheckBox_CustomDisplaySpeed
        '
        Me.CheckBox_CustomDisplaySpeed.AutoSize = True
        Me.CheckBox_CustomDisplaySpeed.Location = New System.Drawing.Point(280, 38)
        Me.CheckBox_CustomDisplaySpeed.Name = "CheckBox_CustomDisplaySpeed"
        Me.CheckBox_CustomDisplaySpeed.Size = New System.Drawing.Size(228, 17)
        Me.CheckBox_CustomDisplaySpeed.TabIndex = 2
        Me.CheckBox_CustomDisplaySpeed.Text = "Benutzerdefinierte Anzeigegeschwindigkeit"
        Me.CheckBox_CustomDisplaySpeed.UseVisualStyleBackColor = True
        '
        'NumericUpDown_CustomDisplaySpeed
        '
        Me.NumericUpDown_CustomDisplaySpeed.Location = New System.Drawing.Point(280, 150)
        Me.NumericUpDown_CustomDisplaySpeed.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.NumericUpDown_CustomDisplaySpeed.Name = "NumericUpDown_CustomDisplaySpeed"
        Me.NumericUpDown_CustomDisplaySpeed.Size = New System.Drawing.Size(47, 20)
        Me.NumericUpDown_CustomDisplaySpeed.TabIndex = 3
        Me.NumericUpDown_CustomDisplaySpeed.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label_Animation
        '
        Me.Label_Animation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Label_Animation.Location = New System.Drawing.Point(12, 260)
        Me.Label_Animation.Name = "Label_Animation"
        Me.Label_Animation.Size = New System.Drawing.Size(496, 16)
        Me.Label_Animation.TabIndex = 4
        '
        'Button_Back
        '
        Me.Button_Back.Location = New System.Drawing.Point(12, 290)
        Me.Button_Back.Name = "Button_Back"
        Me.Button_Back.Size = New System.Drawing.Size(108, 24)
        Me.Button_Back.TabIndex = 5
        Me.Button_Back.Text = "<< zurück"
        Me.Button_Back.UseVisualStyleBackColor = True
        '
        'Button_Forward
        '
        Me.Button_Forward.Location = New System.Drawing.Point(126, 290)
        Me.Button_Forward.Name = "Button_Forward"
        Me.Button_Forward.Size = New System.Drawing.Size(108, 24)
        Me.Button_Forward.TabIndex = 6
        Me.Button_Forward.Text = "weiter >>"
        Me.Button_Forward.UseVisualStyleBackColor = True
        '
        'NumericUpDown_ZoomFactor
        '
        Me.NumericUpDown_ZoomFactor.Location = New System.Drawing.Point(280, 221)
        Me.NumericUpDown_ZoomFactor.Name = "NumericUpDown_ZoomFactor"
        Me.NumericUpDown_ZoomFactor.Size = New System.Drawing.Size(47, 20)
        Me.NumericUpDown_ZoomFactor.TabIndex = 7
        '
        'ComboBox_SizeMode
        '
        Me.ComboBox_SizeMode.FormattingEnabled = True
        Me.ComboBox_SizeMode.Items.AddRange(New Object() {"normale Anzeige", "zentrierte Anzeige", "gezoomte Anzeige", "ausgefüllte Anzeige"})
        Me.ComboBox_SizeMode.Location = New System.Drawing.Point(280, 185)
        Me.ComboBox_SizeMode.Name = "ComboBox_SizeMode"
        Me.ComboBox_SizeMode.Size = New System.Drawing.Size(163, 21)
        Me.ComboBox_SizeMode.TabIndex = 8
        '
        'Label_FramesPerSecound
        '
        Me.Label_FramesPerSecound.AutoSize = True
        Me.Label_FramesPerSecound.Location = New System.Drawing.Point(333, 152)
        Me.Label_FramesPerSecound.Name = "Label_FramesPerSecound"
        Me.Label_FramesPerSecound.Size = New System.Drawing.Size(81, 13)
        Me.Label_FramesPerSecound.TabIndex = 9
        Me.Label_FramesPerSecound.Text = "Bilder/Sekunde"
        '
        'Label_Zoomfaktor
        '
        Me.Label_Zoomfaktor.AutoSize = True
        Me.Label_Zoomfaktor.Location = New System.Drawing.Point(333, 223)
        Me.Label_Zoomfaktor.Name = "Label_Zoomfaktor"
        Me.Label_Zoomfaktor.Size = New System.Drawing.Size(110, 13)
        Me.Label_Zoomfaktor.TabIndex = 10
        Me.Label_Zoomfaktor.Text = "% ausgefülltes Control"
        '
        'Button_StartAnimation
        '
        Me.Button_StartAnimation.Location = New System.Drawing.Point(242, 290)
        Me.Button_StartAnimation.Name = "Button_StartAnimation"
        Me.Button_StartAnimation.Size = New System.Drawing.Size(108, 24)
        Me.Button_StartAnimation.TabIndex = 11
        Me.Button_StartAnimation.Text = "Animation starten"
        Me.Button_StartAnimation.UseVisualStyleBackColor = True
        '
        'Button_StopAnimation
        '
        Me.Button_StopAnimation.Location = New System.Drawing.Point(356, 290)
        Me.Button_StopAnimation.Name = "Button_StopAnimation"
        Me.Button_StopAnimation.Size = New System.Drawing.Size(108, 24)
        Me.Button_StopAnimation.TabIndex = 12
        Me.Button_StopAnimation.Text = "Animation stoppen"
        Me.Button_StopAnimation.UseVisualStyleBackColor = True
        '
        'CheckBox_AdaptiveTiming
        '
        Me.CheckBox_AdaptiveTiming.AutoSize = True
        Me.CheckBox_AdaptiveTiming.Location = New System.Drawing.Point(280, 61)
        Me.CheckBox_AdaptiveTiming.Name = "CheckBox_AdaptiveTiming"
        Me.CheckBox_AdaptiveTiming.Size = New System.Drawing.Size(102, 17)
        Me.CheckBox_AdaptiveTiming.TabIndex = 13
        Me.CheckBox_AdaptiveTiming.Text = "Adaptive Timing"
        Me.CheckBox_AdaptiveTiming.UseVisualStyleBackColor = True
        '
        'FormAniGif
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(513, 324)
        Me.Controls.Add(Me.CheckBox_AdaptiveTiming)
        Me.Controls.Add(Me.Button_StopAnimation)
        Me.Controls.Add(Me.Button_StartAnimation)
        Me.Controls.Add(Me.Label_Zoomfaktor)
        Me.Controls.Add(Me.Label_FramesPerSecound)
        Me.Controls.Add(Me.ComboBox_SizeMode)
        Me.Controls.Add(Me.NumericUpDown_ZoomFactor)
        Me.Controls.Add(Me.Button_Forward)
        Me.Controls.Add(Me.Button_Back)
        Me.Controls.Add(Me.Label_Animation)
        Me.Controls.Add(Me.NumericUpDown_CustomDisplaySpeed)
        Me.Controls.Add(Me.CheckBox_CustomDisplaySpeed)
        Me.Controls.Add(Me.CheckBox_AutoPlay)
        Me.Controls.Add(Me.AniGif)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormAniGif"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "AniGif Demo"
        CType(Me.NumericUpDown_CustomDisplaySpeed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_ZoomFactor, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents AniGif As SchlumpfSoft.ControlsNET.AniGifControl.AniGif
    Private WithEvents CheckBox_AutoPlay As CheckBox
    Private WithEvents CheckBox_CustomDisplaySpeed As CheckBox
    Private WithEvents NumericUpDown_CustomDisplaySpeed As NumericUpDown
    Private WithEvents Label_Animation As Label
    Private WithEvents Button_Back As Button
    Private WithEvents Button_Forward As Button
    Private WithEvents NumericUpDown_ZoomFactor As NumericUpDown
    Private WithEvents ComboBox_SizeMode As ComboBox
    Private WithEvents Label_FramesPerSecound As Label
    Private WithEvents Label_Zoomfaktor As Label
    Private WithEvents Button_StartAnimation As Button
    Friend WithEvents Button_StopAnimation As Button
    Private WithEvents CheckBox_AdaptiveTiming As CheckBox
End Class
