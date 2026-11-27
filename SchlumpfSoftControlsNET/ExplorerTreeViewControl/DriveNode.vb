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
            Me.New($"{GetVolumeLabel(Drive)} ({GetDriveName(Drive)})", Drive.Name, GetImageKey(GetDriveTypeString(Drive)))
        End Sub

        ''' <summary>Erstellt einen Laufwerksknoten aus bereits ermittelten Metadaten ohne weitere Laufwerksabfrage.</summary>
        ''' <param name="text">Anzeigename des Laufwerks.</param>
        ''' <param name="path">Vollständiger Laufwerkspfad.</param>
        ''' <param name="imageKey">Schlüssel des Laufwerkssymbols.</param>
        Friend Sub New(text As String, path As String, imageKey As String)
            Me.Text = text
            Me.Tag = path
            Me.ImageKey = imageKey
            Me.SelectedImageKey = imageKey
            Me.AddPlaceholder()
        End Sub

    End Class

End Namespace

