' --------------------------------------------------------------------------------------------------------
' Datei: ListEditEventArgs.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace IniFileControl

    ''' <summary>
    ''' Stellt Ereignisdaten bereit, wenn sich der aktuell ausgewählte Listeneintrag ändert.
    ''' </summary>
    Public Class ListEditSelectedElementChangedEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Enthält den aktuell ausgewählten Eintrag.
        ''' </summary>
        Public Property SelectedElement As String = String.Empty

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="ListEditSelectedElementChangedEventArgs"/>-Klasse.
        ''' </summary>
        ''' <param name="SelectedElement">Der neue, aktuell ausgewählte Eintrag.</param>
        Public Sub New(SelectedElement As String)
            Me.SelectedElement = SelectedElement
        End Sub

    End Class

End Namespace