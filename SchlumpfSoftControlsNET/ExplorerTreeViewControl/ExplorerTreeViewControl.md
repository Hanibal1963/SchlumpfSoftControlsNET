# ExplorerTreeViewControl

Eine wiederverwendbare WinForms-Komponente zur Anzeige und Navigation der Windows-Verzeichnisstruktur (ähnlich dem linken Bereich des Windows-Explorers).

Unterstützt Laufwerke, spezielle Benutzerordner (Desktop, Dokumente, Downloads, Musik, Bilder, Videos) sowie die rekursive Navigation durch Unterordner.

Änderungen am Dateisystem (neue, gelöschte oder umbenannte Ordner, Laufwerks-Hotplug) werden dynamisch erkannt.

## 1. Konstruktoren

- `New()`: Initialisiert das Steuerelement, lädt die erforderlichen Bilder und setzt den Wurzelknoten des TreeViews. Spezialordner und Laufwerke werden nach Erstellung des TreeView-Handles im Hintergrund ermittelt.

---

## 2. Ereignisse

- `SelectedPathChanged`: Wird nach erfolgreicher Zugriffsprüfung auf dem UI-Thread ausgelöst. Bei interaktiver Auswahl erfolgt diese Prüfung im Hintergrund; überholte Auswahlprüfungen lösen kein Ereignis aus.

---

## 3. Eigenschaften

- `SelectedPath` (nur lesen): Gibt den zuletzt erfolgreich geprüften ausgewählten Pfad zurück. Während einer laufenden Prüfung oder bei nicht zugänglicher Auswahl bleibt der bisherige Wert erhalten.
- `LineColor`: Gibt die Farbe der Linien zwischen den Knoten zurück oder legt diese fest.
- `ShowLines`: Gibt an, ob Linien zwischen den Knoten angezeigt werden.
- `ShowPlusMinus`: Legt fest, ob die Plus- und Minuszeichen zum Anzeigen von Unterknoten angezeigt werden.
- `ShowRootLines`: Gibt an, ob Linien zwischen den Stammknoten angezeigt werden.
- `Indent`: Ruft den Abstand für das Einrücken der einzelnen Ebenen von untergeordneten Strukturknoten ab oder legt diesen fest.
- `ItemHeight`: Ruft die Höhe des jeweiligen Strukturknotens im Strukturansicht-Steuerelement ab oder legt diese fest.
- `BackColor`: Legt die Hintergrundfarbe für das Steuerelement fest oder gibt diese zurück.
- `ForeColor`: Legt die Vordergrundfarbe für das Anzeigen von Text fest oder gibt diese zurück.
- `Font`: Legt die Schriftart für den Text im Steuerelement fest oder gibt diese zurück.

---

## 4. Funktionen

- `ExpandPath(path As String) As Boolean`: Öffnet und selektiert den Knoten zum angegebenen Verzeichnispfad (auch bei noch nicht geladenen Unterknoten) und liefert `True` bei Erfolg, sonst `False`. `Nothing`, eine leere Zeichenfolge und reine Leerzeichen liefern `False`. Die Navigation und die Auswahlprüfung erfolgen synchron und können den UI-Thread blockieren.
- `ExpandPathAsync(path As String) As Task(Of Boolean)`: Navigiert über noch nicht geladene Ebenen und bestätigt die Auswahl nach einer Zugriffsprüfung im Hintergrund. Die Aufgabe liefert `True` bei erfolgreicher Auswahl und sonst `False`, unter anderem für leere, fehlende oder nicht zugängliche Pfade sowie bei Abbruch. Eine neue Navigation, eine andere Auswahl, das Zuklappen eines Knotens, die Zerstörung des Handles oder die Entsorgung bricht einen laufenden Auftrag ab. Der Aufruf muss auf dem UI-Thread bei erstelltem TreeView-Handle erfolgen; andernfalls schlägt die Aufgabe mit `InvalidOperationException` fehl. Fehler bei der Pfadverarbeitung oder unerwartete Abfragefehler werden über die Aufgabe weitergegeben.

---

## 5. Hilfstypen/Strukturen

- `SelectedPathChangedEventArgs`: Enthält die Ereignisdaten für Pfadänderungen.

---

## 6. Hinweise

- Dateisystemänderungen werden über interne FileSystemWatcher und Laufwerksüberwachung nachgeführt.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Schnelle Navigation in großen Verzeichnisstrukturen mit reaktiver UI.

---

### 7.2. Lazy-Loading weiter ausbauen

**Status:** &#x2705; Umgesetzt.

**Umsetzung:**

- Unterknoten nur bei Expand laden.

- Platzhalterknoten nutzen und nachladen.

- Metadaten (Exists/AccessDenied) vorhalten.

Verzeichnisknoten erhalten zunächst einen Platzhalter; ihre unmittelbaren Unterordner werden erst beim Aufklappen ermittelt. Der letzte Ladezustand unterscheidet unbekannte, zugängliche, fehlende, zugriffsgeschützte und nicht verfügbare Verzeichnisse. Fehlerhafte Knoten bleiben für einen erneuten Versuch aufklappbar und werden entsprechend markiert. Nach dem Zuklappen oder einer Dateisystemänderung wird erneut geladen. Weitere Hintergrund-I/O bleibt Gegenstand von 7.3.

**Nutzen:** Schneller Initialstart, weniger I/O-Spitzen.

---

### 7.3. Dateisystemzugriffe entkoppeln

