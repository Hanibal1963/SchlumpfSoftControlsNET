Namespace ExtendedRTFControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ExtendedRTF
        Inherits System.Windows.Forms.RichTextBox

        'RichTextBox überschreibt Dispose, um die Komponentenliste zu bereinigen.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
                ' Falls Entwickler vergessen hat EndUpdate mehrfach aufzurufen.
                While Me._updateNesting > 0
                    Me._updateNesting = 1
                    Me.EndUpdate()
                End While
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Erforderlich für den Windows-Form-Designer
        Private components As System.ComponentModel.IContainer

        'HINWEIS: Die folgende Prozedur ist für den Windows-Form-Designer erforderlich.
        'Sie kann mit dem Windows-Form-Designer geändert werden.
        'Nicht mit dem Code-Editor ändern.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'ExtendedRTF
            '
            Me.Name = "ExtendedRTF"
            Me.ResumeLayout(False)
        End Sub

    End Class

End Namespace
