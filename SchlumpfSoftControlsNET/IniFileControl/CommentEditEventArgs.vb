' --------------------------------------------------------------------------------------------------------
' Datei: CommentEditEventArgs.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel

Namespace IniFileControl

    ''' <summary>
    ''' Enthält Ereignisdaten für bestätigte Änderungen eines Kommentartexts.
    ''' </summary>
    ''' <remarks>
    ''' Die Kommentarinhalte werden als Zeilenarray geführt, damit die ursprüngliche Zeilenstruktur direkt erhalten
    ''' bleibt.
    ''' </remarks>
    Public Class CommentEditEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Gibt den Kommentartext als Zeilenarray zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Kommentarzeilen der bestätigten Änderung.</value>
        Public Property Comment As String() = Array.Empty(Of String)()

        ''' <summary>
        ''' Gibt den Namen des Abschnitts zurück oder legt ihn fest, dem der Kommentar zugeordnet ist.
        ''' </summary>
        ''' <value>Abschnittsname ohne eckige Klammern, z. B. <c>"General"</c>.</value>
        Public Property Section As String = String.Empty

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="CommentEditEventArgs"/>.
        ''' </summary>
        ''' <param name="Section">Name des betroffenen Abschnitts.</param>
        ''' <param name="Comment">Bestätigter Kommentartext als Zeilenarray.</param>
        Public Sub New(Section As String, Comment() As String)
            ' Legt fest, zu welchem Abschnitt der Kommentar gehört.
            Me.Section = Section
            ' Übernimmt den Kommentar als Zeilenarray (referenziert das übergebene Array).
            Me.Comment = If(Comment, Array.Empty(Of String)())
        End Sub

    End Class

End Namespace
