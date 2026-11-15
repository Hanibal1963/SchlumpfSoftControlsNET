' --------------------------------------------------------------------------------------------------------
' Datei: DriveInfoSnapshotReader.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Security

Namespace DriveWatcherControl

    Friend NotInheritable Class DriveInfoSnapshotReader

        Private Sub New()
        End Sub

        Friend Shared Function Read(name As String) As DriveAddedEventArgs

            Return Read(name, Function(driveName) New DriveInfoSource(driveName))

        End Function

        Friend Shared Function Read(name As String, factory As Func(Of String, IDriveInfoSource)) As DriveAddedEventArgs

#Disable Warning CA1510 ' ThrowIfNull ist unter .NET Framework nicht verfügbar.
            If factory Is Nothing Then Throw New ArgumentNullException(NameOf(factory))
#Enable Warning CA1510

            Dim result As New DriveAddedEventArgs(name) With {.VolumeLabel = "", .DriveFormat = ""}
            Dim source = ReadValue(result, NameOf(DriveInfo), Function() factory(name), DirectCast(Nothing, IDriveInfoSource))

            If source Is Nothing Then

                If result.ReadErrors.Count = 0 Then Throw New InvalidOperationException("Die Laufwerksabfrage lieferte keine Datenquelle.")
                Return result

            End If

            result.DriveType = ReadValue(result, NameOf(result.DriveType), Function() source.DriveType, DriveType.Unknown)
            result.IsReady = ReadValue(result, NameOf(result.IsReady), Function() source.IsReady, False)

            If result.IsReady Then

                ' Einzelne Fehler dürfen die übrigen Metadaten nicht unterdrücken.
                result.VolumeLabel = ReadValue(result, NameOf(result.VolumeLabel), Function() source.VolumeLabel, "")
                result.AvailableFreeSpace = ReadValue(result, NameOf(result.AvailableFreeSpace), Function() source.AvailableFreeSpace, 0L)
                result.TotalFreeSpace = ReadValue(result, NameOf(result.TotalFreeSpace), Function() source.TotalFreeSpace, 0L)
                result.TotalSize = ReadValue(result, NameOf(result.TotalSize), Function() source.TotalSize, 0L)
                result.DriveFormat = ReadValue(result, NameOf(result.DriveFormat), Function() source.DriveFormat, "")

            End If

            Return result

        End Function

        Private Shared Function ReadValue(Of T)(result As DriveAddedEventArgs, propertyName As String, getter As Func(Of T), fallback As T) As T

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

            Catch ex As ArgumentException When propertyName = NameOf(DriveInfo)
                result.AddReadError(propertyName, ex)

            End Try

            Return fallback

        End Function

    End Class

End Namespace