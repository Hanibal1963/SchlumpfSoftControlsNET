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

- `NoAnimation`: Wird ausgelöst, wenn bei gewünschter Wiedergabe und sichtbarem Control die Grafik nicht animiert werden kann. Eine gleichwertige FPS-Zuweisung löst keine erneute Prüfung aus.

- `AutoPlayChanged`: Wird ausgelöst, wenn sich die Eigenschaft `AutoPlay` geändert hat.

- `AnimationStarted`: Wird ausgelöst, wenn die Animation startet oder nach einer Unterbrechung fortgesetzt wird. Reine Timingänderungen bei weiterlaufender Wiedergabe lösen das Ereignis nicht aus.

- `AnimationStopped`: Wird ausgelöst, wenn eine laufende Animation angehalten oder gestoppt wird, beispielsweise durch Pause oder Unsichtbarkeit. Reine Timingänderungen bei weiterlaufender Wiedergabe lösen das Ereignis nicht aus.

## 3. Eigenschaften

- `AutoPlay`: Steuert, ob die GIF-Animation automatisch gestartet wird, sobald ein animierbares Bild vorhanden und das Control sichtbar ist. Eine explizite Pause verändert den Wert nicht.

- `Gif`: Gibt die aktuell verwendete interne Bitmap zurück oder legt eine GIF-Grafik fest. Bei der Zuweisung erstellt das Control eine eigene Kopie und gibt vom Aufrufer verwaltete Bilder nicht frei. Die zurückgegebene interne Bitmap gehört dem Control und wird bei einem Bildwechsel oder bei der Ressourcenfreigabe freigegeben. `Nothing` stellt eine Kopie des Standardbilds wieder her. Nach Beginn der Ressourcenfreigabe lösen beide Zuweisungen eine `ObjectDisposedException` aus, ohne das übergebene Bild zu verändern. Nach abgeschlossener Freigabe gibt der Getter `Nothing` zurück. Bei aktivierter Teilinvalidierung kann der erste Zugriff nach einer Teilinvalidierung eine vollständige Neuzeichnung auslösen. Die Zuweisung muss auf dem UI-Thread des Controls erfolgen.

- `GifSizeMode`: Legt die Skalierung und Ausrichtung der GIF-Grafik fest. `Zoom` passt das Bild proportional in die Clientfläche ein, `Fill` füllt sie proportional und kann Bildränder abschneiden. `Normal` und `CenterImage` verwenden die Originalgröße.

- `ScalingQualityPreset`: Legt die Interpolation für `ImageSizeMode.Zoom` und `ImageSizeMode.Fill` fest. Standard ist `Photo`; ein Wechsel fordert eine Neuzeichnung an. Andere Werte als `Photo` und `PixelArt` lösen `System.ComponentModel.InvalidEnumArgumentException` aus.

- `UsePartialFrameInvalidation`: Begrenzt bei `True` die Invalidierung von Timer-Framewechseln und `SeekFrame()` nach Möglichkeit auf den Bildbereich. Standard ist `False`; ein Wechsel fordert eine vollständige Neuzeichnung an. Bei unbekannter Geometrie wird zunächst vollständig invalidiert. Nach Herausgabe der internen Bitmap über `Gif` bleibt die vollständige Invalidierung bis zur nächsten Bildzuweisung bestehen. Die Eigenschaft muss auf dem UI-Thread verwendet werden.

- `CustomDisplaySpeed`: Legt fest, ob die timerbasierte Wiedergabe die benutzerdefinierte Anzeigegeschwindigkeit (FPS) oder normalisierte GIF-Metadaten-Delays verwendet. Ein Moduswechsel unterbricht eine laufende Wiedergabe nicht.

- `FramesPerSecond`: Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern pro Sekunde (FPS) fest und dient auch als Fallback für fehlende oder ungültige GIF-Delays. Werte werden auf 1 bis 50 begrenzt; eine nach der Begrenzung gleichwertige Zuweisung bleibt ohne Wirkung.

