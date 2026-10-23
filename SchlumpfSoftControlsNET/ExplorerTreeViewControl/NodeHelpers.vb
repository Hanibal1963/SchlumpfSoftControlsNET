' --------------------------------------------------------------------------------------------------------
' Datei: NodeHelpers.vb
' Author: Andreas Sauer
' Datum: 23.08.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.IO

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Enthält Hilfsmethoden und Konstanten für die Arbeit mit Knoten im ExplorerTreeViewControl.
    ''' </summary>
    Friend Module NodeHelpers

#Region "Laufwerkstypen"

        Public Const DRIVETYPE_FIXED As String = "Fixed" ' Schlüssel für Laufwerkstyp: Lokaler Datenträger
        Public Const DRIVETYPE_CDROM As String = "CDROM"  ' Schlüssel für Laufwerkstyp: CD/DVD/BD-Laufwerk
        Public Const DRIVETYPE_REMOVABLE As String = "Removable" ' Schlüssel für Laufwerkstyp: Wechselmedium (z. B. USB-Stick)
        Public Const DRIVETYPE_NETWORK As String = "Network" ' Schlüssel für Laufwerkstyp: Netzlaufwerk
        Public Const DRIVETYPE_RAM As String = "RamDisk" ' Schlüssel für Laufwerkstyp: RAM-Disk
        Public Const DRIVETYPE_NOROOT As String = "NoRoot" ' Schlüssel für Laufwerkstyp: Kein Root-Verzeichnis vorhanden
        Public Const DRIVETYPE_UNKNOWN As String = "Unknown" ' Schlüssel für Laufwerkstyp: Unbekannt
        Public Const DRIVETYPE_SYSTEM As String = "System" ' Schlüssel für Laufwerkstyp: Systemlaufwerk
        Public Const DRIVETYPE_FLOPPY As String = "Floppy" ' Schlüssel für Laufwerkstyp: Diskettenlaufwerk

#End Region

#Region " Anzeigenamen der Laufwerkstypen"

        Public Const DRIVE_DESC_FIXED As String = "Lokaler Datenträger" ' Anzeigename für Laufwerkstyp: Lokaler Datenträger
        Public Const DRIVE_DESC_CDROM As String = "CD-Laufwerk" ' Anzeigename für Laufwerkstyp: CD-Laufwerk
        Public Const DRIVE_DESC_FLOPPY As String = "Diskettenlaufwerk" ' Anzeigename für Laufwerkstyp: Diskettenlaufwerk
        Public Const DRIVE_DESC_REMOVABLE As String = "Wechselmedium" ' Anzeigename für Laufwerkstyp: Wechselmedium
        Public Const DRIVE_DESC_NETWORK As String = "Netzlaufwerk" ' Anzeigename für Laufwerkstyp: Netzlaufwerk
        Public Const DRIVE_DESC_RAM As String = "Ramlaufwerk" ' Anzeigename für Laufwerkstyp: RAM-Laufwerk
        Public Const DRIVE_DESC_NOROOT As String = "kein Root-Verzeichnis" ' Anzeigename für Laufwerkstyp: Kein Root-Verzeichnis
        Public Const DRIVE_DESC_UNKNOWN As String = "Unbekanntes Laufwerk" ' Anzeigename für Laufwerkstyp: Unbekanntes Laufwerk

#End Region

#Region "Ordnernamen"

        Public Const FOLDER_COMPUTER As String = "Computer" ' Schlüssel für speziellen Ordner: Computer
        Public Const FOLDER_DESKTOP As String = "Desktop" ' Schlüssel für speziellen Ordner: Desktop
        Public Const FOLDER_DOKUMENTE As String = "Dokumente" ' Schlüssel für speziellen Ordner: Dokumente
        Public Const FOLDER_DOWNLOADS As String = "Downloads" ' Schlüssel für speziellen Ordner: Downloads
        Public Const FOLDER_MUSIK As String = "Musik" ' Schlüssel für speziellen Ordner: Musik
        Public Const FOLDER_BILDER As String = "Bilder" ' Schlüssel für speziellen Ordner: Bilder
        Public Const FOLDER_VIDEOS As String = "Videos" ' Schlüssel für speziellen Ordner: Videos
        Public Const FOLDER_FOLDER As String = "Folder" ' Schlüssel für allgemeinen Ordner

#End Region

#Region "Symbolbezeichnungen"

        Public Const ICON_COMPUTER As String = "Computer" ' Symbolschlüssel: Computer
        Public Const ICON_FOLDER_DESKTOP As String = "FolderDesktop" ' Symbolschlüssel: Ordner Desktop
        Public Const ICON_FOLDER_DOCUMENTS As String = "FolderDocuments" ' Symbolschlüssel: Ordner Dokumente
        Public Const ICON_FOLDER_DOWNLOADS As String = "FolderDownloads" ' Symbolschlüssel: Ordner Downloads
        Public Const ICON_FOLDER_MUSIC As String = "FolderMusic" 'Symbolschlüssel: Ordner Musik
        Public Const ICON_FOLDER_PICTURES As String = "FolderPictures" ' Symbolschlüssel: Ordner Bilder
        Public Const ICON_FOLDER_VIDEOS As String = "FolderVideos" ' Symbolschlüssel: Ordner Videos
        Public Const ICON_FOLDER_FOLDER As String = "Folder" ' Symbolschlüssel: Allgemeiner Ordner
        Public Const ICON_DRIVE_SYSTEM As String = "DriveSystem" ' Symbolschlüssel: Systemlaufwerk
        Public Const ICON_DRIVE_FIXED As String = "DriveFixed" ' Symbolschlüssel: Lokaler Datenträger
        Public Const ICON_DRIVE_CDROM As String = "DriveCDROM" ' Symbolschlüssel: CD/DVD/BD-Laufwerk
        Public Const ICON_DRIVE_FLOPPY As String = "DriveFloppy" ' Symbolschlüssel: Diskettenlaufwerk
        Public Const ICON_DRIVE_REMOVABLE As String = "DriveRemovable" ' Symbolschlüssel: Wechselmedium
        Public Const ICON_DRIVE_NETWORK As String = "DriveNetwork" ' Symbolschlüssel: Netzlaufwerk
        Public Const ICON_DRIVE_RAM As String = "DiveRamDisk" ' Symbolschlüssel: RAM-Disk
        Public Const ICON_DRIVE_NOROOT As String = "DriveNoRoot" ' Symbolschlüssel: Kein Root-Verzeichnis
        Public Const ICON_DRIVE_UNKNOWN As String = "DriveUnknown" ' Symbolschlüssel: Unbekannter Laufwerkstyp

#End Region

#Region "interne Dictionarys"

        ''' <summary>
        ''' Dieses Dictionary wird verwendet, um den Typ eines Laufwerks in eine menschenlesbare Zeichenfolge zu
        ''' übersetzen.
        ''' </summary>
        Private ReadOnly DriveTypeMappings As New Collections.Generic.Dictionary(Of DriveType, String) From {
            {DriveType.Fixed, DRIVETYPE_FIXED},
            {DriveType.CDRom, DRIVETYPE_CDROM},
            {DriveType.Removable, DRIVETYPE_REMOVABLE},
            {DriveType.Network, DRIVETYPE_NETWORK},
            {DriveType.Ram, DRIVETYPE_RAM},
            {DriveType.NoRootDirectory, DRIVETYPE_NOROOT},
            {DriveType.Unknown, DRIVETYPE_UNKNOWN}
        }

        ''' <summary>
        ''' Dieses Dictionary wird verwendet, um den vollständigen Pfad eines speziellen Ordners basierend auf seinem
        ''' Namen zu ermitteln.
        ''' </summary>
        Private ReadOnly FolderMappings As New Collections.Generic.Dictionary(Of String, String) From {
            {FOLDER_DESKTOP, Environment.GetFolderPath(Environment.SpecialFolder.Desktop)},
            {FOLDER_DOKUMENTE, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)},
            {FOLDER_DOWNLOADS, GetDownloadsFolderPath()},
            {FOLDER_MUSIK, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)},
            {FOLDER_BILDER, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)},
            {FOLDER_VIDEOS, Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)}
        }

        ''' <summary>
        ''' Dieses Dictionary wird verwendet, um die Imagekeys für Ordner und Laufwerke zu ermitteln.
        ''' </summary>
        Private ReadOnly ImageKeyMappings As New Collections.Generic.Dictionary(Of String, String) From {
            {FOLDER_COMPUTER, ICON_COMPUTER},
            {FOLDER_DESKTOP, ICON_FOLDER_DESKTOP},
            {FOLDER_DOKUMENTE, ICON_FOLDER_DOCUMENTS},
            {FOLDER_DOWNLOADS, ICON_FOLDER_DOWNLOADS},
            {FOLDER_MUSIK, ICON_FOLDER_MUSIC},
            {FOLDER_BILDER, ICON_FOLDER_PICTURES},
            {FOLDER_VIDEOS, ICON_FOLDER_VIDEOS},
            {FOLDER_FOLDER, ICON_FOLDER_FOLDER},
            {DRIVETYPE_SYSTEM, ICON_DRIVE_SYSTEM},
            {DRIVETYPE_FIXED, ICON_DRIVE_FIXED},
            {DRIVETYPE_CDROM, ICON_DRIVE_CDROM},
            {DRIVETYPE_FLOPPY, ICON_DRIVE_FLOPPY},
            {DRIVETYPE_REMOVABLE, ICON_DRIVE_REMOVABLE},
            {DRIVETYPE_NETWORK, ICON_DRIVE_NETWORK},
            {DRIVETYPE_RAM, ICON_DRIVE_RAM},
            {DRIVETYPE_NOROOT, ICON_DRIVE_NOROOT},
            {DRIVETYPE_UNKNOWN, ICON_DRIVE_UNKNOWN}
        }

