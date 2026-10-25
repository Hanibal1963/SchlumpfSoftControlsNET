<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormSevenSegment
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
        Me.SingleDigit = New SchlumpfSoft.ControlsNET.SevenSegmentControl.SingleDigit()
        Me.MultiDigit = New SchlumpfSoft.ControlsNET.SevenSegmentControl.MultiDigit()
        Me.TextBox_SingleDigit = New System.Windows.Forms.TextBox()
        Me.TextBox_MultiDigit = New System.Windows.Forms.TextBox()
        Me.Label_SingleDigit = New System.Windows.Forms.Label()
        Me.Label_MultiDigit = New System.Windows.Forms.Label()
        Me.Button_InactiveColor = New System.Windows.Forms.Button()
        Me.Button_ForeColor = New System.Windows.Forms.Button()
        Me.ColorDialog = New System.Windows.Forms.ColorDialog()
        Me.SuspendLayout()
        '
        'SingleDigit
        '
        Me.SingleDigit.ColonActive = False
        Me.SingleDigit.DecimalPointActive = False
        Me.SingleDigit.DigitValue = Nothing
        Me.SingleDigit.ItalicFactor = -0.1!
        Me.SingleDigit.Location = New System.Drawing.Point(12, 12)
        Me.SingleDigit.Name = "SingleDigit"
        Me.SingleDigit.Padding = New System.Windows.Forms.Padding(10, 4, 10, 4)
        Me.SingleDigit.ShowColon = False
        Me.SingleDigit.ShowDecimalPoint = False
        Me.SingleDigit.Size = New System.Drawing.Size(37, 51)
        Me.SingleDigit.TabIndex = 0
        Me.SingleDigit.TabStop = False
        '
        'MultiDigit
        '
        Me.MultiDigit.DigitCount = 4
        Me.MultiDigit.DigitPadding = New System.Windows.Forms.Padding(10, 4, 10, 4)
        Me.MultiDigit.ItalicFactor = -0.1!
        Me.MultiDigit.Location = New System.Drawing.Point(12, 109)
        Me.MultiDigit.Name = "MultiDigit"
        Me.MultiDigit.ShowDecimalPoint = False
        Me.MultiDigit.Size = New System.Drawing.Size(157, 47)
        Me.MultiDigit.TabIndex = 1
        Me.MultiDigit.TabStop = False
        '
        'TextBox_SingleDigit
        '
        Me.TextBox_SingleDigit.Location = New System.Drawing.Point(286, 47)
        Me.TextBox_SingleDigit.MaxLength = 1
        Me.TextBox_SingleDigit.Name = "TextBox_SingleDigit"
        Me.TextBox_SingleDigit.Size = New System.Drawing.Size(29, 20)
        Me.TextBox_SingleDigit.TabIndex = 2
        '
        'TextBox_MultiDigit
        '
        Me.TextBox_MultiDigit.Location = New System.Drawing.Point(286, 140)
        Me.TextBox_MultiDigit.MaxLength = 4
        Me.TextBox_MultiDigit.Name = "TextBox_MultiDigit"
        Me.TextBox_MultiDigit.Size = New System.Drawing.Size(72, 20)
        Me.TextBox_MultiDigit.TabIndex = 3
        '
        'Label_SingleDigit
        '
        Me.Label_SingleDigit.AutoSize = True
        Me.Label_SingleDigit.Location = New System.Drawing.Point(175, 50)
        Me.Label_SingleDigit.Name = "Label_SingleDigit"
        Me.Label_SingleDigit.Size = New System.Drawing.Size(105, 13)
        Me.Label_SingleDigit.TabIndex = 4
        Me.Label_SingleDigit.Text = "anzuzeigender Wert:"
        '
        'Label_MultiDigit
        '
        Me.Label_MultiDigit.AutoSize = True
        Me.Label_MultiDigit.Location = New System.Drawing.Point(175, 143)
        Me.Label_MultiDigit.Name = "Label_MultiDigit"
        Me.Label_MultiDigit.Size = New System.Drawing.Size(105, 13)
        Me.Label_MultiDigit.TabIndex = 5
        Me.Label_MultiDigit.Text = "anzuzeigender Wert:"
        '
        'Button_InactiveColor
        '
        Me.Button_InactiveColor.Location = New System.Drawing.Point(376, 46)
        Me.Button_InactiveColor.Name = "Button_InactiveColor"
        Me.Button_InactiveColor.Size = New System.Drawing.Size(157, 21)
        Me.Button_InactiveColor.TabIndex = 6
        Me.Button_InactiveColor.Text = "Farbe für inaktive Segmente"
        Me.Button_InactiveColor.UseVisualStyleBackColor = True
        '
        'Button_ForeColor
        '
        Me.Button_ForeColor.Location = New System.Drawing.Point(376, 139)
        Me.Button_ForeColor.Name = "Button_ForeColor"
        Me.Button_ForeColor.Size = New System.Drawing.Size(157, 21)
        Me.Button_ForeColor.TabIndex = 7
        Me.Button_ForeColor.Text = "Farbe für aktive Segmente"
        Me.Button_ForeColor.UseVisualStyleBackColor = True
        '
        'FormSevenSegment
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(555, 192)
        Me.Controls.Add(Me.Button_ForeColor)
        Me.Controls.Add(Me.Button_InactiveColor)
        Me.Controls.Add(Me.Label_MultiDigit)
        Me.Controls.Add(Me.Label_SingleDigit)
        Me.Controls.Add(Me.TextBox_MultiDigit)
        Me.Controls.Add(Me.TextBox_SingleDigit)
        Me.Controls.Add(Me.MultiDigit)
        Me.Controls.Add(Me.SingleDigit)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormSevenSegment"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "FormSevenSegment"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents SingleDigit As SchlumpfSoft.ControlsNET.SevenSegmentControl.SingleDigit
    Private WithEvents MultiDigit As SchlumpfSoft.ControlsNET.SevenSegmentControl.MultiDigit
    Private WithEvents TextBox_SingleDigit As TextBox
    Private WithEvents TextBox_MultiDigit As TextBox
    Private WithEvents Label_SingleDigit As Label
    Private WithEvents Label_MultiDigit As Label
    Private WithEvents Button_InactiveColor As Button
    Private WithEvents Button_ForeColor As Button
    Private WithEvents ColorDialog As ColorDialog
End Class
