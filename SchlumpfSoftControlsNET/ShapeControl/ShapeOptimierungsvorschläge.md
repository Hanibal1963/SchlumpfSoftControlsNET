# Optimierungsvorschläge für ShapeControl

## 1. Zielbild

Leichtgewichtiges Vektor-Control mit sauberer Zeichenqualität.

## 2. Zeichenpfade cachen

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Geometrie für Rechteck/Ellipse/Diagonale bei Größenänderung vorberechnen.

- Nur bei Property-Änderung aktualisieren.

**Nutzen:** Weniger CPU-Last bei häufigen Repaints.

## 3. GDI-Objekte wiederverwenden

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Pens/Brushes nur bei Farb- oder Breitenänderung neu erstellen.

- Korrektes Dispose sicherstellen.

**Nutzen:** Bessere Stabilität über lange Laufzeit.

## 4. Pixelgenauigkeit verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Linienstärken auf Device-Pixel ausrichten.

- Antialiasing abhängig von Formtyp steuern.

**Nutzen:** Schärfere Linien ohne „verwaschene“ Kanten.

## 5. Property-Setter optimieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Nur bei echter Wertänderung `Invalidate` auslösen.

- Für mehrere Änderungen `BeginUpdate/EndUpdate` anbieten.

**Nutzen:** Reduziert redundante Repaints.

## 6. Designer-Verhalten härten

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Standardwerte mit `DefaultValue`-Attributen pflegen.

- `ShouldSerialize`-Methoden ergänzen.

**Nutzen:** Sauberere Designer-Serialisierung.

## 7. Empfohlene Reihenfolge

1. Setter-Optimierung
2. Geometrie-Caching
3. GDI-Management
4. Pixelgenauigkeit

## 5. Designer-Metadaten