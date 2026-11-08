# AniGif Control

Ein Control zum Anzeigen von animierten GIF-Grafiken.

Grundlage und Anregung für dieses Steuerelement stammen aus dem Buch **"Visual Basic 2015 - Grundlagen und Profiwissen"** von Walter Dobrenz und Thomas Gewinnus.

Der ursprüngliche Quelltext wurde verändert und um weitere Funktionen erweitert.

> &#x26A0; **Hinweis**
>
> Dieser Code sollte für mich als Übung dienen und ich denke das er auch für andere Anfänger interessant sein dürfte.

Weitere Infos unter:

[HANSER Fachbuch](https://www.hanser-fachbuch.de/fachbuch/artikel/9783446446052)

[Buchleser freigegeben auf onedrive](https://onedrive.live.com/?id=root&cid=D73E81A6F971DBA7&qt=people&personId=de18bb46da92110)

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz des Controls und setzt die Standarddarstellung.

## 2. Ereignisse

- `NoAnimation`: Wird ausgelöst, wenn die Grafik nicht animiert werden kann.
- `AutoPlayChanged`: Wird ausgelöst, wenn sich die Eigenschaft `AutoPlay` geändert hat.
- `AnimationStarted`: Wird ausgelöst, wenn die Animation gestartet wurde.
- `AnimationStopped`: Wird ausgelöst, wenn die Animation gestoppt wurde.

## 3. Eigenschaften

- `AutoPlay`: Steuert, ob die GIF-Animation automatisch gestartet wird, sobald ein Bild vorhanden ist.
- `Gif`: Gibt die animierte GIF-Grafik zurück oder legt diese fest.
- `GifSizeMode`: Gibt den Anzeigemodus (Skalierung/Ausrichtung) der GIF-Grafik zurück oder legt ihn fest.
- `ScalingQualityPreset`: Legt die Interpolation für `ImageSizeMode.Zoom` und `ImageSizeMode.Fill` fest. Standard ist `Photo`; ein Wechsel fordert eine Neuzeichnung an. Andere Werte als `Photo` und `PixelArt` lösen `System.ComponentModel.InvalidEnumArgumentException` aus.
- `CustomDisplaySpeed`: Legt fest, ob die benutzerdefinierte Anzeigegeschwindigkeit (Timer/FPS) oder normalisierte GIF-Metadaten-Delays verwendet werden.
- `FramesPerSecond`: Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern pro Sekunde (FPS) fest.
- `MinimumFrameIntervalMs`: Legt die untere Grenze für normalisierte Frame-Delays in Millisekunden fest.
- `MaximumFrameIntervalMs`: Legt die obere Grenze für normalisierte Frame-Delays in Millisekunden fest.
- `AdaptiveTiming`: Aktiviert optional eine adaptive Drift-Korrektur bei UI-Thread-Last.
- `ZoomFactor`: Legt den Zoomfaktor in Prozent fest, mit dem das GIF skaliert wird.
- `MaximumSize`: Ausgeblendet, da für dieses Control nicht relevant.
- `MinimumSize`: Ausgeblendet, da für dieses Control nicht relevant.
- `Padding`: Ausgeblendet, da für dieses Control nicht relevant.
- `RightToLeft`: Ausgeblendet, da für dieses Control nicht relevant.
- `Text`: Ausgeblendet, da für dieses Control nicht relevant.
- `AllowDrop`: Ausgeblendet, da für dieses Control nicht relevant.
- `AutoScrollOffset`: Ausgeblendet, da für dieses Control nicht relevant.
- `AutoSize`: Ausgeblendet, da für dieses Control nicht relevant.
- `BackgroundImage`: Ausgeblendet, da für dieses Control nicht relevant.
- `BackgroundImageLayout`: Ausgeblendet, da für dieses Control nicht relevant.
- `ContextMenuStrip`: Ausgeblendet, da für dieses Control nicht relevant.
- `Dock`: Ausgeblendet, da für dieses Control nicht relevant.
- `Font`: Ausgeblendet, da für dieses Control nicht relevant.
- `ForeColor`: Ausgeblendet, da für dieses Control nicht relevant.

## 4. Funktionen

- `StartAnimation()`: Startet die Animation (falls noch nicht aktiv); nach `Dispose()` ohne Wirkung.
- `StopAnimation()`: Stoppt die Animation und beendet Timer sowie `ImageAnimator`; nach `Dispose()` ohne Wirkung.

## 5. Hilfstypen/Strukturen

- `ImageSizeMode`: Definiert die verfügbaren Modi zur Größenanpassung und Ausrichtung der GIF-Grafik.
- `GifScalingQualityPreset`: `Photo` verwendet `HighQualityBicubic` für geglättete Bilder; `PixelArt` verwendet `NearestNeighbor` und `PixelOffsetMode.Half` für harte Pixelkanten.

## 6. Hinweise

- `ScalingQualityPreset` wirkt nur bei `Zoom` und `Fill`; `Normal` und `CenterImage` bleiben unverändert. `Photo` entspricht der bisherigen Standarddarstellung.
- Repaints werden bei laufender Animation nur noch angefordert, wenn das Control sichtbar ist.
- Frame-Delays werden aus den GIF-Metadaten (`0x5100`) gelesen, mit Fallbacks versehen und auf `MinimumFrameIntervalMs`/`MaximumFrameIntervalMs` begrenzt.
- Im Timer-Modus wird nur bei einem tatsächlichen Framewechsel neu gezeichnet.
- Mit `AdaptiveTiming = True` wird das nächste Intervall über eine Drift-Korrektur an die tatsächliche UI-Thread-Laufzeit angepasst.
- Beim Austausch von `Gif` werden laufender Timer und `ImageAnimator` vor dem Wechsel gestoppt; intern erzeugte Bilder werden freigegeben.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Stabilere GIF-Wiedergabe mit geringerer CPU-Last und sauberer Ressourcenfreigabe.

### 7.2. Rendering-Pipeline entkoppeln (umgesetzt)

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

### 7.3. Ressourcenlebenszyklus härten (umgesetzt)

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

### 7.4. Frame-Timing präzisieren (umgesetzt)

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

### 7.5. Qualitätsoptionen beim Skalieren (umgesetzt)

**Umsetzung:**

- Für die `GifSizeMode`-Einstellungen `Zoom` und `Fill` den Interpolationsmodus über `ScalingQualityPreset` festlegen.

- Pixel-Art und Foto-Inhalt über die Presets `PixelArt` (`NearestNeighbor`) und `Photo` (`HighQualityBicubic`) steuern.

**Nutzen:** Bessere Bildqualität bei Zoom/Skalierung.

### 7.6. Diagnostik für Betrieb hinzufügen

**Umsetzung:**

- Interne Metriken (FPS-ist, Dropped Frames, Renderzeit) optional erfassen.

- Über Debug-Eigenschaft oder Event ausgebbar machen.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

### 7.8. Empfohlene Reihenfolge

1. ~~Ressourcenlebenszyklus~~
2. ~~Rendering-Pipeline~~
3. ~~Frame-Timing~~
4. ~~Qualitätsoptionen~~
5. Diagnostik

## 8. Erweiterungsvorschläge

## 8.1 Playback-API erweitern

**Vorschlag:** `PauseAnimation()`, `ResumeAnimation()`, `SeekFrame(index)` ergänzen.
**Umsetzung:** Internen Zustand (`Stopped/Playing/Paused`) einführen und Timer-/Animator-Verhalten daran koppeln.

## 8.2 Loop- und Segmentsteuerung

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.
**Umsetzung:** Framezähler mit optionalem Abbruch bzw. Segmentwiedergabe.

## 8.3 Eventmodell ausbauen

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.
**Umsetzung:** Ereignisse nach erfolgreichem Framewechsel bzw. Fehlerpfad auslösen.

## 8.4 Quellenvielfalt erhöhen

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.
**Umsetzung:** Überladene `LoadGif(...)`-Methoden mit einheitlicher Validierung.

## 8.5 Design-Time-Unterstützung

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.
**Umsetzung:** DesignerActionList ergänzen.
