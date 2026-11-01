Namespace IniFileControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class CommentEdit

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
            Me.GroupBox = New System.Windows.Forms.GroupBox()
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
            Me.TextBox = New System.Windows.Forms.TextBox()
            Me.Button = New System.Windows.Forms.Button()
            Me.GroupBox.SuspendLayout()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.SuspendLayout()
            '
            'GroupBox
            '
            Me.GroupBox.Controls.Add(Me.TableLayoutPanel1)
            Me.GroupBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.GroupBox.Location = New System.Drawing.Point(0, 0)
            Me.GroupBox.Name = "GroupBox"
            Me.GroupBox.Size = New System.Drawing.Size(161, 117)
            Me.GroupBox.TabIndex = 0
            Me.GroupBox.TabStop = False
            Me.GroupBox.Text = "CommentEdit"
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
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(155, 98)
            Me.TableLayoutPanel1.TabIndex = 2
            '
            'TextBox
            '
            Me.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.TextBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TextBox.Location = New System.Drawing.Point(3, 3)
            Me.TextBox.Multiline = True
            Me.TextBox.Name = "TextBox"
            Me.TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.TextBox.Size = New System.Drawing.Size(149, 61)
            Me.TextBox.TabIndex = 0
            Me.TextBox.WordWrap = False
            '
            'Button
            '
            Me.Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.Button.Location = New System.Drawing.Point(48, 70)
            Me.Button.Name = "Button"
            Me.Button.Size = New System.Drawing.Size(104, 25)
            Me.Button.TabIndex = 1
            Me.Button.Text = "übernehmen"
            Me.Button.UseVisualStyleBackColor = True
            '
            'CommentEdit
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.GroupBox)
            Me.Name = "CommentEdit"
            Me.Size = New System.Drawing.Size(161, 117)
            Me.GroupBox.ResumeLayout(False)
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Private ReadOnly components As System.ComponentModel.IContainer
        Private WithEvents GroupBox As System.Windows.Forms.GroupBox
        Private WithEvents Button As System.Windows.Forms.Button
        Private WithEvents TextBox As System.Windows.Forms.TextBox
        Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel

    End Class

End Namespace