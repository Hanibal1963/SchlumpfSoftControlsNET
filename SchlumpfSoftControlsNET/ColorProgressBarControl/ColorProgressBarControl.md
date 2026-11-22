# ColorProgressBar Control

Ein benutzerdefiniertes Windows Forms-Steuerelement zur Anzeige eines farbigen Fortschrittsbalkens mit optionalem Rahmen und Glanzeffekt.

Die Idee hinter dem `ColorProgressBarControl` ist es, einen Fortschrittsbalken zu erstellen, der optisch anpassbar ist.

Der Standard-Fortschrittsbalken in Windows ist ein einfacher Balken, der den Fortschritt in Form einer Füllung anzeigt.

Der `ColorProgressBarControl` hingegen kann in verschiedenen Farben und Stilen angezeigt werden.

Als Anregung diente der Artikel [A Better ProgressBar - Using Panels!](https://www.codeproject.com/Articles/31903/A-Better-ProgressBar-Using-Panels) von Saul Johnson.

Da die Downloads auf der Seite nicht mehr zu funktionieren scheinen und die Beschreibung nur Ausschnitte aus dem Original-C#-Code enthält, wurde das Control in VB NET umgesetzt.

> &#x26A0; **Hinweis**
>
> Die Seite mit dem Artikel scheint mittlerweile nicht mehr zu existieren.
>
> Eine im Web-Archiv gespeicherte Version der Seite ist verfügbar.
>
> [Web Archiv Kopie](https://web.archive.org/web/20250319060356/<https://www.codeproject.com/Articles/31903/A-Better-ProgressBar-Using-Panels>)

## 1. Konstruktoren

- `New()`: Initialisiert das Steuerelement, aktiviert flimmerfreies Zeichnen und setzt die Standarddarstellung.

## 2. Ereignisse

- `Click`: Klicks auf das Steuerelement lösen direkt das Click-Ereignis aus.

- `Paint`: Registrierte Ereignishandler werden nach der Darstellung des Steuerelements aufgerufen und können diese ergänzen.

- `SystemColorsChanged`: Bei Systemfarben in `BarColor`, `EmptyColor` oder einem Schwellenwert werden betroffene Brushes vor dem Aufruf der Ereignishandler verworfen und eine Neuzeichnung angefordert.

## 3. Eigenschaften

- `Value` (`Int32`, Standardwert `1`): Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest. Eingaben werden auf den Bereich `0` bis `ProgressMaximumValue` begrenzt. Bleibt der begrenzte Wert unverändert, wird keine Invalidierung ausgelöst.

- `ProgressMaximumValue` (`Int32`, Standardwert `10`): Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest. Eingaben kleiner als `1` werden auf `1` begrenzt; ein höherer Fortschrittswert wird auf das neue Maximum reduziert. Bleibt das begrenzte Maximum unverändert, wird keine Invalidierung ausgelöst.

- `BarColor`: Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest. Transparente Farben, `Color.Empty` und Systemfarben werden unterstützt.

- `Thresholds` (`ColorProgressBarThresholdCollection`, schreibgeschützt): Programmatisch bearbeitbare, geordnete Sammlung von Farbbereichen; standardmäßig leer. Einfügen oder Ersetzen mit `Nothing` löst `ArgumentNullException` aus. Die Sammlung wird nicht im WinForms-Designer serialisiert.

- `EmptyColor`: Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest. Transparente Farben, `Color.Empty` und Systemfarben werden unterstützt.

- `BorderColor`: Gibt die Farbe des Rahmens zurück oder legt diese fest.

- `ShowBorder`: Legt fest, ob ein Rahmen um die Fortschrittsanzeige angezeigt wird.

- `IsGlossy`: Legt fest, ob ein Glanzeffekt über dem gefüllten und dem leeren Fortschrittsbereich angezeigt wird.

- `ProgressStyle` (`ColorProgressBarStyle`, Standardwert `Solid`): Wählt eine einfarbige, verlaufende, gestreifte oder animierte Darstellung. Nicht definierte Werte lösen `InvalidEnumArgumentException` aus.

- `AnimateValueChanges` (`Boolean`, Standardwert `False`): Animiert Wertänderungen in den wertabhängigen Darstellungen; das Abschalten übernimmt den Zielwert sofort.

- `ShowSegments` (`Boolean`, Standardwert `False`): Aktiviert die segmentierte Darstellung.

- `SegmentCount` (`Int32`, Standardwert `10`): Gewünschte Segmentanzahl von `1` bis `1000`. Andere Werte lösen `ArgumentOutOfRangeException` aus.

- `SegmentSpacing` (`Int32`, Standardwert `2`): Gewünschter Abstand zwischen Segmenten in Pixeln von `0` bis `1000`. Andere Werte lösen `ArgumentOutOfRangeException` aus.

- `ShowText` (`Boolean`, Standardwert `False`): Zeigt optional eine mittige Fortschrittsbeschriftung an.

- `TextFormat` (`ColorProgressBarTextFormat`, Standardwert `Percentage`): Wählt Prozent (`Percentage`) oder aktuellen Wert und Maximum (`Value`). Nicht definierte Werte lösen `InvalidEnumArgumentException` aus.

- `AccessibleName` (geerbt): Ein expliziter Name überschreibt die zugeordnete Beschriftung und den lokalisierten Standardnamen. Eine leere Zeichenfolge bleibt ein explizit leerer Name; `Nothing` aktiviert die Standardermittlung.

- `AccessibleDescription` (geerbt): Eine explizite Beschreibung überschreibt die lokalisierte Standardbeschreibung. `Nothing` aktiviert die Standardbeschreibung.

- `AccessibleRole` (geerbt): Standardmäßig `ProgressBar`; eine explizit gesetzte andere Rolle bleibt erhalten. `Default` verwendet ebenfalls die Fortschrittsrolle.

- `AccessibilityObject` (geerbt, schreibgeschützt): Stellt Name, Beschreibung, Rolle und den aktuellen Fortschritt für MSAA bereit. Sein `Value` ist ein lokalisierter Prozenttext und schreibgeschützt; ein Schreibversuch löst `InvalidOperationException` aus.

- `BackColor` (überschrieben, ausgeblendet): Hintergrundfarbe ist intern als Rahmenfarbe vorgesehen.

- `BackgroundImage` (überschrieben, ausgeblendet): Hintergrundgrafik wird vom Steuerelement nicht unterstützt.

- `BackgroundImageLayout` (überschrieben, ausgeblendet): Hintergrundgrafik-Layout wird vom Steuerelement nicht unterstützt.

- `BorderStyle` (ausgeblendet): Rahmen wird über `ShowBorder` und `BorderColor` gesteuert.

- `ForeColor` (überschrieben, ausgeblendet): Vordergrundfarbe wird vom Steuerelement nicht verwendet.

- `Padding` (überladen, ausgeblendet): Inneres Padding wird intern zur Rahmendarstellung verwaltet.

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## 5. Hilfstypen/Strukturen

- `ColorProgressBarTextFormat`: Enumeration mit `Percentage` und `Value` für die sichtbare Beschriftung.

- `ColorProgressBarStyle`: Enumeration mit `Solid`, `Gradient`, `Striped` und `Marquee`.

- `ColorProgressBarThreshold(minimum, maximum, color)`: Unveränderlicher Farbbereich mit inklusiven Grenzen `Minimum`, `Maximum` und `Color`. `minimum` muss mindestens `0`, `maximum` mindestens `minimum` sein; andernfalls wird `ArgumentOutOfRangeException` ausgelöst. Transparente Farben, `Color.Empty` und Systemfarben sind zulässig.

- `ColorProgressBarThresholdCollection`: Geordnete Sammlung von `ColorProgressBarThreshold`; Hinzufügen, Ersetzen, Entfernen und Leeren aktualisieren die Anzeige. Eine Instanz wird vom Control bereitgestellt.

## 6. Hinweise

- Das Minimum für `Value` ist fest `0`; `ProgressMaximumValue` liegt zwischen `1` und `Int32.MaxValue`. Werte außerhalb des gültigen Bereichs werden ohne Ausnahme auf die jeweiligen Grenzen begrenzt.

- Das nachträgliche Erhöhen von `ProgressMaximumValue` stellt einen zuvor begrenzten Fortschrittswert nicht wieder her. Soll ein größerer Wert angezeigt werden, muss das Maximum vor oder der Wert nach der Erhöhung gesetzt werden.

- Ohne expliziten Namen wird eine zugeordnete WinForms-Beschriftung verwendet, falls vorhanden; ansonsten wird ein lokalisierter Standardname bereitgestellt. Die Standardbeschreibung wird unabhängig davon verwendet, wenn keine explizite Beschreibung gesetzt ist. Die Standardtexte richten sich nach `CurrentUICulture`: Deutsch und Englisch werden unterstützt, andere Sprachen verwenden Deutsch. Sie werden nicht als feste Eigenschaftswerte im Designer serialisiert.

- Der Accessibility-Prozentwert und die optionale sichtbare Prozentanzeige werden anhand von `CurrentCulture` formatiert und auf ganze Prozent abgerundet; `100 %` wird erst beim Maximum ausgegeben. Das Wertformat zeigt den aktuellen Wert und das Maximum als `Wert / Maximum`.

- Die Beschriftung wird auf die Innenfläche begrenzt und bei Platzmangel gekürzt. Ihre Farbe wird für den gefüllten und den leeren Bereich separat als Schwarz oder Weiß anhand der Hintergrundhelligkeit gewählt. Bei transparenten Farben wird die Hintergrundfarbe des übergeordneten Controls berücksichtigt; Hintergrundbilder und der Glanzeffekt bleiben bei der Kontrastberechnung unberücksichtigt.

- `Gradient` zeigt innerhalb der Füllung einen vertikalen Verlauf: Oben liegt die Mischung aus `BarColor` und Schwarz beziehungsweise Weiß, unten `BarColor`. `Striped` ergänzt diagonale Kontraststreifen. Die Textkontrastberechnung berücksichtigt diese Muster nicht. `Marquee` bewegt unabhängig von `Value` einen Balken durch die Innenfläche; optionaler Text und Accessibility melden weiterhin den tatsächlichen Wert. Die Animation läuft mit 50 ms Timerintervall nur zur Laufzeit, solange das Control sichtbar ist und ein Handle besitzt; nach Entsorgen wird der Timer freigegeben.

- Bei `AnimateValueChanges` interpoliert die Füllung über 300 ms mit kubischem Ease-out zum Zielwert. Weitere Wertänderungen starten beim aktuellen Zwischenstand; Text und Accessibility melden sofort den Zielwert. Änderungen des Maximums, ausgeblendete Controls, Handle-Verlust, Designer und `Marquee` übernehmen den Zielwert sofort. Der Timer läuft nur während eines sichtbaren Übergangs und wird beim Entsorgen freigegeben.

- Für Schwellenwerte gilt der erste Bereich, der den aktuell dargestellten Wert einschließlich seiner Grenzen enthält; ohne Treffer gilt `BarColor`. Überlappungen werden nach Reihenfolge der Sammlung aufgelöst. Bei Animation werden auch gebrochene Zwischenwerte geprüft; zwischen Bereichen mit Abstand gilt dort `BarColor`. Bei `Marquee` wird der tatsächliche `Value` verwendet. Füllung und Textkontrast verwenden dieselbe ausgewählte Farbe; `BarColor`, Wert und Accessibility bleiben unverändert. Bereiche oberhalb des Maximums bleiben gespeichert und werden bei einer späteren Erhöhung berücksichtigt.

  Beispiel (bei `ProgressMaximumValue = 100`):

  ```vb
  progressBar.Thresholds.Add(New ColorProgressBarThreshold(0, 49, Color.Red))
  progressBar.Thresholds.Add(New ColorProgressBarThreshold(50, 100, Color.Green))
  ```

- Segmente teilen die Innenfläche gleichmäßig auf; der Fortschritt füllt sie weiterhin kontinuierlich, sodass auch Teilsegmente möglich sind. Die Zwischenräume werden in `BackColor` gezeichnet. Füllung, Glanz und Text werden auf Segmentflächen beschränkt; `Paint`-Handler erhalten wieder die ursprüngliche Zeichenbeschränkung. Alle Darstellungsstile und animierte Übergänge sind kombinierbar. Bei Platzmangel wird zuerst die sichtbare Anzahl auf die Pixelbreite und danach der Abstand reduziert, sodass jedes sichtbare Segment mindestens ein Pixel breit bleibt. `SegmentSpacing = 0` zeichnet ohne Zwischenräume.

- UI Automation bietet die schreibgeschützten Muster `RangeValue` (Minimum `0`, Maximum `ProgressMaximumValue`, aktueller `Value`) und `Value` (Prozenttext). Bei Änderungen werden MSAA- und UI-Automation-Clients benachrichtigt; ohne Fenster-Handle wird kein Handle allein für eine Benachrichtigung erzeugt.

- Für UI Automation werden die Windows-Desktop-Framework-APIs verwendet; unter modernem .NET ist dafür zusätzlich die Frameworkreferenz `Microsoft.WindowsDesktop.App.WPF` erforderlich. Es wurden keine NuGet-Pakete ergänzt.

- Ein Neuzeichnen kann auch bei unverändertem Fortschritt weiterhin mit `Invalidate()` oder `Refresh()` angefordert werden. Das `Paint`-Ereignis wird nach dem Zeichnen ausgelöst.

- Die Zeichenrechtecke werden zwischengespeichert und bei Änderungen ihrer Geometrie- oder Fortschrittsdaten aktualisiert. Der Rahmen entsteht weiterhin durch Hintergrundfarbe und Padding.

- Die Brushes für Füllung, Leerbereich und Glanz werden bei Bedarf pro Control erzeugt und wiederverwendet. Ersetzte Brushes und die verbleibenden Brushes beim Entsorgen des Controls werden freigegeben.

- Eine Zuweisung an `BarColor` oder `EmptyColor` invalidiert auch bei unveränderter Farbe; der vorhandene Brush bleibt dabei erhalten.

- Während oder nach dem Entsorgen löst ein direkter `OnPaint`-Aufruf aus einer abgeleiteten Klasse eine `ObjectDisposedException` aus; es werden keine neuen Brushes angelegt.

- Die Darstellung erfolgt ohne interne Panels. Anwendungen, die über `Controls` auf diese Panels zugegriffen haben, müssen entsprechend angepasst werden.

## [7. Optimierungsvorschläge](ColorProgressBarOptimierungsvorschläge.md)

z.Zt. alle umgesetzt

## [8. Erweiterungsvorschläge](ColorProgressBarErweiterungsvorschläge.md)

z.Zt. alle umgesetzt