- `MinimumFrameIntervalMs`: Legt die untere Grenze für normalisierte Frame-Delays in Millisekunden fest. Werte unter 1 werden auf 1 gesetzt; überschreitet die Grenze das Maximum, wird dieses entsprechend erhöht.

- `MaximumFrameIntervalMs`: Legt die obere Grenze für normalisierte Frame-Delays in Millisekunden fest. Werte unter 1 werden auf 1 gesetzt; unterschreitet die Grenze das Minimum, wird dieses entsprechend gesenkt.

- `AdaptiveTiming`: Aktiviert optional eine adaptive Drift-Korrektur bei UI-Thread-Last.

- `DiagnosticsEnabled`: Aktiviert die optionale Diagnostik; der Standardwert ist `False`. Das Aktivieren beginnt eine neue Messung, das Deaktivieren friert die Werte ein. Eine Aktivierung nach Beginn der Ressourcenfreigabe löst `ObjectDisposedException` aus. Die Eigenschaft wird nicht im Designer serialisiert und muss auf dem UI-Thread verwendet werden.

- `Diagnostics`: Gibt eine unveränderliche `AniGifDiagnostics`-Momentaufnahme zurück. Vor der ersten Aktivierung sind die Zähler leer; nach dem Deaktivieren und nach `Dispose()` bleiben die eingefrorenen Werte lesbar. Der Abruf muss auf dem UI-Thread erfolgen. Die Beschreibung der einzelnen Werte steht in [AniGifDiagnostics.md](AniGifDiagnostics.md).

- `ZoomFactor`: Legt den Zoomfaktor für `Zoom` in Prozent der eingepassten Größe fest. Der Standardwert ist 50; Werte außerhalb von 1 bis 100 werden auf diesen Bereich begrenzt. In den übrigen Modi hat der Wert keine Wirkung.

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

- `ResetDiagnostics()`: Setzt alle Diagnosewerte zurück, ohne den Wiedergabezustand zu ändern. Bei aktivierter Diagnostik beginnt eine neue Messung; nach `Dispose()` bleibt der Aufruf ohne Wirkung.

Die Wiedergabesteuerungsmethoden bleiben nach `Dispose()` ohne Wirkung; `GetPlaybackState()` gibt weiterhin den gespeicherten Zustand zurück. Alle genannten Methoden sollen auf dem UI-Thread des Controls aufgerufen werden.

## 5. Hilfstypen/Strukturen

- `ImageSizeMode`: Definiert die Modi `Normal`, `CenterImage`, `Zoom` und `Fill` zur Größenanpassung und Ausrichtung der GIF-Grafik.

- `GifScalingQualityPreset`: `Photo` verwendet `HighQualityBicubic` für geglättete Bilder; `PixelArt` verwendet `NearestNeighbor` und `PixelOffsetMode.Half` für harte Pixelkanten.

- `PlaybackState`: Öffentliche Aufzählung mit `Stopped = 0` (gestoppt), `Playing = 1` (Wiedergabe), und `Paused = 2` (pausiert).

- `AniGifDiagnostics`: Unveränderliche Momentaufnahme mit Aktivierungszustand, aktiver Messzeit, mittlerer Bildrate, gezeichneten und verworfenen Timer-Frames sowie Anzahl, mittlerer und maximaler Dauer der GIF-Zeichenaufrufe. Einzelheiten stehen in [AniGifDiagnostics.md](AniGifDiagnostics.md).

## 6. Hinweise

- Gegenüber der früheren Darstellung berücksichtigen `Zoom` und `Fill` das Seitenverhältnis der Clientfläche. Die Zentrierung erfolgt auch bei `CenterImage` anhand der Clientfläche und nicht anhand der äußeren Control-Größe.

