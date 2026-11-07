# Erweiterungsvorschläge – ShapeControl

## 1) Zusätzliche Formen

**Vorschlag:** Polygon, abgerundetes Rechteck, Pfeil, Stern.
**Umsetzung:** `ShapeMode` erweitern und Geometrie-Builder ergänzen.

## 2) Linienarten

**Vorschlag:** Gestrichelt, punktiert, benutzerdefinierte Dash-Patterns.
**Umsetzung:** Neue Properties für `DashStyle`/`DashPattern`.

## 3) Gradientenfüllung

**Vorschlag:** Linear/Radial statt nur Vollton.
**Umsetzung:** Fill-Strategie mit Brush-Factory.

## 4) Interaktionsereignisse

**Vorschlag:** HitTest-basiertes `ShapeClicked`.
**Umsetzung:** Punkt-in-Form-Prüfung und Eventauslösung.

## 5) Transformationsoptionen

**Vorschlag:** Rotation und Skalierung.
**Umsetzung:** Matrix-Transformation vor dem Zeichnen.
