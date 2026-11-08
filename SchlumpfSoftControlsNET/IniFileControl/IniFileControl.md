# IniFileControl

Ein Set von Controls zum Verwalten und Bearbeiten von INI-Dateien.

Das Set beinhaltet die folgenden Controls:

- **IniFile**: Eine Komponente zum Laden und Bearbeiten von INI-Dateien.
- **ContentView**: Ein Control zum Anzeigen des Inhalts einer INI-Datei.
- **CommentEdit**: Ein Control zum Bearbeiten des Datei- oder Abschnittskommentars.
- **ListEdit**: Ein Control zum Auswählen, Hinzufügen, Umbenennen oder Löschen von Abschnitten oder Einträgen.
- **EntryValueEdit**: Ein Control zum Bearbeiten des Wertes eines Eintrags.

## 1. Konstruktoren

- `ContentView.New()`: Initialisiert das Control und übernimmt den initialen Titel.
- `CommentEdit.New()`: Initialisiert das Control und setzt den Startzustand.
- `ListEdit.New()`: Initialisiert das Control und übernimmt den Starttitel.
- `EntryValueEdit.New()`: Initialisiert das Control und übernimmt den Starttitel.

## 2. Ereignisse

- `IniFile.FileContentChanged`: Wird ausgelöst, wenn sich der Dateiinhalt geändert hat.
- `IniFile.SectionNameExist`: Wird ausgelöst, wenn ein Abschnittsname bereits vorhanden ist.
- `IniFile.EntryNameExist`: Wird ausgelöst, wenn ein Eintragsname bereits vorhanden ist.
- `CommentEdit.CommentChanged`: Wird ausgelöst, wenn der bearbeitete Kommentar übernommen wurde.
- `ListEdit.ItemAdd`: Wird ausgelöst, wenn ein neuer Eintrag angefordert wurde.
- `ListEdit.ItemRename`: Wird ausgelöst, wenn ein Eintrag umbenannt werden soll.
- `ListEdit.ItemRemove`: Wird ausgelöst, wenn ein Eintrag gelöscht werden soll.
- `ListEdit.SelectedItemChanged`: Wird ausgelöst, wenn sich die Auswahl geändert hat.
- `EntryValueEdit.ValueChanged`: Wird ausgelöst, wenn ein bearbeiteter Wert übernommen wurde.

## 3. Eigenschaften

- `IniFile.FileSaved`: Gibt an, ob der aktuelle Zustand bereits gespeichert wurde.
- `IniFile.CommentPrefix`: Gibt das Präfixzeichen für Kommentare zurück oder legt es fest.
- `IniFile.FileName`: Gibt den aktuellen Dateinamen zurück oder legt ihn fest.
- `IniFile.FilePath`: Gibt den Pfad zur INI-Datei zurück oder legt ihn fest.
- `IniFile.AutoSave`: Legt fest, ob Änderungen automatisch gespeichert werden.
- `ContentView.TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `ContentView.Lines`: Gibt den angezeigten Inhalt als Zeilenarray zurück oder legt ihn fest.
- `CommentEdit.TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `CommentEdit.Comment`: Gibt den Kommentar als Zeilenarray zurück oder legt ihn fest.
- `CommentEdit.SectionName`: Gibt den Namen des aktuell bearbeiteten Abschnitts zurück oder legt ihn fest.
- `ListEdit.TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `ListEdit.ListItems`: Gibt die anzuzeigenden Listeneinträge zurück oder ersetzt diese.
- `ListEdit.SelectedElement`: Gibt das aktuell ausgewählte Element zurück.
- `EntryValueEdit.TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `EntryValueEdit.SelectedSection`: Gibt den aktuell ausgewählten Abschnitt zurück oder legt ihn fest.
- `EntryValueEdit.SelectedEntry`: Gibt den aktuell ausgewählten Eintrag zurück oder legt ihn fest.
- `EntryValueEdit.Value`: Gibt den aktuell bearbeiteten Eintragswert zurück oder legt ihn fest.

## 4. Funktionen

