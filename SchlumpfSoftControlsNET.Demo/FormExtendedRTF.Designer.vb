<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormExtendedRTF
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

        Me.ExtendedRTF = New SchlumpfSoft.ControlsNET.ExtendedRTFControl.ExtendedRTF()
        Me.TableLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.ToolStrip_Format = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton_Bold = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_Italic = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_UnderLine = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_Strikeout = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_FontFormat = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_FontSizeUp = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_FontSizeDown = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_ForeColor = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_BackColor = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_DelFormat = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_ToggleBullets = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_LeftIndentUp = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_LeftIndentDown = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_TextLeft = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_TextCenter = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton_TextRight = New System.Windows.Forms.ToolStripButton()
        Me.TableLayoutPanel.SuspendLayout()
        Me.ToolStrip_Format.SuspendLayout()
        Me.SuspendLayout()
        '
        'ExtendedRTF
        '
        Me.ExtendedRTF.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ExtendedRTF.Location = New System.Drawing.Point(3, 28)
        Me.ExtendedRTF.Name = "ExtendedRTF"
        Me.ExtendedRTF.SelectionBold = False
        Me.ExtendedRTF.SelectionFontSize = 8.25!
        Me.ExtendedRTF.SelectionForeColor = System.Drawing.Color.Black
        Me.ExtendedRTF.SelectionItalic = False
        Me.ExtendedRTF.SelectionLeftIndent = 0
        Me.ExtendedRTF.SelectionStrikeout = False
        Me.ExtendedRTF.SelectionUnderline = False
        Me.ExtendedRTF.Size = New System.Drawing.Size(802, 531)
        Me.ExtendedRTF.TabIndex = 0
        Me.ExtendedRTF.Text = ""
        '
        'TableLayoutPanel
        '
        Me.TableLayoutPanel.ColumnCount = 1
        Me.TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel.Controls.Add(Me.ToolStrip_Format, 0, 0)
        Me.TableLayoutPanel.Controls.Add(Me.ExtendedRTF, 0, 2)
        Me.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel.Name = "TableLayoutPanel"
        Me.TableLayoutPanel.RowCount = 3
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel.Size = New System.Drawing.Size(808, 562)
        Me.TableLayoutPanel.TabIndex = 1
        '
        'ToolStrip_Format
        '
        Me.ToolStrip_Format.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton_Bold, Me.ToolStripButton_Italic, Me.ToolStripButton_UnderLine, Me.ToolStripButton_Strikeout, Me.ToolStripButton_FontFormat, Me.ToolStripButton_FontSizeUp, Me.ToolStripButton_FontSizeDown, Me.ToolStripButton_ForeColor, Me.ToolStripButton_BackColor, Me.ToolStripButton_DelFormat, Me.ToolStripButton_ToggleBullets, Me.ToolStripButton_LeftIndentUp, Me.ToolStripButton_LeftIndentDown, Me.ToolStripButton_TextLeft, Me.ToolStripButton_TextCenter, Me.ToolStripButton_TextRight})
        Me.ToolStrip_Format.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip_Format.Name = "ToolStrip_Format"
        Me.ToolStrip_Format.Size = New System.Drawing.Size(808, 25)
        Me.ToolStrip_Format.TabIndex = 1
        Me.ToolStrip_Format.Text = "ToolStrip_Format"
        '
        'ToolStripButton_Bold
        '
        Me.ToolStripButton_Bold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_Bold.Image = My.Resources.Resources.ExtendedRTF_Bold
        Me.ToolStripButton_Bold.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_Bold.Name = "ToolStripButton_Bold"
        Me.ToolStripButton_Bold.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_Bold.Text = "ToolStripButton_Bold"
        Me.ToolStripButton_Bold.ToolTipText = "Markierung fett formatieren"
        '
        'ToolStripButton_Italic
        '
        Me.ToolStripButton_Italic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_Italic.Image = My.Resources.Resources.ExtendedRTF_Italic
        Me.ToolStripButton_Italic.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_Italic.Name = "ToolStripButton_Italic"
        Me.ToolStripButton_Italic.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_Italic.Text = "ToolStripButton_Italic"
        Me.ToolStripButton_Italic.ToolTipText = "Markierung kursiv formatieren"
        '
        'ToolStripButton_UnderLine
        '
        Me.ToolStripButton_UnderLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_UnderLine.Image = My.Resources.Resources.ExtendedRTF_Underline
        Me.ToolStripButton_UnderLine.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_UnderLine.Name = "ToolStripButton_UnderLine"
        Me.ToolStripButton_UnderLine.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_UnderLine.Text = "ToolStripButton_UnderLine"
        Me.ToolStripButton_UnderLine.ToolTipText = "Markierung unterstrichen formatieren"
        '
        'ToolStripButton_Strikeout
        '
        Me.ToolStripButton_Strikeout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_Strikeout.Image = My.Resources.Resources.ExtendedRTF_Strikeout
        Me.ToolStripButton_Strikeout.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_Strikeout.Name = "ToolStripButton_Strikeout"
        Me.ToolStripButton_Strikeout.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_Strikeout.Text = "ToolStripButton_Strikeout"
        Me.ToolStripButton_Strikeout.ToolTipText = "Markierung durchgestrichen formatieren"
        '
        'ToolStripButton_FontFormat
        '
        Me.ToolStripButton_FontFormat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_FontFormat.Image = My.Resources.Resources.ExtendedRTF_Fontformat
        Me.ToolStripButton_FontFormat.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_FontFormat.Name = "ToolStripButton_FontFormat"
        Me.ToolStripButton_FontFormat.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_FontFormat.Text = "ToolStripButton_FontFormat"
        Me.ToolStripButton_FontFormat.ToolTipText = "Schriftart für Markierung auswählen"
        '
        'ToolStripButton_FontSizeUp
        '
        Me.ToolStripButton_FontSizeUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_FontSizeUp.Image = My.Resources.Resources.ExtendedRTF_Fontup
        Me.ToolStripButton_FontSizeUp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_FontSizeUp.Name = "ToolStripButton_FontSizeUp"
        Me.ToolStripButton_FontSizeUp.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_FontSizeUp.Text = "ToolStripButton_FontSizeUp"
        Me.ToolStripButton_FontSizeUp.ToolTipText = "Schrift der Markierung vergrössern"
        '
        'ToolStripButton_FontSizeDown
        '
        Me.ToolStripButton_FontSizeDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_FontSizeDown.Image = My.Resources.Resources.ExtendedRTF_Fontdown
        Me.ToolStripButton_FontSizeDown.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_FontSizeDown.Name = "ToolStripButton_FontSizeDown"
        Me.ToolStripButton_FontSizeDown.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_FontSizeDown.Text = "ToolStripButton_FontSizeDown"
        Me.ToolStripButton_FontSizeDown.ToolTipText = "Scrift der Markierung verkleinern"
        '
        'ToolStripButton_ForeColor
        '
        Me.ToolStripButton_ForeColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_ForeColor.Image = My.Resources.Resources.ExtendedRTF_Forecolor
        Me.ToolStripButton_ForeColor.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_ForeColor.Name = "ToolStripButton_ForeColor"
        Me.ToolStripButton_ForeColor.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_ForeColor.Text = "ToolStripButton_ForeColor"
        Me.ToolStripButton_ForeColor.ToolTipText = "Schriftfarbe der Markierung auswählen"
        '
        'ToolStripButton_BackColor
        '
        Me.ToolStripButton_BackColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_BackColor.Image = My.Resources.Resources.ExtendedRTF_Backcolor
        Me.ToolStripButton_BackColor.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_BackColor.Name = "ToolStripButton_BackColor"
        Me.ToolStripButton_BackColor.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_BackColor.Text = "ToolStripButton_BackColor"
        Me.ToolStripButton_BackColor.ToolTipText = "Hintergrundfarbe der Markierung auswählen"
        '
        'ToolStripButton_DelFormat
        '
        Me.ToolStripButton_DelFormat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_DelFormat.Image = My.Resources.Resources.ExtendedRTF_Delformat
        Me.ToolStripButton_DelFormat.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_DelFormat.Name = "ToolStripButton_DelFormat"
        Me.ToolStripButton_DelFormat.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_DelFormat.Text = "ToolStripButton_DelFormat"
        Me.ToolStripButton_DelFormat.ToolTipText = "Formatierung der Markierung entfernen"
        '
        'ToolStripButton_ToggleBullets
        '
        Me.ToolStripButton_ToggleBullets.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_ToggleBullets.Image = My.Resources.Resources.ExtendedRTF_Togglebullets
        Me.ToolStripButton_ToggleBullets.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_ToggleBullets.Name = "ToolStripButton_ToggleBullets"
        Me.ToolStripButton_ToggleBullets.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_ToggleBullets.Text = "ToolStripButton_ToggleBullets"
        Me.ToolStripButton_ToggleBullets.ToolTipText = "Bullets der Markierung umschalten"
        '
        'ToolStripButton_LeftIndentUp
        '
        Me.ToolStripButton_LeftIndentUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_LeftIndentUp.Image = My.Resources.Resources.ExtendedRTF_LeftIndentUp
        Me.ToolStripButton_LeftIndentUp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_LeftIndentUp.Name = "ToolStripButton_LeftIndentUp"
        Me.ToolStripButton_LeftIndentUp.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_LeftIndentUp.Text = "ToolStripButton_LeftIndentUp"
        Me.ToolStripButton_LeftIndentUp.ToolTipText = "Einzug der Markierung vergrössern"
        '
        'ToolStripButton_LeftIndentDown
        '
        Me.ToolStripButton_LeftIndentDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_LeftIndentDown.Image = My.Resources.Resources.ExtendedRTF_LeftIndentDown
        Me.ToolStripButton_LeftIndentDown.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_LeftIndentDown.Name = "ToolStripButton_LeftIndentDown"
        Me.ToolStripButton_LeftIndentDown.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_LeftIndentDown.Text = "ToolStripButton_LeftIndentDown"
        Me.ToolStripButton_LeftIndentDown.ToolTipText = "Einzug der Markierung verkleinern"
        '
        'ToolStripButton_TextLeft
        '
        Me.ToolStripButton_TextLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_TextLeft.Image = My.Resources.Resources.ExtendedRTF_Textleft
        Me.ToolStripButton_TextLeft.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_TextLeft.Name = "ToolStripButton_TextLeft"
        Me.ToolStripButton_TextLeft.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_TextLeft.Text = "ToolStripButton_TextLeft"
        Me.ToolStripButton_TextLeft.ToolTipText = "Markierung Linksbündig formatieren"
        '
        'ToolStripButton_TextCenter
        '
        Me.ToolStripButton_TextCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_TextCenter.Image = My.Resources.Resources.ExtendedRTF_Textcenter
        Me.ToolStripButton_TextCenter.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_TextCenter.Name = "ToolStripButton_TextCenter"
        Me.ToolStripButton_TextCenter.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_TextCenter.Text = "ToolStripButton_TextCenter"
        Me.ToolStripButton_TextCenter.ToolTipText = "Markierung zentriert formatieren"
        '
        'ToolStripButton_TextRight
        '
        Me.ToolStripButton_TextRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_TextRight.Image = My.Resources.Resources.ExtendedRTF_Textright
        Me.ToolStripButton_TextRight.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_TextRight.Name = "ToolStripButton_TextRight"
        Me.ToolStripButton_TextRight.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_TextRight.Text = "ToolStripButton_TextRight"
        Me.ToolStripButton_TextRight.ToolTipText = "Markierung Rechtsbündig formatieren"
        '
        'FormExtendedRTF
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(808, 562)
        Me.Controls.Add(Me.TableLayoutPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormExtendedRTF"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "ExtendedRTF Demo"
        Me.TableLayoutPanel.ResumeLayout(False)
        Me.TableLayoutPanel.PerformLayout()
        Me.ToolStrip_Format.ResumeLayout(False)
        Me.ToolStrip_Format.PerformLayout()
        Me.ResumeLayout(False)
    End Sub
    Private WithEvents ExtendedRTF As SchlumpfSoft.ControlsNET.ExtendedRTFControl.ExtendedRTF
    Private WithEvents TableLayoutPanel As TableLayoutPanel
    Private WithEvents ToolStrip_Format As ToolStrip
    Private WithEvents ToolStripButton_Bold As ToolStripButton
    Private WithEvents ToolStripButton_Italic As ToolStripButton
    Private WithEvents ToolStripButton_UnderLine As ToolStripButton
    Private WithEvents ToolStripButton_Strikeout As ToolStripButton
    Private WithEvents ToolStripButton_ToggleBullets As ToolStripButton
    Private WithEvents ToolStripButton_FontSizeUp As ToolStripButton
    Private WithEvents ToolStripButton_FontSizeDown As ToolStripButton
    Private WithEvents ToolStripButton_ForeColor As ToolStripButton
    Private WithEvents ToolStripButton_BackColor As ToolStripButton
    Private WithEvents ToolStripButton_DelFormat As ToolStripButton
    Private WithEvents ToolStripButton_LeftIndentUp As ToolStripButton
    Private WithEvents ToolStripButton_LeftIndentDown As ToolStripButton
    Private WithEvents ToolStripButton_TextLeft As ToolStripButton
    Private WithEvents ToolStripButton_TextCenter As ToolStripButton
    Private WithEvents ToolStripButton_TextRight As ToolStripButton
    Private WithEvents ToolStripButton_FontFormat As ToolStripButton
End Class
