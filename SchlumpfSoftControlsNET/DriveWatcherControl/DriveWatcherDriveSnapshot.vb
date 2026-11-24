' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherDriveSnapshot.vb
' Author: Andreas Sauer
' Datum: 24.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO

Namespace DriveWatcherControl

    ''' <summary>Stellt die beim Abruf ermittelten Informationen eines Laufwerks unveränderlich bereit.</summary>
    ''' <remarks>Fehlgeschlagene Abfragen sind in <see cref="ReadErrors"/> enthalten; Detailwerte werden nur bei Bereitschaft gelesen.</remarks>
    Public NotInheritable Class DriveWatcherDriveSnapshot

        ''' <summary>Erstellt eine unabhängige Momentaufnahme aus den gelesenen Laufwerksdaten.</summary>
        ''' <param name="source">Die durch den Laufwerksleser ermittelten Daten.</param>
        Friend Sub New(source As DriveWatcherAddedEventArgs)
            Me.DriveName = source.DriveName
            Me.DriveType = source.DriveType
            Me.IsReady = source.IsReady
            Me.VolumeLabel = source.VolumeLabel
            Me.DriveFormat = source.DriveFormat
            Me.AvailableFreeSpace = source.AvailableFreeSpace
            Me.TotalFreeSpace = source.TotalFreeSpace
            Me.TotalSize = source.TotalSize
            Dim errors As New Dictionary(Of String, Exception)(StringComparer.Ordinal)
            For Each entry In source.ReadErrors
                errors.Add(entry.Key, entry.Value)
            Next
            Me.ReadErrors = New ReadOnlyDictionary(Of String, Exception)(errors)
        End Sub

        ''' <summary>Ruft den Namen des Laufwerks ab.</summary>
        Public ReadOnly Property DriveName As String

        ''' <summary>Ruft den Laufwerkstyp oder bei einem Lesefehler Unknown ab.</summary>
        Public ReadOnly Property DriveType As DriveType

        ''' <summary>Gibt an, ob das Laufwerk zum Zeitpunkt der Abfrage bereit war.</summary>
        Public ReadOnly Property IsReady As Boolean

        ''' <summary>Ruft die Volumebezeichnung ab; ohne verfügbaren Wert ist sie leer.</summary>
        Public ReadOnly Property VolumeLabel As String

        ''' <summary>Ruft das Dateisystem ab; ohne verfügbaren Wert ist es leer.</summary>
        Public ReadOnly Property DriveFormat As String

        ''' <summary>Ruft den für den aktuellen Benutzer verfügbaren Speicher in Bytes ab.</summary>
        Public ReadOnly Property AvailableFreeSpace As Int64

        ''' <summary>Ruft den gesamten freien Speicher in Bytes ab.</summary>
        Public ReadOnly Property TotalFreeSpace As Int64

        ''' <summary>Ruft die Gesamtgröße des Laufwerks in Bytes ab.</summary>
        Public ReadOnly Property TotalSize As Int64

        ''' <summary>Ruft die schreibgeschützte Zuordnung fehlgeschlagener Einzelabfragen ab.</summary>
        Public ReadOnly Property ReadErrors As IReadOnlyDictionary(Of String, Exception)

    End Class

End Namespace