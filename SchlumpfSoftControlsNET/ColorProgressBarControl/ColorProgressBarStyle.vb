' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBarStyle.vb
' Author: Andreas Sauer
' Datum: 21.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace ColorProgressBarControl

    ''' <summary>Bestimmt die Darstellung des Fortschrittsbalkens.</summary>
    Public Enum ColorProgressBarStyle

        ''' <summary>Zeigt den Fortschritt in einer einheitlichen Farbe.</summary>
        Solid

        ''' <summary>Zeigt den Fortschritt mit einem vertikalen Farbverlauf.</summary>
        Gradient

        ''' <summary>Zeigt den Fortschritt mit diagonalen Streifen.</summary>
        Striped

        ''' <summary>Zeigt einen animierten, vom Fortschrittswert unabhängigen Balken.</summary>
        Marquee

    End Enum

End Namespace