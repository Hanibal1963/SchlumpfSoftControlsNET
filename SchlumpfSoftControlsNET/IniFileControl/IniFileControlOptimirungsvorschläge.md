# Optimierungsvorschläge – IniFileControl

## Zielbild

Konsistente, robuste INI-Verarbeitung und reaktive UI-Teilcontrols.

## 1) Datenmodell von UI trennen

**Umsetzung:**

- Parsing/Schreiblogik in klar getrennten Core-Service auslagern.

- UI-Controls (`ContentView`, `CommentEdit`, `ListEdit`, `EntryValueEdit`) nur als Präsentationsschicht nutzen.

**Nutzen:** Bessere Testbarkeit und geringere Kopplung.

## 2) Parser robuster machen

**Umsetzung:**

- Edge-Cases abdecken: leere Zeilen, Escaping, doppelte Keys, BOM/Encoding.

- Fehler- und Warnmodell (z. B. ParseWarnings) ergänzen.

**Nutzen:** Stabileres Verhalten bei realen INI-Varianten.

## 3) Änderungsverfolgung verbessern

**Umsetzung:**

- Dirty-Tracking pro Abschnitt/Eintrag einführen.

- `AutoSave` entprellen (Debounce) und atomisch speichern.

**Nutzen:** Weniger I/O-Spitzen, geringeres Datenverlustrisiko.

## 4) Eventmodell konsolidieren

**Umsetzung:**

- Einheitliche Events für Add/Rename/Delete mit gemeinsamen EventArgs.

- Reihenfolge und Garantien der Events dokumentieren.

**Nutzen:** Einfachere Anbindung im Host-Code.

## 5) Performance bei großen Dateien

**Umsetzung:**

- Interne Lookup-Strukturen für Sections/Entries (Dictionary) ergänzen.

- `Get*`-Methoden ohne wiederholtes lineares Scannen umsetzen.

**Nutzen:** Spürbar schneller bei großen INI-Dateien.

## Empfohlene Reihenfolge

1. Parser-Härtung

2. Datenmodell-Trennung

3. Dirty-Tracking/Save

4. Lookup-Optimierung

5. Eventkonsolidierung
