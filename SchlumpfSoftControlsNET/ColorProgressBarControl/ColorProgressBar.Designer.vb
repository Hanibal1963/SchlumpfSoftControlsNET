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

        ' HINWEIS: Die folgende Prozedur wird vom Windows Form-Designer benötigt
        ' Sie kann mit dem Windows Form-Designer geändert werden.  
        ' Nicht mit dem Code-Editor ändern.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            ' ColorProgressBar
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Name = "ColorProgressBar"
            Me.Padding = New System.Windows.Forms.Padding(1)
            Me.Size = New System.Drawing.Size(220, 23)
            Me.ResumeLayout(False)
        End Sub

    End Class

End Namespace

