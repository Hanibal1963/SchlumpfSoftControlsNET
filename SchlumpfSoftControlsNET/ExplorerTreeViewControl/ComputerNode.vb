' --------------------------------------------------------------------------------------------------------
' Datei: ComputerNode.vb
' Author: Andreas Sauer
' Datum: 23.08.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.IO
Imports System.Windows.Forms

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Repräsentiert den Knoten für "Dieser Computer" im ExplorerTreeViewControl.
    ''' </summary>
    ''' <remarks>
    ''' Dieser Knoten enthält spezielle Ordner und Laufwerke des Computers.
    ''' </remarks>
    Friend NotInheritable Class ComputerNode

        Inherits TreeNode

        ''' <summary>Enthält Daten eines Wurzeleintrags unabhängig von TreeView-Knoten.</summary>
        Friend NotInheritable Class RootEntry

            ''' <summary>Initialisiert die Anzeige- und Pfaddaten eines Wurzeleintrags.</summary>
            ''' <param name="text">Anzeigename.</param>
            ''' <param name="path">Vollständiger Pfad.</param>
            ''' <param name="imageKey">Schlüssel des Symbols.</param>
            ''' <param name="isDrive">Kennzeichnet einen Laufwerkseintrag.</param>
            Friend Sub New(text As String, path As String, imageKey As String, isDrive As Boolean)
                Me.Text = text
                Me.Path = path
                Me.ImageKey = imageKey
                Me.IsDrive = isDrive
            End Sub

            ''' <summary>Gibt den Anzeigenamen zurück.</summary>
            Friend ReadOnly Property Text As String

            ''' <summary>Gibt den vollständigen Pfad zurück.</summary>
            Friend ReadOnly Property Path As String

            ''' <summary>Gibt den Symbolschlüssel zurück.</summary>
            Friend ReadOnly Property ImageKey As String

            ''' <summary>Gibt an, ob der Eintrag ein Laufwerk beschreibt.</summary>
            Friend ReadOnly Property IsDrive As Boolean

        End Class

        ''' <summary>Ermittelt Spezialordner und Laufwerksmetadaten ohne Zugriff auf Baumknoten.</summary>
        ''' <returns>Die Daten für die unmittelbaren Wurzelknoten.</returns>
        Friend Shared Function ReadEntries() As List(Of RootEntry)
            Dim entries As New List(Of RootEntry)
            For Each name As String In {"Desktop", "Dokumente", "Downloads", "Musik", "Bilder", "Videos"}
                entries.Add(New RootEntry(name, GetSpezialFolderPath(name), GetImageKey(name), False))
            Next
            For Each drive As DriveInfo In DriveInfo.GetDrives()
                entries.Add(ReadDriveEntry(drive))
            Next
            Return entries
        End Function

        ''' <summary>Ermittelt die Anzeige- und Symboldaten eines Laufwerks ohne Zugriff auf Baumknoten.</summary>
        ''' <param name="drive">Das abzufragende Laufwerk.</param>
        ''' <returns>Vorbereitete Daten für den Laufwerksknoten.</returns>
        Friend Shared Function ReadDriveEntry(drive As DriveInfo) As RootEntry
            Return New RootEntry($"{GetVolumeLabel(drive)} ({GetDriveName(drive)})", drive.Name, GetImageKey(GetDriveTypeString(drive)), True)
        End Function

        ''' <summary>Übernimmt vorbereitete Wurzeleinträge auf dem UI-Thread.</summary>
        ''' <param name="entries">Zuvor ermittelte Daten für Ordner und Laufwerke.</param>
        Friend Sub ApplyEntries(entries As List(Of RootEntry))
            Me.Nodes.Clear()
            For Each entry As RootEntry In entries
                Dim node = If(entry.IsDrive,
                    New DriveNode(entry.Text, entry.Path, entry.ImageKey),
                    DirectCast(New SpecialFolderNode(entry.Text, entry.Path, entry.ImageKey), TreeNode))
                Dim unused = Me.Nodes.Add(node)
            Next
        End Sub

        ''' <summary>
        ''' Initialisiert eine neue Instanz der ComputerNode-Klasse und setzt den Text des Knotens auf den
        ''' Computernamen.
        ''' </summary>
        Public Sub New()
            'Setze den Text des Knotens mit dem Computernamen
            Me.Text = $"Dieser Computer ({Environment.MachineName})"
            ' Setze das Icon für den Knoten
            Dim key As String = GetImageKey(ICON_COMPUTER)
            Me.ImageKey = key
            Me.SelectedImageKey = key
            ' Leert die Knoten, um Platz für spezielle Ordner und Laufwerke zu schaffen
            Me.Nodes.Clear()
            ' Füge Platzhalterknoten hinzu, die später durch spezielle Ordner und Laufwerke ersetzt werden
            Me.Nodes.AddRange({New TreeNode("Spezielle Ordner laden ..."), New TreeNode("Laufwerke laden ...")})
        End Sub

        ''' <summary>
        ''' Lädt die speziellen Ordner wie Desktop, Dokumente, Downloads usw. und fügt sie als Knoten hinzu.
        ''' </summary>
        Public Sub LoadSpecialFolders()
            Me.Nodes.AddRange({
                  New SpecialFolderNode("Desktop"),
                  New SpecialFolderNode("Dokumente"),
                  New SpecialFolderNode("Downloads"),
                  New SpecialFolderNode("Musik"),
                  New SpecialFolderNode("Bilder"),
                  New SpecialFolderNode("Videos")})
        End Sub

        ''' <summary>
        ''' Lädt die verfügbaren Laufwerke des Computers und fügt sie als DriveNode-Knoten hinzu.
        ''' </summary>
        Public Sub LoadDrives()
            For Each drive As DriveInfo In DriveInfo.GetDrives()
                Dim driveNode As New DriveNode(drive)
                Dim unused = Me.Nodes.Add(driveNode)
            Next
        End Sub

    End Class

End Namespace

