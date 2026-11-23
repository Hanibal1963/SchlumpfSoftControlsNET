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

---

## 2. Ereignisse

- `NoAnimation`: Wird ausgelöst, wenn bei gewünschter Wiedergabe und sichtbarem Control die Grafik nicht animiert werden kann. Eine gleichwertige FPS-Zuweisung löst keine erneute Prüfung aus.

- `AutoPlayChanged`: Wird ausgelöst, wenn sich die Eigenschaft `AutoPlay` geändert hat.

- `AnimationStarted`: Wird ausgelöst, wenn die Animation startet oder nach einer Unterbrechung fortgesetzt wird. Reine Timingänderungen bei weiterlaufender Wiedergabe lösen das Ereignis nicht aus.

- `AnimationStopped`: Wird ausgelöst, wenn eine laufende Animation angehalten oder gestoppt wird, beispielsweise durch Pause oder Unsichtbarkeit. Reine Timingänderungen bei weiterlaufender Wiedergabe lösen das Ereignis nicht aus.

- `FrameChanged(sender, e As FrameChangedEventArgs)`: Wird nach einem tatsächlichen Framewechsel durch den Timer oder `SeekFrame()` ausgelöst, auch bei indirekter Frameauswahl durch eine Bereichsänderung oder einen Neustart. `e.PreviousFrame` und `e.CurrentFrame` enthalten die nullbasierten Indizes. Eine Bildzuweisung, ein unveränderter Index und ein Einzelbildsegment ohne Framewechsel lösen das Ereignis nicht aus. Das Ereignis erfolgt nach der Frameauswahl und Timerkonfiguration, vor der Invalidierung; es bestätigt keine Bildschirmdarstellung.

- `PlaybackCompleted(sender, e As EventArgs)`: Wird einmal nach dem regulären Abschluss einer begrenzten Wiedergabe ausgelöst, nach `AutoPlayChanged` und dem gegebenenfalls ausgelösten `AnimationStopped`. Pause, manuelles Stoppen, Fehler und unbegrenzte Wiedergabe lösen es nicht aus. Ein vorheriger Ereignishandler kann bereits eine neue Wiedergabe gestartet haben.

- `PlaybackError(sender, e As System.IO.ErrorEventArgs)`: Meldet `ArgumentException`, `ObjectDisposedException` und `ExternalException` bei der Frameauswahl durch Timer oder `SeekFrame()`. `e.GetException()` liefert die ursprüngliche Ausnahme, die anschließend erneut ausgelöst wird; der Wiedergabezustand wird dadurch nicht automatisch geändert. Validierungsfehler, Bildinitialisierung, Zeichenfehler und Ausnahmen von Ereignishandlern werden nicht als `PlaybackError` gemeldet. Die Ereignisse werden synchron auf dem aufrufenden UI-Thread ausgelöst; Ausnahmen der Handler werden weitergegeben.

---

## 3. Eigenschaften

- `AutoPlay`: Steuert, ob die GIF-Animation automatisch gestartet wird, sobald ein animierbares Bild vorhanden und das Control sichtbar ist. Eine explizite Pause verändert den Wert nicht.

- `LoopCount`: Anzahl der Segmentdurchläufe; `0` (Standard) bedeutet unbegrenzte Wiedergabe. Negative Werte lösen `ArgumentOutOfRangeException` aus. Nach dem letzten Frameintervall eines begrenzten Durchlaufs wird `AutoPlay` auf `False` und der Wiedergabezustand auf `Stopped` gesetzt; der letzte Frame bleibt sichtbar. Ein erneuter Start beginnt am Segmentanfang.

- `PlayRangeStart`: Nullbasierter erster Segmentframe, Standard `0`. Negative Werte lösen `ArgumentOutOfRangeException` aus.

- `PlayRangeEnd`: Nullbasierter letzter Segmentframe, einschließlich; `-1` (Standard) bezeichnet das Bildende. Werte kleiner als `-1` lösen `ArgumentOutOfRangeException` aus.

  Die wirksamen Grenzen werden auf das aktuelle Bild begrenzt; ein Ende vor dem Anfang ergibt ein Segment aus einem Frame. Änderungen des Bereichs setzen den Durchlaufzähler zurück und wählen bei Bedarf den Segmentanfang. Die Werte bleiben bei Bildwechsel erhalten. Die Eigenschaften müssen auf dem UI-Thread verwendet werden.

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

