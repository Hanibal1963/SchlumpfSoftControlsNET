' --------------------------------------------------------------------------------------------------------
' Datei: SelectedPathChangedEventArgs.vb
' Author: Andreas Sauer
' Datum: 23.08.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Stellt Daten für das Ereignis bereit, das ausgelöst wird, wenn sich der
    ''' ausgewählte Pfad ändert.
    ''' </summary>
    ''' <remarks>
    ''' Dieser Typ wird typischerweise mit einem Ereignis vom Typ <see
    ''' cref="EventHandler(Of TEventArgs)"/> verwendet, um den neuen Pfad an
    ''' Abonnenten zu übermitteln.
    ''' </remarks>
    Public Class SelectedPathChangedEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Ruft den aktuell ausgewählten Pfad ab.
        ''' </summary>
        ''' <remarks>
        ''' Der Wert repräsentiert typischerweise einen Dateisystempfad (z. B.
        ''' "C:\Ordner\Datei.txt").
        ''' </remarks>
        ''' <value>
        ''' Der ausgewählte Pfad als Zeichenfolge.
        ''' </value>
        Public ReadOnly Property SelectedPath As String

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see
        ''' cref="SelectedPathChangedEventArgs"/>-Klasse.
        ''' </summary>
        ''' <remarks>
        ''' Der übergebene Wert wird der schreibgeschützten Eigenschaft <see
        ''' cref="SelectedPath"/> zugewiesen.
        ''' </remarks>
        ''' <param name="Path">Der neue ausgewählte Pfad.</param>
        Public Sub New(Path As String)
            Me.SelectedPath = Path
        End Sub

    End Class

End Namespace
