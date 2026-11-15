' --------------------------------------------------------------------------------------------------------
' Datei: DriveInfoSource.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Namespace DriveWatcherControl

    Friend NotInheritable Class DriveInfoSource

        Implements IDriveInfoSource

        Private ReadOnly _drive As DriveInfo

        Friend Sub New(name As String)
            Me._drive = New DriveInfo(name)
        End Sub

        Public ReadOnly Property DriveType As DriveType Implements IDriveInfoSource.DriveType
            Get
                Return Me._drive.DriveType
            End Get
        End Property

        Public ReadOnly Property IsReady As Boolean Implements IDriveInfoSource.IsReady
            Get
                Return Me._drive.IsReady
            End Get
        End Property

        Public ReadOnly Property VolumeLabel As String Implements IDriveInfoSource.VolumeLabel
            Get
                Return Me._drive.VolumeLabel
            End Get
        End Property

        Public ReadOnly Property AvailableFreeSpace As Long Implements IDriveInfoSource.AvailableFreeSpace
            Get
                Return Me._drive.AvailableFreeSpace
            End Get
        End Property

        Public ReadOnly Property TotalFreeSpace As Long Implements IDriveInfoSource.TotalFreeSpace
            Get
                Return Me._drive.TotalFreeSpace
            End Get
        End Property

        Public ReadOnly Property TotalSize As Long Implements IDriveInfoSource.TotalSize
            Get
                Return Me._drive.TotalSize
            End Get
        End Property

        Public ReadOnly Property DriveFormat As String Implements IDriveInfoSource.DriveFormat
            Get
                Return Me._drive.DriveFormat
            End Get
        End Property
    End Class

End Namespace
