<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormShape
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
        Me.PictureBox = New System.Windows.Forms.PictureBox()
        Me.ComboBox_ShapeMode = New System.Windows.Forms.ComboBox()
        Me.Label_ShapeMode = New System.Windows.Forms.Label()
        Me.GroupBox_Options = New System.Windows.Forms.GroupBox()
        Me.Button_FillColor = New System.Windows.Forms.Button()
        Me.Button_LineColor = New System.Windows.Forms.Button()
        Me.Label_LineWidth = New System.Windows.Forms.Label()
        Me.NumericUpDown_LineWidth = New System.Windows.Forms.NumericUpDown()
        Me.Label_DiagonalLineMode = New System.Windows.Forms.Label()
        Me.ComboBox_DiagonalLineMode = New System.Windows.Forms.ComboBox()
        Me.Shape = New SchlumpfSoft.ControlsNET.ShapeControl.Shape()
        Me.ColorDialog = New System.Windows.Forms.ColorDialog()
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox_Options.SuspendLayout()
        CType(Me.NumericUpDown_LineWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox
        '
        Me.PictureBox.Image = Global.SchlumpfsoftControlsNETDemo.My.Resources.Resources.PapaSchlumpf1
        Me.PictureBox.Location = New System.Drawing.Point(55, 46)
        Me.PictureBox.Name = "PictureBox"
        Me.PictureBox.Size = New System.Drawing.Size(135, 135)
        Me.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox.TabIndex = 1
        Me.PictureBox.TabStop = False
        '
        'ComboBox_ShapeMode
        '
        Me.ComboBox_ShapeMode.FormattingEnabled = True
        Me.ComboBox_ShapeMode.Items.AddRange(New Object() {"Horizontale Linie", "Vertikale Linie", "Diagonale Linie", "Rechteck", "Gefülltes Rechteck", "Elipse", "Gefüllte Elipse"})
        Me.ComboBox_ShapeMode.Location = New System.Drawing.Point(75, 23)
        Me.ComboBox_ShapeMode.Name = "ComboBox_ShapeMode"
        Me.ComboBox_ShapeMode.Size = New System.Drawing.Size(161, 21)
        Me.ComboBox_ShapeMode.TabIndex = 2
        '
        'Label_ShapeMode
        '
        Me.Label_ShapeMode.AutoSize = True
        Me.Label_ShapeMode.Location = New System.Drawing.Point(16, 26)
        Me.Label_ShapeMode.Name = "Label_ShapeMode"
        Me.Label_ShapeMode.Size = New System.Drawing.Size(42, 13)
        Me.Label_ShapeMode.TabIndex = 3
        Me.Label_ShapeMode.Text = "Modus:"
        '
        'GroupBox_Options
        '
        Me.GroupBox_Options.Controls.Add(Me.Button_FillColor)
        Me.GroupBox_Options.Controls.Add(Me.Button_LineColor)
        Me.GroupBox_Options.Controls.Add(Me.Label_LineWidth)
        Me.GroupBox_Options.Controls.Add(Me.NumericUpDown_LineWidth)
        Me.GroupBox_Options.Controls.Add(Me.Label_DiagonalLineMode)
        Me.GroupBox_Options.Controls.Add(Me.ComboBox_DiagonalLineMode)
        Me.GroupBox_Options.Controls.Add(Me.ComboBox_ShapeMode)
        Me.GroupBox_Options.Controls.Add(Me.Label_ShapeMode)
        Me.GroupBox_Options.Location = New System.Drawing.Point(252, 23)
        Me.GroupBox_Options.Name = "GroupBox_Options"
        Me.GroupBox_Options.Size = New System.Drawing.Size(255, 175)
        Me.GroupBox_Options.TabIndex = 4
        Me.GroupBox_Options.TabStop = False
        Me.GroupBox_Options.Text = "Optionen"
        '
        'Button_FillColor
        '
        Me.Button_FillColor.Location = New System.Drawing.Point(19, 135)
        Me.Button_FillColor.Name = "Button_FillColor"
        Me.Button_FillColor.Size = New System.Drawing.Size(217, 26)
        Me.Button_FillColor.TabIndex = 9
        Me.Button_FillColor.Text = "Füllfarbe auswählen ..."
        Me.Button_FillColor.UseVisualStyleBackColor = True
        '
        'Button_LineColor
        '
        Me.Button_LineColor.Location = New System.Drawing.Point(19, 103)
        Me.Button_LineColor.Name = "Button_LineColor"
        Me.Button_LineColor.Size = New System.Drawing.Size(217, 26)
        Me.Button_LineColor.TabIndex = 8
        Me.Button_LineColor.Text = "Linienfarbe auswählen ..."
        Me.Button_LineColor.UseVisualStyleBackColor = True
        '
        'Label_LineWidth
        '
        Me.Label_LineWidth.AutoSize = True
        Me.Label_LineWidth.Location = New System.Drawing.Point(16, 79)
        Me.Label_LineWidth.Name = "Label_LineWidth"
        Me.Label_LineWidth.Size = New System.Drawing.Size(161, 13)
        Me.Label_LineWidth.TabIndex = 7
        Me.Label_LineWidth.Text = "Breite der Linien in Pixeln (1-10) :"
        '
        'NumericUpDown_LineWidth
        '
        Me.NumericUpDown_LineWidth.Location = New System.Drawing.Point(183, 77)
        Me.NumericUpDown_LineWidth.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.NumericUpDown_LineWidth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.NumericUpDown_LineWidth.Name = "NumericUpDown_LineWidth"
        Me.NumericUpDown_LineWidth.Size = New System.Drawing.Size(53, 20)
        Me.NumericUpDown_LineWidth.TabIndex = 6
        Me.NumericUpDown_LineWidth.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label_DiagonalLineMode
        '
        Me.Label_DiagonalLineMode.AutoSize = True
        Me.Label_DiagonalLineMode.Location = New System.Drawing.Point(16, 53)
        Me.Label_DiagonalLineMode.Name = "Label_DiagonalLineMode"
        Me.Label_DiagonalLineMode.Size = New System.Drawing.Size(53, 13)
        Me.Label_DiagonalLineMode.TabIndex = 5
        Me.Label_DiagonalLineMode.Text = "Linie von "
        '
        'ComboBox_DiagonalLineMode
        '
        Me.ComboBox_DiagonalLineMode.FormattingEnabled = True
        Me.ComboBox_DiagonalLineMode.Items.AddRange(New Object() {"links oben nach rechts unten", "links unten nach rechts oben"})
        Me.ComboBox_DiagonalLineMode.Location = New System.Drawing.Point(75, 50)
        Me.ComboBox_DiagonalLineMode.Name = "ComboBox_DiagonalLineMode"
        Me.ComboBox_DiagonalLineMode.Size = New System.Drawing.Size(161, 21)
        Me.ComboBox_DiagonalLineMode.TabIndex = 4
        '
        'Shape
        '
        Me.Shape.Location = New System.Drawing.Point(35, 23)
        Me.Shape.Name = "Shape"
        Me.Shape.Size = New System.Drawing.Size(187, 175)
        Me.Shape.TabIndex = 0
        '
        'ColorDialog
        '
        Me.ColorDialog.AllowFullOpen = False
        '
        'FormShape
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(538, 238)
        Me.Controls.Add(Me.GroupBox_Options)
        Me.Controls.Add(Me.Shape)
        Me.Controls.Add(Me.PictureBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormShape"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Shape Demo"
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox_Options.ResumeLayout(False)
        Me.GroupBox_Options.PerformLayout()
        CType(Me.NumericUpDown_LineWidth, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents Shape As SchlumpfSoft.ControlsNET.ShapeControl.Shape
    Private WithEvents PictureBox As PictureBox
    Private WithEvents ComboBox_ShapeMode As ComboBox
    Private WithEvents Label_ShapeMode As Label
    Private WithEvents GroupBox_Options As GroupBox
    Private WithEvents ComboBox_DiagonalLineMode As ComboBox
    Private WithEvents Label_DiagonalLineMode As Label
    Private WithEvents NumericUpDown_LineWidth As NumericUpDown
    Private WithEvents Label_LineWidth As Label
    Private WithEvents Button_LineColor As Button
    Private WithEvents Button_FillColor As Button
    Private WithEvents ColorDialog As ColorDialog
End Class
