Imports System
Imports System.Drawing
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

        <TestMethod>
        Public Sub Constructor_InitializesTreeViewAndRootNode()
            Using control As New ExplorerTreeView()
                Dim tv As TreeView = FindControl(Of TreeView)(control, "TV")

                Assert.IsNotNull(tv)
                Assert.AreEqual(1, tv.Nodes.Count)
                Assert.StartsWith(tv.Nodes(0).Text, "Dieser Computer")
                Assert.IsTrue(String.IsNullOrEmpty(control.SelectedPath))
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
