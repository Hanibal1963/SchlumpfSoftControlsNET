# DriveWatcher Control

Ein Control zum Überwachen von logischen Laufwerken (Volumes) unter Windows.

Grundlage und Anregung für dieses Control stammen aus dem Internet.

[ActiveVB - VB.NET Tipp 0055: Hinzufügen und Entfernen von USB-Wechselmedien erkennen](http://www.activevb.de/tipps/vbnettipps/tipp0055.html)

Wenn ein neues Laufwerk angeschlossen oder erstellt wird (z. B. eine virtuelle Festplatte), wird ein Ereignis ausgelöst und es werden verschiedene Eigenschaften übergeben.

Wenn ein Laufwerk getrennt wird, wird ebenfalls ein Ereignis ausgelöst und der Laufwerksname übergeben.

DriveWatcher Control wurde mit folgenden Geräten getestet:

- USB-Stick

- CD- oder DVD-Laufwerk (fest verbaut oder über USB-Adapter)

- Festplatten über USB-Adapter

- USB-Floppylaufwerk

- Virtuelle Festplatte

- Netzlaufwerk

---

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `DriveWatcher` und erstellt ein Fenster für den Empfang von Geräteänderungen. Falls das Fenster nicht erstellt werden kann, kann eine `Win32Exception` auftreten.

- `New(container As IContainer)`: Initialisiert die Instanz und fügt sie dem angegebenen Komponentencontainer hinzu. `Nothing` ist zulässig und fügt die Instanz keinem Container hinzu. Fehler beim Hinzufügen werden weitergegeben; bereits angelegte Ressourcen werden dabei freigegeben. Auch bei diesem Konstruktor kann die Fenstererstellung eine `Win32Exception` auslösen.

---

## 2. Ereignisse

Ohne `SynchronizingObject` werden alle Ereignisse synchron auf dem Thread ausgelöst, der das Empfangsfenster erstellt hat. Bei einem konfigurierten Synchronisierungsobjekt erfolgt die Zustellung auf dessen Thread. Geschützte `On...`-Methoden, die von abgeleiteten Klassen direkt aufgerufen werden, verwenden ohne Synchronisierungsobjekt den jeweiligen aufrufenden Thread.

Gleiche Ereignisse desselben Laufwerks werden innerhalb von 200 Millisekunden nach der ersten zugelassenen Meldung unterdrückt. Laufwerksnamen werden ohne Beachtung der Groß-/Kleinschreibung verglichen; andere Laufwerke und Ereignistypen besitzen jeweils eigene Einträge. Ein entgegengesetzter Zustandswechsel (hinzugefügt/entfernt beziehungsweise Medium eingelegt/entfernt) hebt den Eintrag seines Ereignispartners auf, sodass beispielsweise Hinzufügen–Entfernen–Hinzufügen vollständig gemeldet wird. Unterdrückte Duplikate verlängern das Zeitfenster nicht. Die Prüfung erfolgt vor einer gegebenenfalls asynchronen Zustellung.

- `DriveAdded`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.

- `DriveRemoved`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.

- `MediaInserted`: Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.

- `MediaRemoved`: Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.

- `NetworkDriveAdded`: Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.

- `NetworkDriveRemoved`: Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.

---

## 3. Eigenschaften

- `TraceLevel As System.Diagnostics.TraceLevel`: Instanzbezogene Diagnosestufe, standardmäßig `Off`. `Error` meldet Lesefehler mit Ereignistyp, Laufwerk, Feld, Ausnahmetyp und Meldung; `Warning` enthält derzeit dieselben Meldungen. `Info` ergänzt alle verarbeiteten Ereignisse einschließlich ihrer Unterdrückungsentscheidung; `Verbose` ergänzt native Volume-Rohmeldungen mit Windows-Ereigniscode, Laufwerksmaske und Flags. Jeder Eintrag enthält einen UTC-Zeitstempel im Roundtrip-Format. Ungültige Enumwerte lösen `InvalidEnumArgumentException` aus, das Setzen nach `Dispose()` löst `ObjectDisposedException` aus.

```vb
DriveWatcher1.TraceLevel = System.Diagnostics.TraceLevel.Verbose
```

- `SynchronizingObject As System.ComponentModel.ISynchronizeInvoke`: Optionales Ziel für die Zustellung aller sechs Ereignisse; der Standardwert ist `Nothing`. `Nothing` stellt die direkte Zustellung wieder her. Ist `InvokeRequired` wahr, wird die Zustellung asynchron mit `BeginInvoke` vorgemerkt, andernfalls erfolgt sie direkt. Ein Windows Forms-Steuerelement muss bei der Zustellung ein bereits erstelltes Fensterhandle besitzen; andernfalls wird `InvalidOperationException` ausgelöst. Das Setzen nach `Dispose()` löst `ObjectDisposedException` aus. Die Eigenschaft wird zur Laufzeit gesetzt und nicht vom Designer serialisiert.

```vb
' Nach Erstellung des Formularhandles, zum Beispiel im Shown-Ereignis:
DriveWatcher1.SynchronizingObject = Me
```

---

## 4. Funktionen

- `Dispose()`: Meldet die internen Ereignishandler ab, verwirft noch nicht begonnene vorgemerkte Zustellungen und gibt das Empfangsfenster sowie die verwalteten Komponenten frei. Die Referenz auf das Synchronisierungsobjekt wird entfernt; das Objekt selbst wird nicht freigegeben. Bereits laufende Rückrufe können noch beendet werden. Ein mehrfacher Aufruf ist zulässig. Ein Komponentencontainer gibt die bei ihm angemeldete Instanz ebenfalls frei, wenn er selbst freigegeben wird.

---

## 5. Hilfstypen/Strukturen

- `DriveWatcherAddedEventArgs`: Enthält `DriveName`, `DriveType`, `IsReady`, `VolumeLabel`, `DriveFormat`, `AvailableFreeSpace`, `TotalFreeSpace` und `TotalSize`. Neben dem parameterlosen Konstruktor akzeptieren die weiteren Konstruktoren den Laufwerksnamen und optional Typ und Bereitschaft. Sie führen keine Systemabfragen durch.

- `DriveWatcherAddedEventArgs.ReadErrors As IReadOnlyDictionary(Of String, Exception)`: Schreibgeschützte Fehlerzuordnung für Systemabfragen. Der Schlüssel ist der Name der betroffenen Eigenschaft; `DriveInfo` bezeichnet einen Fehler beim Erstellen der Abfrage. Bei selbst erstellten Ereignisdaten ist die Zuordnung leer. Fehlgeschlagene Felder behalten ihren neutralen Wert (`Unknown`, `False`, leere Zeichenfolge oder `0`). Bei nicht bereitem Laufwerk werden Detailfelder nicht abgefragt. Ein leeres Wörterbuch bedeutet dabei nicht, dass die Detailwerte gelesen wurden.

- `DriveWatcherRemovedEventArgs`: Enthält den gemeldeten Laufwerksnamen; beim Entfernen sind keine Metadatenabfragen erforderlich.

---

## 6. Hinweise

- Das Control reagiert auf Laufwerksänderungen des Betriebssystems und meldet diese über die genannten Ereignisse.

- Die Informationen hinzugefügter Laufwerke werden unabhängig voneinander gelesen. Erwartete Zugriffs- und Systemfehler stehen in `ReadErrors`; andere Felder bleiben erhalten. `IsReady` kann vom Betriebssystem auch ohne Ausnahme `False` melden, sodass die Ursache nicht immer erkennbar ist. Unerwartete Ausnahmen werden nicht unterdrückt. Die Komponente führt keine WMI-Abfragen durch.

- Nur Ankunfts- und vollständige Entfernungsbenachrichtigungen mit gültiger Volume-Struktur werden ausgewertet. Fehlende oder zu kurze Strukturen und ungültige Laufwerksmasken werden über `Trace.TraceWarning` gemeldet. Bei mehreren gesetzten Laufwerksbits wird weiterhin das erste ausgewertet.

- Jede Instanz besitzt genau ein eigenes unsichtbares Empfangsfenster. Windows sendet die standardmäßigen Volume-Benachrichtigungen an dieses Fenster; eine zusätzliche Geräteanmeldung ist dafür nicht erforderlich.

- Das Fenster wird auf dem Thread erstellt, der den Konstruktor aufruft. Für den automatischen Empfang muss dieser Thread eine Windows-Nachrichtenschleife betreiben. `Dispose()` sollte auf demselben Thread aufgerufen werden, damit das Fenster unmittelbar zerstört wird; bei einem Aufruf von einem anderen Thread kann sich die Zerstörung bis zur Verarbeitung einer Fensternachricht verzögern.

- Das Synchronisierungsobjekt muss während der Zustellung verfügbar bleiben; sein Thread benötigt für asynchrone Aufrufe eine Nachrichtenschleife. Eine Änderung von `SynchronizingObject` betrifft nachfolgende Ereignisse, nicht bereits vorgemerkte Zustellungen. Die Handlerliste wird erst bei der tatsächlichen Zustellung ausgewertet.

- Fehler beim Vormerken werden weitergegeben. Ausnahmen asynchroner Handler folgen der Fehlerbehandlung des jeweiligen `ISynchronizeInvoke`-Objekts; bei Windows Forms erfolgt diese über den zugehörigen Thread. Für den nativen Nachrichtenweg gelten außerdem die Fehlerbehandlungsregeln von Windows Forms.

- Die Komponente ist für Windows und Windows Forms vorgesehen.

- Das Entprellungsfenster verwendet eine monotone Zeitmessung und ist nicht konfigurierbar. Abgelaufene Einträge werden bei nachfolgenden Ereignissen entfernt; beim Freigeben wird der Cache geleert.

- Die optionale Diagnose schreibt synchron auf dem empfangenden beziehungsweise aufrufenden Thread über `System.Diagnostics.Trace.Listeners`. Die Anwendung verwaltet ihre Listener selbst; sie muss einen geeigneten Listener konfigurieren und bei Bedarf leeren. Trace-Ausgaben benötigen ein mit `TRACE` kompiliertes Control. Listenerfehler werden nicht abgefangen und können die Ereignisverarbeitung unterbrechen. Ausnahmetexte und Laufwerksnamen können sensible Informationen enthalten. Die vorhandenen Warnungen bei ungültigen Windows-Nachrichten bleiben unabhängig von `TraceLevel` aktiv.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Robustes und fehlertolerantes Laufwerks-Monitoring mit stabilen Events.

---

### 7.2. Event-Deduplizierung einführen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Kurzzeit-Cache je Laufwerk und Ereignistyp mit monotoner Zeitmessung und automatischer Entfernung abgelaufener Einträge.

- Gleiche Ereignisse innerhalb von 200 Millisekunden unterdrücken. Entgegengesetzte Zustandswechsel geben das zugehörige Ereignis wieder frei; verschiedene Laufwerke und Ereignistypen bleiben unabhängig.

**Nutzen:** Verhindert Event-Stürme bei USB-/Volume-Flapping.

---

### 7.3. Threading klarstellen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Über das optionale `SynchronizingObject` können alle Ereignisse auf dessen Thread zugestellt werden; bei erforderlichem Threadwechsel erfolgt dies asynchron.

- Ohne Synchronisierungsobjekt bleiben Geräteereignisse auf dem Thread des Empfangsfensters. Ein Windows Forms-Ziel benötigt ein erstelltes Fensterhandle und eine Nachrichtenschleife.

- Vorgemerkte, noch nicht begonnene Zustellungen werden bei `Dispose` verworfen; bereits laufende Rückrufe können noch beendet werden.

**Nutzen:** Weniger Cross-Thread-Fehler in Host-Anwendungen.

### 7.4. WMI/Win32-Fehlerpfade absichern

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Laufwerksinformationen werden unabhängig voneinander abgefragt; erwartete Fehler werden in `ReadErrors` bereitgestellt. WMI wird nicht verwendet.

- Teilinformationen bleiben in den EventArgs erhalten. Ungültige Win32-Nachrichten werden geprüft und diagnostiziert.

**Nutzen:** Höhere Stabilität bei exotischer Hardware.

---

### 7.5. Lifecycle und Cleanup schärfen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Pro Instanz wird genau ein Empfangsfenster erstellt; eine zusätzliche Anmeldung für die standardmäßigen Volume-Broadcasts ist nicht erforderlich.

- Die Ereignishandler werden vor der Freigabe des Fensters abgemeldet. Ein vorhandenes Fensterhandle kann nicht doppelt erstellt und nach `Dispose` nicht erneut registriert werden.

- Schlägt das Hinzufügen zu einem Komponentencontainer fehl, werden bereits angelegte Ressourcen freigegeben. Der Container kann eine erfolgreich hinzugefügte Instanz ebenfalls freigeben.

**Nutzen:** Keine Handle-Leaks bei dynamischem Laden/Entladen.

---

### 7.6. Diagnoselogs optional bereitstellen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Die instanzbezogene Eigenschaft `TraceLevel` ist standardmäßig ausgeschaltet. Ab `Info` werden Ereignistyp, Laufwerk, UTC-Zeitstempel und Unterdrückungsentscheidung protokolliert; `Verbose` ergänzt die nativen Rohdaten. Lesefehler sind ab `Error` sichtbar.

**Nutzen:** Schnellere Ursachenanalyse in Produktion.

---

### 7.7. Empfohlene Reihenfolge

Aktuell sind keine Optimierungspunkte mehr offen.

---

## 8. geplante Erweiterungen

### 8.1. Filterbare Events

**Status:** &#x1F4A1; offen

**Vorschlag:** Nur bestimmte DriveTypes oder Buchstaben melden.

**Umsetzung:** Filter-Properties (`IncludeDriveTypes`, `IncludeLetters`) vor Eventauslösung prüfen.

---

### 8.2. Snapshot-API

**Status:** &#x1F4A1; offen

**Vorschlag:** `GetCurrentDrives()` mit Metadaten (Typ, Label, Dateisystem, frei/belegt).

**Umsetzung:** Einheitliches DriveInfo-Modell definieren.

---

### 8.3. Erweiterte EventArgs

**Status:** &#x1F4A1; offen

**Vorschlag:** Seriennummer und IsNetwork. Dateisystem (`DriveFormat`) und `VolumeLabel` sind bereits in `DriveWatcherAddedEventArgs` enthalten.

**Umsetzung:** EventArgs-Klassen erweitern und Abwärtskompatibilität sichern.

---

### 8.4. Polling-Fallback

**Status:** &#x1F4A1; offen

**Vorschlag:** Optionaler Fallback-Modus bei fehlenden Systemevents.

**Umsetzung:** Intervallbasierten Vergleich gegen letzten Snapshot.

---

### 8.5. Diagnosemodus

**Status:** &#x1F4A1; offen

**Vorschlag:** `DiagnosticMode` mit Rohereignis-Feed.

**Umsetzung:** Separates Event `RawDeviceEventReceived` ergänzen.
