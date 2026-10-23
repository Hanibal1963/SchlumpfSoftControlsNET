# ExtendedRTF Control

Erweiterte RichTextBox für Windows Forms mit komfortablen Formatierungs- und Abfrage-Hilfen (Schriftgröße, Stil-Flags, Farben, Einzüge, Ausrichtung) sowie Redraw-Suppression (flackerreduziertes Batch-Update).

- Toolbox: Über Attribute für die Toolbox vorbereitet (`ProvideToolboxControlAttribute`, `ToolboxItem(True)`, `ToolboxBitmap`).
- Mischzustände: Abfragen liefern, wo sinnvoll, `Nothing` (Nullable), wenn die Auswahl uneinheitlich formatiert ist.
- Redraw-Suppression: Internes, verschachtelbares Batching mittels `WM_SETREDRAW` verringert Flackern bei Massenänderungen.
- Ereignis-Steuerung: Interne Scans unterdrücken `SelectionChanged`, um UI-Flackern/Feedback-Schleifen zu vermeiden.

> **Hinweis:**
>
>Die Konstante `MIN_FONT_SIZE` wird verwendet, muss aber im Projekt definiert sein (z. B. als `Private Const MIN_FONT_SIZE As Single = 6.0F` in der Klasse).>

## Eigenschaften

- `SelectionFontSize`: Liest oder setzt die Schriftgröße der aktuellen Auswahl bzw. am Caret; bei uneinheitlicher Auswahl wird `Nothing` zurückgegeben.
- `SelectionBold`: Liest oder setzt den Fettdruck der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionItalic`: Liest oder setzt Kursiv (`Italic`) der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionUnderline`: Liest oder setzt Unterstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionStrikeout`: Liest oder setzt Durchstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.
- `SelectionForeColor`: Liest oder setzt die aktuelle Vordergrundfarbe (Textfarbe) der Auswahl bzw. am Caret.
- `SelectionBackColor`: Liest oder setzt die aktuelle Hintergrund-/Highlightfarbe der Auswahl bzw. am Caret.
- `SelectionLeftIndent`: Liest oder setzt den linken Absatz-Einzug (in Pixel) der aktuellen Absatz-/Absatzauswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

## Methoden

- `ClearFormatting()`: Entfernt Formatierungen (Schriftstil, Vorder-/Hintergrundfarbe, Bullet-Aufzählung) aus der aktuellen Auswahl oder – ohne Auswahl – ab der Caret-Position.
- `SetSelectionAlignment(alignment)`: Setzt die horizontale Ausrichtung der aktuellen Absatz-/Absatzauswahl.
- `ToggleBold()`: Schaltet Fettdruck für aktuelle Auswahl bzw. Caret um.
- `ToggleItalic()`: Schaltet Kursiv für aktuelle Auswahl bzw. Caret um.
- `ToggleUnderline()`: Schaltet Unterstreichung für aktuelle Auswahl bzw. Caret um.
- `ToggleStrikeout()`: Schaltet Durchstreichung für aktuelle Auswahl bzw. Caret um.
- `ToggleBullet()`: Schaltet Bullet-Aufzählung für aktuelle Absatz-/Absatzauswahl um.

## Ereignisse

- Keine zusätzlichen öffentlichen Ereignisse definiert (es gelten die geerbten Ereignisse der `RichTextBox`).
