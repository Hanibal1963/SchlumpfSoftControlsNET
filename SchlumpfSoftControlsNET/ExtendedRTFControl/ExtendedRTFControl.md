# ExtendedRTFControl

Erweiterte RichTextBox für Windows Forms mit komfortablen Formatierungs- und Abfragehilfen (Schriftgröße, Stil-Flags, Farben, Einzüge, Ausrichtung) sowie Redraw-Suppression (flackerreduziertes Batch-Update).

- Toolbox: Über Attribute für die Toolbox vorbereitet (`ProvideToolboxControlAttribute`, `ToolboxItem(True)`, `ToolboxBitmap`).

- Mischzustände: Abfragen liefern, wo sinnvoll, `Nothing` (Nullable), wenn die Auswahl uneinheitlich formatiert ist.

- Redraw-Suppression: Internes, verschachtelbares Batching mittels `WM_SETREDRAW` verringert Flackern bei Massenänderungen.

- Mischzustände werden über native Rich-Edit-Formatabfragen ohne Änderung der Auswahl und ohne Unterdrückung von `SelectionChanged` ermittelt.

## 1. Konstruktoren

- `ExtendedRTF.New()`: Initialisiert das Steuerelement und seine Designerkomponenten.

---

## 2. Ereignisse

- Keine zusätzlichen öffentlichen Ereignisse definiert (es gelten die geerbten Ereignisse der `RichTextBox`).

---

## 3. Eigenschaften

- `SelectionFontSize`: Liest oder setzt die Schriftgröße der aktuellen Auswahl bzw. am Caret; bei uneinheitlicher Auswahl wird `Nothing` zurückgegeben. Beim Setzen bewirkt `Nothing` keine Änderung; Werte unter 8 lösen `ArgumentOutOfRangeException` aus.

- `SelectionBold`: Liest oder setzt den Fettdruck der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

- `SelectionItalic`: Liest oder setzt Kursiv (`Italic`) der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

- `SelectionUnderline`: Liest oder setzt Unterstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

- `SelectionStrikeout`: Liest oder setzt Durchstreichung der aktuellen Auswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben.

- `SelectionForeColor`: Liest oder setzt die aktuelle Vordergrundfarbe (Textfarbe) der Auswahl bzw. am Caret.

- `SelectionBackColor`: Liest oder setzt die aktuelle Hintergrund-/Highlightfarbe der Auswahl bzw. am Caret.

- `SelectionLeftIndent`: Liest oder setzt den linken Absatz-Einzug (in Pixel) der aktuellen Absatz-/Absatzauswahl bzw. am Caret; bei Mischzustand wird `Nothing` zurückgegeben. Beim Setzen bewirkt `Nothing` keine Änderung; negative Werte lösen `ArgumentOutOfRangeException` aus.

---

## 4. Funktionen

- `LoadRtfOrPlainText(content As String) As Boolean`: Ersetzt den Inhalt durch das bereits dekodierte RTF-Dokument. Leere Eingaben und von Rich Edit abgelehntes RTF werden als Klartext übernommen; die Rückgabe ist dann `False`, bei RTF `True`. `Nothing` löst `ArgumentNullException` aus.

- `LoadRtfOrPlainText(content As Byte(), plainTextEncoding As Encoding) As Boolean`: Liest RTF aus den Originalbytes einschließlich der darin deklarierten Codepage. Bei Ablehnung wird Klartext mit der ausdrücklich angegebenen Kodierung dekodiert; eine dazu passende BOM wird entfernt. `True` meldet RTF, `False` Klartext einschließlich leerer Eingaben. `Nothing` löst `ArgumentNullException` aus; nicht dekodierbare Klartextbytes lösen `DecoderFallbackException` aus, ohne den bisherigen Inhalt zu ändern.

- `ClearFormatting()`: Entfernt Formatierungen (Schriftstil, Vorder-/Hintergrundfarbe, Bullet-Aufzählung) aus der aktuellen Auswahl oder ohne Auswahl ab der Caret-Position.

- `SetSelectionAlignment(alignment)`: Setzt die horizontale Ausrichtung der aktuellen Absatz-/Absatzauswahl.

- `ToggleBold()`: Schaltet Fettdruck für aktuelle Auswahl bzw. Caret um.

- `ToggleItalic()`: Schaltet Kursiv für aktuelle Auswahl bzw. Caret um.

- `ToggleUnderline()`: Schaltet Unterstreichung für aktuelle Auswahl bzw. Caret um.

