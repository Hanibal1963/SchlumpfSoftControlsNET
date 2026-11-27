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
            Me.New(Text, GetSpezialFolderPath(Text), GetImageKey(Text))
        End Sub

        ''' <summary>Erstellt einen Spezialordnerknoten aus zuvor ermittelten Daten.</summary>
        ''' <param name="text">Anzeigename des Spezialordners.</param>
        ''' <param name="path">Ermittelter Verzeichnispfad.</param>
        ''' <param name="imageKey">Schlüssel des Ordnersymbols.</param>
        Friend Sub New(text As String, path As String, imageKey As String)
            Me.Text = text
            Me.Tag = path
            Me.ImageKey = imageKey
            Me.SelectedImageKey = imageKey
            Me.AddPlaceholder()
        End Sub

    End Class

End Namespace
