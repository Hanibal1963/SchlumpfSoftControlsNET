' --------------------------------------------------------------------------------------------------------
' Datei: GifScalingQualityPreset.vb
' Author: Andreas Sauer
' Datum: 07.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace AniGifControl

	''' <summary>
	''' Legt die Darstellungsqualität für die Skalierung einer GIF-Grafik fest.
	''' </summary>
	Public Enum GifScalingQualityPreset

		''' <summary>
		''' Glättet Foto-Inhalte mit hochwertiger bikubischer Interpolation.
		''' </summary>
		Photo = 0

		''' <summary>
		''' Erhält bei Pixel-Art harte Kanten durch Nächster-Nachbar-Interpolation.
		''' </summary>
		PixelArt = 1

	End Enum

End Namespace
