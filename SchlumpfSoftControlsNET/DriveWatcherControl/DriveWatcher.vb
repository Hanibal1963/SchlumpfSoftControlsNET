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
Imports System.Collections.Generic

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

        ''' <summary>Timer für den optionalen, auf dem Fensterthread laufenden Vergleich.</summary>
        Private WithEvents PollingTimer As System.Windows.Forms.Timer

        ''' <summary>Synchronisiert Zustellung, Freigabe und Diagnoseeinstellungen.</summary>
        Private ReadOnly _notificationSyncRoot As New Object

        ''' <summary>Optionales Ziel für die threadübergreifende Ereigniszustellung.</summary>
        Private _synchronizingObject As ISynchronizeInvoke

        ''' <summary>Kennzeichnet die beendete Ereigniszustellung.</summary>
        Private _notificationDisposed As Boolean

        ''' <summary>Aktuelle Diagnosestufe dieser Instanz.</summary>
        Private _traceLevel As TraceLevel = TraceLevel.Off

        ''' <summary>Aktiviert die optionale Zustellung nativer Rohereignisse.</summary>
        Private _diagnosticMode As Boolean

        ''' <summary>Unterdrückt wiederholte Laufwerksereignisse innerhalb des Zeitfensters.</summary>
        Private ReadOnly _debouncer As New DriveWatcherEventDebouncer()

        ''' <summary>Momentaufnahme der zugelassenen Laufwerkstypen; leer bedeutet keine Einschränkung.</summary>
        Private _includeDriveTypes As DriveType() = Array.Empty(Of DriveType)()

        ''' <summary>Momentaufnahme der zugelassenen Laufwerksbuchstaben; leer bedeutet keine Einschränkung.</summary>
        Private _includeLetters As Char() = Array.Empty(Of Char)()

        ''' <summary>Zuletzt beobachtete Typen, auch für nachfolgende Entfernungsereignisse.</summary>
        Private ReadOnly _driveTypes As New Dictionary(Of String, DriveType)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Thread, auf dem Empfangsfenster und Polling-Timer betrieben werden.</summary>
        Private ReadOnly _ownerThreadId As Int32 = Environment.CurrentManagedThreadId

        ''' <summary>Abfrageintervall in Millisekunden.</summary>
        Private _pollingInterval As Int32 = 2000

        ''' <summary>Aktivierter Polling-Modus; native Ereignisse werden dann nicht zugestellt.</summary>
        Private _pollingEnabled As Boolean

        ''' <summary>Verhindert verschachtelte Abfragen durch eine erneut betretene Nachrichtenschleife.</summary>
        Private _pollingBusy As Boolean

        ''' <summary>Letzter erfolgreicher Vergleichszustand, unabhängig von Ereignisfiltern.</summary>
        Private _pollingState As New Dictionary(Of String, PollingDriveState)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Liefert vollständige Ereignisdaten für den nächsten Vergleich.</summary>
        Private _pollingSource As Func(Of IReadOnlyList(Of DriveWatcherAddedEventArgs)) = AddressOf ReadPollingDrives

#End Region

