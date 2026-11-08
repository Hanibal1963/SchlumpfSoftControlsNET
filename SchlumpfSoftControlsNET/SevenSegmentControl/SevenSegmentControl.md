# SevenSegment Control

Diese Bibliothek enthält zwei Controls:

- **SevenSegmentSingleDigit**: Ein Control, das eine einzelne Ziffer von 0 bis 9 anzeigt.
- **MultiDigitSevenSegment**: Ein Control, das mehrere Ziffern anzeigt und damit ganze Zahlen darstellen kann.

## 1. Konstruktoren

- `SevenSegmentSingleDigit.New()`: Initialisiert eine neue Instanz der `SingleDigit`-Klasse.
- `MultiDigitSevenSegment.New()`: Initialisiert eine neue Instanz von `MultiDigit`.

## 2. Ereignisse

- `SevenSegmentSingleDigit`: Keine eigenen öffentlichen Ereignisse.
- `MultiDigitSevenSegment`: Keine eigenen öffentlichen Ereignisse.

## 3. Eigenschaften

- `SevenSegmentSingleDigit.InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `SevenSegmentSingleDigit.SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `SevenSegmentSingleDigit.ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `SevenSegmentSingleDigit.DigitValue`: Legt das anzuzeigende Zeichen fest oder gibt dieses zurück.
- `SevenSegmentSingleDigit.CustomBitPattern`: Legt ein benutzerdefiniertes Bitmuster für die sieben Segmente fest.
- `SevenSegmentSingleDigit.ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `SevenSegmentSingleDigit.DecimalPointActive`: Gibt an, ob die Dezimalpunkt-LED aktiv ist.
- `SevenSegmentSingleDigit.ShowColon`: Gibt an, ob die Doppelpunkt-LEDs angezeigt werden.
- `SevenSegmentSingleDigit.ColonActive`: Gibt an, ob die Doppelpunkt-LEDs aktiv sind.
- `SevenSegmentSingleDigit.BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `SevenSegmentSingleDigit.ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.
- `MultiDigitSevenSegment.InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `MultiDigitSevenSegment.SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `MultiDigitSevenSegment.ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `MultiDigitSevenSegment.ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `MultiDigitSevenSegment.DigitCount`: Anzahl der Digits in diesem Control.
- `MultiDigitSevenSegment.DigitPadding`: Auffüllung, die für jedes Digit im Control gilt.
- `MultiDigitSevenSegment.Value`: Der auf dem Control anzuzeigende Wert.
- `MultiDigitSevenSegment.BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `MultiDigitSevenSegment.ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## 5. Hilfstypen/Strukturen

- `SevenSegmentSingleDigit`: Einzelnes 7-Segment-Control.
- `MultiDigitSevenSegment`: Mehrstelliges 7-Segment-Control.

## 6. Hinweise

- Die Datei dokumentiert zwei öffentliche Controls in einer gemeinsamen Übersicht.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Schnelles, präzises Segment-Rendering für Single- und MultiDigit-Varianten.

### 7.2.  Geometrie-Caching einführen

**Umsetzung:**

- Segmentpolygone pro Größe/ItalicFactor/SegmentWidth cachen.

- Neu berechnen nur bei relevanten Property-Änderungen.

**Nutzen:** Deutlich weniger Rechenaufwand beim Repaint.

### 7.3. Repaint-Granularität verbessern

**Umsetzung:**

- Bei MultiDigit nur betroffene Ziffern invalidieren.

- Bei gleichem `Value` keine Neuzeichnung.

**Nutzen:** Bessere Performance bei häufigen Updates.

### 7.4. Zeichnungsqualität steuerbar machen

**Umsetzung:**

- Smoothing/PixelOffset als QualityMode anbieten.

- Default auf Performance, optional HighQuality.

**Nutzen:** Besserer Kompromiss zwischen Schärfe und Geschwindigkeit.

### 7.5. Eingabevalidierung konsolidieren

**Umsetzung:**

- `DigitValue`/`CustomBitPattern` zentral validieren.

- Fehlerfälle klar dokumentieren (Fallback-Zeichen).

**Nutzen:** Vorhersagbares API-Verhalten.

### 7.6. Farb- und Kontrastdefaults optimieren

**Umsetzung:**

- Kontraststarke Standardfarbkombinationen definieren.

- Automatische Ableitung inaktiver Farbe aus `ForeColor` ermöglichen.

**Nutzen:** Lesbarkeit out-of-the-box verbessern.

### 7.7. Empfohlene Reihenfolge

1. Geometrie-Caching
2. Repaint-Optimierung
3. Validierung
4. QualityMode
5. Farbdefaults

## 8. Erweiterungsvorschläge

### 8.1. Alphanumerische Musterbibliothek

**Vorschlag:** Vordefinierte Muster für A-F, Sonderzeichen, Minus.
**Umsetzung:** Erweiterte Mapping-Tabelle mit Fallback.

### 8.2. Blinking- und Alarmmodus

**Vorschlag:** Blinken einzelner Digits oder ganzer Anzeige.
**Umsetzung:** Timer-gesteuerter Sichtbarkeits- oder Aktivierungswechsel.

### 8.3. Übergangsanimationen

**Vorschlag:** Soft-Transition beim Wertwechsel.
**Umsetzung:** Interpolierte Segmenthelligkeit über mehrere Frames.

### 8.4. Formatierungsfunktionen

**Vorschlag:** Padding, Ausrichtung, Leading-Zeros, Dezimaltrennzeichen.
**Umsetzung:** `DisplayFormat`-Property mit Parser.

### 8.5. Datenquellenbindung

**Vorschlag:** Bindings für numerische Werte mit Autoformat.
**Umsetzung:** `Value`-Converter für Zahlen/Strings.
