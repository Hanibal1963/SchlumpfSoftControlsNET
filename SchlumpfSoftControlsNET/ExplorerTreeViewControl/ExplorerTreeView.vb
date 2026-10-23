' --------------------------------------------------------------------------------------------------------
' Datei: ExplorerTreeView.vb
' Author: Andreas Sauer
' Datum: 26.06.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace ExplorerTreeViewControl

    ''' <summary>
    ''' Steuerelement zur Anzeige und Navigation der lokalen Verzeichnisstruktur.
    ''' </summary>
    ''' <remarks>
    ''' <para>Das Control zeigt eine hierarchische Baumstruktur mit: </para>
    ''' <list type="bullet">
    '''  <item>
    '''   <description>dem Wurzelknoten "Dieser Computer", allen verfügbaren Laufwerken,
    ''' sowie bekannten Spezialordnern (z. B. Desktop, Dokumente, Bilder). Funktionen:
    ''' </description>
    '''  </item>
    '''  <item>
    '''   <description>Lazy-Loading von Unterordnern beim Expandieren eines Knotens.
    ''' </description>
    '''  </item>
    '''  <item>
    '''   <description>Live-Aktualisierung durch <see cref="FileSystemWatcher"/> bei
    ''' Änderungen im Dateisystem. </description>
    '''  </item>
    '''  <item>
    '''   <description>Ereignis <see cref="SelectedPathChanged"/> bei Auswahl eines
    ''' Knotens mit gültigem Pfad. </description>
    '''  </item>
    '''  <item>
    '''   <description>Öffnen und Selektieren eines Pfads über <see
    ''' cref="ExpandPath(String)"/>. </description>
    '''  </item>
    ''' </list>
    ''' <para><b>Hinweise:</b> </para>
    ''' <list type="bullet">
    '''  <item>
    '''   <description>Der Wurzelknoten ("Dieser Computer") besitzt keinen Pfad.
    ''' </description>
    '''  </item>
    '''  <item>
    '''   <description>Für geöffnete Knoten wird automatisch ein FileSystemWatcher
    ''' angelegt; beim Zuklappen wird er entfernt.</description>
    '''  </item>
    ''' </list>
    ''' </remarks>
    <Description("Stellt ein Steuerelement zur Anzeige und Navigation der Verzeichnisstruktur des Computers bereit.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ExplorerTreeView), "ExplorerTreeViewControl.ExplorerTreeView.bmp")>
    Public Class ExplorerTreeView : Inherits UserControl

        Implements IDisposable

#Region "Variablen"

        Private disposedValue As Boolean
        Private ReadOnly _FileSystemWatchers As New Dictionary(Of String, FileSystemWatcher)
        Private _SelectedPath As String

#End Region

#Region "Ereignisse"

        ''' <summary>
        ''' Ereignis, das ausgelöst wird, wenn sich der ausgewählte Pfad geändert hat.
        ''' </summary>
        ''' <remarks>
        '''
        ''' <para>Dieses Ereignis wird verwendet, um andere Steuerelemente oder Logik zu benachrichtigen, wenn der
        ''' Benutzer einen anderen Knoten im TreeView auswählt. </para>
        '''
        ''' <para>Es ermöglicht eine reaktive Programmierung, bei der andere Teile der Anwendung auf Änderungen im
        ''' ausgewählten Pfad reagieren können.</para>
        ''' </remarks>
        <Description("Wird ausgelöst, wenn sich der ausgewählte Pfad geändert hat.")>
        <Browsable(True)>
        Public Event SelectedPathChanged(sender As Object, e As SelectedPathChangedEventArgs)

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt den aktuell ausgewählten Pfad zurück.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)>
        Public ReadOnly Property SelectedPath As String
            Get
                Return Me._SelectedPath
            End Get
        End Property

        ''' <summary>
        ''' Gibt die Farbe der Linien zwischen den Knoten zurück oder legt diese fest.
        ''' </summary>
        ''' <remarks>
        ''' Diese Eigenschaft beeinflusst die Farbe der Verbindungslinien im TreeView.<br/> Eine Änderung wirkt sich
        ''' sofort auf die Darstellung aus.
        ''' </remarks>
        <Category("Behavior")>
        <Description("Gibt die Farbe der Linien zwischen den Knoten zurück oder legt diese fest.")>
        <Browsable(True)>
        <DefaultValue(GetType(Color), "ControlDark")>
        Public Property LineColor As Color
            Get
                Return Me.TV.LineColor
            End Get
            Set(value As Color)
                Me.TV.LineColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob Linien zwischen den Knoten angezeigt werden.
        ''' </summary>
        ''' <remarks>
        ''' Wenn <see langword="True"/>, werden Verbindungslinien zwischen Eltern- und Kindknoten dargestellt.
        ''' </remarks>
        <Category("Behavior")>
        <Description("Gibt an, ob Linien zwischen den Knoten angezeigt werden.")>
        <Browsable(True)>
        <DefaultValue(True)>
        Public Property ShowLines As Boolean
            Get
                Return Me.TV.ShowLines
            End Get
            Set(value As Boolean)
                Me.TV.ShowLines = value
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob die Plus- und Minuszeichen zum Anzeigen von Unterknoten angezeigt werden.
        ''' </summary>
        ''' <remarks>
        ''' Wenn <see langword="True"/>, werden Expand-/Collapse-Glyphen (Plus/Minus) neben Knoten angezeigt.
        ''' </remarks>
        <Category("Behavior")>
        <Description("Legt fest ob die Plus- und Minuszeichen zum Anzeigen von Unterknoten angezeigt werden.")>
        <Browsable(True)>
        <DefaultValue(True)>
        Public Property ShowPlusMinus As Boolean
            Get
                Return Me.TV.ShowPlusMinus
            End Get
            Set(value As Boolean)
                Me.TV.ShowPlusMinus = value
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob Linien zwischen den Stammknoten angezeigt werden.
        ''' </summary>
        ''' <remarks>
        ''' Betrifft ausschließlich die Linien zwischen den obersten Knotenebenen (Root-Level).
        ''' </remarks>
        <Category("Behavior")>
        <Description("Gibt an, ob Linien zwischen den Stammknoten angezeigt werden.")>
        <Browsable(True)>
        <DefaultValue(True)>
        Public Property ShowRootLines As Boolean
            Get
                Return Me.TV.ShowRootLines
            End Get
            Set(value As Boolean)
                Me.TV.ShowRootLines = value
            End Set
        End Property

        ''' <summary>
        ''' Ruft den Abstand für das Einrücken der einzelnen Ebenen von untergeordneten Strukturknoten ab oder legt
        ''' diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Bestimmt die horizontale Einrückung pro Ebene in Pixel.
        ''' </remarks>
        <Category("Behavior")>
        <Description("Ruft den Abstand für das Einrücken der einzelnen Ebenen von untergeordneten Strukturknoten ab oder legt diesen fest.")>
        <Browsable(True)>
        <DefaultValue(19)>
        Public Property Indent As Int32
            Get
                Return Me.TV.Indent
            End Get
            Set(value As Int32)
                Me.TV.Indent = value
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Höhe des jeweiligen Strukturknotens im Strukturansicht-Steuerelement ab oder legt diese fest.
        ''' </summary>
        ''' <remarks>
        ''' Die Knotenhöhe wird in Pixel angegeben und beeinflusst die vertikale Dichte der Einträge.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Ruft die Höhe des jeweiligen Strukturknotens im Strukturansicht-Steuerelement ab oder legt diese fest.")>
        <Browsable(True)>
        <DefaultValue(16)>
        Public Property ItemHeight As Int32
            Get
                Return Me.TV.ItemHeight
            End Get
            Set(value As Int32)
                Me.TV.ItemHeight = value
            End Set
        End Property

        ''' <summary>
        ''' Legt die Hintergrundfarbe für das Steuerelement fest oder gibt diese zurück.
        ''' </summary>
        ''' <remarks>
        ''' Die Hintergrundfarbe des UserControls wird übernommen und auf das interne TreeView angewendet, sodass beide
        ''' konsistent erscheinen.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Legt die Hintergrundfarbe für das Steuerelement fest oder gibt diese zurück.")>
        <Browsable(True)>
        Public Overrides Property BackColor As Color
            Get
                Return MyBase.BackColor
            End Get
            Set(value As Color)
                MyBase.BackColor = value
                Me.TV.BackColor = value
            End Set
        End Property

        ''' <summary>
        ''' Legt die Vordergrundfarbe für das Anzeigen von Text fest oder gibt diese zurück.
        ''' </summary>
        ''' <remarks>
        ''' Die Textfarbe des UserControls wird übernommen und auf das interne TreeView angewendet.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Legt die Vordergrundfarbe für das Anzeigen von Text fest oder gibt diese zurück.")>
        <Browsable(True)>
        Public Overrides Property ForeColor As Color
            Get
                Return MyBase.ForeColor
            End Get
            Set(value As Color)
                MyBase.ForeColor = value
                Me.TV.ForeColor = value
            End Set
        End Property

        ''' <summary>
        ''' Legt die Schriftart für den Text im Steuerelement fest oder gibt diese zurück.
        ''' </summary>
        ''' <remarks>
        ''' Die Schriftart wird sowohl auf das UserControl als auch auf das interne TreeView angewendet, um eine
        ''' einheitliche Darstellung sicherzustellen.
        ''' </remarks>
        ''' <value>
        ''' Aktuell verwendete Schriftart.
        ''' </value>
        <Category("Appearance")>
        <Description("Legt die Schriftart für den Text im Steuerelement fest oder gibt diese zurück.")>
        <Browsable(True)>
        Public Overrides Property Font As Font
            Get
                Return MyBase.Font
            End Get
            Set(value As Font)
                MyBase.Font = value
                Me.TV.Font = value
            End Set
        End Property

        ''' <summary>
        ''' Ist für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
            End Set
        End Property

        ''' <summary>
        ''' Ist für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImage As Image
            Get
                Return MyBase.BackgroundImage
            End Get
            Set(value As Image)
                MyBase.BackgroundImage = value
            End Set
        End Property

        ''' <summary>
        ''' Ist für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImageLayout As ImageLayout
            Get
                Return MyBase.BackgroundImageLayout
            End Get
            Set(value As ImageLayout)
                MyBase.BackgroundImageLayout = value
            End Set
        End Property

#End Region

#Region "öffentliche Methoden"

        ''' <summary>
        ''' Konstruktor für das ExplorerTreeView-Steuerelement.
        ''' </summary>
        ''' <remarks>
        '''
        ''' <para>Dieser Konstruktor initialisiert das Steuerelement und lädt die erforderlichen Bilder. </para>
        '''
        ''' <para>Außerdem wird der Wurzelknoten des TreeViews gesetzt, um die Struktur des Steuerelements zu
        ''' definieren.</para>
        ''' </remarks>
        Public Sub New()
            Me.InitializeComponent()
            Me.TV.ShowNodeToolTips = True
            Me.LoadImages()
            Me.SetRootNode()
        End Sub

        ''' <summary>
        ''' Öffnet und selektiert den Knoten zum angegebenen Verzeichnispfad.
        ''' </summary>
        ''' <remarks>
        ''' Funktioniert auch bei noch nicht geladenen Unterknoten.
        ''' </remarks>
        ''' <param name="Path">Vollständiger Pfad der göffnet werden soll.</param>
        ''' <returns>
        ''' <see langword="true"/>, wenn der Knoten gefunden wurde, sonst <see langword="false"/>
        ''' </returns>
        Public Function ExpandPath(Path As String) As Boolean
            If String.IsNullOrWhiteSpace(Path) Then Return False
            Dim lastpath As String = String.Empty
            Dim lastnode As TreeNode = Me.TV.Nodes.Item(0)
            lastnode.Expand()
            Dim foundNode As TreeNode
            For Each pathsegment As String In GetPathSegments(Path)
                lastpath = IO.Path.Combine(lastpath, pathsegment)
                foundNode = FindNodeByPath(lastnode.Nodes, lastpath)
                If Microsoft.VisualBasic.IsNothing(foundNode) Then Return False
                foundNode.Expand()
                lastnode = foundNode
            Next
            Me.TV.SelectedNode = lastnode
            Return True
        End Function

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Teilt den angegebenen Pfad in seine einzelnen Segmente auf und gibt sie als Liste zurück.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <returns></returns>
        Private Shared Function GetPathSegments(Path As String) As List(Of String)
            Dim dirInfo As New DirectoryInfo(Path)
            Dim result As New List(Of String)
            While dirInfo IsNot Nothing AndAlso Not String.IsNullOrEmpty(dirInfo.Name)
                result.Insert(0, dirInfo.Name)
                dirInfo = dirInfo.Parent
            End While
            Return result
        End Function

        ''' <summary>
        ''' Sucht rekursiv nach einem Knoten mit dem angegebenen Suchpfad in der angegebenen Knotenliste.
        ''' </summary>
        ''' <param name="Nodes"></param>
        ''' <param name="SearchPath"></param>
        ''' <returns></returns>
        Private Shared Function FindNodeByPath(Nodes As TreeNodeCollection, SearchPath As String) As TreeNode
            For Each node As TreeNode In Nodes
                If String.Equals(GetDirectoryPath(node), SearchPath, StringComparison.OrdinalIgnoreCase) Then
                    Return node
                End If
                Dim found As TreeNode = FindNodeByPath(node.Nodes, SearchPath)
                If found IsNot Nothing Then
                    Return found
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Gibt den vollständigen Pfad des Verzeichnisses zurück, das dem angegebenen Knoten entspricht.
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Private Shared Function GetDirectoryPath(node As TreeNode) As String
            Select Case True
                Case TypeOf node Is ComputerNode : Return String.Empty
                Case TypeOf node Is DriveNode : Return CType(node, DriveNode).FullPath
                Case TypeOf node Is SpecialFolderNode : Return CType(node, SpecialFolderNode).FullPath
                Case TypeOf node Is FolderNode : Return CType(node, FolderNode).FullPath
                Case Else : Return String.Empty
            End Select
        End Function

        ''' <summary>
        ''' Setzt den Wurzelknoten des TreeViews auf "Dieser Computer" und fügt Platzhalterknoten für spezielle Ordner
        ''' und Laufwerke hinzu.
        ''' </summary>
        Private Sub SetRootNode()
            Me.TV.Nodes.Clear()
            Dim rootnode As New ComputerNode With {.ImageKey = $"Computer", .SelectedImageKey = $"Computer"}
            Dim unused = Me.TV.Nodes.Add(rootnode)
            Me.TV.Nodes.Item(0).Expand()
        End Sub

        ''' <summary>
        ''' Lädt die erforderlichen Bilder in die ImageList des TreeViews, um die verschiedenen Knotenarten
        ''' darzustellen.
        ''' </summary>
        Private Sub LoadImages()
            Me.TV.ImageList.Images.Clear()
            Me.TV.ImageList.Images.Add(ICON_COMPUTER, My.Resources.ExplorerTreeViewControl_Computer)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_SYSTEM, My.Resources.ExplorerTreeViewControl_DriveSystem)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_FIXED, My.Resources.ExplorerTreeViewControl_DriveFixed)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_CDROM, My.Resources.ExplorerTreeViewControl_DriveCDRom)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_REMOVABLE, My.Resources.ExplorerTreeViewControl_DriveRemovable)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_NETWORK, My.Resources.ExplorerTreeViewControl_DriveNetwork)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_RAM, My.Resources.ExplorerTreeViewControl_DriveRamDisk)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_FLOPPY, My.Resources.ExplorerTreeViewControl_DriveDisk)
            Me.TV.ImageList.Images.Add(ICON_DRIVE_UNKNOWN, My.Resources.ExplorerTreeViewControl_DriveUnknown)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_FOLDER, My.Resources.ExplorerTreeViewControl_Folder)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_DESKTOP, My.Resources.ExplorerTreeViewControl_FolderDesktop)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_DOCUMENTS, My.Resources.ExplorerTreeViewControl_FolderDocuments)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_DOWNLOADS, My.Resources.ExplorerTreeViewControl_FolderDownloads)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_MUSIC, My.Resources.ExplorerTreeViewControl_FolderMusic)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_PICTURES, My.Resources.ExplorerTreeViewControl_FolderPictures)
            Me.TV.ImageList.Images.Add(ICON_FOLDER_VIDEOS, My.Resources.ExplorerTreeViewControl_FolderVideos)
        End Sub

        ''' <summary>
        ''' Lädt die Unterordner des angegebenen Knotens und aktualisiert die Anzeige der Knoten entsprechend.
        ''' </summary>
        ''' <param name="Node"></param>
        Private Sub LoadSubfolders(Node As TreeNode)
            Node.Nodes.Clear()
            Select Case True
                Case TypeOf Node Is ComputerNode
                    CType(Node, ComputerNode).LoadSpecialFolders()
                    CType(Node, ComputerNode).LoadDrives()
                Case TypeOf Node Is SpecialFolderNode
                    CType(Node, SpecialFolderNode).LoadSubfolders()
                Case TypeOf Node Is DriveNode
                    CType(Node, DriveNode).LoadSubfolders()
                Case TypeOf Node Is FolderNode
                    CType(Node, FolderNode).LoadSubfolders()
            End Select

            For Each child As TreeNode In Node.Nodes
                Me.UpdateNodeAccessVisual(child)
            Next
        End Sub

        ''' <summary>
        ''' Erstellt einen FileSystemWatcher für das angegebene Verzeichnis, um Änderungen im Dateisystem zu überwachen.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        Private Sub CreateFileSystemWatcher(FolderPath As String)
            If String.IsNullOrEmpty(FolderPath) OrElse Not Directory.Exists(FolderPath) Then Return
            If Me._FileSystemWatchers.ContainsKey(FolderPath) Then Return
            Try
                Dim FSW As New FileSystemWatcher(FolderPath) With {.NotifyFilter = NotifyFilters.DirectoryName, .IncludeSubdirectories = False}
                AddHandler FSW.Created, AddressOf Me.FSW_DirectoryChanged
                AddHandler FSW.Deleted, AddressOf Me.FSW_DirectoryChanged
                AddHandler FSW.Renamed, AddressOf Me.FSW_DirectoryChanged
                Me._FileSystemWatchers.Add(FolderPath, FSW)
                FSW.EnableRaisingEvents = True
            Catch ex As Exception
                Debug.WriteLine($"Fehler beim Erstellen des FileSystemWatchers: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Entfernt und entsorgt alle FileSystemWatcher, die mit dem angegebenen Verzeichnis oder seinen
        ''' Unterverzeichnissen verknüpft sind.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        Private Sub RemoveFileSystemWatchers(FolderPath As String)
            Dim toRemove As New List(Of String)
            Me.FindWatchersToRemove(FolderPath, toRemove)
            Me.RemoveAndDisposeWatchers(toRemove)
        End Sub

        ''' <summary>
        ''' Sucht alle FileSystemWatcher, die mit dem angegebenen Verzeichnis oder seinen Unterverzeichnissen verknüpft
        ''' sind, und fügt deren Pfade der Liste "toRemove" hinzu.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        ''' <param name="toRemove"></param>
        Private Sub FindWatchersToRemove(FolderPath As String, toRemove As List(Of String))
            For Each watcherPath In Me._FileSystemWatchers.Keys
                If watcherPath.Equals(FolderPath, StringComparison.OrdinalIgnoreCase) OrElse
                       watcherPath.StartsWith(FolderPath & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) Then
                    toRemove.Add(watcherPath)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Entfernt und entsorgt die FileSystemWatcher, deren Pfade in der Liste "toRemove" enthalten sind.
        ''' </summary>
        ''' <param name="toRemove"></param>
        Private Sub RemoveAndDisposeWatchers(toRemove As List(Of String))
            For Each watcherPath In toRemove
                Dim watcher = Me._FileSystemWatchers(watcherPath)
                watcher.EnableRaisingEvents = False
                Me.RemoveWatcherHandlers(watcher)
                watcher.Dispose()
                Dim unused = Me._FileSystemWatchers.Remove(watcherPath)
            Next
        End Sub

        ''' <summary>
        ''' Entfernt die Ereignishandler für den angegebenen FileSystemWatcher, um Speicherlecks zu vermeiden.
        ''' </summary>
        ''' <param name="watcher"></param>
        Private Sub RemoveWatcherHandlers(watcher As FileSystemWatcher)
            RemoveHandler watcher.Created, AddressOf Me.FSW_DirectoryChanged
            RemoveHandler watcher.Deleted, AddressOf Me.FSW_DirectoryChanged
            RemoveHandler watcher.Renamed, AddressOf Me.FSW_DirectoryChanged
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, wenn sich ein Verzeichnis ändert (erstellt, gelöscht oder umbenannt). Aktualisiert die
        ''' Unterordner des entsprechenden Knotens im TreeView.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub FSW_DirectoryChanged(sender As Object, e As FileSystemEventArgs)
            If Me.TV.InvokeRequired Then
                Dim unused = Me.TV.Invoke(New MethodInvoker(Sub() Me.FSW_DirectoryChanged(sender, e)))
                Return
            End If
            Dim node As TreeNode = FindNodeByPath(Me.TV.Nodes, CType(sender, FileSystemWatcher).Path)
            Select Case True
                Case TypeOf node Is DriveNode : Me.LoadSubfolders(node)
                Case TypeOf node Is SpecialFolderNode : Me.LoadSubfolders(node)
                Case TypeOf node Is FolderNode : Me.LoadSubfolders(node)
            End Select
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, bevor ein Knoten im TreeView erweitert wird. Lädt die Unterordner des Knotens, wenn es sich
        ''' um einen ComputerNode handelt, oder überprüft den Zugriff auf das Verzeichnis und lädt die Unterordner, wenn
        ''' der Zugriff erlaubt ist.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TV_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles TV.BeforeExpand
            If TypeOf e.Node Is ComputerNode Then
                Me.LoadSubfolders(e.Node)
                Return
            End If

            Dim path As String = GetDirectoryPath(e.Node)
            If Not CanOpenFolder(path) Then
                e.Cancel = True ' anzeigen ja, öffnen nein
                Return
            End If

            Me.LoadSubfolders(e.Node)
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, nachdem ein Knoten im TreeView erweitert wurde. Erstellt einen FileSystemWatcher für das
        ''' geöffnete Verzeichnis, um Änderungen im Dateisystem zu überwachen.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TV_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles TV.AfterExpand
            Me.CreateFileSystemWatcher(GetDirectoryPath(e.Node)) ' Einen FileSystemWatcher für das geöffnete Verzeichnis erstellen
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, nachdem ein Knoten im TreeView zusammengeklappt wurde. Entfernt den FileSystemWatcher für
        ''' das geschlossene Verzeichnis, um Ressourcen freizugeben.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TV_AfterCollapse(sender As Object, e As TreeViewEventArgs) Handles TV.AfterCollapse
            Me.RemoveFileSystemWatchers(GetDirectoryPath(e.Node))
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, nachdem ein Knoten im TreeView ausgewählt wurde. Aktualisiert den ausgewählten Pfad und
        ''' löst das Ereignis SelectedPathChanged aus, wenn der ausgewählte Knoten einen gültigen Pfad hat.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TV_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TV.AfterSelect
            Dim selectedpath As String = GetDirectoryPath(e.Node)
            If String.IsNullOrEmpty(selectedpath) Then Exit Sub
            If Not CanOpenFolder(selectedpath) Then Exit Sub ' kein Event für geschützte Ordner

            Me._SelectedPath = selectedpath
            RaiseEvent SelectedPathChanged(Me, New SelectedPathChangedEventArgs(selectedpath))
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, wenn ein neues Laufwerk hinzugefügt wird. Fügt einen neuen DriveNode für das hinzugefügte
        ''' Laufwerk in die TreeView-Struktur ein.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub DW_DriveAdded(sender As Object, e As DriveWatcherControl.DriveAddedEventArgs) Handles DW.DriveAdded
            Dim newNode As New DriveNode(New DriveInfo(e.DriveName)) With {.Tag = e.DriveName}
            Dim inserted As Boolean = False
            For i As Int32 = 0 To Me.TV.Nodes.Item(0).Nodes.Count - 1
                Dim currNode As TreeNode = Me.TV.Nodes.Item(0).Nodes(i)
                If TypeOf currNode Is DriveNode AndAlso
                        String.Compare(
                        currNode.Tag.ToString,
                        newNode.Tag.ToString,
                        StringComparison.OrdinalIgnoreCase) > 0 Then
                    Me.TV.Nodes.Item(0).Nodes.Insert(i, newNode)
                    inserted = True
                    Exit For
                End If
            Next
            If Not inserted Then
                Dim unused = Me.TV.Nodes.Item(0).Nodes.Add(newNode)
            End If
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, wenn ein Laufwerk entfernt wird. Entfernt den entsprechenden DriveNode aus der
        ''' TreeView-Struktur.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub DW_DriveRemoved(sender As Object, e As DriveWatcherControl.DriveRemovedEventArgs) Handles DW.DriveRemoved
            For Each node As TreeNode In Me.TV.Nodes.Item(0).Nodes
                If TypeOf node Is DriveNode Then
                    If CType(node, DriveNode).Tag.ToString() = e.DriveName Then
                        CType(node, DriveNode).Remove()
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Überprüft, ob der angegebene Ordnerpfad geöffnet werden kann, indem versucht wird, auf die
        ''' Dateisystemeinträge zuzugreifen.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Private Shared Function CanOpenFolder(path As String) As Boolean
            If String.IsNullOrWhiteSpace(path) Then Return False

            Try
                Using it = Directory.EnumerateFileSystemEntries(path).GetEnumerator()
                    Dim unused = it.MoveNext() ' Zugriffstest (auch bei leerem Ordner gültig)
                End Using
                Return True
            Catch ex As UnauthorizedAccessException
                Return False
            Catch ex As IOException
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Aktualisiert die visuelle Darstellung des Knotens basierend auf dem Zugriff auf das zugehörige Verzeichnis.
        ''' </summary>
        ''' <param name="node"></param>
        Private Sub UpdateNodeAccessVisual(node As TreeNode)
            Dim path As String = GetDirectoryPath(node)
            If String.IsNullOrWhiteSpace(path) Then Exit Sub

            If CanOpenFolder(path) Then
                node.ForeColor = Me.TV.ForeColor
                node.ToolTipText = String.Empty
            Else
                node.ForeColor = Drawing.Color.Gray
                node.ToolTipText = "Kein Zugriff auf diesen Ordner"
            End If
        End Sub

#End Region

    End Class

End Namespace

