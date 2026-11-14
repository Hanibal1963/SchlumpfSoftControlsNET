# Optimierungsvorschläge für AniGifControl

## 1. Zielbild

Stabilere GIF-Wiedergabe mit geringerer CPU-Last und sauberer Ressourcenfreigabe.

## 2. Rendering-Pipeline entkoppeln

**Status:** Umgesetzt.

**Umsetzung:**

- Frame-Berechnung und Zeichenanforderung trennen.

- Nur dann `Invalidate()` auslösen, wenn sich der Frame tatsächlich geändert hat.

- Bei `Visible = False` bzw. `Parent.Visible = False` Updates pausieren.

**Nutzen:** Weniger unnötige Repaints, bessere Laufzeit auf schwachen Systemen.

## 3. Ressourcenlebenszyklus härten

**Status:** Umgesetzt.

**Umsetzung:**

- Bei Austausch von `Gif` altes Bild/Timer sicher freigeben.

- `Dispose(Boolean)` vollständig implementieren (Timer, Handles, Event-Abos).

- Guard gegen mehrfaches `StartAnimation()`/`StopAnimation()` einbauen.

**Nutzen:** Verhindert Speicherlecks und Zombie-Timer.

## 4. Frame-Timing präzisieren

**Status:** Umgesetzt.

**Umsetzung:**

- GIF-Frame-Delays aus Metadaten konsistent normalisieren.

- Mindest-/Maximalintervall definieren, um Ausreißer abzufangen.

- Optionales „Adaptive Timing“ für UI-Thread-Last vorsehen.

**Nutzen:** Ruhigere Wiedergabe und konsistente Darstellung zwischen GIF-Dateien.

## 5. Qualitätsoptionen beim Skalieren

**Status:** Umgesetzt.

**Umsetzung:**

- Für die `GifSizeMode`-Einstellungen `Zoom` und `Fill` den Interpolationsmodus über `ScalingQualityPreset` festlegen.

- Pixel-Art und Foto-Inhalt über die Presets `PixelArt` (`NearestNeighbor`) und `Photo` (`HighQualityBicubic`) steuern.

**Nutzen:** Bessere Bildqualität bei Zoom/Skalierung.

## 6. Diagnostik für Betrieb hinzufügen

**Status:** Umgesetzt.

**Umsetzung:**

- `DiagnosticsEnabled` aktiviert die Erfassung optional; `Diagnostics` liefert eine unveränderliche Momentaufnahme und `ResetDiagnostics()` beginnt eine neue Messung.

- Timer-Framegenerationen, aktive Wiedergabezeit und GIF-Zeichenaufrufe werden getrennt erfasst. Wiederholte Repaints, nicht zeichnungsfähige Zustände und absichtliche Framewechsel verfälschen die Framezähler nicht.

- Tests prüfen Messwerte, Clips, beide Geschwindigkeitsarten, adaptives Timing, Zustandswechsel, verschachtelte und umgehängte Eltern, wiedereintretende Handler, Ressourcenfreigabe, unveränderte Darstellung und einen echten WinForms-Timerlauf.

**Nutzen:** Erleichtert Fehlersuche bei Performanceproblemen.

## 7. GIF-Zuweisungen nach der Ressourcenfreigabe absichern

**Status:** Umgesetzt.

**Ausgangslage:** `SetGifImage()` erstellte und speicherte auch nach `Dispose()` eine Bitmap-Kopie. `DisposeAnimationResources()` gab wegen des gesetzten Dispose-Zustands anschließend keine weitere Bitmap frei.

**Umsetzung:**

- `SetGifImage()` prüft den Dispose-Zustand vor der Auswahl der Bildquelle und dem Klonen. Nach Beginn der Ressourcenfreigabe lösen sowohl `Gif = Nothing` als auch Bildzuweisungen eine `ObjectDisposedException` aus.

- Abgelehnte Zuweisungen ändern weder das Bild noch den Timer und lösen keine Ereignisse aus. Das übergebene Bild wird dabei nicht freigegeben oder anderweitig verwendet.

- Tests sichern Zuweisungen während und nach der Ressourcenfreigabe, die Wiederholung solcher Zuweisungen sowie den unveränderten Umgang mit gültigen Bildern ab.

**Nutzen:** Verhindert, dass ein bereits freigegebenes Control erneut eine eigene Bitmap ohne explizite spätere Freigabe hält.

## 8. Skalierungsgeometrie an die Clientfläche anpassen

