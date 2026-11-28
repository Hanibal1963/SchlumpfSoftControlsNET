' --------------------------------------------------------------------------------------------------------
' Datei: ExplorerTreeViewTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.ExplorerTreeViewControl

Namespace SchlumpfSoftControlsNET.Tests.ExplorerTreeViewControl
    <TestClass>
    Public Class ExplorerTreeViewTests
        ''' <summary>Prüft die unabhängige Lebensdauer der Bildlisten bei gemeinsamem Symbolcache.</summary>
        <STATestMethod>
        Public Sub ImageCache_InstancesRemainIndependentAfterDisposal()
            Dim first As New ExplorerTreeView()
            Using second As New ExplorerTreeView()
                Dim firstImages As ImageList = FindControl(Of TreeView)(first, "TV").ImageList
                Dim secondImages As ImageList = FindControl(Of TreeView)(second, "TV").ImageList
                Assert.AreNotSame(firstImages, secondImages)
                Assert.AreEqual(17, firstImages.Images.Count)
                Assert.AreEqual(firstImages.Images.Count, secondImages.Images.Count)
                For Each key As String In {ICON_COMPUTER, ICON_DRIVE_NOROOT, ICON_DRIVE_RAM, ICON_FOLDER_FOLDER, ICON_FOLDER_DOWNLOADS}
                    Assert.IsTrue(firstImages.Images.ContainsKey(key))
                    Assert.IsTrue(secondImages.Images.ContainsKey(key))
                Next
                first.Dispose()
                Assert.IsNotNull(secondImages.Images(ICON_FOLDER_FOLDER))
                Using third As New ExplorerTreeView()
                    Assert.IsNotNull(FindControl(Of TreeView)(third, "TV").ImageList.Images(ICON_DRIVE_NOROOT))
                End Using
            End Using
        End Sub

        ''' <summary>Prüft, dass ein zuvor leerer Knoten nach einem Fehler erneut geöffnet werden kann.</summary>
        <TestMethod>
        Public Sub DirectoryNode_FailedRefresh_RestoresRetryPlaceholder()
            Dim node As New FolderNode("test", "C:\test")
            Assert.IsTrue(node.ApplyLoadResult(New DirectoryNode.LoadResult(DirectoryNode.DirectoryStatus.Accessible, Array.Empty(Of String)())))
            Assert.AreEqual(0, node.Nodes.Count)
            Assert.IsFalse(node.ApplyLoadResult(New DirectoryNode.LoadResult(DirectoryNode.DirectoryStatus.Unavailable, Nothing)))
            Assert.AreEqual(1, node.Nodes.Count)
            Assert.IsFalse(node.ChildrenLoaded)
        End Sub

        ''' <summary>Prüft den gemeinsamen Spezialordnercache bei gleichzeitigen Abfragen.</summary>
        <TestMethod>
        Public Sub SpecialFolderCache_ConcurrentQueriesReturnSamePaths()
            Dim names As String() = {FOLDER_DESKTOP, FOLDER_DOKUMENTE, FOLDER_DOWNLOADS, FOLDER_MUSIK, FOLDER_BILDER, FOLDER_VIDEOS}
            Dim expected As String() = names.Select(Function(name) GetSpezialFolderPath(name)).ToArray()
            Dim tasks = Enumerable.Range(0, 16).Select(Function(index) Threading.Tasks.Task.Run(Function() names.Select(Function(name) GetSpezialFolderPath(name)).ToArray())).ToArray()
            Threading.Tasks.Task.WaitAll(tasks)
            For Each task In tasks
                CollectionAssert.AreEqual(expected, task.Result)
            Next
        End Sub

        <TestMethod>
        Public Sub SelectedPathChangedEventArgs_Constructor_SetsSelectedPath()
            Dim expectedPath As String = "C:\TestFolder"
            Dim args As New SelectedPathChangedEventArgs(expectedPath)

            Assert.AreEqual(expectedPath, args.SelectedPath)
        End Sub

        ''' <summary>Prüft die Handle-Voraussetzung der asynchronen Navigation.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_WithoutHandle_Faults()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim destroy = GetType(Control).GetMethod("DestroyHandle", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                Dim ignored = destroy.Invoke(tv, Nothing)
                Assert.IsFalse(tv.IsHandleCreated)
                Dim navigation = control.ExpandPathAsync(Path.GetTempPath())
                Dim unused1 = Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub()
                        Dim unused = navigation.GetAwaiter().GetResult()
                    End Sub)
            End Using
        End Sub

        ''' <summary>Prüft den Abbruch bei Neuerstellung des TreeView-Handles.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_HandleRecreationCancelsPendingNavigation()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim navigation = control.ExpandPathAsync(Path.GetTempPath())
                Dim recreate = GetType(Control).GetMethod("RecreateHandle", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                Dim ignored = recreate.Invoke(tv, Nothing)
                Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
                Assert.IsFalse(navigation.GetAwaiter().GetResult())
            End Using
        End Sub

        ''' <summary>Prüft den Abbruch einer laufenden Navigation beim Zuklappen des Wurzelknotens.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_CollapseCancelsPendingNavigation()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim root As TreeNode = tv.Nodes(0)
                Assert.IsTrue(root.IsExpanded)
                Dim navigation = control.ExpandPathAsync(Path.GetTempPath())
                root.Collapse()
                Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
                Assert.IsFalse(navigation.GetAwaiter().GetResult())
                Assert.IsFalse(root.IsExpanded)
            End Using
        End Sub

        ''' <summary>Prüft die Threadvoraussetzung der asynchronen Navigation.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_WrongThread_Faults()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim navigation = Threading.Tasks.Task.Run(Function() control.ExpandPathAsync(Path.GetTempPath()))
                Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
                Dim unused1 = Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub()
                        Dim unused = navigation.GetAwaiter().GetResult()
                    End Sub)
            End Using
        End Sub

        ''' <summary>Prüft asynchrone Navigation über mehrere noch nicht geladene Ebenen.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_LoadsAndSelectsNestedDirectory()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim target As String = Path.Combine(root, "first", "nested")
            Dim unused = Directory.CreateDirectory(target)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim paths As New List(Of String)
                    AddHandler control.SelectedPathChanged, Sub(sender, args) paths.Add(args.SelectedPath)
                    Dim navigation = control.ExpandPathAsync(target)
                    Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted), "Die Navigation wurde nicht abgeschlossen.")
                    Assert.IsTrue(navigation.GetAwaiter().GetResult())
                    Assert.AreEqual(target, control.SelectedPath, True)
                    Assert.AreEqual("nested", tv.SelectedNode.Text)
                    CollectionAssert.AreEqual(New String() {target}, paths)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft, dass ein neuer Navigationsauftrag den vorherigen ersetzt.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_NewerRequestCancelsPreviousRequest()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim first As String = Path.Combine(root, "first")
            Dim second As String = Path.Combine(root, "second")
            Dim unused = Directory.CreateDirectory(first)
            Dim created = Directory.CreateDirectory(second)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim previous = control.ExpandPathAsync(first)
                    Dim current = control.ExpandPathAsync(second)
                    Assert.IsTrue(WaitForCondition(Function() previous.IsCompleted AndAlso current.IsCompleted))
                    Assert.IsFalse(previous.GetAwaiter().GetResult())
                    Assert.IsTrue(current.GetAwaiter().GetResult())
                    Assert.AreEqual(second, control.SelectedPath, True)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft den sofortigen Abbruch einer Navigation bei der Entsorgung.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_DisposeCancelsPendingNavigation()
            Dim control As New ExplorerTreeView()
            control.CreateControl()
            Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
            Dim handle As IntPtr = tv.Handle
            Dim navigation = control.ExpandPathAsync(Path.GetTempPath())
            control.Dispose()
            Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
            Assert.IsFalse(navigation.GetAwaiter().GetResult())
        End Sub

        ''' <summary>Prüft, dass ungültige und fehlende Pfade keine Auswahl veröffentlichen.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_InvalidAndMissingPaths_ReturnFalse()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim paths As New List(Of String)
                AddHandler control.SelectedPathChanged, Sub(sender, args) paths.Add(args.SelectedPath)
                Assert.IsFalse(control.ExpandPathAsync(Nothing).GetAwaiter().GetResult())
                Assert.IsFalse(control.ExpandPathAsync("   ").GetAwaiter().GetResult())
                Dim missing As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
                Dim navigation = control.ExpandPathAsync(missing)
                Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
                Assert.IsFalse(navigation.GetAwaiter().GetResult())
                Assert.IsNull(control.SelectedPath)
                Assert.IsEmpty(paths)
            End Using
        End Sub

        ''' <summary>Prüft, dass eine Benutzerwahl eine laufende Pfadnavigation abbricht.</summary>
        <STATestMethod>
        Public Sub ExpandPathAsync_SelectionCancelsPendingNavigation()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim navigation = control.ExpandPathAsync(Path.GetTempPath())
                tv.SelectedNode = tv.Nodes(0)
                Assert.IsTrue(WaitForCondition(Function() navigation.IsCompleted))
                Assert.IsFalse(navigation.GetAwaiter().GetResult())
                Assert.AreSame(tv.Nodes(0), tv.SelectedNode)
            End Using
        End Sub

        ''' <summary>Prüft, dass entfernte Hotplug-Aufträge verworfen und erneute Einfügungen asynchron verarbeitet werden.</summary>
        <STATestMethod>
        Public Sub DriveHotplug_RemoveAndReadd_DiscardsObsoleteRequest()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim root As TreeNode = tv.Nodes(0)
                Dim drivePath As String = Path.GetPathRoot(Environment.SystemDirectory)
                Dim matchingDrives As Func(Of Int32) = Function() root.Nodes.Cast(Of TreeNode)().OfType(Of DriveNode)().Count(Function(node) String.Equals(node.FullPath, drivePath, StringComparison.OrdinalIgnoreCase))
                Assert.IsTrue(WaitForCondition(Function() matchingDrives() = 1), "Das Systemlaufwerk wurde nicht geladen.")
                Dim initialCount As Int32 = root.Nodes.Count
                Dim flags As Reflection.BindingFlags = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
                Dim added = GetType(ExplorerTreeView).GetMethod("DW_DriveAdded", flags)
                Dim removed = GetType(ExplorerTreeView).GetMethod("DW_DriveRemoved", flags)
                Dim pending = DirectCast(GetType(ExplorerTreeView).GetField("_pendingDriveAdds", flags).GetValue(control), Dictionary(Of String, Int64))
                Dim addArgs As New SchlumpfSoft.ControlsNET.DriveWatcherControl.DriveWatcherAddedEventArgs(drivePath)
                Dim removeArgs As New SchlumpfSoft.ControlsNET.DriveWatcherControl.DriveWatcherRemovedEventArgs(drivePath)

                Dim ignored = removed.Invoke(control, {Nothing, removeArgs})
                Assert.AreEqual(0, matchingDrives())
                ignored = added.Invoke(control, {Nothing, addArgs})
                Assert.AreEqual(0, matchingDrives())
                Assert.IsTrue(pending.ContainsKey(drivePath))
                Dim originalGeneration As Int64 = pending(drivePath)
                ignored = removed.Invoke(control, {Nothing, removeArgs})
                Assert.IsFalse(pending.ContainsKey(drivePath))
                ignored = added.Invoke(control, {Nothing, addArgs})
                Assert.AreNotEqual(originalGeneration, pending(drivePath))
                ignored = added.Invoke(control, {Nothing, addArgs})
                Dim beforeRecreate As Int64 = pending(drivePath)
                Dim recreate = GetType(Control).GetMethod("RecreateHandle", flags)
                ignored = recreate.Invoke(tv, Nothing)
                Assert.IsTrue(pending.ContainsKey(drivePath))
                Assert.AreNotEqual(beforeRecreate, pending(drivePath))
                Assert.IsTrue(WaitForCondition(Function() matchingDrives() = 1), "Das Laufwerk wurde nicht erneut eingefügt.")
                Assert.IsFalse(pending.ContainsKey(drivePath))
                Assert.AreEqual(initialCount, root.Nodes.Count)
            End Using
        End Sub

        ''' <summary>Prüft die Wurzelauflistung nach Erstellung des TreeView-Handles.</summary>
        <STATestMethod>
        Public Sub Root_LoadsDrivesAndSpecialFoldersAsynchronously()
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim root As TreeNode = tv.Nodes(0)
                Assert.IsTrue(root.Nodes.Cast(Of TreeNode)().Any(Function(node) Not TypeOf node Is DirectoryNode))
                Assert.IsTrue(WaitForCondition(Function() root.Nodes.Cast(Of TreeNode)().Any(Function(node) TypeOf node Is DriveNode)), "Die Laufwerke wurden nicht geladen.")
                Assert.AreEqual(6, root.Nodes.Cast(Of TreeNode)().Count(Function(node) TypeOf node Is SpecialFolderNode))
                Assert.IsFalse(root.Nodes.Cast(Of TreeNode)().Any(Function(node) Not TypeOf node Is DirectoryNode))
                Dim existingDrive As DriveNode = root.Nodes.Cast(Of TreeNode)().OfType(Of DriveNode)().First()
                Dim previousCount As Int32 = root.Nodes.Count
                Dim handler = GetType(ExplorerTreeView).GetMethod("DW_DriveAdded", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                Dim ignored = handler.Invoke(control, {Nothing, New SchlumpfSoft.ControlsNET.DriveWatcherControl.DriveWatcherAddedEventArgs(existingDrive.FullPath)})
                Assert.AreEqual(previousCount, root.Nodes.Count)
            End Using
        End Sub

        ''' <summary>Prüft, dass ein fehlendes Verzeichnis den bestätigten Pfad nicht überschreibt.</summary>
        <STATestMethod>
        Public Sub Selection_MissingDirectoryKeepsConfirmedPath()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim unused = Directory.CreateDirectory(root)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim existing As New FolderNode("existing", root)
                    Dim missing As New FolderNode("missing", Path.Combine(root, "missing"))
                    Dim unused2 = tv.Nodes.Add(existing)
                    Dim unused1 = tv.Nodes.Add(missing)
                    Dim paths As New List(Of String)
                    AddHandler control.SelectedPathChanged, Sub(sender, args) paths.Add(args.SelectedPath)
                    tv.SelectedNode = existing
                    Assert.IsTrue(WaitForCondition(Function() paths.Count = 1))
                    tv.SelectedNode = missing
                    Dim pending = GetType(ExplorerTreeView).GetField("_pendingSelection", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                    Assert.IsTrue(WaitForCondition(Function() pending.GetValue(control) Is Nothing), "Die fehlgeschlagene Prüfung wurde nicht beendet.")
                    Assert.AreEqual(root, control.SelectedPath)
                    CollectionAssert.AreEqual(New String() {root}, paths)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft, dass nur die zuletzt gewählte Auswahl nach der Hintergrundprüfung veröffentlicht wird.</summary>
        <STATestMethod>
        Public Sub Selection_RapidChangesPublishOnlyLatestPath()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim first As String = Path.Combine(root, "first")
            Dim second As String = Path.Combine(root, "second")
            Dim unused = Directory.CreateDirectory(first)
            Dim created = Directory.CreateDirectory(second)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim firstNode As New FolderNode("first", first)
                    Dim secondNode As New FolderNode("second", second)
                    Dim unused2 = tv.Nodes.Add(firstNode)
                    Dim unused1 = tv.Nodes.Add(secondNode)
                    Dim paths As New List(Of String)
                    AddHandler control.SelectedPathChanged, Sub(sender, args) paths.Add(args.SelectedPath)

                    tv.SelectedNode = firstNode
                    tv.SelectedNode = secondNode
                    Assert.IsNull(control.SelectedPath)
                    Assert.IsEmpty(paths)
                    Assert.IsTrue(WaitForCondition(Function() paths.Count > 0), "Die Auswahl wurde nicht bestätigt.")
                    Assert.AreEqual(second, control.SelectedPath)
                    CollectionAssert.AreEqual(New String() {second}, paths)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft die synchrone Navigation auf einen bereits gewählten, noch ungeprüften Knoten.</summary>
        <STATestMethod>
        Public Sub ExpandPath_ConfirmsPendingSelectionSynchronously()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim target As String = Path.Combine(root, "child")
            Dim unused = Directory.CreateDirectory(target)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Assert.IsTrue(control.ExpandPath(target))
                    Dim targetNode As TreeNode = tv.SelectedNode
                    Dim paths As New List(Of String)
                    AddHandler control.SelectedPathChanged, Sub(sender, args) paths.Add(args.SelectedPath)
                    tv.SelectedNode = tv.Nodes(0)
                    tv.SelectedNode = targetNode
                    Assert.IsEmpty(paths)

                    Assert.IsTrue(control.ExpandPath(target))
                    Assert.AreEqual(target, control.SelectedPath, True)
                    CollectionAssert.AreEqual(New String() {target}, paths)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft, dass Aktualisierungen vorhandene Unterbäume nicht ersetzen.</summary>
        <TestMethod>
        Public Sub FolderNode_Refresh_PreservesExistingChildren()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim first As String = Path.Combine(root, "first")
            Dim second As String = Path.Combine(root, "second")
            Dim unused = Directory.CreateDirectory(first)
            Try
                Dim node As New FolderNode("root", root)
                Assert.IsTrue(node.LoadChildren())
                Dim original As FolderNode = DirectCast(node.Nodes(0), FolderNode)
                Dim created = Directory.CreateDirectory(second)
                Assert.IsTrue(node.LoadChildren(True))
                Assert.AreEqual(2, node.Nodes.Count)
                Assert.AreSame(original, node.Nodes(0))
                Assert.AreEqual(second, DirectCast(node.Nodes(1), FolderNode).FullPath)
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft die asynchrone Einrichtung, Änderungszustellung und Entfernung der Überwachung.</summary>
        <STATestMethod>
        Public Sub DirectoryWatcher_RefreshesAndStopsOnCollapse()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim unused = Directory.CreateDirectory(root)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim node As New FolderNode("test", root)
                    Dim unused1 = tv.Nodes.Add(node)
                    node.Expand()
                    Dim flags As Reflection.BindingFlags = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
                    Dim gate As Object = GetType(ExplorerTreeView).GetField("_watcherLock", flags).GetValue(control)
                    Dim watchers = DirectCast(GetType(ExplorerTreeView).GetField("_FileSystemWatchers", flags).GetValue(control), Dictionary(Of String, FileSystemWatcher))
                    Dim isWatching As Func(Of Boolean) = Function()
                                                             SyncLock gate
                                                                 Return watchers.ContainsKey(root)
                                                             End SyncLock
                                                         End Function
                    Assert.IsTrue(WaitForCondition(isWatching), "Die Überwachung wurde nicht eingerichtet.")
                    Dim child As String = Path.Combine(root, "new")
                    Dim created = Directory.CreateDirectory(child)
                    Assert.IsTrue(WaitForCondition(Function() node.ChildrenLoaded AndAlso node.Nodes.Count = 1 AndAlso TypeOf node.Nodes(0) Is FolderNode AndAlso DirectCast(node.Nodes(0), FolderNode).FullPath = child), "Die Änderung wurde nicht angezeigt.")
                    node.Collapse()
                    Assert.IsFalse(isWatching(), "Die Überwachung wurde beim Zuklappen nicht entfernt.")
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft Bündelung, unabhängige Pfade und das Verwerfen beim Zuklappen.</summary>
        <STATestMethod>
        Public Sub DirectoryWatcher_DebouncesPerPathAndCancelsOnCollapse()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim first As String = Path.Combine(root, "first")
            Dim second As String = Path.Combine(root, "second")
            Dim unused = Directory.CreateDirectory(first)
            Dim created = Directory.CreateDirectory(second)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim firstNode As New FolderNode("first", first)
                    Dim secondNode As New FolderNode("second", second)
                    Dim unused2 = tv.Nodes.Add(firstNode)
                    Dim unused1 = tv.Nodes.Add(secondNode)
                    firstNode.Expand()
                    secondNode.Expand()
                    Dim flags As Reflection.BindingFlags = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
                    Dim gate As Object = GetType(ExplorerTreeView).GetField("_watcherLock", flags).GetValue(control)
                    Dim watchers = DirectCast(GetType(ExplorerTreeView).GetField("_FileSystemWatchers", flags).GetValue(control), Dictionary(Of String, FileSystemWatcher))
                    Dim pending = DirectCast(GetType(ExplorerTreeView).GetField("_pendingRefreshes", flags).GetValue(control), System.Collections.IDictionary)
                    Assert.IsTrue(WaitForCondition(Function()
                                                       SyncLock gate
                                                           Return watchers.ContainsKey(first) AndAlso watchers.ContainsKey(second) AndAlso pending.Count = 0
                                                       End SyncLock
                                                   End Function))
                    Dim queue = GetType(ExplorerTreeView).GetMethod("QueueWatcherRefresh", flags)
                    Dim generation = GetType(ExplorerTreeView).GetField("_loadGeneration", flags)
                    Dim initial As Int64 = CLng(generation.GetValue(control))
                    Dim ignored = queue.Invoke(control, {watchers(first), first})
                    ignored = queue.Invoke(control, {watchers(first), first})
                    ignored = queue.Invoke(control, {watchers(second), second})
                    SyncLock gate
                        Assert.AreEqual(2, pending.Count)
                    End SyncLock
                    Assert.AreEqual(initial, CLng(generation.GetValue(control)))
                    Assert.IsTrue(WaitForCondition(Function() CLng(generation.GetValue(control)) = initial + 2))
                    ignored = queue.Invoke(control, {watchers(first), first})
                    firstNode.Collapse()
                    SyncLock gate
                        Assert.IsFalse(pending.Contains(first))
                    End SyncLock
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft die Überwachung mehrfach dargestellter Pfade nach dem Zuklappen einer Darstellung.</summary>
        <STATestMethod>
        Public Sub DirectoryWatcher_DuplicatePath_RemainsWatchedAfterCollapse()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim unused = Directory.CreateDirectory(root)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim first As New FolderNode("first", root)
                    Dim second As New FolderNode("second", root)
                    Dim unused2 = tv.Nodes.Add(first)
                    Dim unused1 = tv.Nodes.Add(second)
                    first.Expand()
                    second.Expand()
                    Dim flags As Reflection.BindingFlags = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic
                    Dim gate As Object = GetType(ExplorerTreeView).GetField("_watcherLock", flags).GetValue(control)
                    Dim watchers = DirectCast(GetType(ExplorerTreeView).GetField("_FileSystemWatchers", flags).GetValue(control), Dictionary(Of String, FileSystemWatcher))
                    Assert.IsTrue(WaitForCondition(Function()
                                                       SyncLock gate
                                                           Return watchers.ContainsKey(root)
                                                       End SyncLock
                                                   End Function))
                    first.Collapse()
                    Assert.IsTrue(WaitForCondition(Function()
                                                       SyncLock gate
                                                           Return watchers.ContainsKey(root)
                                                       End SyncLock
                                                   End Function))
                    Dim child As String = Path.Combine(root, "child")
                    Dim created = Directory.CreateDirectory(child)
                    Assert.IsTrue(WaitForCondition(Function() second.Nodes.Cast(Of TreeNode)().OfType(Of FolderNode)().Any(Function(node) node.FullPath = child)))
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Verarbeitet UI-Nachrichten bis zur Bedingung oder bis zum Zeitlimit.</summary>
        ''' <param name="condition">Zu prüfender Zustand.</param>
        ''' <returns>True, wenn der Zustand innerhalb des Zeitlimits erreicht wurde.</returns>
        Private Shared Function WaitForCondition(condition As Func(Of Boolean)) As Boolean
            Dim timeout As Stopwatch = Stopwatch.StartNew()
            Do
                Application.DoEvents()
                If condition() Then Return True
                Threading.Thread.Sleep(10)
            Loop While timeout.Elapsed < TimeSpan.FromSeconds(10)
            Return False
        End Function

        ''' <summary>Prüft die Übernahme einer im Hintergrund ermittelten Verzeichnisliste.</summary>
        <STATestMethod>
        Public Sub ExpandDirectory_LoadsChildrenAsynchronously()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim child As String = Path.Combine(root, "child")
            Dim unused = Directory.CreateDirectory(child)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Dim node As New FolderNode("test", root)
                    Dim unused1 = tv.Nodes.Add(node)
                    node.Expand()
                    Dim timeout As Stopwatch = Stopwatch.StartNew()
                    While Not node.ChildrenLoaded AndAlso timeout.Elapsed < TimeSpan.FromSeconds(10)
                        Application.DoEvents()
                        Threading.Thread.Sleep(10)
                    End While
                    Assert.IsTrue(node.ChildrenLoaded)
                    Assert.AreEqual(DirectoryNode.DirectoryStatus.Accessible, node.Status)
                    Assert.AreEqual(1, node.Nodes.Count)
                    Assert.AreEqual(child, DirectCast(node.Nodes(0), FolderNode).FullPath)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft die gezielte Behandlung eines überlangen Pfads beim Laden.</summary>
        <TestMethod>
        Public Sub DirectoryNode_ReadChildren_PathTooLong_IsClassified()
            Dim path As String = "C:\" & New String("a"c, 33000)
            Dim result = DirectoryNode.ReadChildren(path)
            Assert.AreEqual(DirectoryNode.DirectoryStatus.PathTooLong, result.Status)
            Assert.IsNull(result.Directories)
        End Sub

        ''' <summary>Prüft die Fehlermarkierung und deren Rücknahme nach erneuter Auswahl.</summary>
        <STATestMethod>
        Public Sub Selection_MissingDirectory_IsMarkedAndCanRecover()
            Dim path As String = IO.Path.Combine(IO.Path.GetTempPath(), Guid.NewGuid().ToString())
            Using control As New ExplorerTreeView()
                control.CreateControl()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                Dim handle As IntPtr = tv.Handle
                Dim node As New FolderNode("missing", path)
                Dim added = tv.Nodes.Add(node)
                tv.SelectedNode = node
                Assert.IsTrue(WaitForCondition(Function() node.Status = DirectoryNode.DirectoryStatus.Missing))
                Assert.AreEqual(Color.Gray, node.ForeColor)
                Assert.AreEqual("Ordner nicht gefunden", node.ToolTipText)
                Assert.AreNotEqual(path, control.SelectedPath)
                Dim created = Directory.CreateDirectory(path)
                Try
                    tv.SelectedNode = tv.Nodes(0)
                    tv.SelectedNode = node
                    Assert.IsTrue(WaitForCondition(Function() node.Status = DirectoryNode.DirectoryStatus.Accessible))
                    Assert.AreEqual(String.Empty, node.ToolTipText)
                    Assert.AreEqual(path, control.SelectedPath)
                Finally
                    Directory.Delete(path, True)
                End Try
            End Using
        End Sub

        ''' <summary>Prüft die getrennte Abfrage und anschließende Übernahme in den Baum.</summary>
        <TestMethod>
        Public Sub DirectoryNode_ReadChildren_DoesNotChangeNodeUntilApplied()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim child As String = Path.Combine(root, "child")
            Dim unused = Directory.CreateDirectory(child)
            Try
                Dim node As New FolderNode("root", root)
                Dim result As DirectoryNode.LoadResult = DirectoryNode.ReadChildren(root)
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Accessible, result.Status)
                CollectionAssert.AreEquivalent(New String() {child}, result.Directories)
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Unknown, node.Status)
                Assert.IsFalse(node.ChildrenLoaded)
                Assert.IsNotInstanceOfType(Of FolderNode)(node.Nodes(0))

                Assert.IsTrue(node.ApplyLoadResult(result))
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Accessible, node.Status)
                Assert.AreEqual(child, DirectCast(node.Nodes(0), FolderNode).FullPath)
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft die Pfadnavigation durch mehrere noch nicht geladene Ebenen.</summary>
        <TestMethod>
        Public Sub ExpandPath_LoadsAndSelectsNestedDirectory()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim target As String = Path.Combine(root, "first", "nested")
            Dim unused = Directory.CreateDirectory(target)
            Try
                Using control As New ExplorerTreeView()
                    control.CreateControl()
                    Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")
                    Dim handle As IntPtr = tv.Handle
                    Assert.IsTrue(control.ExpandPath(target))
                    Assert.AreEqual(target, control.SelectedPath, True)
                    Assert.AreEqual("nested", tv.SelectedNode.Text)
                End Using
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft Platzhalter, verzögertes Laden und erneutes Laden nach Änderungen.</summary>
        <TestMethod>
        Public Sub FolderNode_LoadChildren_LoadsOnlyOnDemandAndRefreshes()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim unused1 = Directory.CreateDirectory(Path.Combine(root, "first", "nested"))
            Try
                Dim node As New FolderNode("root", root)
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Unknown, node.Status)
                Assert.AreEqual(1, node.Nodes.Count)
                Assert.IsNotInstanceOfType(Of FolderNode)(node.Nodes(0))

                Assert.IsTrue(node.LoadChildren())
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Accessible, node.Status)
                Assert.AreEqual(1, node.Nodes.Count)
                Dim child As FolderNode = DirectCast(node.Nodes(0), FolderNode)
                Assert.AreEqual("first", child.Text)
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Unknown, child.Status)
                Assert.AreEqual(1, child.Nodes.Count)
                Assert.IsNotInstanceOfType(Of FolderNode)(child.Nodes(0))

                Dim unused = Directory.CreateDirectory(Path.Combine(root, "second"))
                Assert.IsTrue(node.LoadChildren())
                Assert.AreEqual(1, node.Nodes.Count)
                Assert.IsTrue(node.LoadChildren(True))
                Assert.AreEqual(2, node.Nodes.Count)
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        ''' <summary>Prüft, dass fehlende Verzeichnisse erneut geladen werden können.</summary>
        <TestMethod>
        Public Sub FolderNode_LoadChildren_MissingDirectoryCanBeRetried()
            Dim root As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim node As New FolderNode("root", root)
            Assert.IsFalse(node.LoadChildren())
            Assert.AreEqual(DirectoryNode.DirectoryStatus.Missing, node.Status)
            Assert.AreEqual(1, node.Nodes.Count)
            Dim unused = Directory.CreateDirectory(root)
            Try
                Assert.IsTrue(node.LoadChildren())
                Assert.AreEqual(DirectoryNode.DirectoryStatus.Accessible, node.Status)
                Assert.AreEqual(0, node.Nodes.Count)
            Finally
                Directory.Delete(root, True)
            End Try
        End Sub

        <TestMethod>
        Public Sub Constructor_InitializesTreeViewAndRootNode()
            Using control As New ExplorerTreeView()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")

                Assert.IsNotNull(tv)
                Assert.AreEqual(1, tv.Nodes.Count)
                Assert.StartsWith("Dieser Computer", tv.Nodes(0).Text)
                Assert.IsNull(control.SelectedPath)
            End Using
        End Sub

        <TestMethod>
        Public Sub DisplayProperties_PassThroughToInnerTreeView()
            Using control As New ExplorerTreeView()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")

                control.ShowLines = False
                Assert.IsFalse(control.ShowLines)
                Assert.IsFalse(tv.ShowLines)

                control.ShowPlusMinus = False
                Assert.IsFalse(control.ShowPlusMinus)
                Assert.IsFalse(tv.ShowPlusMinus)

                control.ShowRootLines = False
                Assert.IsFalse(control.ShowRootLines)
                Assert.IsFalse(tv.ShowRootLines)

                control.Indent = 28
                Assert.AreEqual(28, control.Indent)
                Assert.AreEqual(28, tv.Indent)

                control.ItemHeight = 24
                Assert.AreEqual(24, control.ItemHeight)
                Assert.AreEqual(24, tv.ItemHeight)

                control.LineColor = Color.Blue
                Assert.AreEqual(Color.Blue, control.LineColor)
                Assert.AreEqual(Color.Blue, tv.LineColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub VisualStyles_SynchronizeWithInnerTreeView()
            Using control As New ExplorerTreeView()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")

                control.BackColor = Color.LightYellow
                Assert.AreEqual(Color.LightYellow, control.BackColor)
                Assert.AreEqual(Color.LightYellow, tv.BackColor)

                control.ForeColor = Color.DarkGreen
                Assert.AreEqual(Color.DarkGreen, control.ForeColor)
                Assert.AreEqual(Color.DarkGreen, tv.ForeColor)

                Using testFont As New Font("Tahoma", 10.0F)
                    control.Font = testFont
                    Assert.AreEqual(testFont.Name, control.Font.Name)
                    Assert.AreEqual(testFont.Name, tv.Font.Name)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ExpandPath_WithInvalidOrEmptyPath_ReturnsFalse()
            Using control As New ExplorerTreeView()
                Assert.IsFalse(control.ExpandPath(Nothing))
                Assert.IsFalse(control.ExpandPath(String.Empty))
                Assert.IsFalse(control.ExpandPath("   "))
                Assert.IsFalse(control.ExpandPath("Z:\GibtsNicht_Pfad_12345"))
            End Using
        End Sub

        Private Shared Function FindControl(Of T As Control)(root As Control, controlName As String) As T
            Dim result As T = TryFindControl(Of T)(root, controlName)
            If result IsNot Nothing Then
                Return result
            End If

            Assert.Fail($"Control '{controlName}' wurde nicht gefunden.")
            Return Nothing
        End Function

        Private Shared Function TryFindControl(Of T As Control)(root As Control, controlName As String) As T
            For Each child As Control In root.Controls
                If TypeOf child Is T AndAlso String.Equals(child.Name, controlName, StringComparison.Ordinal) Then
                    Return DirectCast(child, T)
                End If

                Dim nested As T = TryFindControl(Of T)(child, controlName)
                If nested IsNot Nothing Then
                    Return nested
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace
