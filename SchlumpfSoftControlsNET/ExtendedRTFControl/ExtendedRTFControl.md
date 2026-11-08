# ExtendedRTF Control

Erweiterte RichTextBox für Windows Forms mit komfortablen Formatierungs- und Abfragehilfen (Schriftgröße, Stil-Flags, Farben, Einzüge, Ausrichtung) sowie Redraw-Suppression (flackerreduziertes Batch-Update).

- Toolbox: Über Attribute für die Toolbox vorbereitet (`ProvideToolboxControlAttribute`, `ToolboxItem(True)`, `ToolboxBitmap`).
- Mischzustände: Abfragen liefern, wo sinnvoll, `Nothing` (Nullable), wenn die Auswahl uneinheitlich formatiert ist.
- Redraw-Suppression: Internes, verschachtelbares Batching mittels `WM_SETREDRAW` verringert Flackern bei Massenänderungen.
- Ereignis-Steuerung: Interne Scans unterdrücken `SelectionChanged`, um UI-Flackern und Feedback-Schleifen zu vermeiden.

## 1. Konstruktoren

- Keine zusätzlichen öffentlichen Konstruktoren dokumentiert.

## 2. Ereignisse

- Keine zusätzlichen öffentlichen Ereignisse definiert (es gelten die geerbten Ereignisse der `RichTextBox`).

## 3. Eigenschaften

- `SelectionFontSize`: Liest oder setzt die Schriftgröße der aktuellen Auswahl bzw. am Caret; bei uneinheitlicher Auswahl wird `Nothing` zurückgegeben.
- `SelectionBold`: Liest oder setzt den Fettdruck der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionItalic`: Liest oder setzt Kursiv (`Italic`) der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionUnderline`: Liest oder setzt Unterstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionStrikeout`: Liest oder setzt Durchstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionForeColor`: Liest oder setzt die aktuelle Vordergrundfarbe (Textfarbe) der Auswahl bzw. am Caret.
- `SelectionBackColor`: Liest oder setzt die aktuelle Hintergrund-/Highlightfarbe der Auswahl bzw. am Caret.
- `SelectionLeftIndent`: Liest oder setzt den linken Absatz-Einzug (in Pixel) der aktuellen Absatz-/Absatzauswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

## 4. Funktionen

- `ClearFormatting()`: Entfernt Formatierungen (Schriftstil, Vorder-/Hintergrundfarbe, Bullet-Aufzählung) aus der aktuellen Auswahl oder ohne Auswahl ab der Caret-Position.
- `SetSelectionAlignment(alignment)`: Setzt die horizontale Ausrichtung der aktuellen Absatz-/Absatzauswahl.
- `ToggleBold()`: Schaltet Fettdruck für aktuelle Auswahl bzw. Caret um.
- `ToggleItalic()`: Schaltet Kursiv für aktuelle Auswahl bzw. Caret um.
- `ToggleUnderline()`: Schaltet Unterstreichung für aktuelle Auswahl bzw. Caret um.
- `ToggleStrikeout()`: Schaltet Durchstreichung für aktuelle Auswahl bzw. Caret um.
- `ToggleBullet()`: Schaltet Bullet-Aufzählung für aktuelle Absatz-/Absatzauswahl um.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Die Konstante `MIN_FONT_SIZE` wird verwendet und muss im Projekt definiert sein (z. B. als `Private Const MIN_FONT_SIZE As Single = 6.0F` in der Klasse).

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Stabile RichText-Formatierung mit performantem Batch-Verhalten.

### 7.2. Redraw-Suppression vereinheitlichen

**Umsetzung:**

- Alle formatierenden Methoden in ein gemeinsames Batch-API einhängen.

- Verschachtelungszähler atomar verwalten.

**Nutzen:** Weniger Flackern und konsistentes Repaint-Verhalten.

### 7.3. Font-Objekte und Stilwechsel optimieren

**Umsetzung:**

- Wiederholte `New Font(...)`-Erzeugungen in Utility kapseln.

- Nur bei effektiver Stiländerung neu setzen.

**Nutzen:** Geringerer GC-Druck bei intensiver Bearbeitung.

### 7.4. Mixed-State-Ermittlung beschleunigen

**Umsetzung:**

- Selektionsscans minimieren und Ergebnisse temporär cachen.

- Ereignisunterdrückung granularer steuern.

**Nutzen:** Flüssigere Toolbar-Updates bei großer Selektion.

### 7.5. RTF-Interoperabilität härten

**Umsetzung:**

- Grenzfälle (leer, ungültiges RTF, fremde Encodings) robust behandeln.

- Fallback auf PlainText definieren.

**Nutzen:** Weniger Laufzeitfehler bei externen Inhalten.

### 7.6. API-Dokumentation präzisieren

**Umsetzung:**

- Verhalten bei `Nothing`/Mischzustand explizit dokumentieren.

- Nebenwirkungen von Toggle-Methoden klar beschreiben.

**Nutzen:** Weniger Integrationsfehler in Host-Apps.

### 7.7. Empfohlene Reihenfolge

1. Batch-API
2. Mixed-State-Performance
3. Font-/Stiloptimierung
4. Interoperabilität
5. Dokumentation

## 8. Erweiterungsvorschläge

### 8.1. Undo/Redo-Transaktionen

**Vorschlag:** Batch-Formatierungen als eine Undo-Operation gruppieren.
**Umsetzung:** Native Nachrichten oder internes Kommandomodell kapseln.

### 8.2. Stilvorlagen

**Vorschlag:** Wiederverwendbare Textstile (`Heading`, `Code`, `Note`).
**Umsetzung:** Style-Objekte definieren und auf Selektion anwenden.

### 8.3. Einfüge-APIs erweitern

**Vorschlag:** `InsertImage`, `InsertLink`, `InsertTableLikeText`.
**Umsetzung:** RTF-Snippet-Builder mit Validierung.

### 8.4. Exportoptionen

**Vorschlag:** Export nach PlainText/HTML/Markdown (Basis).
**Umsetzung:** Adapter-Layer je Zielformat.

### 8.5. Ereignisse für Editorintegration

**Vorschlag:** `FormattingChanged`, `SelectionStyleChanged`.
**Umsetzung:** Bestehende `SelectionChanged`-Ableitung mit Debounce ergänzen.