**Status** &#x26A0; Teilweise umgesetzt: Die Ermittlung der Wurzeleinträge und der für neue Laufwerksknoten benötigten Metadaten, Verzeichnisauflistungen beim Aufklappen und Aktualisieren, die Einrichtung der Verzeichnisüberwachung und die Prüfung interaktiver Auswahlen erfolgen im Hintergrund. `ExpandPathAsync` ermöglicht die asynchrone Pfadnavigation; `ExpandPath` und seine Auswahlprüfung bleiben synchron. Überholte Ergebnisse werden verworfen. Nach Einrichtung der Überwachung wird erneut geladen, damit zwischenzeitliche Änderungen berücksichtigt werden; vorhandene Unterbäume bleiben bei der Aktualisierung erhalten. Der vorgelagerte `DriveWatcher` kann vor der Zustellung eines Hotplug-Ereignisses selbst synchron Laufwerksmetadaten abfragen und wurde nicht geändert. Die Prüfung mit langsamen Netzlaufwerken steht noch aus.

**Umsetzung:**

- Verzeichnis- und Laufwerksauflistungen sowie weitere möglicherweise blockierende Dateisystemabfragen beim Initialisieren, Aufklappen, Auswählen und Aktualisieren im Hintergrund ausführen. Auch das Einrichten der Verzeichnisüberwachung berücksichtigen.

- Ergebnisse je Ladeauftrag gebündelt per `BeginInvoke` auf dem UI-Thread anwenden; Baumknoten und Steuerelemente nicht im Hintergrund verändern.

- Ladezustand und Platzhalter während laufender Aufträge anzeigen; fehlgeschlagene Zugriffe weiterhin markieren und erneute Versuche ermöglichen.

- Veraltete Ergebnisse nach erneutem Laden, Zuklappen, Entfernen eines Knotens oder Entsorgen des Steuerelements verwerfen. Gleichzeitige Aufträge und Dateisystemänderungen so koordinieren, dass ältere Ergebnisse neuere Zustände nicht überschreiben.

- Pfadnavigation über noch nicht geladene Ebenen asynchron ermöglichen und Auswahl sowie `SelectedPathChanged` erst nach erfolgreicher Zugriffsprüfung aktualisieren. Die bestehende synchrone `ExpandPath`-API kompatibel erhalten und deren gegebenenfalls weiterhin blockierendes Verhalten dokumentieren; für nicht blockierende Navigation eine asynchrone API ergänzen.

- Verhalten bei langsamen oder nicht verfügbaren Laufwerken, konkurrierenden Aktualisierungen, Zuklappen und Entsorgung sowie die bestehende synchrone Pfadnavigation durch Tests überprüfen.

**Nutzen:** Die asynchronen Lade- und Navigationspfade halten die UI bei langsamen Verzeichnisabfragen reaktionsfähig. Die synchrone API und vorgelagerte `DriveWatcher`-Abfragen können weiterhin blockieren.

---

### 7.4. Caching für Icons und SpecialFolders

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Drive-/Folder-Icons zentral cachen.

- KnownFolder-Auflösung einmalig pro Session durchführen.

**Nutzen:** Reduzierte Systemaufrufe und schnelleres Rendering.

---

### 7.5. Fehlerbehandlung verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- `UnauthorizedAccessException`, `PathTooLongException`, Offline-Shares gezielt behandeln.

- Defekte Knoten visuell markieren statt Ausnahme weiterreichen.

**Nutzen:** Robuste Navigation auch in problematischen Ordnern.

---

### 7.6. Refresh-Strategie optimieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Dateiänderungen per Debounce bündeln.

- Nur betroffene Teilbäume aktualisieren.

**Nutzen:** Weniger Flackern und CPU-Last bei häufigen Änderungen.

---

### 7.7. Empfohlene Reihenfolge

1. Initialisierung, Auswahlprüfung, Überwachung und Pfadnavigation einbeziehen; asynchrone Navigation ergänzen und die synchrone API kompatibel halten.
2. Die betroffenen Abläufe und Grenzfälle testen und die API-Dokumentation aktualisieren.
3. Fehlerbehandlung für problematische Pfade und Offline-Laufwerke weiter verbessern (7.5).
4. Icons und SpecialFolders cachen (7.4).
5. Teilbaum-Refresh und Debounce optimieren (7.6).

---

## 8. geplante Erweiterungen

### 8.1. Kontextmenü-Integration

**Status:** &#x1F4A1; offen

**Vorschlag:** Optionales Shell-nahes Kontextmenü je Knoten.

**Umsetzung:** Ereignis `BuildContextMenu(node, menu)` bereitstellen.

---

### 8.2. Multi-Selection-Modus

**Status:** &#x1F4A1; offen

**Vorschlag:** Mehrfachauswahl von Ordnern.

**Umsetzung:** Interne Selektionsverwaltung plus API `SelectedPaths`.

---

### 8.3. Such- und Filterfunktion

**Status:** &#x1F4A1; offen

**Vorschlag:** Knoten per Muster/Präfix filtern.

**Umsetzung:** In-Memory-Filter mit Highlighting und optionalem „nur Treffer“-Modus.

---

### 8.4. Drag&Drop-Unterstützung

**Status:** &#x1F4A1; offen

**Vorschlag:** Dateien/Ordner in den Tree ziehen oder verschieben.

**Umsetzung:** `AllowDrop` aktivieren, Zielpfad über Ereignisse validieren.

---

### 8.5. Bookmark/Favoriten

**Status:** &#x1F4A1; offen

**Vorschlag:** Häufige Pfade als Favoritenknoten.

**Umsetzung:** Persistente Favoritenliste mit eigener Root-Gruppe.

---

### 8.6. Empfohlene Reihenfolge

1. Kontextmenü-Integration
2. Multi-Selection-Modus
3. Such- und Filterfunktion
4. Drag&Drop-Unterstützung
5. Bookmark/Favoriten
