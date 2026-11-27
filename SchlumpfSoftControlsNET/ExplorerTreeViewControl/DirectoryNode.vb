' --------------------------------------------------------------------------------------------------------
' Datei: DirectoryNode.vb
' Author: Andreas Sauer
' --------------------------------------------------------------------------------------------------------

Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms

Namespace ExplorerTreeViewControl

    ''' <summary>Verwaltet den verzögerten Lade- und Zugriffsstatus eines Verzeichnisknotens.</summary>
    Friend MustInherit Class DirectoryNode
        Inherits TreeNode

        ''' <summary>Beschreibt das Ergebnis des letzten Ladeversuchs; vor dem ersten Versuch ist es unbekannt.</summary>
        Friend Enum DirectoryStatus
            Unknown
            Accessible
            Missing
            AccessDenied
            Unavailable
        End Enum

        ''' <summary>Gibt den vollständigen Verzeichnispfad zurück.</summary>
        Friend MustOverride ReadOnly Property DirectoryPath As String

        ''' <summary>Gibt den zuletzt ermittelten Zustand des Verzeichnisses zurück.</summary>
        Friend Property Status As DirectoryStatus = DirectoryStatus.Unknown

        ''' <summary>Gibt an, ob die Unterverzeichnisse erfolgreich geladen wurden.</summary>
        Friend Property ChildrenLoaded As Boolean

        ''' <summary>Markiert einen noch nicht geladenen Knoten als aufklappbar, ohne das Dateisystem abzufragen.</summary>
        Protected Sub AddPlaceholder()
            Dim unused = Me.Nodes.Add(New TreeNode("Ordner laden ..."))
        End Sub

        ''' <summary>Lädt die unmittelbaren Unterordner und hält das Ergebnis für weitere Aufklappvorgänge vor.</summary>
        ''' <param name="refresh">Erzwingt eine erneute Auflistung nach einer Dateisystemänderung.</param>
        ''' <returns>True, wenn die Auflistung erfolgreich war.</returns>
        Friend Function LoadChildren(Optional refresh As Boolean = False) As Boolean
            If Me.ChildrenLoaded AndAlso Not refresh Then Return True
            If String.IsNullOrWhiteSpace(Me.DirectoryPath) Then
                Me.Status = DirectoryStatus.Missing
                Me.ChildrenLoaded = False
                Return False
            End If

            Try
                Dim directories As String() = Directory.GetDirectories(Me.DirectoryPath)
                Me.Nodes.Clear()
                For Each directory As String In directories
                    Dim unused = Me.Nodes.Add(New FolderNode(Path.GetFileName(directory), directory))
                Next
                Me.Status = DirectoryStatus.Accessible
                Me.ChildrenLoaded = True
                Return True
            Catch ex As UnauthorizedAccessException
                Me.Status = DirectoryStatus.AccessDenied
                Debug.WriteLine($"Zugriff auf '{Me.DirectoryPath}' verweigert: {ex.Message}")
            Catch ex As DirectoryNotFoundException
                Me.Status = DirectoryStatus.Missing
                Debug.WriteLine($"Verzeichnis '{Me.DirectoryPath}' nicht gefunden: {ex.Message}")
            Catch ex As IOException
                Me.Status = DirectoryStatus.Unavailable
                Debug.WriteLine($"Verzeichnis '{Me.DirectoryPath}' nicht verfügbar: {ex.Message}")
            End Try

            Me.ChildrenLoaded = False
            Return False
        End Function
    End Class
End Namespace