- `ToggleStrikeout()`: Schaltet Durchstreichung für aktuelle Auswahl bzw. Caret um.

- `ToggleBullet()`: Schaltet Bullet-Aufzählung für aktuelle Absatz-/Absatzauswahl um.

---

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

---

## 6. Hinweise

- Die Konstante `MIN_FONT_SIZE` ist in `ApiDefinitions` mit dem Wert `8.0F` definiert.

- Der Import verwendet den Windows-Rich-Edit-Parser; dessen Akzeptanz ist keine vollständige RTF-Syntaxvalidierung. Nicht unterstützte oder bereits fehlerhaft dekodierte Eingaben können nicht automatisch repariert werden. Die geerbten APIs `Rtf`, `SelectedRtf` und `LoadFile` behalten ihr bisheriges Verhalten; der Klartext-Fallback ist nur über `LoadRtfOrPlainText` verfügbar.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Stabile RichText-Formatierung mit performantem Batch-Verhalten.

---

### 7.2. Redraw-Suppression vereinheitlichen

**Status:** &#x2705; Umgesetzt

**Nutzen:** Weniger Flackern und konsistentes Repaint-Verhalten.

---

### 7.3. Font-Objekte und Stilwechsel optimieren

**Status:** &#x2705; Umgesetzt

Eine gemeinsame Font-Hilfsfunktion vermeidet unveränderte Zuweisungen. Bei Auswahltransformationen werden Fonts anhand der Ausgangsattribute wiederverwendet und anschließend freigegeben.

**Nutzen:** Geringerer GC-Druck bei intensiver Bearbeitung.

---

### 7.4. Mixed-State-Ermittlung beschleunigen

**Status:** &#x2705; Umgesetzt

Native Formatabfragen ersetzen die Selektionsscans. Ihre Ergebnisse werden nur während der jeweiligen Abfrage gehalten, damit auch Änderungen über geerbte Eigenschaften sofort sichtbar sind.

**Nutzen:** Flüssigere Toolbar-Updates bei großer Selektion.

---

### 7.5. RTF-Interoperabilität härten

**Status:** &#x2705; Umgesetzt

Ein expliziter Import mit erkennbarer Rückgabe verwendet bei Ablehnung durch Rich Edit einen Klartext-Fallback. Byte-Eingaben erhalten die RTF-Codepage; Klartext wird mit der angegebenen Kodierung streng dekodiert.

**Nutzen:** Weniger Laufzeitfehler bei externen Inhalten.

---

### 7.6. API-Dokumentation präzisieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Verhalten bei `Nothing`/Mischzustand explizit dokumentieren.

- Nebenwirkungen von Toggle-Methoden klar beschreiben.

**Nutzen:** Weniger Integrationsfehler in Host-Apps.

---

### 7.7. Empfohlene Reihenfolge

1. Dokumentation

---

## 8. geplante Erweiterungen

### 8.1. Undo/Redo-Transaktionen

**Status:** &#x1F4A1; offen

**Vorschlag:** Batch-Formatierungen als eine Undo-Operation gruppieren.

**Umsetzung:** Native Nachrichten oder internes Kommandomodell kapseln.

---

### 8.2. Stilvorlagen

**Status:** &#x1F4A1; offen

**Vorschlag:** Wiederverwendbare Textstile (`Heading`, `Code`, `Note`).

**Umsetzung:** Style-Objekte definieren und auf Selektion anwenden.

---

### 8.3. Einfüge-APIs erweitern

**Status:** &#x1F4A1; offen

**Vorschlag:** `InsertImage`, `InsertLink`, `InsertTableLikeText`.

**Umsetzung:** RTF-Snippet-Builder mit Validierung.

---

### 8.4. Exportoptionen

**Status:** &#x1F4A1; offen

**Vorschlag:** Export nach PlainText/HTML/Markdown (Basis).

**Umsetzung:** Adapter-Layer je Zielformat.

---

### 8.5. Ereignisse für Editorintegration

**Status:** &#x1F4A1; offen

**Vorschlag:** `FormattingChanged`, `SelectionStyleChanged`.

**Umsetzung:** Bestehende `SelectionChanged`-Ableitung mit Debounce ergänzen.

---

### 8.6. Empfohlene Reihenfolge

1. Undo/Redo-Transaktionen
2. Stilvorlagen
3. Einfüge-APIs erweitern
4. Exportoptionen
5. Ereignisse für Editorintegration
