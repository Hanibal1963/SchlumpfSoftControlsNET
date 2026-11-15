Namespace ColorProgressBarControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ColorProgressBar

        Inherits System.Windows.Forms.UserControl

        ''' <summary>Gibt die vom Designer verwalteten Komponenten frei.</summary>
        ''' <param name="disposing">Gibt an, ob verwaltete Ressourcen freigegeben werden sollen.</param>
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

        ''' <summary>Enthält die vom Windows Forms-Designer verwalteten Komponenten.</summary>
        Private components As System.ComponentModel.IContainer

        ''' <summary>Initialisiert die vom Windows Forms-Designer festgelegten Eigenschaften.</summary>
        ''' <remarks>HINWEIS: Die folgende Prozedur wird vom Windows Form-Designer benötigt
        ''' Sie kann mit dem Windows Form-Designer geändert werden.
        ''' Nicht mit dem Code-Editor ändern.</remarks>
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