#End Region

#Region "öffentliche Methoden"

        ''' <summary>
        ''' Gibt den Laufwerksnamen ohne den abschließenden Backslash zurück (z. B. "C:" statt "C:\").
        ''' </summary>
        ''' <param name="drive"></param>
        ''' <returns></returns>
        Public Function GetDriveName(drive As DriveInfo) As String
            ' Der Laufwerksname endet mit einem Backslash, der entfernt werden muss
            Return drive.Name.Substring(0, drive.Name.Length - 1)
        End Function

        ''' <summary>
        ''' Gibt das VolumeLabel (Laufwerksbezeichnung) des angegebenen Laufwerks zurück. Wenn das Laufwerk kein Label
        ''' besitzt oder nicht bereit ist, wird stattdessen eine Beschreibung des Laufwerkstyps zurückgegeben.
        ''' </summary>
        ''' <param name="drive"></param>
        ''' <returns></returns>
        Public Function GetVolumeLabel(drive As DriveInfo) As String

            ' Überprüfen, ob das Laufwerk bereit ist (z. B. ob ein Medium eingelegt und lesbar ist)
            If drive.IsReady Then
                ' Wenn das Laufwerk bereit ist, wird das VolumeLabel (Laufwerksbezeichnung) ermittelt.
                ' Falls das Laufwerk kein Label besitzt (VolumeLabel ist leer oder Nothing),
                ' wird stattdessen die Beschreibung des Laufwerkstyps als Label verwendet.
                Return If(String.IsNullOrEmpty(drive.VolumeLabel), GetDriveTypeDescription(drive), drive.VolumeLabel)
            Else
                ' Wenn das Laufwerk nicht bereit ist (z. B. kein Medium eingelegt),
                ' wird die Beschreibung des Laufwerkstyps als Label verwendet.
                Return GetDriveTypeDescription(drive)
            End If

            ' Rückgabe eines leeren Strings als Fallback (sollte eigentlich nie erreicht werden)
            Return String.Empty

        End Function

        ''' <summary>
        ''' Gibt den Laufwerkstyp des angegebenen Laufwerks als String zurück. Dies kann z. B. "Fixed", "CDROM",
        ''' "Removable" usw. sein. Für Systemlaufwerke und Diskettenlaufwerke werden spezielle Typen zurückgegeben.
        ''' </summary>
        ''' <param name="Drive"></param>
        ''' <returns></returns>
        Public Function GetDriveTypeString(Drive As DriveInfo) As String

            ' Überprüfen, ob das angegebene Laufwerk ein Systemlaufwerk ist.
            ' Systemlaufwerke sind in der Regel die primären Laufwerke, auf denen das Betriebssystem installiert ist.
            If IsSystemDrive(Drive) Then
                Return DRIVETYPE_SYSTEM
            End If

            ' Überprüfen, ob das angegebene Laufwerk ein Diskettenlaufwerk ist.
            ' Diskettenlaufwerke sind veraltete Speichermedien, die selten verwendet werden.
            If IsFloppyDrive(Drive) Then
                Return DRIVETYPE_FLOPPY
            End If

            Dim value As String = Nothing
            ' Versuchen, den Laufwerkstyp (DriveType) aus der vordefinierten Mapping-Tabelle zu ermitteln.
            ' Die Mapping-Tabelle ordnet DriveType-Werte (z. B. Fixed, CDRom) entsprechenden Zeichenfolgen zu.
            If DriveTypeMappings.TryGetValue(Drive.DriveType, value) Then
                Return value
            End If

            ' Wenn der Laufwerkstyp nicht erkannt wird, wird eine leere Zeichenfolge zurückgegeben.
            Return String.Empty

        End Function

        ''' <summary>
        ''' Gibt den vollständigen Pfad eines speziellen Ordners basierend auf dem angegebenen Text zurück. Wenn der
        ''' Text nicht in der Mapping-Tabelle vorhanden ist, wird eine leere Zeichenfolge zurückgegeben.
        ''' </summary>
        ''' <param name="Text"></param>
        ''' <returns></returns>
        Public Function GetSpezialFolderPath(Text As String) As String
            Dim value As String = Nothing
            Return If(FolderMappings.TryGetValue(Text, value), value, String.Empty)
        End Function

        ''' <summary>
        ''' Gibt den ImageKey für das angegebene IconTypeString zurück. Dies wird verwendet, um das passende Symbol für
        ''' spezielle Ordner oder Laufwerke zu ermitteln. Wenn der IconTypeString nicht in der Mapping-Tabelle vorhanden
        ''' ist, wird eine leere Zeichenfolge zurückgegeben.
        ''' </summary>
        ''' <param name="IconTypeString"></param>
        ''' <returns></returns>
        Public Function GetImageKey(IconTypeString As String) As String
            Dim value As String = Nothing
            ' Überprüft, ob das Dictionary "ImageKeyMappings" den angegebenen Schlüssel ("IconTypeString") enthält.
            ' Dies ist z. B. der Name eines Ordners oder Laufwerkstyps, für den ein passender ImageKey gesucht wird.
            If ImageKeyMappings.TryGetValue(IconTypeString, value) Then
                ' Wenn der Schlüssel gefunden wurde, wird der zugehörige ImageKey aus dem Dictionary zurückgegeben.
                Return value
            Else
                ' Falls der Schlüssel nicht existiert, wird eine leere Zeichenfolge zurückgegeben.
                ' Dies dient als Standardwert für unbekannte oder nicht zugeordnete Schlüssel.
                Return String.Empty
            End If

        End Function

#End Region

#Region "Interne Methoden"

        Private Function GetDownloadsFolderPath() As String

            ' Liest den tatsächlichen Pfad des Downloads-Ordners aus der Registry,
            ' da "Downloads" kein Standard-SpecialFolder ist und vom Benutzer verschoben werden kann.
            Dim path As String = CStr(Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", Nothing))

            If Not String.IsNullOrEmpty(path) Then
                Return path
            End If

            ' Fallback auf den Standard-Pfad, falls der Registry-Wert nicht vorhanden ist
            Return IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")

        End Function

        Private Function IsFloppyDrive(drive As DriveInfo) As Boolean

            ' Überprüft, ob das angegebene Laufwerk ein Diskettenlaufwerk ist.
            ' Diskettenlaufwerke sind traditionell die Laufwerke "A:" und "B:" unter Windows.
            ' Die Methode prüft, ob der Name des Laufwerks mit "a" oder "b" beginnt (unabhängig von Groß-/Kleinschreibung).
            ' Dies ist eine einfache Heuristik, da Diskettenlaufwerke in modernen Systemen selten sind,
            ' aber historisch immer mit diesen Buchstaben bezeichnet wurden.
            If drive.Name.StartsWith("a", StringComparison.OrdinalIgnoreCase) Or drive.Name.StartsWith("b", StringComparison.OrdinalIgnoreCase) Then
                Return True ' Wenn das Laufwerk mit "A" oder "B" beginnt, handelt es sich um ein Diskettenlaufwerk.
            End If

            ' Wenn das Laufwerk nicht mit "A" oder "B" beginnt, ist es kein Diskettenlaufwerk.
            Return False

        End Function

        Private Function IsSystemDrive(drive As DriveInfo) As Boolean

            ' Ermittelt das Root-Verzeichnis des Systemlaufwerks, indem der Pfad des Windows-Ordners verwendet wird.
            ' Beispiel: Wenn Windows auf "C:\Windows" installiert ist, ergibt Path.GetPathRoot(...) "C:\".
            Dim systemdrive As String = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows))

            ' Vergleicht den Namen des übergebenen Laufwerks mit dem ermittelten Systemlaufwerk.
            ' String.Equals wird verwendet, um eine kulturunabhängige, nicht case-sensitive Prüfung durchzuführen.
            If String.Equals(drive.Name, systemdrive, StringComparison.OrdinalIgnoreCase) Then
                Return True ' Wenn die Namen übereinstimmen, handelt es sich um das Systemlaufwerk.
            End If

            ' Falls keine Übereinstimmung vorliegt, ist das Laufwerk kein Systemlaufwerk.
            Return False

        End Function

        ''' <summary>
        ''' Gibt eine menschenlesbare Beschreibung des Laufwerkstyps zurück, basierend auf dem DriveType des angegebenen
        ''' DriveInfo-Objekts.
        ''' </summary>
        ''' <param name="drive"></param>
        ''' <returns></returns>
        Private Function GetDriveTypeDescription(drive As DriveInfo) As String

            Select Case drive.DriveType
                Case DriveType.Fixed : Return DRIVE_DESC_FIXED
                Case DriveType.CDRom : Return DRIVE_DESC_CDROM
                Case DriveType.Removable : Return If(IsFloppyDrive(drive), DRIVE_DESC_FLOPPY, DRIVE_DESC_REMOVABLE)
                Case DriveType.Network : Return DRIVE_DESC_NETWORK
                Case DriveType.Ram : Return DRIVE_DESC_RAM
                Case DriveType.NoRootDirectory : Return DRIVE_DESC_NOROOT
                Case DriveType.Unknown : Return DRIVE_DESC_UNKNOWN
                Case Else : Return String.Empty ' Fallback für unbekannte oder zukünftige DriveType-Werte
            End Select

        End Function

#End Region

    End Module

End Namespace
