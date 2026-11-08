# Shape Control

Steuerelement zum Darstellen einer Linie, eines Rechtecks oder einer Ellipse.

`ShapeControl` ist ein leichtgewichtiges WinForms-Control zur Darstellung einfacher Vektorformen ohne Abhängigkeit zu GDI+-High-Level-Wrappern oder externen Bibliotheken. Es eignet sich für UI-Trennlinien, einfache Markierungen, Status-Indikatoren oder visuelle Gruppierungen.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `Shape` und richtet Standardwerte sowie Zeichenstile ein.

## 2. Ereignisse

- Keine steuerelement-spezifischen Ereignisse dokumentiert.

## 3. Eigenschaften

- `ShapeModus`: Legt die anzuzeigende Form fest oder gibt diese zurück.
- `LineWidth`: Legt die Breite der Linie oder Rahmenlinie fest oder gibt diese zurück.
- `LineColor`: Legt die Farbe der Linie oder Rahmenlinie fest oder gibt diese zurück.
- `FillColor`: Legt die Füllfarbe für die Form fest oder gibt diese zurück.
- `DiagonalLineModus`: Legt fest, in welcher Richtung eine diagonale Linie gezeichnet wird.
- `BackColor`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `BackgroundImage`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `BackgroundImageLayout`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `Font`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `ForeColor`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `RightToLeft`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.
- `Text`: Im Designer ausgeblendet, da für dieses Steuerelement nicht relevant.

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Das Steuerelement zeichnet ausschließlich einfache Formen (Linie, Rechteck, Ellipse) und ist für leichte UI-Dekorationen ausgelegt.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Leichtgewichtiges Vektor-Control mit sauberer Zeichenqualität.

### 7.2. Zeichenpfade cachen

**Umsetzung:**

- Geometrie für Rechteck/Ellipse/Diagonale bei Größenänderung vorberechnen.

- Nur bei Property-Änderung aktualisieren.

**Nutzen:** Weniger CPU-Last bei häufigen Repaints.

### 7.3. GDI-Objekte wiederverwenden

**Umsetzung:**

- Pens/Brushes nur bei Farb- oder Breitenänderung neu erstellen.

- Korrektes Dispose sicherstellen.

**Nutzen:** Bessere Stabilität über lange Laufzeit.

### 7.4. Pixelgenauigkeit verbessern

**Umsetzung:**

- Linienstärken auf Device-Pixel ausrichten.

- Antialiasing abhängig von Formtyp steuern.

**Nutzen:** Schärfere Linien ohne „verwaschene“ Kanten.

### 7.5. Property-Setter optimieren

**Umsetzung:**

- Nur bei echter Wertänderung `Invalidate` auslösen.

- Für mehrere Änderungen `BeginUpdate/EndUpdate` anbieten.

**Nutzen:** Reduziert redundante Repaints.

### 7.6. Designer-Verhalten härten

**Umsetzung:**

- Standardwerte mit `DefaultValue`-Attributen pflegen.

- `ShouldSerialize`-Methoden ergänzen.

**Nutzen:** Sauberere Designer-Serialisierung.

### 7.7. Empfohlene Reihenfolge

1. Setter-Optimierung
2. Geometrie-Caching
3. GDI-Management
4. Pixelgenauigkeit
5. Designer-Metadaten

## 8. Erweiterungsvorschläge

### 8.1. Zusätzliche Formen

**Vorschlag:** Polygon, abgerundetes Rechteck, Pfeil, Stern.
**Umsetzung:** `ShapeMode` erweitern und Geometrie-Builder ergänzen.

### 8.2. Linienarten

**Vorschlag:** Gestrichelt, punktiert, benutzerdefinierte Dash-Patterns.
**Umsetzung:** Neue Properties für `DashStyle`/`DashPattern`.

### 8.3. Gradientenfüllung

**Vorschlag:** Linear/Radial statt nur Vollton.
**Umsetzung:** Fill-Strategie mit Brush-Factory.

### 8.4. Interaktionsereignisse

**Vorschlag:** HitTest-basiertes `ShapeClicked`.
**Umsetzung:** Punkt-in-Form-Prüfung und Eventauslösung.

### 8.5. Transformationsoptionen

**Vorschlag:** Rotation und Skalierung.
**Umsetzung:** Matrix-Transformation vor dem Zeichnen.
