' --------------------------------------------------------------------------------------------------------
' Datei: ExplorerTreeViewTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.ExplorerTreeViewControl

Namespace SchlumpfSoftControlsNET.Tests.ExplorerTreeViewControl
    <TestClass>
    Public Class ExplorerTreeViewTests
        <TestMethod>
        Public Sub SelectedPathChangedEventArgs_Constructor_SetsSelectedPath()
            Dim expectedPath As String = "C:\TestFolder"
            Dim args As New SelectedPathChangedEventArgs(expectedPath)

            Assert.AreEqual(expectedPath, args.SelectedPath)
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
                    tv.Nodes.Add(existing)
                    tv.Nodes.Add(missing)
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
                    tv.Nodes.Add(firstNode)
                    tv.Nodes.Add(secondNode)
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
                    tv.Nodes.Add(node)
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
                    tv.Nodes.Add(node)
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
