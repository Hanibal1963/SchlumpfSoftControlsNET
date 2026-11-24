# DriveWatcherDriveSnapshot

Unveränderliche Laufwerksinformationen, die von `DriveWatcher.GetCurrentDrives()` ermittelt werden. Jede Instanz enthält den Zustand zum jeweiligen Abfragezeitpunkt und gegebenenfalls feldbezogene Lesefehler.

## 1. Konstruktoren

Es gibt keinen öffentlichen Konstruktor. Instanzen werden durch `DriveWatcher.GetCurrentDrives()` erstellt.

## 2. Ereignisse

Keine.

## 3. Eigenschaften

- `DriveName As String`: Betriebssystemname des Laufwerks, beispielsweise `C:\`.
- `DriveType As System.IO.DriveType`: Laufwerkstyp; bei einem Lesefehler `Unknown`.
- `IsReady As Boolean`: Bereitschaft zum Zeitpunkt der Abfrage; bei einem Lesefehler `False`.
- `VolumeLabel As String` und `DriveFormat As String`: Bezeichnung und Dateisystem; bei nicht verfügbarem Wert jeweils eine leere Zeichenfolge.
- `AvailableFreeSpace As Long`, `TotalFreeSpace As Long` und `TotalSize As Long`: Verfügbarer Benutzerspeicher, gesamter freier Speicher und Gesamtgröße in Bytes; bei nicht verfügbarem Wert jeweils `0`.
- `ReadErrors As IReadOnlyDictionary(Of String, Exception)`: Schreibgeschützte Zuordnung fehlgeschlagener Einzelabfragen. Der Schlüssel ist der Eigenschaftsname; `DriveInfo` bezeichnet einen Fehler beim Erstellen der Laufwerksabfrage.

Alle Eigenschaften sind schreibgeschützt. Bei nicht bereitem Laufwerk werden Bezeichnung, Dateisystem und Speichergrößen nicht gelesen; eine leere Fehlerzuordnung bestätigt daher nicht, dass diese Werte verfügbar waren.

## 4. Funktionen

Keine eigenen öffentlichen Methoden.

## 5. Hilfstypen/Strukturen

Keine.

## 6. Hinweise

Die Werte werden nacheinander gelesen und sind nicht atomar. `AvailableFreeSpace` kann wegen Benutzerkontingenten von `TotalFreeSpace` abweichen. Die Daten werden nach Erstellung nicht aktualisiert; für neue Werte ist ein erneuter Aufruf erforderlich.

## 7. geplante Optimierungen

Derzeit keine.

## 8. geplante Erweiterungen

Derzeit keine.