---

## 4. Funktionen

- `LoadGif(source As Stream)`: Liest GIF-Daten ab der aktuellen Position bis zum Streamende, auch aus nicht suchbaren Streams. Der Stream muss lesbar sein, bleibt geöffnet und kann nach dem Aufruf freigegeben werden. `Nothing` löst `ArgumentNullException`, ein nicht lesbarer Stream `ArgumentException` aus. Lesefehler werden weitergegeben.

- `LoadGif(data As Byte())`: Lädt GIF-Daten aus einem Bytearray. Das Array wird weder verändert noch für die spätere Wiedergabe benötigt. `Nothing` löst `ArgumentNullException` aus.

- `LoadGif(path As String)`: Lädt eine GIF-Datei und schließt sie nach dem Einlesen. `Nothing` löst `ArgumentNullException`, ein leerer oder nur aus Leerraum bestehender Pfad `ArgumentException` aus. Dateisystem- und Zugriffsfehler werden weitergegeben.

  Alle drei Überladungen akzeptieren ausschließlich GIF-Daten, erstellen eine interne Datenkopie und verwenden die bestehende Bildwechsel- und Wiedergabesteuerung. Ungültige oder andere Bildformate lösen `ArgumentException` aus; je nach GDI+-Version können ungültige Bilddaten auch `OutOfMemoryException` auslösen. Bei Validierungs- oder Lesefehlern bleibt das bisherige Bild erhalten. Nach Beginn der Ressourcenfreigabe wird `ObjectDisposedException` ausgelöst. Die Methoden müssen auf dem UI-Thread aufgerufen werden. Der interne Datenstream wird bei Bildwechsel oder Ressourcenfreigabe nach der Bitmap freigegeben.

- `StartAnimation()`: Setzt `AutoPlay` auf `True` und startet die Animation beziehungsweise setzt eine Pause beim aktuellen Frame fort. Ein bereits laufender Start ist wirkungslos.

- `PauseAnimation()`: Hält eine laufende Wiedergabe beim aktuellen Frame an, ohne `AutoPlay` zu ändern. Bei bereits pausierter oder gestoppter Wiedergabe bleibt der Aufruf ohne Wirkung.

- `ResumeAnimation()`: Setzt eine pausierte Wiedergabe beim aktuellen Frame fort. Dessen vollständiges Intervall beginnt neu; bei unsichtbarem Control wird bis zur erneuten Sichtbarkeit gewartet. Ohne vorherige Pause bleibt der Aufruf ohne Wirkung.

- `SeekFrame(index As Int32)`: Wählt den nullbasierten Frame `index` aus, ohne den Wiedergabezustand zu ändern. Gültig sind Werte von `0` bis zum letzten Frame; bei einem statischen Bild ist nur `0` gültig. Ungültige Werte lösen `ArgumentOutOfRangeException` aus, ein fehlendes Bild löst `InvalidOperationException` aus. Bei laufender Wiedergabe beginnt das Intervall des gewählten Frames neu.

- `StopAnimation()`: Setzt `AutoPlay` auf `False` und stoppt die Wiedergabe. Der aktuelle Frame bleibt erhalten.

- `GetPlaybackState() As PlaybackState`: Gibt den gewünschten Wiedergabezustand zurück. Der Anfangszustand ist `Stopped`; bei der Ressourcenfreigabe wird der Zustand auf `Stopped` gesetzt. Die Methode benötigt keine Parameter und löst keine eigenen Ausnahmen aus.

- `ResetDiagnostics()`: Setzt alle Diagnosewerte zurück, ohne den Wiedergabezustand zu ändern. Bei aktivierter Diagnostik beginnt eine neue Messung; nach `Dispose()` bleibt der Aufruf ohne Wirkung.

Die Wiedergabesteuerungsmethoden bleiben nach `Dispose()` ohne Wirkung; `GetPlaybackState()` gibt weiterhin den gespeicherten Zustand zurück. Alle genannten Methoden sollen auf dem UI-Thread des Controls aufgerufen werden.

---

## 5. Hilfstypen/Strukturen

- `FrameChangedEventArgs`: Unveränderliche Ereignisdaten mit `PreviousFrame` und `CurrentFrame` als `Int32`. Der öffentliche Konstruktor `New(previousFrame As Int32, currentFrame As Int32)` übernimmt die Werte unverändert und führt keine Validierung durch.

