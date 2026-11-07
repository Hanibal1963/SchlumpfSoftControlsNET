# Optimierungsvorschläge – AniGifControl

## Zielbild

Stabilere GIF-Wiedergabe mit geringerer CPU-Last und sauberer Ressourcenfreigabe.

## 1) Rendering-Pipeline entkoppeln (umgesetzt)

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

## 2) Ressourcenlebenszyklus härten (umgesetzt)

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

## 3) Frame-Timing präzisieren (umgesetzt)

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

## 4) Qualitätsoptionen beim Skalieren

**Umsetzung:**

- Für `GifSizeMode` Interpolationsmodus konfigurierbar machen.

- Pixel-art vs. Foto-Inhalt über Presets steuern (`NearestNeighbor`, `HighQualityBicubic`).

**Nutzen:** Bessere Bildqualität bei Zoom/Skalierung.

## 5) Diagnostik für Betrieb hinzufügen

**Umsetzung:**

- Interne Metriken (FPS-ist, Dropped Frames, Renderzeit) optional erfassen.

- Über Debug-Eigenschaft oder Event ausgebbar machen.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

## Empfohlene Reihenfolge

~~1. Ressourcenlebenszyklus~~

~~2. Rendering-Pipeline~~

~~3. Frame-Timing~~

4. Qualitätsoptionen

5. Diagnostik
