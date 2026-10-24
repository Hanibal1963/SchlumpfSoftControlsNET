Imports System
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.FileListControl

Namespace SchlumpfSoftControlsNET.Tests.FileListControl
    <TestClass>
    Public Class FileListTests
        <TestMethod>
        Public Sub Constructor_SetsExpectedDefaults()
            Using control As New FileList()
                Dim lv As ListView = FindControl(Of ListView)(control, "listViewEntries")

                Assert.AreEqual(String.Empty, control.StartFolder)
                Assert.IsTrue(control.AutoResizeColumnsEnabled)
                Assert.IsNotNull(lv)
                Assert.AreEqual(View.Details, lv.View)
                Assert.IsTrue(lv.AllowColumnReorder)
                Assert.AreEqual(6, lv.Columns.Count)
            End Using
        End Sub

        <TestMethod>
        Public Sub StartFolder_TrimsAndNormalizesNullToEmpty()
            Using control As New FileList()
                control.StartFolder = Nothing
                Assert.AreEqual(String.Empty, control.StartFolder)

                control.StartFolder = "   C:\Temp   "
                Assert.AreEqual("C:\Temp", control.StartFolder)

                control.StartFolder = "c:\temp"
                Assert.AreEqual("C:\Temp", control.StartFolder)
            End Using
        End Sub

        <TestMethod>
        Public Sub AutoResizeColumnsEnabled_CanBeToggled()
            Using control As New FileList()
                control.AutoResizeColumnsEnabled = False
                Assert.IsFalse(control.AutoResizeColumnsEnabled)

                control.AutoResizeColumnsEnabled = True
                Assert.IsTrue(control.AutoResizeColumnsEnabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub ColumnOrderState_DefaultValue_ReturnsExpectedColumns()
            Using control As New FileList()
                Assert.AreEqual("Name;Type;Size;Created;LastAccess;LastWrite", control.ColumnOrderState)
            End Using
        End Sub

        <TestMethod>
        Public Sub ColumnOrderState_ApplyValidOrder_ReordersDisplayIndices()
            Using control As New FileList()
                Dim targetOrder As String = "Type;Name;Size;Created;LastAccess;LastWrite"
                control.ColumnOrderState = targetOrder

                Assert.AreEqual(targetOrder, control.ColumnOrderState)

                Dim lv As ListView = FindControl(Of ListView)(control, "listViewEntries")
                Assert.AreEqual(0, lv.Columns(1).DisplayIndex) ' HeaderType
                Assert.AreEqual(1, lv.Columns(0).DisplayIndex) ' HeaderName
            End Using
        End Sub

        <TestMethod>
        Public Sub ColumnOrderState_InvalidOrIncompleteValue_IsIgnored()
            Using control As New FileList()
                Dim originalOrder As String = control.ColumnOrderState

                control.ColumnOrderState = "Name;Type;Size"
                Assert.AreEqual(originalOrder, control.ColumnOrderState)

                control.ColumnOrderState = String.Empty
                Assert.AreEqual(originalOrder, control.ColumnOrderState)

                control.ColumnOrderState = Nothing
                Assert.AreEqual(originalOrder, control.ColumnOrderState)
            End Using
        End Sub

        <TestMethod>
        Public Sub RefreshEntries_ExecutesWithoutError()
            Using control As New FileList()
                control.RefreshEntries()
                Assert.AreEqual(String.Empty, control.StartFolder)
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
