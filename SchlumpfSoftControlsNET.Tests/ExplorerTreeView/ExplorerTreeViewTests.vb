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
