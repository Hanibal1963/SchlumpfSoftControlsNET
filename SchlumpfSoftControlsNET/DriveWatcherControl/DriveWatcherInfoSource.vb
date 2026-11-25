' --------------------------------------------------------------------------------------------------------
' Datei: DriveInfoSource.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO
Imports System.ComponentModel
Imports System.Runtime.InteropServices

Namespace DriveWatcherControl

    ''' <summary>Stellt Laufwerksinformationen über <see cref="DriveInfo"/> bereit.</summary>
    Friend NotInheritable Class DriveWatcherInfoSource

        Implements IDriveWatcherDriveInfoSource

        ''' <summary>Fragt die Volumeseriennummer über die Windows-Dateisystem-API ab.</summary>
        ''' <param name="rootPath">Stammverzeichnis des Volumes mit abschließendem Backslash.</param>
        ''' <param name="volumeName">Nicht benötigter Namenspuffer.</param>
        ''' <param name="volumeNameSize">Größe des Namenspuffers.</param>
        ''' <param name="serialNumber">Empfängt die Volumeseriennummer.</param>
        ''' <param name="maximumComponentLength">Nicht benötigte maximale Komponentenlänge.</param>
        ''' <param name="fileSystemFlags">Nicht benötigte Dateisystemflags.</param>
        ''' <param name="fileSystemName">Nicht benötigter Dateisystempuffer.</param>
        ''' <param name="fileSystemNameSize">Größe des Dateisystempuffers.</param>
        ''' <returns>Gibt an, ob die Abfrage erfolgreich war.</returns>
        <DllImport("kernel32.dll", EntryPoint:="GetVolumeInformationW", CharSet:=CharSet.Unicode, SetLastError:=True, ExactSpelling:=True)>
        Private Shared Function GetVolumeInformation(rootPath As String, volumeName As IntPtr, volumeNameSize As UInteger, ByRef serialNumber As UInteger, maximumComponentLength As IntPtr, fileSystemFlags As IntPtr, fileSystemName As IntPtr, fileSystemNameSize As UInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>Enthält das zugrunde liegende Laufwerksobjekt.</summary>
        Private ReadOnly _drive As DriveInfo

        ''' <summary>Initialisiert die Datenquelle für ein Laufwerk.</summary>
        ''' <param name="name">Name des abzufragenden Laufwerks.</param>
        Friend Sub New(name As String)
            ' Die einzelnen Eigenschaften werden erst bei ihrem Abruf gelesen.
            Me._drive = New DriveInfo(name)
        End Sub

        ''' <summary>Ruft den Laufwerkstyp ab.</summary>
        Public ReadOnly Property DriveType As DriveType Implements IDriveWatcherDriveInfoSource.DriveType
            Get
                Return Me._drive.DriveType
            End Get
        End Property

        ''' <summary>Ruft die Volumeseriennummer ab und meldet Windows-Fehler als Ausnahme.</summary>
        Public ReadOnly Property VolumeSerialNumber As UInteger Implements IDriveWatcherDriveInfoSource.VolumeSerialNumber
            Get
                Dim serialNumber As UInteger
                If Not GetVolumeInformation(Me._drive.Name, IntPtr.Zero, 0UI, serialNumber, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0UI) Then
                    Throw New Win32Exception(Marshal.GetLastWin32Error())
                End If
                Return serialNumber
            End Get
        End Property

        ''' <summary>Ruft den Bereitschaftsstatus ab.</summary>
        Public ReadOnly Property IsReady As Boolean Implements IDriveWatcherDriveInfoSource.IsReady
            Get
                Return Me._drive.IsReady
            End Get
        End Property

		''' <summary>Ruft die Volumebezeichnung ab.</summary>
        Public ReadOnly Property VolumeLabel As String Implements IDriveWatcherDriveInfoSource.VolumeLabel
            Get
                Return Me._drive.VolumeLabel
            End Get
        End Property

		''' <summary>Ruft den für den aktuellen Benutzer verfügbaren freien Speicherplatz in Bytes ab.</summary>
        Public ReadOnly Property AvailableFreeSpace As Int64 Implements IDriveWatcherDriveInfoSource.AvailableFreeSpace
            Get
                Return Me._drive.AvailableFreeSpace
            End Get
        End Property

		''' <summary>Ruft den gesamten freien Speicherplatz in Bytes ab.</summary>
        Public ReadOnly Property TotalFreeSpace As Int64 Implements IDriveWatcherDriveInfoSource.TotalFreeSpace
            Get
                Return Me._drive.TotalFreeSpace
            End Get
        End Property

		''' <summary>Ruft die Gesamtgröße in Bytes ab.</summary>
        Public ReadOnly Property TotalSize As Int64 Implements IDriveWatcherDriveInfoSource.TotalSize
            Get
                Return Me._drive.TotalSize
            End Get
        End Property

        ''' <summary>Ruft den Namen des Dateisystems ab.</summary>
        Public ReadOnly Property DriveFormat As String Implements IDriveWatcherDriveInfoSource.DriveFormat
            Get
                Return Me._drive.DriveFormat
            End Get
        End Property

    End Class

End Namespace
