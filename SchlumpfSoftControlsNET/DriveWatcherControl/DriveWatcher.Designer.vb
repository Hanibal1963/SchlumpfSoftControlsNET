Namespace DriveWatcherControl

    Partial Class DriveWatcher

        Inherits System.ComponentModel.Component

        ''' <summary>
        ''' Initialisiert eine neue Instanz und fügt sie dem angegebenen Komponentencontainer hinzu.
        ''' </summary>
        ''' <param name="container">Der Komponentencontainer oder <see langword="Nothing"/>.</param>
        ''' <remarks>
        ''' Schlägt das Hinzufügen zum Container fehl, werden die bereits angelegten Ressourcen freigegeben.
        ''' Das Empfangsfenster sollte auf dem Thread seiner Erstellung freigegeben werden.
        ''' </remarks>
        ''' <exception cref="System.ComponentModel.Win32Exception">Das Empfangsfenster konnte nicht erstellt werden.</exception>
        <System.Diagnostics.DebuggerNonUserCode()>
        Public Sub New(ByVal container As System.ComponentModel.IContainer)

            MyClass.New()

            'Erforderlich für die Unterstützung des Windows.Forms-Klassenkompositions-Designers
            If (container IsNot Nothing) Then
                Dim registered As Boolean = False
                Try
                    container.Add(Me)
                    registered = True
                Finally
                    If Not registered Then
                        ' Ein abgeleiteter Typ ist zu diesem Zeitpunkt möglicherweise noch nicht initialisiert.
                        MyClass.Dispose(True)
                        GC.SuppressFinalize(Me)
                    End If
                End Try
            End If

        End Sub

        ''' <summary>
        ''' Gibt das Empfangsfenster und die verwalteten Komponenten frei.
        ''' </summary>
        ''' <param name="disposing">Gibt an, ob verwaltete Ressourcen freigegeben werden sollen.</param>
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            Try
                If disposing Then
                    Dim nativeForm As NativeForm = NatForm
                    ' Die WithEvents-Handler vor dem Zerstören des Fensters abmelden.
                    NatForm = Nothing
                    Try
                        If nativeForm IsNot Nothing Then nativeForm.Dispose()
                    Finally
                        If components IsNot Nothing Then components.Dispose()
                    End Try
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
