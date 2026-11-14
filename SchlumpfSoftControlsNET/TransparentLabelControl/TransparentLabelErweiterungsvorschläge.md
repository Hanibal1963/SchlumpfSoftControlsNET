# Erweiterungsvorschläge für TransparentLabelControl

## 1. Rich-Text-Light

**Vorschlag:** Teilformatierung (fett, kursiv, farbig) im Label.
**Umsetzung:** Segmentmodell für Textläufe und eigener Renderer.

## 2. Textschatten/Outline

**Vorschlag:** Verbesserte Lesbarkeit über komplexem Hintergrund.
**Umsetzung:** Schattierungs- und Outline-Properties.

## 3. AutoFit-Modi

**Vorschlag:** `AutoSize`, `Wrap`, `Ellipsis`, `ScaleToBounds`.
**Umsetzung:** Einheitliche Messlogik mit Modusumschaltung.

## 4. Drehung/Transformation

**Vorschlag:** Textrotation (z. B. 90°/270°).
**Umsetzung:** Graphics-Transformation vor Textausgabe.

## 5. Hintergrundprobe für adaptive Farbe

**Vorschlag:** Automatische Schriftfarbe abhängig vom Hintergrund.
**Umsetzung:** Luminanzmessung aus Parent-Renderbereich.