# Erweiterungsvorschläge – IniFileControl

## 1) Schema-/Validierungsregeln

**Vorschlag:** Optionales INI-Schema (Pflichtsektionen, Pflichtkeys, Datentypen).
**Umsetzung:** Validierungs-Engine mit Ergebnisobjekt (Fehler/Warnungen).

## 2) Include- und Merge-Unterstützung

**Vorschlag:** Mehrere INI-Dateien zusammenführen (`include=`-Konzept).
**Umsetzung:** Resolver mit Prioritätsregeln und Konfliktstrategie.

## 3) Historie und Undo/Redo

**Vorschlag:** Änderungsverlauf für Abschnitt/Eintrag.
**Umsetzung:** Kommandomodell über Add/Rename/Delete/SetValue.

## 4) Vergleichsansicht

**Vorschlag:** Zwei INI-Dateien gegenüberstellen.
**Umsetzung:** Diff-API plus optionale Visualisierung in `ContentView`.

## 5) Erweiterte Suche

**Vorschlag:** Volltext- und strukturierte Suche über Schlüssel/Werte/Kommentare.
**Umsetzung:** Indizierung beim Laden, Filter-API für UI-Controls.