#Region "Öffentliche Ereignisse"

        ''' <summary>Wird bei aktivem Diagnosemodus für jede empfangene WM_DEVICECHANGE-Nachricht ausgelöst.</summary>
        ''' <param name="sender">Die auslösende DriveWatcher-Instanz.</param>
        ''' <param name="e">Kopierte Rohdaten ohne native Zeiger.</param>
        ''' <remarks>Das Ereignis ist unabhängig von Laufwerksfiltern, Entprellung und Polling und verwendet SynchronizingObject.</remarks>
        <Description("Meldet native Geräteänderungen im Diagnosemodus.")>
        Public Event RawDeviceEventReceived(sender As Object, e As DriveWatcherRawDeviceEventArgs)

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

        ''' <summary>Aktiviert die Zustellung von nativen Geräteänderungen über RawDeviceEventReceived.</summary>
        ''' <value>Standardmäßig <see langword="False"/>; unabhängig von TraceLevel und PollingEnabled.</value>
        ''' <remarks>Das Abschalten betrifft neue Nachrichten; bereits vorgemerkte Zustellungen bleiben bestehen.</remarks>
        ''' <exception cref="ObjectDisposedException">Die Eigenschaft wird nach der Freigabe gesetzt.</exception>
        <DefaultValue(False)>
        <Description("Aktiviert die Zustellung nativer Geräte-Rohereignisse.")>
        Public Property DiagnosticMode As Boolean
            Get
                SyncLock Me._notificationSyncRoot
                    Return Me._diagnosticMode
                End SyncLock
            End Get
            Set(value As Boolean)
                SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                    If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
                    Me._diagnosticMode = value
                End SyncLock
            End Set
        End Property

        ''' <summary>Aktiviert den Vergleich von Laufwerksmomentaufnahmen anstelle nativer Ereignisse.</summary>
        ''' <remarks>Das Setzen erfolgt auf dem Thread des Empfangsfensters. Die erste synchrone Abfrage erzeugt keine Ereignisse; danach ist eine Nachrichtenschleife erforderlich.</remarks>
        ''' <exception cref="ObjectDisposedException">Die Komponente wurde freigegeben.</exception>
        ''' <exception cref="InvalidOperationException">Das Setzen erfolgt auf einem anderen Thread oder während einer Polling-Abfrage.</exception>
        <DefaultValue(False)>
        <Description("Aktiviert den laufwerksbasierten Polling-Fallback anstelle nativer Ereignisse.")>
        Public Property PollingEnabled As Boolean
            Get
                SyncLock Me._notificationSyncRoot
                    Return Me._pollingEnabled
                End SyncLock
            End Get
            Set(value As Boolean)
                Me.CheckPollingThread()
                If value AndAlso Me._pollingBusy Then Throw New InvalidOperationException("Der Polling-Modus kann während einer Abfrage nicht aktiviert werden.")
                If value = Me._pollingEnabled Then Return
                If value Then
                    Dim initial = Me.CreatePollingState(Me._pollingSource(), Nothing)
                    Dim timer As New System.Windows.Forms.Timer With {.Interval = Me._pollingInterval}
                    Dim started As Boolean = False
                    Try
                        timer.Start()
                        started = True
                    Finally
                        If Not started Then timer.Dispose()
                    End Try
                    Me._pollingState = initial
                    Me.pollingTimer = timer
                    SyncLock Me._notificationSyncRoot
                        Me._pollingEnabled = True
                        Me._driveTypes.Clear()
                    End SyncLock
                    Me._debouncer.Clear()
                    Me.RememberPollingTypes(initial)
                Else
                    SyncLock Me._notificationSyncRoot
                        Me._pollingEnabled = False
                    End SyncLock
                    Dim timer = Me.pollingTimer
                    Me.pollingTimer = Nothing
                    If timer IsNot Nothing Then timer.Dispose()
                    Me._pollingState = New Dictionary(Of String, PollingDriveState)(StringComparer.OrdinalIgnoreCase)
                    Me._debouncer.Clear()
                End If
            End Set
        End Property

        ''' <summary>Ruft das Polling-Intervall in Millisekunden ab oder legt es fest.</summary>
        ''' <value>Ein positiver Wert; standardmäßig 2000 Millisekunden.</value>
        ''' <exception cref="ArgumentOutOfRangeException">Das Intervall ist nicht positiv.</exception>
        ''' <exception cref="ObjectDisposedException">Die Komponente wurde freigegeben.</exception>
        ''' <exception cref="InvalidOperationException">Das Setzen erfolgt nicht auf dem Thread des Empfangsfensters.</exception>
        <DefaultValue(2000)>
        <Description("Intervall des Polling-Fallbacks in Millisekunden.")>
        Public Property PollingInterval As Int32
            Get
                Return Me._pollingInterval
            End Get
            Set(value As Int32)
                Me.CheckPollingThread()
#If NET8_0_OR_GREATER Then
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, NameOf(value))
#Else
                If value <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(value))
#End If
                If Me.pollingTimer IsNot Nothing Then Me.pollingTimer.Interval = value
                Me._pollingInterval = value
            End Set
        End Property

        ''' <summary>Ruft die zugelassenen Laufwerkstypen ab oder legt sie fest.</summary>
        ''' <value>Eine kopierte Liste; <see langword="Nothing"/> oder eine leere Liste deaktiviert den Typfilter.</value>
        ''' <remarks>Bei Entfernungen wird der zuletzt beobachtete Typ verwendet; ohne bekannten Typ gilt Unknown.</remarks>
        ''' <exception cref="InvalidEnumArgumentException">Ein Eintrag ist kein gültiger Laufwerkstyp.</exception>
        ''' <exception cref="ObjectDisposedException">Die Eigenschaft wird nach dem Freigeben gesetzt.</exception>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property IncludeDriveTypes As DriveType()
            Get
                SyncLock Me._notificationSyncRoot
                    Return DirectCast(Me._includeDriveTypes.Clone(), DriveType())
                End SyncLock
            End Get
            Set(value As DriveType())
                SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                    If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
                    Dim values As DriveType() = If(value Is Nothing, Array.Empty(Of DriveType)(), DirectCast(value.Clone(), DriveType()))
                    For Each driveType In values
