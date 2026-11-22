' --------------------------------------------------------------------------------------------------------
' Datei: DriveInfoSource.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Namespace DriveWatcherControl

		''' <summary>Stellt Laufwerksinformationen über <see cref="DriveInfo"/> bereit.</summary>
    Friend NotInheritable Class DriveWatcherInfoSource

        Implements DriveWatcherIDriveInfoSource

		''' <summary>Enthält das zugrunde liegende Laufwerksobjekt.</summary>
        Private ReadOnly _drive As DriveInfo

		''' <summary>Initialisiert die Datenquelle für ein Laufwerk.</summary>
		''' <param name="name">Name des abzufragenden Laufwerks.</param>
        Friend Sub New(name As String)
			' Die einzelnen Eigenschaften werden erst bei ihrem Abruf gelesen.
            Me._drive = New DriveInfo(name)
        End Sub

		''' <summary>Ruft den Laufwerkstyp ab.</summary>
        Public ReadOnly Property DriveType As DriveType Implements DriveWatcherIDriveInfoSource.DriveType
            Get
                Return Me._drive.DriveType
            End Get
        End Property

		''' <summary>Ruft den Bereitschaftsstatus ab.</summary>
        Public ReadOnly Property IsReady As Boolean Implements DriveWatcherIDriveInfoSource.IsReady
            Get
                Return Me._drive.IsReady
            End Get
        End Property

		''' <summary>Ruft die Volumebezeichnung ab.</summary>
        Public ReadOnly Property VolumeLabel As String Implements DriveWatcherIDriveInfoSource.VolumeLabel
            Get
                Return Me._drive.VolumeLabel
            End Get
        End Property

		''' <summary>Ruft den für den aktuellen Benutzer verfügbaren freien Speicherplatz in Bytes ab.</summary>
        Public ReadOnly Property AvailableFreeSpace As Long Implements DriveWatcherIDriveInfoSource.AvailableFreeSpace
            Get
                Return Me._drive.AvailableFreeSpace
            End Get
        End Property

		''' <summary>Ruft den gesamten freien Speicherplatz in Bytes ab.</summary>
        Public ReadOnly Property TotalFreeSpace As Long Implements DriveWatcherIDriveInfoSource.TotalFreeSpace
            Get
                Return Me._drive.TotalFreeSpace
            End Get
        End Property

		''' <summary>Ruft die Gesamtgröße in Bytes ab.</summary>
        Public ReadOnly Property TotalSize As Long Implements DriveWatcherIDriveInfoSource.TotalSize
            Get
                Return Me._drive.TotalSize
            End Get
        End Property

        ''' <summary>Ruft den Namen des Dateisystems ab.</summary>
        Public ReadOnly Property DriveFormat As String Implements DriveWatcherIDriveInfoSource.DriveFormat
            Get
                Return Me._drive.DriveFormat
            End Get
        End Property

    End Class

End Namespace
