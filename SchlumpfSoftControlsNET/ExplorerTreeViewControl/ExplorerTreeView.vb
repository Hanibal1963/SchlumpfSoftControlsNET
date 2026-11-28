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
Imports System.Threading.Tasks

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
    Public Class ExplorerTreeView

        Inherits UserControl

        Implements IDisposable

#Region "Variablen"

        Private disposedValue As Boolean

        ''' <summary>Speichert die unveränderten Quellbilder für alle Instanzen dieser Anwendung.</summary>
        Private Shared ReadOnly _imageCache As New Lazy(Of List(Of KeyValuePair(Of String, Bitmap)))(
            Function() New Dictionary(Of String, Bitmap) From {
                {ICON_COMPUTER, My.Resources.ExplorerTreeViewControl_Computer},
                {ICON_DRIVE_SYSTEM, My.Resources.ExplorerTreeViewControl_DriveSystem},
                {ICON_DRIVE_FIXED, My.Resources.ExplorerTreeViewControl_DriveFixed},
                {ICON_DRIVE_CDROM, My.Resources.ExplorerTreeViewControl_DriveCDRom},
                {ICON_DRIVE_REMOVABLE, My.Resources.ExplorerTreeViewControl_DriveRemovable},
                {ICON_DRIVE_NETWORK, My.Resources.ExplorerTreeViewControl_DriveNetwork},
                {ICON_DRIVE_RAM, My.Resources.ExplorerTreeViewControl_DriveRamDisk},
                {ICON_DRIVE_FLOPPY, My.Resources.ExplorerTreeViewControl_DriveDisk},
                {ICON_DRIVE_NOROOT, My.Resources.ExplorerTreeViewControl_DriveNoRoot},
                {ICON_DRIVE_UNKNOWN, My.Resources.ExplorerTreeViewControl_DriveUnknown},
                {ICON_FOLDER_FOLDER, My.Resources.ExplorerTreeViewControl_Folder},
                {ICON_FOLDER_DESKTOP, My.Resources.ExplorerTreeViewControl_FolderDesktop},
                {ICON_FOLDER_DOCUMENTS, My.Resources.ExplorerTreeViewControl_FolderDocuments},
                {ICON_FOLDER_DOWNLOADS, My.Resources.ExplorerTreeViewControl_FolderDownloads},
                {ICON_FOLDER_MUSIC, My.Resources.ExplorerTreeViewControl_FolderMusic},
                {ICON_FOLDER_PICTURES, My.Resources.ExplorerTreeViewControl_FolderPictures},
                {ICON_FOLDER_VIDEOS, My.Resources.ExplorerTreeViewControl_FolderVideos}
            }.OrderBy(Function(entry) If(entry.Key = ICON_COMPUTER, 0, 1)).ToList())

        ''' <summary>Hält Bildkopien bis zur Entsorgung der zugehörigen ImageList verfügbar.</summary>
        Private ReadOnly _ownedImages As New List(Of Bitmap)

        Private ReadOnly _FileSystemWatchers As New Dictionary(Of String, FileSystemWatcher)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Schützt Überwachungen und ihre noch laufende Einrichtung vor konkurrierendem Entsorgen.</summary>
        Private ReadOnly _watcherLock As New Object()

        ''' <summary>Enthält die Generationen der noch nicht fertig eingerichteten Überwachungen.</summary>
        Private ReadOnly _pendingWatchers As New Dictionary(Of String, Int64)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Bündelt Änderungen je überwachtem Verzeichnis bis zum Ablauf der Ruhezeit.</summary>
        Private ReadOnly _pendingRefreshes As New Dictionary(Of String, RefreshRequest)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Wartezeit nach der letzten Verzeichnisänderung in Millisekunden.</summary>
        Private Const RefreshDelayMilliseconds As Int32 = 200

        ''' <summary>Identifiziert einen entprellten Auftrag auch nach einer erneuten Änderung.</summary>
        Private NotInheritable Class RefreshRequest
            ''' <summary>Der Timer wird beim Ersetzen oder Entfernen des Auftrags entsorgt.</summary>
            Friend Property Timer As System.Threading.Timer
        End Class

        ''' <summary>Erzeugt eindeutige Generationen für die Einrichtung einer Überwachung.</summary>
        Private _watcherGeneration As Int64

        ''' <summary>Verhindert nach dem Entsorgen die Registrierung weiterer Überwachungen.</summary>
        Private _watchersStopped As Boolean

        Private _SelectedPath As String

        ''' <summary>Verwirft Ergebnisse älterer Auswahlprüfungen nach einer erneuten Auswahl.</summary>
        Private _selectionGeneration As Int64

        ''' <summary>Merkt eine noch nicht bestätigte Auswahl für die synchrone Pfadnavigation.</summary>
        Private _pendingSelection As TreeNode

        ''' <summary>Ordnet laufenden Verzeichnisabfragen ihre aktuelle Generation zu.</summary>
        Private ReadOnly _pendingLoads As New Dictionary(Of DirectoryNode, Int64)

        ''' <summary>Erzeugt eindeutige Generationen für Verzeichnisabfragen.</summary>
        Private _loadGeneration As Int64

        ''' <summary>Kennzeichnet die weiterhin synchrone öffentliche Pfadnavigation.</summary>
        Private _synchronousNavigation As Boolean

        ''' <summary>Verwirft überholte Ergebnisse der Wurzelauflistung.</summary>
        Private _rootGeneration As Int64

        ''' <summary>Gibt an, ob die aktuelle Wurzelauflistung noch läuft.</summary>
        Private _rootLoading As Boolean

        ''' <summary>Gibt an, ob die Wurzeleinträge erfolgreich übernommen wurden.</summary>
        Private _rootLoaded As Boolean

        ''' <summary>Ordnet ausstehenden Laufwerksabfragen ihre aktuelle Generation zu.</summary>
        Private ReadOnly _pendingDriveAdds As New Dictionary(Of String, Int64)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Erzeugt eindeutige Generationen für die Abfrage neu hinzugefügter Laufwerke.</summary>
        Private _driveAddGeneration As Int64

        ''' <summary>Enthält den Abbruchzustand der aktuellen asynchronen Navigation.</summary>
        Private _activeNavigation As NavigationState

        ''' <summary>Unterscheidet die eigene Knotenauswahl von einer Benutzeraktion.</summary>
        Private _navigationSelecting As Boolean

        ''' <summary>Unterdrückt Abbrüche durch automatische Knotenänderungen beim Anwenden eines Ergebnisses.</summary>
        Private _navigationMutation As Boolean

        ''' <summary>Stellt ein Signal zum Abbruch einer asynchronen Navigation bereit.</summary>
        Private NotInheritable Class NavigationState

            ''' <summary>Wird beim Abbruch gesetzt und beendet wartende Navigationsschritte.</summary>
            Friend ReadOnly Stopped As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)

        End Class

        ''' <summary>Beschreibt einen auf dem UI-Thread vorbereiteten Navigationsschritt.</summary>
        Private NotInheritable Class NavigationStep

            ''' <summary>Gibt den zu ladenden Knoten an.</summary>
            Friend Property Node As DirectoryNode

            ''' <summary>Gibt den Verzeichnispfad für den Hintergrundzugriff an.</summary>
            Friend Property Path As String

            ''' <summary>Gibt die beim Start beobachtete Ladegeneration an.</summary>
            Friend Property Generation As Int64?

            ''' <summary>Gibt an, ob eine Verzeichnisabfrage benötigt wird.</summary>
            Friend Property NeedsRead As Boolean

        End Class

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
        ''' <para>Bei interaktiver Auswahl erfolgt die Zugriffsprüfung im Hintergrund; das Ereignis wird danach auf
        ''' dem UI-Thread ausgelöst, sofern der Knoten weiterhin ausgewählt ist.</para>
        ''' </remarks>
        <Description("Wird ausgelöst, wenn sich der ausgewählte Pfad geändert hat.")>
        <Browsable(True)>
        Public Event SelectedPathChanged(sender As Object, e As SelectedPathChangedEventArgs)

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt den zuletzt erfolgreich auf Zugriff geprüften ausgewählten Pfad zurück.
        ''' </summary>
        ''' <remarks>Während einer asynchronen Prüfung bleibt der zuletzt bestätigte Wert erhalten.</remarks>
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
        ''' <para>Die Spezialordner und Laufwerke werden nach der Erstellung des TreeView-Handles im Hintergrund
        ''' ermittelt. Ein Aufruf von <see cref="ExpandPath(String)"/> kann die Ermittlung synchron vorziehen.</para>
        ''' </remarks>
        Public Sub New()
            Me.InitializeComponent()
            Me.TV.ShowNodeToolTips = True
            Me.LoadImages()
            Me.SetRootNode()
        End Sub

        ''' <summary>Stellt Überwachungen für weiterhin geöffnete alternative Darstellungen wieder her.</summary>
        ''' <param name="nodes">Zu durchsuchende sichtbare Knoten.</param>
        ''' <param name="folderPath">Pfad des geschlossenen Teilbaums.</param>
        ''' <param name="collapsedNode">Gerade geschlossener Knoten, der nicht wieder überwacht werden soll.</param>
        Private Sub RestoreVisibleWatchers(nodes As TreeNodeCollection, folderPath As String, collapsedNode As TreeNode)
            For Each node As TreeNode In nodes
                If node IsNot collapsedNode AndAlso node.IsExpanded Then
                    Dim path As String = GetDirectoryPath(node)
                    If Not String.IsNullOrEmpty(path) AndAlso IsPathWithin(path, folderPath) Then Me.CreateFileSystemWatcher(path)
                    Me.RestoreVisibleWatchers(node.Nodes, folderPath, collapsedNode)
                End If
            Next
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
            Me.CancelActiveNavigation()
            If String.IsNullOrWhiteSpace(Path) Then Return False
            Dim previousSynchronousNavigation As Boolean = Me._synchronousNavigation
            Me._synchronousNavigation = True
            Try
                Dim lastpath As String = String.Empty
                Dim lastnode As TreeNode = Me.TV.Nodes.Item(0)
                lastnode.Expand()
                If Not Me._rootLoaded Then Me.LoadRootSynchronously(DirectCast(lastnode, ComputerNode))
                Dim foundNode As TreeNode
                For Each pathsegment As String In GetPathSegments(Path)
                    lastpath = IO.Path.Combine(lastpath, pathsegment)
                    foundNode = FindNodeByPath(lastnode.Nodes, lastpath)
                    If Microsoft.VisualBasic.IsNothing(foundNode) Then Return False
                    Dim directoryNode As DirectoryNode = TryCast(foundNode, DirectoryNode)
                    If directoryNode IsNot Nothing AndAlso foundNode.IsExpanded AndAlso Not directoryNode.ChildrenLoaded Then
                        Dim removed = Me._pendingLoads.Remove(directoryNode)
                        If Not directoryNode.LoadChildren() Then
                            Me.UpdateNodeAccessVisual(directoryNode)
                            Return False
                        End If
                    End If
                    foundNode.Expand()
                    lastnode = foundNode
                Next
                If Me.TV.SelectedNode Is lastnode Then
                    If Me._pendingSelection Is lastnode Then
                        Me.TV_AfterSelect(Me.TV, New TreeViewEventArgs(lastnode))
                    Else
                        Me.ApplyAccessStatus(lastnode, CheckFolderAccess(GetDirectoryPath(lastnode)))
                    End If
                Else
                    Me.TV.SelectedNode = lastnode
                End If
                Dim selectedDirectory As DirectoryNode = TryCast(lastnode, DirectoryNode)
                Return selectedDirectory IsNot Nothing AndAlso selectedDirectory.Status = DirectoryNode.DirectoryStatus.Accessible AndAlso
                    String.Equals(Me._SelectedPath, GetDirectoryPath(lastnode), StringComparison.OrdinalIgnoreCase)
            Catch ex As PathTooLongException
                Debug.WriteLine($"Verzeichnispfad '{Path}' ist zu lang: {ex.Message}")
                Return False
            Finally
                Me._synchronousNavigation = previousSynchronousNavigation
            End Try
        End Function

        ''' <summary>Öffnet und selektiert einen Verzeichnispfad ohne blockierende Dateisystemabfragen auf dem UI-Thread.</summary>
        ''' <param name="path">Vollständiger Pfad des auszuwählenden Verzeichnisses.</param>
        ''' <returns>Eine Aufgabe mit True bei erfolgreicher Auswahl; False bei ungültigem, nicht verfügbarem oder überholtem Pfad.</returns>
        ''' <exception cref="InvalidOperationException">Der Aufruf erfolgt nicht auf dem UI-Thread oder das TreeView besitzt kein Handle.</exception>
        ''' <remarks>Ein neuer Navigationsauftrag, eine Benutzerauswahl, ein Zuklappen oder die Zerstörung des Handles bricht den laufenden Auftrag ab.</remarks>
        Public Async Function ExpandPathAsync(path As String) As Task(Of Boolean)
            If Not Me.TV.IsHandleCreated OrElse Me.TV.InvokeRequired Then
                Throw New InvalidOperationException("ExpandPathAsync muss auf dem UI-Thread mit erstelltem TreeView-Handle aufgerufen werden.")
            End If
            Me.CancelActiveNavigation()
            If String.IsNullOrWhiteSpace(path) Then Return False
            Dim segments As List(Of String)
            Try
                segments = GetPathSegments(path)
            Catch ex As PathTooLongException
                Debug.WriteLine($"Verzeichnispfad '{path}' ist zu lang: {ex.Message}")
                Return False
            End Try
            Dim state As New NavigationState()
            Me._activeNavigation = state
            Try
                Dim root As ComputerNode = DirectCast(Me.TV.Nodes(0), ComputerNode)
                Me.ExpandForNavigation(root)
                Dim rootGeneration As Int64 = Me._rootGeneration
                If Not Me._rootLoaded Then
                    Dim rootRead = Task.Run(Function() ComputerNode.ReadEntries())
                    Dim entries = Await AwaitNavigation(rootRead, state).ConfigureAwait(False)
                    Dim ready = Await AwaitNavigation(Me.PostNavigation(state,
                        Function()
                            If root.TreeView IsNot Me.TV OrElse Not root.IsExpanded OrElse rootGeneration <> Me._rootGeneration Then Return False
                            If Not Me._rootLoaded Then
                                Me._rootGeneration += 1
                                Me._rootLoading = False
                                root.ApplyEntries(entries)
                                Me.MarkUnavailableDrives(root, entries)
                                root.ToolTipText = String.Empty
                                Me._rootLoaded = True
                            End If
                            Return True
                        End Function), state).ConfigureAwait(False)
                    If Not ready Then Return False
                End If

                Dim parent As TreeNode = root
                Dim currentPath As String = String.Empty
                For Each segment As String In segments
                    currentPath = IO.Path.Combine(currentPath, segment)
                    Dim searchPath As String = currentPath
                    Dim navStep = Await AwaitNavigation(Me.PostNavigation(state,
                        Function() Me.PrepareNavigationStep(parent, searchPath)), state).ConfigureAwait(False)
                    If navStep Is Nothing Then Return False
                    While navStep.NeedsRead
                        Dim readTask = Task.Run(Function() DirectoryNode.ReadChildren(navStep.Path))
                        Dim result = Await AwaitNavigation(readTask, state).ConfigureAwait(False)
                        Dim outcome = Await AwaitNavigation(Me.PostNavigation(state,
                            Function() Me.ApplyNavigationStep(navStep, result)), state).ConfigureAwait(False)
                        If outcome = 0 Then Return False
                        If outcome = 1 Then Exit While
                        navStep = Await AwaitNavigation(Me.PostNavigation(state,
                            Function() Me.PrepareNavigationStep(parent, searchPath)), state).ConfigureAwait(False)
                        If navStep Is Nothing Then Return False
                    End While
                    parent = navStep.Node
                Next

                Dim target As TreeNode = parent
                Dim selectionPath As String = currentPath
                If String.IsNullOrEmpty(selectionPath) Then Return False
                Dim accessTask = Task.Run(Function() CheckFolderAccess(selectionPath))
                Dim accessStatus = Await AwaitNavigation(accessTask, state).ConfigureAwait(False)
                Return Await AwaitNavigation(Me.PostNavigation(state,
                    Function()
                        If target.TreeView IsNot Me.TV Then Return False
                        Me.ApplyAccessStatus(target, accessStatus)
                        If accessStatus <> DirectoryNode.DirectoryStatus.Accessible Then Return False
                        Dim changed As Boolean = Me.TV.SelectedNode IsNot target OrElse Me._pendingSelection Is target OrElse
                            Not String.Equals(Me._SelectedPath, selectionPath, StringComparison.OrdinalIgnoreCase)
                        Me._selectionGeneration += 1
                        Me._pendingSelection = Nothing
                        Me._navigationSelecting = True
                        Try
                            Me.TV.SelectedNode = target
                        Finally
                            Me._navigationSelecting = False
                        End Try
                        If Me.TV.SelectedNode IsNot target Then Return False
                        If changed Then Me.PublishSelectedPath(selectionPath)
                        Return True
                    End Function), state).ConfigureAwait(False)
            Catch ex As OperationCanceledException
                Return False
            Finally
                Me.QueueNavigationCleanup(state)
            End Try
        End Function

#End Region

#Region "Interne Methoden"

        ''' <summary>Bricht die aktive Navigation ab, ohne auf laufende Dateisystemaufrufe zu warten.</summary>
        Private Sub CancelActiveNavigation()
            Dim state As NavigationState = Me._activeNavigation
            Me._activeNavigation = Nothing
            If state IsNot Nothing Then
                Dim unused = state.Stopped.TrySetResult(True)
            End If
        End Sub

        ''' <summary>Wartet auf eine Aufgabe oder den Abbruch der zugehörigen Navigation.</summary>
        ''' <typeparam name="T">Ergebnistyp der Aufgabe.</typeparam>
        ''' <param name="operation">Auszuführende Hintergrund- oder UI-Aufgabe.</param>
        ''' <param name="state">Abbruchzustand der Navigation.</param>
        ''' <returns>Das Ergebnis der abgeschlossenen Aufgabe.</returns>
        Private Shared Async Function AwaitNavigation(Of T)(operation As Task(Of T), state As NavigationState) As Task(Of T)
            Dim observed = operation.ContinueWith(
                Sub(failed As Task(Of T)) Debug.WriteLine($"Asynchroner Navigationsschritt fehlgeschlagen: {failed.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted)
            Dim completed = Await Task.WhenAny(operation, state.Stopped.Task).ConfigureAwait(False)
            If completed IsNot operation OrElse state.Stopped.Task.IsCompleted Then Throw New OperationCanceledException()
            Return Await operation.ConfigureAwait(False)
        End Function

        ''' <summary>Stellt einen Navigationsschritt auf dem UI-Thread zu und übergibt dessen Ergebnis als Aufgabe.</summary>
        ''' <typeparam name="T">Ergebnistyp des UI-Schritts.</typeparam>
        ''' <param name="state">Abbruchzustand der Navigation.</param>
        ''' <param name="action">Auf dem UI-Thread auszuführender Schritt.</param>
        ''' <returns>Eine Aufgabe mit dem Ergebnis oder einer Zustellungs- beziehungsweise Ausführungsausnahme.</returns>
        Private Function PostNavigation(Of T)(state As NavigationState, action As Func(Of T)) As Task(Of T)
            Dim completion As New TaskCompletionSource(Of T)(TaskCreationOptions.RunContinuationsAsynchronously)
            Try
                Dim unused = Me.TV.BeginInvoke(New MethodInvoker(
                    Sub()
                        If state.Stopped.Task.IsCompleted OrElse Me.IsDisposed OrElse Me.Disposing Then
                            Dim cancelled = completion.TrySetCanceled()
                            Return
                        End If
                        Try
                            Dim completed = completion.TrySetResult(action())
                        Catch ex As Exception
                            Dim failed = completion.TrySetException(ex)
                        End Try
                    End Sub))
            Catch ex As InvalidOperationException
                Dim failed = completion.TrySetException(ex)
            End Try
            Return completion.Task
        End Function

        ''' <summary>Entfernt einen abgeschlossenen Navigationsauftrag auf dem UI-Thread.</summary>
        ''' <param name="state">Abgeschlossener Navigationsauftrag.</param>
        Private Sub QueueNavigationCleanup(state As NavigationState)
            If state.Stopped.Task.IsCompleted Then Return
            Try
                Dim unused = Me.TV.BeginInvoke(New MethodInvoker(
                    Sub()
                        If Me._activeNavigation Is state Then Me._activeNavigation = Nothing
                    End Sub))
            Catch ex As InvalidOperationException
                Debug.WriteLine($"Navigationszustand konnte nicht bereinigt werden: {ex.Message}")
            End Try
        End Sub

        ''' <summary>Ermittelt und öffnet einen Pfadabschnitt auf dem UI-Thread.</summary>
        ''' <param name="parent">Übergeordneter Baumknoten.</param>
        ''' <param name="path">Vollständiger Pfad des gesuchten Abschnitts.</param>
        ''' <returns>Ein vorbereiteter Schritt oder Nothing, wenn der Knoten nicht verfügbar ist.</returns>
        Private Function PrepareNavigationStep(parent As TreeNode, path As String) As NavigationStep
            If parent.TreeView IsNot Me.TV OrElse Not parent.IsExpanded Then Return Nothing
            Dim node As DirectoryNode = TryCast(FindNodeByPath(parent.Nodes, path), DirectoryNode)
            If node Is Nothing Then Return Nothing
            Me.ExpandForNavigation(node)
            Dim generation As Int64
            Dim pending As Int64? = Nothing
            If Me._pendingLoads.TryGetValue(node, generation) Then pending = generation
            Return New NavigationStep With {.Node = node, .Path = node.DirectoryPath, .Generation = pending, .NeedsRead = Not node.ChildrenLoaded OrElse pending.HasValue}
        End Function

        ''' <summary>Öffnet einen Knoten ohne den synchronen Ladepfad einer verschachtelten Navigation zu übernehmen.</summary>
        ''' <param name="node">Zu öffnender Knoten.</param>
        Private Sub ExpandForNavigation(node As TreeNode)
            Dim previousSynchronousNavigation As Boolean = Me._synchronousNavigation
            Me._synchronousNavigation = False
            Try
                node.Expand()
            Finally
                Me._synchronousNavigation = previousSynchronousNavigation
            End Try
        End Sub

        ''' <summary>Übernimmt ein Abfrageergebnis, sofern keine aktuellere Verzeichnisänderung vorliegt.</summary>
        ''' <param name="navStep">Vorbereiteter Navigationsschritt.</param>
        ''' <param name="result">Im Hintergrund ermitteltes Verzeichnisergebnis.</param>
        ''' <returns>0 bei Fehlschlag, 1 bei Erfolg und 2, wenn wegen eines neueren Auftrags erneut gelesen werden muss.</returns>
        Private Function ApplyNavigationStep(navStep As NavigationStep, result As DirectoryNode.LoadResult) As Int32
            Dim node As DirectoryNode = navStep.Node
            If node.TreeView IsNot Me.TV Then Return 0
            Dim ancestor As TreeNode = node.Parent
            While ancestor IsNot Nothing
                If Not ancestor.IsExpanded Then Return 0
                ancestor = ancestor.Parent
            End While
            Dim current As Int64
            Dim hasCurrent As Boolean = Me._pendingLoads.TryGetValue(node, current)
            If Not hasCurrent AndAlso node.ChildrenLoaded Then Return 1
            If hasCurrent <> navStep.Generation.HasValue OrElse (hasCurrent AndAlso current <> navStep.Generation.GetValueOrDefault()) Then Return 2
            Dim removed = Me._pendingLoads.Remove(node)
            Me.TV.BeginUpdate()
            Me._navigationMutation = True
            Try
                If result.Status = DirectoryNode.DirectoryStatus.Accessible Then Me.RemoveMissingChildWatchers(node, result.Directories)
                Dim loaded As Boolean = node.ApplyLoadResult(result)
                Me.UpdateNodeAccessVisual(node)
                Return If(loaded, 1, 0)
            Finally
                Me._navigationMutation = False
                Me.TV.EndUpdate()
            End Try
        End Function

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
            For Each image As Bitmap In Me._ownedImages
                image.Dispose()
            Next
            Me._ownedImages.Clear()
            ' GDI+-Bilder dürfen nicht gleichzeitig von mehreren UI-Threads kopiert werden.
            SyncLock _imageCache
                For Each entry In _imageCache.Value
                    Dim image As New Bitmap(entry.Value)
                    Me._ownedImages.Add(image)
                    Me.TV.ImageList.Images.Add(entry.Key, image)
                Next
            End SyncLock
        End Sub

        ''' <summary>
        ''' Lädt die Unterordner des angegebenen Knotens und aktualisiert die Anzeige der Knoten entsprechend.
        ''' </summary>
        ''' <param name="Node"></param>
        Private Sub LoadSubfolders(Node As TreeNode)
            If TypeOf Node Is ComputerNode Then
                If Me._synchronousNavigation Then
                    Me.LoadRootSynchronously(DirectCast(Node, ComputerNode))
                Else
                    Me.LoadRootAsync(DirectCast(Node, ComputerNode))
                End If
            ElseIf TypeOf Node Is DirectoryNode Then
                Dim directoryNode As DirectoryNode = CType(Node, DirectoryNode)
                Me.LoadDirectoryAsync(directoryNode)
            End If
        End Sub

        ''' <summary>Lädt die Wurzel für die bestehende synchrone Pfadnavigation und verwirft ältere Aufträge.</summary>
        ''' <param name="node">Wurzelknoten des Steuerelements.</param>
        Private Sub LoadRootSynchronously(node As ComputerNode)
            If Me._rootLoaded Then Return
            Me._rootGeneration += 1
            Me._rootLoading = False
            Dim entries As List(Of ComputerNode.RootEntry) = ComputerNode.ReadEntries()
            node.ApplyEntries(entries)
            Me.MarkUnavailableDrives(node, entries)
            node.ToolTipText = String.Empty
            Me._rootLoaded = True
        End Sub

        ''' <summary>Ermittelt Wurzeleinträge im Hintergrund und übernimmt nur das aktuelle Ergebnis.</summary>
        ''' <param name="node">Wurzelknoten des Steuerelements.</param>
        Private Sub LoadRootAsync(node As ComputerNode)
            If Me._rootLoaded OrElse Me._rootLoading OrElse Not Me.TV.IsHandleCreated Then Return
            Me._rootGeneration += 1
            Dim generation As Int64 = Me._rootGeneration
            Me._rootLoading = True
            Dim unused = Task.Run(Function() ComputerNode.ReadEntries()).ContinueWith(
                Sub(task As Task(Of List(Of ComputerNode.RootEntry)))
                    If task.IsFaulted Then Debug.WriteLine($"Fehler beim Laden der Wurzeleinträge: {task.Exception}")
                    Try
                        Dim ignored = Me.TV.BeginInvoke(New MethodInvoker(
                            Sub()
                                If Me.IsDisposed OrElse Me.Disposing OrElse generation <> Me._rootGeneration OrElse
                                    node.TreeView IsNot Me.TV OrElse Not node.IsExpanded Then Return
                                Me._rootLoading = False
                                If task.IsFaulted Then
                                    node.ToolTipText = "Laufwerke und Spezialordner konnten nicht geladen werden. Erneut aufklappen, um es nochmals zu versuchen."
                                    Return
                                End If
                                Me.TV.BeginUpdate()
                                Try
                                    node.ApplyEntries(task.Result)
                                    Me.MarkUnavailableDrives(node, task.Result)
                                    node.ToolTipText = String.Empty
                                    Me._rootLoaded = True
                                Finally
                                    Me.TV.EndUpdate()
                                End Try
                            End Sub))
                    Catch ex As InvalidOperationException
                        Debug.WriteLine($"Wurzeleinträge konnten nicht zugestellt werden: {ex.Message}")
                    End Try
                End Sub)
        End Sub

        ''' <summary>Startet nach Wiederherstellung des Handles eine noch ausstehende Wurzelauflistung.</summary>
        ''' <param name="sender">Das TreeView.</param>
        ''' <param name="e">Ereignisdaten.</param>
        Private Sub TV_HandleCreated(sender As Object, e As EventArgs) Handles TV.HandleCreated
            If Me.TV.Nodes.Count > 0 Then
                Dim root As ComputerNode = TryCast(Me.TV.Nodes(0), ComputerNode)
                If root IsNot Nothing AndAlso root.IsExpanded Then Me.LoadRootAsync(root)
            End If
            Dim pending As New List(Of String)(Me._pendingDriveAdds.Keys)
            Me._pendingDriveAdds.Clear()
            If Me._rootLoaded Then
                For Each path As String In pending
                    Me.StartDriveAdd(path)
                Next
            End If
            SyncLock Me._watcherLock
                For Each entry In Me._FileSystemWatchers
                    Me.QueueWatcherRefresh(entry.Value, entry.Key)
                Next
            End SyncLock
        End Sub

        ''' <summary>Verwirft Ergebnisse, die an ein zerstörtes Handle gebunden sind.</summary>
        ''' <param name="sender">Das TreeView.</param>
        ''' <param name="e">Ereignisdaten.</param>
        Private Sub TV_HandleDestroyed(sender As Object, e As EventArgs) Handles TV.HandleDestroyed
            Me.CancelActiveNavigation()
            Me._rootGeneration += 1
            Me._rootLoading = False
        End Sub

        ''' <summary>Fragt Unterverzeichnisse im Hintergrund ab und übernimmt nur das neueste gültige Ergebnis.</summary>
        ''' <param name="node">Der auf dem UI-Thread verwaltete Verzeichnisknoten.</param>
        Private Sub LoadDirectoryAsync(node As DirectoryNode)
            Me._loadGeneration += 1
            Dim generation As Int64 = Me._loadGeneration
            Me._pendingLoads(node) = generation
            Dim path As String = node.DirectoryPath
            Dim unused = Task.Run(Function() DirectoryNode.ReadChildren(path)).ContinueWith(
                Sub(task As Task(Of DirectoryNode.LoadResult))
                    If task.IsFaulted Then
                        Debug.WriteLine($"Fehler beim Laden von '{path}': {task.Exception}")
                    End If
                    Try
                        Dim ignored = Me.TV.BeginInvoke(New MethodInvoker(
                            Sub()
                                Dim current As Int64
                                If Me.IsDisposed OrElse Not Me._pendingLoads.TryGetValue(node, current) OrElse current <> generation Then Return
                                Dim removed = Me._pendingLoads.Remove(node)
                                If node.TreeView IsNot Me.TV OrElse Not node.IsExpanded Then Return
                                Dim ancestor As TreeNode = node.Parent
                                While ancestor IsNot Nothing
                                    If Not ancestor.IsExpanded Then Return
                                    ancestor = ancestor.Parent
                                End While
                                Me.TV.BeginUpdate()
                                Try
                                    If task.IsFaulted Then
                                        node.Status = DirectoryNode.DirectoryStatus.Unavailable
                                        node.ChildrenLoaded = False
                                        node.EnsureRetryPlaceholder()
                                    Else
                                        If task.Result.Status = DirectoryNode.DirectoryStatus.Accessible Then
                                            Me.RemoveMissingChildWatchers(node, task.Result.Directories)
                                        End If
                                        Dim loaded = node.ApplyLoadResult(task.Result)
                                    End If
                                    Me.UpdateNodeAccessVisual(node)
                                Finally
                                    Me.TV.EndUpdate()
                                End Try
                            End Sub))
                    Catch ex As InvalidOperationException
                        Debug.WriteLine($"Ladeergebnis für '{path}' konnte nicht zugestellt werden: {ex.Message}")
                    End Try
                End Sub)
        End Sub

        ''' <summary>Entfernt Überwachungen für Unterbäume, die im neuen Verzeichnisergebnis nicht mehr vorhanden sind.</summary>
        ''' <param name="node">Aktualisierter Verzeichnisknoten.</param>
        ''' <param name="directories">Aktuell vorhandene unmittelbare Unterverzeichnisse.</param>
        Private Sub RemoveMissingChildWatchers(node As DirectoryNode, directories As String())
            Dim present As New HashSet(Of String)(directories, StringComparer.OrdinalIgnoreCase)
            For Each child As TreeNode In node.Nodes
                Dim folder As FolderNode = TryCast(child, FolderNode)
                If folder IsNot Nothing AndAlso Not present.Contains(folder.FullPath) Then
                    Me.RemoveFileSystemWatchers(folder.FullPath)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Startet die Einrichtung einer Verzeichnisüberwachung auf einem Hintergrundthread.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        Private Sub CreateFileSystemWatcher(FolderPath As String)
            If String.IsNullOrEmpty(FolderPath) Then Return
            Dim generation As Int64
            SyncLock Me._watcherLock
                If Me._watchersStopped OrElse Me._FileSystemWatchers.ContainsKey(FolderPath) OrElse Me._pendingWatchers.ContainsKey(FolderPath) Then Return
                Me._watcherGeneration += 1
                generation = Me._watcherGeneration
                Me._pendingWatchers.Add(FolderPath, generation)
            End SyncLock
            Dim unused = Task.Run(Sub() Me.InitializeFileSystemWatcher(FolderPath, generation)).ContinueWith(
                Sub(task As Task)
                    If task.IsFaulted Then Debug.WriteLine($"Fehler beim Erstellen des FileSystemWatchers für '{FolderPath}': {task.Exception}")
                End Sub)
        End Sub

        ''' <summary>Erstellt und registriert einen Watcher, sofern seine Anforderung noch gültig ist.</summary>
        ''' <param name="folderPath">Zu überwachendes Verzeichnis.</param>
        ''' <param name="generation">Generation der ursprünglichen Anforderung.</param>
        Private Sub InitializeFileSystemWatcher(folderPath As String, generation As Int64)
            Dim watcher As FileSystemWatcher = Nothing
            Dim registered As Boolean = False
            Try
                watcher = New FileSystemWatcher(folderPath) With {.NotifyFilter = NotifyFilters.DirectoryName, .IncludeSubdirectories = False}
                AddHandler watcher.Created, AddressOf Me.FSW_DirectoryChanged
                AddHandler watcher.Deleted, AddressOf Me.FSW_DirectoryChanged
                AddHandler watcher.Renamed, AddressOf Me.FSW_DirectoryChanged
                watcher.EnableRaisingEvents = True
                SyncLock Me._watcherLock
                    Dim current As Int64
                    If Not Me._watchersStopped AndAlso Me._pendingWatchers.TryGetValue(folderPath, current) AndAlso current = generation Then
                        Dim removed = Me._pendingWatchers.Remove(folderPath)
                        Me._FileSystemWatchers.Add(folderPath, watcher)
                        registered = True
                    End If
                End SyncLock
                If registered Then Me.QueueWatcherRefresh(watcher, folderPath)
            Catch ex As IOException
                Debug.WriteLine($"Verzeichnisüberwachung für '{folderPath}' nicht verfügbar: {ex.Message}")
            Catch ex As UnauthorizedAccessException
                Debug.WriteLine($"Zugriff auf Verzeichnisüberwachung für '{folderPath}' verweigert: {ex.Message}")
            Catch ex As ArgumentException
                Debug.WriteLine($"Ungültiges Verzeichnis für Überwachung '{folderPath}': {ex.Message}")
            Catch ex As System.Security.SecurityException
                Debug.WriteLine($"Verzeichnisüberwachung für '{folderPath}' nicht erlaubt: {ex.Message}")
            Finally
                SyncLock Me._watcherLock
                    Dim current As Int64
                    If Me._pendingWatchers.TryGetValue(folderPath, current) AndAlso current = generation Then
                        Dim removed = Me._pendingWatchers.Remove(folderPath)
                    End If
                End SyncLock
                If watcher IsNot Nothing AndAlso Not registered Then
                    Me.RemoveWatcherHandlers(watcher)
                    watcher.Dispose()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Entfernt und entsorgt alle FileSystemWatcher, die mit dem angegebenen Verzeichnis oder seinen
        ''' Unterverzeichnissen verknüpft sind.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        Private Sub RemoveFileSystemWatchers(FolderPath As String)
            Dim toRemove As New List(Of String)
            SyncLock Me._watcherLock
                Me.FindWatchersToRemove(FolderPath, toRemove)
                Dim pending As New List(Of String)(Me._pendingWatchers.Keys)
                For Each path As String In pending
                    If IsPathWithin(path, FolderPath) Then
                        Dim removed = Me._pendingWatchers.Remove(path)
                    End If
                Next
            End SyncLock
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
                If IsPathWithin(watcherPath, FolderPath) Then
                    toRemove.Add(watcherPath)
                End If
            Next
        End Sub

        ''' <summary>Prüft, ob ein Pfad im angegebenen Teilbaum liegt; ein leerer Basispfad bezeichnet den gesamten Baum.</summary>
        ''' <param name="candidate">Zu prüfender Pfad.</param>
        ''' <param name="parent">Basispfad des Teilbaums.</param>
        ''' <returns>True für den Basispfad und seine Unterverzeichnisse.</returns>
        Private Shared Function IsPathWithin(candidate As String, parent As String) As Boolean
            If String.IsNullOrEmpty(parent) Then Return True
            If candidate.Equals(parent, StringComparison.OrdinalIgnoreCase) Then Return True
            Dim prefix As String = parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar
            Return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
        End Function

        ''' <summary>
        ''' Entfernt und entsorgt die FileSystemWatcher, deren Pfade in der Liste "toRemove" enthalten sind.
        ''' </summary>
        ''' <param name="toRemove"></param>
        Private Sub RemoveAndDisposeWatchers(toRemove As List(Of String))
            Dim watchers As New List(Of FileSystemWatcher)
            SyncLock Me._watcherLock
                For Each watcherPath In toRemove
                    Dim request As RefreshRequest = Nothing
                    If Me._pendingRefreshes.TryGetValue(watcherPath, request) Then
                        Dim removedRefresh = Me._pendingRefreshes.Remove(watcherPath)
                        request.Timer.Dispose()
                    End If
                    Dim watcher As FileSystemWatcher = Nothing
                    If Me._FileSystemWatchers.TryGetValue(watcherPath, watcher) Then
                        Dim removed = Me._FileSystemWatchers.Remove(watcherPath)
                        watchers.Add(watcher)
                    End If
                Next
            End SyncLock
            For Each watcher As FileSystemWatcher In watchers
                watcher.EnableRaisingEvents = False
                Me.RemoveWatcherHandlers(watcher)
                watcher.Dispose()
            Next
        End Sub

        ''' <summary>Verhindert weitere Registrierungen und entsorgt alle bestehenden Verzeichnisüberwachungen.</summary>
        Private Sub StopFileSystemWatchers()
            Dim paths As List(Of String)
            SyncLock Me._watcherLock
                Me._watchersStopped = True
                Me._pendingWatchers.Clear()
                paths = New List(Of String)(Me._FileSystemWatchers.Keys)
            End SyncLock
            Me.RemoveAndDisposeWatchers(paths)
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
            Dim watcher As FileSystemWatcher = DirectCast(sender, FileSystemWatcher)
            Me.QueueWatcherRefresh(watcher, watcher.Path)
        End Sub

        ''' <summary>Bündelt Ereignisse einer registrierten Überwachung bis zum Ende einer Ruhezeit.</summary>
        ''' <param name="watcher">Auslösende Überwachung.</param>
        ''' <param name="folderPath">Überwachter Verzeichnispfad.</param>
        Private Sub QueueWatcherRefresh(watcher As FileSystemWatcher, folderPath As String)
            SyncLock Me._watcherLock
                Dim current As FileSystemWatcher = Nothing
                If Me._watchersStopped OrElse Not Me._FileSystemWatchers.TryGetValue(folderPath, current) OrElse current IsNot watcher Then Return
                Dim previous As RefreshRequest = Nothing
                If Me._pendingRefreshes.TryGetValue(folderPath, previous) Then previous.Timer.Dispose()
                Dim request As New RefreshRequest()
                request.Timer = New System.Threading.Timer(Sub(state) Me.DeliverWatcherRefresh(watcher, folderPath, request), Nothing, RefreshDelayMilliseconds, System.Threading.Timeout.Infinite)
                Me._pendingRefreshes(folderPath) = request
            End SyncLock
        End Sub

        ''' <summary>Stellt den letzten gültigen Auftrag auf dem UI-Thread zu.</summary>
        ''' <param name="watcher">Auslösende Überwachung.</param>
        ''' <param name="folderPath">Überwachter Verzeichnispfad.</param>
        ''' <param name="request">Identität des entprellten Auftrags.</param>
        Private Sub DeliverWatcherRefresh(watcher As FileSystemWatcher, folderPath As String, request As RefreshRequest)
            Try
                Dim unused = Me.TV.BeginInvoke(New MethodInvoker(
                    Sub()
                        SyncLock Me._watcherLock
                            Dim current As FileSystemWatcher = Nothing
                            If Me._watchersStopped OrElse Not Me._FileSystemWatchers.TryGetValue(folderPath, current) OrElse current IsNot watcher Then Return
                            Dim pending As RefreshRequest = Nothing
                            If Not Me._pendingRefreshes.TryGetValue(folderPath, pending) OrElse pending IsNot request Then Return
                            Dim removed = Me._pendingRefreshes.Remove(folderPath)
                            request.Timer.Dispose()
                        End SyncLock
                        Me.RefreshMatchingNodes(Me.TV.Nodes, folderPath)
                    End Sub))
            Catch ex As InvalidOperationException
                Debug.WriteLine($"Aktualisierung für '{folderPath}' konnte nicht zugestellt werden: {ex.Message}")
            End Try
        End Sub

        ''' <summary>Aktualisiert alle sichtbaren Darstellungen des betroffenen Verzeichnisses.</summary>
        ''' <param name="nodes">Zu durchsuchende Knoten.</param>
        ''' <param name="folderPath">Pfad des geänderten Verzeichnisses.</param>
        Private Sub RefreshMatchingNodes(nodes As TreeNodeCollection, folderPath As String)
            For Each node As TreeNode In nodes
                If TypeOf node Is DirectoryNode AndAlso node.IsExpanded AndAlso String.Equals(GetDirectoryPath(node), folderPath, StringComparison.OrdinalIgnoreCase) Then
                    Me.LoadSubfolders(node)
                End If
                If node.IsExpanded Then Me.RefreshMatchingNodes(node.Nodes, folderPath)
            Next
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

            Dim directoryNode As DirectoryNode = TryCast(e.Node, DirectoryNode)
            If directoryNode Is Nothing Then Return
            If Me._synchronousNavigation Then
                Dim removed = Me._pendingLoads.Remove(directoryNode)
                e.Cancel = Not directoryNode.LoadChildren()
                Me.UpdateNodeAccessVisual(directoryNode)
            ElseIf Not directoryNode.ChildrenLoaded Then
                Me.LoadDirectoryAsync(directoryNode)
            End If
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
            If Not Me._navigationMutation Then Me.CancelActiveNavigation()
            If TypeOf e.Node Is ComputerNode Then
                Me._rootGeneration += 1
                Me._rootLoading = False
                Me._rootLoaded = False
                Me._pendingDriveAdds.Clear()
            End If
            Me.RemoveFileSystemWatchers(GetDirectoryPath(e.Node))
            Me.RestoreVisibleWatchers(Me.TV.Nodes, GetDirectoryPath(e.Node), e.Node)
            Dim obsolete As New List(Of DirectoryNode)
            For Each pending As DirectoryNode In Me._pendingLoads.Keys
                Dim ancestor As TreeNode = pending
                While ancestor IsNot Nothing
                    If ancestor Is e.Node Then
                        obsolete.Add(pending)
                        Exit While
                    End If
                    ancestor = ancestor.Parent
                End While
            Next
            For Each pending As DirectoryNode In obsolete
                Dim removed = Me._pendingLoads.Remove(pending)
                pending.ChildrenLoaded = False
            Next
            Dim directoryNode As DirectoryNode = TryCast(e.Node, DirectoryNode)
            If directoryNode IsNot Nothing Then
                directoryNode.ChildrenLoaded = False
            End If
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, nachdem ein Knoten im TreeView ausgewählt wurde. Aktualisiert den ausgewählten Pfad und
        ''' löst das Ereignis SelectedPathChanged nach erfolgreicher Zugriffsprüfung aus.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TV_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TV.AfterSelect
            If Not Me._navigationSelecting Then Me.CancelActiveNavigation()
            Me._selectionGeneration += 1
            Dim generation As Int64 = Me._selectionGeneration
            Me._pendingSelection = Nothing
            If Me._navigationSelecting Then Return
            Dim selectedpath As String = GetDirectoryPath(e.Node)
            If String.IsNullOrEmpty(selectedpath) Then Exit Sub
            If Me._synchronousNavigation Then
                Dim status = CheckFolderAccess(selectedpath)
                Me.ApplyAccessStatus(e.Node, status)
                If status = DirectoryNode.DirectoryStatus.Accessible Then Me.PublishSelectedPath(selectedpath)
                Return
            End If

            Dim selectedNode As TreeNode = e.Node
            Me._pendingSelection = selectedNode
            Dim unused = Task.Run(Function() CheckFolderAccess(selectedpath)).ContinueWith(
                Sub(task As Task(Of DirectoryNode.DirectoryStatus))
                    If task.IsFaulted Then
                        Debug.WriteLine($"Fehler bei der Zugriffsprüfung für '{selectedpath}': {task.Exception}")
                    End If
                    Try
                        Dim ignored = Me.TV.BeginInvoke(New MethodInvoker(
                            Sub()
                                If Me.IsDisposed OrElse Me.Disposing OrElse generation <> Me._selectionGeneration OrElse
                                    selectedNode.TreeView IsNot Me.TV OrElse Me.TV.SelectedNode IsNot selectedNode Then Return
                                Me._pendingSelection = Nothing
                                If task.Status = TaskStatus.RanToCompletion Then
                                    Me.ApplyAccessStatus(selectedNode, task.Result)
                                    If task.Result = DirectoryNode.DirectoryStatus.Accessible Then Me.PublishSelectedPath(selectedpath)
                                End If
                            End Sub))
                    Catch ex As InvalidOperationException
                        Debug.WriteLine($"Auswahlprüfung für '{selectedpath}' konnte nicht zugestellt werden: {ex.Message}")
                    End Try
                End Sub)
        End Sub

        ''' <summary>Übernimmt einen bestätigten Pfad und benachrichtigt auf dem UI-Thread über die Auswahl.</summary>
        ''' <param name="path">Der erfolgreich geprüfte Verzeichnispfad.</param>
        Private Sub PublishSelectedPath(path As String)
            Me._SelectedPath = path
            RaiseEvent SelectedPathChanged(Me, New SelectedPathChangedEventArgs(path))
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, wenn ein neues Laufwerk hinzugefügt wird. Fügt einen neuen DriveNode für das hinzugefügte
        ''' Laufwerk in die TreeView-Struktur ein.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub DW_DriveAdded(sender As Object, e As DriveWatcherControl.DriveWatcherAddedEventArgs) Handles DW.DriveAdded
            If Not Me._rootLoaded Then
                Me.RestartRootLoading()
                Return
            End If
            Me.StartDriveAdd(e.DriveName)
        End Sub

        ''' <summary>Ermittelt die Metadaten eines hinzugefügten Laufwerks im Hintergrund.</summary>
        ''' <param name="path">Vollständiger Pfad des neuen Laufwerks.</param>
        Private Sub StartDriveAdd(path As String)
            For Each node As TreeNode In Me.TV.Nodes.Item(0).Nodes
                Dim drive As DriveNode = TryCast(node, DriveNode)
                If drive IsNot Nothing AndAlso String.Equals(drive.FullPath, path, StringComparison.OrdinalIgnoreCase) Then Return
            Next
            If Me._pendingDriveAdds.ContainsKey(path) Then Return
            Me._driveAddGeneration += 1
            Dim generation As Int64 = Me._driveAddGeneration
            Dim rootGeneration As Int64 = Me._rootGeneration
            Dim root As TreeNode = Me.TV.Nodes(0)
            Me._pendingDriveAdds.Add(path, generation)
            Dim unused = Task.Run(Function() ComputerNode.ReadDriveEntry(New DriveInfo(path))).ContinueWith(
                Sub(task As Task(Of ComputerNode.RootEntry))
                    If task.IsFaulted Then Debug.WriteLine($"Laufwerk '{path}' konnte nicht geladen werden: {task.Exception}")
                    Try
                        Dim ignored = Me.TV.BeginInvoke(New MethodInvoker(
                            Sub()
                                Dim current As Int64
                                If Me.IsDisposed OrElse Me.Disposing OrElse Not Me._pendingDriveAdds.TryGetValue(path, current) OrElse current <> generation Then Return
                                Dim removed = Me._pendingDriveAdds.Remove(path)
                                If task.IsFaulted OrElse Not Me._rootLoaded OrElse rootGeneration <> Me._rootGeneration OrElse
                                    root.TreeView IsNot Me.TV Then Return
                                Me.InsertDriveNode(root, task.Result)
                            End Sub))
                    Catch ex As InvalidOperationException
                        Debug.WriteLine($"Laufwerk '{path}' konnte nicht zugestellt werden: {ex.Message}")
                    End Try
                End Sub)
        End Sub

        ''' <summary>Fügt einen vorbereiteten Laufwerksknoten sortiert ein, sofern er noch nicht vorhanden ist.</summary>
        ''' <param name="root">Wurzelknoten des TreeViews.</param>
        ''' <param name="entry">Im Hintergrund ermittelte Laufwerksdaten.</param>
        Private Sub InsertDriveNode(root As TreeNode, entry As ComputerNode.RootEntry)
            For Each node As TreeNode In root.Nodes
                Dim drive As DriveNode = TryCast(node, DriveNode)
                If drive IsNot Nothing AndAlso String.Equals(drive.FullPath, entry.Path, StringComparison.OrdinalIgnoreCase) Then Return
            Next
            Dim newNode As New DriveNode(entry.Text, entry.Path, entry.ImageKey)
            If entry.Unavailable Then Me.ApplyAccessStatus(newNode, DirectoryNode.DirectoryStatus.Unavailable)
            Dim inserted As Boolean = False
            For i As Int32 = 0 To root.Nodes.Count - 1
                Dim currNode As TreeNode = root.Nodes(i)
                If TypeOf currNode Is DriveNode AndAlso
                        String.Compare(
                        currNode.Tag.ToString,
                        newNode.Tag.ToString,
                        StringComparison.OrdinalIgnoreCase) > 0 Then
                    root.Nodes.Insert(i, newNode)
                    inserted = True
                    Exit For
                End If
            Next
            If Not inserted Then
                Dim unused = root.Nodes.Add(newNode)
            End If
        End Sub

        ''' <summary>Markiert Laufwerke, deren Metadaten beim Laden nicht verfügbar waren.</summary>
        ''' <param name="root">Wurzelknoten mit den neu angelegten Laufwerken.</param>
        ''' <param name="entries">Zugehörige Laufwerks- und Spezialordnerdaten.</param>
        Private Sub MarkUnavailableDrives(root As ComputerNode, entries As List(Of ComputerNode.RootEntry))
            For index As Int32 = 0 To entries.Count - 1
                If entries(index).Unavailable Then Me.ApplyAccessStatus(root.Nodes(index), DirectoryNode.DirectoryStatus.Unavailable)
            Next
        End Sub

        ''' <summary>
        ''' Wird aufgerufen, wenn ein Laufwerk entfernt wird. Entfernt den entsprechenden DriveNode aus der
        ''' TreeView-Struktur.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub DW_DriveRemoved(sender As Object, e As DriveWatcherControl.DriveWatcherRemovedEventArgs) Handles DW.DriveRemoved
            Dim cancelled = Me._pendingDriveAdds.Remove(e.DriveName)
            If Not Me._rootLoaded Then
                Me.RemoveFileSystemWatchers(e.DriveName)
                Me.RestartRootLoading()
                Return
            End If
            For Each node As TreeNode In Me.TV.Nodes.Item(0).Nodes
                If TypeOf node Is DriveNode Then
                    If String.Equals(CType(node, DriveNode).FullPath, e.DriveName, StringComparison.OrdinalIgnoreCase) Then
                        Me.RemoveFileSystemWatchers(CType(node, DriveNode).FullPath)
                        CType(node, DriveNode).Remove()
                    End If
                End If
            Next
        End Sub

        ''' <summary>Startet nach einer Laufwerksänderung eine noch nicht abgeschlossene Wurzelauflistung neu.</summary>
        Private Sub RestartRootLoading()
            Me._rootGeneration += 1
            Me._rootLoading = False
            Me._pendingDriveAdds.Clear()
            If Me.TV.Nodes.Count > 0 Then
                Dim root As ComputerNode = TryCast(Me.TV.Nodes(0), ComputerNode)
                If root IsNot Nothing AndAlso root.IsExpanded Then Me.LoadRootAsync(root)
            End If
        End Sub

        ''' <summary>
        ''' Überprüft, ob der angegebene Ordnerpfad geöffnet werden kann, indem versucht wird, auf die
        ''' Dateisystemeinträge zuzugreifen.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Private Shared Function CheckFolderAccess(path As String) As DirectoryNode.DirectoryStatus
            If String.IsNullOrWhiteSpace(path) Then Return DirectoryNode.DirectoryStatus.Missing

            Try
                Using it = Directory.EnumerateFileSystemEntries(path).GetEnumerator()
                    Dim unused = it.MoveNext() ' Zugriffstest (auch bei leerem Ordner gültig)
                End Using
                Return DirectoryNode.DirectoryStatus.Accessible
            Catch ex As UnauthorizedAccessException
                Debug.WriteLine($"Zugriff auf '{path}' verweigert: {ex.Message}")
                Return DirectoryNode.DirectoryStatus.AccessDenied
            Catch ex As DirectoryNotFoundException
                Debug.WriteLine($"Verzeichnis '{path}' nicht gefunden: {ex.Message}")
                Return DirectoryNode.DirectoryStatus.Missing
            Catch ex As PathTooLongException
                Debug.WriteLine($"Verzeichnispfad '{path}' ist zu lang: {ex.Message}")
                Return DirectoryNode.DirectoryStatus.PathTooLong
            Catch ex As System.Security.SecurityException
                Debug.WriteLine($"Zugriff auf '{path}' nicht erlaubt: {ex.Message}")
                Return DirectoryNode.DirectoryStatus.AccessDenied
            Catch ex As IOException
                Debug.WriteLine($"Verzeichnis '{path}' nicht verfügbar: {ex.Message}")
                Return DirectoryNode.DirectoryStatus.Unavailable
            End Try
        End Function

        ''' <summary>Übernimmt das Ergebnis einer Auswahlprüfung und aktualisiert die Fehlermarkierung.</summary>
        ''' <param name="node">Geprüfter Verzeichnisknoten.</param>
        ''' <param name="status">Ergebnis der Zugriffsprüfung.</param>
        Private Sub ApplyAccessStatus(node As TreeNode, status As DirectoryNode.DirectoryStatus)
            Dim directoryNode As DirectoryNode = TryCast(node, DirectoryNode)
            If directoryNode Is Nothing Then Return
            directoryNode.Status = status
            If status <> DirectoryNode.DirectoryStatus.Accessible Then
                directoryNode.ChildrenLoaded = False
                directoryNode.EnsureRetryPlaceholder()
            End If
            Me.UpdateNodeAccessVisual(directoryNode)
        End Sub

        ''' <summary>
        ''' Aktualisiert die visuelle Darstellung des Knotens basierend auf dem Zugriff auf das zugehörige Verzeichnis.
        ''' </summary>
        ''' <param name="node"></param>
        Private Sub UpdateNodeAccessVisual(node As TreeNode)
            Dim path As String = GetDirectoryPath(node)
            If String.IsNullOrWhiteSpace(path) Then Exit Sub

            Dim directoryNode As DirectoryNode = TryCast(node, DirectoryNode)
            If directoryNode Is Nothing OrElse directoryNode.Status = DirectoryNode.DirectoryStatus.Unknown OrElse
                directoryNode.Status = DirectoryNode.DirectoryStatus.Accessible Then
                node.ForeColor = Me.TV.ForeColor
                node.ToolTipText = String.Empty
            Else
                node.ForeColor = Drawing.Color.Gray
                Select Case directoryNode.Status
                    Case DirectoryNode.DirectoryStatus.AccessDenied
                        node.ToolTipText = "Kein Zugriff auf diesen Ordner"
                    Case DirectoryNode.DirectoryStatus.Missing
                        node.ToolTipText = "Ordner nicht gefunden"
                    Case DirectoryNode.DirectoryStatus.PathTooLong
                        node.ToolTipText = "Verzeichnispfad ist zu lang"
                    Case Else
                        node.ToolTipText = "Ordner nicht verfügbar"
                End Select
            End If
        End Sub

#End Region

    End Class

End Namespace