**Status:** Umgesetzt.

**Ausgangslage:** `GetRectStartSize()` in [FunctionDefinitions.vb](FunctionDefinitions.vb) berücksichtigte für `Zoom` und `Fill` nur die Orientierung des Bildes, nicht das Seitenverhältnis der Clientfläche. Beispielsweise wurde ein quadratisches Bild bei einem Control von 400 × 100 und `ZoomFactor = 100` auf 400 × 400 skaliert und dadurch abgeschnitten. Bei 100 × 400 und `Fill` blieb es dagegen 100 × 100 groß und füllte die Fläche nicht aus.

**Umsetzung:**

- Für `Zoom` wird der kleinere Faktor aus Clientbreite geteilt durch Bildbreite und Clienthöhe geteilt durch Bildhöhe mit `ZoomFactor / 100` multipliziert; `Fill` verwendet den größeren Faktor. Die Berechnung berücksichtigt Rundungen auch bei periodischen Faktoren.

- Die Bildposition wird anhand der Clientfläche bestimmt. Leere Clientflächen werden nicht gezeichnet; bei Größenänderungen wird die gesamte Clientfläche neu gezeichnet.

- Tests decken Hoch-, Quer- und quadratische Formate, unterschiedliche Zoomfaktoren, Rundungen, Rahmen, Transparenz, leere Flächen, Größenänderungen und beide Qualitätsoptionen ab. Die XML-Kommentare beschreiben die geänderte Darstellung.

**Nutzen:** `Zoom` passt das Bild zuverlässig in die verfügbare Fläche ein; `Fill` deckt sie vollständig ab.

## 9. Unnötige Timer-Neustarts vermeiden

**Status:** Umgesetzt.

**Ausgangslage:** `UpdateTimerState()` stoppte die Wiedergabe zunächst immer, auch wenn sie nach einer Einstellungsänderung weiterlaufen sollte. Zudem löste `FramesPerSecond` selbst bei unverändertem Wert die Aktualisierung aus. Bei laufender Animation entstanden dadurch zusätzliche Stopp- und Startereignisse.

**Umsetzung:**

- Der begrenzte FPS-Wert wird mit dem bisherigen Wert verglichen; gleichwertige Zuweisungen beenden die Aktualisierung vor `FramesPerSecondChanged`.

- `UpdateTimerState()` unterscheidet Zustandswechsel von Konfigurationsänderungen. Ein geändertes Sollintervall wird bei Bedarf ohne explizites Stoppen der Wiedergabe übernommen; eine bestehende adaptive Korrektur bleibt bei unverändertem Sollintervall erhalten.

- Tests sichern FPS- und Intervallgrenzen, Metadaten-Fallbacks, beide Geschwindigkeitsarten, adaptives Timing, Sichtbarkeit, Pause, `NoAnimation`, wiedereintretende Ereignishandler und reale Timer-Ticks ab. Gleichwertige FPS-Zuweisungen lösen auch keine erneute `NoAnimation`-Prüfung aus.

**Nutzen:** Weniger Timer-Neustarts, Ereignisse und Unterbrechungen bei unverändertem Wiedergabezustand.

## 10. Frame-Delay-Metadaten wiederverwenden

**Status:** Umgesetzt.

**Ausgangslage:** `RefreshFrameDelayCache()` las bei animierten Bildern die GIF-Metadaten erneut und erstellte ein neues Array, wenn `FramesPerSecond`, `MinimumFrameIntervalMs` oder `MaximumFrameIntervalMs` geändert wurde.

**Umsetzung:**

- Unnormalisierte, in Millisekunden umgerechnete Delay-Rohwerte werden pro Bitmap gespeichert. Bei FPS-Änderungen werden bei unveränderten Metadaten nur benötigte Fallbacks angepasst; geänderte Intervallgrenzen werden erneut auf die Rohwerte angewendet. Die normalisierten und unnormalisierten Arrays werden bei unveränderter Frameanzahl wiederverwendet.

- Nach der Rückgabe der internen Bitmap über `Gif` werden die Metadaten bei jeder tatsächlichen FPS- oder Grenzänderung erneut geprüft, damit auch spätere Änderungen über eine gehaltene Referenz erkannt werden. Die Markierung wird beim Bildwechsel zurückgesetzt; bei statischen Bildern und `Dispose()` werden die Caches geleert.

