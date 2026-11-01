' --------------------------------------------------------------------------------------------------------
' Datei: ListEditRenameEventArgs.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace IniFileControl

    ''' <summary>
    ''' Stellt Ereignisdaten für das Umbenennen eines Listeneintrags bereit.
    ''' </summary>
    Public Class ListEditRenameEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Enthält den ursprünglichen Namen des Eintrags vor der Umbenennung.
        ''' </summary>
        Public Property OldName As String

        ''' <summary>
        ''' Enthält den neuen Namen des Eintrags nach der Umbenennung.
        ''' </summary>
        Public Property NewName As String

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="ListEditRenameEventArgs"/>-Klasse.
        ''' </summary>
        ''' <param name="OldName">Der bisherige Name des Eintrags.</param>
        ''' <param name="NewName">Der neue Name, der gesetzt werden soll.</param>
        Public Sub New(OldName As String, NewName As String)
            ' Der alte Name wird gespeichert, um in Event-Handlern Vergleiche,
            ' Protokollierung oder Rückabwicklungen zu ermöglichen.
            Me.OldName = OldName
            ' Der neue Name wird separat abgelegt, damit Handler direkt mit dem Zielwert
            ' weiterarbeiten können, ohne zusätzliche Konvertierung oder Nachschlagen.
            Me.NewName = NewName
        End Sub

    End Class

End Namespace
