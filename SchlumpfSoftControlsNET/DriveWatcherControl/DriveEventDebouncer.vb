' --------------------------------------------------------------------------------------------------------
' Datei: DriveEventDebouncer.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics

Namespace DriveWatcherControl

    Friend NotInheritable Class DriveEventDebouncer

        Private Const WindowMilliseconds As Integer = 200

        Private ReadOnly _syncRoot As New Object()
        Private ReadOnly _events As New Dictionary(Of String, Long?())(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _clock As Func(Of Long)
        Private ReadOnly _frequency As Long

        Friend Sub New()

            Me.New(AddressOf Stopwatch.GetTimestamp, Stopwatch.Frequency)

        End Sub

        Friend Sub New(clock As Func(Of Long), frequency As Long)

#Disable Warning CA1510, CA1512 ' Die ThrowIf-Hilfen sind unter .NET Framework nicht verfügbar.
            If clock Is Nothing Then Throw New ArgumentNullException(NameOf(clock))
            If frequency <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(frequency))
#Enable Warning CA1510, CA1512
            Me._clock = clock
            Me._frequency = frequency

        End Sub

        Friend Function IsDuplicate(driveName As String, eventType As DriveEventType) As Boolean

            SyncLock Me._syncRoot

                Dim now As Long = Me._clock()
                Dim expiredDrives As New List(Of String)()

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
                events(eventIndex Xor 1) = Nothing
                events(eventIndex) = now

                Return False

            End SyncLock

        End Function

        Private Function IsExpired(now As Long, previous As Long) As Boolean

            Return CDbl(now - previous) * 1000.0 / Me._frequency > WindowMilliseconds

        End Function

        Friend Sub Clear()

            SyncLock Me._syncRoot

                Me._events.Clear()

            End SyncLock

        End Sub

    End Class

End Namespace