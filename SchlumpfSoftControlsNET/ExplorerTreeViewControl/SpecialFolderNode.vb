' --------------------------------------------------------------------------------------------------------
' Datei: SpecialFolderNode.vb
' Author: Andreas Sauer
' Datum: 23.08.2026
' --------------------------------------------------------------------------------------------------------

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Repräsentiert einen Knoten für einen speziellen Windows-Ordner (z. B. Desktop, Dokumente, Downloads) im
    ''' ExplorerTreeViewControl.
    ''' </summary>
    ''' <remarks>
    ''' Dieser Knoten speichert den angezeigten Namen sowie den vollständigen Pfad des Spezialordners und kann
    ''' Unterordner dynamisch laden.
    ''' </remarks>
    Friend NotInheritable Class SpecialFolderNode

        Inherits TreeNode

        ''' <summary>
        ''' Gibt den vollständigen Pfad des Spezialordners zurück, der im Tag-Property gespeichert ist.
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property FullPath As String
            Get
                Return Me.Tag.ToString()
            End Get
        End Property

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="SpecialFolderNode"/> -Klasse.
        ''' </summary>
        ''' <param name="Text"></param>
        Public Sub New(Text As String)

            ' Setzt den angezeigten Text des Knotens auf den übergebenen Namen des Spezialordners
            Me.Text = Text

            ' Speichert den vollständigen Pfad des Spezialordners im Tag-Property des Knotens
            ' Die Methode GetSpezialFolderPath(Text) ermittelt den Pfad basierend auf dem Namen des Spezialordners (z.B. "Desktop")
            Me.Tag = GetSpezialFolderPath(Text)

            ' Ermittelt den Schlüssel für das anzuzeigende Symbol (ImageKey) anhand des Ordnernamens
            ' Die Hilfsmethode NodeHelpers.GetImageKey(Text) liefert einen passenden Schlüssel für die Bildliste
            Dim key As String = GetImageKey(Text)

            ' Setzt das Symbol des Knotens auf den ermittelten Schlüssel
            Me.ImageKey = key
            Me.SelectedImageKey = key

            ' Entfernt alle vorhandenen untergeordneten Knoten, um Platz für die später geladenen Unterordner zu schaffen
            Me.Nodes.Clear()

            ' Fügt einen Platzhalterknoten hinzu, der dem Benutzer anzeigt, dass die Unterordner noch geladen werden
            ' Dieser Platzhalter wird später durch die tatsächlichen Unterordner ersetzt, sobald diese geladen wurden
            Dim unused = Me.Nodes.Add(New System.Windows.Forms.TreeNode("Ordner laden ..."))

        End Sub

        ''' <summary>
        ''' Lädt die Unterordner des aktuellen Spezialordners und fügt sie als <see cref="FolderNode"/> -Knoten hinzu.
        ''' </summary>
        Public Sub LoadSubfolders()

            ' Versucht, die Unterordner des angegebenen Spezialordners zu laden
            Try

                ' Durchläuft alle Verzeichnisse (Unterordner) im Pfad des Spezialordners
                For Each dir As String In System.IO.Directory.GetDirectories(Me.FullPath)

                    ' Fügt für jeden gefundenen Unterordner einen neuen FolderNode zum aktuellen Knoten hinzu
                    ' IO.Path.GetFileName(dir) extrahiert den Ordnernamen aus dem vollständigen Pfad
                    ' "dir" ist der vollständige Pfad des Unterordners
                    Dim unused = Me.Nodes.Add(New FolderNode(System.IO.Path.GetFileName(dir), dir))

                Next

            Catch ex As System.UnauthorizedAccessException
                ' Falls der Zugriff auf einen Ordner verweigert wird, wird die Ausnahme abgefangen
                ' und der entsprechende Ordner übersprungen, ohne die Anwendung zu unterbrechen

            Catch ex As System.IO.DirectoryNotFoundException
                ' Ordner existiert nicht (z. B. wurde er verschoben)

            End Try
        End Sub

    End Class

End Namespace
