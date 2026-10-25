Namespace SevenSegmentControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class MultiDigit

        Inherits System.Windows.Forms.Control

        'UserControl überschreibt Dispose, um die Komponentenliste zu bereinigen.
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

        'Erforderlich für den Windows Forms-Designer
        Private components As System.ComponentModel.IContainer

        'HINWEIS: Das folgende Verfahren ist für den Windows Forms-Designer erforderlich
        'Es kann mit dem Windows Forms-Designer geändert werden.  
        'Nicht mit dem Code-Editor ändern.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'Toolbox-Steuerelement
            '
            Me.Name = "MultiDigit"
            Me.ResumeLayout(False)
        End Sub

    End Class

End Namespace
