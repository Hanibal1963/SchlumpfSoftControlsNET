Namespace IniFileControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class AddItemDialog

        Inherits System.Windows.Forms.Form

        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    ' Gibt den vom Designer verwalteten Komponentencontainer frei.
                    components.Dispose()
                End If
            Finally
                ' Stellt sicher, dass die Basisklasse immer ihre Aufräumlogik ausführt.
                MyBase.Dispose(disposing)
            End Try
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Dim TableLayoutPanel As System.Windows.Forms.TableLayoutPanel
            Me.ButtonOK = New System.Windows.Forms.Button()
            Me.ButtonCancel = New System.Windows.Forms.Button()
            Me.Label = New System.Windows.Forms.Label()
            Me.TextBox = New System.Windows.Forms.TextBox()
            TableLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
            TableLayoutPanel.SuspendLayout()
            Me.SuspendLayout()
            '
            'TableLayoutPanel
            '
            TableLayoutPanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            TableLayoutPanel.ColumnCount = 2
            TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            TableLayoutPanel.Controls.Add(Me.ButtonOK, 0, 0)
            TableLayoutPanel.Controls.Add(Me.ButtonCancel, 1, 0)
            TableLayoutPanel.Location = New System.Drawing.Point(182, 55)
            TableLayoutPanel.Name = "TableLayoutPanel"
            TableLayoutPanel.RowCount = 1
            TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            TableLayoutPanel.Size = New System.Drawing.Size(146, 29)
            TableLayoutPanel.TabIndex = 0
            '
            'ButtonOK
            '
            Me.ButtonOK.Anchor = System.Windows.Forms.AnchorStyles.None
            Me.ButtonOK.Location = New System.Drawing.Point(3, 3)
            Me.ButtonOK.Name = "ButtonOK"
            Me.ButtonOK.Size = New System.Drawing.Size(67, 23)
            Me.ButtonOK.TabIndex = 0
            Me.ButtonOK.Text = "OK"
            AddHandler Me.ButtonOK.Click, AddressOf Me.Button_Click
            '
            'ButtonCancel
            '
            Me.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.None
            Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.ButtonCancel.Location = New System.Drawing.Point(76, 3)
            Me.ButtonCancel.Name = "ButtonCancel"
            Me.ButtonCancel.Size = New System.Drawing.Size(67, 23)
            Me.ButtonCancel.TabIndex = 1
            Me.ButtonCancel.Text = "Abbrechen"
            AddHandler Me.ButtonCancel.Click, AddressOf Me.Button_Click
            '
            'Label
            '
            Me.Label.AutoSize = True
            Me.Label.Location = New System.Drawing.Point(15, 9)
            Me.Label.Name = "Label"
            Me.Label.Size = New System.Drawing.Size(232, 13)
            Me.Label.TabIndex = 1
            Me.Label.Text = "Geben Sie den Name für das neue Element ein:"
            '
            'TextBox
            '
            Me.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.TextBox.Location = New System.Drawing.Point(12, 25)
            Me.TextBox.Name = "TextBox"
            Me.TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.TextBox.Size = New System.Drawing.Size(316, 20)
            Me.TextBox.TabIndex = 2
            Me.TextBox.WordWrap = False
            AddHandler Me.TextBox.TextChanged, AddressOf Me.TextBox_TextChanged
            '
            'AddItemDialog
            '
            Me.AcceptButton = Me.ButtonOK
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.ButtonCancel
            Me.ClientSize = New System.Drawing.Size(333, 96)
            Me.ControlBox = False
            Me.Controls.Add(Me.TextBox)
            Me.Controls.Add(Me.Label)
            Me.Controls.Add(TableLayoutPanel)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "AddItemDialog"
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "Element hinzufügen"
            TableLayoutPanel.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents Label As System.Windows.Forms.Label
        Private WithEvents TextBox As System.Windows.Forms.TextBox
        Private WithEvents ButtonOK As System.Windows.Forms.Button
        Private WithEvents ButtonCancel As System.Windows.Forms.Button
        Private ReadOnly components As System.ComponentModel.IContainer

    End Class

End Namespace

