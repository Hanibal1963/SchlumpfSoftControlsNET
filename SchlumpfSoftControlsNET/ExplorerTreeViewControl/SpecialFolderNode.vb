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

        Inherits DirectoryNode

        ''' <summary>Gibt den Pfad für das verzögerte Laden zurück.</summary>
        Friend Overrides ReadOnly Property DirectoryPath As String
            Get
                Return Me.FullPath
            End Get
        End Property

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
            Me.AddPlaceholder()

        End Sub

    End Class

End Namespace
