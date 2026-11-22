' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherLifecycleTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Reflection
Imports System.Runtime.ExceptionServices
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    <STATestClass>
    Public Class DriveWatcherLifecycleTests

        ''' <summary>Stellt das Abbruchtoken für wartende Testoperationen bereit.</summary>
        Public Property TestContext As TestContext

        Private Const WM_DEVICECHANGE As Int32 = &H219
        Private Const DBT_DEVICEARRIVAL As Int32 = &H8000
        Private Const DBT_DEVICEREMOVECOMPLETE As Int32 = &H8004
        Private Const DBT_DEVTYP_VOLUME As Int32 = 2
        Private Const DBTF_MEDIA As Int16 = 1
        Private Const DBTF_NET As Int16 = 2

        <TestMethod>
        Public Sub Dispose_ReleasesTheOwnedWindowAndCanBeRepeated()
            Dim watcher As New DriveWatcher()
            Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
            Dim handle As IntPtr = nativeWindow.Handle

            Try
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Assert.IsTrue(IsWindow(handle))
                Assert.AreSame(nativeWindow, NativeWindow.FromHandle(handle))
            Finally
                watcher.Dispose()
            End Try

            Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
            Assert.IsFalse(IsWindow(handle))
            Assert.IsNull(NativeWindow.FromHandle(handle))
            watcher.Dispose()
            Assert.IsFalse(IsWindow(handle))
        End Sub

        <TestMethod>
        Public Sub ShortHeader_DoesNotReadVolumeOrRaiseEvents()
            Using watcher As New CountingDriveWatcher()
                Dim buffer As IntPtr = Marshal.AllocHGlobal(4)
                Try
                    Marshal.WriteInt32(buffer, 4)
                    Dim unused = SendMessage(GetNativeWindow(watcher).Handle, WM_DEVICECHANGE, New IntPtr(DBT_DEVICEARRIVAL), buffer)
                    Assert.AreEqual(0, watcher.AdditionDispatches)
                Finally
                    Marshal.FreeHGlobal(buffer)
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub DerivedWatcher_DisposeAfterConstruction_InvokesOverrideAndReleasesWindow()
            Dim watcher As New DerivedDriveWatcher(Nothing)
            Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
            Dim handle As IntPtr = nativeWindow.Handle

            watcher.Dispose()

            Assert.AreEqual(1, watcher.DisposeCalls)
            Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
            Assert.IsFalse(IsWindow(handle))
        End Sub

        <TestMethod>
        Public Sub Constructor_WithNothingContainer_CreatesAndReleasesAWindow()
            Dim watcher As New DriveWatcher(Nothing)
            Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
            Dim handle As IntPtr = nativeWindow.Handle

            Try
                Assert.IsTrue(IsWindow(handle))
            Finally
                watcher.Dispose()
            End Try

            Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
            Assert.IsFalse(IsWindow(handle))
        End Sub

        <TestMethod>
        Public Sub CreateHandle_RejectsASecondHandleForTheSameWindow()
            Using watcher As New DriveWatcher()
                Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
                Dim originalHandle As IntPtr = nativeWindow.Handle

                Dim unused = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() nativeWindow.CreateHandle(New CreateParams()))

                Assert.AreEqual(originalHandle, nativeWindow.Handle)
                Assert.IsTrue(IsWindow(originalHandle))
                Assert.AreSame(nativeWindow, NativeWindow.FromHandle(originalHandle))
            End Using
        End Sub

        <TestMethod>
        Public Sub CreateHandle_AfterDispose_DoesNotRegisterAnotherWindow()
            Dim watcher As New DriveWatcher()
            Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
            watcher.Dispose()

            Try
                Dim unused = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() nativeWindow.CreateHandle(New CreateParams()))
                Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
            Finally
                If nativeWindow.Handle <> IntPtr.Zero Then nativeWindow.DestroyHandle()
            End Try
        End Sub

        <TestMethod>
        <DataRow(False)>
        <DataRow(True)>
        Public Sub Constructor_WhenContainerRegistrationFails_ReleasesTheWindow(partiallyRegister As Boolean)
            Using container As New RejectingContainer(partiallyRegister)
                Try
                    Dim exception As InvalidOperationException = Assert.ThrowsExactly(Of InvalidOperationException)(
                        Sub()
                            Dim watcher As New DriveWatcher(container)
                        End Sub)

                    Assert.AreEqual("Container registration failed.", exception.Message)
                    Assert.IsNotNull(container.CapturedWindow)
                    Assert.AreNotEqual(IntPtr.Zero, container.CapturedHandle)
                    Assert.AreEqual(IntPtr.Zero, container.CapturedWindow.Handle)
                    Assert.IsFalse(IsWindow(container.CapturedHandle))
                    Assert.IsNull(NativeWindow.FromHandle(container.CapturedHandle))
                    Assert.IsEmpty(container.Components)
                Finally
                    If container.CapturedWatcher IsNot Nothing Then container.CapturedWatcher.Dispose()
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub ConstructorFailure_DoesNotInvokeDisposeOnUnconstructedDerivedInstance()
            Using container As New RejectingContainer(False)
                Try
                    Dim exception As InvalidOperationException = Assert.ThrowsExactly(Of InvalidOperationException)(
                        Sub()
                            Dim watcher As New DerivedDriveWatcher(container)
                        End Sub)

                    Assert.AreEqual("Container registration failed.", exception.Message)
                    Assert.IsNotNull(container.CapturedWindow)
                    Assert.AreEqual(IntPtr.Zero, container.CapturedWindow.Handle)
                    Assert.IsFalse(IsWindow(container.CapturedHandle))
                Finally
                    If container.CapturedWindow IsNot Nothing AndAlso container.CapturedWindow.Handle <> IntPtr.Zero Then
                        container.CapturedWindow.DestroyHandle()
                    End If
                End Try
            End Using
        End Sub

        <TestMethod>
        <DataRow(0, 0, 0)>
        <DataRow(1, 1, 0)>
        <DataRow(2, 0, 1)>
        Public Sub VolumeArrival_ForwardsEachApplicableEventOnce(flags As Int32, expectedMedia As Int32, expectedNetwork As Int32)
            Using watcher As New CountingDriveWatcher()
                Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
                Dim systemRoot As String = IO.Path.GetPathRoot(Environment.SystemDirectory)
                Assert.IsNotNull(systemRoot)
                Dim driveIndex As Int32 = AscW(Char.ToUpperInvariant(systemRoot(0))) - AscW("A"c)
                Assert.IsInRange(0, 25, driveIndex)
                Dim driveEvents As Int32 = 0
                Dim mediaEvents As Int32 = 0
                Dim networkEvents As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e) driveEvents += 1
                AddHandler watcher.MediaInserted, Sub(sender, e) mediaEvents += 1
                AddHandler watcher.NetworkDriveAdded, Sub(sender, e) networkEvents += 1

                SendVolumeChange(nativeWindow.Handle, DBT_DEVICEARRIVAL, 1 << driveIndex, CShort(flags))

                Assert.AreEqual(1, watcher.AdditionDispatches)
                Assert.AreEqual(1, driveEvents)
                Assert.AreEqual(expectedMedia, mediaEvents)
                Assert.AreEqual(expectedNetwork, networkEvents)
            End Using
        End Sub

        <TestMethod>
        Public Sub ContainerDispose_ReleasesOnlyItsWatcherAndEachMessageIsHandledOnce()
            Using container As New Container(), otherWatcher As New CountingDriveWatcher()
                Dim ownedWatcher As New CountingDriveWatcher(container)
                Dim ownedWindow As NativeWindow = GetNativeWindow(ownedWatcher)
                Dim otherWindow As NativeWindow = GetNativeWindow(otherWatcher)
                Dim ownedHandle As IntPtr = ownedWindow.Handle
                Dim otherHandle As IntPtr = otherWindow.Handle
                Dim ownedMediaEvents As Int32 = 0
                Dim otherNetworkEvents As Int32 = 0
                AddHandler ownedWatcher.MediaRemoved, Sub(sender, e) ownedMediaEvents += 1
                AddHandler otherWatcher.NetworkDriveRemoved, Sub(sender, e) otherNetworkEvents += 1

                Assert.HasCount(1, container.Components)
                Assert.AreNotEqual(ownedHandle, otherHandle)
                SendVolumeRemoval(ownedHandle, 4, DBTF_MEDIA)
                SendVolumeRemoval(otherHandle, 4, DBTF_NET)
                Assert.AreEqual(1, ownedWatcher.RemovalDispatches)
                Assert.AreEqual(1, ownedMediaEvents)
                Assert.AreEqual(1, otherWatcher.RemovalDispatches)
                Assert.AreEqual(1, otherNetworkEvents)

                container.Dispose()

                Assert.AreEqual(IntPtr.Zero, ownedWindow.Handle)
                Assert.IsFalse(IsWindow(ownedHandle))
                Assert.IsTrue(IsWindow(otherHandle))
                SendVolumeRemoval(otherHandle, 8, DBTF_NET)
                Assert.AreEqual(2, otherWatcher.RemovalDispatches)
                Assert.AreEqual(2, otherNetworkEvents)
            End Using
        End Sub

        <TestMethod>
        Public Sub DisposeInsideEvent_DetachesHandlersBeforeFollowUpEvents()
            Using watcher As New CountingDriveWatcher()
                Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
                Dim handle As IntPtr = nativeWindow.Handle
                Dim removedEvents As Int32 = 0
                Dim mediaEvents As Int32 = 0
                AddHandler watcher.DriveRemoved,
                    Sub(sender, e)
                        removedEvents += 1
                        watcher.Dispose()
                    End Sub
                AddHandler watcher.MediaRemoved, Sub(sender, e) mediaEvents += 1

                SendVolumeRemoval(handle, 4, DBTF_MEDIA)

                Assert.AreEqual(1, watcher.RemovalDispatches)
                Assert.AreEqual(1, removedEvents)
                Assert.AreEqual(0, mediaEvents)
                Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
                Assert.IsFalse(IsWindow(handle))
            End Using
        End Sub

        <TestMethod>
        <DataRow(False)>
        <DataRow(True)>
        Public Sub NativeDeviceMessage_RaisesEventOnExpectedThread(useSynchronizingControl As Boolean)
            Using target As New Control(), ready As New ManualResetEventSlim(), stopWorker As New ManualResetEventSlim(), received As New ManualResetEventSlim()
                Dim targetHandle As IntPtr = target.Handle
                Dim targetThread As Int32 = Environment.CurrentManagedThreadId
                Dim sourceThread As Int32 = 0
                Dim handlerThread As Int32 = 0
                Dim sourceHandle As IntPtr = IntPtr.Zero
                Dim eventSender As Object = Nothing
                Dim expectedSender As DriveWatcher = Nothing
                Dim driveName As String = Nothing
                Dim workerError As Exception = Nothing

                Dim worker As New Thread(Sub()
                                             Try
                                                 Using watcher As New DriveWatcher()
                                                     expectedSender = watcher
                                                     If useSynchronizingControl Then watcher.SynchronizingObject = target
                                                     AddHandler watcher.DriveRemoved,
                                                         Sub(sender, e)
                                                             eventSender = sender
                                                             driveName = e.DriveName
                                                             handlerThread = Environment.CurrentManagedThreadId
                                                             received.Set()
                                                         End Sub
                                                     sourceThread = Environment.CurrentManagedThreadId
                                                     sourceHandle = GetNativeWindow(watcher).Handle
                                                     ready.Set()
                                                     While Not stopWorker.Wait(5, Me.TestContext.CancellationToken)
                                                         Application.DoEvents()
                                                     End While
                                                 End Using
                                             Catch ex As Exception
                                                 workerError = ex
                                             Finally
                                                 ready.Set()
                                             End Try
                                         End Sub) With {
                    .IsBackground = True
                                         }
                worker.SetApartmentState(ApartmentState.STA)
                worker.Start()

                Dim senderTask As Task = Nothing
                Dim workerStopped As Boolean = False
                Try
                    Assert.AreNotEqual(IntPtr.Zero, targetHandle)
                    Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(5), Me.TestContext.CancellationToken))
                    If workerError IsNot Nothing Then ExceptionDispatchInfo.Capture(workerError).Throw()
                    Assert.AreNotEqual(IntPtr.Zero, sourceHandle)
                    Assert.AreNotEqual(targetThread, sourceThread)

                    senderTask = Task.Run(Sub() SendVolumeRemoval(sourceHandle, 4, DBTF_MEDIA), Me.TestContext.CancellationToken)
                    Assert.IsTrue(senderTask.Wait(TimeSpan.FromSeconds(5), Me.TestContext.CancellationToken), "Die native Nachricht muss ohne Warten auf den Zielthread zurückkehren.")

                    Dim timer As Stopwatch = Stopwatch.StartNew()
                    While Not received.IsSet AndAlso timer.Elapsed < TimeSpan.FromSeconds(5)
                        Application.DoEvents()
                        Dim unused = Thread.Yield()
                    End While

                    Assert.IsTrue(received.IsSet, "Das Ereignis wurde nicht auf dem Zielthread zugestellt.")
                    Assert.AreEqual(If(useSynchronizingControl, targetThread, sourceThread), handlerThread)
                    Assert.AreSame(expectedSender, eventSender)
                    Assert.AreEqual("C:\", driveName)
                Finally
                    stopWorker.Set()
                    If senderTask IsNot Nothing AndAlso Not senderTask.IsCompleted Then Application.DoEvents()
                    workerStopped = worker.Join(TimeSpan.FromSeconds(5))
                End Try

                If Not workerStopped Then Throw New TimeoutException("Der Empfangsthread wurde nicht beendet.")
                If workerError IsNot Nothing Then ExceptionDispatchInfo.Capture(workerError).Throw()
            End Using
        End Sub

        <TestMethod>
        Public Sub RepeatedCreationAndDisposal_DoesNotLeaveWindowsBehind()
            For iteration As Int32 = 1 To 32
                Dim watcher As New DriveWatcher()
                Dim nativeWindow As NativeWindow = GetNativeWindow(watcher)
                Dim handle As IntPtr = nativeWindow.Handle

                Try
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Assert.IsTrue(IsWindow(handle))
                Finally
                    watcher.Dispose()
                End Try

                Assert.AreEqual(IntPtr.Zero, nativeWindow.Handle)
                Assert.IsFalse(IsWindow(handle))
                Assert.IsNull(NativeWindow.FromHandle(handle))
            Next
        End Sub

        Private Shared Function GetNativeWindow(watcher As DriveWatcher) As NativeWindow
            Dim nativeFormProperty As PropertyInfo = GetType(DriveWatcher).GetProperty("NatForm", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(nativeFormProperty)
            Dim nativeWindow As NativeWindow = DirectCast(nativeFormProperty.GetValue(watcher), NativeWindow)
            Assert.IsNotNull(nativeWindow)
            Return nativeWindow
        End Function

        <TestMethod>
        <DataRow(0, 2, 1)>
        <DataRow(12, 2, 1)>
        <DataRow(20, 0, 1)>
        <DataRow(20, 2, 0)>
        Public Sub InvalidBroadcast_DoesNotRaiseEvents(size As Int32, deviceType As Int32, mask As Int32)
            Using watcher As New CountingDriveWatcher()
                Dim volume As New DeviceBroadcastVolume With {.Size = size, .DeviceType = deviceType, .UnitMask = mask}
                Dim buffer As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Of DeviceBroadcastVolume)())
                Try
                    Marshal.StructureToPtr(volume, buffer, False)
                    Dim unused = SendMessage(GetNativeWindow(watcher).Handle, WM_DEVICECHANGE, New IntPtr(DBT_DEVICEARRIVAL), buffer)
                    Assert.AreEqual(0, watcher.AdditionDispatches)
                    Assert.AreEqual(0, watcher.RemovalDispatches)
                Finally
                    Marshal.FreeHGlobal(buffer)
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub NullOrUnrelatedBroadcast_DoesNotRaiseEvents()
            Using watcher As New CountingDriveWatcher()
                Dim handle As IntPtr = GetNativeWindow(watcher).Handle
                Dim unused = SendMessage(handle, WM_DEVICECHANGE, New IntPtr(DBT_DEVICEARRIVAL), IntPtr.Zero)
                unused = SendMessage(handle, WM_DEVICECHANGE, New IntPtr(7), IntPtr.Zero)
                Assert.AreEqual(0, watcher.AdditionDispatches)
                Assert.AreEqual(0, watcher.RemovalDispatches)
            End Using
        End Sub

        Private Shared Sub SendVolumeRemoval(handle As IntPtr, unitMask As Int32, flags As Int16)
            SendVolumeChange(handle, DBT_DEVICEREMOVECOMPLETE, unitMask, flags)
        End Sub

        Private Shared Sub SendVolumeChange(handle As IntPtr, eventType As Int32, unitMask As Int32, flags As Int16)
            Dim volume As New DeviceBroadcastVolume With {
                .Size = Marshal.SizeOf(Of DeviceBroadcastVolume)(),
                .DeviceType = DBT_DEVTYP_VOLUME,
                .UnitMask = unitMask,
                .Flags = flags
            }
            Dim buffer As IntPtr = Marshal.AllocHGlobal(volume.Size)

            Try
                ' Eine echte Fensternachricht macht den Test unabhängig von angeschlossener Hardware.
                Marshal.StructureToPtr(volume, buffer, False)
                Dim unused = SendMessage(handle, WM_DEVICECHANGE, New IntPtr(eventType), buffer)
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try
        End Sub

        <StructLayout(LayoutKind.Sequential)>
        Private Structure DeviceBroadcastVolume
            Public Size As Int32
            Public DeviceType As Int32
            Public Reserved As Int32
            Public UnitMask As Int32
            Public Flags As Int16
        End Structure

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function IsWindow(handle As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function SendMessage(handle As IntPtr, message As Int32, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        Private NotInheritable Class CountingDriveWatcher

            Inherits DriveWatcher

            Private _additionDispatches As Int32
            Private _removalDispatches As Int32

            Public Sub New()
                MyBase.New()
            End Sub

            Public Sub New(container As IContainer)
                MyBase.New(container)
            End Sub

            Friend ReadOnly Property AdditionDispatches As Int32
                Get
                    Return Me._additionDispatches
                End Get
            End Property

            Friend ReadOnly Property RemovalDispatches As Int32
                Get
                    Return Me._removalDispatches
                End Get
            End Property

            Protected Overrides Sub OnDriveAdded(e As DriveWatcherAddedEventArgs)
                Me._additionDispatches += 1
                MyBase.OnDriveAdded(e)
            End Sub

            Protected Overrides Sub OnDriveRemoved(e As DriveWatcherRemovedEventArgs)
                Me._removalDispatches += 1
                MyBase.OnDriveRemoved(e)
            End Sub

        End Class

        Private NotInheritable Class RejectingContainer

            Inherits Container

            Private ReadOnly _partiallyRegister As Boolean

            Public Sub New(partiallyRegister As Boolean)
                Me._partiallyRegister = partiallyRegister
            End Sub

            Public Property CapturedWatcher As DriveWatcher

            Public Property CapturedWindow As NativeWindow

            Public Property CapturedHandle As IntPtr

            Public Overrides Sub Add(component As IComponent)
                Me.CapturedWatcher = DirectCast(component, DriveWatcher)
                Me.CapturedWindow = GetNativeWindow(Me.CapturedWatcher)
                Me.CapturedHandle = Me.CapturedWindow.Handle
                If Me._partiallyRegister Then MyBase.Add(component)
                Throw New InvalidOperationException("Container registration failed.")
            End Sub

        End Class

        Private NotInheritable Class DerivedDriveWatcher

            Inherits DriveWatcher

            Private ReadOnly _initialized As Boolean
            Private _disposeCalls As Int32

            Public Sub New(container As IContainer)
                MyBase.New(container)
                Me._initialized = True
            End Sub

            Friend ReadOnly Property DisposeCalls As Int32
                Get
                    Return Me._disposeCalls
                End Get
            End Property

            Protected Overrides Sub Dispose(disposing As Boolean)

                If disposing AndAlso Not Me._initialized Then
                    Throw New InvalidOperationException("Derived component is not initialized.")
                End If
                Me._disposeCalls += 1
                MyBase.Dispose(disposing)

            End Sub

        End Class

    End Class

End Namespace