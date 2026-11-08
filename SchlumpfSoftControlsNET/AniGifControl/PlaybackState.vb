' --------------------------------------------------------------------------------------------------------
' Datei: PlaybackState.vb
' Author: Andreas Sauer
' Datum: 08.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace AniGifControl

    ''' <summary>
    ''' Legt den aktuellen Wiedergabestatus einer Animation fest.
    ''' </summary>
    Public Enum PlaybackState

        ''' <summary>
        ''' Die Animation ist gestoppt.
        ''' </summary>
        Stopped = 0

        ''' <summary>
        ''' Die Animation wird abgespielt.
        ''' </summary>
        Playing = 1

        ''' <summary>
        ''' Die Animation ist pausiert.
        ''' </summary>
        Paused = 2

    End Enum

End Namespace
