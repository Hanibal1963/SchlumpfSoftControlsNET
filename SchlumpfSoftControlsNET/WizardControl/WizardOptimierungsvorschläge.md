# Optimierungsvorschläge für WizardControl

## 1. Zielbild

Vorhersagbarer Seitenfluss mit sauberer Validierung und performanter UI.

## 2. Navigationszustand zentralisieren

**Umsetzung:**

- Zustandsmaschine für `Back`, `Next`, `Finish`, `Cancel` einführen.

- Aktivierungslogik der Buttons aus einem zentralen Evaluator ableiten.

**Nutzen:** Keine inkonsistenten Navigationszustände.

## 3. Seitenwechsel robuster machen

**Umsetzung:**

- `BeforeSwitchPages` mit klarer Cancel-Semantik absichern.

- Reentrancy bei rekursiven Wechseln verhindern.

**Nutzen:** Stabiler Ablauf auch bei komplexer Validierung.

## 4. Rendering/Ladezeiten reduzieren

**Umsetzung:**

- Header-/Welcome-Bilder skaliert cachen.

- Nur aktive Seite vollständig rendern.

**Nutzen:** Schnellere Umschaltung bei großen Bildern.

## 5. Ressourcenverwaltung verbessern

**Umsetzung:**

- Dynamisch erzeugte Seiten und Event-Abos in `Dispose` lösen.

**Nutzen:** Keine Lecks bei wiederholter Wizard-Nutzung.

## 6. Fehlersichtbarkeit erhöhen

**Umsetzung:**

- Validierungsfehler an Feldern markieren.

- Optionales Fehlermodell pro Seite (`ValidationSummary`) anbieten.

**Nutzen:** Höhere Abschlussquote und bessere UX.

## 7. Empfohlene Reihenfolge

1. Navigationszustandsmaschine
2. Seitenwechsel-Härtung
3. Ressourcenmanagement
4. Rendering-Caches
5. Validierungsfeedback