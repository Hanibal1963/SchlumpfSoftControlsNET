# AniGifControl

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
- `AnimationStarted`: Wird ausgelöst, wenn die Animation startet oder nach einer Unterbrechung fortgesetzt wird.
- `AnimationStopped`: Wird ausgelöst, wenn eine laufende Animation angehalten oder gestoppt wird, beispielsweise durch Pause oder Unsichtbarkeit.

## 3. Eigenschaften

- `AutoPlay`: Steuert, ob die GIF-Animation automatisch gestartet wird, sobald ein animierbares Bild vorhanden und das Control sichtbar ist. Eine explizite Pause verändert den Wert nicht.
- `Gif`: Gibt die aktuell verwendete Bitmap zurück oder legt eine GIF-Grafik fest. Bei der Zuweisung wird eine eigene Kopie erstellt; `Nothing` stellt eine Kopie des Standardbilds wieder her.
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

- `StartAnimation()`: Setzt `AutoPlay` auf `True` und startet die Animation beziehungsweise setzt eine Pause beim aktuellen Frame fort. Ein bereits laufender Start ist wirkungslos.
- `PauseAnimation()`: Hält eine laufende Wiedergabe beim aktuellen Frame an, ohne `AutoPlay` zu ändern. Bei bereits pausierter oder gestoppter Wiedergabe bleibt der Aufruf ohne Wirkung.
- `ResumeAnimation()`: Setzt eine pausierte Wiedergabe beim aktuellen Frame fort. Dessen vollständiges Intervall beginnt neu; bei unsichtbarem Control wird bis zur erneuten Sichtbarkeit gewartet. Ohne vorherige Pause bleibt der Aufruf ohne Wirkung.
- `SeekFrame(index As Int32)`: Wählt den nullbasierten Frame `index` aus, ohne den Wiedergabezustand zu ändern. Gültig sind Werte von `0` bis zum letzten Frame; bei einem statischen Bild ist nur `0` gültig. Ungültige Werte lösen `ArgumentOutOfRangeException` aus, ein fehlendes Bild löst `InvalidOperationException` aus. Bei laufender Wiedergabe beginnt das Intervall des gewählten Frames neu.
- `StopAnimation()`: Setzt `AutoPlay` auf `False` und stoppt die Wiedergabe. Der aktuelle Frame bleibt erhalten.
- `GetPlaybackState() As PlaybackState`: Gibt den gewünschten Wiedergabezustand zurück. Der Anfangszustand ist `Stopped`; bei der Ressourcenfreigabe wird der Zustand auf `Stopped` gesetzt. Die Methode benötigt keine Parameter und löst keine eigenen Ausnahmen aus.

Die Wiedergabesteuerungsmethoden bleiben nach `Dispose()` ohne Wirkung; `GetPlaybackState()` gibt weiterhin den gespeicherten Zustand zurück. Alle genannten Methoden sollen auf dem UI-Thread des Controls aufgerufen werden.

## 5. Hilfstypen/Strukturen

- `ImageSizeMode`: Definiert die verfügbaren Modi zur Größenanpassung und Ausrichtung der GIF-Grafik.
- `GifScalingQualityPreset`: `Photo` verwendet `HighQualityBicubic` für geglättete Bilder; `PixelArt` verwendet `NearestNeighbor` und `PixelOffsetMode.Half` für harte Pixelkanten.
- `PlaybackState`: Öffentliche Aufzählung mit `Stopped = 0` (gestoppt), `Playing = 1` (Wiedergabe), und `Paused = 2` (pausiert).

## 6. Hinweise

- `ScalingQualityPreset` wirkt nur bei `Zoom` und `Fill`; `Normal` und `CenterImage` bleiben unverändert. `Photo` entspricht der bisherigen Standarddarstellung.
- Repaints werden bei laufender Animation nur noch angefordert, wenn das Control sichtbar ist.
- Frame-Delays werden aus den GIF-Metadaten (`0x5100`) gelesen, mit Fallbacks versehen und auf `MinimumFrameIntervalMs`/`MaximumFrameIntervalMs` begrenzt.
- Beide Geschwindigkeitsarten verwenden den Timer; neu gezeichnet wird nur bei einem tatsächlichen Framewechsel.
- Mit `AdaptiveTiming = True` wird das nächste Intervall über eine Drift-Korrektur an die tatsächliche UI-Thread-Laufzeit angepasst.
- Pause und Fortsetzen verändern `AutoPlay` nicht. `AnimationStopped` beziehungsweise `AnimationStarted` werden nur beim tatsächlichen Anhalten beziehungsweise Wiederanlaufen ausgelöst; eine Frameauswahl löst diese Ereignisse nicht aus.
- Der mit `GetPlaybackState()` abgefragte Zustand ist unabhängig von der Sichtbarkeit. `Playing` bedeutet nicht zwangsläufig, dass die Animation tatsächlich läuft, beispielsweise bei einem unsichtbaren Control oder einer nicht animierbaren Grafik.
- Beim Austausch von `Gif` werden laufende Wiedergabemechanismen gestoppt, intern erzeugte Bilder freigegeben und der Frame auf `0` gesetzt. Eine explizite Pause bleibt erhalten.
- Jede Instanz besitzt eine eigene Bitmap-Kopie des Standardbilds, damit Frameänderungen andere Instanzen nicht beeinflussen.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Stabilere GIF-Wiedergabe mit geringerer CPU-Last und sauberer Ressourcenfreigabe.

### 7.2. ~~Rendering-Pipeline entkoppeln~~ (umgesetzt)

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

### 7.3. ~~Ressourcenlebenszyklus härten~~ (umgesetzt)

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

### 7.4. ~~Frame-Timing präzisieren~~ (umgesetzt)

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

### 7.5. ~~Qualitätsoptionen beim Skalieren~~ (umgesetzt)

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

1. ~~Ressourcenlebenszyklus~~ (umgesetzt)
2. ~~Rendering-Pipeline~~ (umgesetzt)
3. ~~Frame-Timing~~ (umgesetzt)
4. ~~Qualitätsoptionen~~ (umgesetzt)
5. Diagnostik

## 8. Erweiterungsvorschläge

### 8.1. ~~Playback-API erweitern~~ (umgesetzt)

**Umsetzung:** `PauseAnimation()`, `ResumeAnimation()` und `SeekFrame(index)` verwenden einen internen Wiedergabezustand und steuern den Timer entsprechend.

### 8.2 Loop- und Segmentsteuerung

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.
**Umsetzung:** Framezähler mit optionalem Abbruch bzw. Segmentwiedergabe.

### 8.3 Eventmodell ausbauen

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.
**Umsetzung:** Ereignisse nach erfolgreichem Framewechsel bzw. Fehlerpfad auslösen.

### 8.4 Quellenvielfalt erhöhen

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.
**Umsetzung:** Überladene `LoadGif(...)`-Methoden mit einheitlicher Validierung.

### 8.5 Design-Time-Unterstützung

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.
**Umsetzung:** DesignerActionList ergänzen.