- Skalierte Zielabmessungen werden auf ganze Pixel gerundet; Halbwerte werden aufgerundet und jede Dimension beträgt bei nicht leerer Clientfläche mindestens ein Pixel. Dadurch kann das Seitenverhältnis bei sehr kleinen Darstellungen geringfügig abweichen.

- Bei leerer Clientfläche wird kein Bild gezeichnet. Größenänderungen veranlassen eine vollständige Neuzeichnung, auch wenn keine Animation läuft.

- Die Zielabmessungen verwenden `Int32`. Übersteigt eine skalierte Abmessung dessen darstellbaren Bereich, wird eine `OverflowException` ausgelöst.

- `ScalingQualityPreset` wirkt nur bei `Zoom` und `Fill`; auf `Normal` und `CenterImage` hat es keinen Einfluss. `Photo` entspricht dem bisherigen Standard-Interpolationsverfahren.

- Repaints werden bei laufender Animation nur noch angefordert, wenn das Control sichtbar ist.

- Frame-Delays werden aus den GIF-Metadaten (`0x5100`) gelesen, mit Fallbacks versehen und auf `MinimumFrameIntervalMs`/`MaximumFrameIntervalMs` begrenzt.

- Beide Geschwindigkeitsarten verwenden den Timer; neu gezeichnet wird nur bei einem tatsächlichen Framewechsel.

- Bei aktivierter `UsePartialFrameInvalidation` wird nach einer vollständigen Neuzeichnung der Bildbereich mit einem Sicherheitsrand von zwei Pixeln und begrenzt auf die Clientfläche invalidiert. Bild-, Layout-, Größen-, Hintergrund-, Sichtbarkeits- und Handleänderungen führen bei Bedarf zunächst wieder zu einer vollständigen Invalidierung. `Fill` oder ein großes Bild können die gesamte Clientfläche beanspruchen.

- Im Opt-in wird die Elternkette auf Hintergrund-, Größen-, Positions-, Layout-, Scroll-, Sichtbarkeits- und Elternwechsel beobachtet. Die Ereignisabonnements werden beim Umhängen angepasst und beim Deaktivieren oder Freigeben des Controls entfernt.

- Die Teilinvalidierung ist ein Opt-in: `Paint`- und `Invalidated`-Handler können kleinere Bereiche erhalten. Eigene Paint- oder Hintergrundroutinen dürfen dann nicht bei jedem Frame eine Aktualisierung außerhalb des Bildbereichs benötigen und müssen Transformation und Clip des `Graphics`-Objekts vor der GIF-Zeichnung unverändert lassen. Im Demo kann die Option über „Teilaktualisierung“ ein- und ausgeschaltet werden.

- Mit `AdaptiveTiming = True` wird das nächste Intervall über eine Drift-Korrektur an die tatsächliche UI-Thread-Laufzeit angepasst.

- Bei deaktiviertem `AdaptiveTiming` werden im Wiedergabepfad keine Zeitstempel zur Drift-Korrektur erfasst. Die optionale Diagnostik verwendet weiterhin ihre eigenen Zeitmessungen.

- Änderungen von Geschwindigkeit und Intervallgrenzen übernehmen ein geändertes Sollintervall des aktuellen Frames, ohne die laufende Wiedergabe zu stoppen. Wird dafür ein anderer Timerwert gesetzt, beginnt bei aktiviertem `AdaptiveTiming` die Zeitmessung für dieses Intervall neu. Bei unverändertem Sollintervall bleibt eine adaptive Korrektur erhalten; entspricht das neue Sollintervall bereits dem Timerwert, bleibt auch die laufende Zeitmessung unverändert.

- Bei animierten Bildern werden die unnormalisierten Frame-Delays der internen Bitmap zwischengespeichert. Solange diese Bitmap nicht über `Gif` zurückgegeben wurde, werden ihre Metadaten bei Einstellungsänderungen nicht erneut gelesen.

