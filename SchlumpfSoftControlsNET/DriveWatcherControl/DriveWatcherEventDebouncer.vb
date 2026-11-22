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
    Friend NotInheritable Class DriveWatcherEventDebouncer

		''' <summary>Zeitfenster für Wiederholungen in Millisekunden.</summary>
        Private Const WindowMilliseconds As Int32 = 200

        ''' <summary>Schützt den Ereignisspeicher vor gleichzeitigem Zugriff.</summary>
        Private ReadOnly _syncRoot As New Object()

        ''' <summary>Speichert Zeitstempel je Laufwerk und Ereignistyp.</summary>
        Private ReadOnly _events As New Dictionary(Of String, Int64?())(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Liefert den aktuellen Zeitstempel.</summary>
        Private ReadOnly _clock As Func(Of Int64)

        ''' <summary>Gibt die Zeitstempeltakte pro Sekunde an.</summary>
        Private ReadOnly _frequency As Int64

        ''' <summary>Initialisiert die Entprellung mit der Stoppuhr als Zeitquelle.</summary>
        Friend Sub New()

            Me.New(AddressOf Stopwatch.GetTimestamp, Stopwatch.Frequency)

        End Sub

        ''' <summary>Initialisiert die Entprellung mit einer vorgegebenen Zeitquelle.</summary>
        ''' <param name="clock">Funktion zum Abrufen des aktuellen Zeitstempels.</param>
        ''' <param name="frequency">Positive Anzahl der Zeitstempeltakte pro Sekunde.</param>
        ''' <exception cref="ArgumentNullException">Die Zeitquelle ist <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Die Frequenz ist nicht positiv.</exception>
        Friend Sub New(clock As Func(Of Int64), frequency As Int64)

#If NET8_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(clock, NameOf(clock))
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequency, NameOf(frequency))
#Else
            If clock Is Nothing Then Throw New ArgumentNullException(NameOf(clock))
            If frequency <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(frequency))
#End If
            Me._clock = clock
            Me._frequency = frequency

        End Sub

        ''' <summary>Prüft ein Ereignis und merkt neue Ereignisse für das Zeitfenster vor.</summary>
        ''' <param name="driveName">Laufwerksname; <see langword="Nothing"/> gilt als leerer Name.</param>
        ''' <param name="eventType">Art des Ereignisses.</param>
        ''' <returns><see langword="True"/>, wenn das Ereignis noch als Duplikat gilt.</returns>
        Friend Function IsDuplicate(driveName As String, eventType As DriveWatcherEventType) As Boolean

            SyncLock Me._syncRoot

                Dim now As Int64 = Me._clock()
                Dim expiredDrives As New List(Of String)()

				' Abgelaufene Zeitstempel entfernen und leere Laufwerkseinträge sammeln.
                For Each entry In Me._events

                    Dim hasActiveEvent As Boolean = False

                    For index As Int32 = 0 To entry.Value.Length - 1

                        If entry.Value(index).HasValue Then

                            If Me.IsExpired(now, entry.Value(index).Value) Then entry.Value(index) = Nothing
                            If entry.Value(index).HasValue Then hasActiveEvent = True

                        End If

                    Next

                    If Not hasActiveEvent Then expiredDrives.Add(entry.Key)

                Next

				' Einträge erst nach dem Durchlaufen des Wörterbuchs entfernen.
                For Each expiredDrive In expiredDrives

                    Dim unused = Me._events.Remove(expiredDrive)

                Next

                Dim key As String = If(driveName, "")
                Dim events As Int64?() = Nothing

                If Not Me._events.TryGetValue(key, events) Then

                    events = New Int64?(5) {}
                    Me._events.Add(key, events)

                End If

                Dim eventIndex As Int32 = CInt(eventType)
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
        Private Function IsExpired(now As Int64, previous As Int64) As Boolean

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