- `ImageSizeMode`: Definiert die Modi `Normal`, `CenterImage`, `Zoom` und `Fill` zur Größenanpassung und Ausrichtung der GIF-Grafik.

- `GifScalingQualityPreset`: `Photo` verwendet `HighQualityBicubic` für geglättete Bilder; `PixelArt` verwendet `NearestNeighbor` und `PixelOffsetMode.Half` für harte Pixelkanten.

- `PlaybackState`: Öffentliche Aufzählung mit `Stopped = 0` (gestoppt), `Playing = 1` (Wiedergabe), und `Paused = 2` (pausiert).

- `AniGifDiagnostics`: Unveränderliche Momentaufnahme mit Aktivierungszustand, aktiver Messzeit, mittlerer Bildrate, gezeichneten und verworfenen Timer-Frames sowie Anzahl, mittlerer und maximaler Dauer der GIF-Zeichenaufrufe. Einzelheiten stehen in [AniGifDiagnostics.md](AniGifDiagnostics.md).

---

## 6. Hinweise

- Der Smart-Tag bietet die Einstellungen `AutoPlay`, `CustomDisplaySpeed`, `FramesPerSecond` und `GifSizeMode`. Änderungen erfolgen über die Property-Descriptoren des Controls, damit der Designer sie wie Änderungen im Eigenschaftenfenster behandeln kann. Die Bildrate wirkt unmittelbar nur bei aktivierter benutzerdefinierter Geschwindigkeit.

- Der Smart-Tag verwendet im klassischen .NET-Framework-Designer die Framework-Designer-API und für .NET 8 bis .NET 10 die Out-of-Process-Designer-API des `Microsoft.WinForms.Designer.SDK`. Die Designerregistrierung wird zur Laufzeit als Typname bereitgestellt und erfordert keinen laufenden Designer-Host.

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

- Ein Durchlauf endet nach Ablauf des Intervalls des letzten Segmentframes. Bei einem Segment aus einem Frame wird dessen Intervall ebenfalls für jeden Durchlauf abgewartet. Pause und Unsichtbarkeit erhalten den Zähler; ein expliziter Stopp, eine Änderung von `LoopCount` oder des Bereichs, `SeekFrame()` und ein Bildwechsel setzen ihn zurück. Ein außerhalb des Segments ausgewählter Frame wird beim nächsten Timer-Tick durch den Segmentanfang ersetzt. Statische Bilder lösen weiterhin `NoAnimation` aus.

- Der mit `GetPlaybackState()` abgefragte Zustand ist unabhängig von der Sichtbarkeit. `Playing` bedeutet nicht zwangsläufig, dass die Animation tatsächlich läuft, beispielsweise bei einem unsichtbaren Control oder einer nicht animierbaren Grafik.

- Bei ausgeblendeten Eltern kann der Timer weiterlaufen, wenn WinForms keinen entsprechenden Sichtbarkeitswechsel am Control auslöst. Solange `Visible` wegen der Elternkette `False` liefert, schalten Timer-Ticks keinen Frame um und fordern keine Frame-Neuzeichnung an.

- Beim Austausch von `Gif` werden laufende Wiedergabemechanismen gestoppt, intern erzeugte Bilder freigegeben und der Frame auf den wirksamen Segmentanfang gesetzt. Eine explizite Pause bleibt erhalten.

- Jede Instanz besitzt eine eigene Bitmap-Kopie des Standardbilds, damit Frameänderungen andere Instanzen nicht beeinflussen.

- Die Diagnostik erfasst nur bei laufender Wiedergabe, sichtbarer Elternkette, vorhandenem Handle und nicht leerer Clientfläche aktive Zeit und Timer-Framegenerationen. Pause, Stopp, Unsichtbarkeit, fehlendes Handle und leere Clientfläche unterbrechen diese Zeitmessung.

- Sichtbarkeitswechsel der Eltern werden auf dem UI-Thread verarbeitet. Zuvor ausgeführte, blockierende Ereignishandler können den erfassten Zeitpunkt des Wechsels verschieben.

