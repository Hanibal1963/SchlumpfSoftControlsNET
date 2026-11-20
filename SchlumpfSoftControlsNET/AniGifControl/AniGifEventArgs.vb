' --------------------------------------------------------------------------------------------------------
' Datei: AniGifEventArgs.vb
' Author: Andreas Sauer
' Datum: 20.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace AniGifControl

    ''' <summary>Enthält die nullbasierten Indizes eines erfolgreichen Framewechsels.</summary>
    Public NotInheritable Class FrameChangedEventArgs
        Inherits EventArgs

        ''' <summary>Erstellt Ereignisdaten für einen Framewechsel.</summary>
        ''' <param name="previousFrame">Index des vorherigen Frames.</param>
        ''' <param name="currentFrame">Index des neuen Frames.</param>
        Public Sub New(previousFrame As Int32, currentFrame As Int32)
            Me.PreviousFrame = previousFrame
            Me.CurrentFrame = currentFrame
        End Sub

        ''' <summary>Gibt den Index des vorherigen Frames zurück.</summary>
        Public ReadOnly Property PreviousFrame As Int32

        ''' <summary>Gibt den Index des neuen Frames zurück.</summary>
        Public ReadOnly Property CurrentFrame As Int32
    End Class

End Namespace