- Tests sichern fehlende, unvollständige und ungültige Metadaten, Überläufe, Fallbacks, Bildwechsel, externe Änderungen und beide Geschwindigkeitsarten ab. Der Rohdaten-Cache benötigt zusätzlich ungefähr 4 Byte pro Frame zuzüglich Array-Overhead; nach Herausgabe der Bitmap entstehen weiterhin temporäre Allokationen für Metadatenzugriffe.

**Nutzen:** Weniger Metadatenzugriffe und Array-Neuanlagen bei Einstellungsänderungen, ohne die dokumentierte Behandlung extern veränderter Metadaten zu ändern.

**Messung:** Bei einem lokalen Debug-Lauf unter .NET Framework 4.8.1 mit je 9.000 FPS- und Grenzänderungen nach dem Aufwärmen sanken die verwalteten Thread-Allokationen ohne vorherigen `Gif`-Getter bei 3 Frames von 1.728.000 auf 0 Byte und bei 169 Frames von 13.680.000 auf 0 Byte. Nach Herausgabe der Bitmap sanken sie auf 1.368.000 beziehungsweise 7.344.000 Byte. Die Wiedergabe war gestoppt; die Werte sind keine Messung der Timerleistung.

## 11. Wiederholte Berechnungen pro Timer-Tick reduzieren

**Status:** Umgesetzt.

**Ausgangslage:** `Timer_Tick()` las und berechnete die verstrichene Zeit auch bei deaktiviertem `AdaptiveTiming`. Im benutzerdefinierten Geschwindigkeitsmodus wurde das FPS-Intervall bei jedem Framewechsel erneut berechnet.

**Umsetzung:**

- Zeitstempel und Zeitdifferenz werden nur für das adaptive Timing erfasst. Beim Aktivieren, Starten, Fortsetzen und bei einer Frameauswahl während laufender Wiedergabe wird der Zeitstempel weiterhin initialisiert; beim Deaktivieren wird er zurückgesetzt, ohne das aktuelle Timerintervall sofort zu ändern. Die Diagnostik bleibt unabhängig davon.

- Das begrenzte FPS-Intervall wird bei der Initialisierung sowie bei Änderungen von FPS oder Intervallgrenzen vorberechnet und als benutzerdefiniertes Intervall beziehungsweise Metadaten-Fallback wiederverwendet. Es wird vor dem erneuten Metadatenzugriff aktualisiert, damit es auch nach einer dort ausgelösten Ausnahme zu den geänderten Einstellungen passt. Rundung, Fallbacks und Grenzen bleiben unverändert.

- Tests sichern beide Geschwindigkeitsarten, Rundung und Grenzwechsel, Metadatenfehler, das Ein- und Ausschalten von `AdaptiveTiming`, Pause, Sichtbarkeit, Frameauswahl und die unabhängige Diagnostik ab.

**Nutzen:** Weniger wiederholte Berechnungen im häufig aufgerufenen Timerpfad bei unverändertem Wiedergabeverhalten.

**Messung:** In einem lokalen Debug-Lauf unter .NET Framework 4.8.1 benötigten jeweils 2.000.000 isolierte FPS-Intervallabfragen bei 25 FPS zuvor 293–874 ms und danach 11–14 ms; bei 16 FPS waren es zuvor 1.362–1.404 ms und danach 11–12 ms. Bei jeweils 12.000 manuell aufgerufenen Ticks ohne Control-Handle schwankten die Zeiten über beide Geschwindigkeits- und Timingarten zuvor zwischen 3,2 und 9,9 Sekunden und danach zwischen 3,0 und 9,9 Sekunden. Daraus lässt sich kein verlässlicher Gesamtgeschwindigkeitsgewinn ableiten; Bildausgabe und reale Timerintervalle wurden dabei nicht gemessen.

## 12. Sichtbarkeitsprüfung vereinfachen

**Status:** Umgesetzt.

**Ausgangslage:** `IsControlTreeVisible()` prüfte nach `Visible` nochmals alle übergeordneten Controls. Bei aktivierter Diagnostik wurde die Sichtbarkeit im Timerpfad unmittelbar danach erneut vor dem Framewechsel geprüft.

**Umsetzung:**

