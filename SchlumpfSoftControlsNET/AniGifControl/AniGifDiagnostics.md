# AniGifDiagnostics

`AniGifDiagnostics` ist eine unveränderliche Momentaufnahme der optionalen Diagnostik eines `AniGif`-Controls. Sie beschreibt dessen Timer-Framegenerationen und GIF-Zeichenaufrufe, nicht die bestätigte Bildschirmausgabe.

## 1. Konstruktoren

Es gibt keinen öffentlichen Konstruktor. Eine Momentaufnahme wird über die Eigenschaft `AniGif.Diagnostics` erstellt.

## 2. Ereignisse

Die Klasse stellt keine eigenen Ereignisse bereit.

## 3. Eigenschaften

Alle Eigenschaften sind schreibgeschützt und beziehen sich auf den Zeitpunkt des Abrufs.

- `IsEnabled As Boolean`: Gibt an, ob die Diagnostik bei Erstellung der Momentaufnahme aktiviert war.
- `ActiveDuration As TimeSpan`: Kumulierte Zeit, in der die Animation lief und das Control sichtbar, mit Handle und nicht leerer Clientfläche zeichnungsfähig war.
- `ActualFps As Double?`: Mittlere Anzahl unterschiedlicher gezeichneter Timer-Framegenerationen pro aktiver Sekunde. Ohne messbare aktive Zeit ist der Wert `Nothing`; bei aktiver Zeit ohne gezeichneten Timer-Frame ist er `0`.
- `RenderedFrames As Int64`: Anzahl unterschiedlicher Timer-Framegenerationen mit mindestens einem erfolgreichen GIF-Zeichenaufruf, dessen Zielbereich den wirksamen Clip schneidet. Wiederholte Repaints und manuelle Frameauswahl zählen nicht zusätzlich.
- `DroppedFrames As Int64`: Anzahl anstehender Timer-Framegenerationen, die vor einem passenden Zeichenaufruf durch einen späteren Timer-Frame ersetzt wurden. Absichtliche Abbrüche und verspätete Timer-Ticks zählen nicht als Drops.
- `RenderOperations As Int64`: Anzahl erfolgreicher GIF-Zeichenaufrufe, einschließlich wiederholter Repaints und statischer Bilder.
- `AverageRenderTimeMs As Double?`: Durchschnittliche Dauer der erfolgreichen GIF-Zeichenaufrufe in Millisekunden; ohne Zeichenaufruf `Nothing`.
- `MaxRenderTimeMs As Double?`: Längste Dauer der erfolgreichen GIF-Zeichenaufrufe in Millisekunden; ohne Zeichenaufruf `Nothing`.

## 4. Funktionen

Die Klasse stellt keine eigenen öffentlichen Methoden bereit.

## 5. Hilfstypen/Strukturen

Die Klasse definiert keine eigenen öffentlichen Hilfstypen oder Strukturen.

## 6. Hinweise

- Die Diagnostik wird mit `AniGif.DiagnosticsEnabled` aktiviert. Eine erneute Aktivierung und `AniGif.ResetDiagnostics()` beginnen eine neue Messung; Deaktivieren und `Dispose()` frieren die bisherigen Werte ein.
- Eine Pause, Unsichtbarkeit innerhalb der Elternkette, ein fehlendes Handle oder eine leere Clientfläche unterbrechen die aktive Zeitmessung. Ausstehende Timer-Frames werden dabei verworfen, ohne als Drops zu zählen; dasselbe gilt für `SeekFrame()`, Bildwechsel und Reset.
- Sichtbarkeitswechsel werden auf dem UI-Thread erfasst. Zuvor ausgeführte, blockierende Ereignishandler der Eltern können den gemessenen Zeitpunkt des Wechsels verschieben.
- Die Renderzeit umfasst die Geometrieberechnung und `DrawImage`, nicht `MyBase.OnPaint`, externe Paint-Handler oder die Ausgabe durch das Betriebssystem. Ein erfolgreicher Zeichenaufruf wird auch dann als Renderoperation gezählt, wenn sein Zielbereich den Clip nicht schneidet.
- Die Messung verwendet `System.Diagnostics.Stopwatch`. Die Frame- und Operationszähler verwenden `Int64`; die Zeitangaben und die Bildrate verwenden `Double`.
- Das Control und der Abruf der Momentaufnahme müssen auf dessen UI-Thread verwendet werden. Bereits abgerufene Momentaufnahmen ändern sich nicht, wenn weitere Messwerte erfasst werden.

## 7. Optimierungsvorschläge

Keine.

## 8. Erweiterungsvorschläge

Keine.