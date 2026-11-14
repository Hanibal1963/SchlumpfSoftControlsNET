# Erweiterungsvorschläge für den ExplorerTreeViewControl

## 1. Kontextmenü-Integration

**Vorschlag:** Optionales Shell-nahes Kontextmenü je Knoten.
**Umsetzung:** Ereignis `BuildContextMenu(node, menu)` bereitstellen.

## 2. Multi-Selection-Modus

**Vorschlag:** Mehrfachauswahl von Ordnern.
**Umsetzung:** Interne Selektionsverwaltung plus API `SelectedPaths`.

## 3. Such- und Filterfunktion

**Vorschlag:** Knoten per Muster/Präfix filtern.
**Umsetzung:** In-Memory-Filter mit Highlighting und optionalem „nur Treffer“-Modus.

## 3. Drag&Drop-Unterstützung

**Vorschlag:** Dateien/Ordner in den Tree ziehen oder verschieben.
**Umsetzung:** `AllowDrop` aktivieren, Zielpfad über Ereignisse validieren.

## 4. Bookmark/Favoriten

**Vorschlag:** Häufige Pfade als Favoritenknoten.
**Umsetzung:** Persistente Favoritenliste mit eigener Root-Gruppe.
