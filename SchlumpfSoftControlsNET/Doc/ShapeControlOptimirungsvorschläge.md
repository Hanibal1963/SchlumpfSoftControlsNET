# Optimierungsvorschläge – ShapeControl

## Zielbild

Leichtgewichtiges Vektor-Control mit sauberer Zeichenqualität.

## 1) Zeichenpfade cachen

**Umsetzung:**

- Geometrie für Rechteck/Ellipse/Diagonale bei Größenänderung vorberechnen.

- Nur bei Property-Änderung aktualisieren.

**Nutzen:** Weniger CPU-Last bei häufigen Repaints.

## 2) GDI-Objekte wiederverwenden

**Umsetzung:**

- Pens/Brushes nur bei Farb- oder Breitenänderung neu erstellen.

- Korrektes Dispose sicherstellen.

**Nutzen:** Bessere Stabilität über lange Laufzeit.

## 3) Pixelgenauigkeit verbessern

**Umsetzung:**

- Linienstärken auf Device-Pixel ausrichten.

- Antialiasing abhängig von Formtyp steuern.

**Nutzen:** Schärfere Linien ohne „verwaschene“ Kanten.

## 4) Property-Setter optimieren

**Umsetzung:**

- Nur bei echter Wertänderung `Invalidate` auslösen.

- Für mehrere Änderungen `BeginUpdate/EndUpdate` anbieten.

**Nutzen:** Reduziert redundante Repaints.

## 5) Designer-Verhalten härten

**Umsetzung:**

- Standardwerte mit `DefaultValue`-Attributen pflegen.

- `ShouldSerialize`-Methoden ergänzen.

**Nutzen:** Sauberere Designer-Serialisierung.

## Empfohlene Reihenfolge

1. Setter-Optimierung

2. Geometrie-Caching

3. GDI-Management

4. Pixelgenauigkeit

5. Designer-Metadaten
