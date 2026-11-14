' --------------------------------------------------------------------------------------------------------
' Datei: FunctionDefinitions.vb
' Author: Andreas Sauer
' Datum: 26.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Drawing

Namespace AniGifControl

    ''' <summary>
    ''' Hilfsklasse mit gemeinsam genutzten Berechnungsfunktionen für das AniGif-Control.<br/> Berechnet
    ''' Zeichenpositionen und -größen abhängig vom gewählten <see cref="ImageSizeMode"/> <br/> sowie
    ''' Validierungsfunktionen für Zoom- und FPS-Werte.
    ''' </summary>
    Friend NotInheritable Class FunctionDefinitions

        ''' <summary>
        ''' Berechnet den Startpunkt (linke obere Ecke des Zeichenrechtecks) innerhalb der Clientfläche<br/>
        ''' in Abhängigkeit vom gewählten <see cref="ImageSizeMode"/>.
        ''' </summary>
        ''' <param name="Mode">
        ''' Der Anzeigemodus, der bestimmt wie das Bild positioniert wird.
        ''' </param>
        ''' <param name="Control">
        ''' Das AniGif-Control, auf dem das Bild gezeichnet wird.
        ''' </param>
        ''' <param name="Gif">
        ''' Das anzuzeigende Bitmap (wird für Originalgröße bei CenterImage benötigt).
        ''' </param>
        ''' <param name="RectStartSize">
        ''' Die bereits berechnete Zielgröße des Zeichenrechtecks (relevant bei Zoom und Fill).
        ''' </param>
        ''' <returns>
        ''' Der Punkt, an dem das Zeichnen beginnt (linke obere Ecke); bei fehlendem Bild oder leerer Clientfläche
        ''' <see cref="Point.Empty"/>.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">Das Control ist <see langword="Nothing"/>.</exception>
        Friend Shared Function GetRectStartPoint(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, RectStartSize As Size) As Point

            Dim clientSize As Size = GetClientSize(Control)
            If Gif Is Nothing Then Return Point.Empty

            If clientSize.Width <= 0 OrElse clientSize.Height <= 0 Then Return Point.Empty

            Select Case Mode

                Case ImageSizeMode.Normal
                    Return Point.Empty

                Case ImageSizeMode.CenterImage
                    Return New Point(CInt((clientSize.Width - Gif.Width) / 2D), CInt((clientSize.Height - Gif.Height) / 2D))

                Case ImageSizeMode.Zoom, ImageSizeMode.Fill
                    ' Bei Fill können negative Koordinaten durch den mittigen Beschnitt entstehen.
                    Return New Point(CInt((clientSize.Width - RectStartSize.Width) / 2D), CInt((clientSize.Height - RectStartSize.Height) / 2D))

                Case Else
                    Return Point.Empty

            End Select

        End Function

        ''' <summary>
        ''' Berechnet die Zielgröße des Zeichenrechtecks anhand der Clientfläche und des gewählten
        ''' <see cref="ImageSizeMode"/>.<br/> Das Ergebnis wird an <see cref="GetRectStartPoint"/> weitergegeben.
        ''' </summary>
        ''' <param name="Mode">
        ''' Der Anzeigemodus, der bestimmt wie das Bild skaliert wird.
        ''' </param>
        ''' <param name="Control">
        ''' Das AniGif-Control, das die verfügbare Zeichenfläche vorgibt.
        ''' </param>
        ''' <param name="Gif">
        ''' Das anzuzeigende Bitmap mit der Originalgröße als Berechnungsgrundlage.
        ''' </param>
        ''' <param name="Zoom">
        ''' Der relative Zoomfaktor von 0 bis 1 (nur im Modus <see cref="ImageSizeMode.Zoom"/> wirksam;
        ''' 0 ist nicht zulässig).
        ''' </param>
        ''' <returns>
        ''' Die berechnete Größe des Zeichenrechtecks oder <see cref="Size.Empty"/> bei fehlendem Bild
        ''' oder leerer Clientfläche. Positive skalierte Größen betragen mindestens ein Pixel je Dimension.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">Das Control ist <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Der relative Zoomfaktor liegt im Zoom-Modus nicht im gültigen Bereich.</exception>
        Friend Shared Function GetRectStartSize(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, Zoom As Decimal) As Size

            Dim clientSize As Size = GetClientSize(Control)
            If Mode = ImageSizeMode.Zoom AndAlso (Zoom <= 0D OrElse Zoom > 1D) Then Throw New ArgumentOutOfRangeException(NameOf(Zoom))
            If Gif Is Nothing Then Return Size.Empty

            If clientSize.Width <= 0 OrElse clientSize.Height <= 0 Then Return Size.Empty
            Dim imageSize As Size = Gif.Size

            Select Case Mode

                Case ImageSizeMode.Normal, ImageSizeMode.CenterImage
                    Return imageSize

                Case ImageSizeMode.Zoom, ImageSizeMode.Fill
                    Dim widthRatio As Long = CLng(clientSize.Width) * imageSize.Height
                    Dim heightRatio As Long = CLng(clientSize.Height) * imageSize.Width
                    Dim useWidth As Boolean = If(Mode = ImageSizeMode.Zoom, widthRatio <= heightRatio, widthRatio >= heightRatio)
                    Dim clientBasis As Integer = If(useWidth, clientSize.Width, clientSize.Height)
                    Dim imageBasis As Integer = If(useWidth, imageSize.Width, imageSize.Height)
                    Dim zoomScale As Decimal = If(Mode = ImageSizeMode.Zoom, Zoom, 1D)

                    ' Erst multiplizieren, damit ein periodischer Faktor Halbwerte nicht vorzeitig abrundet.
                    Dim scaledWidth As Decimal = CDec(imageSize.Width) * clientBasis * zoomScale / imageBasis
                    Dim scaledHeight As Decimal = CDec(imageSize.Height) * clientBasis * zoomScale / imageBasis
                    Dim width As Integer = Math.Max(1, CInt(Math.Round(scaledWidth, MidpointRounding.AwayFromZero)))
                    Dim height As Integer = Math.Max(1, CInt(Math.Round(scaledHeight, MidpointRounding.AwayFromZero)))
                    Return New Size(width, height)

                Case Else
                    Return imageSize

            End Select

        End Function

        Private Shared Function GetClientSize(Control As AniGif) As Size
