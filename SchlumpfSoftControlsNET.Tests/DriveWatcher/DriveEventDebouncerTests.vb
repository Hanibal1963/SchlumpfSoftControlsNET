' --------------------------------------------------------------------------------------------------------
' Datei: DriveEventDebouncerTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Threading
Imports System.Threading.Tasks

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl
    <TestClass>
    Public NotInheritable Class DriveEventDebouncerTests
        <TestMethod>
        Public Sub IsDuplicate_InterleavedDrivesAndTypes_TracksEachIndependently()
            Dim now As Long = 1000
            Dim cache As New DriveEventDebouncer(Function() now, 1000)
            Assert.IsFalse(cache.IsDuplicate("E:\", DriveEventType.Added))
            Assert.IsFalse(cache.IsDuplicate("F:\", DriveEventType.Added))
            Assert.IsFalse(cache.IsDuplicate("E:\", DriveEventType.MediaInserted))
            Assert.IsTrue(cache.IsDuplicate("e:\", DriveEventType.Added))
            Assert.IsTrue(cache.IsDuplicate("F:\", DriveEventType.Added))
            Assert.IsTrue(cache.IsDuplicate("E:\", DriveEventType.MediaInserted))
        End Sub

        <TestMethod>
        Public Sub IsDuplicate_TimeBoundary_DoesNotExtendWindowForDuplicates()
            Dim now As Long = 1000
            Dim cache As New DriveEventDebouncer(Function() now, 1000)
            Assert.IsFalse(cache.IsDuplicate("E:\", DriveEventType.Added))
            now = 1200
            Assert.IsTrue(cache.IsDuplicate("E:\", DriveEventType.Added))
            now = 1201
            Assert.IsFalse(cache.IsDuplicate("E:\", DriveEventType.Added))
        End Sub

        <TestMethod>
        <DataRow(0, 1)>
        <DataRow(1, 0)>
        <DataRow(2, 3)>
        <DataRow(3, 2)>
        <DataRow(4, 5)>
        <DataRow(5, 4)>
        Public Sub IsDuplicate_OppositeTransition_AllowsOriginalAgain(firstValue As Integer, oppositeValue As Integer)
            Dim first As DriveEventType = CType(firstValue, DriveEventType)
            Dim opposite As DriveEventType = CType(oppositeValue, DriveEventType)
            Dim cache As New DriveEventDebouncer(Function() 1000L, 1000)
            Assert.IsFalse(cache.IsDuplicate("E:\", first))
            Assert.IsFalse(cache.IsDuplicate("E:\", opposite))
            Assert.IsFalse(cache.IsDuplicate("E:\", first))
            Assert.IsTrue(cache.IsDuplicate("E:\", first))
        End Sub

        <TestMethod>
        Public Sub IsDuplicate_ConcurrentCalls_AdmitsOnlyOne()
            Dim cache As New DriveEventDebouncer(Function() 1000L, 1000)
            Dim admitted As Integer = 0
            Parallel.For(0, 100, Sub(index)
                                     If Not cache.IsDuplicate("E:\", DriveEventType.Added) Then Interlocked.Increment(admitted)
                                 End Sub)
            Assert.AreEqual(1, admitted)
            cache.Clear()
            Assert.IsFalse(cache.IsDuplicate("E:\", DriveEventType.Added))
        End Sub
    End Class
End Namespace