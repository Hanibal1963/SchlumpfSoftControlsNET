<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormColorProgressBar
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
        Me.NumericUpDown_ProgressValue = New System.Windows.Forms.NumericUpDown()
        Me.CheckBox_ShowGliss = New System.Windows.Forms.CheckBox()
        Me.CheckBox_ShowBorder = New System.Windows.Forms.CheckBox()
        Me.Button_BarColorChange = New System.Windows.Forms.Button()
        Me.Button_BorderColorChange = New System.Windows.Forms.Button()
        Me.Button_EmptyColorChange = New System.Windows.Forms.Button()
        Me.LabelProgressValue = New System.Windows.Forms.Label()
        Me.ColorDialog = New System.Windows.Forms.ColorDialog()
        Me.ColorProgressBar = New SchlumpfSoft.ControlsNET.ColorProgressBarControl.ColorProgressBar()
        CType(Me.NumericUpDown_ProgressValue, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'NumericUpDown_ProgressValue
        '
        Me.NumericUpDown_ProgressValue.Location = New System.Drawing.Point(117, 74)
        Me.NumericUpDown_ProgressValue.Name = "NumericUpDown_ProgressValue"
        Me.NumericUpDown_ProgressValue.Size = New System.Drawing.Size(46, 20)
        Me.NumericUpDown_ProgressValue.TabIndex = 1
        '
        'CheckBox_ShowGliss
        '
        Me.CheckBox_ShowGliss.AutoSize = True
        Me.CheckBox_ShowGliss.Location = New System.Drawing.Point(23, 107)
        Me.CheckBox_ShowGliss.Name = "CheckBox_ShowGliss"
        Me.CheckBox_ShowGliss.Size = New System.Drawing.Size(99, 17)
        Me.CheckBox_ShowGliss.TabIndex = 2
        Me.CheckBox_ShowGliss.Text = "Glanz anzeigen"
        Me.CheckBox_ShowGliss.UseVisualStyleBackColor = True
        '
        'CheckBox_ShowBorder
        '
        Me.CheckBox_ShowBorder.AutoSize = True
        Me.CheckBox_ShowBorder.Location = New System.Drawing.Point(23, 136)
        Me.CheckBox_ShowBorder.Name = "CheckBox_ShowBorder"
        Me.CheckBox_ShowBorder.Size = New System.Drawing.Size(112, 17)
        Me.CheckBox_ShowBorder.TabIndex = 3
        Me.CheckBox_ShowBorder.Text = "Rahmen anzeigen"
        Me.CheckBox_ShowBorder.UseVisualStyleBackColor = True
        '
        'Button_BarColorChange
        '
        Me.Button_BarColorChange.Location = New System.Drawing.Point(333, 71)
        Me.Button_BarColorChange.Name = "Button_BarColorChange"
        Me.Button_BarColorChange.Size = New System.Drawing.Size(145, 23)
        Me.Button_BarColorChange.TabIndex = 4
        Me.Button_BarColorChange.Text = "Balkenfarbe ..."
        Me.Button_BarColorChange.UseVisualStyleBackColor = True
        '
        'Button_BorderColorChange
        '
        Me.Button_BorderColorChange.Location = New System.Drawing.Point(333, 103)
        Me.Button_BorderColorChange.Name = "Button_BorderColorChange"
        Me.Button_BorderColorChange.Size = New System.Drawing.Size(145, 23)
        Me.Button_BorderColorChange.TabIndex = 5
        Me.Button_BorderColorChange.Text = "Rahmenfarbe ..."
        Me.Button_BorderColorChange.UseVisualStyleBackColor = True
        '
        'Button_EmptyColorChange
        '
        Me.Button_EmptyColorChange.Location = New System.Drawing.Point(333, 132)
        Me.Button_EmptyColorChange.Name = "Button_EmptyColorChange"
        Me.Button_EmptyColorChange.Size = New System.Drawing.Size(145, 23)
        Me.Button_EmptyColorChange.TabIndex = 6
        Me.Button_EmptyColorChange.Text = "Balkenhintergrundfarbe ..."
        Me.Button_EmptyColorChange.UseVisualStyleBackColor = True
        '
        'LabelProgressValue
        '
        Me.LabelProgressValue.AutoSize = True
        Me.LabelProgressValue.Location = New System.Drawing.Point(20, 76)
        Me.LabelProgressValue.Name = "LabelProgressValue"
        Me.LabelProgressValue.Size = New System.Drawing.Size(91, 13)
        Me.LabelProgressValue.TabIndex = 7
        Me.LabelProgressValue.Text = "Wert des Balkens"
        '
        'ColorProgressBar
        '
        Me.ColorProgressBar.BackColor = System.Drawing.Color.Black
        Me.ColorProgressBar.Location = New System.Drawing.Point(23, 35)
        Me.ColorProgressBar.Name = "ColorProgressBar"
        Me.ColorProgressBar.Padding = New System.Windows.Forms.Padding(1)
        Me.ColorProgressBar.Size = New System.Drawing.Size(455, 23)
        Me.ColorProgressBar.TabIndex = 0
        '
        'FormColorProgressBar
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(501, 176)
        Me.Controls.Add(Me.LabelProgressValue)
        Me.Controls.Add(Me.Button_EmptyColorChange)
        Me.Controls.Add(Me.Button_BorderColorChange)
        Me.Controls.Add(Me.Button_BarColorChange)
        Me.Controls.Add(Me.CheckBox_ShowBorder)
        Me.Controls.Add(Me.CheckBox_ShowGliss)
        Me.Controls.Add(Me.NumericUpDown_ProgressValue)
        Me.Controls.Add(Me.ColorProgressBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormColorProgressBar"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "ColorProgressBar Demo"
        CType(Me.NumericUpDown_ProgressValue, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents ColorProgressBar As SchlumpfSoft.ControlsNET.ColorProgressBarControl.ColorProgressBar
    Private WithEvents NumericUpDown_ProgressValue As NumericUpDown
    Private WithEvents CheckBox_ShowGliss As CheckBox
    Private WithEvents CheckBox_ShowBorder As CheckBox
    Private WithEvents Button_BarColorChange As Button
    Private WithEvents Button_BorderColorChange As Button
    Private WithEvents Button_EmptyColorChange As Button
    Private WithEvents LabelProgressValue As Label
    Private WithEvents ColorDialog As ColorDialog
End Class
