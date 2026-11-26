' --------------------------------------------------------------------------------------------------------
' Datei: DriveInfoSnapshotReader.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Security
Imports System.Collections.Generic

Namespace DriveWatcherControl

    ''' <summary>Erstellt eine Momentaufnahme verfügbarer Laufwerksinformationen.</summary>
    Friend NotInheritable Class DriveWatcherInfoSnapshotReader

        ''' <summary>Verhindert die Instanziierung der Hilfsklasse.</summary>
        Private Sub New()
        End Sub

        ''' <summary>Liest eine Momentaufnahme über die reguläre Laufwerksdatenquelle.</summary>
        ''' <param name="name">Name des abzufragenden Laufwerks.</param>
        ''' <returns>Die Laufwerksdaten und gegebenenfalls Lesefehler.</returns>
        Friend Shared Function Read(name As String) As DriveWatcherAddedEventArgs

            Return Read(name, Function(driveName) New DriveWatcherInfoSource(driveName))

        End Function

        ''' <summary>Liest die angegebenen Laufwerke über die bereitgestellte Datenquellenfabrik.</summary>
        ''' <param name="names">Bei der Enumeration ermittelte Laufwerksnamen.</param>
        ''' <param name="factory">Erstellt die Datenquelle für einen Laufwerksnamen.</param>
        ''' <returns>Eine schreibgeschützte Liste unabhängiger Momentaufnahmen.</returns>
        Friend Shared Function ReadCurrent(names As IEnumerable(Of String), factory As Func(Of String, IDriveWatcherDriveInfoSource)) As IReadOnlyList(Of DriveWatcherDriveSnapshot)
            Dim result As New List(Of DriveWatcherDriveSnapshot)()
            For Each name In names
                result.Add(New DriveWatcherDriveSnapshot(Read(name, factory, False)))
            Next
            Return result.AsReadOnly()
        End Function

        ''' <summary>Liest eine Momentaufnahme über eine vorgegebene Datenquellenfabrik.</summary>
        ''' <param name="name">Name des abzufragenden Laufwerks.</param>
        ''' <param name="factory">Erstellt die Datenquelle für den Laufwerksnamen.</param>
        ''' <param name="includeSerialNumber">Gibt an, ob die Volumeseriennummer gelesen wird.</param>
        ''' <returns>Die Laufwerksdaten und gegebenenfalls Lesefehler.</returns>
        ''' <exception cref="ArgumentNullException">Die Fabrik ist <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Die Fabrik liefert ohne erfassten Lesefehler keine Datenquelle.</exception>
        Friend Shared Function Read(name As String, factory As Func(Of String, IDriveWatcherDriveInfoSource), Optional includeSerialNumber As Boolean = True) As DriveWatcherAddedEventArgs

#If NET8_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(factory, NameOf(factory))
#Else
            If factory Is Nothing Then Throw New ArgumentNullException(NameOf(factory))
#End If

            Dim result As New DriveWatcherAddedEventArgs(name) With {.VolumeLabel = "", .DriveFormat = ""}
            Dim source = ReadValue(result, NameOf(DriveInfo), Function() factory(name), DirectCast(Nothing, IDriveWatcherDriveInfoSource))

            If source Is Nothing Then

                If result.ReadErrors.Count = 0 Then Throw New InvalidOperationException("Die Laufwerksabfrage lieferte keine Datenquelle.")
                Return result

            End If

            result.DriveType = ReadValue(result, NameOf(result.DriveType), Function() source.DriveType, DriveType.Unknown)
            result.IsNetwork = result.DriveType = DriveType.Network
            result.IsReady = ReadValue(result, NameOf(result.IsReady), Function() source.IsReady, False)

            ' Medienabhängige Eigenschaften nur bei gemeldeter Bereitschaft abfragen.
            If result.IsReady Then

                ' Einzelne Fehler dürfen die übrigen Metadaten nicht unterdrücken.
                result.VolumeLabel = ReadValue(result, NameOf(result.VolumeLabel), Function() source.VolumeLabel, "")
                result.AvailableFreeSpace = ReadValue(result, NameOf(result.AvailableFreeSpace), Function() source.AvailableFreeSpace, 0L)
                result.TotalFreeSpace = ReadValue(result, NameOf(result.TotalFreeSpace), Function() source.TotalFreeSpace, 0L)
                result.TotalSize = ReadValue(result, NameOf(result.TotalSize), Function() source.TotalSize, 0L)
                result.DriveFormat = ReadValue(result, NameOf(result.DriveFormat), Function() source.DriveFormat, "")
                If includeSerialNumber Then
                    result.VolumeSerialNumber = ReadValue(Of UInt32?)(result, NameOf(result.VolumeSerialNumber), Function() source.VolumeSerialNumber, Nothing)
                End If

            End If

            Return result

        End Function

		''' <summary>Liest einen Wert und erfasst erwartete Zugriffs- und Abfragefehler.</summary>
		''' <typeparam name="T">Typ des gelesenen Werts.</typeparam>
		''' <param name="result">Momentaufnahme zur Aufnahme eines Lesefehlers.</param>
		''' <param name="propertyName">Name der Eigenschaft oder der Laufwerksabfrage.</param>
		''' <param name="getter">Funktion zum Lesen des Werts.</param>
		''' <param name="fallback">Ersatzwert bei einem erfassten Fehler.</param>
		''' <returns>Der gelesene Wert oder der Ersatzwert.</returns>
        Private Shared Function ReadValue(Of T)(result As DriveWatcherAddedEventArgs, propertyName As String, getter As Func(Of T), fallback As T) As T

            Try

                Return getter()

            Catch ex As IOException
                result.AddReadError(propertyName, ex)

            Catch ex As UnauthorizedAccessException
                result.AddReadError(propertyName, ex)

            Catch ex As SecurityException
                result.AddReadError(propertyName, ex)

            Catch ex As Win32Exception
                result.AddReadError(propertyName, ex)

			' Argumentfehler nur beim Erstellen der Datenquelle abfangen.
            Catch ex As ArgumentException When propertyName = NameOf(DriveInfo)
                result.AddReadError(propertyName, ex)

            End Try

            Return fallback

        End Function

    End Class

End Namespace