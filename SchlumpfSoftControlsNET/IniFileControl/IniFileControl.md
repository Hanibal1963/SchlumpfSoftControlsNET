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

## [7. Optimierungsvorschläge](IniFileOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](IniFileErweiterungsvorschläge.md)