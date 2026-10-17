' --------------------------------------------------------------------------------------------------------
' Datei: DiagonalLineModes.vb
' Author: Andreas Sauer
' Datum: 05.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel

Namespace ShapeControl

    ''' <summary>
    ''' Legt fest, in welcher Richtung eine diagonale Linie gezeichnet wird.
    ''' </summary>
    Public Enum DiagonalLineModes

        ''' <summary>
        ''' Zeichnet die diagonale Linie von links oben nach rechts unten.
        ''' </summary>
        ''' <remarks>
        ''' Die Linie verläuft von der oberen linken Ecke des Steuerelements zur unteren rechten Ecke.
        ''' </remarks>
        TopLeftToBottomRight = 0

        ''' <summary>
        ''' Zeichnet die diagonale Linie von links unten nach rechts oben.
        ''' </summary>
        ''' <remarks>
        ''' Die Linie verläuft von der unteren linken Ecke des Steuerelements zur oberen rechten Ecke.
        ''' </remarks>
        BottomLeftToTopRight = 1

    End Enum

End Namespace
