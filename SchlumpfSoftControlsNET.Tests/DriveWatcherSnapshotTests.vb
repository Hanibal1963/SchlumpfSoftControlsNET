' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherSnapshotTests.vb
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Security
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl
    <TestClass>
    Public NotInheritable Class DriveWatcherSnapshotTests
        <TestMethod>
        Public Sub Read_ReadyDrive_ReturnsAllValues()
            Dim source As New FakeSource()
            Dim result = DriveInfoSnapshotReader.Read("X:\", Function(name) source)
            Assert.AreEqual("X:\", result.DriveName)
            Assert.AreEqual(DriveType.Fixed, result.DriveType)
            Assert.IsTrue(result.IsReady)
            Assert.AreEqual("Data", result.VolumeLabel)
            Assert.AreEqual(11L, result.AvailableFreeSpace)
            Assert.AreEqual(22L, result.TotalFreeSpace)
            Assert.AreEqual(33L, result.TotalSize)
            Assert.AreEqual("NTFS", result.DriveFormat)
            Assert.IsEmpty(result.ReadErrors)
        End Sub

        <TestMethod>
        <DataRow("VolumeLabel")>
        <DataRow("AvailableFreeSpace")>
        <DataRow("TotalFreeSpace")>
        <DataRow("TotalSize")>
        <DataRow("DriveFormat")>
        <DataRow("DriveType")>
        Public Sub Read_IndividualFailure_PreservesOtherValues(propertyName As String)
            Dim source As New FakeSource()
            Dim failure As New IOException("read failed")
            source.Failures.Add(propertyName, failure)
            Dim result = DriveInfoSnapshotReader.Read("X:\", Function(name) source)
            Assert.HasCount(1, result.ReadErrors)
            Assert.AreSame(failure, result.ReadErrors(propertyName))
            Assert.AreEqual(If(propertyName = "DriveType", DriveType.Unknown, DriveType.Fixed), result.DriveType)
            Assert.IsTrue(result.IsReady)
            Assert.AreEqual(If(propertyName = "VolumeLabel", "", "Data"), result.VolumeLabel)
            Assert.AreEqual(If(propertyName = "AvailableFreeSpace", 0L, 11L), result.AvailableFreeSpace)
            Assert.AreEqual(If(propertyName = "TotalFreeSpace", 0L, 22L), result.TotalFreeSpace)
            Assert.AreEqual(If(propertyName = "TotalSize", 0L, 33L), result.TotalSize)
            Assert.AreEqual(If(propertyName = "DriveFormat", "", "NTFS"), result.DriveFormat)
        End Sub

        <TestMethod>
        Public Sub Read_MultipleFailures_RecordsEachException()
            Dim source As New FakeSource()
            Dim denied As New UnauthorizedAccessException()
            Dim security As New SecurityException()
            Dim win32 As New Win32Exception()
            source.Failures.Add("VolumeLabel", denied)
            source.Failures.Add("TotalSize", security)
            source.Failures.Add("DriveFormat", win32)
            Dim result = DriveInfoSnapshotReader.Read("X:\", Function(name) source)
            Assert.HasCount(3, result.ReadErrors)
            Assert.AreSame(denied, result.ReadErrors("VolumeLabel"))
            Assert.AreSame(security, result.ReadErrors("TotalSize"))
            Assert.AreSame(win32, result.ReadErrors("DriveFormat"))
            Assert.AreEqual(11L, result.AvailableFreeSpace)
            Assert.AreEqual(22L, result.TotalFreeSpace)
            Assert.ThrowsExactly(Of NotSupportedException)(Sub() DirectCast(result.ReadErrors, IDictionary(Of String, Exception)).Clear())
        End Sub

        <TestMethod>
        Public Sub Read_NotReadyOrReadinessFailure_DoesNotReadDetails()
            Dim source As New FakeSource With {.Ready = False}
            Dim result = DriveInfoSnapshotReader.Read("X:\", Function(name) source)
            Assert.IsFalse(result.IsReady)
            Assert.IsEmpty(result.ReadErrors)
            Assert.AreEqual(0, source.DetailReads)
            Dim failure As New IOException()
            source.Failures.Add("IsReady", failure)
            result = DriveInfoSnapshotReader.Read("X:\", Function(name) source)
            Assert.IsFalse(result.IsReady)
            Assert.AreSame(failure, result.ReadErrors("IsReady"))
            Assert.AreEqual(0, source.DetailReads)
        End Sub

        <TestMethod>
        Public Sub Read_FactoryFailure_RetainsNameAndReportsFailure()
            Dim failure As New ArgumentException("invalid drive")
            Dim factory As Func(Of String, IDriveInfoSource) = Function(name)
                Throw failure
            End Function
            Dim result = DriveInfoSnapshotReader.Read("X:\", factory)
            Assert.AreEqual("X:\", result.DriveName)
            Assert.AreSame(failure, result.ReadErrors("DriveInfo"))
            Assert.AreEqual(DriveType.Unknown, result.DriveType)
            Assert.IsFalse(result.IsReady)
        End Sub

        <TestMethod>
        Public Sub Read_UnexpectedFailure_Propagates()
            Dim source As New FakeSource()
            source.Failures.Add("VolumeLabel", New InvalidOperationException("unexpected"))
            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() DriveInfoSnapshotReader.Read("X:\", Function(name) source))
        End Sub

        Private NotInheritable Class FakeSource
            Implements IDriveInfoSource

            Friend ReadOnly Failures As New Dictionary(Of String, Exception)()
            Friend Ready As Boolean = True
            Friend DetailReads As Integer

            Private Function Value(Of T)(name As String, resultValue As T, Optional detail As Boolean = False) As T
                If detail Then Me.DetailReads += 1
                If Me.Failures.ContainsKey(name) Then Throw Me.Failures(name)
                Return resultValue
            End Function

            Public ReadOnly Property DriveType As DriveType Implements IDriveInfoSource.DriveType
                Get
                    Return Me.Value(NameOf(DriveType), IO.DriveType.Fixed)
                End Get
            End Property
            Public ReadOnly Property IsReady As Boolean Implements IDriveInfoSource.IsReady
                Get
                    Return Me.Value(NameOf(IsReady), Me.Ready)
                End Get
            End Property
            Public ReadOnly Property VolumeLabel As String Implements IDriveInfoSource.VolumeLabel
                Get
                    Return Me.Value(NameOf(VolumeLabel), "Data", True)
                End Get
            End Property
            Public ReadOnly Property AvailableFreeSpace As Long Implements IDriveInfoSource.AvailableFreeSpace
                Get
                    Return Me.Value(NameOf(AvailableFreeSpace), 11L, True)
                End Get
            End Property
            Public ReadOnly Property TotalFreeSpace As Long Implements IDriveInfoSource.TotalFreeSpace
                Get
                    Return Me.Value(NameOf(TotalFreeSpace), 22L, True)
                End Get
            End Property
            Public ReadOnly Property TotalSize As Long Implements IDriveInfoSource.TotalSize
                Get
                    Return Me.Value(NameOf(TotalSize), 33L, True)
                End Get
            End Property
            Public ReadOnly Property DriveFormat As String Implements IDriveInfoSource.DriveFormat
                Get
                    Return Me.Value(NameOf(DriveFormat), "NTFS", True)
                End Get
            End Property
        End Class
    End Class
End Namespace