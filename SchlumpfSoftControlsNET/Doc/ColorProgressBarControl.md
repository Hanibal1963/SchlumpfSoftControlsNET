# ColorProgressBar Control

Ein benutzerdefiniertes Windows Forms-Steuerelement zur Anzeige eines farbigen Fortschrittsbalkens mit optionalem Rahmen und Glanzeffekt.

Die Idee hinter dem `ColorProgressBarControl` ist es, einen Fortschrittsbalken zu erstellen, der in optisch anpassbar ist.

Der Standard-Fortschrittsbalken in Windows ist ein einfacher Balken, der den Fortschritt in Form einer Füllung anzeigt.

Der `ColorProgressBarControl` hingegen kann in verschiedenen Farben und Stilen angezeigt werden.

Als Anregung diente der Artikel [A Better ProgressBar - Using Panels!](https://www.codeproject.com/Articles/31903/A-Better-ProgressBar-Using-Panels) von Saul Johnson.

Da die Downloads auf der Seite nicht mehr zu funktionieren scheinen und die Beschreibung nur Ausschnitte aus dem Original C# Code enthält und ich wenig Ahnung von C# habe, habe ich das Control in VB NET umgesetzt.

## Eigenschaften

- `Value`: Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest (Bereich: `0` bis `ProgressMaximumValue`).
- `ProgressMaximumValue`: Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest.
- `BarColor`: Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest.
- `EmptyColor`: Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest.
- `BorderColor`: Gibt die Farbe des Rahmens zurück oder legt diese fest.
- `ShowBorder`: Legt fest, ob ein Rahmen um die Fortschrittsanzeige angezeigt wird.
- `IsGlossy`: Legt fest, ob ein Glanzeffekt auf der Fortschrittsleiste angezeigt wird.
- `BackColor` (überschrieben, ausgeblendet): Hintergrundfarbe ist intern als Rahmenfarbe vorgesehen.
- `BackgroundImage` (überschrieben, ausgeblendet): Hintergrundgrafik wird vom Steuerelement nicht unterstützt.
- `BackgroundImageLayout` (überschrieben, ausgeblendet): Hintergrundgrafik-Layout wird vom Steuerelement nicht unterstützt.
- `BorderStyle` (ausgeblendet): Rahmen wird über `ShowBorder` und `BorderColor` gesteuert.
- `ForeColor` (überschrieben, ausgeblendet): Vordergrundfarbe wird vom Steuerelement nicht verwendet.
- `Padding` (überladen, ausgeblendet): Inneres Padding wird intern zur Rahmendarstellung verwaltet.

## Methoden

- `New()`: Initialisiert das Steuerelement, aktiviert flimmerfreies Zeichnen und setzt die Standarddarstellung.

## Ereignisse

- `Click`: Klicks auf die inneren Panels werden an das Steuerelement weitergeleitet, damit ein einheitliches Click-Ereignis entsteht.

