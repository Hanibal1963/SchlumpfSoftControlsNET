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
        ''' Die Grafik wird unter Berücksichtigung von <see cref="AniGif.ZoomFactor"/> proportional in die Clientfläche
        ''' eingepasst und zentriert. Bei 100 % ist sie innerhalb dieser Fläche maximal groß.
        ''' </summary>
        Zoom = 2

        ''' <summary>
        ''' Die Grafik wird proportional skaliert und zentriert, sodass die Clientfläche vollständig gefüllt ist.
        ''' Dabei können Bildbereiche an den Rändern abgeschnitten werden.
        ''' </summary>
        Fill = 3

    End Enum

End Namespace
