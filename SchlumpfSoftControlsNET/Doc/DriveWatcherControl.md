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

## Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `DriveWatcher` und richtet den internen Empfang von Geräteänderungen ein.

## Ereignisse

- `DriveAdded`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) hinzugefügt wurde.
- `DriveRemoved`: Wird ausgelöst, wenn ein logisches Laufwerk (Volume) entfernt wurde.
- `MediaInserted`: Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
- `MediaRemoved`: Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
- `NetworkDriveAdded`: Wird ausgelöst, wenn ein Netzlaufwerk hinzugefügt wurde.
- `NetworkDriveRemoved`: Wird ausgelöst, wenn ein Netzlaufwerk entfernt wurde.

## Eigenschaften

- Keine öffentlichen Eigenschaften.

## Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## Hinweise

- Das Control reagiert auf Laufwerksänderungen des Betriebssystems und meldet diese über die genannten Ereignisse.

## Optimierungs und Verbesserungsmöglichkeiten

- [Optimierungsvorschläge](DriveWatcherControlOptimirungsvorschläge.md)
- [Erweiterungsvorschläge](DriveWatcherControlErweiterunsvorschläge.md)
