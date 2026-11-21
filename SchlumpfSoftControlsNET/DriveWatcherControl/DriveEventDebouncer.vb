' --------------------------------------------------------------------------------------------------------
' Datei: DriveEventDebouncer.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics

Namespace DriveWatcherControl

		''' <summary>Unterdrückt kurzzeitig wiederholte Laufwerksereignisse gleichen Typs.</summary>
    Friend NotInheritable Class DriveEventDebouncer

		''' <summary>Zeitfenster für Wiederholungen in Millisekunden.</summary>
        Private Const WindowMilliseconds As Integer = 200

        ''' <summary>Schützt den Ereignisspeicher vor gleichzeitigem Zugriff.</summary>
        Private ReadOnly _syncRoot As New Object()

        ''' <summary>Speichert Zeitstempel je Laufwerk und Ereignistyp.</summary>
        Private ReadOnly _events As New Dictionary(Of String, Long?())(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Liefert den aktuellen Zeitstempel.</summary>
        Private ReadOnly _clock As Func(Of Long)

        ''' <summary>Gibt die Zeitstempeltakte pro Sekunde an.</summary>
        Private ReadOnly _frequency As Long

		''' <summary>Initialisiert die Entprellung mit der Stoppuhr als Zeitquelle.</summary>
        Friend Sub New()

            Me.New(AddressOf Stopwatch.GetTimestamp, Stopwatch.Frequency)

        End Sub

        ''' <summary>Initialisiert die Entprellung mit einer vorgegebenen Zeitquelle.</summary>
        ''' <param name="clock">Funktion zum Abrufen des aktuellen Zeitstempels.</param>
        ''' <param name="frequency">Positive Anzahl der Zeitstempeltakte pro Sekunde.</param>
        ''' <exception cref="ArgumentNullException">Die Zeitquelle ist <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Die Frequenz ist nicht positiv.</exception>
        Friend Sub New(clock As Func(Of Long), frequency As Long)

            'NOTE: Vermeidung prüfen
#Disable Warning CA1510, CA1512 ' Die ThrowIf-Hilfen sind unter .NET Framework nicht verfügbar.
            If clock Is Nothing Then Throw New ArgumentNullException(NameOf(clock))
            If frequency <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(frequency))
#Enable Warning CA1510, CA1512
            Me._clock = clock
            Me._frequency = frequency

        End Sub

        ''' <summary>Prüft ein Ereignis und merkt neue Ereignisse für das Zeitfenster vor.</summary>
        ''' <param name="driveName">Laufwerksname; <see langword="Nothing"/> gilt als leerer Name.</param>
        ''' <param name="eventType">Art des Ereignisses.</param>
        ''' <returns><see langword="True"/>, wenn das Ereignis noch als Duplikat gilt.</returns>
        Friend Function IsDuplicate(driveName As String, eventType As DriveEventType) As Boolean

            SyncLock Me._syncRoot

                Dim now As Long = Me._clock()
                Dim expiredDrives As New List(Of String)()

				' Abgelaufene Zeitstempel entfernen und leere Laufwerkseinträge sammeln.
                For Each entry In Me._events

                    Dim hasActiveEvent As Boolean = False

                    For index As Integer = 0 To entry.Value.Length - 1

                        If entry.Value(index).HasValue Then

                            If Me.IsExpired(now, entry.Value(index).Value) Then entry.Value(index) = Nothing
                            If entry.Value(index).HasValue Then hasActiveEvent = True

                        End If

                    Next

                    If Not hasActiveEvent Then expiredDrives.Add(entry.Key)

                Next

				' Einträge erst nach dem Durchlaufen des Wörterbuchs entfernen.
                For Each expiredDrive In expiredDrives

                    Me._events.Remove(expiredDrive)

                Next

                Dim key As String = If(driveName, "")
                Dim events As Long?() = Nothing

                If Not Me._events.TryGetValue(key, events) Then

                    events = New Long?(5) {}
                    Me._events.Add(key, events)

                End If

                Dim eventIndex As Integer = CInt(eventType)
                If events(eventIndex).HasValue Then Return True

                ' Ein entgegengesetzter Zustandswechsel macht die frühere Meldung erneut möglich.
				' Die Aufzählungswerte ordnen entgegengesetzte Ereignisse paarweise an.
                events(eventIndex Xor 1) = Nothing
                events(eventIndex) = now

                Return False

            End SyncLock

        End Function

		''' <summary>Prüft, ob ein Zeitstempel außerhalb des Entprellungsfensters liegt.</summary>
		''' <param name="now">Aktueller Zeitstempel.</param>
		''' <param name="previous">Zeitstempel des früheren Ereignisses.</param>
		''' <returns><see langword="True"/>, wenn das Zeitfenster überschritten ist.</returns>
        Private Function IsExpired(now As Long, previous As Long) As Boolean

            Return CDbl(now - previous) * 1000.0 / Me._frequency > WindowMilliseconds

        End Function

		''' <summary>Entfernt alle gespeicherten Ereignisse.</summary>
        Friend Sub Clear()

            SyncLock Me._syncRoot

                Me._events.Clear()

            End SyncLock

        End Sub

    End Class

End Namespace