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
- `CustomDisplaySpeed`: Legt fest, ob die benutzerdefinierte Anzeigegeschwindigkeit (Timer/FPS) oder die im GIF hinterlegte Bildfolge (`ImageAnimator`) verwendet wird.
- `FramesPerSecond`: Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern pro Sekunde (FPS) fest.
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

- `StartAnimation()`: Startet die Animation (falls noch nicht aktiv).
- `StopAnimation()`: Stoppt die Animation und beendet Timer sowie `ImageAnimator`.

## Hilfstypen/Strukturen

- `ImageSizeMode`: Definiert die verfügbaren Modi zur Größenanpassung und Ausrichtung der GIF-Grafik.

## Hinweise

- Repaints werden bei laufender Animation nur noch angefordert, wenn das Control sichtbar ist.
- Im Timer-Modus wird nur bei einem tatsächlichen Framewechsel neu gezeichnet.

