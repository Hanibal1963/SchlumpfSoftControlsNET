' --------------------------------------------------------------------------------------------------------
' Datei: ShapeModes.vb
' Author: Andreas Sauer
' Datum: 05.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel

Namespace ShapeControl

    ''' <summary>
    ''' Legt fest, welche Form vom <see cref="Shape"/> -Steuerelement dargestellt wird.
    ''' </summary>
    Public Enum ShapeModes

        ''' <summary>
        ''' Zeichnet eine horizontale Linie in der vertikalen Mitte des Steuerelements.
        ''' </summary>
        ''' <remarks>
        ''' Die Linie verläuft von der linken Kante des Steuerelements zur rechten Kante.
        ''' </remarks>
        HorizontalLine = 0

        ''' <summary>
        ''' Zeichnet eine vertikale Linie in der horizontalen Mitte des Steuerelements.
        ''' </summary>
        ''' <remarks>
        ''' Die Linie verläuft von der oberen Kante des Steuerelements zur unteren Kante.
        ''' </remarks>
        VerticalLine = 1

        ''' <summary>
        ''' Zeichnet eine diagonale Linie entsprechend dem Wert von <see cref="DiagonalLineModes"/>.
        ''' </summary>
        ''' <remarks>
        ''' Die Richtung der Linie wird über die Eigenschaft <see cref="DiagonalLineModes"/> festgelegt.
        ''' </remarks>
        DiagonalLine = 2

        ''' <summary>
        ''' Zeichnet ein nicht gefülltes Rechteck.
        ''' </summary>
        ''' <remarks>
        ''' Das Rechteck wird nur mit einem Rahmen gezeichnet, der Innenbereich bleibt transparent.
        ''' </remarks>
        Rectangle = 3

        ''' <summary>
        ''' Zeichnet ein Rechteck und füllt dessen Innenbereich.
        ''' </summary>
        ''' <remarks>
        ''' Das Rechteck wird mit einem Rahmen gezeichnet und der Innenbereich wird mit der Hintergrundfarbe gefüllt.
        ''' </remarks>
        FilledRectangle = 4

        ''' <summary>
        ''' Zeichnet eine nicht gefüllte Ellipse (bei gleichen Seitenlängen ein Kreis).
        ''' </summary>
        ''' <remarks>
        ''' Die Ellipse wird nur mit einem Rahmen gezeichnet, der Innenbereich bleibt transparent.
        ''' </remarks>
        Ellipse = 5

        ''' <summary>
        ''' Zeichnet eine Ellipse und füllt deren Innenbereich (bei gleichen Seitenlängen ein Kreis).
        ''' </summary>
        ''' <remarks>
        ''' Die Ellipse wird mit einem Rahmen gezeichnet und der Innenbereich wird mit der Hintergrundfarbe gefüllt.
        ''' </remarks>
        FilledEllipse = 6

    End Enum

End Namespace
