# ShapeControl

Steuerelement zum Darstellen einer Linie, eines Rechtecks oder einer Ellipse.

`ShapeControl` ist ein leichtgewichtiges WinForms-Control zur Darstellung einfacher Vektorformen mit Zeichenoperationen aus `System.Drawing`. Es eignet sich für UI-Trennlinien, einfache Markierungen, Status-Indikatoren oder visuelle Gruppierungen.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `Shape` und richtet Standardwerte sowie Zeichenstile ein.

---

## 2. Ereignisse

- Keine steuerelement-spezifischen Ereignisse dokumentiert.

---

## 3. Eigenschaften

- `ShapeModus`: Legt die anzuzeigende Form fest oder gibt diese zurück.
- `LineWidth`: Legt die Breite der Linie oder Rahmenlinie fest oder gibt diese zurück. Werte kleiner als 1 werden auf 1 gesetzt; der Standardwert ist 2.
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

---

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

---

## 5. Hilfstypen/Strukturen

- `ShapeModes`: Horizontale, vertikale und diagonale Linie sowie Rechteck und Ellipse, letztere auch gefüllt.
- `DiagonalLineModes`: Diagonale von links oben nach rechts unten oder von links unten nach rechts oben.

---

## 6. Hinweise

- Das Steuerelement zeichnet ausschließlich einfache Formen (Linie, Rechteck, Ellipse) und ist für leichte UI-Dekorationen ausgelegt.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Leichtgewichtiges Vektor-Control mit sauberer Zeichenqualität.

---

### 7.2. Zeichenpfade cachen

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Geometrie für Rechteck/Ellipse/Diagonale bei Größenänderung vorberechnen.

- Nur bei Property-Änderung aktualisieren.

**Nutzen:** Weniger CPU-Last bei häufigen Repaints.

---

### 7.3. GDI-Objekte wiederverwenden

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Pens/Brushes nur bei Farb- oder Breitenänderung neu erstellen.

- Korrektes Dispose sicherstellen.

**Nutzen:** Bessere Stabilität über lange Laufzeit.

---

### 7.4. Pixelgenauigkeit verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Linienstärken auf Device-Pixel ausrichten.

- Antialiasing abhängig von Formtyp steuern.

**Nutzen:** Schärfere Linien ohne „verwaschene“ Kanten.

---

### 7.5. Property-Setter optimieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Nur bei echter Wertänderung `Invalidate` auslösen.

- Für mehrere Änderungen `BeginUpdate/EndUpdate` anbieten.

**Nutzen:** Reduziert redundante Repaints.

---

### 7.6. Designer-Verhalten härten

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Standardwerte mit `DefaultValue`-Attributen pflegen.

- `ShouldSerialize`-Methoden ergänzen.

**Nutzen:** Sauberere Designer-Serialisierung.

---

### 7.7. Empfohlene Reihenfolge

1. Setter-Optimierung
2. Geometrie-Caching
3. GDI-Management
4. Pixelgenauigkeit

Der separate Vorschlag enthielt außerdem die Überschrift „Designer-Metadaten“ ohne weiteren Inhalt; das Thema wird oben unter 7.6 behandelt.

---

## 8. geplante Erweiterungen

### 8.1. Zusätzliche Formen

**Vorschlag:** Polygon, abgerundetes Rechteck, Pfeil, Stern.
**Umsetzung:** `ShapeModes` erweitern und Geometrie-Builder ergänzen.

---

### 8.2. Linienarten

**Vorschlag:** Gestrichelt, punktiert, benutzerdefinierte Dash-Patterns.
**Umsetzung:** Neue Properties für `DashStyle`/`DashPattern`.

---

### 8.3. Gradientenfüllung

**Vorschlag:** Linear/Radial statt nur Vollton.
**Umsetzung:** Fill-Strategie mit Brush-Factory.

---

### 8.4. Interaktionsereignisse

**Vorschlag:** HitTest-basiertes `ShapeClicked`.
**Umsetzung:** Punkt-in-Form-Prüfung und Eventauslösung.

---

### 8.5. Transformationsoptionen

**Vorschlag:** Rotation und Skalierung.
**Umsetzung:** Matrix-Transformation vor dem Zeichnen.

---

### 8.6. Reihenfolge zur Durchführung

1. Zusätzliche Formen
2. Linienarten
3. Gradientenfüllung
4. Interaktionsereignisse
5. Transformationsoptionen
