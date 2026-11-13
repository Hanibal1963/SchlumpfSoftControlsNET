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
- `Gif`: Gibt die aktuell verwendete interne Bitmap zurück oder legt eine GIF-Grafik fest. Bei der Zuweisung erstellt das Control eine eigene Kopie und gibt vom Aufrufer verwaltete Bilder nicht frei. Die zurückgegebene interne Bitmap gehört dem Control und wird bei einem Bildwechsel oder bei der Ressourcenfreigabe freigegeben. `Nothing` stellt eine Kopie des Standardbilds wieder her. Nach Beginn der Ressourcenfreigabe lösen beide Zuweisungen eine `ObjectDisposedException` aus, ohne das übergebene Bild zu verändern. Nach abgeschlossener Freigabe gibt der Getter `Nothing` zurück. Die Zuweisung muss auf dem UI-Thread des Controls erfolgen.
- `GifSizeMode`: Legt die Skalierung und Ausrichtung der GIF-Grafik fest. `Zoom` passt das Bild proportional in die Clientfläche ein, `Fill` füllt sie proportional und kann Bildränder abschneiden. `Normal` und `CenterImage` verwenden die Originalgröße.
- `ScalingQualityPreset`: Legt die Interpolation für `ImageSizeMode.Zoom` und `ImageSizeMode.Fill` fest. Standard ist `Photo`; ein Wechsel fordert eine Neuzeichnung an. Andere Werte als `Photo` und `PixelArt` lösen `System.ComponentModel.InvalidEnumArgumentException` aus.
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
- Mit `AdaptiveTiming = True` wird das nächste Intervall über eine Drift-Korrektur an die tatsächliche UI-Thread-Laufzeit angepasst.
- Änderungen von Geschwindigkeit und Intervallgrenzen übernehmen ein geändertes Sollintervall des aktuellen Frames, ohne die laufende Wiedergabe zu stoppen. Wird dafür ein anderer Timerwert gesetzt, beginnt die Zeitmessung für dieses Intervall neu. Bei unverändertem Sollintervall bleibt eine adaptive Korrektur erhalten; entspricht das neue Sollintervall bereits dem Timerwert, bleibt auch die laufende Zeitmessung unverändert.
- Bei animierten Bildern werden Änderungen an den Metadaten der über `Gif` zurückgegebenen Bitmap beim nächsten tatsächlichen Wechsel von FPS oder Intervallgrenzen beziehungsweise bei einer Bildzuweisung eingelesen. Eine gleichwertige FPS-Zuweisung erneuert den Delay-Cache nicht.
- Pause und Fortsetzen verändern `AutoPlay` nicht. `AnimationStopped` beziehungsweise `AnimationStarted` werden nur beim tatsächlichen Anhalten beziehungsweise Wiederanlaufen ausgelöst; eine Frameauswahl löst diese Ereignisse nicht aus.
- Der mit `GetPlaybackState()` abgefragte Zustand ist unabhängig von der Sichtbarkeit. `Playing` bedeutet nicht zwangsläufig, dass die Animation tatsächlich läuft, beispielsweise bei einem unsichtbaren Control oder einer nicht animierbaren Grafik.
- Beim Austausch von `Gif` werden laufende Wiedergabemechanismen gestoppt, intern erzeugte Bilder freigegeben und der Frame auf `0` gesetzt. Eine explizite Pause bleibt erhalten.
- Jede Instanz besitzt eine eigene Bitmap-Kopie des Standardbilds, damit Frameänderungen andere Instanzen nicht beeinflussen.
- Die Diagnostik erfasst nur bei laufender Wiedergabe, sichtbarer Elternkette, vorhandenem Handle und nicht leerer Clientfläche aktive Zeit und Timer-Framegenerationen. Pause, Stopp, Unsichtbarkeit, fehlendes Handle und leere Clientfläche unterbrechen diese Zeitmessung.
- Sichtbarkeitswechsel der Eltern werden auf dem UI-Thread verarbeitet. Zuvor ausgeführte, blockierende Ereignishandler können den erfassten Zeitpunkt des Wechsels verschieben.
- `ActualFps` ist der Mittelwert der mindestens einmal mit passendem Clip gezeichneten Timer-Framegenerationen über die aktive Messzeit. Ohne messbare aktive Zeit ist der Wert `Nothing`; bei aktiver Zeit ohne gezeichneten Timer-Frame ist er `0`. Wiederholte Repaints und `SeekFrame()` zählen nicht als zusätzliche Frames.
- `DroppedFrames` zählt Timer-Framegenerationen, die vor einem passenden Zeichenaufruf durch einen späteren Timer-Frame ersetzt wurden. Abgebrochene ausstehende Frames bei Pause, Unsichtbarkeit, Handle-Verlust, leerer Clientfläche, `SeekFrame()`, Bildwechsel, Reset oder Ressourcenfreigabe zählen nicht als Drops. Verspätete Timer-Ticks sind keine übersprungenen GIF-Frames.
- Die Renderzeiten erfassen erfolgreiche GIF-Zeichenaufrufe einschließlich Geometrieberechnung und `DrawImage`, aber ohne `MyBase.OnPaint` und externe Paint-Handler. Sie umfassen auch Repaints und statische Bilder; ohne erfolgreichen Zeichenaufruf sind Mittelwert und Maximum `Nothing`. Fehler beim Zeichnen werden weiterhin an den Aufrufer weitergegeben.
- FPS und Drops beschreiben Zeichenaufrufe des Controls, nicht die tatsächliche vollständige Ausgabe auf dem Bildschirm. Bei deaktivierter Diagnostik werden keine Diagnosezeitstempel pro Tick oder Paint erfasst; eine Momentaufnahme wird erst beim Abruf erstellt.

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

