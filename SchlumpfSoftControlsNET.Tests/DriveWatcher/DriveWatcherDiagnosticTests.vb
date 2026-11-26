' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherDiagnosticTests.vb
' Author: Andreas Sauer
' Datum: 26.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    ''' <summary>Prüft den Diagnosefeed über echte Nachrichten an das Empfangsfenster.</summary>
    <STATestClass>
    Public NotInheritable Class DriveWatcherDiagnosticTests

        ''' <summary>Prüft Standardeinstellung, nicht ausgewertete Codes und Lebensdauer.</summary>
        <TestMethod>
        Public Sub DiagnosticMode_DefaultDisabledAndOtherMessageIsReportedWhenEnabled()
            Using watcher As New DriveWatcher()
                Dim received As New List(Of DriveWatcherRawDeviceEventArgs)()
                AddHandler watcher.RawDeviceEventReceived, Sub(sender, e)
                                                               Assert.AreSame(watcher, sender)
                                                               received.Add(e)
                                                           End Sub

                Assert.IsFalse(watcher.DiagnosticMode)
                Send(watcher, &H7, IntPtr.Zero)
                Assert.IsEmpty(received)
                watcher.DiagnosticMode = True
                Dim before = DateTimeOffset.UtcNow
                Send(watcher, &H7, IntPtr.Zero)
                Dim after = DateTimeOffset.UtcNow
                Assert.HasCount(1, received)
                Assert.AreEqual(7L, received(0).EventCode)
                Assert.IsFalse(received(0).DeviceType.HasValue)
                Assert.IsFalse(received(0).UnitMask.HasValue)
                Assert.IsFalse(received(0).Flags.HasValue)
                Assert.AreEqual(TimeSpan.Zero, received(0).TimestampUtc.Offset)
                Assert.IsGreaterThanOrEqualTo(before, received(0).TimestampUtc)
                Assert.IsLessThanOrEqualTo(after, received(0).TimestampUtc)
                watcher.Dispose()
                Assert.IsFalse(watcher.DiagnosticMode)
                Dim unused = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() watcher.DiagnosticMode = True)
            End Using
        End Sub

        ''' <summary>Rohdaten werden vor Filtern, Entprellung und unabhängig vom Polling gemeldet.</summary>
        <TestMethod>
        Public Sub DiagnosticMode_RawEventPrecedesNormalEventAndIsNotDebounced()
            Using watcher As New DriveWatcher()
                Dim events As New List(Of String)()
                AddHandler watcher.RawDeviceEventReceived, Sub(sender, e) events.Add("raw")
                AddHandler watcher.DriveRemoved, Sub(sender, e) events.Add("removed")
                watcher.DiagnosticMode = True
                Dim volume As New BroadcastVolume With {.Size = Marshal.SizeOf(Of BroadcastVolume)(), .DeviceType = 2, .UnitMask = 4}
                SendVolume(watcher, &H8004, volume)
                SendVolume(watcher, &H8004, volume)
                Dim strvar As String() = New String() {"raw", "removed", "raw"}
                CollectionAssert.AreEqual(strvar, events)
            End Using
        End Sub

        ''' <summary>Rohdaten werden vor Filtern und unabhängig vom Polling gemeldet.</summary>
        <TestMethod>
        Public Sub DiagnosticMode_VolumeDataBypassesFilterDebounceAndPolling()
            Using watcher As New DriveWatcher()
                Dim received As New List(Of DriveWatcherRawDeviceEventArgs)()
                Dim removed As Int32 = 0
                AddHandler watcher.RawDeviceEventReceived, Sub(sender, e) received.Add(e)
                AddHandler watcher.DriveRemoved, Sub(sender, e) removed += 1
                watcher.IncludeLetters = {"F"c}
                watcher.DiagnosticMode = True
                Dim volume As New BroadcastVolume With {.Size = Marshal.SizeOf(Of BroadcastVolume)(), .DeviceType = 2, .UnitMask = 4, .Flags = Int16.MinValue}
                SendVolume(watcher, &H8004, volume)
                SendVolume(watcher, &H8004, volume)
                Assert.HasCount(2, received)
                Assert.AreEqual(0, removed)
                Assert.AreEqual(&H8004L, received(0).EventCode)
                Assert.AreEqual(2, received(0).DeviceType.Value)
                Assert.AreEqual(4, received(0).UnitMask.Value)
                Assert.AreEqual(Int16.MinValue, received(0).Flags.Value)
                watcher.SetPollingSource(Function() Array.Empty(Of DriveWatcherAddedEventArgs)())
                watcher.PollingEnabled = True
                SendVolume(watcher, &H8004, volume)
                Assert.HasCount(3, received)
                Assert.AreEqual(0, removed)
            End Using
        End Sub

        ''' <summary>Unvollständige Strukturen liefern nur die jeweils sicher gelesenen Werte.</summary>
        <TestMethod>
        Public Sub DiagnosticMode_InvalidVolumeAndHeaderAreReportedWithoutInventingFields()
            Using watcher As New DriveWatcher()
                Dim received As New List(Of DriveWatcherRawDeviceEventArgs)()
                AddHandler watcher.RawDeviceEventReceived, Sub(sender, e) received.Add(e)
                watcher.DiagnosticMode = True
                Send(watcher, &H8004, IntPtr.Zero)
                Dim volume As New BroadcastVolume With {.Size = 12, .DeviceType = 2, .UnitMask = 4}
                SendVolume(watcher, &H8004, volume)
                volume.Size = Marshal.SizeOf(Of BroadcastVolume)()
                volume.UnitMask = 0
                SendVolume(watcher, &H8004, volume)
                volume.DeviceType = 5
                SendVolume(watcher, &H8004, volume)
                Assert.HasCount(4, received)
                Assert.IsFalse(received(0).DeviceType.HasValue)
                Assert.AreEqual(2, received(1).DeviceType.Value)
                Assert.IsFalse(received(1).UnitMask.HasValue)
                Assert.AreEqual(0, received(2).UnitMask.Value)
                Assert.AreEqual(CShort(0), received(2).Flags.Value)
                Assert.AreEqual(5, received(3).DeviceType.Value)
                Assert.IsFalse(received(3).Flags.HasValue)
            End Using
        End Sub

        ''' <summary>Asynchrone Zustellung behält kopierte Daten und wird bei Dispose verworfen.</summary>
        <TestMethod>
        Public Sub DiagnosticMode_QueuedEventsSurviveDisableButNotDispose()
            Using watcher As New DriveWatcher()
                Dim target As New QueuedSynchronizer()
                Dim received As New List(Of DriveWatcherRawDeviceEventArgs)()
                AddHandler watcher.RawDeviceEventReceived, Sub(sender, e) received.Add(e)
                watcher.SynchronizingObject = target
                watcher.DiagnosticMode = True
                Dim volume As New BroadcastVolume With {.Size = Marshal.SizeOf(Of BroadcastVolume)(), .DeviceType = 2, .UnitMask = 0, .Flags = 3}
                SendVolume(watcher, &H8004, volume)
                watcher.DiagnosticMode = False
                Assert.AreEqual(1, target.PendingCount)
                target.ExecutePending()
                Assert.HasCount(1, received)
                Assert.AreEqual(CShort(3), received(0).Flags.Value)
                watcher.DiagnosticMode = True
                Send(watcher, &H7, IntPtr.Zero)
                watcher.Dispose()
                target.ExecutePending()
                Assert.HasCount(1, received)
            End Using
        End Sub

        ''' <summary>Sendet eine Nachricht an das native Fenster der Komponente.</summary>
        ''' <param name="watcher">Die empfangende Komponente.</param>
        ''' <param name="eventCode">Windows-Ereigniscode.</param>
        ''' <param name="data">Optionaler nativer Datenzeiger.</param>
        Private Shared Sub Send(watcher As DriveWatcher, eventCode As Int32, data As IntPtr)
            Dim prop = GetType(DriveWatcher).GetProperty("NatForm", BindingFlags.NonPublic Or BindingFlags.Instance)
            Dim native = DirectCast(prop.GetValue(watcher), NativeWindow)
            Dim unused = SendMessage(native.Handle, &H219, New IntPtr(eventCode), data)
        End Sub

        ''' <summary>Stellt eine Volume-Struktur für die Dauer der synchronen Nachricht bereit.</summary>
        ''' <param name="watcher">Die empfangende Komponente.</param>
        ''' <param name="eventCode">Windows-Ereigniscode.</param>
        ''' <param name="volume">Kopierte native Volume-Daten.</param>
        Private Shared Sub SendVolume(watcher As DriveWatcher, eventCode As Int32, volume As BroadcastVolume)
            Dim buffer = Marshal.AllocHGlobal(Marshal.SizeOf(Of BroadcastVolume)())
            Try
                Marshal.StructureToPtr(volume, buffer, False)
                Send(watcher, eventCode, buffer)
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try
        End Sub

        ''' <summary>Testdarstellung des Windows-Volume-Broadcasts.</summary>
        <StructLayout(LayoutKind.Sequential)>
        Private Structure BroadcastVolume
            ''' <summary>Größe der gemeldeten Struktur.</summary>
            Public Size As Int32
            ''' <summary>Windows-Gerätetyp.</summary>
            Public DeviceType As Int32
            ''' <summary>Reserviertes Feld.</summary>
            Public Reserved As Int32
            ''' <summary>Laufwerksbitmaske.</summary>
            Public UnitMask As Int32
            ''' <summary>Volume-Flags.</summary>
            Public Flags As Int16
        End Structure

        ''' <summary>Sendet eine Windows-Nachricht synchron.</summary>
        ''' <param name="handle">Empfangendes Fenster.</param>
        ''' <param name="message">Nachrichtencode.</param>
        ''' <param name="wParam">Ereigniscode.</param>
        ''' <param name="lParam">Optionaler Datenzeiger.</param>
        ''' <returns>Ergebnis der Fensterprozedur.</returns>
        <DllImport("user32.dll", CharSet:=CharSet.Auto)>
        Private Shared Function SendMessage(handle As IntPtr, message As Int32, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        ''' <summary>Erlaubt kontrollierte, verzögerte Zustellung ohne UI-Nachrichtenschleife.</summary>
        Private NotInheritable Class QueuedSynchronizer
            Implements ISynchronizeInvoke

            ''' <summary>Ausstehende Aktionen.</summary>
            Private ReadOnly _pending As New Queue(Of Action)()
            ''' <summary>Anzahl ausstehender Aktionen.</summary>
            Friend ReadOnly Property PendingCount As Int32
                Get
                    Return Me._pending.Count
                End Get
            End Property
            ''' <summary>Erzwingt asynchrone Zustellung.</summary>
            Public ReadOnly Property InvokeRequired As Boolean Implements ISynchronizeInvoke.InvokeRequired
                Get
                    Return True
                End Get
            End Property
            ''' <summary>Merkt eine Aktion für die spätere Ausführung vor.</summary>
            ''' <param name="method">Auszuführende Aktion.</param>
            ''' <param name="args">Optionale Argumente.</param>
            ''' <returns>Abgeschlossenes Platzhalterergebnis.</returns>
            Public Function BeginInvoke(method As [Delegate], args As Object()) As IAsyncResult Implements ISynchronizeInvoke.BeginInvoke
                Me._pending.Enqueue(DirectCast(method, Action))
                Return Task.CompletedTask
            End Function
            ''' <summary>Wird von diesen Tests nicht benötigt.</summary>
            ''' <param name="result">Asynchrones Ergebnis.</param>
            ''' <returns>Kein Ergebnis.</returns>
            Public Function EndInvoke(result As IAsyncResult) As Object Implements ISynchronizeInvoke.EndInvoke
                Throw New NotSupportedException()
            End Function
            ''' <summary>Wird von diesen Tests nicht benötigt.</summary>
            ''' <param name="method">Auszuführender Delegat.</param>
            ''' <param name="args">Optionale Argumente.</param>
            ''' <returns>Kein Ergebnis.</returns>
            Public Function Invoke(method As [Delegate], args As Object()) As Object Implements ISynchronizeInvoke.Invoke
                Throw New NotSupportedException()
            End Function
            ''' <summary>Führt alle vorgemerkten Aktionen aus.</summary>
            Friend Sub ExecutePending()
                While Me._pending.Count > 0
                    Me._pending.Dequeue().Invoke()
                End While
            End Sub
        End Class
    End Class
End Namespace