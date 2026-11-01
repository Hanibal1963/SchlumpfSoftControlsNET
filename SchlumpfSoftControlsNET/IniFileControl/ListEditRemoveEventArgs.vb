' --------------------------------------------------------------------------------------------------------
' Datei: ListEditRemoveEventArgs.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace IniFileControl

    ''' <summary>
    ''' Stellt Ereignisdaten für das Entfernen eines vorhandenen Listeneintrags bereit.
    ''' </summary>
    Public Class ListEditRemoveEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Enthält den Eintrag, der aus der Liste entfernt werden soll oder bereits entfernt wurde.
        ''' </summary>
        Public Property ItemToRemove As String

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="ListEditRemoveEventArgs"/>-Klasse.
        ''' </summary>
        ''' <param name="ItemToRemove">Der eindeutige Name oder Text des zu entfernenden Eintrags.</param>
        Public Sub New(ItemToRemove As String)
            ' Der übergebene Wert wird unverändert übernommen, damit Event-Handler
            ' exakt den vom Aufrufer bestimmten Eintrag referenzieren können.
            Me.ItemToRemove = ItemToRemove
        End Sub

    End Class

End Namespace