**Status:** Umgesetzt.

**Umsetzung:**

- `DiagnosticsEnabled` aktiviert die Erfassung optional; `Diagnostics` liefert eine unveränderliche Momentaufnahme und `ResetDiagnostics()` beginnt eine neue Messung.

- Timer-Framegenerationen, aktive Wiedergabezeit und GIF-Zeichenaufrufe werden getrennt erfasst. Wiederholte Repaints, nicht zeichnungsfähige Zustände und absichtliche Framewechsel verfälschen die Framezähler nicht.

- Tests prüfen Messwerte, Clips, beide Geschwindigkeitsarten, adaptives Timing, Zustandswechsel, verschachtelte und umgehängte Eltern, wiedereintretende Handler, Ressourcenfreigabe, unveränderte Darstellung und einen echten WinForms-Timerlauf.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

### 7.7. GIF-Zuweisungen nach der Ressourcenfreigabe absichern

**Status:** Umgesetzt.

**Ausgangslage:** `SetGifImage()` erstellte und speicherte auch nach `Dispose()` eine Bitmap-Kopie. `DisposeAnimationResources()` gab wegen des gesetzten Dispose-Zustands anschließend keine weitere Bitmap frei.

**Umsetzung:**

- `SetGifImage()` prüft den Dispose-Zustand vor der Auswahl der Bildquelle und dem Klonen. Nach Beginn der Ressourcenfreigabe lösen sowohl `Gif = Nothing` als auch Bildzuweisungen eine `ObjectDisposedException` aus.

- Abgelehnte Zuweisungen ändern weder das Bild noch den Timer und lösen keine Ereignisse aus. Das übergebene Bild wird dabei nicht freigegeben oder anderweitig verwendet.

- Tests sichern Zuweisungen während und nach der Ressourcenfreigabe, die Wiederholung solcher Zuweisungen sowie den unveränderten Umgang mit gültigen Bildern ab.

**Nutzen:** Verhindert, dass ein bereits freigegebenes Control erneut eine eigene Bitmap ohne explizite spätere Freigabe hält.

### 7.8. Skalierungsgeometrie an die Clientfläche anpassen

**Status:** Umgesetzt.