#If NET6_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(Control, NameOf(Control))
#Else
            If Control Is Nothing Then Throw New ArgumentNullException(NameOf(Control))
#End If
            Return Control.ClientSize
        End Function

        ''' <summary>
        ''' Überprüft, ob der angegebene Zoomfaktor im zulässigen Bereich liegt, und korrigiert ihn gegebenenfalls.<br/>
        ''' Der gültige Bereich ist 1 bis 100 (entspricht 1 % bis 100 % Zoom).
        ''' </summary>
        ''' <param name="ZoomFactor">Der zu prüfende Zoomfaktor.</param>
        ''' <returns>
        ''' Den korrigierten Zoomfaktor innerhalb des Bereichs [1, 100].
        ''' </returns>
        Friend Shared Function CheckZoomFactorValue(ZoomFactor As Decimal) As Decimal

            Select Case ZoomFactor

                Case Is < 1
                    ' Unterschreitung: Mindestzoom (1) zurückgeben
                    Return 1

                Case Is > 100
                    ' Überschreitung: Maximalzoom (100) zurückgeben
                    Return 100

                Case Else
                    ' Wert liegt im gültigen Bereich → unverändert zurückgeben
                    Return ZoomFactor

            End Select

        End Function

        ''' <summary>
        ''' Überprüft, ob die angegebene Bildrate im zulässigen Bereich liegt, und korrigiert sie gegebenenfalls.<br/>
        ''' Der gültige Bereich ist 1 bis 50 Frames pro Sekunde.
        ''' </summary>
        ''' <param name="Frames">
        ''' Die zu prüfende Bildrate in Frames pro Sekunde (FPS).
        ''' </param>
        ''' <returns>
        ''' Die korrigierte Bildrate innerhalb des Bereichs [1, 50].
        ''' </returns>
        Friend Shared Function CheckFramesPerSecondValue(Frames As Decimal) As Decimal

            ' Stellt sicher, dass der FPS-Wert im zulässigen Bereich liegt (1 bis 50)
            Select Case Frames

                Case Is < 1
                    ' Unterschreitung: Mindest-FPS (1) zurückgeben
                    Return 1

                Case Is > 50
                    ' Überschreitung: Maximal-FPS (50) zurückgeben
                    Return 50

                Case Else
                    ' Wert liegt im gültigen Bereich → unverändert zurückgeben
                    Return Frames

            End Select

        End Function

    End Class

End Namespace
