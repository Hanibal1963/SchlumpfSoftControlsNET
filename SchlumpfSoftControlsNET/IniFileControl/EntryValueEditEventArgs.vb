' --------------------------------------------------------------------------------------------------------
' Datei: EntryValueEditEventArgs.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel

Namespace IniFileControl

    ''' <summary>
    ''' Enthält Ereignisdaten für bestätigte Wertänderungen eines INI-Eintrags.
    ''' </summary>
    ''' <remarks>
    ''' Diese Klasse wird im Ereignis <c>ValueChanged</c> von
    ''' <see cref="EntryValueEdit"/> verwendet.
    ''' </remarks>
    Public Class EntryValueEditEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Gibt den Namen der betroffenen INI-Sektion zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Abschnittsname ohne eckige Klammern, z. B. <c>"General"</c>.</value>
        Public Property SelectedSection As String = String.Empty

        ''' <summary>
        ''' Gibt den Namen des betroffenen Eintrags innerhalb der Sektion zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Schlüsselname, z. B. <c>"Theme"</c>.</value>
        Public Property SelectedEntry As String = String.Empty

        ''' <summary>
        ''' Gibt den neuen Wert des Eintrags zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Bestätigter Zielwert des Eintrags.</value>
        Public Property NewValue As String = String.Empty

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="EntryValueEditEventArgs"/>.
        ''' </summary>
        ''' <param name="SelectedSection">Name der INI-Sektion.</param>
        ''' <param name="SelectedEntry">Name des betroffenen Eintrags.</param>
        ''' <param name="NewValue">Bestätigter neuer Wert des Eintrags.</param>
        Public Sub New(SelectedSection As String, SelectedEntry As String, NewValue As String)
            ' Übergibt den Abschnittskontext an Event-Handler.
            Me.SelectedSection = SelectedSection
            ' Übergibt den betroffenen Schlüssel an Event-Handler.
            Me.SelectedEntry = SelectedEntry
            ' Übergibt den neuen Eintragswert an Event-Handler.
            Me.NewValue = NewValue
        End Sub

    End Class

End Namespace
