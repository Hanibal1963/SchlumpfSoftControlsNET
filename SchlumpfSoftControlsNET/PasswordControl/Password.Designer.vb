Namespace PasswordControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class Password
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
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

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer


        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.TB = New System.Windows.Forms.TextBox()
            Me.PB = New System.Windows.Forms.PictureBox()
            CType(Me.PB, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'TB
            '
            Me.TB.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.TB.Location = New System.Drawing.Point(0, 0)
            Me.TB.Margin = New System.Windows.Forms.Padding(0)
            Me.TB.Name = "TB"
            Me.TB.Size = New System.Drawing.Size(110, 20)
            Me.TB.TabIndex = 0
            '
            'PB
            '
            Me.PB.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.PB.Location = New System.Drawing.Point(113, 0)
            Me.PB.Name = "PB"
            Me.PB.Size = New System.Drawing.Size(20, 20)
            Me.PB.TabIndex = 1
            Me.PB.TabStop = False
            '
            'Password
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.PB)
            Me.Controls.Add(Me.TB)
            Me.Margin = New System.Windows.Forms.Padding(0)
            Me.MinimumSize = New System.Drawing.Size(0, 20)
            Me.Name = "Password"
            Me.Size = New System.Drawing.Size(133, 31)
            CType(Me.PB, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private WithEvents TB As System.Windows.Forms.TextBox
        Private WithEvents PB As System.Windows.Forms.PictureBox
    End Class

End Namespace