- `ActualFps` ist der Mittelwert der mindestens einmal mit passendem Clip gezeichneten Timer-Framegenerationen über die aktive Messzeit. Ohne messbare aktive Zeit ist der Wert `Nothing`; bei aktiver Zeit ohne gezeichneten Timer-Frame ist er `0`. Wiederholte Repaints und `SeekFrame()` zählen nicht als zusätzliche Frames.

- `DroppedFrames` zählt Timer-Framegenerationen, die vor einem passenden Zeichenaufruf durch einen späteren Timer-Frame ersetzt wurden. Abgebrochene ausstehende Frames bei Pause, Unsichtbarkeit, Handle-Verlust, leerer Clientfläche, `SeekFrame()`, Bildwechsel, Reset oder Ressourcenfreigabe zählen nicht als Drops. Verspätete Timer-Ticks sind keine übersprungenen GIF-Frames.

- Die Renderzeiten erfassen erfolgreiche GIF-Zeichenaufrufe einschließlich Geometrieberechnung und `DrawImage`, aber ohne `MyBase.OnPaint` und externe Paint-Handler. Sie umfassen auch Repaints und statische Bilder; ohne erfolgreichen Zeichenaufruf sind Mittelwert und Maximum `Nothing`. Fehler beim Zeichnen werden weiterhin an den Aufrufer weitergegeben.

- FPS und Drops beschreiben Zeichenaufrufe des Controls, nicht die tatsächliche vollständige Ausgabe auf dem Bildschirm. Bei deaktivierter Diagnostik werden keine Diagnosezeitstempel pro Tick oder Paint erfasst; eine Momentaufnahme wird erst beim Abruf erstellt.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Stabilere GIF-Wiedergabe mit geringerer CPU-Last und sauberer Ressourcenfreigabe.

---

### 7.2. Rendering-Pipeline entkoppeln

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

---

### 7.3. Ressourcenlebenszyklus härten

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

---

### 7.4. Frame-Timing präzisieren

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

---

### 7.5. Qualitätsoptionen beim Skalieren

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Für die `GifSizeMode`-Einstellungen `Zoom` und `Fill` den Interpolationsmodus über `ScalingQualityPreset` festlegen.

- Pixel-Art und Foto-Inhalt über die Presets `PixelArt` (`NearestNeighbor`) und `Photo` (`HighQualityBicubic`) steuern.

**Nutzen:** Bessere Bildqualität bei Zoom/Skalierung.

---

### 7.6. Diagnostik für Betrieb hinzufügen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- `DiagnosticsEnabled` aktiviert die Erfassung optional; `Diagnostics` liefert eine unveränderliche Momentaufnahme und `ResetDiagnostics()` beginnt eine neue Messung.

- Timer-Framegenerationen, aktive Wiedergabezeit und GIF-Zeichenaufrufe werden getrennt erfasst. Wiederholte Repaints, nicht zeichnungsfähige Zustände und absichtliche Framewechsel verfälschen die Framezähler nicht.

- Tests prüfen Messwerte, Clips, beide Geschwindigkeitsarten, adaptives Timing, Zustandswechsel, verschachtelte und umgehängte Eltern, wiedereintretende Handler, Ressourcenfreigabe, unveränderte Darstellung und einen echten WinForms-Timerlauf.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

---

### 7.7. GIF-Zuweisungen nach der Ressourcenfreigabe absichern

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `SetGifImage()` erstellte und speicherte auch nach `Dispose()` eine Bitmap-Kopie. `DisposeAnimationResources()` gab wegen des gesetzten Dispose-Zustands anschließend keine weitere Bitmap frei.

**Umsetzung:**

- `SetGifImage()` prüft den Dispose-Zustand vor der Auswahl der Bildquelle und dem Klonen. Nach Beginn der Ressourcenfreigabe lösen sowohl `Gif = Nothing` als auch Bildzuweisungen eine `ObjectDisposedException` aus.

- Abgelehnte Zuweisungen ändern weder das Bild noch den Timer und lösen keine Ereignisse aus. Das übergebene Bild wird dabei nicht freigegeben oder anderweitig verwendet.

- Tests sichern Zuweisungen während und nach der Ressourcenfreigabe, die Wiederholung solcher Zuweisungen sowie den unveränderten Umgang mit gültigen Bildern ab.

**Nutzen:** Verhindert, dass ein bereits freigegebenes Control erneut eine eigene Bitmap ohne explizite spätere Freigabe hält.

---

