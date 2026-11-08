Namespace ColorProgressBarControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ColorProgressBar

        Inherits System.Windows.Forms.UserControl

        ' UserControl überschreibt Dispose, um die Komponentenliste zu bereinigen.
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

        ' Erforderlich für den Windows Form-Designer
        Private components As System.ComponentModel.IContainer
        Private WithEvents ProgressFull As System.Windows.Forms.Panel
        Private WithEvents ProgressEmpty As System.Windows.Forms.Panel
        Private WithEvents GlossLeft As System.Windows.Forms.Panel
        Private WithEvents GlossRight As System.Windows.Forms.Panel

        ' HINWEIS: Die folgende Prozedur wird vom Windows Form-Designer benötigt
        ' Sie kann mit dem Windows Form-Designer geändert werden.  
        ' Nicht mit dem Code-Editor ändern.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.ProgressFull = New System.Windows.Forms.Panel()
            Me.GlossLeft = New System.Windows.Forms.Panel()
            Me.ProgressEmpty = New System.Windows.Forms.Panel()
            Me.GlossRight = New System.Windows.Forms.Panel()
            Me.ProgressFull.SuspendLayout()
            Me.ProgressEmpty.SuspendLayout()
            Me.SuspendLayout()
            '
            ' ProgressFull
            '
            Me.ProgressFull.BackColor = System.Drawing.SystemColors.HotTrack
            Me.ProgressFull.Controls.Add(Me.GlossLeft)
            Me.ProgressFull.Dock = System.Windows.Forms.DockStyle.Left
            Me.ProgressFull.Location = New System.Drawing.Point(1, 1)
            Me.ProgressFull.Name = "ProgressFull"
            Me.ProgressFull.Size = New System.Drawing.Size(20, 21)
            Me.ProgressFull.TabIndex = 0
            '
            ' GlossLeft
            '
            Me.GlossLeft.Dock = System.Windows.Forms.DockStyle.Top
            Me.GlossLeft.Location = New System.Drawing.Point(0, 0)
            Me.GlossLeft.Name = "GlossLeft"
            Me.GlossLeft.Size = New System.Drawing.Size(20, 10)
            Me.GlossLeft.TabIndex = 0
            '
            ' ProgressEmpty
            '
            Me.ProgressEmpty.Controls.Add(Me.GlossRight)
            Me.ProgressEmpty.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ProgressEmpty.Location = New System.Drawing.Point(21, 1)
            Me.ProgressEmpty.Name = "ProgressEmpty"
            Me.ProgressEmpty.Size = New System.Drawing.Size(198, 21)
            Me.ProgressEmpty.TabIndex = 1
            '
            ' GlossRight
            '
            Me.GlossRight.Dock = System.Windows.Forms.DockStyle.Top
            Me.GlossRight.Location = New System.Drawing.Point(0, 0)
            Me.GlossRight.Name = "GlossRight"
            Me.GlossRight.Size = New System.Drawing.Size(198, 10)
            Me.GlossRight.TabIndex = 0
            '
            ' ColorProgressBar
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.ProgressEmpty)
            Me.Controls.Add(Me.ProgressFull)
            Me.Name = "ColorProgressBar"
            Me.Padding = New System.Windows.Forms.Padding(1)
            Me.Size = New System.Drawing.Size(220, 23)
            Me.ProgressFull.ResumeLayout(False)
            Me.ProgressEmpty.ResumeLayout(False)
            Me.ResumeLayout(False)
        End Sub

    End Class

End Namespace

