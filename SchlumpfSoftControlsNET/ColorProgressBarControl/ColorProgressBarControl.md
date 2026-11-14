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

## 3. Eigenschaften

- `Value`: Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest. Eingaben werden auf den Bereich `0` bis `ProgressMaximumValue` begrenzt. Bleibt der begrenzte Wert unverändert, wird keine Invalidierung ausgelöst.

- `ProgressMaximumValue`: Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest. Eingaben kleiner als `1` werden auf `1` begrenzt; ein höherer Fortschrittswert wird auf das neue Maximum reduziert. Bleibt das begrenzte Maximum unverändert, wird keine Invalidierung ausgelöst.

- `BarColor`: Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest.

- `EmptyColor`: Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest.

- `BorderColor`: Gibt die Farbe des Rahmens zurück oder legt diese fest.

- `ShowBorder`: Legt fest, ob ein Rahmen um die Fortschrittsanzeige angezeigt wird.

- `IsGlossy`: Legt fest, ob ein Glanzeffekt über dem gefüllten und dem leeren Fortschrittsbereich angezeigt wird.

- `BackColor` (überschrieben, ausgeblendet): Hintergrundfarbe ist intern als Rahmenfarbe vorgesehen.

- `BackgroundImage` (überschrieben, ausgeblendet): Hintergrundgrafik wird vom Steuerelement nicht unterstützt.

- `BackgroundImageLayout` (überschrieben, ausgeblendet): Hintergrundgrafik-Layout wird vom Steuerelement nicht unterstützt.

- `BorderStyle` (ausgeblendet): Rahmen wird über `ShowBorder` und `BorderColor` gesteuert.

- `ForeColor` (überschrieben, ausgeblendet): Vordergrundfarbe wird vom Steuerelement nicht verwendet.

- `Padding` (überladen, ausgeblendet): Inneres Padding wird intern zur Rahmendarstellung verwaltet.

## 4. Funktionen

- Keine zusätzlichen öffentlichen Funktionen dokumentiert.

## 5. Hilfstypen/Strukturen

- Keine zusätzlichen öffentlichen Hilfstypen oder Strukturen dokumentiert.

## 6. Hinweise

- Werte außerhalb des gültigen Bereichs werden ohne Ausnahme auf die jeweiligen Grenzen begrenzt.

- Ein Neuzeichnen kann auch bei unverändertem Fortschritt weiterhin mit `Invalidate()` oder `Refresh()` angefordert werden. Das `Paint`-Ereignis wird nach dem Zeichnen ausgelöst.

- Die Zeichenrechtecke werden zwischengespeichert und bei Änderungen ihrer Geometrie- oder Fortschrittsdaten aktualisiert. Der Rahmen entsteht weiterhin durch Hintergrundfarbe und Padding.

- Die Darstellung erfolgt ohne interne Panels. Anwendungen, die über `Controls` auf diese Panels zugegriffen haben, müssen entsprechend angepasst werden.

## [7. Optimierungsvorschläge](ColorProgressBarOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](ColorProgressBarErweiterungsvorschläge.md)