' --------------------------------------------------------------------------------------------------------
' Datei: IDriveInfoSource.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Namespace DriveWatcherControl

    ''' <summary>Definiert die Datenquelle für eine Laufwerksmomentaufnahme.</summary>
    Friend Interface IDriveWatcherDriveInfoSource

        ''' <summary>Ruft den Laufwerkstyp ab.</summary>
        ReadOnly Property DriveType As DriveType

        ''' <summary>Ruft den Bereitschaftsstatus ab.</summary>
        ReadOnly Property IsReady As Boolean

        ''' <summary>Ruft die Volumebezeichnung ab.</summary>
        ReadOnly Property VolumeLabel As String

        ''' <summary>Ruft den für den aktuellen Benutzer verfügbaren freien Speicherplatz in Bytes ab.</summary>
        ReadOnly Property AvailableFreeSpace As Int64

        ''' <summary>Ruft den gesamten freien Speicherplatz in Bytes ab.</summary>
        ReadOnly Property TotalFreeSpace As Int64

        ''' <summary>Ruft die Gesamtgröße in Bytes ab.</summary>
        ReadOnly Property TotalSize As Int64

        ''' <summary>Ruft den Namen des Dateisystems ab.</summary>
        ReadOnly Property DriveFormat As String

        ''' <summary>Ruft die vom Dateisystem vergebene Volumeseriennummer ab.</summary>
        ReadOnly Property VolumeSerialNumber As UInt32

    End Interface

End Namespace

