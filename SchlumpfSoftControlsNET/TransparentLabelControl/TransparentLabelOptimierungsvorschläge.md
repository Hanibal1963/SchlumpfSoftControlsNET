# Optimierungsvorschläge für TransparentLabelControl

## 1. Zielbild

Stabiles transparentes Label ohne Flackern und mit guter Lesbarkeit.

## 2. Repaint-Reihenfolge kontrollieren

**Umsetzung:**

- Parent-Invalidation gezielt nur bei Bedarf triggern.

- `OnPaintBackground` bewusst steuern, um Doppelzeichnen zu vermeiden.

**Nutzen:** Weniger Flackern bei bewegten Hintergründen.

## 3. Text-Rendering modernisieren

**Umsetzung:**

- Optional zwischen GDI (`TextRenderer`) und GDI+ (`Graphics.DrawString`) umschaltbar machen.

- DPI-abhängige Messung vereinheitlichen.

**Nutzen:** Bessere Schärfe und konsistente Umbrüche.

## 4. Layoutkosten reduzieren

**Umsetzung:**

- Größe nur bei Text-/Font-Änderung neu berechnen.

- Mehrfache Property-Updates in Batch zusammenfassen.

**Nutzen:** Geringere Layoutlast in komplexen Forms.

## 5. Kontrastunterstützung ergänzen

**Umsetzung:**

- Optionalen Schatten/Outline für Text anbieten.

- Automatische Kontrastfarbe gegen Hintergrundprobe.

**Nutzen:** Höhere Lesbarkeit über Bildern.

## 6. Parent-Wechsel robust machen

**Umsetzung:**

- Event-Abos auf Parent beim Reparenting sauber umhängen.

**Nutzen:** Stabilität bei dynamischer UI-Komposition.

## 7. Empfohlene Reihenfolge

1. Repaint-Pipeline
2. Parent-Reparenting
3. Textmessung/Rendering
4. Layout-Batching
5. Kontrastfeatures