' --------------------------------------------------------------------------------------------------------
' Datei: DriveNode.vb
' Author: Andreas Sauer
' Datum: 23.08.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.IO
Imports System.Windows.Forms

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Repräsentiert einen Knoten für ein Laufwerk im ExplorerTreeViewControl.
    ''' </summary>
    Friend NotInheritable Class DriveNode

        Inherits DirectoryNode

        ''' <summary>Gibt den Pfad für das verzögerte Laden zurück.</summary>
        Friend Overrides ReadOnly Property DirectoryPath As String
            Get
                Return Me.FullPath
            End Get
        End Property

        ''' <summary>
        ''' Gibt den vollständigen Pfad des Laufwerks zurück (z. B. "C:\").
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property FullPath As String
            Get
                Return Me.Tag.ToString()
            End Get
        End Property

        ''' <summary>
        ''' Gibt den Laufwerkstyp zurück (z. B. "Lokaler Datenträger", "CD-Laufwerk").
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DriveType As DriveType
            Get
                Return New DriveInfo(Me.Tag.ToString()).DriveType
            End Get
        End Property

        ''' <summary>
        ''' Initialisiert eine neue Instanz der DriveNode-Klasse mit den Informationen des angegebenen Laufwerks.
        ''' </summary>
        ''' <param name="Drive"></param>
        Public Sub New(Drive As DriveInfo)
            ' Setzt den Text des Knotens auf das Laufwerkslabel und den Laufwerksnamen (z. B. "Lokaler Datenträger (C:)").
            Me.Text = $"{GetVolumeLabel(Drive)} ({GetDriveName(Drive)})"
            ' Speichert den Laufwerksnamen (z. B. "C:\") im Tag des Knotens.
            Me.Tag = Drive.Name
            ' Ermittelt den Laufwerkstyp als String (z. B. "Lokaler Datenträger", "CD-Laufwerk").
            Dim drivetypestring As String = GetDriveTypeString(Drive)
            ' Ermittelt den Schlüssel für das Symbol basierend auf dem Laufwerkstyp.
            Dim key As String = GetImageKey(drivetypestring)
            ' Setzt das Symbol des Knotens (ImageKey) und das Symbol für den ausgewählten Zustand (SelectedImageKey).
            Me.ImageKey = key
            Me.SelectedImageKey = key
            ' Leert die Knoten, um Platz für Unterordner zu schaffen
            Me.Nodes.Clear()
            ' Füge einen Platzhalterknoten hinzu, der später durch die Unterordner ersetzt wird
            Me.AddPlaceholder()
        End Sub

    End Class

End Namespace

