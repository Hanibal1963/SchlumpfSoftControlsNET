# SevenSegmentControl

Diese Bibliothek enthält zwei Controls:

- **SingleDigit**: Ein Control, das ein einzelnes unterstütztes Zeichen anzeigt.
- **MultiDigit**: Ein Control, das mehrere Zeichen und Dezimalpunkte anzeigen kann.

## 1. Konstruktoren

- `SingleDigit.New()`: Initialisiert eine neue Instanz der `SingleDigit`-Klasse.
- `MultiDigit.New()`: Initialisiert eine neue Instanz von `MultiDigit`.

---

## 2. Ereignisse

- `SingleDigit`: Keine eigenen öffentlichen Ereignisse.
- `MultiDigit`: Keine eigenen öffentlichen Ereignisse.

---

## 3. Eigenschaften

- `SingleDigit.InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `SingleDigit.SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `SingleDigit.ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `SingleDigit.DigitValue`: Legt das anzuzeigende Zeichen fest oder gibt dieses zurück.
- `SingleDigit.CustomBitPattern`: Legt ein benutzerdefiniertes Bitmuster für die sieben Segmente fest.
- `SingleDigit.ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `SingleDigit.DecimalPointActive`: Gibt an, ob die Dezimalpunkt-LED aktiv ist.
- `SingleDigit.ShowColon`: Gibt an, ob die Doppelpunkt-LEDs angezeigt werden.
- `SingleDigit.ColonActive`: Gibt an, ob die Doppelpunkt-LEDs aktiv sind.
- `SingleDigit.BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `SingleDigit.ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.
- `MultiDigit.InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `MultiDigit.SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `MultiDigit.ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `MultiDigit.ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `MultiDigit.DigitCount`: Anzahl der Digits; Werte von 1 bis 100 werden übernommen, andere ignoriert.
- `MultiDigit.DigitPadding`: Auffüllung, die für jedes Digit im Control gilt.
- `MultiDigit.Value`: Der auf dem Control anzuzeigende Wert.
- `MultiDigit.BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `MultiDigit.ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.

---

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

---

## 5. Hilfstypen/Strukturen

- `SingleDigit`: Einzelnes 7-Segment-Control.
- `MultiDigit`: Mehrstelliges 7-Segment-Control.

---

## 6. Hinweise

- Die Datei dokumentiert zwei öffentliche Controls in einer gemeinsamen Übersicht.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Schnelles, präzises Segment-Rendering für Single- und MultiDigit-Varianten.

---

### 7.2. Geometrie-Caching einführen

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Segmentpolygone pro Größe/ItalicFactor/SegmentWidth cachen.

- Neu berechnen nur bei relevanten Property-Änderungen.

**Nutzen:** Deutlich weniger Rechenaufwand beim Repaint.

---

### 7.3. Repaint-Granularität verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Bei MultiDigit nur betroffene Ziffern invalidieren.

- Bei gleichem `Value` keine Neuzeichnung.

**Nutzen:** Bessere Performance bei häufigen Updates.

---

### 7.4. Zeichnungsqualität steuerbar machen

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Smoothing/PixelOffset als QualityMode anbieten.

- Default auf Performance, optional HighQuality.

**Nutzen:** Besserer Kompromiss zwischen Schärfe und Geschwindigkeit.

---

### 7.5. Eingabevalidierung konsolidieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- `DigitValue`/`CustomBitPattern` zentral validieren.

- Fehlerfälle klar dokumentieren (Fallback-Zeichen).

**Nutzen:** Vorhersagbares API-Verhalten.

---

### 7.6. Farb- und Kontrastdefaults optimieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Kontraststarke Standardfarbkombinationen definieren.

- Automatische Ableitung inaktiver Farbe aus `ForeColor` ermöglichen.

**Nutzen:** Lesbarkeit out-of-the-box verbessern.

---

### 7.7. Empfohlene Reihenfolge

1. Geometrie-Caching
2. Repaint-Optimierung
3. Validierung
4. QualityMode
5. Farbdefaults

---

## 8. geplante Erweiterungen

### 8.1. Alphanumerische Musterbibliothek

**Vorschlag:** Vordefinierte Muster für A-F, Sonderzeichen, Minus.
**Umsetzung:** Erweiterte Mapping-Tabelle mit Fallback.

---

### 8.2. Blinking- und Alarmmodus

**Vorschlag:** Blinken einzelner Digits oder ganzer Anzeige.
**Umsetzung:** Timer-gesteuerter Sichtbarkeits- oder Aktivierungswechsel.

---

### 8.3. Übergangsanimationen

**Vorschlag:** Soft-Transition beim Wertwechsel.
**Umsetzung:** Interpolierte Segmenthelligkeit über mehrere Frames.

---

### 8.4. Formatierungsfunktionen

**Vorschlag:** Padding, Ausrichtung, Leading-Zeros, Dezimaltrennzeichen.
**Umsetzung:** `DisplayFormat`-Property mit Parser.

---

### 8.5. Datenquellenbindung

**Vorschlag:** Bindings für numerische Werte mit Autoformat.
**Umsetzung:** `Value`-Converter für Zahlen/Strings.

---

### 8.6. Empfohlene Reihenfolge

1. Alphanumerische Musterbibliothek
2. Blinking- und Alarmmodus
3. Übergangsanimationen
4. Formatierungsfunktionen
5. Datenquellenbindung
