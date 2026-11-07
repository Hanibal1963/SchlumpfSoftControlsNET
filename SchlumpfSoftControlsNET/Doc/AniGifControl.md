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

## Konstruktoren

- `New()`: Initialisiert eine neue Instanz des Controls und setzt die Standarddarstellung.

## Ereignisse

- `NoAnimation`: Wird ausgelöst, wenn die Grafik nicht animiert werden kann.
- `AutoPlayChanged`: Wird ausgelöst, wenn sich die Eigenschaft `AutoPlay` geändert hat.
- `AnimationStarted`: Wird ausgelöst, wenn die Animation gestartet wurde.
- `AnimationStopped`: Wird ausgelöst, wenn die Animation gestoppt wurde.

## Eigenschaften

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

## Funktionen

- `StartAnimation()`: Startet die Animation (falls noch nicht aktiv); nach `Dispose()` ohne Wirkung.
- `StopAnimation()`: Stoppt die Animation und beendet Timer sowie `ImageAnimator`; nach `Dispose()` ohne Wirkung.

## Hilfstypen/Strukturen

- `ImageSizeMode`: Definiert die verfügbaren Modi zur Größenanpassung und Ausrichtung der GIF-Grafik.
- `GifScalingQualityPreset`: `Photo` verwendet `HighQualityBicubic` für geglättete Bilder; `PixelArt` verwendet `NearestNeighbor` und `PixelOffsetMode.Half` für harte Pixelkanten.

## Hinweise

- `ScalingQualityPreset` wirkt nur bei `Zoom` und `Fill`; `Normal` und `CenterImage` bleiben unverändert. `Photo` entspricht der bisherigen Standarddarstellung.
- Repaints werden bei laufender Animation nur noch angefordert, wenn das Control sichtbar ist.
- Frame-Delays werden aus den GIF-Metadaten (`0x5100`) gelesen, mit Fallbacks versehen und auf `MinimumFrameIntervalMs`/`MaximumFrameIntervalMs` begrenzt.
- Im Timer-Modus wird nur bei einem tatsächlichen Framewechsel neu gezeichnet.
- Mit `AdaptiveTiming = True` wird das nächste Intervall über eine Drift-Korrektur an die tatsächliche UI-Thread-Laufzeit angepasst.
- Beim Austausch von `Gif` werden laufender Timer und `ImageAnimator` vor dem Wechsel gestoppt; intern erzeugte Bilder werden freigegeben.

## Optimierungs und Verbesserungsmöglichkeiten

- [Optimierungsvorschläge](AniGifControlOptimirungsvorschläge.md) (teilweise umgesetzt)
- [Erweiterungsvorschläge](AniGifControlErweiterunsvorschläge.md)
