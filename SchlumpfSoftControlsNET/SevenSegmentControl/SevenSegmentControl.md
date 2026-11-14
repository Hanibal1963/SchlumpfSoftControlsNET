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

## [7. Optimierungsvorschläge](SevenSegmentOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](SevenSegmentErweiterungsvorschläge.md)