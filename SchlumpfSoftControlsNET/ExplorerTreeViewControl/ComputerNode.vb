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

