' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherTraceTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    <STATestClass>
    <DoNotParallelize>
    Public NotInheritable Class DriveWatcherTraceTests
        <TestMethod>
        Public Sub TraceLevel_DefaultAndInvalidValues_AreHandledExplicitly()
            Using watcher As New TestWatcher()
                Assert.AreEqual(TraceLevel.Off, watcher.TraceLevel)
                Dim unused = Assert.ThrowsExactly(Of InvalidEnumArgumentException)(Sub() watcher.TraceLevel = CType(99, TraceLevel))
                Assert.AreEqual(TraceLevel.Off, watcher.TraceLevel)
                watcher.Dispose()
                Dim unused1 = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() watcher.TraceLevel = TraceLevel.Info)
            End Using
        End Sub

        <TestMethod>
        Public Sub TraceLevel_Info_ReportsAllEventsAndSuppressedDuplicates()
            Using listener As New RecordingListener(), watcher As New TestWatcher(), other As New TestWatcher()
                Dim unused = Trace.Listeners.Add(listener)
                Try
                    watcher.EmitAll()
                    Assert.IsEmpty(listener.Lines)
                    watcher.TraceLevel = TraceLevel.Info
                    other.EmitAll()
                    Assert.IsEmpty(listener.Lines)
                    watcher.EmitAll("F:\")
                    watcher.EmitAdded("F:\")
                    watcher.EmitAdded("F:\")
                    Assert.HasCount(8, listener.Lines)
                    For Each name In {"Added", "Removed", "MediaInserted", "MediaRemoved", "NetworkAdded", "NetworkRemoved"}
                        Assert.Contains("Event=" & name & " ", String.Join(vbLf, listener.Lines))
                    Next
                    Assert.Contains("Drive=F:\ Suppressed=True", listener.Lines(7))
                    Dim timestamp As String = listener.Lines(0).Split(" "c)(1).Substring("Timestamp=".Length)
                    Dim parsed As DateTimeOffset
                    Assert.IsTrue(DateTimeOffset.TryParseExact(timestamp, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, parsed))
                    Assert.AreEqual(TimeSpan.Zero, parsed.Offset)
                Finally
                    Trace.Listeners.Remove(listener)
                End Try
            End Using
        End Sub

        <TestMethod>
        <DataRow(0, False)>
        <DataRow(1, True)>
        <DataRow(2, True)>
        Public Sub TraceLevel_ReportsReadErrorsAccordingToThreshold(level As Integer, expected As Boolean)
            Using listener As New RecordingListener(), watcher As New TestWatcher()
                Dim unused = Trace.Listeners.Add(listener)
                Try
                    watcher.TraceLevel = CType(level, TraceLevel)
                    watcher.EmitAdded("E:\")
                    Assert.IsEmpty(listener.Lines)
                    Dim method = GetType(DriveWatcher).GetMethod("ReadDriveInfo", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Assert.IsNotNull(method)
                    Dim result = DirectCast(method.Invoke(watcher, New Object() {"", DriveWatcherEventType.Added}), DriveWatcherAddedEventArgs)
                    Assert.Contains("DriveInfo", result.ReadErrors.Keys)
                    If expected Then
                        Assert.HasCount(1, listener.Lines)
                        Assert.Contains("ReadError Event=Added Drive= Field=DriveInfo", listener.Lines(0))
                        Assert.Contains("Exception=System.ArgumentException", listener.Lines(0))
                    Else
                        Assert.IsEmpty(listener.Lines)
                    End If
                Finally
                    Trace.Listeners.Remove(listener)
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub TraceLevel_Verbose_ReportsNativeRawFieldsBeforeEvents()
            Using listener As New RecordingListener(), watcher As New TestWatcher()
                Dim unused = Trace.Listeners.Add(listener)
                Try
                    watcher.TraceLevel = TraceLevel.Verbose
                    Dim nativeProperty = GetType(DriveWatcher).GetProperty("NatForm", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim native = DirectCast(nativeProperty.GetValue(watcher), NativeWindow)
                    Dim volume As New BroadcastVolume With {.Size = Marshal.SizeOf(Of BroadcastVolume)(), .DeviceType = 2, .UnitMask = 4, .Flags = Short.MinValue}
                    Dim buffer As IntPtr = Marshal.AllocHGlobal(volume.Size)
                    Try
                        Marshal.StructureToPtr(volume, buffer, False)
                        Dim unused1 = SendMessage(native.Handle, &H219, New IntPtr(&H8004), buffer)
                    Finally
                        Marshal.FreeHGlobal(buffer)
                    End Try
                    Assert.HasCount(2, listener.Lines)
                    Assert.Contains("Native EventCode=0x8004 Drive=C:\ UnitMask=0x00000004 Flags=0x8000", listener.Lines(0))
                    Assert.Contains("Event=Removed Drive=C:\ Suppressed=False", listener.Lines(1))
                Finally
                    Trace.Listeners.Remove(listener)
                End Try
            End Using
        End Sub

        Private NotInheritable Class RecordingListener
            Inherits TraceListener
            Friend ReadOnly Lines As New List(Of String)()
            Public Overrides Sub Write(message As String)
            End Sub
            Public Overrides Sub WriteLine(message As String)
                If message.StartsWith("DriveWatcher Timestamp=", StringComparison.Ordinal) Then Me.Lines.Add(message)
            End Sub
        End Class

        Private NotInheritable Class TestWatcher
            Inherits DriveWatcher
            Friend Sub EmitAdded(name As String)
                Me.OnDriveAdded(New DriveWatcherAddedEventArgs(name))
            End Sub
            Friend Sub EmitAll(Optional name As String = "E:\")
                Me.OnDriveAdded(New DriveWatcherAddedEventArgs(name))
                Me.OnDriveRemoved(New DriveWatcherRemovedEventArgs(name))
                Me.OnMediaInserted(New DriveWatcherAddedEventArgs(name))
                Me.OnMediaRemoved(New DriveWatcherRemovedEventArgs(name))
                Me.OnNetworkDriveAdded(New DriveWatcherAddedEventArgs(name))
                Me.OnNetworkDriveRemoved(New DriveWatcherRemovedEventArgs(name))
            End Sub
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Private Structure BroadcastVolume
            Public Size As Integer
            Public DeviceType As Integer
            Public Reserved As Integer
            Public UnitMask As Integer
            Public Flags As Short
        End Structure

        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function SendMessage(handle As IntPtr, message As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function
    End Class
End Namespace