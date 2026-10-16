' --------------------------------------------------------------------------------------------------------
' Datei: ImageSizeMode.vb
' Author: Andreas Sauer
' Datum: 25.04.2026
' --------------------------------------------------------------------------------------------------------

Namespace AniGifControl

    ''' <summary>
    ''' Legt fest, wie eine Grafik innerhalb der verfügbaren Client-Fläche eines Controls angezeigt wird.
    ''' </summary>
    Public Enum ImageSizeMode

        ''' <summary>
        ''' Die Grafik wird in Originalgröße angezeigt; Ausrichtung erfolgt oben links.
        ''' </summary>
        Normal = 0

        ''' <summary>
        ''' Die Grafik wird in Originalgröße zentriert angezeigt.
        ''' </summary>
        CenterImage = 1

        ''' <summary>
        ''' Die Größe der Grafik wird einheitlich skaliert, sodass sie in den verfügbaren Bereich des Controls passt;
        ''' zentrierte Ausrichtung (1–100%).
        ''' </summary>
        Zoom = 2

        ''' <summary>
        ''' Die Grafik wird so skaliert, dass der verfügbare Bereich vollständig gefüllt wird; zentrierte Ausrichtung.
        ''' </summary>
        Fill = 3

    End Enum

End Namespace
