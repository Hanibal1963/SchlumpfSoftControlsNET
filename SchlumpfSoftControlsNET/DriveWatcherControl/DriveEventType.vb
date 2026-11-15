' --------------------------------------------------------------------------------------------------------
' Datei: DriveEventType.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace DriveWatcherControl

		''' <summary>Bezeichnet die paarweise gegenüberstehenden Laufwerksereignisse.</summary>
    Friend Enum DriveEventType

		''' <summary>Ein Laufwerk wurde hinzugefügt.</summary>
        Added = 0
		''' <summary>Ein Laufwerk wurde entfernt.</summary>
        Removed = 1
		''' <summary>Ein Netzlaufwerk wurde hinzugefügt.</summary>
        NetworkAdded = 2
		''' <summary>Ein Netzlaufwerk wurde entfernt.</summary>
        NetworkRemoved = 3
		''' <summary>Ein Medium wurde eingelegt.</summary>
        MediaInserted = 4
		''' <summary>Ein Medium wurde entfernt.</summary>
        MediaRemoved = 5

    End Enum

End Namespace
