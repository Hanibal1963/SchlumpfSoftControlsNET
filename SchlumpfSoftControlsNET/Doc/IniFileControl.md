# IniFileControl

Ein Set von Controls zum Verwalten und bearbeiten von INI - Dateien.

Das Set beinhaltet die folgenden Controls:

- **IniFile** - Eine Komponente zum laden und bearbeiten von INI - Dateien.
- **ContentView** - Ein Control zum Anzeigen des Inhaltes einer INI - Datei.
- **CommentEdit** - Ein Control zum bearbeiten des Datei- oder Abschnittskommentars.
- **ListEdit** - Ein Control zum auswählen, hinzufügen,  umbenennen oder löschen von Abschnitten oder Einträgen.
- **EntryValueEdit** - Ein Control zum bearbeiten des Wertes eines Eintrages.

---

## Beschreibung der Controls

## Eigenschaften - IniFile

- `FileSaved`: Gibt an, ob der aktuelle Zustand bereits gespeichert wurde.
- `CommentPrefix`: Gibt das Prefixzeichen für Kommentare zurück oder legt es fest.
- `FileName`: Gibt den aktuellen Dateinamen zurück oder legt ihn fest.
- `FilePath`: Gibt den Pfad zur INI-Datei zurück oder legt ihn fest.
- `AutoSave`: Legt fest, ob Änderungen automatisch gespeichert werden.

### Methoden - IniFile

- `CreateNewFile()`: Erzeugt eine neue INI-Datei mit Beispielinhalt.
- `CreateNewFile(CommentPrefix As Char)`: Erzeugt eine neue INI-Datei mit Beispielinhalt und definierbarem Kommentar-Präfix.
- `LoadFile(FilePathAndName As String)`: Lädt eine INI-Datei über den vollständigen Pfad.
- `LoadFile()`: Lädt die Datei aus `FilePath` und `FileName`.
- `SaveFileAs(FilePathAndName As String)`: Speichert den aktuellen Inhalt unter einem vollständigen Zielpfad.
- `SaveFile()`: Speichert den aktuellen Inhalt nach `FilePath` und `FileName`.
- `GetFileContent() As String()`: Gibt den aktuellen Dateiinhalt als Zeilenarray zurück.
- `GetFileComment() As String()`: Gibt die Kommentarzeilen im Dateikopf zurück.
- `SetFileComment(CommentLines() As String)`: Ersetzt den Dateikopf-Kommentar vollständig.
- `GetSectionNames() As String()`: Gibt alle Abschnittsnamen zurück.
- `GetEntryNames(SectionName As String) As String()`: Gibt alle Eintragsnamen eines Abschnitts zurück.
- `AddSection(Name As String)`: Fügt einen neuen Abschnitt hinzu.
- `AddEntry(Section As String, Name As String)`: Fügt einen neuen Eintrag in einem Abschnitt hinzu.
- `RenameSection(OldName As String, NewName As String)`: Benennt einen Abschnitt um.
- `RenameEntry(Section As String, OldName As String, NewName As String)`: Benennt einen Eintrag um.
- `DeleteSection(Name As String)`: Löscht einen Abschnitt samt zugehörigem Kommentar.
- `DeleteEntry(Section As String, Entry As String)`: Löscht einen Eintrag aus einem Abschnitt.
- `GetSectionComment(SectionName As String) As String()`: Gibt die Kommentarzeilen eines Abschnitts zurück.
- `GetEntryValue(Section As String, Entry As String) As String`: Gibt den Wert eines Eintrags zurück.
- `SetSectionComment(Name As String, CommentLines() As String)`: Ersetzt den Kommentar eines Abschnitts vollständig.
- `SetEntryValue(Section As String, Entry As String, Value As String)`: Setzt den Wert eines Eintrags.

### Ereignisse - IniFile

- `FileContentChanged`: Wird ausgelöst, wenn sich der Dateiinhalt geändert hat.
- `SectionNameExist`: Wird ausgelöst, wenn ein Abschnittsname bereits vorhanden ist.
- `EntryNameExist`: Wird ausgelöst, wenn ein Eintragsname bereits vorhanden ist.

### Eigenschaften - ContentView

- `TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `Lines`: Gibt den angezeigten Inhalt als Zeilenarray zurück oder legt ihn fest.

### Methoden - ContentView

- `New()`: Initialisiert das Control und übernimmt den initialen Titel.

### Ereignisse - ContentView

- Keine öffentlichen Ereignisse.

### Eigenschaften - CommentEdit

- `TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `Comment`: Gibt den Kommentar als Zeilenarray zurück oder legt ihn fest.
- `SectionName`: Gibt den Namen des aktuell bearbeiteten Abschnitts zurück oder legt ihn fest.

### Methoden - CommentEdit

- `New()`: Initialisiert das Control und setzt den Startzustand.

### Ereignisse - CommentEdit

- `CommentChanged`: Wird ausgelöst, wenn der bearbeitete Kommentar übernommen wurde.

### Eigenschaften - ListEdit

- `TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `ListItems`: Gibt die anzuzeigenden Listeneinträge zurück oder ersetzt diese.
- `SelectedElement`: Gibt das aktuell ausgewählte Element zurück.

### Methoden - ListEdit

- `New()`: Initialisiert das Control und übernimmt den Starttitel.

### Ereignisse - ListEdit

- `ItemAdd`: Wird ausgelöst, wenn ein neuer Eintrag angefordert wurde.
- `ItemRename`: Wird ausgelöst, wenn ein Eintrag umbenannt werden soll.
- `ItemRemove`: Wird ausgelöst, wenn ein Eintrag gelöscht werden soll.
- `SelectedItemChanged`: Wird ausgelöst, wenn sich die Auswahl geändert hat.

### Eigenschaften - EntryValueEdit

- `TitelText`: Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
- `SelectedSection`: Gibt den aktuell ausgewählten Abschnitt zurück oder legt ihn fest.
- `SelectedEntry`: Gibt den aktuell ausgewählten Eintrag zurück oder legt ihn fest.
- `Value`: Gibt den aktuell bearbeiteten Eintragswert zurück oder legt ihn fest.

### Methoden - EntryValueEdit

- `New()`: Initialisiert das Control und übernimmt den Starttitel.

### Ereignisse - EntryValueEdit

- `ValueChanged`: Wird ausgelöst, wenn ein bearbeiteter Wert übernommen wurde.