- Nach einer Rückgabe über `Gif` werden mögliche externe Metadatenänderungen bis zum nächsten Bildwechsel bei jedem tatsächlichen Wechsel von FPS oder Intervallgrenzen erneut eingelesen. Eine Bildzuweisung liest die Metadaten der neuen Bitmap-Kopie ein. Gleichwertige Einstellungen und ein bloßer Geschwindigkeitsmoduswechsel erneuern den Delay-Cache nicht.

- Pause und Fortsetzen verändern `AutoPlay` nicht. `AnimationStopped` beziehungsweise `AnimationStarted` werden nur beim tatsächlichen Anhalten beziehungsweise Wiederanlaufen ausgelöst; eine Frameauswahl löst diese Ereignisse nicht aus.

- Der mit `GetPlaybackState()` abgefragte Zustand ist unabhängig von der Sichtbarkeit. `Playing` bedeutet nicht zwangsläufig, dass die Animation tatsächlich läuft, beispielsweise bei einem unsichtbaren Control oder einer nicht animierbaren Grafik.

- Bei ausgeblendeten Eltern kann der Timer weiterlaufen, wenn WinForms keinen entsprechenden Sichtbarkeitswechsel am Control auslöst. Solange `Visible` wegen der Elternkette `False` liefert, schalten Timer-Ticks keinen Frame um und fordern keine Frame-Neuzeichnung an.

- Beim Austausch von `Gif` werden laufende Wiedergabemechanismen gestoppt, intern erzeugte Bilder freigegeben und der Frame auf `0` gesetzt. Eine explizite Pause bleibt erhalten.

- Jede Instanz besitzt eine eigene Bitmap-Kopie des Standardbilds, damit Frameänderungen andere Instanzen nicht beeinflussen.

- Die Diagnostik erfasst nur bei laufender Wiedergabe, sichtbarer Elternkette, vorhandenem Handle und nicht leerer Clientfläche aktive Zeit und Timer-Framegenerationen. Pause, Stopp, Unsichtbarkeit, fehlendes Handle und leere Clientfläche unterbrechen diese Zeitmessung.

- Sichtbarkeitswechsel der Eltern werden auf dem UI-Thread verarbeitet. Zuvor ausgeführte, blockierende Ereignishandler können den erfassten Zeitpunkt des Wechsels verschieben.

- `ActualFps` ist der Mittelwert der mindestens einmal mit passendem Clip gezeichneten Timer-Framegenerationen über die aktive Messzeit. Ohne messbare aktive Zeit ist der Wert `Nothing`; bei aktiver Zeit ohne gezeichneten Timer-Frame ist er `0`. Wiederholte Repaints und `SeekFrame()` zählen nicht als zusätzliche Frames.

- `DroppedFrames` zählt Timer-Framegenerationen, die vor einem passenden Zeichenaufruf durch einen späteren Timer-Frame ersetzt wurden. Abgebrochene ausstehende Frames bei Pause, Unsichtbarkeit, Handle-Verlust, leerer Clientfläche, `SeekFrame()`, Bildwechsel, Reset oder Ressourcenfreigabe zählen nicht als Drops. Verspätete Timer-Ticks sind keine übersprungenen GIF-Frames.

- Die Renderzeiten erfassen erfolgreiche GIF-Zeichenaufrufe einschließlich Geometrieberechnung und `DrawImage`, aber ohne `MyBase.OnPaint` und externe Paint-Handler. Sie umfassen auch Repaints und statische Bilder; ohne erfolgreichen Zeichenaufruf sind Mittelwert und Maximum `Nothing`. Fehler beim Zeichnen werden weiterhin an den Aufrufer weitergegeben.

- FPS und Drops beschreiben Zeichenaufrufe des Controls, nicht die tatsächliche vollständige Ausgabe auf dem Bildschirm. Bei deaktivierter Diagnostik werden keine Diagnosezeitstempel pro Tick oder Paint erfasst; eine Momentaufnahme wird erst beim Abruf erstellt.

## [7. Optimierungsvorschläge](AniGifOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](AniGifErweiterungsvorschläge.md)