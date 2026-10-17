# TransparentLabel Control

Ein .NET Windows Forms Steuerelement (`TransparentLabel`) zum Anzeigen von Text mit durchscheinendem (quasi transparentem) Hintergrund über anderen Steuerelementen oder Grafiken.

Es eignet sich besonders für Oberflächen, bei denen der Hintergrund durchscheinen soll, z. B. bei überlagerten Texten auf Bildern oder farbigen Flächen.

Die Idee hinter diesem Projekt ist, z.Bsp. einen Text teilweise über ein Bild zu legen ohne sich großartig Gedanken über Grafikroutinen zu machen.

Mit diesem Control ist das in wenigen Zeilen Code erledigt bzw. im Designer zusammengeklickt.

## Eigenschaften

- `BackColor` (Override): Ausgeblendet, da für dieses Control nicht relevant; es wird intern immer `Color.Transparent` erzwungen.
- `BackgroundImage` (Override): Ausgeblendet, da für dieses Control nicht relevant.
- `BackgroundImageLayout` (Override): Ausgeblendet, da für dieses Control nicht relevant.
- `FlatStyle` (Shadows): Ausgeblendet, da für dieses Control nicht relevant.
- `CreateParams` (Protected Override, ReadOnly): Liefert angepasste Erstellungsparameter und aktiviert `WS_EX_TRANSPARENT`, damit der Hintergrund durchscheinen kann.

## Methoden

- `New()`: Initialisiert eine neue Instanz der Klasse `TransparentLabel`.

## Ereignisse

- Keine eigenen Ereignisse definiert.
