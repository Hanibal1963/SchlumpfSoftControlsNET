Namespace IniFileControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class EntryValueEdit

        Inherits System.Windows.Forms.UserControl

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

        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.Button = New System.Windows.Forms.Button()
            Me.TextBox = New System.Windows.Forms.TextBox()
            Me.GroupBox = New System.Windows.Forms.GroupBox()
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
            Me.GroupBox.SuspendLayout()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.SuspendLayout()
            '
            'Button
            '
            Me.Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Button.Location = New System.Drawing.Point(71, 31)
            Me.Button.Name = "Button"
            Me.Button.Size = New System.Drawing.Size(98, 25)
            Me.Button.TabIndex = 0
            Me.Button.Text = "übernehmen"
            Me.Button.UseVisualStyleBackColor = True
            '
            'TextBox
            '
            Me.TextBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.TextBox.Location = New System.Drawing.Point(3, 3)
            Me.TextBox.Name = "TextBox"
            Me.TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.TextBox.Size = New System.Drawing.Size(166, 20)
            Me.TextBox.TabIndex = 1
            Me.TextBox.WordWrap = False
            '
            'GroupBox
            '
            Me.GroupBox.Controls.Add(Me.TableLayoutPanel1)
            Me.GroupBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.GroupBox.Location = New System.Drawing.Point(0, 0)
            Me.GroupBox.Name = "GroupBox"
            Me.GroupBox.Size = New System.Drawing.Size(178, 78)
            Me.GroupBox.TabIndex = 2
            Me.GroupBox.TabStop = False
            Me.GroupBox.Text = "EntryValueEdit"
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 1
            Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Controls.Add(Me.TextBox, 0, 0)
            Me.TableLayoutPanel1.Controls.Add(Me.Button, 0, 1)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 16)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 2
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(172, 59)
            Me.TableLayoutPanel1.TabIndex = 2
            '
            'EntryValueEdit
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.GroupBox)
            Me.Name = "EntryValueEdit"
            Me.Size = New System.Drawing.Size(178, 78)
            Me.GroupBox.ResumeLayout(False)
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Private ReadOnly components As System.ComponentModel.IContainer
        Private WithEvents Button As System.Windows.Forms.Button
        Private WithEvents TextBox As System.Windows.Forms.TextBox
        Private WithEvents GroupBox As System.Windows.Forms.GroupBox
        Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel

    End Class

End Namespace