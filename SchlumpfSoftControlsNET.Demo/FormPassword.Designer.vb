<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormPassword
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
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.LabelInfo1 = New System.Windows.Forms.Label()
        Me.LabelMsg1 = New System.Windows.Forms.Label()
        Me.Label_2_PW = New System.Windows.Forms.Label()
        Me.Label_1_PW = New System.Windows.Forms.Label()
        Me.ButtonOk1 = New System.Windows.Forms.Button()
        Me.Password1 = New SchlumpfSoft.ControlsNET.PasswordControl.Password()
        Me.Password2 = New SchlumpfSoft.ControlsNET.PasswordControl.Password()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.ButtonOk2 = New System.Windows.Forms.Button()
        Me.LabelInfo2 = New System.Windows.Forms.Label()
        Me.LabelMsg2 = New System.Windows.Forms.Label()
        Me.Password3 = New SchlumpfSoft.ControlsNET.PasswordControl.Password()
        Me.Label_3_PW = New System.Windows.Forms.Label()
        Me.ButtonPwDelete = New System.Windows.Forms.Button()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.LabelInfo1)
        Me.Panel1.Controls.Add(Me.LabelMsg1)
        Me.Panel1.Controls.Add(Me.Label_2_PW)
        Me.Panel1.Controls.Add(Me.Label_1_PW)
        Me.Panel1.Controls.Add(Me.ButtonOk1)
        Me.Panel1.Controls.Add(Me.Password1)
        Me.Panel1.Controls.Add(Me.Password2)
        Me.Panel1.Location = New System.Drawing.Point(12, 12)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(455, 295)
        Me.Panel1.TabIndex = 2
        Me.Panel1.Visible = False
        '
        'LabelInfo1
        '
        Me.LabelInfo1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelInfo1.Image = My.Resources.Resources.Information
        Me.LabelInfo1.Location = New System.Drawing.Point(23, 24)
        Me.LabelInfo1.Name = "LabelInfo1"
        Me.LabelInfo1.Size = New System.Drawing.Size(403, 109)
        Me.LabelInfo1.TabIndex = 6
        Me.LabelInfo1.Text = "LabelInfo1"
        '
        'LabelMsg1
        '
        Me.LabelMsg1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelMsg1.Location = New System.Drawing.Point(23, 212)
        Me.LabelMsg1.Name = "LabelMsg1"
        Me.LabelMsg1.Size = New System.Drawing.Size(403, 18)
        Me.LabelMsg1.TabIndex = 5
        Me.LabelMsg1.Text = "LabelMsg1"
        Me.LabelMsg1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label_2_PW
        '
        Me.Label_2_PW.Location = New System.Drawing.Point(23, 182)
        Me.Label_2_PW.Name = "Label_2_PW"
        Me.Label_2_PW.Size = New System.Drawing.Size(137, 13)
        Me.Label_2_PW.TabIndex = 4
        Me.Label_2_PW.Text = "Label_2_PW"
        '
        'Label_1_PW
        '
        Me.Label_1_PW.Location = New System.Drawing.Point(23, 148)
        Me.Label_1_PW.Name = "Label_1_PW"
        Me.Label_1_PW.Size = New System.Drawing.Size(137, 13)
        Me.Label_1_PW.TabIndex = 3
        Me.Label_1_PW.Text = "Label_1_PW"
        '
        'ButtonOk1
        '
        Me.ButtonOk1.Location = New System.Drawing.Point(327, 245)
        Me.ButtonOk1.Name = "ButtonOk1"
        Me.ButtonOk1.Size = New System.Drawing.Size(99, 29)
        Me.ButtonOk1.TabIndex = 2
        Me.ButtonOk1.Text = "OK"
        Me.ButtonOk1.UseVisualStyleBackColor = True
        '
        'Password1
        '
        Me.Password1.Location = New System.Drawing.Point(163, 147)
        Me.Password1.Margin = New System.Windows.Forms.Padding(0)
        Me.Password1.MinimumSize = New System.Drawing.Size(100, 20)
        Me.Password1.Name = "Password1"
        Me.Password1.Size = New System.Drawing.Size(263, 20)
        Me.Password1.TabIndex = 0
        '
        'Password2
        '
        Me.Password2.Location = New System.Drawing.Point(163, 182)
        Me.Password2.Margin = New System.Windows.Forms.Padding(0)
        Me.Password2.MinimumSize = New System.Drawing.Size(100, 20)
        Me.Password2.Name = "Password2"
        Me.Password2.Size = New System.Drawing.Size(263, 20)
        Me.Password2.TabIndex = 1
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.ButtonOk2)
        Me.Panel2.Controls.Add(Me.LabelInfo2)
        Me.Panel2.Controls.Add(Me.LabelMsg2)
        Me.Panel2.Controls.Add(Me.Password3)
        Me.Panel2.Controls.Add(Me.Label_3_PW)
        Me.Panel2.Controls.Add(Me.ButtonPwDelete)
        Me.Panel2.Location = New System.Drawing.Point(486, 12)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(455, 295)
        Me.Panel2.TabIndex = 3
        Me.Panel2.Visible = False
        '
        'ButtonOk2
        '
        Me.ButtonOk2.Location = New System.Drawing.Point(335, 245)
        Me.ButtonOk2.Name = "ButtonOk2"
        Me.ButtonOk2.Size = New System.Drawing.Size(99, 29)
        Me.ButtonOk2.TabIndex = 9
        Me.ButtonOk2.Text = "OK"
        Me.ButtonOk2.UseVisualStyleBackColor = True
        '
        'LabelInfo2
        '
        Me.LabelInfo2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelInfo2.Image = My.Resources.Resources.Information
        Me.LabelInfo2.Location = New System.Drawing.Point(31, 24)
        Me.LabelInfo2.Name = "LabelInfo2"
        Me.LabelInfo2.Size = New System.Drawing.Size(403, 109)
        Me.LabelInfo2.TabIndex = 8
        Me.LabelInfo2.Text = "LabelInfo2"
        '
        'LabelMsg2
        '
        Me.LabelMsg2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelMsg2.Location = New System.Drawing.Point(31, 212)
        Me.LabelMsg2.Name = "LabelMsg2"
        Me.LabelMsg2.Size = New System.Drawing.Size(403, 18)
        Me.LabelMsg2.TabIndex = 7
        Me.LabelMsg2.Text = "LabelMsg2"
        Me.LabelMsg2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Password3
        '
        Me.Password3.Location = New System.Drawing.Point(175, 175)
        Me.Password3.Margin = New System.Windows.Forms.Padding(0)
        Me.Password3.MinimumSize = New System.Drawing.Size(100, 20)
        Me.Password3.Name = "Password3"
        Me.Password3.Size = New System.Drawing.Size(263, 20)
        Me.Password3.TabIndex = 6
        '
        'Label_3_PW
        '
        Me.Label_3_PW.Location = New System.Drawing.Point(28, 182)
        Me.Label_3_PW.Name = "Label_3_PW"
        Me.Label_3_PW.Size = New System.Drawing.Size(137, 13)
        Me.Label_3_PW.TabIndex = 5
        Me.Label_3_PW.Text = "Label_3_PW"
        '
        'ButtonPwDelete
        '
        Me.ButtonPwDelete.Location = New System.Drawing.Point(218, 245)
        Me.ButtonPwDelete.Name = "ButtonPwDelete"
        Me.ButtonPwDelete.Size = New System.Drawing.Size(99, 29)
        Me.ButtonPwDelete.TabIndex = 3
        Me.ButtonPwDelete.Text = "Passwort löschen"
        Me.ButtonPwDelete.UseVisualStyleBackColor = True
        '
        'FormPassword
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(953, 537)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormPassword"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Password Demo"
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents Password1 As SchlumpfSoft.ControlsNET.PasswordControl.Password
    Private WithEvents Password2 As SchlumpfSoft.ControlsNET.PasswordControl.Password
    Private WithEvents Panel1 As Panel
    Private WithEvents ButtonOk1 As Button
    Private WithEvents Label_1_PW As Label
    Private WithEvents Label_2_PW As Label
    Private WithEvents LabelMsg1 As Label
    Private WithEvents LabelInfo1 As Label
    Private WithEvents Panel2 As Panel
    Private WithEvents ButtonPwDelete As Button
    Private WithEvents Password3 As SchlumpfSoft.ControlsNET.PasswordControl.Password
    Private WithEvents Label_3_PW As Label
    Private WithEvents LabelMsg2 As Label
    Private WithEvents LabelInfo2 As Label
    Private WithEvents ButtonOk2 As Button
End Class
