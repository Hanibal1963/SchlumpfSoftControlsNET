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

### 7.2. Rendering-Pipeline entkoppeln

**Status:** Umgesetzt.

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

### 7.3. Ressourcenlebenszyklus härten

**Status:** Umgesetzt.

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

### 7.4. Frame-Timing präzisieren

**Status:** Umgesetzt.

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

### 7.5. Qualitätsoptionen beim Skalieren

**Status:** Umgesetzt.

**Umsetzung:**

- Für die `GifSizeMode`-Einstellungen `Zoom` und `Fill` den Interpolationsmodus über `ScalingQualityPreset` festlegen.

- Pixel-Art und Foto-Inhalt über die Presets `PixelArt` (`NearestNeighbor`) und `Photo` (`HighQualityBicubic`) steuern.

**Nutzen:** Bessere Bildqualität bei Zoom/Skalierung.

### 7.6. Diagnostik für Betrieb hinzufügen

**Status:** Offen.

**Umsetzung:**

- Interne Metriken (FPS-ist, Dropped Frames, Renderzeit) optional erfassen.

- Über Debug-Eigenschaft oder Event ausgebbar machen.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

### 7.7. GIF-Zuweisungen nach der Ressourcenfreigabe absichern

**Status:** Offen.

**Ausgangslage:** `SetGifImage()` erstellt und speichert auch nach `Dispose()` eine Bitmap-Kopie. `DisposeAnimationResources()` gibt wegen des gesetzten Dispose-Zustands anschließend keine weitere Bitmap frei.

**Umsetzung:**

- Den Dispose-Zustand vor dem Klonen prüfen und für `Gif = Nothing` sowie für Bildzuweisungen ein einheitliches Verhalten festlegen, beispielsweise einen wirkungslosen Aufruf oder eine `ObjectDisposedException`.

- Das gewählte Verhalten in den XML-Kommentaren und der API-Dokumentation beschreiben und mit Tests absichern. Das übergebene Bild darf dabei nicht freigegeben werden.

**Nutzen:** Verhindert, dass ein bereits freigegebenes Control erneut eine eigene Bitmap ohne explizite spätere Freigabe hält.

### 7.8. Skalierungsgeometrie an die Clientfläche anpassen

**Status:** Offen.

**Ausgangslage:** `GetRectStartSize()` in [FunctionDefinitions.vb](FunctionDefinitions.vb) berücksichtigt für `Zoom` und `Fill` nur die Orientierung des Bildes, nicht das Seitenverhältnis der Clientfläche. Beispielsweise wird ein quadratisches Bild bei einem Control von 400 × 100 und `ZoomFactor = 100` auf 400 × 400 skaliert und dadurch abgeschnitten. Bei 100 × 400 und `Fill` bleibt es dagegen 100 × 100 groß und füllt die Fläche nicht aus.

**Umsetzung:**

- Die Skalierungsfaktoren aus Clientbreite geteilt durch Bildbreite und Clienthöhe geteilt durch Bildhöhe berechnen. Für `Zoom` den kleineren Faktor mit `ZoomFactor / 100` multiplizieren; für `Fill` den größeren Faktor verwenden.

- Leere Zeichenflächen und Rundungen berücksichtigen und die Position anhand der Clientfläche bestimmen. Tests für Hoch-, Quer- und quadratische Formate, unterschiedliche Zoomfaktoren und beide Qualitätsoptionen ergänzen.

- Die Änderung der bisherigen Darstellung und die zugehörigen XML-Kommentare dokumentieren.

**Nutzen:** `Zoom` passt das Bild zuverlässig in die verfügbare Fläche ein; `Fill` deckt sie vollständig ab.

### 7.9. Unnötige Timer-Neustarts vermeiden

**Status:** Offen.

**Ausgangslage:** `UpdateTimerState()` stoppt die Wiedergabe zunächst immer, auch wenn sie nach einer Einstellungsänderung weiterlaufen soll. Zudem löst `FramesPerSecond` selbst bei unverändertem Wert die Aktualisierung aus. Bei laufender Animation entstehen dadurch zusätzliche Stopp- und Startereignisse.

**Umsetzung:**

- Den begrenzten FPS-Wert mit dem bisherigen Wert vergleichen und gleichwertige Zuweisungen vor dem Auslösen von `FramesPerSecondChanged` beenden.

- In `UpdateTimerState()` zwischen tatsächlichen Zustandswechseln und reinen Konfigurationsänderungen unterscheiden. Geänderte Intervalle gezielt übernehmen, ohne die Wiedergabe unnötig zu stoppen oder Start-/Stoppereignisse auszulösen.

- Sichtbarkeit, Pause, `NoAnimation` und wiedereintretende Ereignishandler mit Tests absichern. Das veränderte Ereignisverhalten bei Einstellungsänderungen dokumentieren.

**Nutzen:** Weniger Timer-Neustarts, Ereignisse und Unterbrechungen bei unverändertem Wiedergabezustand.

### 7.10. Frame-Delay-Metadaten wiederverwenden

**Status:** Offen.

**Ausgangslage:** `RefreshFrameDelayCache()` liest bei animierten Bildern die GIF-Metadaten erneut und erstellt ein neues Array, wenn `FramesPerSecond`, `MinimumFrameIntervalMs` oder `MaximumFrameIntervalMs` geändert wird.

**Umsetzung:**

- Die unveränderten Delay-Rohwerte beim Bildwechsel zwischenspeichern und bei Änderungen von FPS oder Intervallgrenzen daraus nur die erforderlichen normalisierten Werte und Fallbacks neu berechnen.

- Fehlende, unvollständige und ungültige Metadaten, Bildwechsel und den Wechsel zwischen beiden Geschwindigkeitsarten testen. Den zusätzlichen Speicherbedarf berücksichtigen.

