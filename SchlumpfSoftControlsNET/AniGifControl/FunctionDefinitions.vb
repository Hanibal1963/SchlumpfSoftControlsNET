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
        ''' Berechnet den Startpunkt (linke obere Ecke des Zeichenrechtecks) auf dem Control<br/> in Abhängigkeit vom
        ''' gewählten <see cref="ImageSizeMode"/>.
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
        ''' Der Punkt, an dem das Zeichnen beginnt (linke obere Ecke).
        ''' </returns>
        Friend Shared Function GetRectStartPoint(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, RectStartSize As Size) As Point

            ' Bestimmt den Startpunkt (linke obere Ecke) des Zeichenrechtecks auf dem Control
            Select Case Mode

                Case ImageSizeMode.Normal
                    ' Bild beginnt ungeskalt an der linken oberen Ecke des Controls (Koordinatenursprung)
                    Return New Point(0, 0)

                Case ImageSizeMode.CenterImage
                    ' Bild wird in Originalgröße mittig im Control platziert.
                    ' Ein negativer Wert ist möglich, wenn das Bild größer als das Control ist
                    ' (dann wird das Bild außerhalb des sichtbaren Bereichs begonnen und erscheint abgeschnitten).
                    ' X = (Control-Breite  - Bild-Breite)  / 2
                    ' Y = (Control-Höhe - Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - Gif.Size.Width) / 2), CInt((Control.Height - Gif.Size.Height) / 2))

                Case ImageSizeMode.Zoom
                    ' Das skalierte Bild wird mittig im Control ausgerichtet.
                    ' RectStartSize enthält die bereits berechnete Größe des skalierten Bildes.
                    ' X = (Control-Breite  - skalierte Bild-Breite)  / 2
                    ' Y = (Control-Höhe - skalierte Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - RectStartSize.Width) / 2), CInt((Control.Height - RectStartSize.Height) / 2))

                Case ImageSizeMode.Fill
                    ' Das auf Control-Größe gestrekte Bild wird mittig ausgerichtet.
                    ' Da beim Fill-Modus das Bild mindestens eine Dimension vollständig füllt,
                    ' kann ein Wert 0 oder negativ sein (kein sichtbarer Versatz bzw. Überlappung).
                    ' X = (Control-Breite  - gestreckte Bild-Breite)  / 2
                    ' Y = (Control-Höhe - gestreckte Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - RectStartSize.Width) / 2), CInt((Control.Height - RectStartSize.Height) / 2))

                Case Else
                    ' Fallback: Zeichnen ab der linken oberen Ecke
                    Return New Point(0, 0)

            End Select

        End Function

        ''' <summary>
        ''' Berechnet die Zielgröße des Zeichenrechtecks in Abhängigkeit vom gewählten <see cref="ImageSizeMode"/>
        ''' .<br/> Das Ergebnis wird als Parameter an <see cref="GetRectStartPoint"/> weitergegeben.
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
        ''' Der Zoomfaktor (nur im Modus <see cref="ImageSizeMode.Zoom"/> wirksam).
        ''' </param>
        ''' <returns>
        ''' Die berechnete Größe des Zeichenrechtecks.
        ''' </returns>
        Friend Shared Function GetRectStartSize(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, Zoom As Decimal) As Size

            ' Null-Schutz: Wenn kein Bild vorhanden ist, leere Größe zurückgeben
            If Gif Is Nothing Then Return Size.Empty

            Select Case Mode

                Case ImageSizeMode.Normal
                    ' Bild wird ungeskalt in Originalgröße dargestellt
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

                Case ImageSizeMode.CenterImage
                    ' Bild wird ebenfalls in Originalgröße dargestellt, nur die Position ist zentriert
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

                Case ImageSizeMode.Zoom
                    ' Bild wird unter Beibehaltung des Seitenverhältnisses mit dem Zoomfaktor skaliert.
                    ' Je nach Ausrichtung (Hochformat/Querformat) wird eine andere Control-Dimension als Basis verwendet.
                    If Gif.Size.Width < Gif.Size.Height Then
                        ' Hochformat: Bild ist höher als breit → Höhe des Controls als Skalierungsbasis.
                        ' Breite  = Control-Höhe / Seitenverhältnis (Höhe/Breite) * Zoom
                        '         = Control-Höhe * (Bild-Breite / Bild-Höhe) * Zoom
                        ' Höhe = Control-Höhe * Zoom
                        Return New Size(CInt(Control.Height / CDec(Gif.Size.Height / Gif.Size.Width) * Zoom), CInt(Control.Height * Zoom))
                    Else
                        ' Querformat: Bild ist breiter als hoch (oder quadratisch) → Breite des Controls als Skalierungsbasis.
                        ' Breite = Control-Breite * Zoom
                        ' Höhe  = Control-Breite * Seitenverhältnis (Höhe/Breite) * Zoom
                        Return New Size(CInt(Control.Width * Zoom), CInt(Control.Width * CDec(Gif.Size.Height / Gif.Size.Width) * Zoom))
                    End If

                Case ImageSizeMode.Fill
                    ' Bild wird so skaliert, dass es das Control in einer Dimension vollständig ausfüllt.
                    ' Das Seitenverhältnis bleibt erhalten; die andere Dimension kann das Control überschreiten.
                    If Gif.Size.Width < Gif.Size.Height Then
                        ' Hochformat: Höhe des Controls als Basis, Breite wird proportional berechnet.
                        ' Breite = Control-Höhe / Seitenverhältnis (Höhe/Breite)
                        '        = Control-Höhe * (Bild-Breite / Bild-Höhe)
                        ' Höhe  = Control-Höhe (füllt das Control vollständig in der Höhe)
                        Return New Size(CInt(Control.Height / CDec(Gif.Size.Height / Gif.Size.Width)), Control.Height)
                    Else
                        ' Querformat: Breite des Controls als Basis, Höhe wird proportional berechnet.
                        ' Breite = Control-Breite (füllt das Control vollständig in der Breite)
                        ' Höhe  = Control-Breite * Seitenverhältnis (Höhe/Breite)
                        Return New Size(Control.Width, CInt(Control.Width * CDec(Gif.Size.Height / Gif.Size.Width)))
                    End If

                Case Else
                    ' Fallback: Originalgröße zurückgeben
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

            End Select

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
