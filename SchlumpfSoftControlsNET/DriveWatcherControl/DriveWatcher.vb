' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcher.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Drawing
Imports System.Diagnostics
Imports System.Globalization

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
    ''' Identische Ereignisse desselben Laufwerks werden innerhalb von 200 Millisekunden unterdrückt;
    ''' ein entgegengesetzter Zustandswechsel hebt die Unterdrückung für das entsprechende Ereignispaar auf.
    ''' </remarks>
    <ToolboxItem(True)>
    <DesignTimeVisible(True)>
    <Description("Steuerelement zum Überwachen der Anzahl der Laufwerke.")>
    <ToolboxBitmap(GetType(DriveWatcher), "DriveWatcherControl.DriveWatcher.bmp")>
    Public Class DriveWatcher

        Inherits Component

#Region "Variablen"

        ''' <summary>Empfangsfenster für native Geräteänderungen und deren Ereignisse.</summary>
        Private WithEvents NatForm As DriveWatcherNativeForm ' Unsichtbares natives Fenster für den Empfang von WM_DEVICECHANGE.

        ''' <summary>Synchronisiert Zustellung, Freigabe und Diagnoseeinstellungen.</summary>
        Private ReadOnly _notificationSyncRoot As New Object

        ''' <summary>Optionales Ziel für die threadübergreifende Ereigniszustellung.</summary>
        Private _synchronizingObject As ISynchronizeInvoke

        ''' <summary>Kennzeichnet die beendete Ereigniszustellung.</summary>
        Private _notificationDisposed As Boolean

        ''' <summary>Aktuelle Diagnosestufe dieser Instanz.</summary>
        Private _traceLevel As TraceLevel = TraceLevel.Off

        ''' <summary>Unterdrückt wiederholte Laufwerksereignisse innerhalb des Zeitfensters.</summary>
        Private ReadOnly _debouncer As New DriveWatcherEventDebouncer()

#End Region

#Region "Öffentliche Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveWatcherAddedEventArgs"/>.</param>
        ''' <remarks>
        ''' Das Ereignis wird typischerweise bei Anschluss eines Wechselmediums (z. B. USB-Stick) ausgelöst. Wenn das
        ''' Volume noch nicht bereit ist (<see cref="DriveWatcherAddedEventArgs.IsReady"/> = False), sind Größen-/Formatfelder
        ''' neutral befüllt.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Laufwerk hinzugefügt wurde.")>
        Public Event DriveAdded(sender As Object, e As DriveWatcherAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum entfernten Laufwerk im <see cref="DriveWatcherRemovedEventArgs"/>.</param>
        ''' <remarks>
        ''' Das Ereignis enthält mindestens den Laufwerksnamen (z. B. "E:\"). Nach dem Entfernen sind keine
        ''' Größen-/Formatinformationen verfügbar.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Laufwerk entfernt wurde.")>
        Public Event DriveRemoved(sender As Object, e As DriveWatcherRemovedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveWatcherAddedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Medium eingelegt wurde.")>
        Public Event MediaInserted(sender As Object, e As DriveWatcherAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum Laufwerk im <see cref="DriveWatcherRemovedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Medium entfernt wurde.")>
        Public Event MediaRemoved(sender As Object, e As DriveWatcherRemovedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen im <see cref="DriveWatcherAddedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Netzlaufwerk hinzugefügt wurde.")>
        Public Event NetworkDriveAdded(sender As Object, e As DriveWatcherAddedEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz des <see cref="DriveWatcher"/>.</param>
        ''' <param name="e">Informationen zum entfernten Netzlaufwerk im <see cref="DriveWatcherRemovedEventArgs"/>.</param>
        <Description("Wird ausgelöst wenn ein Netzlaufwerk entfernt wurde.")>
        Public Event NetworkDriveRemoved(sender As Object, e As DriveWatcherRemovedEventArgs)

#End Region

#Region "Öffentliche Eigenschaften"

        ''' <summary>Ruft die optionale Diagnosestufe für diese Instanz ab oder legt sie fest.</summary>
        ''' <value>Die Diagnosestufe; standardmäßig <see cref="System.Diagnostics.TraceLevel.Off"/>.</value>
        ''' <remarks>
        ''' Error meldet Fehler beim Lesen von Laufwerksinformationen, Info zusätzlich Ereignisse einschließlich
        ''' unterdrückter Duplikate und Verbose zusätzlich native Volume-Rohdaten. Warning enthält dieselben
        ''' Meldungen wie Error. Die Ausgabe erfolgt über <see cref="Trace.Listeners"/> auf dem aufrufenden Thread.
        ''' Warnungen über ungültige native Nachrichten sind von dieser Einstellung unabhängig.
        ''' </remarks>
        ''' <exception cref="InvalidEnumArgumentException">Der Wert ist keine gültige Diagnosestufe.</exception>
        ''' <exception cref="ObjectDisposedException">Die Eigenschaft wird nach dem Freigeben gesetzt.</exception>
        <DefaultValue(TraceLevel.Off)>
        <Description("Optionale Diagnosestufe für Laufwerksmeldungen.")>
        Public Property TraceLevel As TraceLevel
            Get
                SyncLock Me._notificationSyncRoot
                    Return Me._traceLevel
                End SyncLock
            End Get
            Set(value As TraceLevel)
                SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                    If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
#Disable Warning CA2263 ' Die generische IsDefined-Überladung ist unter .NET Framework nicht verfügbar.
                    If Not [Enum].IsDefined(GetType(TraceLevel), value) Then
#Enable Warning CA2263
                        Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(TraceLevel))
                    End If
                    Me._traceLevel = value
                End SyncLock
            End Set
        End Property

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
            Me.NatForm = New DriveWatcherNativeForm()

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
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="e">Der Name des hinzugefügten Laufwerks.</param>
        Private Sub NatForm_DriveAdded(sender As Object, e As String) Handles NatForm.DriveAdded
            Me.OnDriveAdded(Me.ReadDriveInfo(e, DriveWatcherEventType.Added))
        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Laufwerk.
        ''' </summary>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="e">Der Name des entfernten Laufwerks.</param>
        Private Sub NatForm_DriveRemoved(sender As Object, e As String) Handles NatForm.DriveRemoved

            ' Beim Entfernen ist typischerweise nur noch der Laufwerksname zuverlässig vorhanden.
            Dim arg As New DriveWatcherRemovedEventArgs(e)
            Me.OnDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein eingelegtes Medium in einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaInserted(sender As Object, e As String) Handles NatForm.MediaInserted

            Me.OnMediaInserted(Me.ReadDriveInfo(e, DriveWatcherEventType.MediaInserted))

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Medium aus einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaRemoved(sender As Object, e As String) Handles NatForm.MediaRemoved

            Dim arg As New DriveWatcherRemovedEventArgs(e)
            Me.OnMediaRemoved(arg)

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein hinzugefügtes Netzlaufwerk.
        ''' </summary>
        ''' <remarks>
        ''' Die Methode überführt Laufwerksinformationen in stabile EventArgs. Falls ein Laufwerk noch nicht
        ''' bereit ist, bleiben Detailfelder neutral befüllt.
        ''' </remarks>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="e">Der Name des hinzugefügten Netzlaufwerks.</param>
        Private Sub NatForm_NetworkDriveAdded(sender As Object, e As String) Handles NatForm.NetworkDriveAdded

            Me.OnNetworkDriveAdded(Me.ReadDriveInfo(e, DriveWatcherEventType.NetworkAdded))

        End Sub

        ''' <summary>Protokolliert bei ausführlicher Diagnose die nativen Volume-Daten.</summary>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="eventCode">Der native Ereigniscode.</param>
        ''' <param name="driveName">Der Name des betroffenen Laufwerks.</param>
        ''' <param name="unitMask">Die native Laufwerksbitmaske.</param>
        ''' <param name="flags">Die nativen Volume-Flags.</param>
        Private Sub NatForm_VolumeObserved(sender As Object, eventCode As Int32, driveName As String, unitMask As Int32, flags As Int16) Handles NatForm.VolumeObserved
            If Me.IsTraceEnabled(System.Diagnostics.TraceLevel.Verbose) Then
                WriteDiagnostic($"Native EventCode=0x{eventCode:X4} Drive={driveName} UnitMask=0x{unitMask:X8} Flags=0x{CInt(flags) And &HFFFF:X4}")
            End If
        End Sub

        ''' <summary>Liest einen Laufwerks-Snapshot und protokolliert gegebenenfalls Lesefehler.</summary>
        ''' <param name="driveName">Der Name des zu lesenden Laufwerks.</param>
        ''' <param name="eventType">Der Ereignistyp für die Diagnose.</param>
        ''' <returns>Die gelesenen Laufwerksinformationen einschließlich etwaiger Lesefehler.</returns>
        Private Function ReadDriveInfo(driveName As String, eventType As DriveWatcherEventType) As DriveWatcherAddedEventArgs
            Dim result As DriveWatcherAddedEventArgs = DriveWatcherInfoSnapshotReader.Read(driveName)
            If Me.IsTraceEnabled(System.Diagnostics.TraceLevel.Error) Then
                For Each entry In result.ReadErrors
                    WriteDiagnostic($"ReadError Event={eventType} Drive={driveName} Field={entry.Key} Exception={entry.Value.GetType().FullName} Message={entry.Value.Message}")
                Next
            End If
            Return result
        End Function

        ''' <summary>Schreibt eine Diagnosemeldung mit UTC-Zeitstempel in den Trace.</summary>
        ''' <param name="message">Der Inhalt der Diagnosemeldung.</param>
        Private Shared Sub WriteDiagnostic(message As String)
            Dim timestamp As String = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            Trace.WriteLine($"DriveWatcher Timestamp={timestamp} {message}")
        End Sub

        ''' <summary>Prüft die Diagnosestufe unter Berücksichtigung der Freigabe.</summary>
        ''' <param name="level">Die mindestens benötigte Diagnosestufe.</param>
        ''' <returns>Ob die Diagnose für diese Stufe aktiviert ist.</returns>
        Private Function IsTraceEnabled(level As System.Diagnostics.TraceLevel) As Boolean
            SyncLock Me._notificationSyncRoot
                Return Not Me._notificationDisposed AndAlso Me._traceLevel >= level
            End SyncLock
        End Function

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Netzlaufwerk.
        ''' </summary>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="e">Der Name des entfernten Netzlaufwerks.</param>
        Private Sub NatForm_NetworkDriveRemoved(sender As Object, e As String) Handles NatForm.NetworkDriveRemoved

            Dim arg As New DriveWatcherRemovedEventArgs(e)
            Me.OnNetworkDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Laufwerks.</param>
        Protected Overridable Sub OnDriveAdded(e As DriveWatcherAddedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.Added) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Laufwerks.</param>
        Protected Overridable Sub OnDriveRemoved(e As DriveWatcherRemovedEventArgs)

            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.Removed) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaInserted"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des eingelegten Mediums.</param>
        Protected Overridable Sub OnMediaInserted(e As DriveWatcherAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.MediaInserted) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaInserted(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Mediums.</param>
        Protected Overridable Sub OnMediaRemoved(e As DriveWatcherRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.MediaRemoved) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveAdded(e As DriveWatcherAddedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.NetworkAdded) Then Return

            Me.DispatchEvent(Sub() RaiseEvent NetworkDriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveRemoved(e As DriveWatcherRemovedEventArgs)

            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.NetworkRemoved) Then Return

            Me.DispatchEvent(Sub() RaiseEvent NetworkDriveRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Stellt ein Ereignis direkt oder über das konfigurierte Synchronisierungsobjekt zu.
        ''' </summary>
        ''' <param name="raiseEventAction">Die auszuführende Ereignisbenachrichtigung.</param>
        Private Sub DispatchEvent(raiseEventAction As Action)
            Dim target As ISynchronizeInvoke
            ' Das Ziel wird einmalig erfasst; spätere Änderungen betreffen nur neue Ereignisse.
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
                    ' Die Freigabe wird beim späteren Ausführen erneut geprüft.
                    Dim unused = target.BeginInvoke(New Action(Sub() Me.RaiseEventIfActive(raiseEventAction)), Nothing)
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
            ' Handler außerhalb der Sperre ausführen, damit deren Code die Zustellung nicht blockiert.
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
            Me._debouncer.Clear()
        End Sub

        ''' <summary>
        ''' Prüft, ob ein Laufwerksereignis als Duplikat innerhalb des Entprellungsfensters gilt.
        ''' </summary>
        ''' <param name="driveName">Der Laufwerksname aus dem Ereignis.</param>
        ''' <param name="eventType">Der Typ des Laufwerksereignisses.</param>
        ''' <returns>
        ''' <see langword="True"/>, wenn das Ereignis unterdrückt werden soll; andernfalls <see langword="False"/>.
        ''' </returns>
        Private Function IsDebouncedEvent(driveName As String, eventType As DriveWatcherEventType) As Boolean

            Dim duplicate As Boolean = Me._debouncer.IsDuplicate(driveName, eventType)
            If Me.IsTraceEnabled(System.Diagnostics.TraceLevel.Info) Then
                WriteDiagnostic($"Event={eventType} Drive={driveName} Suppressed={duplicate}")
            End If
            Return duplicate

        End Function

#End Region

    End Class

End Namespace
