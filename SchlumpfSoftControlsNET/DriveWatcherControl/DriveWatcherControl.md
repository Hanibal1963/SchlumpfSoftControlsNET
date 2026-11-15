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

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `DriveWatcher` und erstellt ein Fenster für den Empfang von Geräteänderungen. Falls das Fenster nicht erstellt werden kann, kann eine `Win32Exception` auftreten.
- `New(container As IContainer)`: Initialisiert die Instanz und fügt sie dem angegebenen Komponentencontainer hinzu. `Nothing` ist zulässig und fügt die Instanz keinem Container hinzu. Fehler beim Hinzufügen werden weitergegeben; bereits angelegte Ressourcen werden dabei freigegeben. Auch bei diesem Konstruktor kann die Fenstererstellung eine `Win32Exception` auslösen.

## 2. Ereignisse

- `DriveAdded`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.
- `DriveRemoved`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.
- `MediaInserted`: Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
- `MediaRemoved`: Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
- `NetworkDriveAdded`: Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.
- `NetworkDriveRemoved`: Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.

## 3. Eigenschaften

- Keine öffentlichen Eigenschaften.

## 4. Funktionen

- `Dispose()`: Meldet die internen Ereignishandler ab und gibt das Empfangsfenster sowie die verwalteten Komponenten frei. Ein mehrfacher Aufruf ist zulässig. Ein Komponentencontainer gibt die bei ihm angemeldete Instanz ebenfalls frei, wenn er selbst freigegeben wird.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Das Control reagiert auf Laufwerksänderungen des Betriebssystems und meldet diese über die genannten Ereignisse.
- Jede Instanz besitzt genau ein eigenes unsichtbares Empfangsfenster. Windows sendet die standardmäßigen Volume-Benachrichtigungen an dieses Fenster; eine zusätzliche Geräteanmeldung ist dafür nicht erforderlich.
- Das Fenster wird auf dem Thread erstellt, der den Konstruktor aufruft. Für den automatischen Empfang muss dieser Thread eine Windows-Nachrichtenschleife betreiben. `Dispose()` sollte auf demselben Thread aufgerufen werden, damit das Fenster unmittelbar zerstört wird; bei einem Aufruf von einem anderen Thread kann sich die Zerstörung bis zur Verarbeitung einer Fensternachricht verzögern.
- Die Komponente ist für Windows und Windows Forms vorgesehen.

## [7. Optimierungsvorschläge](DriveWatcherOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](DriveWatcherErweiterungsvorschläge.md)