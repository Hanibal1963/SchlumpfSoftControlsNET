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
    ''' Ohne <see cref="SynchronizingObject"/> werden Geräteereignisse auf dem Thread des Empfangsfensters ausgelöst.
    ''' Mit einem Synchronisierungsobjekt können die Ereignisse auf dessen Thread zugestellt werden.
    ''' </remarks>
    <ToolboxItem(True)>
    <DesignTimeVisible(True)>
    <Description("Steuerelement zum Überwachen der Anzahl der Laufwerke.")>
    <ToolboxBitmap(GetType(DriveWatcher), "DriveWatcherControl.DriveWatcher.bmp")>
    Public Class DriveWatcher

        Inherits Component

#Region "Konstanten"

        Private Const DriveEventDebounceMs As Int32 = 200

#End Region

#Region "Variablen"

        Private WithEvents NatForm As NativeForm ' Unsichtbares natives Fenster für den Empfang von WM_DEVICECHANGE.

        Private ReadOnly _notificationSyncRoot As New Object

        Private _synchronizingObject As ISynchronizeInvoke
        Private _notificationDisposed As Boolean
        ' Felder für eine kurze Entprellung direkt aufeinanderfolgender identischer Ereignisse.
        Private ReadOnly _DebounceSyncRoot As New Object
        Private _LastDriveEventName As String = ""
        Private _LastDriveEventType As DriveEventType = DriveEventType.None
        Private _LastDriveEventTimeUtc As DateTime = DateTime.MinValue

#End Region

#Region "private Auflistungen"

        Private Enum DriveEventType
            None = 0
            Added = 1
            Removed = 2
            NetworkAdded = 3
            NetworkRemoved = 4
            MediaInserted = 5
            MediaRemoved = 6
        End Enum

#End Region

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

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Ruft das optionale Objekt für die Zustellung der Ereignisse auf dessen Thread ab oder legt es fest.
        ''' </summary>
        ''' <value>Das Synchronisierungsobjekt oder <see langword="Nothing"/> für die direkte Zustellung.</value>
        ''' <remarks>
        ''' Ohne Synchronisierungsobjekt werden Geräteereignisse synchron auf dem Thread des Empfangsfensters
        ''' ausgelöst. Ist <see cref="ISynchronizeInvoke.InvokeRequired"/> wahr, erfolgt die Zustellung asynchron
        ''' über <see cref="ISynchronizeInvoke.BeginInvoke"/>. Ein Windows Forms-Steuerelement muss beim Auslösen
        ''' bereits ein Fensterhandle besitzen und sein Thread muss Nachrichten verarbeiten. Eine Änderung gilt für
        ''' nachfolgende Ereignisse; bereits vorgemerkte Zustellungen behalten ihr ursprüngliches Ziel.
        ''' Beim Freigeben der Komponente werden noch nicht begonnene vorgemerkte Zustellungen verworfen; bereits
        ''' laufende Rückrufe können noch beendet werden. Die Handlerliste wird bei der tatsächlichen Zustellung
        ''' ausgewertet. Fehler des Synchronisierungsobjekts und der Handler werden nicht abgefangen.
        ''' </remarks>
        ''' <exception cref="ObjectDisposedException">Die Eigenschaft wird nach dem Freigeben der Komponente gesetzt.</exception>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        <Description("Optionales Objekt zur Zustellung der Ereignisse auf dessen Thread.")>
        Public Property SynchronizingObject As ISynchronizeInvoke
            Get
                SyncLock Me._notificationSyncRoot
                    Return Me._synchronizingObject
                End SyncLock
            End Get
            Set(value As ISynchronizeInvoke)
                SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                    If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
                    Me._synchronizingObject = value
                End SyncLock
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="DriveWatcher"/>.
        ''' </summary>
        ''' <remarks>
        ''' Das Empfangsfenster wird auf dem aufrufenden Thread erstellt und sollte auf demselben Thread mit
        ''' <see cref="Component.Dispose()"/> freigegeben werden.
        ''' </remarks>
        ''' <exception cref="Win32Exception">Das Empfangsfenster konnte nicht erstellt werden.</exception>
        Public Sub New()

            MyBase.New()
            Me.InitializeComponent()
            ' WithEvents registriert die Handler einmalig für dieses Empfangsfenster.
            Me.NatForm = New NativeForm()

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein hinzugefügtes Laufwerk.
        ''' </summary>
        ''' <remarks>
        ''' Die Methode überführt Laufwerksinformationen in stabile EventArgs. Falls ein Laufwerk noch nicht
        ''' bereit ist, bleiben Detailfelder neutral befüllt.
        ''' </remarks>
        Private Sub NatForm_DriveAdded(sender As Object, e As String) Handles NatForm.DriveAdded
            Me.OnDriveAdded(DriveInfoSnapshotReader.Read(e))
        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Laufwerk.
        ''' </summary>
        Private Sub NatForm_DriveRemoved(sender As Object, e As String) Handles NatForm.DriveRemoved

            ' Beim Entfernen ist typischerweise nur noch der Laufwerksname zuverlässig vorhanden.
            Dim arg As New DriveRemovedEventArgs(e)
            Me.OnDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein eingelegtes Medium in einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaInserted(sender As Object, e As String) Handles NatForm.MediaInserted

            Me.OnMediaInserted(DriveInfoSnapshotReader.Read(e))

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Medium aus einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaRemoved(sender As Object, e As String) Handles NatForm.MediaRemoved

            Dim arg As New DriveRemovedEventArgs(e)
            Me.OnMediaRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein hinzugefügtes Netzlaufwerk.
        ''' </summary>
        ''' <remarks>
        ''' Die Methode überführt Laufwerksinformationen in stabile EventArgs. Falls ein Laufwerk noch nicht
        ''' bereit ist, bleiben Detailfelder neutral befüllt.
        ''' </remarks>
        Private Sub NatForm_NetworkDriveAdded(sender As Object, e As String) Handles NatForm.NetworkDriveAdded

            Me.OnNetworkDriveAdded(DriveInfoSnapshotReader.Read(e))

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Netzlaufwerk.
        ''' </summary>
        Private Sub NatForm_NetworkDriveRemoved(sender As Object, e As String) Handles NatForm.NetworkDriveRemoved

            Dim arg As New DriveRemovedEventArgs(e)
            Me.OnNetworkDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Laufwerks.</param>
        Protected Overridable Sub OnDriveAdded(e As DriveAddedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.Added) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Laufwerks.</param>
        Protected Overridable Sub OnDriveRemoved(e As DriveRemovedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.Removed) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaInserted"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des eingelegten Mediums.</param>
        Protected Overridable Sub OnMediaInserted(e As DriveAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.MediaInserted) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaInserted(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Mediums.</param>
        Protected Overridable Sub OnMediaRemoved(e As DriveRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.MediaRemoved) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveAdded(e As DriveAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.NetworkAdded) Then Return

            Me.DispatchEvent(Sub() RaiseEvent NetworkDriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveRemoved(e As DriveRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveEventType.NetworkRemoved) Then Return

            Me.DispatchEvent(Sub() RaiseEvent NetworkDriveRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Stellt ein Ereignis direkt oder über das konfigurierte Synchronisierungsobjekt zu.
        ''' </summary>
        ''' <param name="raiseEventAction">Die auszuführende Ereignisbenachrichtigung.</param>
        Private Sub DispatchEvent(raiseEventAction As Action)
            Dim target As ISynchronizeInvoke
            SyncLock Me._notificationSyncRoot
                If Me._notificationDisposed Then Return
                target = Me._synchronizingObject
            End SyncLock

            If target IsNot Nothing Then
                Dim targetControl As System.Windows.Forms.Control = TryCast(target, System.Windows.Forms.Control)
                ' InvokeRequired kann ohne Fensterhandle fälschlich False zurückgeben.
                If targetControl IsNot Nothing AndAlso Not targetControl.IsHandleCreated Then
                    Throw New InvalidOperationException("Das Synchronisierungsobjekt muss ein erstelltes Fensterhandle besitzen.")
                End If

                If target.InvokeRequired Then
                    target.BeginInvoke(New Action(Sub() Me.RaiseEventIfActive(raiseEventAction)), Nothing)
                    Return
                End If
            End If

            Me.RaiseEventIfActive(raiseEventAction)
        End Sub

        ''' <summary>
        ''' Unterdrückt vorgemerkte Ereignisse, wenn die Komponente inzwischen freigegeben wurde.
        ''' </summary>
        ''' <param name="raiseEventAction">Die auszuführende Ereignisbenachrichtigung.</param>
        Private Sub RaiseEventIfActive(raiseEventAction As Action)
            SyncLock Me._notificationSyncRoot
                If Me._notificationDisposed Then Return
            End SyncLock
            raiseEventAction()
        End Sub

        ''' <summary>
        ''' Beendet die Ereigniszustellung und entfernt die Referenz auf das Synchronisierungsobjekt.
        ''' </summary>
        Private Sub StopEventDispatch()
            SyncLock Me._notificationSyncRoot
                Me._notificationDisposed = True
                Me._synchronizingObject = Nothing
            End SyncLock
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
