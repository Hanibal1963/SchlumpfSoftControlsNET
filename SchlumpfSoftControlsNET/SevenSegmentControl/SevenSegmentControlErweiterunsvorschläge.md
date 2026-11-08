# Erweiterungsvorschläge – SevenSegmentControl

## 1) Alphanumerische Musterbibliothek

**Vorschlag:** Vordefinierte Muster für A-F, Sonderzeichen, Minus.
**Umsetzung:** Erweiterte Mapping-Tabelle mit Fallback.

## 2) Blinking- und Alarmmodus

**Vorschlag:** Blinken einzelner Digits oder ganzer Anzeige.
**Umsetzung:** Timer-gesteuerter Sichtbarkeits- oder Aktivierungswechsel.

## 3) Übergangsanimationen

**Vorschlag:** Soft-Transition beim Wertwechsel.
**Umsetzung:** Interpolierte Segmenthelligkeit über mehrere Frames.

## 4) Formatierungsfunktionen

**Vorschlag:** Padding, Ausrichtung, Leading-Zeros, Dezimaltrennzeichen.
**Umsetzung:** `DisplayFormat`-Property mit Parser.

## 5) Datenquellenbindung

**Vorschlag:** Bindings für numerische Werte mit Autoformat.
**Umsetzung:** `Value`-Converter für Zahlen/Strings.