### 7.8. Skalierungsgeometrie an die Clientfläche anpassen

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `GetRectStartSize()` in [FunctionDefinitions.vb](FunctionDefinitions.vb) berücksichtigte für `Zoom` und `Fill` nur die Orientierung des Bildes, nicht das Seitenverhältnis der Clientfläche. Beispielsweise wurde ein quadratisches Bild bei einem Control von 400 × 100 und `ZoomFactor = 100` auf 400 × 400 skaliert und dadurch abgeschnitten. Bei 100 × 400 und `Fill` blieb es dagegen 100 × 100 groß und füllte die Fläche nicht aus.

**Umsetzung:**

- Für `Zoom` wird der kleinere Faktor aus Clientbreite geteilt durch Bildbreite und Clienthöhe geteilt durch Bildhöhe mit `ZoomFactor / 100` multipliziert; `Fill` verwendet den größeren Faktor. Die Berechnung berücksichtigt Rundungen auch bei periodischen Faktoren.

- Die Bildposition wird anhand der Clientfläche bestimmt. Leere Clientflächen werden nicht gezeichnet; bei Größenänderungen wird die gesamte Clientfläche neu gezeichnet.

- Tests decken Hoch-, Quer- und quadratische Formate, unterschiedliche Zoomfaktoren, Rundungen, Rahmen, Transparenz, leere Flächen, Größenänderungen und beide Qualitätsoptionen ab. Die XML-Kommentare beschreiben die geänderte Darstellung.

**Nutzen:** `Zoom` passt das Bild zuverlässig in die verfügbare Fläche ein; `Fill` deckt sie vollständig ab.

---

### 7.9. Unnötige Timer-Neustarts vermeiden

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `UpdateTimerState()` stoppte die Wiedergabe zunächst immer, auch wenn sie nach einer Einstellungsänderung weiterlaufen sollte. Zudem löste `FramesPerSecond` selbst bei unverändertem Wert die Aktualisierung aus. Bei laufender Animation entstanden dadurch zusätzliche Stopp- und Startereignisse.

**Umsetzung:**

- Der begrenzte FPS-Wert wird mit dem bisherigen Wert verglichen; gleichwertige Zuweisungen beenden die Aktualisierung vor `FramesPerSecondChanged`.

- `UpdateTimerState()` unterscheidet Zustandswechsel von Konfigurationsänderungen. Ein geändertes Sollintervall wird bei Bedarf ohne explizites Stoppen der Wiedergabe übernommen; eine bestehende adaptive Korrektur bleibt bei unverändertem Sollintervall erhalten.

- Tests sichern FPS- und Intervallgrenzen, Metadaten-Fallbacks, beide Geschwindigkeitsarten, adaptives Timing, Sichtbarkeit, Pause, `NoAnimation`, wiedereintretende Ereignishandler und reale Timer-Ticks ab. Gleichwertige FPS-Zuweisungen lösen auch keine erneute `NoAnimation`-Prüfung aus.

**Nutzen:** Weniger Timer-Neustarts, Ereignisse und Unterbrechungen bei unverändertem Wiedergabezustand.

---

### 7.10. Frame-Delay-Metadaten wiederverwenden

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `RefreshFrameDelayCache()` las bei animierten Bildern die GIF-Metadaten erneut und erstellte ein neues Array, wenn `FramesPerSecond`, `MinimumFrameIntervalMs` oder `MaximumFrameIntervalMs` geändert wurde.

**Umsetzung:**

- Unnormalisierte, in Millisekunden umgerechnete Delay-Rohwerte werden pro Bitmap gespeichert. Bei FPS-Änderungen werden bei unveränderten Metadaten nur benötigte Fallbacks angepasst; geänderte Intervallgrenzen werden erneut auf die Rohwerte angewendet. Die normalisierten und unnormalisierten Arrays werden bei unveränderter Frameanzahl wiederverwendet.

- Nach der Rückgabe der internen Bitmap über `Gif` werden die Metadaten bei jeder tatsächlichen FPS- oder Grenzänderung erneut geprüft, damit auch spätere Änderungen über eine gehaltene Referenz erkannt werden. Die Markierung wird beim Bildwechsel zurückgesetzt; bei statischen Bildern und `Dispose()` werden die Caches geleert.

