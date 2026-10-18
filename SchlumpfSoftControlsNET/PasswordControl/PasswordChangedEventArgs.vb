' --------------------------------------------------------------------------------------------------------
' Datei: PasswordChangedEventArgs.vb
' Author: Andreas Sauer
' Datum: 24.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace PasswordControl

    ''' <summary>
    ''' Stellt Daten für das Ereignis bei Änderung des Passwortes bereit.
    ''' </summary>
    Public Class PasswordChangedEventArgs

        Inherits EventArgs

        ''' <summary>
        ''' Gibt den erzeugten Code des Passwortes zurück.
        ''' </summary>
        Public ReadOnly Property PasswordCode As String

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="PasswordChangedEventArgs" />-Klasse.
        ''' </summary>
        ''' <param name="Code">Der erzeugte Code des Passwortes.</param>
        Public Sub New(Code As String)
            ' Speichert den übergebenen Code für den späteren Zugriff im Ereignishandler.
            Me.PasswordCode = Code
        End Sub

    End Class

End Namespace