- `IniFile.CreateNewFile()`: Erzeugt eine neue INI-Datei mit Beispielinhalt.
- `IniFile.CreateNewFile(CommentPrefix As Char)`: Erzeugt eine neue INI-Datei mit Beispielinhalt und definierbarem Kommentar-Präfix.
- `IniFile.LoadFile(FilePathAndName As String)`: Lädt eine INI-Datei über den vollständigen Pfad.
- `IniFile.LoadFile()`: Lädt die Datei aus `FilePath` und `FileName`.
- `IniFile.SaveFileAs(FilePathAndName As String)`: Speichert den aktuellen Inhalt unter einem vollständigen Zielpfad.
- `IniFile.SaveFile()`: Speichert den aktuellen Inhalt nach `FilePath` und `FileName`.
- `IniFile.GetFileContent() As String()`: Gibt den aktuellen Dateiinhalt als Zeilenarray zurück.
- `IniFile.GetFileComment() As String()`: Gibt die Kommentarzeilen im Dateikopf zurück.
- `IniFile.SetFileComment(CommentLines() As String)`: Ersetzt den Dateikopf-Kommentar vollständig.
- `IniFile.GetSectionNames() As String()`: Gibt alle Abschnittsnamen zurück.
- `IniFile.GetEntryNames(SectionName As String) As String()`: Gibt alle Eintragsnamen eines Abschnitts zurück.
- `IniFile.AddSection(Name As String)`: Fügt einen neuen Abschnitt hinzu.
- `IniFile.AddEntry(Section As String, Name As String)`: Fügt einen neuen Eintrag in einem Abschnitt hinzu.
- `IniFile.RenameSection(OldName As String, NewName As String)`: Benennt einen Abschnitt um.
- `IniFile.RenameEntry(Section As String, OldName As String, NewName As String)`: Benennt einen Eintrag um.
- `IniFile.DeleteSection(Name As String)`: Löscht einen Abschnitt samt zugehörigem Kommentar.
- `IniFile.DeleteEntry(Section As String, Entry As String)`: Löscht einen Eintrag aus einem Abschnitt.
- `IniFile.GetSectionComment(SectionName As String) As String()`: Gibt die Kommentarzeilen eines Abschnitts zurück.
- `IniFile.GetEntryValue(Section As String, Entry As String) As String`: Gibt den Wert eines Eintrags zurück.
- `IniFile.SetSectionComment(Name As String, CommentLines() As String)`: Ersetzt den Kommentar eines Abschnitts vollständig.
- `IniFile.SetEntryValue(Section As String, Entry As String, Value As String)`: Setzt den Wert eines Eintrags.

## 5. Hilfstypen/Strukturen

- `IniFile`: Kernkomponente für Laden, Speichern und Bearbeiten von INI-Dateien.
- `ContentView`: Anzeige-Control für den aktuellen INI-Inhalt.
- `CommentEdit`: Editor-Control für Datei- und Abschnittskommentare.
- `ListEdit`: Editor-Control für Abschnitte und Einträge.
- `EntryValueEdit`: Editor-Control für Eintragswerte.

## 6. Hinweise

- Die Dokumentation fasst mehrere öffentliche Controls des IniFile-Sets in einer Datei zusammen.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Konsistente, robuste INI-Verarbeitung und reaktive UI-Teilcontrols.

### 7.2. Datenmodell von UI trennen

**Umsetzung:**

- Parsing/Schreiblogik in klar getrennten Core-Service auslagern.

- UI-Controls (`ContentView`, `CommentEdit`, `ListEdit`, `EntryValueEdit`) nur als Präsentationsschicht nutzen.

**Nutzen:** Bessere Testbarkeit und geringere Kopplung.

### 7.3. Parser robuster machen

**Umsetzung:**

- Edge-Cases abdecken: leere Zeilen, Escaping, doppelte Keys, BOM/Encoding.

- Fehler- und Warnmodell (z. B. ParseWarnings) ergänzen.

**Nutzen:** Stabileres Verhalten bei realen INI-Varianten.

### 7.4. Änderungsverfolgung verbessern

**Umsetzung:**

- Dirty-Tracking pro Abschnitt/Eintrag einführen.

- `AutoSave` entprellen (Debounce) und atomisch speichern.

**Nutzen:** Weniger I/O-Spitzen, geringeres Datenverlustrisiko.

### 7.5. Eventmodell konsolidieren

**Umsetzung:**

- Einheitliche Events für Add/Rename/Delete mit gemeinsamen EventArgs.

- Reihenfolge und Garantien der Events dokumentieren.

**Nutzen:** Einfachere Anbindung im Host-Code.

### 7.6. Performance bei großen Dateien

**Umsetzung:**

- Interne Lookup-Strukturen für Sections/Entries (Dictionary) ergänzen.

- `Get*`-Methoden ohne wiederholtes lineares Scannen umsetzen.

**Nutzen:** Spürbar schneller bei großen INI-Dateien.

### 7.7. Empfohlene Reihenfolge

1. Parser-Härtung
2. Datenmodell-Trennung
3. Dirty-Tracking/Save
4. Lookup-Optimierung
5. Eventkonsolidierung

## 8. Erweiterungsvorschläge

### 8.1. Schema-/Validierungsregeln

**Vorschlag:** Optionales INI-Schema (Pflichtsektionen, Pflichtkeys, Datentypen).
**Umsetzung:** Validierungs-Engine mit Ergebnisobjekt (Fehler/Warnungen).

### 8.2. Include- und Merge-Unterstützung

**Vorschlag:** Mehrere INI-Dateien zusammenführen (`include=`-Konzept).
**Umsetzung:** Resolver mit Prioritätsregeln und Konfliktstrategie.

### 8.3. Historie und Undo/Redo

**Vorschlag:** Änderungsverlauf für Abschnitt/Eintrag.
**Umsetzung:** Kommandomodell über Add/Rename/Delete/SetValue.

### 8.4. Vergleichsansicht

**Vorschlag:** Zwei INI-Dateien gegenüberstellen.
**Umsetzung:** Diff-API plus optionale Visualisierung in `ContentView`.

### 8.5. Erweiterte Suche

**Vorschlag:** Volltext- und strukturierte Suche über Schlüssel/Werte/Kommentare.
**Umsetzung:** Indizierung beim Laden, Filter-API für UI-Controls.
