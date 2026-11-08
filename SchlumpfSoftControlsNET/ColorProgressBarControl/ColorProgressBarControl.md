# ColorProgressBar Control

Ein benutzerdefiniertes Windows Forms-Steuerelement zur Anzeige eines farbigen Fortschrittsbalkens mit optionalem Rahmen und Glanzeffekt.

Die Idee hinter dem `ColorProgressBarControl` ist es, einen Fortschrittsbalken zu erstellen, der optisch anpassbar ist.

Der Standard-Fortschrittsbalken in Windows ist ein einfacher Balken, der den Fortschritt in Form einer Füllung anzeigt.

Der `ColorProgressBarControl` hingegen kann in verschiedenen Farben und Stilen angezeigt werden.

Als Anregung diente der Artikel [A Better ProgressBar - Using Panels!](https://www.codeproject.com/Articles/31903/A-Better-ProgressBar-Using-Panels) von Saul Johnson.

Da die Downloads auf der Seite nicht mehr zu funktionieren scheinen und die Beschreibung nur Ausschnitte aus dem Original-C#-Code enthält, wurde das Control in VB NET umgesetzt.

> &#x26A0; **Hinweis**
>
> Die Seite mit dem Artikel scheint mittlerweile nicht mehr zu existieren.
>
> Eine im Web-Archiv gespeicherte Version der Seite ist verfügbar.
>
> [Web Archiv Kopie](https://web.archive.org/web/20250319060356/<https://www.codeproject.com/Articles/31903/A-Better-ProgressBar-Using-Panels>)

## 1. Konstruktoren

- `New()`: Initialisiert das Steuerelement, aktiviert flimmerfreies Zeichnen und setzt die Standarddarstellung.

## 2. Ereignisse

- `Click`: Klicks auf die inneren Panels werden an das Steuerelement weitergeleitet, damit ein einheitliches Click-Ereignis entsteht.

## 3. Eigenschaften

- `Value`: Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest (Bereich: `0` bis `ProgressMaximumValue`).
- `ProgressMaximumValue`: Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest.
- `BarColor`: Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest.
- `EmptyColor`: Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest.
- `BorderColor`: Gibt die Farbe des Rahmens zurück oder legt diese fest.
- `ShowBorder`: Legt fest, ob ein Rahmen um die Fortschrittsanzeige angezeigt wird.
- `IsGlossy`: Legt fest, ob ein Glanzeffekt auf der Fortschrittsleiste angezeigt wird.
- `BackColor` (überschrieben, ausgeblendet): Hintergrundfarbe ist intern als Rahmenfarbe vorgesehen.
- `BackgroundImage` (überschrieben, ausgeblendet): Hintergrundgrafik wird vom Steuerelement nicht unterstützt.
- `BackgroundImageLayout` (überschrieben, ausgeblendet): Hintergrundgrafik-Layout wird vom Steuerelement nicht unterstützt.
- `BorderStyle` (ausgeblendet): Rahmen wird über `ShowBorder` und `BorderColor` gesteuert.
- `ForeColor` (überschrieben, ausgeblendet): Vordergrundfarbe wird vom Steuerelement nicht verwendet.
- `Padding` (überladen, ausgeblendet): Inneres Padding wird intern zur Rahmendarstellung verwaltet.

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Werte außerhalb des gültigen Bereichs von `0` bis `ProgressMaximumValue` werden über die Guard-Logik der `Value`-Eigenschaft abgefangen.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Flüssige Fortschrittsdarstellung ohne Flackern bei hoher Updatefrequenz.

### 7.2. Vollständig owner-drawn rendern

**Umsetzung:**

- Panel-basierte Darstellung auf ein einziges `OnPaint` konsolidieren.

- `DoubleBuffered` + `AllPaintingInWmPaint` + `ResizeRedraw` aktiv halten.

**Nutzen:** Weniger Control-Overhead, stabilere Darstellung.

### 7.3. Layout-Neuberechnung minimieren

**Umsetzung:**

- Wertänderungen clampen und nur bei echter Änderung rendern.

- Berechnete Rechtecke (Fill/Gloss/Border) cachen und nur bei Size/Style ändern.

**Nutzen:** Geringere CPU-Last bei häufigen Fortschrittsupdates.

### 7.4. Farbübergänge effizient gestalten

**Umsetzung:**

- Brushes/Pens nur bei Farbänderung neu erzeugen.

- Disposable-GDI-Objekte konsequent freigeben.

**Nutzen:** Weniger GDI-Druck und stabilere Langzeitnutzung.

### 7.5. API-Konsistenz verbessern

**Umsetzung:**

- `ProgressMaximumValue`/`Value` Validierung zentralisieren.

- Optional `Minimum` ergänzen, um Standard-Progress-Semantik abzubilden.

**Nutzen:** Vorhersagbares Verhalten für Integratoren.

### 7.6. Accessibility-Basis ergänzen

**Umsetzung:**

- `AccessibleName`, `AccessibleDescription`, `AccessibleRole=ProgressBar` sinnvoll setzen.

- Optionalen Text-Overlay (z. B. Prozent) als Screenreader-Text bereitstellen.

**Nutzen:** Bessere Nutzbarkeit und Compliance.

### 7.7. Empfohlene Reihenfolge

1. Owner-Draw-Rendering
2. Layout-Caching
3. GDI-Objektmanagement
4. API-Konsistenz
5. Accessibility

## 8. Erweiterungsvorschläge

### 8.1. Mehr Darstellungsmodi

**Vorschlag:** `ProgressStyle` mit `Solid`, `Gradient`, `Striped`, `Marquee`.
**Umsetzung:** Renderingpfad je Style verzweigen, gemeinsame Geometrie wiederverwenden.

### 8.2. Textanzeige integrieren

**Vorschlag:** Prozent-/Werttext im Balken (`ShowText`, `TextFormat`).
**Umsetzung:** Textmessung zentralisieren, Kontrastfarbe automatisch wählen.

### 8.3. Animierte Übergänge

**Vorschlag:** Weiches Interpolieren bei Sprüngen des `Value`.
**Umsetzung:** Zielwert und Istwert trennen, Timer-basiertes easing.

### 8.4. Segmentierter Fortschritt

**Vorschlag:** Optionale Segmentanzeige (z. B. 10 Blöcke).
**Umsetzung:** Segmentanzahl und Abstand als Properties.

### 8.5. Schwellenwertfarben

**Vorschlag:** Dynamischer Farbwechsel bei Schwellwerten.
**Umsetzung:** `Thresholds`-Collection mit Bereich/Farbe.
