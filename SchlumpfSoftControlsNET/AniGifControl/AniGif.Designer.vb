Namespace AniGifControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class AniGif

        Inherits System.Windows.Forms.UserControl

        ' UserControl überschreibt Dispose, um die Komponentenliste zu bereinigen.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing Then
                    Me.DisposeAnimationResources()

                    If components IsNot Nothing Then
                        components.Dispose()
                    End If
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Wird vom Windows Form-Designer benötigt.
        Private components As System.ComponentModel.IContainer
        Private WithEvents Timer As System.Windows.Forms.Timer

        'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.Timer = New System.Windows.Forms.Timer(Me.components)
            Me.SuspendLayout()
            '
            'Timer
            '
            Me.Timer.Interval = 200
            '
            'AniGif
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.DoubleBuffered = True
            Me.Name = "AniGif"
            Me.Size = New System.Drawing.Size(137, 114)
            Me.ResumeLayout(False)

        End Sub

    End Class

End Namespace
