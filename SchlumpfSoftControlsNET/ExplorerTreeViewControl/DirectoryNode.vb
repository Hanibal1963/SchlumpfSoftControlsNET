' --------------------------------------------------------------------------------------------------------
' Datei: DirectoryNode.vb
' Author: Andreas Sauer
' Datum: 27.11.2026
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

        ''' <summary>Enthält das Ergebnis einer Verzeichnisabfrage ohne Verweise auf UI-Elemente.</summary>
        Friend NotInheritable Class LoadResult

            ''' <summary>Initialisiert Status und ermittelte Unterverzeichnisse.</summary>
            ''' <param name="status">Ergebnis der Abfrage.</param>
            ''' <param name="directories">Vollständige Pfade der Unterverzeichnisse.</param>
            Friend Sub New(status As DirectoryStatus, directories As String())
                Me.Status = status
                Me.Directories = directories
            End Sub

            ''' <summary>Gibt den Zugriffsstatus zurück.</summary>
            Friend ReadOnly Property Status As DirectoryStatus

            ''' <summary>Gibt die ermittelten Unterverzeichnisse zurück.</summary>
            Friend ReadOnly Property Directories As String()
        End Class

        ''' <summary>Markiert einen noch nicht geladenen Knoten als aufklappbar, ohne das Dateisystem abzufragen.</summary>
        Protected Sub AddPlaceholder()
            Dim unused = Me.Nodes.Add(New TreeNode("Ordner laden ..."))
        End Sub

        ''' <summary>Lädt die unmittelbaren Unterordner und hält das Ergebnis für weitere Aufklappvorgänge vor.</summary>
        ''' <param name="refresh">Erzwingt eine erneute Auflistung nach einer Dateisystemänderung.</param>
        ''' <returns>True, wenn die Auflistung erfolgreich war.</returns>
        Friend Function LoadChildren(Optional refresh As Boolean = False) As Boolean
            If Me.ChildrenLoaded AndAlso Not refresh Then Return True
            Return Me.ApplyLoadResult(ReadChildren(Me.DirectoryPath))
        End Function

        ''' <summary>Ermittelt Unterverzeichnisse ohne Zugriff auf Baumknoten und kann im Hintergrund ausgeführt werden.</summary>
        ''' <param name="directoryPath">Pfad des zu lesenden Verzeichnisses.</param>
        ''' <returns>Verzeichnispfade und Zugriffsstatus.</returns>
        Friend Shared Function ReadChildren(directoryPath As String) As LoadResult

            If String.IsNullOrWhiteSpace(directoryPath) Then
                Return New LoadResult(DirectoryStatus.Missing, Nothing)
            End If

            Try

                Return New LoadResult(DirectoryStatus.Accessible, Directory.GetDirectories(directoryPath))

            Catch ex As UnauthorizedAccessException
                Debug.WriteLine($"Zugriff auf '{directoryPath}' verweigert: {ex.Message}")
                Return New LoadResult(DirectoryStatus.AccessDenied, Nothing)

            Catch ex As DirectoryNotFoundException
                Debug.WriteLine($"Verzeichnis '{directoryPath}' nicht gefunden: {ex.Message}")
                Return New LoadResult(DirectoryStatus.Missing, Nothing)

            Catch ex As IOException
                Debug.WriteLine($"Verzeichnis '{directoryPath}' nicht verfügbar: {ex.Message}")
                Return New LoadResult(DirectoryStatus.Unavailable, Nothing)

            End Try

        End Function

        ''' <summary>Übernimmt ein Abfrageergebnis auf dem Thread des zugehörigen TreeViews.</summary>
        ''' <param name="result">Das zuvor ermittelte Ergebnis.</param>
        ''' <returns>True bei erfolgreicher Auflistung.</returns>
        Friend Function ApplyLoadResult(result As LoadResult) As Boolean

            Me.Status = result.Status

            If result.Status = DirectoryStatus.Accessible Then
                Dim remaining As New HashSet(Of String)(result.Directories, StringComparer.OrdinalIgnoreCase)
                For index As Int32 = Me.Nodes.Count - 1 To 0 Step -1
                    Dim child As FolderNode = TryCast(Me.Nodes(index), FolderNode)
                    If child Is Nothing OrElse Not remaining.Remove(child.FullPath) Then
                        Me.Nodes.RemoveAt(index)
                    End If
                Next
                For Each directory As String In result.Directories
                    If remaining.Remove(directory) Then
                        Dim unused = Me.Nodes.Add(New FolderNode(Path.GetFileName(directory), directory))
                    End If
                Next
                Me.ChildrenLoaded = True
                Return True
            End If

            Me.ChildrenLoaded = False
            Return False

        End Function

    End Class

End Namespace