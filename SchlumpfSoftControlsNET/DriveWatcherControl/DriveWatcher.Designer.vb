Namespace DriveWatcherControl

    Partial Class DriveWatcher

        Inherits System.ComponentModel.Component

        <System.Diagnostics.DebuggerNonUserCode()>
        Public Sub New(ByVal container As System.ComponentModel.IContainer)

            MyClass.New()

            'Erforderlich für die Unterstützung des Windows.Forms-Klassenkompositions-Designers
            If (container IsNot Nothing) Then
                container.Add(Me)
            End If

        End Sub

        'Die Komponente überschreibt den Löschvorgang zum Bereinigen der Komponentenliste.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            Try
                If disposing Then
                    If NatForm IsNot Nothing Then
                        NatForm.Dispose()
                        NatForm = Nothing
                    End If

                    If components IsNot Nothing Then
                        components.Dispose()
                    End If
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try

        End Sub

        'Wird vom Komponenten-Designer benötigt.
        Private components As System.ComponentModel.IContainer

        'Hinweis: Die folgende Prozedur ist für den Komponenten-Designer erforderlich.
        'Das Bearbeiten ist mit dem Komponenten-Designer möglich.
        'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()

            components = New System.ComponentModel.Container()

        End Sub

    End Class

End Namespace
