' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherPollingTests.vb
' Author: Andreas Sauer
' Datum: 25.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    ''' <summary>Prüft die Polling-Zustandswechsel ohne reale Geräteänderungen.</summary>
    <TestClass>
    Public NotInheritable Class DriveWatcherPollingTests

        ''' <summary>Prüft Ausgangszustand, Laufwerke, Medien und Netzlaufwerke.</summary>
        <TestMethod>
        Public Sub Polling_ReportsChangesOnceAndUsesInitialState()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = {New DriveWatcherAddedEventArgs("E:\", DriveType.CDRom, False)}
            Using watcher As DriveWatcher = CreateWatcher(Function() drives)
                Dim events As New List(Of String)()
                AddHandler watcher.DriveAdded, Sub(sender, e) events.Add("add:" & e.DriveName)
                AddHandler watcher.DriveRemoved, Sub(sender, e) events.Add("remove:" & e.DriveName)
                AddHandler watcher.MediaInserted, Sub(sender, e) events.Add("insert:" & e.DriveName)
                AddHandler watcher.MediaRemoved, Sub(sender, e) events.Add("eject:" & e.DriveName)
                AddHandler watcher.NetworkDriveAdded, Sub(sender, e) events.Add("network+:" & e.DriveName)
                AddHandler watcher.NetworkDriveRemoved, Sub(sender, e) events.Add("network-:" & e.DriveName)
                watcher.PollingEnabled = True
                Assert.IsEmpty(events)
                drives = {New DriveWatcherAddedEventArgs("e:\", DriveType.CDRom, True), New DriveWatcherAddedEventArgs("Z:\", DriveType.Network, True), New DriveWatcherAddedEventArgs("F:\", DriveType.Fixed, True)}
                watcher.PollNow()
                watcher.PollNow()
                CollectionAssert.AreEquivalent(New String() {"insert:e:\", "network+:Z:\", "add:F:\"}, events)
                drives = {New DriveWatcherAddedEventArgs("E:\", DriveType.CDRom, False)}
                watcher.PollNow()
                CollectionAssert.AreEquivalent(New String() {"insert:e:\", "network+:Z:\", "add:F:\", "eject:E:\", "network-:Z:\", "remove:F:\"}, events)
            End Using
        End Sub

        ''' <summary>Die reguläre Datenquelle kann einen echten Ausgangszustand erfassen und wieder stoppen.</summary>
        <TestMethod>
        Public Sub Polling_ProductionSourceCanStartAndStop()
            Using watcher As New DriveWatcher()
                watcher.PollingEnabled = True
                Assert.IsTrue(watcher.PollingEnabled)
                watcher.PollNow()
                watcher.PollingEnabled = False
                Assert.IsFalse(watcher.PollingEnabled)
            End Using
        End Sub

        ''' <summary>Ein Wechsel zwischen Netzwerk- und lokalem Laufwerk wird als Austausch gemeldet.</summary>
        <TestMethod>
        Public Sub Polling_NetworkReplacementReportsRemovalAndAddition()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = {New DriveWatcherAddedEventArgs("Z:\", DriveType.Network, True)}
            Using watcher As DriveWatcher = CreateWatcher(Function() drives)
                Dim events As New List(Of String)()
                AddHandler watcher.NetworkDriveRemoved, Sub(sender, e) events.Add("network-:" & e.DriveName)
                AddHandler watcher.DriveAdded, Sub(sender, e) events.Add("add:" & e.DriveName)
                watcher.PollingEnabled = True
                drives = {New DriveWatcherAddedEventArgs("Z:\", DriveType.Fixed, True)}
                watcher.PollNow()
                CollectionAssert.AreEqual(New String() {"network-:Z:\", "add:Z:\"}, events)
            End Using
        End Sub

        ''' <summary>Die Polling-Konfiguration darf nicht auf einen fremden Thread verschoben werden.</summary>
        <TestMethod>
        Public Sub Polling_ConfigurationRejectsOtherThread()
            Using watcher As New DriveWatcher()
                Dim errorValue As Exception = Nothing
                Dim worker As New Thread(Sub()
                                             Try
                                                 watcher.PollingEnabled = True
                                             Catch ex As InvalidOperationException
                                                 errorValue = ex
                                             End Try
                                         End Sub)
                worker.Start()
                worker.Join()
                Assert.IsInstanceOfType(Of InvalidOperationException)(errorValue)
                Assert.IsFalse(watcher.PollingEnabled)
            End Using
        End Sub

        ''' <summary>Der Windows-Forms-Timer löst den Vergleich auf dem Fensterthread aus.</summary>
        <TestMethod>
        Public Sub Polling_TimerAutomaticallyDetectsChange()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = Array.Empty(Of DriveWatcherAddedEventArgs)()
            Using watcher As DriveWatcher = CreateWatcher(Function() drives)
                Dim added As Int32 = 0
                Dim handlerThread As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e)
                                                   added += 1
                                                   handlerThread = Environment.CurrentManagedThreadId
                                               End Sub
                watcher.PollingInterval = 20
                watcher.PollingEnabled = True
                drives = {New DriveWatcherAddedEventArgs("F:\", DriveType.Fixed, True)}
                Dim clock = Stopwatch.StartNew()
                While added = 0 AndAlso clock.ElapsedMilliseconds < 5000
                    Application.DoEvents()
                    Thread.Sleep(10)
                End While
                Assert.AreEqual(1, added)
                Assert.AreEqual(Environment.CurrentManagedThreadId, handlerThread)
            End Using
        End Sub

        ''' <summary>Der Start schlägt sichtbar fehl, ohne einen Timer zu aktivieren.</summary>
        <TestMethod>
        Public Sub Polling_FailedInitialSnapshotLeavesModeDisabled()
            Dim source As Func(Of IReadOnlyList(Of DriveWatcherAddedEventArgs)) = Function()
                                                                                      Throw New IOException("initial failure")
                                                                                  End Function
            Using watcher As DriveWatcher = CreateWatcher(source)
                Dim errorValue = Assert.ThrowsExactly(Of IOException)(Sub() watcher.PollingEnabled = True)
                Assert.AreEqual("initial failure", errorValue.Message)
                Assert.IsFalse(watcher.PollingEnabled)
            End Using
        End Sub

        ''' <summary>Ein Handler darf das Polling während der Zustellung stoppen.</summary>
        <TestMethod>
        Public Sub Polling_HandlerCanStopFurtherNotifications()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = Array.Empty(Of DriveWatcherAddedEventArgs)()
            Using watcher As DriveWatcher = CreateWatcher(Function() drives)
                Dim added As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e)
                                                   added += 1
                                                   watcher.PollingEnabled = False
                                               End Sub
                watcher.PollingEnabled = True
                drives = {New DriveWatcherAddedEventArgs("E:\", DriveType.Fixed, True), New DriveWatcherAddedEventArgs("F:\", DriveType.Fixed, True)}
                watcher.PollNow()
                Assert.AreEqual(1, added)
                Assert.IsFalse(watcher.PollingEnabled)
            End Using
        End Sub

        ''' <summary>Lesefehler dürfen keinen erfundenen Medienwechsel erzeugen.</summary>
        <TestMethod>
        Public Sub Polling_ReadinessFailureRetainsLastKnownState()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = {New DriveWatcherAddedEventArgs("E:\", DriveType.Removable, True)}
            Using watcher As DriveWatcher = CreateWatcher(Function() drives)
                Dim removed As Int32 = 0
                AddHandler watcher.MediaRemoved, Sub(sender, e) removed += 1
                watcher.PollingEnabled = True
                Dim failed As New DriveWatcherAddedEventArgs("E:\", DriveType.Removable, False)
                failed.AddReadError("IsReady", New IOException("read failed"))
                drives = {failed}
                watcher.PollNow()
                Assert.AreEqual(0, removed)
                drives = {New DriveWatcherAddedEventArgs("E:\", DriveType.Removable, False)}
                watcher.PollNow()
                Assert.AreEqual(1, removed)
            End Using
        End Sub

        ''' <summary>Ein Fehler bei der Aufzählung verwirft den bisherigen Vergleichszustand nicht.</summary>
        <TestMethod>
        Public Sub Polling_EnumerationFailurePropagatesAndCanBeRetried()
            Dim fail As Boolean = False
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = {New DriveWatcherAddedEventArgs("E:\", DriveType.Fixed, True)}
            Dim source As Func(Of IReadOnlyList(Of DriveWatcherAddedEventArgs)) = Function()
                                                                                      If fail Then Throw New IOException("enumeration failed")
                                                                                      Return drives
                                                                                  End Function
            Using watcher As DriveWatcher = CreateWatcher(source)
                Dim removed As Int32 = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) removed += 1
                watcher.PollingEnabled = True
                fail = True
                Dim errorValue = Assert.ThrowsExactly(Of IOException)(Sub() watcher.PollNow())
                Assert.AreEqual("enumeration failed", errorValue.Message)
                fail = False
                drives = Array.Empty(Of DriveWatcherAddedEventArgs)()
                watcher.PollNow()
                Assert.AreEqual(1, removed)
            End Using
        End Sub

        ''' <summary>Prüft Filter, Konfiguration und erneuten Start ohne künstliche Ereignisse.</summary>
        <TestMethod>
        Public Sub Polling_FiltersRestartAndDisposeAreRespected()
            Dim drives As IReadOnlyList(Of DriveWatcherAddedEventArgs) = {New DriveWatcherAddedEventArgs("E:\", DriveType.Removable, True)}
            Dim watcher As DriveWatcher = CreateWatcher(Function() drives)
            Dim removed As Int32 = 0
            AddHandler watcher.DriveRemoved, Sub(sender, e) removed += 1
            Assert.IsFalse(watcher.PollingEnabled)
            Assert.AreEqual(2000, watcher.PollingInterval)
            Dim invalid = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() watcher.PollingInterval = 0)
            watcher.PollingInterval = 250
            watcher.IncludeDriveTypes = {DriveType.Removable}
            watcher.PollingEnabled = True
            drives = Array.Empty(Of DriveWatcherAddedEventArgs)()
            watcher.PollNow()
            Assert.AreEqual(1, removed)
            watcher.PollingEnabled = False
            drives = {New DriveWatcherAddedEventArgs("E:\", DriveType.Removable, True)}
            watcher.PollingEnabled = True
            watcher.PollNow()
            Assert.AreEqual(1, removed)
            watcher.Dispose()
            Assert.IsFalse(watcher.PollingEnabled)
            Dim disposed = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() watcher.PollingEnabled = True)
        End Sub

        ''' <summary>Erstellt eine Komponente mit kontrollierbarer Datenquelle.</summary>
        ''' <param name="source">Liefert die simulierten Laufwerksdaten.</param>
        ''' <returns>Die für Polling-Tests konfigurierte Komponente.</returns>
        Private Shared Function CreateWatcher(source As Func(Of IReadOnlyList(Of DriveWatcherAddedEventArgs))) As DriveWatcher
            Dim watcher As New DriveWatcher()
            watcher.SetPollingSource(source)
            Return watcher
        End Function
    End Class
End Namespace