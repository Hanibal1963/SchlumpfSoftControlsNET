' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcher.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Drawing

Namespace DriveWatcherControl

    ''' <summary>
    ''' Steuerelement zum Überwachen von logischen Laufwerken (Volumes).<br/> Erkennt das Hinzufügen und Entfernen von
    ''' Laufwerken.
    ''' </summary>
    ''' <remarks>
    ''' Dieses Komponentenobjekt erzeugt intern ein natives Fenster, um Windows-Nachrichten für Geräteänderungen
    ''' (<c>WM_DEVICECHANGE</c>) zu empfangen.<br/> Bei relevanten Ereignissen werden die .NET-Events
    ''' <see cref="DriveAdded"/> und <see cref="DriveRemoved"/> ausgelöst.<br/> Für Medienwechsel werden zusätzlich
    ''' <see cref="MediaInserted"/> und <see cref="MediaRemoved"/> bereitgestellt.<br/> Für Netzlaufwerke werden
    ''' zusätzlich <see cref="NetworkDriveAdded"/> und <see cref="NetworkDriveRemoved"/> bereitgestellt.
    ''' </remarks>
    <ToolboxItem(True)>
    <DesignTimeVisible(True)>
    <Description("Steuerelement zum Überwachen der Anzahl der Laufwerke.")>
    <ToolboxBitmap(GetType(DriveWatcher), "DriveWatcherControl.DriveWatcher.bmp")>
    Public Class DriveWatcher

        Inherits Component

        Private Const DriveEventDebounceMs As Int32 = 200

#Region "Variablen"

        ' Unsichtbares natives Fenster für den Empfang von WM_DEVICECHANGE.
        Private WithEvents NatForm As New NativeForm

        ' Felder für eine kurze Entprellung direkt aufeinanderfolgender identischer Ereignisse.
        Private ReadOnly _DebounceSyncRoot As New Object
        Private _LastDriveEventName As String = ""
        Private _LastDriveEventType As DriveEventType = DriveEventType.None
        Private _LastDriveEventTimeUtc As DateTime = DateTime.MinValue

#End Region

        Private Enum DriveEventType
            None = 0
            Added = 1
            Removed = 2
            NetworkAdded = 3
            NetworkRemoved = 4
            MediaInserted = 5
            MediaRemoved = 6
        End Enum

#Region "Öffentliche Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveAddedEventArgs"/>.</param>
        ''' <remarks>
        ''' Das Ereignis wird typischerweise bei Anschluss eines Wechselmediums (z. B. USB-Stick) ausgelöst. Wenn das
        ''' Volume noch nicht bereit ist (<see cref="DriveAddedEventArgs.IsReady"/> = False), sind Größen-/Formatfelder
        ''' neutral befüllt.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Laufwerk hinzugefügt wurde.")>
        Public Event DriveAdded(sender As Object, e As DriveAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum entfernten Laufwerk im <see cref="DriveRemovedEventArgs"/>.</param>
        ''' <remarks>
        ''' Das Ereignis enthält mindestens den Laufwerksnamen (z. B. "E:\"). Nach dem Entfernen sind keine
        ''' Größen-/Formatinformationen verfügbar.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Laufwerk entfernt wurde.")>
        Public Event DriveRemoved(sender As Object, e As DriveRemovedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveAddedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Medium eingelegt wurde.")>
        Public Event MediaInserted(sender As Object, e As DriveAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum Laufwerk im <see cref="DriveRemovedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Medium entfernt wurde.")>
        Public Event MediaRemoved(sender As Object, e As DriveRemovedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveAddedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Netzlaufwerk hinzugefügt wurde.")>
        Public Event NetworkDriveAdded(sender As Object, e As DriveAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum entfernten Netzlaufwerk im <see cref="DriveRemovedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Netzlaufwerk entfernt wurde.")>
        Public Event NetworkDriveRemoved(sender As Object, e As DriveRemovedEventArgs)

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="DriveWatcher"/>.
        ''' </summary>
        Public Sub New()

            MyBase.New()
            Me.InitializeComponent()

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein hinzugefügtes Laufwerk.
        ''' </summary>
        ''' <remarks>
        ''' Die Methode überführt <see cref="DriveInfo"/> in stabile EventArgs. Falls ein Laufwerk noch nicht
        ''' bereit ist, bleiben Detailfelder neutral befüllt.
        ''' </remarks>
        Private Sub NatForm_DriveAdded(sender As Object, e As DriveInfo) Handles NatForm.DriveAdded

            ' Basisdaten aus der Betriebssystemmeldung übernehmen.
            Dim arg As New DriveAddedEventArgs(e.Name, e.DriveType, e.IsReady)

            ' Mit neutralen Standardwerten initialisieren.
            With arg
                .VolumeLabel = ""
                .AvailableFreeSpace = 0
                .TotalFreeSpace = 0
                .TotalSize = 0
                .DriveFormat = ""
            End With

            If e.IsReady Then
                Try
                    ' Erweiterte Laufwerksinfos nur auslesen, wenn das Volume bereits zugreifbar ist.
                    With arg
                        .VolumeLabel = e.VolumeLabel
                        .AvailableFreeSpace = e.AvailableFreeSpace
                        .TotalFreeSpace = e.TotalFreeSpace
                        .TotalSize = e.TotalSize
                        .DriveFormat = e.DriveFormat
                    End With
                Catch ex As IOException
                    ' Laufwerk kann zwischenzeitlich nicht mehr gelesen werden.
                Catch ex As UnauthorizedAccessException
                    ' Zugriff auf Metadaten wurde vom System verweigert.
                Catch ex As SystemException
                    ' Schutz gegen weitere systemnahe Fehler beim Auslesen.
                End Try
            End If

            Me.OnDriveAdded(arg)
        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Laufwerk.
        ''' </summary>
        Private Sub NatForm_DriveRemoved(sender As Object, e As DriveInfo) Handles NatForm.DriveRemoved

            ' Beim Entfernen ist typischerweise nur noch der Laufwerksname zuverlässig vorhanden.
            Dim arg As New DriveRemovedEventArgs(e.Name)
            Me.OnDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein eingelegtes Medium in einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaInserted(sender As Object, e As DriveInfo) Handles NatForm.MediaInserted

            Dim arg As New DriveAddedEventArgs(e.Name, e.DriveType, e.IsReady)

            With arg
                .VolumeLabel = ""
                .AvailableFreeSpace = 0
                .TotalFreeSpace = 0
                .TotalSize = 0
                .DriveFormat = ""
            End With

            If e.IsReady Then
                Try
                    With arg
                        .VolumeLabel = e.VolumeLabel
                        .AvailableFreeSpace = e.AvailableFreeSpace
                        .TotalFreeSpace = e.TotalFreeSpace
                        .TotalSize = e.TotalSize
                        .DriveFormat = e.DriveFormat
                    End With
                Catch ex As IOException
                Catch ex As UnauthorizedAccessException
                Catch ex As SystemException
                End Try
            End If

            Me.OnMediaInserted(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Medium aus einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaRemoved(sender As Object, e As DriveInfo) Handles NatForm.MediaRemoved

            Dim arg As New DriveRemovedEventArgs(e.Name)
            Me.OnMediaRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein hinzugefügtes Netzlaufwerk.
        ''' </summary>
        ''' <remarks>
        ''' Die Methode überführt <see cref="DriveInfo"/> in stabile EventArgs. Falls ein Laufwerk noch nicht
        ''' bereit ist, bleiben Detailfelder neutral befüllt.
        ''' </remarks>
        Private Sub NatForm_NetworkDriveAdded(sender As Object, e As DriveInfo) Handles NatForm.NetworkDriveAdded

            Dim arg As New DriveAddedEventArgs(e.Name, e.DriveType, e.IsReady)

            With arg
                .VolumeLabel = ""
                .AvailableFreeSpace = 0
                .TotalFreeSpace = 0
                .TotalSize = 0
                .DriveFormat = ""
            End With

            If e.IsReady Then
                Try
                    With arg
                        .VolumeLabel = e.VolumeLabel
                        .AvailableFreeSpace = e.AvailableFreeSpace
                        .TotalFreeSpace = e.TotalFreeSpace
                        .TotalSize = e.TotalSize
                        .DriveFormat = e.DriveFormat
                    End With
                Catch ex As IOException
                Catch ex As UnauthorizedAccessException
                Catch ex As SystemException
                End Try
            End If

            Me.OnNetworkDriveAdded(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Netzlaufwerk.
        ''' </summary>
        Private Sub NatForm_NetworkDriveRemoved(sender As Object, e As DriveInfo) Handles NatForm.NetworkDriveRemoved

            Dim arg As New DriveRemovedEventArgs(e.Name)
            Me.OnNetworkDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Laufwerks.</param>
        Protected Overridable Sub OnDriveAdded(e As DriveAddedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.Added) Then Return

            RaiseEvent DriveAdded(Me, e)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Laufwerks.</param>
        Protected Overridable Sub OnDriveRemoved(e As DriveRemovedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.Removed) Then Return

            RaiseEvent DriveRemoved(Me, e)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaInserted"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des eingelegten Mediums.</param>
        Protected Overridable Sub OnMediaInserted(e As DriveAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.MediaInserted) Then Return

            RaiseEvent MediaInserted(Me, e)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Mediums.</param>
        Protected Overridable Sub OnMediaRemoved(e As DriveRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.MediaRemoved) Then Return

            RaiseEvent MediaRemoved(Me, e)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveAdded(e As DriveAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.NetworkAdded) Then Return

            RaiseEvent NetworkDriveAdded(Me, e)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveRemoved(e As DriveRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.NetworkRemoved) Then Return

            RaiseEvent NetworkDriveRemoved(Me, e)

        End Sub

        ''' <summary>
        ''' Prüft, ob ein Laufwerksereignis als Duplikat innerhalb des Entprellungsfensters gilt.
        ''' </summary>
        ''' <param name="driveName">Der Laufwerksname aus dem Ereignis.</param>
        ''' <param name="eventType">Der Typ des Laufwerksereignisses.</param>
        ''' <returns>
        ''' <see langword="True"/>, wenn das Ereignis unterdrückt werden soll; andernfalls <see langword="False"/>.
        ''' </returns>
        Private Function IsDebouncedEvent(driveName As String, eventType As DriveEventType) As Boolean

            Dim normalizedDriveName As String = If(driveName, "")
            Dim nowUtc As DateTime = DateTime.UtcNow

            SyncLock Me._DebounceSyncRoot
                ' Nur identischer Name + identischer Typ + sehr kurzer zeitlicher Abstand gilt als Duplikat.
                If Me._LastDriveEventType = eventType AndAlso
                   String.Equals(Me._LastDriveEventName, normalizedDriveName, StringComparison.OrdinalIgnoreCase) AndAlso
                   (nowUtc - Me._LastDriveEventTimeUtc).TotalMilliseconds <= DriveEventDebounceMs Then

                    Return True
                End If

                ' Letztes gültiges Ereignis als neue Referenz speichern.
                Me._LastDriveEventName = normalizedDriveName
                Me._LastDriveEventType = eventType
                Me._LastDriveEventTimeUtc = nowUtc
            End SyncLock

            Return False

        End Function

#End Region

    End Class

End Namespace
