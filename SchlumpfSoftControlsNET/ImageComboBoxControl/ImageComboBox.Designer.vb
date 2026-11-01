Namespace ImageComboBoxControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ImageComboBox
        Inherits System.Windows.Forms.ComboBox

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

        ' Erforderlich für den Windows-Form-Designer
        Private components As System.ComponentModel.IContainer

        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'ImageComboBox
            '
            Me.Name = "ImageComboBox"
            Me.ResumeLayout(False)

        End Sub

    End Class

End Namespace