- Da `Gif` die interne Bitmap zurückgibt, eine Cache-Invalidierung für mögliche externe Metadatenänderungen festlegen und das bestehende Verhalten dabei berücksichtigen.

**Nutzen:** Kann wiederholte Metadatenzugriffe und Array-Neuanlagen bei Einstellungsänderungen reduzieren.

### 7.11. Wiederholte Berechnungen pro Timer-Tick reduzieren

**Status:** Offen.

**Ausgangslage:** `Timer_Tick()` liest und berechnet die verstrichene Zeit auch bei deaktiviertem `AdaptiveTiming`. Im benutzerdefinierten Geschwindigkeitsmodus wird das FPS-Intervall bei jedem Framewechsel erneut berechnet.

**Umsetzung:**

- Zeitstempel und Zeitdifferenz nur für das adaptive Timing erfassen. Beim Aktivieren, Fortsetzen und bei einer Frameauswahl den Zeitstempel weiterhin korrekt initialisieren.

- Das begrenzte FPS-Intervall bei Änderungen von FPS oder Intervallgrenzen vorberechnen und während der Wiedergabe wiederverwenden. Rundung, Fallbacks und Grenzen unverändert beibehalten.

- Beide Geschwindigkeitsarten sowie das Ein- und Ausschalten von `AdaptiveTiming` mit Tests überprüfen.

**Nutzen:** Weniger wiederholte Berechnungen im häufig aufgerufenen Timerpfad; der tatsächliche Effekt sollte gemessen werden.

### 7.12. Sichtbarkeitsprüfung vereinfachen

**Status:** Offen.

**Ausgangslage:** `IsControlTreeVisible()` prüft nach `Visible` nochmals alle übergeordneten Controls. Im Timerpfad wird die Sichtbarkeit zusätzlich sowohl vor dem Framewechsel als auch in `InvalidateIfVisible()` geprüft.

**Umsetzung:**

- Die zusätzliche Elternschleife prüfen: Der Getter von [Control.Visible](https://learn.microsoft.com/dotnet/api/system.windows.forms.control.visible) berücksichtigt bereits die Sichtbarkeit des übergeordneten Controls.

- Eine wiederholte Prüfung innerhalb desselben Ticks vermeiden, soweit dazwischen keine Zustandsänderung möglich ist. Die Schutzprüfungen für Dispose-Zustand, Handle und Clientgröße erhalten.

- Sichtbarkeitswechsel des Controls und mehrerer verschachtelter Eltern sowie das Fortsetzen der Wiedergabe testen.

**Nutzen:** Weniger redundante Abfragen pro Frame bei unverändertem Verhalten für ausgeblendete Controls.

### 7.13. Aktualisierungsbereich bei Framewechseln begrenzen

**Status:** Offen.

**Ausgangslage:** `InvalidateIfVisible()` fordert bei einem Framewechsel die Neuzeichnung der gesamten Clientfläche an, auch wenn das Bild nur einen Teil davon belegt.

**Umsetzung:**

- Bei unveränderter Bildgeometrie prüfen, ob nur der nicht leere Schnitt des Bildzielrechtecks mit der Clientfläche über `Invalidate(Rectangle)` aktualisiert werden kann.

- Bei Bild-, Größen- oder Layoutänderungen weiterhin ausreichend große Bereiche aktualisieren; alte und neue Bildpositionen, transparente Frames, Interpolationsränder und externe `Paint`-Handler berücksichtigen.

- Darstellung und Hintergrund für alle Größenmodi mit Tests vergleichen und den Nutzen insbesondere bei kleinen Bildern in großen Controls messen.

**Nutzen:** Kann die zu aktualisierende Fläche und damit den Zeichenaufwand verringern, ohne die bereits umgesetzte Bedingung für Framewechsel zu verändern.

### 7.14. Ungenutzte ImageAnimator-Pfade entfernen

**Status:** Offen.

**Ausgangslage:** Beide Geschwindigkeitsarten verwenden den Timer. Für den privaten `OnNextFrame`-Handler wird kein `ImageAnimator.Animate()` registriert; `StopAnimationCore()` ruft dennoch `ImageAnimator.StopAnimate()` auf.

**Umsetzung:**

- Den ungenutzten Handler, das zugehörige Delegate und den überflüssigen `StopAnimate()`-Aufruf entfernen. Die ebenfalls ungenutzte Konstante `DefaultFrameDelayMs` prüfen.

- Veraltete Kommentare und den XML-Kommentar von `StopAnimation()` an die Timer-basierte Wiedergabe anpassen. Start, Stopp, Pause, Sichtbarkeit und Ressourcenfreigabe in beiden Geschwindigkeitsarten testen.

**Nutzen:** Vereinfacht die Wartung und vermeidet eine unnötige Abmeldung eines nicht registrierten Wiedergabemechanismus.

### 7.15. Empfohlene Reihenfolge

1. GIF-Zuweisungen nach Ressourcenfreigabe absichern (7.7).
2. Skalierungsgeometrie korrigieren (7.8).
3. Ungenutzte ImageAnimator-Pfade entfernen (7.14).
4. Timer-Neustarts und Ereignisse gezielt steuern (7.9).
5. Diagnostik und Messungen für die folgenden Optimierungen vorsehen (7.6).
6. Frame-Delay-Metadaten wiederverwenden (7.10).
7. Timer-Ticks verschlanken (7.11).
8. Sichtbarkeitsprüfung vereinfachen (7.12).
9. Aktualisierungsbereich bei Framewechseln begrenzen (7.13).

## 8. Erweiterungsvorschläge

### 8.1. Playback-API erweitern

**Status:** Umgesetzt.

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
