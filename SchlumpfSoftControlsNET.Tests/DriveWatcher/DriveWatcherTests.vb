' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.IO

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Namespace SchlumpfSoftControlsNET.Tests.DriveWatcherControl

    <TestClass>
    Public Class DriveWatcherTests

        <TestMethod>
        Public Sub DriveAddedEventArgs_DefaultConstructor_InitializesInstance()
            Dim args As New DriveWatcherAddedEventArgs()

            Assert.IsNotNull(args)
            Assert.IsNull(args.DriveName)
        End Sub

        <TestMethod>
        Public Sub Debounce_InterleavedEventsAndOppositeTransition_PreservesRealTransitions()
            Using watcher As New TestDriveWatcher()
                Dim added As Int32 = 0
                Dim removed As Int32 = 0
                Dim media As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e) added += 1
                AddHandler watcher.DriveRemoved, Sub(sender, e) removed += 1
                AddHandler watcher.MediaInserted, Sub(sender, e) media += 1
                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("E:\"))
                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("F:\"))
                watcher.RaiseMediaInserted(New DriveWatcherAddedEventArgs("E:\"))
                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("e:\"))
                Assert.AreEqual(2, added)
                Assert.AreEqual(1, media)
                watcher.RaiseDriveRemoved(New DriveWatcherRemovedEventArgs("E:\"))
                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("E:\"))
                Assert.AreEqual(3, added)
                Assert.AreEqual(1, removed)
            End Using
        End Sub

        <TestMethod>
        Public Sub DriveAddedEventArgs_NameTypeAndReadyConstructor_SetsValues()
            Dim args As New DriveWatcherAddedEventArgs("E:\\", DriveType.Removable, True)

            Assert.AreEqual("E:\\", args.DriveName)
            Assert.AreEqual(DriveType.Removable, args.DriveType)
            Assert.IsTrue(args.IsReady)
        End Sub

        <TestMethod>
        Public Sub DriveRemovedEventArgs_NameConstructor_SetsDriveName()
            Dim args As New DriveWatcherRemovedEventArgs("F:\\")

            Assert.AreEqual("F:\\", args.DriveName)
        End Sub

        <TestMethod>
        Public Sub OnDriveAdded_RaisesDriveAddedEvent()
            Using watcher As New TestDriveWatcher()
                Dim count As Int32 = 0
                Dim raisedName As String = Nothing
                AddHandler watcher.DriveAdded,
                    Sub(sender, e)
                        count += 1
                        raisedName = e.DriveName
                    End Sub

                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("G:\\", DriveType.Removable, True))

                Assert.AreEqual(1, count)
                Assert.AreEqual("G:\\", raisedName)
            End Using
        End Sub

        <TestMethod>
        Public Sub OnDriveRemoved_RaisesDriveRemovedEvent()
            Using watcher As New TestDriveWatcher()
                Dim count As Int32 = 0
                AddHandler watcher.DriveRemoved, Sub(sender, e) count += 1

                watcher.RaiseDriveRemoved(New DriveWatcherRemovedEventArgs("H:\\"))

                Assert.AreEqual(1, count)
            End Using
        End Sub

        <TestMethod>
        Public Sub OnMediaInserted_RaisesMediaInsertedEvent()
            Using watcher As New TestDriveWatcher()
                Dim count As Int32 = 0
                AddHandler watcher.MediaInserted, Sub(sender, e) count += 1

                watcher.RaiseMediaInserted(New DriveWatcherAddedEventArgs("I:\\", DriveType.CDRom, True))

                Assert.AreEqual(1, count)
            End Using
        End Sub

        <TestMethod>
        Public Sub OnNetworkDriveAdded_RaisesNetworkDriveAddedEvent()
            Using watcher As New TestDriveWatcher()
                Dim count As Int32 = 0
                AddHandler watcher.NetworkDriveAdded, Sub(sender, e) count += 1

                watcher.RaiseNetworkDriveAdded(New DriveWatcherAddedEventArgs("Z:\\", DriveType.Network, True))

                Assert.AreEqual(1, count)
            End Using
        End Sub

        <TestMethod>
        Public Sub DriveAdded_DebouncesSameEventTypeAndDriveName()
            Using watcher As New TestDriveWatcher()
                Dim count As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e) count += 1

                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("J:\\", DriveType.Removable, True))
                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("J:\\", DriveType.Removable, True))

                Assert.AreEqual(1, count)
            End Using
        End Sub

        <TestMethod>
        Public Sub Debounce_DoesNotSuppressDifferentEventType()
            Using watcher As New TestDriveWatcher()
                Dim driveAddedCount As Int32 = 0
                Dim mediaInsertedCount As Int32 = 0
                AddHandler watcher.DriveAdded, Sub(sender, e) driveAddedCount += 1
                AddHandler watcher.MediaInserted, Sub(sender, e) mediaInsertedCount += 1

                watcher.RaiseDriveAdded(New DriveWatcherAddedEventArgs("K:\\", DriveType.Removable, True))
                watcher.RaiseMediaInserted(New DriveWatcherAddedEventArgs("K:\\", DriveType.Removable, True))

                Assert.AreEqual(1, driveAddedCount)
                Assert.AreEqual(1, mediaInsertedCount)
            End Using
        End Sub

        Private NotInheritable Class TestDriveWatcher
            Inherits DriveWatcher

            Public Sub RaiseDriveAdded(e As DriveWatcherAddedEventArgs)
                MyBase.OnDriveAdded(e)
            End Sub

            Public Sub RaiseDriveRemoved(e As DriveWatcherRemovedEventArgs)
                MyBase.OnDriveRemoved(e)
            End Sub

            Public Sub RaiseMediaInserted(e As DriveWatcherAddedEventArgs)
                MyBase.OnMediaInserted(e)
            End Sub

            Public Sub RaiseNetworkDriveAdded(e As DriveWatcherAddedEventArgs)
                MyBase.OnNetworkDriveAdded(e)
            End Sub
        End Class

    End Class

End Namespace