**Ausgangslage:** `GetRectStartSize()` in [FunctionDefinitions.vb](FunctionDefinitions.vb) berücksichtigte für `Zoom` und `Fill` nur die Orientierung des Bildes, nicht das Seitenverhältnis der Clientfläche. Beispielsweise wurde ein quadratisches Bild bei einem Control von 400 × 100 und `ZoomFactor = 100` auf 400 × 400 skaliert und dadurch abgeschnitten. Bei 100 × 400 und `Fill` blieb es dagegen 100 × 100 groß und füllte die Fläche nicht aus.

**Umsetzung:**

- Für `Zoom` wird der kleinere Faktor aus Clientbreite geteilt durch Bildbreite und Clienthöhe geteilt durch Bildhöhe mit `ZoomFactor / 100` multipliziert; `Fill` verwendet den größeren Faktor. Die Berechnung berücksichtigt Rundungen auch bei periodischen Faktoren.

- Die Bildposition wird anhand der Clientfläche bestimmt. Leere Clientflächen werden nicht gezeichnet; bei Größenänderungen wird die gesamte Clientfläche neu gezeichnet.

- Tests decken Hoch-, Quer- und quadratische Formate, unterschiedliche Zoomfaktoren, Rundungen, Rahmen, Transparenz, leere Flächen, Größenänderungen und beide Qualitätsoptionen ab. Die XML-Kommentare beschreiben die geänderte Darstellung.

**Nutzen:** `Zoom` passt das Bild zuverlässig in die verfügbare Fläche ein; `Fill` deckt sie vollständig ab.

### 7.9. Unnötige Timer-Neustarts vermeiden

**Status:** Umgesetzt.

**Ausgangslage:** `UpdateTimerState()` stoppte die Wiedergabe zunächst immer, auch wenn sie nach einer Einstellungsänderung weiterlaufen sollte. Zudem löste `FramesPerSecond` selbst bei unverändertem Wert die Aktualisierung aus. Bei laufender Animation entstanden dadurch zusätzliche Stopp- und Startereignisse.

**Umsetzung:**

- Der begrenzte FPS-Wert wird mit dem bisherigen Wert verglichen; gleichwertige Zuweisungen beenden die Aktualisierung vor `FramesPerSecondChanged`.

- `UpdateTimerState()` unterscheidet Zustandswechsel von Konfigurationsänderungen. Ein geändertes Sollintervall wird bei Bedarf ohne explizites Stoppen der Wiedergabe übernommen; eine bestehende adaptive Korrektur bleibt bei unverändertem Sollintervall erhalten.

- Tests sichern FPS- und Intervallgrenzen, Metadaten-Fallbacks, beide Geschwindigkeitsarten, adaptives Timing, Sichtbarkeit, Pause, `NoAnimation`, wiedereintretende Ereignishandler und reale Timer-Ticks ab. Gleichwertige FPS-Zuweisungen lösen auch keine erneute `NoAnimation`-Prüfung aus.

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

**Status:** Umgesetzt.

**Ausgangslage:** Beide Geschwindigkeitsarten verwendeten den Timer. Für den privaten `OnNextFrame`-Handler war kein `ImageAnimator.Animate()` registriert; `StopAnimationCore()` rief dennoch `ImageAnimator.StopAnimate()` auf.

**Umsetzung:**

- Der ungenutzte Handler, das zugehörige Delegate, der überflüssige `StopAnimate()`-Aufruf und die ungenutzte Konstante `DefaultFrameDelayMs` wurden entfernt. Die FPS-basierte Fallback-Berechnung bleibt unverändert.

- Kommentare und XML-Dokumentation beschreiben die Timer-basierte Wiedergabe. Tests sichern Start, Framewechsel, Stopp, Pause, Moduswechsel, Sichtbarkeit, Bildwechsel und Ressourcenfreigabe in beiden Geschwindigkeitsarten ab.

**Nutzen:** Vereinfacht die Wartung und vermeidet eine unnötige Abmeldung eines nicht registrierten Wiedergabemechanismus.

### 7.15. Empfohlene Reihenfolge

1. Frame-Delay-Metadaten wiederverwenden (7.10).
2. Timer-Ticks verschlanken (7.11).
3. Sichtbarkeitsprüfung vereinfachen (7.12).
4. Aktualisierungsbereich bei Framewechseln begrenzen (7.13).

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
