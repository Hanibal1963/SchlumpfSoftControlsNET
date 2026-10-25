# SevenSegment Control

Diese Bibliothek enthält zwei Controls:

- **SevenSegmentSingleDigit:** Ein Control, das eine einzelne Ziffer von 0 bis 9 anzeigt.
- **MultiDigitSevenSegment:** Ein Control, das mehrere Ziffern anzeigt und damit ganze Zahlen darstellen kann.

Ich habe für ein anderes Projekt versucht eine 7-Segmentanzeige zu programmieren.

Nach einigen Fehlversuchen und einer intensive Internetrecherche, bin ich auf GitHub fündig geworden.
([SevenSegment von Dimitry Brant](https://github.com/dbrant/SevenSegment))

Ich habe mich entschlossen den Code in VisualBasic neu zu erstellen da ich mit C# keinerlei Erfahrung habe.

## Beschreibung der Controls

### Eigenschaften - SevenSegmentSingleDigit

- `InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `DigitValue`: Legt das anzuzeigende Zeichen fest oder gibt dieses zurück.
- `CustomBitPattern`: Legt ein benutzerdefiniertes Bitmuster für die sieben Segmente fest.
- `ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `DecimalPointActive`: Gibt an, ob die Dezimalpunkt-LED aktiv ist.
- `ShowColon`: Gibt an, ob die Doppelpunkt-LEDs angezeigt werden.
- `ColonActive`: Gibt an, ob die Doppelpunkt-LEDs aktiv sind.
- `BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.

### Methoden - SevenSegmentSingleDigit

- `New()`: Initialisiert eine neue Instanz der `SingleDigit`-Klasse.

### Ereignisse - SevenSegmentSingleDigit

- Keine eigenen öffentlichen Ereignisse.

### Eigenschaften - MultiDigitSevenSegment

- `InactiveColor`: Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
- `SegmentWidth`: Legt die Breite der LED-Segmente fest oder gibt diese zurück.
- `ItalicFactor`: Scherkoeffizient für die Kursivschrift der Anzeige.
- `ShowDecimalPoint`: Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
- `DigitCount`: Anzahl der Digits in diesem Control.
- `DigitPadding`: Auffüllung, die für jedes Digit im Control gilt.
- `Value`: Der auf dem Control anzuzeigende Wert.
- `BackColor`: Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
- `ForeColor`: Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.

### Methoden - MultiDigitSevenSegment

- `New()`: Initialisiert eine neue Instanz von `MultiDigit`.

### Ereignisse - MultiDigitSevenSegment

- Keine eigenen öffentlichen Ereignisse.

