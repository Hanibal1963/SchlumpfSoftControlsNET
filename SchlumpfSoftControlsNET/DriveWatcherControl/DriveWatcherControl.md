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

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `DriveWatcher` und erstellt ein Fenster für den Empfang von Geräteänderungen. Falls das Fenster nicht erstellt werden kann, kann eine `Win32Exception` auftreten.

- `New(container As IContainer)`: Initialisiert die Instanz und fügt sie dem angegebenen Komponentencontainer hinzu. `Nothing` ist zulässig und fügt die Instanz keinem Container hinzu. Fehler beim Hinzufügen werden weitergegeben; bereits angelegte Ressourcen werden dabei freigegeben. Auch bei diesem Konstruktor kann die Fenstererstellung eine `Win32Exception` auslösen.

## 2. Ereignisse

Ohne `SynchronizingObject` werden alle Ereignisse synchron auf dem Thread ausgelöst, der das Empfangsfenster erstellt hat. Bei einem konfigurierten Synchronisierungsobjekt erfolgt die Zustellung auf dessen Thread. Geschützte `On...`-Methoden, die von abgeleiteten Klassen direkt aufgerufen werden, verwenden ohne Synchronisierungsobjekt den jeweiligen aufrufenden Thread.

Gleiche Ereignisse desselben Laufwerks werden innerhalb von 200 Millisekunden nach der ersten zugelassenen Meldung unterdrückt. Laufwerksnamen werden ohne Beachtung der Groß-/Kleinschreibung verglichen; andere Laufwerke und Ereignistypen besitzen jeweils eigene Einträge. Ein entgegengesetzter Zustandswechsel (hinzugefügt/entfernt beziehungsweise Medium eingelegt/entfernt) hebt den Eintrag seines Ereignispartners auf, sodass beispielsweise Hinzufügen–Entfernen–Hinzufügen vollständig gemeldet wird. Unterdrückte Duplikate verlängern das Zeitfenster nicht. Die Prüfung erfolgt vor einer gegebenenfalls asynchronen Zustellung.

- `DriveAdded`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.

- `DriveRemoved`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.

- `MediaInserted`: Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.

- `MediaRemoved`: Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.

- `NetworkDriveAdded`: Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.

- `NetworkDriveRemoved`: Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.

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

## 4. Funktionen

- `Dispose()`: Meldet die internen Ereignishandler ab, verwirft noch nicht begonnene vorgemerkte Zustellungen und gibt das Empfangsfenster sowie die verwalteten Komponenten frei. Die Referenz auf das Synchronisierungsobjekt wird entfernt; das Objekt selbst wird nicht freigegeben. Bereits laufende Rückrufe können noch beendet werden. Ein mehrfacher Aufruf ist zulässig. Ein Komponentencontainer gibt die bei ihm angemeldete Instanz ebenfalls frei, wenn er selbst freigegeben wird.

## 5. Hilfstypen/Strukturen

- `DriveWatcherAddedEventArgs`: Enthält `DriveName`, `DriveType`, `IsReady`, `VolumeLabel`, `DriveFormat`, `AvailableFreeSpace`, `TotalFreeSpace` und `TotalSize`. Neben dem parameterlosen Konstruktor akzeptieren die weiteren Konstruktoren den Laufwerksnamen und optional Typ und Bereitschaft. Sie führen keine Systemabfragen durch.

- `DriveWatcherAddedEventArgs.ReadErrors As IReadOnlyDictionary(Of String, Exception)`: Schreibgeschützte Fehlerzuordnung für Systemabfragen. Der Schlüssel ist der Name der betroffenen Eigenschaft; `DriveInfo` bezeichnet einen Fehler beim Erstellen der Abfrage. Bei selbst erstellten Ereignisdaten ist die Zuordnung leer. Fehlgeschlagene Felder behalten ihren neutralen Wert (`Unknown`, `False`, leere Zeichenfolge oder `0`). Bei nicht bereitem Laufwerk werden Detailfelder nicht abgefragt. Ein leeres Wörterbuch bedeutet dabei nicht, dass die Detailwerte gelesen wurden.

- `DriveWatcherRemovedEventArgs`: Enthält den gemeldeten Laufwerksnamen; beim Entfernen sind keine Metadatenabfragen erforderlich.

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

## [7. Optimierungsvorschläge](DriveWatcherOptimierungsvorschläge.md)

z.Zt. alle umgesetzt

## [8. Erweiterungsvorschläge](DriveWatcherErweiterungsvorschläge.md)