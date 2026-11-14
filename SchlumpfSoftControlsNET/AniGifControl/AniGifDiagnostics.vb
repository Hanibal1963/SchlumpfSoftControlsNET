' --------------------------------------------------------------------------------------------------------
' Datei: AniGifDiagnostics.vb
' Author: Andreas Sauer
' Datum: 13.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Diagnostics

Namespace AniGifControl

	''' <summary>
	''' Unveränderliche Momentaufnahme der optionalen AniGif-Diagnostik.
	''' </summary>
	''' <remarks>
	''' Die Framewerte beschreiben Zeichenaufrufe des Controls und keine bestätigte Bildschirmausgabe.
	''' </remarks>
	Public NotInheritable Class AniGifDiagnostics

		Friend Sub New(isEnabled As Boolean, activeTicks As Int64, renderedFrames As Int64,
				droppedFrames As Int64, renderOperations As Int64, totalRenderTicks As Int64,
				maximumRenderTicks As Int64)
			Me.IsEnabled = isEnabled
			Me.RenderedFrames = renderedFrames
			Me.DroppedFrames = droppedFrames
			Me.RenderOperations = renderOperations
			Me.ActiveDuration = TimeSpan.FromTicks(CLng(Math.Round(CDbl(activeTicks) * TimeSpan.TicksPerSecond / Stopwatch.Frequency, MidpointRounding.AwayFromZero)))

			If activeTicks > 0 Then
				Me.ActualFps = CDbl(renderedFrames) * Stopwatch.Frequency / activeTicks
			End If

			If renderOperations > 0 Then
				Me.AverageRenderTimeMs = CDbl(totalRenderTicks) * 1000.0R / Stopwatch.Frequency / renderOperations
				Me.MaxRenderTimeMs = CDbl(maximumRenderTicks) * 1000.0R / Stopwatch.Frequency
			End If
		End Sub

		''' <summary>
		''' Gibt an, ob die Diagnostik zum Zeitpunkt der Momentaufnahme aktiviert ist.
		''' </summary>
		Public ReadOnly Property IsEnabled As Boolean

		''' <summary>
		''' Gibt die kumulierte aktive und zeichnungsfähige Wiedergabezeit zurück.
		''' </summary>
		Public ReadOnly Property ActiveDuration As TimeSpan

		''' <summary>
		''' Gibt die mittlere Anzahl gezeichneter Timer-Framegenerationen pro aktiver Sekunde zurück.
		''' Ohne messbare aktive Zeit ist der Wert <see langword="Nothing"/>.
		''' </summary>
		Public ReadOnly Property ActualFps As Double?

		''' <summary>
		''' Gibt die Anzahl der mindestens einmal gezeichneten Timer-Framegenerationen zurück.
		''' </summary>
		Public ReadOnly Property RenderedFrames As Int64

		''' <summary>
		''' Gibt die Anzahl der durch einen späteren Timer-Frame vor dem Zeichnen ersetzten Frames zurück.
		''' </summary>
		Public ReadOnly Property DroppedFrames As Int64

		''' <summary>
		''' Gibt die Anzahl erfolgreicher GIF-Zeichenaufrufe zurück, einschließlich wiederholter Repaints.
		''' </summary>
		Public ReadOnly Property RenderOperations As Int64

		''' <summary>
		''' Gibt die mittlere Dauer eines GIF-Zeichenaufrufs in Millisekunden zurück.
		''' Ohne erfolgreichen Zeichenaufruf ist der Wert <see langword="Nothing"/>.
		''' </summary>
		Public ReadOnly Property AverageRenderTimeMs As Double?

		''' <summary>
		''' Gibt die längste Dauer eines GIF-Zeichenaufrufs in Millisekunden zurück.
		''' Ohne erfolgreichen Zeichenaufruf ist der Wert <see langword="Nothing"/>.
		''' </summary>
		Public ReadOnly Property MaxRenderTimeMs As Double?

	End Class

End Namespace