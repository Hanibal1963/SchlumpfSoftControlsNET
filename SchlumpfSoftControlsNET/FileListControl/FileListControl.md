# FileList Control

FileList ist ein benutzerdefiniertes Control, das Dateien und Verzeichnisse in einem ListView darstellt.

Die folgenden Informationen werden angezeigt:

- Name der Datei oder des Ordners
- Typ der Datei (txt, pdf, ...)
- Die Größe der Datei oder des Ordners (bei der Ordnergröße wird nur der Inhalt dieses Ordners berücksichtigt, nicht die Größe der Unterordner)
- Das Erstelldatum der Datei oder des Ordners
- Das Datum des letzten Zugriffs auf die Datei oder den Ordner
- Das Datum der letzten Änderung der Datei oder des Ordners

Wenn in den Spaltenheader geklickt wird, ändert sich die Sortierung der Liste nach der aktuellen Spalte.

Die Reihenfolge der Spalten kann durch Ziehen verändert werden und wird automatisch beim Entladen des Controls gespeichert.

Die Breite der Spalten passt sich automatisch an den Inhalt an.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz der `FileList`-Klasse, richtet die Benutzeroberfläche ein und lädt die gespeicherte Spaltenreihenfolge.

## 2. Ereignisse

- Keine öffentlichen Ereignisse vorhanden.

## 3. Eigenschaften

- `StartFolder`: Ruft den Pfad des Startordners ab oder legt ihn fest.
- `AutoResizeColumnsEnabled`: Gibt an, ob die automatische Größenanpassung von Spalten aktiviert ist.
- `ColumnOrderState`: Ruft den aktuellen Zustand der Spaltenreihenfolge als Zeichenfolge ab oder legt ihn fest.

## 4. Funktionen

- `RefreshEntries()`: Aktualisiert die Einträge im `ListView` durch erneutes Laden des Startordners.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Sortierung und Spaltenreihenfolge werden über den ListView-Zustand gesteuert und können benutzerspezifisch variieren.

## [7. Optimierungsvorschläge](FileListOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](FileListErweiterungsvorschläge.md)