#Disable Warning CA2263 ' Die generische IsDefined-Überladung ist unter .NET Framework nicht verfügbar.
                        If Not [Enum].IsDefined(GetType(DriveType), driveType) Then Throw New InvalidEnumArgumentException(NameOf(value), CInt(driveType), GetType(DriveType))
#Enable Warning CA2263
                    Next
                    Me._includeDriveTypes = values
                    Me._debouncer.Clear()
                End SyncLock
            End Set
        End Property

        ''' <summary>Ruft die zugelassenen Laufwerksbuchstaben ab oder legt sie fest.</summary>
        ''' <value>Eine kopierte Liste; <see langword="Nothing"/> oder eine leere Liste deaktiviert den Buchstabenfilter.</value>
        ''' <remarks>Nur ASCII-Buchstaben A bis Z sind zulässig; Groß- und Kleinschreibung sind gleichwertig.</remarks>
        ''' <exception cref="ArgumentException">Ein Eintrag ist kein Laufwerksbuchstabe von A bis Z.</exception>
        ''' <exception cref="ObjectDisposedException">Die Eigenschaft wird nach dem Freigeben gesetzt.</exception>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property IncludeLetters As Char()
            Get
                SyncLock Me._notificationSyncRoot
                    Return DirectCast(Me._includeLetters.Clone(), Char())
                End SyncLock
            End Get
            Set(value As Char())
                SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                    If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
                    Dim values As Char() = If(value Is Nothing, Array.Empty(Of Char)(), DirectCast(value.Clone(), Char()))
                    For index As Int32 = 0 To values.Length - 1
                        Dim letter As Char = Char.ToUpperInvariant(values(index))
                        If letter < "A"c OrElse letter > "Z"c Then Throw New ArgumentException("Nur Laufwerksbuchstaben von A bis Z sind zulässig.", NameOf(value))
                        values(index) = letter
                    Next
                    Me._includeLetters = values
                    Me._debouncer.Clear()
                End SyncLock
            End Set
        End Property

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

        ''' <summary>Ermittelt die aktuell vom Betriebssystem gemeldeten logischen Laufwerke und ihre Metadaten.</summary>
        ''' <returns>Eine schreibgeschützte Liste unabhängiger Laufwerksmomentaufnahmen.</returns>
        ''' <remarks>
        ''' Die synchrone Abfrage berücksichtigt keine Ereignisfilter und löst keine Ereignisse aus. Laufwerke und
        ''' Eigenschaften werden nacheinander gelesen; die Daten bilden daher keinen atomaren Systemzustand ab.
        ''' Einzelne erwartete Lesefehler stehen in der jeweiligen Momentaufnahme. Fehler bei der Aufzählung und
        ''' unerwartete Fehler werden weitergegeben.
        ''' </remarks>
        Public Shared Function GetCurrentDrives() As IReadOnlyList(Of DriveWatcherDriveSnapshot)
            Dim names As New List(Of String)()
            For Each drive In DriveInfo.GetDrives()
                names.Add(drive.Name)
            Next
            Return DriveWatcherInfoSnapshotReader.ReadCurrent(names, Function(name) New DriveWatcherInfoSource(name))
        End Function

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

        ''' <summary>Ersetzt die Datenquelle für deterministische Tests vor dem Aktivieren.</summary>
        ''' <param name="source">Liefert die Laufwerksdaten für jeden Vergleich.</param>
        Friend Sub SetPollingSource(source As Func(Of IReadOnlyList(Of DriveWatcherAddedEventArgs)))
            Me.CheckPollingThread()
#If NET8_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(source, NameOf(source))
#Else
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
#End If
            If Me.PollingEnabled Then Throw New InvalidOperationException("Die Datenquelle kann bei aktivem Polling nicht geändert werden.")
            Me._pollingSource = source
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>Prüft Lebensdauer und Threadzugehörigkeit der Polling-Konfiguration.</summary>
        ''' <exception cref="ObjectDisposedException">Die Komponente wurde freigegeben.</exception>
        ''' <exception cref="InvalidOperationException">Der aktuelle Thread ist nicht der Fensterthread.</exception>
        Private Sub CheckPollingThread()
            SyncLock Me._notificationSyncRoot
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
                If Me._notificationDisposed Then Throw New ObjectDisposedException(NameOf(DriveWatcher))
#Enable Warning CA1513
            End SyncLock
            If Environment.CurrentManagedThreadId <> Me._ownerThreadId Then Throw New InvalidOperationException("Polling muss auf dem Thread des Empfangsfensters konfiguriert werden.")
        End Sub

        ''' <summary>Liest alle Laufwerke einschließlich der für Ereignisse benötigten Metadaten.</summary>
        ''' <returns>Die beim aktuellen Durchlauf gelesenen Laufwerksdaten.</returns>
        Private Shared Function ReadPollingDrives() As IReadOnlyList(Of DriveWatcherAddedEventArgs)
            Dim result As New List(Of DriveWatcherAddedEventArgs)()
            For Each drive In DriveInfo.GetDrives()
                result.Add(DriveWatcherInfoSnapshotReader.Read(drive.Name))
            Next
            Return result
        End Function

        ''' <summary>Übernimmt zuverlässige Werte und bewahrt ältere Werte bei fehlgeschlagenen Einzelabfragen.</summary>
        ''' <param name="drives">Aktuell gelesene Laufwerke.</param>
        ''' <param name="previous">Vorheriger Vergleichszustand oder Nothing beim Start.</param>
        ''' <returns>Neuer Zustand mit case-insensitiven Laufwerksnamen.</returns>
        Private Function CreatePollingState(drives As IReadOnlyList(Of DriveWatcherAddedEventArgs), previous As Dictionary(Of String, PollingDriveState)) As Dictionary(Of String, PollingDriveState)
            Dim result As New Dictionary(Of String, PollingDriveState)(StringComparer.OrdinalIgnoreCase)
            For Each drive In drives
                If Me.IsTraceEnabled(System.Diagnostics.TraceLevel.Error) Then
                    For Each entry In drive.ReadErrors
                        WriteDiagnostic($"ReadError Event=Polling Drive={drive.DriveName} Field={entry.Key} Exception={entry.Value.GetType().FullName} Message={entry.Value.Message}")
                    Next
                End If
                Dim old As PollingDriveState = Nothing
                If previous IsNot Nothing Then
                    Dim unused = previous.TryGetValue(drive.DriveName, old)
                End If
                Dim type = drive.DriveType
                Dim ready As Boolean? = drive.IsReady
                If drive.ReadErrors.ContainsKey("DriveInfo") OrElse drive.ReadErrors.ContainsKey(NameOf(drive.DriveType)) Then
                    type = If(old Is Nothing, DriveType.Unknown, old.DriveType)
                End If
                If drive.ReadErrors.ContainsKey("DriveInfo") OrElse drive.ReadErrors.ContainsKey(NameOf(drive.IsReady)) Then
                    ready = old?.Ready
                End If
                result.Add(drive.DriveName, New PollingDriveState(drive, type, ready))
            Next
            Return result
        End Function

        ''' <summary>Merkt Typen auch für später gefilterte Entfernungsereignisse vor.</summary>
        ''' <param name="state">Aktuelle Laufwerkszustände.</param>
        Private Sub RememberPollingTypes(state As Dictionary(Of String, PollingDriveState))
            SyncLock Me._notificationSyncRoot
                For Each entry In state
                    If entry.Value.DriveType <> DriveType.Unknown Then Me._driveTypes(entry.Key) = entry.Value.DriveType
                Next
            End SyncLock
        End Sub

        ''' <summary>Führt den zeitgesteuerten Vergleich auf dem Fensterthread aus.</summary>
        ''' <param name="sender">Der auslösende Timer.</param>
        ''' <param name="e">Die Timer-Ereignisdaten.</param>
        Private Sub PollingTimer_Tick(sender As Object, e As EventArgs) Handles pollingTimer.Tick
            Me.PollNow()
        End Sub

        ''' <summary>Vergleicht einen vollständigen neuen Zustand und meldet daraus abgeleitete Änderungen.</summary>
        ''' <remarks>Ein Fehler bei der Aufzählung wird weitergegeben und verändert den vorherigen Zustand nicht.</remarks>
        Friend Sub PollNow()
            Me.CheckPollingThread()
            If Not Me.PollingEnabled OrElse Me._pollingBusy Then Return
            Me._pollingBusy = True
            Try
                Dim previous = Me._pollingState
                Dim current = Me.CreatePollingState(Me._pollingSource(), previous)
                Me._pollingState = current
                For Each entry In previous
                    If Not Me.PollingEnabled Then Return
                    Dim nextState As PollingDriveState = Nothing
                    If Not current.TryGetValue(entry.Key, nextState) OrElse IsNetworkReplacement(entry.Value, nextState) Then
                        Me.PublishPollingRemoval(entry.Key, entry.Value.DriveType)
                    End If
                Next
                For Each entry In current
                    If Not Me.PollingEnabled Then Return
                    Dim old As PollingDriveState = Nothing
                    If Not previous.TryGetValue(entry.Key, old) OrElse IsNetworkReplacement(old, entry.Value) Then
                        Me.PublishPollingAddition(entry.Value, False)
                    ElseIf (entry.Value.DriveType = DriveType.Removable OrElse entry.Value.DriveType = DriveType.CDRom) AndAlso old.Ready.HasValue AndAlso entry.Value.Ready.HasValue AndAlso old.Ready.Value <> entry.Value.Ready.Value Then
                        If entry.Value.Ready.Value Then
                            Me.PublishPollingAddition(entry.Value, True)
                        Else
                            Me.OnMediaRemoved(New DriveWatcherRemovedEventArgs(entry.Key))
                        End If
                    End If
                Next
                Me.RememberPollingTypes(current)
            Finally
                Me._pollingBusy = False
            End Try
        End Sub

        ''' <summary>Erkennt den Wechsel zwischen Netzwerk- und anderem bekanntem Laufwerkstyp.</summary>
        ''' <param name="old">Vorheriger Zustand.</param>
        ''' <param name="current">Aktueller Zustand.</param>
        ''' <returns>Ob ein Entfernungs- und Hinzufügeereignis erforderlich ist.</returns>
        Private Shared Function IsNetworkReplacement(old As PollingDriveState, current As PollingDriveState) As Boolean
            Return old.DriveType <> DriveType.Unknown AndAlso current.DriveType <> DriveType.Unknown AndAlso old.DriveType = DriveType.Network <> (current.DriveType = DriveType.Network)
        End Function

        ''' <summary>Meldet eine erkannte Entfernung mit dem zuletzt bekannten Typ.</summary>
        ''' <param name="name">Name des entfernten Laufwerks.</param>
        ''' <param name="driveType">Zuletzt bekannter Laufwerkstyp.</param>
        Private Sub PublishPollingRemoval(name As String, driveType As DriveType)
            If driveType = DriveType.Network Then
                Me.OnNetworkDriveRemoved(New DriveWatcherRemovedEventArgs(name))
            Else
                Me.OnDriveRemoved(New DriveWatcherRemovedEventArgs(name))
            End If
        End Sub

        ''' <summary>Meldet ein erkanntes Laufwerk beziehungsweise ein eingelegtes Medium.</summary>
        ''' <param name="state">Aktuelle Laufwerksdaten.</param>
        ''' <param name="media">Gibt an, ob ein Medienwechsel gemeldet wird.</param>
        Private Sub PublishPollingAddition(state As PollingDriveState, media As Boolean)
            Dim data = state.Data
            If media Then
                Me.OnMediaInserted(data)
            ElseIf state.DriveType = DriveType.Network Then
                data.IsNetwork = True
                Me.OnNetworkDriveAdded(data)
            Else
                Me.OnDriveAdded(data)
            End If
        End Sub

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
            If Me.PollingEnabled Then Return
            Me.OnDriveAdded(Me.ReadDriveInfo(e, DriveWatcherEventType.Added))
        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Laufwerk.
        ''' </summary>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="e">Der Name des entfernten Laufwerks.</param>
        Private Sub NatForm_DriveRemoved(sender As Object, e As String) Handles NatForm.DriveRemoved
            If Me.PollingEnabled Then Return

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
            If Me.PollingEnabled Then Return

            Me.OnMediaInserted(Me.ReadDriveInfo(e, DriveWatcherEventType.MediaInserted))

        End Sub

        ''' <summary>
        ''' Übernimmt das interne NativeForm-Ereignis für ein entferntes Medium aus einem bestehenden
        ''' Wechsel-/CD-/DVD-Laufwerk.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen des betroffenen Laufwerks.</param>
        Private Sub NatForm_MediaRemoved(sender As Object, e As String) Handles NatForm.MediaRemoved
            If Me.PollingEnabled Then Return

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
            If Me.PollingEnabled Then Return

            Me.OnNetworkDriveAdded(Me.ReadDriveInfo(e, DriveWatcherEventType.NetworkAdded))

        End Sub

        ''' <summary>Stellt eine native Geräteänderung bei aktiviertem Diagnosemodus zu.</summary>
        ''' <param name="sender">Das native Empfangsfenster.</param>
        ''' <param name="eventCode">Numerischer Windows-Ereigniscode.</param>
        ''' <param name="deviceType">Gerätetyp oder Nothing.</param>
        ''' <param name="unitMask">Volume-Bitmaske oder Nothing.</param>
        ''' <param name="flags">Volume-Flags oder Nothing.</param>
        Private Sub NatForm_RawDeviceObserved(sender As Object, eventCode As Int64, deviceType As Int32?, unitMask As Int32?, flags As Int16?) Handles NatForm.RawDeviceObserved
            SyncLock Me._notificationSyncRoot
                If Me._notificationDisposed OrElse Not Me._diagnosticMode Then Return
            End SyncLock
            Dim data As New DriveWatcherRawDeviceEventArgs(eventCode, deviceType, unitMask, flags, DateTimeOffset.UtcNow)
            Me.DispatchEvent(Sub() RaiseEvent RawDeviceEventReceived(Me, data))
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
            If eventType = DriveWatcherEventType.NetworkAdded Then result.IsNetwork = True
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
            If Me.PollingEnabled Then Return

            Dim arg As New DriveWatcherRemovedEventArgs(e)
            Me.OnNetworkDriveRemoved(arg)

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Laufwerks.</param>
        Protected Overridable Sub OnDriveAdded(e As DriveWatcherAddedEventArgs)

            If Not Me.IsIncluded(e.DriveName, e.DriveType, True) Then Return
            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.Added) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="DriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Laufwerks.</param>
        Protected Overridable Sub OnDriveRemoved(e As DriveWatcherRemovedEventArgs)

            If Not Me.IsIncluded(e.DriveName, DriveType.Unknown, False) Then Return
            ' Verhindert doppelte Signale bei schnellen Mehrfachmeldungen derselben Quelle.
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.Removed) Then Return

            Me.DispatchEvent(Sub() RaiseEvent DriveRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaInserted"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des eingelegten Mediums.</param>
        Protected Overridable Sub OnMediaInserted(e As DriveWatcherAddedEventArgs)

            If Not Me.IsIncluded(e.DriveName, e.DriveType, True) Then Return
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.MediaInserted) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaInserted(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="MediaRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Mediums.</param>
        Protected Overridable Sub OnMediaRemoved(e As DriveWatcherRemovedEventArgs)

            If Not Me.IsIncluded(e.DriveName, DriveType.Unknown, False) Then Return
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.MediaRemoved) Then Return

            Me.DispatchEvent(Sub() RaiseEvent MediaRemoved(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveAdded"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des hinzugefügten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveAdded(e As DriveWatcherAddedEventArgs)

            If Not Me.IsIncluded(e.DriveName, DriveType.Network, True) Then Return
            If Me.IsDebouncedEvent(e.DriveName, DriveWatcherEventType.NetworkAdded) Then Return

            Me.DispatchEvent(Sub() RaiseEvent NetworkDriveAdded(Me, e))

        End Sub

        ''' <summary>
        ''' Löst das öffentliche Ereignis <see cref="NetworkDriveRemoved"/> aus.
        ''' </summary>
        ''' <param name="e">Die Ereignisdaten des entfernten Netzlaufwerks.</param>
        Protected Overridable Sub OnNetworkDriveRemoved(e As DriveWatcherRemovedEventArgs)

            If Not Me.IsIncluded(e.DriveName, DriveType.Network, True) Then Return
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
                Me._diagnosticMode = False
                Me._pollingEnabled = False
                Me._synchronizingObject = Nothing
                Me._driveTypes.Clear()
            End SyncLock
            Dim timer = Me.pollingTimer
            Me.pollingTimer = Nothing
            If timer IsNot Nothing Then timer.Dispose()
            Me._pollingState = New Dictionary(Of String, PollingDriveState)(StringComparer.OrdinalIgnoreCase)
            Me._debouncer.Clear()
        End Sub

        ''' <summary>Aktualisiert bekannte Typen und prüft beide Filter atomar.</summary>
        ''' <param name="driveName">Gemeldeter Laufwerksname.</param>
        ''' <param name="driveType">Beobachteter Typ oder Unknown bei einer Entfernung.</param>
        ''' <param name="hasType">Ob der Typ aus dem aktuellen Ereignis stammt.</param>
        ''' <returns>Ob das Ereignis beide aktiven Filter erfüllt.</returns>
        Private Function IsIncluded(driveName As String, driveType As DriveType, hasType As Boolean) As Boolean
            SyncLock Me._notificationSyncRoot
                If Me._notificationDisposed Then Return False
                If Not String.IsNullOrEmpty(driveName) Then
                    If hasType Then
                        Me._driveTypes(driveName) = driveType
                    Else
                        Dim knownType As DriveType
                        If Me._driveTypes.TryGetValue(driveName, knownType) Then driveType = knownType
                    End If
                End If
                If Me._includeLetters.Length > 0 AndAlso (String.IsNullOrEmpty(driveName) OrElse Array.IndexOf(Me._includeLetters, Char.ToUpperInvariant(driveName(0))) < 0) Then Return False
                Return Me._includeDriveTypes.Length = 0 OrElse Array.IndexOf(Me._includeDriveTypes, driveType) >= 0
            End SyncLock
        End Function

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

#Region "interne Klassen"

        ''' <summary>Speichert die zuletzt verlässlich beobachteten Vergleichswerte eines Laufwerks.</summary>
        Private NotInheritable Class PollingDriveState

            ''' <summary>Beim letzten Vergleich gelesene Ereignisdaten.</summary>
            Friend ReadOnly Data As DriveWatcherAddedEventArgs

            ''' <summary>Letzter bekannter Typ; Unknown bezeichnet einen unbekannten Typ.</summary>
            Friend ReadOnly DriveType As DriveType

            ''' <summary>Letzter bekannter Bereitschaftsstatus; Nothing bezeichnet einen unbekannten Status.</summary>
            Friend ReadOnly Ready As Boolean?

            ''' <summary>Erstellt einen unveränderlichen Vergleichszustand.</summary>
            ''' <param name="data">Aktuelle Ereignisdaten.</param>
            ''' <param name="driveType">Zuletzt bekannter Laufwerkstyp.</param>
            ''' <param name="ready">Zuletzt bekannte Bereitschaft.</param>
            Friend Sub New(data As DriveWatcherAddedEventArgs, driveType As DriveType, ready As Boolean?)
                Me.Data = data
                Me.DriveType = driveType
                Me.Ready = ready
            End Sub

        End Class

#End Region

    End Class

End Namespace