- Tests sichern fehlende, unvollständige und ungültige Metadaten, Überläufe, Fallbacks, Bildwechsel, externe Änderungen und beide Geschwindigkeitsarten ab. Der Rohdaten-Cache benötigt zusätzlich ungefähr 4 Byte pro Frame zuzüglich Array-Overhead; nach Herausgabe der Bitmap entstehen weiterhin temporäre Allokationen für Metadatenzugriffe.

**Nutzen:** Weniger Metadatenzugriffe und Array-Neuanlagen bei Einstellungsänderungen, ohne die dokumentierte Behandlung extern veränderter Metadaten zu ändern.

**Messung:** Bei einem lokalen Debug-Lauf unter .NET Framework 4.8.1 mit je 9.000 FPS- und Grenzänderungen nach dem Aufwärmen sanken die verwalteten Thread-Allokationen ohne vorherigen `Gif`-Getter bei 3 Frames von 1.728.000 auf 0 Byte und bei 169 Frames von 13.680.000 auf 0 Byte. Nach Herausgabe der Bitmap sanken sie auf 1.368.000 beziehungsweise 7.344.000 Byte. Die Wiedergabe war gestoppt; die Werte sind keine Messung der Timerleistung.

---

### 7.11. Wiederholte Berechnungen pro Timer-Tick reduzieren

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `Timer_Tick()` las und berechnete die verstrichene Zeit auch bei deaktiviertem `AdaptiveTiming`. Im benutzerdefinierten Geschwindigkeitsmodus wurde das FPS-Intervall bei jedem Framewechsel erneut berechnet.

**Umsetzung:**

- Zeitstempel und Zeitdifferenz werden nur für das adaptive Timing erfasst. Beim Aktivieren, Starten, Fortsetzen und bei einer Frameauswahl während laufender Wiedergabe wird der Zeitstempel weiterhin initialisiert; beim Deaktivieren wird er zurückgesetzt, ohne das aktuelle Timerintervall sofort zu ändern. Die Diagnostik bleibt unabhängig davon.

- Das begrenzte FPS-Intervall wird bei der Initialisierung sowie bei Änderungen von FPS oder Intervallgrenzen vorberechnet und als benutzerdefiniertes Intervall beziehungsweise Metadaten-Fallback wiederverwendet. Es wird vor dem erneuten Metadatenzugriff aktualisiert, damit es auch nach einer dort ausgelösten Ausnahme zu den geänderten Einstellungen passt. Rundung, Fallbacks und Grenzen bleiben unverändert.

- Tests sichern beide Geschwindigkeitsarten, Rundung und Grenzwechsel, Metadatenfehler, das Ein- und Ausschalten von `AdaptiveTiming`, Pause, Sichtbarkeit, Frameauswahl und die unabhängige Diagnostik ab.

**Nutzen:** Weniger wiederholte Berechnungen im häufig aufgerufenen Timerpfad bei unverändertem Wiedergabeverhalten.

**Messung:** In einem lokalen Debug-Lauf unter .NET Framework 4.8.1 benötigten jeweils 2.000.000 isolierte FPS-Intervallabfragen bei 25 FPS zuvor 293–874 ms und danach 11–14 ms; bei 16 FPS waren es zuvor 1.362–1.404 ms und danach 11–12 ms. Bei jeweils 12.000 manuell aufgerufenen Ticks ohne Control-Handle schwankten die Zeiten über beide Geschwindigkeits- und Timingarten zuvor zwischen 3,2 und 9,9 Sekunden und danach zwischen 3,0 und 9,9 Sekunden. Daraus lässt sich kein verlässlicher Gesamtgeschwindigkeitsgewinn ableiten; Bildausgabe und reale Timerintervalle wurden dabei nicht gemessen.

---

### 7.12. Sichtbarkeitsprüfung vereinfachen

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `IsControlTreeVisible()` prüfte nach `Visible` nochmals alle übergeordneten Controls. Bei aktivierter Diagnostik wurde die Sichtbarkeit im Timerpfad unmittelbar danach erneut vor dem Framewechsel geprüft.

**Umsetzung:**

