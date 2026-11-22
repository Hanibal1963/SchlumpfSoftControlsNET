' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherThreadingTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    <STATestClass>
    Public Class DriveWatcherThreadingTests

        <TestMethod>
        Public Sub WithoutSynchronizingObject_EventsRunSynchronouslyOnCallingThread()
            Using watcher As New TestDriveWatcher()
                Assert.IsNull(watcher.SynchronizingObject)
                Dim testThread As Integer = Environment.CurrentManagedThreadId
                Dim sourceThread As Integer = 0
                Dim handlerThread As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) handlerThread = Environment.CurrentManagedThreadId

                Dim work As Task = Task.Run(Sub()
                                                sourceThread = Environment.CurrentManagedThreadId
                                                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
                                            End Sub)
                Assert.IsTrue(work.Wait(TimeSpan.FromSeconds(5)))
                Assert.AreNotEqual(testThread, sourceThread)
                Assert.AreEqual(sourceThread, handlerThread)
            End Using
        End Sub

        <TestMethod>
        Public Sub SynchronizingObject_WhenInvokeIsNotRequired_RaisesSynchronously()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(False)
                watcher.SynchronizingObject = target
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1

                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))

                Assert.AreEqual(1, calls)
                Assert.AreEqual(0, target.PendingCount)
                Assert.AreSame(target, watcher.SynchronizingObject)
                watcher.SynchronizingObject = Nothing
                Assert.IsNull(watcher.SynchronizingObject)
            End Using
        End Sub

        <TestMethod>
        Public Sub SynchronizingObject_QueuesAllEventsWithOriginalSenderAndArguments()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(True)
                watcher.SynchronizingObject = target
                Dim added As New DriveWatcherAddedEventArgs("C:\", DriveType.Fixed, True)
                Dim removed As New DriveWatcherRemovedEventArgs("C:\")
                Dim received As New List(Of String)()
                AddHandler watcher.DriveAdded, Sub(sender, e) RecordEvent(received, "DriveAdded", watcher, sender, added, e)
                AddHandler watcher.DriveRemoved, Sub(sender, e) RecordEvent(received, "DriveRemoved", watcher, sender, removed, e)
                AddHandler watcher.MediaInserted, Sub(sender, e) RecordEvent(received, "MediaInserted", watcher, sender, added, e)
                AddHandler watcher.MediaRemoved, Sub(sender, e) RecordEvent(received, "MediaRemoved", watcher, sender, removed, e)
                AddHandler watcher.NetworkDriveAdded, Sub(sender, e) RecordEvent(received, "NetworkDriveAdded", watcher, sender, added, e)
                AddHandler watcher.NetworkDriveRemoved, Sub(sender, e) RecordEvent(received, "NetworkDriveRemoved", watcher, sender, removed, e)

                watcher.EmitAll(added, removed)

                Assert.AreEqual(6, target.PendingCount)
                Assert.IsEmpty(received)
                target.ExecutePending()
                Assert.HasCount(6, received)
                Assert.AreEqual("DriveAdded,DriveRemoved,MediaInserted,MediaRemoved,NetworkDriveAdded,NetworkDriveRemoved", String.Join(",", received))
            End Using
        End Sub

        <TestMethod>
        Public Sub ChangingSynchronizingObject_DoesNotMoveAlreadyQueuedEvents()
            Using watcher As New TestDriveWatcher()
                Dim first As New QueuedSynchronizer(True)
                Dim second As New QueuedSynchronizer(True)
                Dim received As New List(Of String)()
                AddHandler watcher.DriveRemoved, Sub(sender, e) received.Add("DriveRemoved")
                AddHandler watcher.MediaRemoved, Sub(sender, e) received.Add("MediaRemoved")
                watcher.SynchronizingObject = first
                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
                watcher.SynchronizingObject = second
                watcher.EmitMediaRemoved(New DriveWatcherRemovedEventArgs("C:\"))

                Assert.AreEqual(1, first.PendingCount)
                Assert.AreEqual(1, second.PendingCount)
                second.ExecutePending()
                first.ExecutePending()
                Assert.AreEqual("MediaRemoved,DriveRemoved", String.Join(",", received))
            End Using
        End Sub

        <TestMethod>
        Public Sub QueuedEvent_UsesHandlersPresentWhenItIsDelivered()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(True)
                watcher.SynchronizingObject = target
                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1

                target.ExecutePending()

                Assert.AreEqual(1, calls)
            End Using
        End Sub

        <TestMethod>
        Public Sub SynchronizingObject_DebouncesBeforeQueueing()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(True)
                watcher.SynchronizingObject = target
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1

                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))

                Assert.AreEqual(1, target.PendingCount)
                target.ExecutePending()
                Assert.AreEqual(1, calls)
            End Using
        End Sub

        <TestMethod>
        Public Sub Dispose_CancelsQueuedEventsAndReleasesSynchronizingObject()
            Dim watcher As New TestDriveWatcher()
            Dim target As New QueuedSynchronizer(True)
            watcher.SynchronizingObject = target
            Dim received As New List(Of String)()
            AddHandler watcher.DriveRemoved, Sub(sender, e) received.Add("DriveRemoved")
            watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
            Assert.AreEqual(1, target.PendingCount)

            watcher.Dispose()
            target.ExecutePending()

            Assert.IsEmpty(received)
            Assert.IsNull(watcher.SynchronizingObject)
            Dim unused = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() watcher.SynchronizingObject = target)
        End Sub

        <TestMethod>
        Public Sub Dispose_DoesNotDisposeTheSynchronizingControl()
            Using watcher As New TestDriveWatcher(), target As New Control()
                Dim handle As IntPtr = target.Handle
                watcher.SynchronizingObject = target

                watcher.Dispose()

                Assert.AreNotEqual(IntPtr.Zero, handle)
                Assert.IsFalse(target.IsDisposed)
                Assert.IsTrue(target.IsHandleCreated)
                Assert.AreEqual(handle, target.Handle)
                Assert.IsNull(watcher.SynchronizingObject)
            End Using
        End Sub

        <TestMethod>
        Public Sub DisposeInQueuedHandler_CancelsSubsequentQueuedEvents()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(True)
                watcher.SynchronizingObject = target
                Dim received As New List(Of String)()
                AddHandler watcher.DriveRemoved,
                    Sub(sender, e)
                        received.Add("DriveRemoved")
                        watcher.Dispose()
                    End Sub
                AddHandler watcher.MediaRemoved, Sub(sender, e) received.Add("MediaRemoved")
                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))
                watcher.EmitMediaRemoved(New DriveWatcherRemovedEventArgs("C:\"))

                target.ExecutePending()

                Assert.HasCount(1, received)
                Assert.AreEqual("DriveRemoved", received(0))
            End Using
        End Sub

        <TestMethod>
        Public Sub SynchronizingControlWithoutHandle_ThrowsWithoutCallingHandler()
            Using watcher As New TestDriveWatcher(), target As New Control()
                watcher.SynchronizingObject = target
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1
                Dim unused = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\")))
                Assert.AreEqual(0, calls)
                Assert.IsFalse(target.IsHandleCreated)
            End Using
        End Sub

        <TestMethod>
        Public Sub DisposedSynchronizingControl_ThrowsWithoutCallingHandler()
            Using watcher As New TestDriveWatcher(), target As New Control()
                Dim handle As IntPtr = target.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                watcher.SynchronizingObject = target
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1
                target.Dispose()
                Dim unused = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\")))
                Assert.AreEqual(0, calls)
            End Using
        End Sub

        <TestMethod>
        Public Sub SynchronizingObject_WhenSchedulingFails_PropagatesException()
            Using watcher As New TestDriveWatcher()
                Dim expected As New InvalidOperationException("Scheduling failed.")
                Dim target As New QueuedSynchronizer(True) With {.BeginInvokeError = expected}
                watcher.SynchronizingObject = target
                Dim calls As Integer = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) calls += 1

                Dim actual As InvalidOperationException = Assert.ThrowsExactly(Of InvalidOperationException)(
                    Sub() watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\")))

                Assert.AreSame(expected, actual)
                Assert.AreEqual(0, calls)
                Assert.AreEqual(0, target.PendingCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub QueuedHandler_WhenItThrows_DoesNotSuppressTheException()
            Using watcher As New TestDriveWatcher()
                Dim target As New QueuedSynchronizer(True)
                watcher.SynchronizingObject = target
                Dim expected As New InvalidOperationException("Handler failed.")
                AddHandler watcher.DriveRemoved, Sub(sender, e) Throw expected
                watcher.EmitDriveRemoved(New DriveWatcherRemovedEventArgs("C:\"))

                Dim actual As InvalidOperationException = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() target.ExecutePending())
                Assert.AreSame(expected, actual)
            End Using
        End Sub

        Private Shared Sub RecordEvent(received As List(Of String), name As String, expectedSender As Object, actualSender As Object, expectedArgs As Object, actualArgs As Object)
            Assert.AreSame(expectedSender, actualSender)
            Assert.AreSame(expectedArgs, actualArgs)
            received.Add(name)
        End Sub

        Private NotInheritable Class TestDriveWatcher
            Inherits DriveWatcher

            Public Sub EmitDriveRemoved(e As DriveWatcherRemovedEventArgs)
                Me.OnDriveRemoved(e)
            End Sub

            Public Sub EmitMediaRemoved(e As DriveWatcherRemovedEventArgs)
                Me.OnMediaRemoved(e)
            End Sub

            Public Sub EmitAll(added As DriveWatcherAddedEventArgs, removed As DriveWatcherRemovedEventArgs)
                Me.OnDriveAdded(added)
                Me.OnDriveRemoved(removed)
                Me.OnMediaInserted(added)
                Me.OnMediaRemoved(removed)
                Me.OnNetworkDriveAdded(added)
                Me.OnNetworkDriveRemoved(removed)
            End Sub
        End Class

        Private NotInheritable Class QueuedSynchronizer
            Implements ISynchronizeInvoke

            Private ReadOnly _pending As New Queue(Of Action)()

            Public Sub New(requiresInvoke As Boolean)
                InvokeRequired = requiresInvoke
            End Sub

            Public Property BeginInvokeError As Exception

            Public ReadOnly Property PendingCount As Integer
                Get
                    Return Me._pending.Count
                End Get
            End Property

            Public ReadOnly Property InvokeRequired As Boolean Implements ISynchronizeInvoke.InvokeRequired

            Public Function BeginInvoke(method As [Delegate], args As Object()) As IAsyncResult Implements ISynchronizeInvoke.BeginInvoke
                If Me.BeginInvokeError IsNot Nothing Then Throw Me.BeginInvokeError
                Me._pending.Enqueue(DirectCast(method, Action))
                Return Task.CompletedTask
            End Function

            Public Function EndInvoke(result As IAsyncResult) As Object Implements ISynchronizeInvoke.EndInvoke
                Throw New NotSupportedException()
            End Function

            Public Function Invoke(method As [Delegate], args As Object()) As Object Implements ISynchronizeInvoke.Invoke
                Throw New NotSupportedException()
            End Function

            Public Sub ExecutePending()
                While Me._pending.Count > 0
                    Me._pending.Dequeue().Invoke()
                End While
            End Sub
        End Class
    End Class
End Namespace