Namespace IniFileControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ContentView

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
            Me.TextBox = New System.Windows.Forms.TextBox()
            Me.GroupBox.SuspendLayout()
            Me.SuspendLayout()
            '
            'GroupBox
            '
            Me.GroupBox.Controls.Add(Me.TextBox)
            Me.GroupBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.GroupBox.Location = New System.Drawing.Point(0, 0)
            Me.GroupBox.Name = "GroupBox"
            Me.GroupBox.Size = New System.Drawing.Size(202, 127)
            Me.GroupBox.TabIndex = 0
            Me.GroupBox.TabStop = False
            Me.GroupBox.Text = "ContentView"
            '
            'TextBox
            '
            Me.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.TextBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TextBox.Location = New System.Drawing.Point(3, 16)
            Me.TextBox.Multiline = True
            Me.TextBox.Name = "TextBox"
            Me.TextBox.ReadOnly = True
            Me.TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.TextBox.Size = New System.Drawing.Size(196, 108)
            Me.TextBox.TabIndex = 0
            Me.TextBox.WordWrap = False
            '
            'ContentView
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.GroupBox)
            Me.Name = "ContentView"
            Me.Size = New System.Drawing.Size(202, 127)
            Me.GroupBox.ResumeLayout(False)
            Me.GroupBox.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Private WithEvents GroupBox As System.Windows.Forms.GroupBox
        Private WithEvents TextBox As System.Windows.Forms.TextBox
        Private ReadOnly components As System.ComponentModel.IContainer

    End Class

End Namespace

