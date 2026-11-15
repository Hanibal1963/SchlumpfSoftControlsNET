' --------------------------------------------------------------------------------------------------------
' Datei: IDriveInfoSource.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Namespace DriveWatcherControl

    Friend Interface IDriveInfoSource

        ReadOnly Property DriveType As DriveType

        ReadOnly Property IsReady As Boolean

        ReadOnly Property VolumeLabel As String

        ReadOnly Property AvailableFreeSpace As Long

        ReadOnly Property TotalFreeSpace As Long

        ReadOnly Property TotalSize As Long

        ReadOnly Property DriveFormat As String

    End Interface

End Namespace

