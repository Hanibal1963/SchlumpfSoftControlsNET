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

        Inherits TreeNode

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
            Dim unused = Me.Nodes.Add(New TreeNode("Ordner laden ..."))
        End Sub

        ''' <summary>
        ''' Lädt die Unterordner des Laufwerks und fügt sie als FolderNode-Knoten hinzu.
        ''' </summary>
        Public Sub LoadSubfolders()
            Try
                ' Erstellt ein DriveInfo-Objekt für das aktuelle Laufwerk
                Dim drive As New DriveInfo(Me.FullPath)
                ' Prüft, ob das Laufwerk bereit ist (z. B. CD eingelegt, Netzwerk verbunden)
                If drive.IsReady Then
                    ' Durchläuft alle Unterverzeichnisse des Laufwerks
                    For Each dir As String In Directory.GetDirectories(Me.FullPath)
                        ' Fügt jeden gefundenen Ordner als FolderNode dem Knoten hinzu
                        Dim unused = Me.Nodes.Add(New FolderNode(Path.GetFileName(dir), dir))
                    Next
                End If
            Catch ex As UnauthorizedAccessException
                ' Zugriff verweigert – Ordner wird übersprungen, keine Fehlermeldung
            Catch ex As IOException
                ' IO-Fehler – z.B. Laufwerk nicht verfügbar, keine Fehlermeldung
            Catch ex As Exception
                ' Allgemeiner Fehler – optional loggen, keine Fehlermeldung
            End Try
        End Sub

    End Class

End Namespace