- `IsControlTreeVisible()` verwendet den Getter von [Control.Visible](https://learn.microsoft.com/dotnet/api/system.windows.forms.control.visible), der die Sichtbarkeit der Elternkette bereits berücksichtigt.

- Bei aktivierter Diagnostik und vorhandenem Bild teilen sich Diagnostik und Frameprüfung dieselbe Sichtbarkeitsmomentaufnahme; zwischen diesen Prüfungen werden keine Ereignishandler ausgelöst. Andere Diagnoseaufrufe und `SeekFrame()` prüfen die Sichtbarkeit weiterhin eigenständig.

- Vor `Invalidate()` bleibt die erneute Sichtbarkeitsprüfung erhalten: Ein abgeleiteter Timer kann bei einer Intervalländerung über seinen überschreibbaren `Enabled`-Getter wiedereintreten und die Sichtbarkeit ändern. Auch die Schutzprüfungen für Dispose-Zustand, Handle und Clientgröße bleiben bestehen.

- Tests sichern verschachtelte und umgehängte Eltern, beide Geschwindigkeitsarten, adaptives Timing, Diagnostik, Pause, fehlendes Handle, leere Clientfläche, Timer-Rückeintritte und einen echten WinForms-Timerlauf ab. Das bestehende Verhalten bleibt erhalten: Bei ausgeblendeten Eltern kann der Timer weiterlaufen, ohne Frames umzuschalten oder Frame-Neuzeichnungen anzufordern.

**Nutzen:** Weniger redundante Abfragen pro Frame bei unverändertem Verhalten für ausgeblendete Controls.

**Messung:** In zwei lokalen Release-Läufen unter .NET Framework 4.8.1 mit jeweils drei Messungen und je 500.000 isolierten Sichtbarkeitsabfragen lagen die Zeiten ohne Eltern zuvor bei 14,8–53,6 ms und danach bei 4,8–10,8 ms; bei zehn Eltern waren es zuvor 860,0–916,7 ms und danach 22,7–71,7 ms. Je 800 manuell aufgerufene Ticks mit Handle, beiden Geschwindigkeitsarten, aktivierter oder deaktivierter Diagnostik und null beziehungsweise fünf Eltern schwankten zuvor zwischen 192 und 708 ms und danach zwischen 194 und 786 ms. Daraus lässt sich kein verlässlicher Gesamtgeschwindigkeitsgewinn ableiten; tatsächliche Timerintervalle und Bildausgabe wurden nicht gemessen.

---

### 7.13. Aktualisierungsbereich bei Framewechseln begrenzen

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** `InvalidateIfVisible()` forderte bei einem Framewechsel die Neuzeichnung der gesamten Clientfläche an, auch wenn das Bild nur einen Teil davon belegte.

**Umsetzung:**

- `UsePartialFrameInvalidation` aktiviert die Optimierung ausdrücklich; standardmäßig bleibt die vollständige Invalidierung einschließlich der bisherigen Paint- und Invalidated-Bereiche erhalten. Das Demo bietet dafür den Schalter „Teilaktualisierung“.

- Nach einer eigenen vollständigen Invalidierung und einem erfolgreichen Paint mit einem die Clientfläche abdeckenden Cliprechteck wird das bereits berechnete Bildzielrechteck wiederverwendet. Es erhält einen Sicherheitsrand von zwei Pixeln und wird auf die Clientfläche begrenzt. Ist der Bereich leer, unbekannt oder vollständig, bleibt die vollständige Invalidierung erhalten; `Rectangle.Empty` wird nicht an `Invalidate(Rectangle)` übergeben.

- Bild-, Layout-, Größen-, Hintergrund-, Sichtbarkeits- und Handleänderungen verwerfen den Bereich. Nach Herausgabe der veränderbaren Bitmap über `Gif` wird bis zur nächsten Bildzuweisung vollständig invalidiert; der erste Zugriff nach einer Teilinvalidierung fordert sofort eine vollständige Neuzeichnung an. Wiedereintretende Handler können Bild und Geometrie ändern oder das Control freigeben, ohne einen veralteten Bereich zu veröffentlichen.

- Bei aktivierter Option werden relevante Hintergrund-, Layout-, Scroll-, Sichtbarkeits- und Elternänderungen der gesamten Elternkette beobachtet. Die Abonnements werden beim Umhängen aktualisiert und beim Deaktivieren oder Freigeben entfernt; die bestehende Diagnostik verwaltet ihre Abonnements weiterhin unabhängig davon.

- Tests vergleichen aufeinanderfolgende vollständige und partielle Paints für alle Größenmodi, beide Qualitätsoptionen, transparente GIF-Frames und Hintergründe, Hintergrundbilder, Rahmen, unterschiedliche Bildgrößen und Zoomfaktoren. Sie sichern außerdem Größen- und Bildwechsel, Bitmap-Mutationen, Diagnosezähler, `SeekFrame()`, Rückeintritte, Fehlerzeitpunkte und einen echten WinForms-Timerlauf ab.

**Nutzen:** Bei unveränderter Geometrie kann die zu aktualisierende Fläche und damit der Zeichenaufwand sinken. Externe Paint- oder Hintergrundroutinen dürfen im Opt-in nicht auf eine frameweise Aktualisierung außerhalb des Bildbereichs angewiesen sein und müssen Transformation und Clip des `Graphics`-Objekts vor der GIF-Zeichnung unverändert lassen.

**Messung:** In lokalen Release-Läufen unter .NET Framework 4.8.1 mit einer Clientfläche von 800 × 600, einem GIF von 32 × 32 und deaktivierter Diagnostik betrug die pro Frame invalidierte und im Paint-Clip ausgewiesene Fläche im Modus `Normal` zuvor 480.000 Pixel; drei Messungen mit je 200 manuell ausgelösten Frames benötigten 989–1.270 ms. Im endgültigen Build sank die Fläche im Teilmodus auf 1.156 Pixel. Bei vier abwechselnden Messungen mit je 50 Frames lagen die Zeiten im Vollmodus bei 128–204 ms und im Teilmodus bei 35–43 ms. Bei `Zoom` mit 50 % sank die Fläche auf 92.416 Pixel, bei `Fill` blieb sie vollständig; die `Fill`-Zeiten überlappten. Die Werte schwankten, und der Test erzwingt durch `Update()` synchrones Zeichnen auf einer nicht sichtbaren Testform statt regulärer asynchroner Timerverarbeitung.

---

### 7.14. Ungenutzte ImageAnimator-Pfade entfernen

**Status:** &#x2705; Umgesetzt.

**Ausgangslage:** Beide Geschwindigkeitsarten verwendeten den Timer. Für den privaten `OnNextFrame`-Handler war kein `ImageAnimator.Animate()` registriert; `StopAnimationCore()` rief dennoch `ImageAnimator.StopAnimate()` auf.

**Umsetzung:**

- Der ungenutzte Handler, das zugehörige Delegate, der überflüssige `StopAnimate()`-Aufruf und die ungenutzte Konstante `DefaultFrameDelayMs` wurden entfernt. Die FPS-basierte Fallback-Berechnung bleibt unverändert.

- Kommentare und XML-Dokumentation beschreiben die Timer-basierte Wiedergabe. Tests sichern Start, Framewechsel, Stopp, Pause, Moduswechsel, Sichtbarkeit, Bildwechsel und Ressourcenfreigabe in beiden Geschwindigkeitsarten ab.

**Nutzen:** Vereinfacht die Wartung und vermeidet eine unnötige Abmeldung eines nicht registrierten Wiedergabemechanismus.

---

### 7.15. Empfohlene Reihenfolge

Aktuell sind keine Optimierungspunkte mehr offen.

---

## 8. geplante Erweiterungen

### 8.1. Playback-API erweitern

**Status:** &#x2705; Umgesetzt.

**Umsetzung:** `PauseAnimation()`, `ResumeAnimation()` und `SeekFrame(index)` verwenden einen internen Wiedergabezustand und steuern den Timer entsprechend.

---

### 8.2. Loop- und Segmentsteuerung

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.

**Umsetzung:** `LoopCount`, `PlayRangeStart` und `PlayRangeEnd` steuern die begrenzte oder endlose Segmentwiedergabe. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

---

### 8.3. Eventmodell ausbauen

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.

**Umsetzung:** Ereignisse melden Framewechsel, den regulären Abschluss begrenzter Wiedergaben und Fehler bei der Frameauswahl. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

---

### 8.4. Quellenvielfalt erhöhen

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.

**Umsetzung:** `LoadGif(...)` lädt GIF-Daten aus Stream, Bytearray oder Datei. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

---

### 8.5. Design-Time-Unterstützung

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.

**Umsetzung:** Eine `DesignerActionList` stellt `AutoPlay`, `CustomDisplaySpeed`, `FramesPerSecond` und `GifSizeMode` im klassischen Designer und im Out-of-Process-Designer für .NET 8 bis .NET 10 bereit. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).
