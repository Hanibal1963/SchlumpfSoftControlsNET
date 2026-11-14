# Optimierungsvorschläge für FileListControl

## 1. Zielbild

Große Verzeichnisse performant laden und bedienen.

## 2. Virtuellen ListView-Modus prüfen

**Umsetzung:**

- Optional `VirtualMode` einführen.

- Datenquelle getrennt halten (Model statt direkter Item-Objekte).

**Nutzen:** Deutlich bessere Performance bei vielen Dateien.

## 3. Ordnergrößenberechnung entkoppeln

**Umsetzung:**

- Größe zunächst als „wird berechnet“ anzeigen.

- Berechnung asynchron pro Eintrag, mit Abbruchtoken bei Ordnerwechsel.

**Nutzen:** Sofortige Anzeige ohne UI-Blockade.

## 4. Sortierung stabilisieren

**Umsetzung:**

- Typisierte Sortierung pro Spalte (Datum/Größe/Text).

- Sekundärsortierung (Name) für reproduzierbare Reihenfolge.

**Nutzen:** Erwartbares Sortierverhalten.

## 5. Persistenz robuster machen

**Umsetzung:**

- `ColumnOrderState` versionieren.

- Ungültige/alte Zustände beim Laden validieren und fallbacken.

**Nutzen:** Weniger Fehler nach Updates.

## 6. Dateisystemfehler abfangen

**Umsetzung:**

- Zugriffsfehler je Eintrag behandeln.

- Teilresultate anzeigen statt Abbruch.

**Nutzen:** Stabile Anzeige auch bei geschützten Pfaden.

## 7. Empfohlene Reihenfolge

1. Async-Ladepfad
2. Virtueller Modus
3. Sortierlogik
4. Persistenz
5. Fehlerbehandlung