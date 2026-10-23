# ExplorerTreeViewControl

Eine wiederverwendbare WinForms-Komponente zur Anzeige und Navigation der Windows Verzeichnisstruktur (ähnlich dem linken Bereich des Windows Explorers).

Unterstützt Laufwerke, spezielle Benutzerordner (Desktop, Dokumente, Downloads, Musik, Bilder, Videos) sowie die rekursive Navigation durch Unterordner.

Änderungen am Dateisystem (neue / gelöschte / umbenannte Ordner, Laufwerks-Hotplug) werden dynamisch erkannt.

## Eigenschaften

- `SelectedPath` (nur lesen): Gibt den aktuell ausgewählten Pfad zurück.
- `LineColor`: Gibt die Farbe der Linien zwischen den Knoten zurück oder legt diese fest.
- `ShowLines`: Gibt an, ob Linien zwischen den Knoten angezeigt werden.
- `ShowPlusMinus`: Legt fest, ob die Plus- und Minuszeichen zum Anzeigen von Unterknoten angezeigt werden.
- `ShowRootLines`: Gibt an, ob Linien zwischen den Stammknoten angezeigt werden.
- `Indent`: Ruft den Abstand für das Einrücken der einzelnen Ebenen von untergeordneten Strukturknoten ab oder legt diesen fest.
- `ItemHeight`: Ruft die Höhe des jeweiligen Strukturknotens im Strukturansicht-Steuerelement ab oder legt diese fest.
- `BackColor`: Legt die Hintergrundfarbe für das Steuerelement fest oder gibt diese zurück.
- `ForeColor`: Legt die Vordergrundfarbe für das Anzeigen von Text fest oder gibt diese zurück.
- `Font`: Legt die Schriftart für den Text im Steuerelement fest oder gibt diese zurück.

## Methoden

- `New()`: Initialisiert das Steuerelement, lädt die erforderlichen Bilder und setzt den Wurzelknoten des TreeViews.
- `ExpandPath(path As String) As Boolean`: Öffnet und selektiert den Knoten zum angegebenen Verzeichnispfad (auch bei noch nicht geladenen Unterknoten) und liefert `True` bei Erfolg, sonst `False`.

## Ereignisse

- `SelectedPathChanged`: Wird ausgelöst, wenn sich der ausgewählte Pfad geändert hat.
