' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBarThreshold.vb
' Author: Andreas Sauer
' Datum: 22.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing

Namespace ColorProgressBarControl

    ''' <summary>Beschreibt einen unveränderlichen inklusiven Wertebereich und seine Balkenfarbe.</summary>
    Public NotInheritable Class ColorProgressBarThreshold

        ''' <summary>Erstellt einen Schwellenwertbereich für nicht negative Fortschrittswerte.</summary>
        ''' <param name="minimum">Inklusive untere Grenze.</param>
        ''' <param name="maximum">Inklusive obere Grenze.</param>
        ''' <param name="color">Farbe für Werte innerhalb des Bereichs.</param>
        ''' <exception cref="ArgumentOutOfRangeException">Die Grenzen sind negativ oder vertauscht.</exception>
        Public Sub New(minimum As Integer, maximum As Integer, color As Color)
#If NETFRAMEWORK Then
            If minimum < 0 Then Throw New ArgumentOutOfRangeException(NameOf(minimum))
            If maximum < minimum Then Throw New ArgumentOutOfRangeException(NameOf(maximum))
#Else
            ArgumentOutOfRangeException.ThrowIfNegative(minimum)
            ArgumentOutOfRangeException.ThrowIfLessThan(maximum, minimum)
#End If
            Me.Minimum = minimum
            Me.Maximum = maximum
            Me.Color = color
        End Sub

        ''' <summary>Inklusive untere Grenze des Bereichs.</summary>
        Public ReadOnly Property Minimum As Integer

        ''' <summary>Inklusive obere Grenze des Bereichs.</summary>
        Public ReadOnly Property Maximum As Integer

        ''' <summary>Farbe des Bereichs, einschließlich transparenter und Systemfarben.</summary>
        Public ReadOnly Property Color As Color

    End Class

End Namespace