- `IsControlTreeVisible()` verwendet den Getter von [Control.Visible](https://learn.microsoft.com/dotnet/api/system.windows.forms.control.visible), der die Sichtbarkeit der Elternkette bereits berücksichtigt.

- Bei aktivierter Diagnostik und vorhandenem Bild teilen sich Diagnostik und Frameprüfung dieselbe Sichtbarkeitsmomentaufnahme; zwischen diesen Prüfungen werden keine Ereignishandler ausgelöst. Andere Diagnoseaufrufe und `SeekFrame()` prüfen die Sichtbarkeit weiterhin eigenständig.

- Vor `Invalidate()` bleibt die erneute Sichtbarkeitsprüfung erhalten: Ein abgeleiteter Timer kann bei einer Intervalländerung über seinen überschreibbaren `Enabled`-Getter wiedereintreten und die Sichtbarkeit ändern. Auch die Schutzprüfungen für Dispose-Zustand, Handle und Clientgröße bleiben bestehen.

- Tests sichern verschachtelte und umgehängte Eltern, beide Geschwindigkeitsarten, adaptives Timing, Diagnostik, Pause, fehlendes Handle, leere Clientfläche, Timer-Rückeintritte und einen echten WinForms-Timerlauf ab. Das bestehende Verhalten bleibt erhalten: Bei ausgeblendeten Eltern kann der Timer weiterlaufen, ohne Frames umzuschalten oder Frame-Neuzeichnungen anzufordern.

**Nutzen:** Weniger redundante Abfragen pro Frame bei unverändertem Verhalten für ausgeblendete Controls.

**Messung:** In zwei lokalen Release-Läufen unter .NET Framework 4.8.1 mit jeweils drei Messungen und je 500.000 isolierten Sichtbarkeitsabfragen lagen die Zeiten ohne Eltern zuvor bei 14,8–53,6 ms und danach bei 4,8–10,8 ms; bei zehn Eltern waren es zuvor 860,0–916,7 ms und danach 22,7–71,7 ms. Je 800 manuell aufgerufene Ticks mit Handle, beiden Geschwindigkeitsarten, aktivierter oder deaktivierter Diagnostik und null beziehungsweise fünf Eltern schwankten zuvor zwischen 192 und 708 ms und danach zwischen 194 und 786 ms. Daraus lässt sich kein verlässlicher Gesamtgeschwindigkeitsgewinn ableiten; tatsächliche Timerintervalle und Bildausgabe wurden nicht gemessen.

## 13. Aktualisierungsbereich bei Framewechseln begrenzen

**Status:** Offen.

**Ausgangslage:** `InvalidateIfVisible()` fordert bei einem Framewechsel die Neuzeichnung der gesamten Clientfläche an, auch wenn das Bild nur einen Teil davon belegt.

**Umsetzung:**

- Bei unveränderter Bildgeometrie prüfen, ob nur der nicht leere Schnitt des Bildzielrechtecks mit der Clientfläche über `Invalidate(Rectangle)` aktualisiert werden kann.

- Bei Bild-, Größen- oder Layoutänderungen weiterhin ausreichend große Bereiche aktualisieren; alte und neue Bildpositionen, transparente Frames, Interpolationsränder und externe `Paint`-Handler berücksichtigen.

- Darstellung und Hintergrund für alle Größenmodi mit Tests vergleichen und den Nutzen insbesondere bei kleinen Bildern in großen Controls messen.

**Nutzen:** Kann die zu aktualisierende Fläche und damit den Zeichenaufwand verringern, ohne die bereits umgesetzte Bedingung für Framewechsel zu verändern.

## 14. Ungenutzte ImageAnimator-Pfade entfernen

**Status:** Umgesetzt.

**Ausgangslage:** Beide Geschwindigkeitsarten verwendeten den Timer. Für den privaten `OnNextFrame`-Handler war kein `ImageAnimator.Animate()` registriert; `StopAnimationCore()` rief dennoch `ImageAnimator.StopAnimate()` auf.

**Umsetzung:**

- Der ungenutzte Handler, das zugehörige Delegate, der überflüssige `StopAnimate()`-Aufruf und die ungenutzte Konstante `DefaultFrameDelayMs` wurden entfernt. Die FPS-basierte Fallback-Berechnung bleibt unverändert.

- Kommentare und XML-Dokumentation beschreiben die Timer-basierte Wiedergabe. Tests sichern Start, Framewechsel, Stopp, Pause, Moduswechsel, Sichtbarkeit, Bildwechsel und Ressourcenfreigabe in beiden Geschwindigkeitsarten ab.

**Nutzen:** Vereinfacht die Wartung und vermeidet eine unnötige Abmeldung eines nicht registrierten Wiedergabemechanismus.

## 15. Empfohlene Reihenfolge

1. Aktualisierungsbereich bei Framewechseln begrenzen (7.13).
