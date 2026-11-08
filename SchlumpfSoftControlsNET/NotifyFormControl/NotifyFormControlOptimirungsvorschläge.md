# Optimierungsvorschläge – NotifyFormControl

## Zielbild

Ressourcenschonende, flüssige Benachrichtigungen mit konsistentem Verhalten.

## 1) Animationslogik zentralisieren

**Umsetzung:**

- Einblend-/Ausblendanimation in klaren Zustandsautomaten überführen.

- Reentrancy-Schutz für mehrfaches `Show()` ergänzen.

**Nutzen:** Keine flackernden oder überlappenden Zustände.

## 2) Timer-/Handle-Management absichern

**Umsetzung:**

- Alle Timer in `Dispose` sicher stoppen und freigeben.

- Fensterlebenszyklus bei schnellem Öffnen/Schließen testen.

**Nutzen:** Keine Lecks bei häufiger Nutzung.

## 3) Multi-Monitor/Taskbar-Kollisionen verbessern

**Umsetzung:**

- Positionierung auf Arbeitsbereich des Zielmonitors.

- Stapelung mehrerer Notifications mit Abstandskalkulation.

**Nutzen:** Zuverlässige Sichtbarkeit auf allen Setups.

## 4) Rendering-Qualität erhöhen

**Umsetzung:**

- Textmessung DPI-sicher gestalten.

- Icons vorab skalieren und cachen.

**Nutzen:** Schärfere Darstellung bei High-DPI.

## 5) Fehlertoleranz im Message-Input

**Umsetzung:**

- Null/leer-Werte robust behandeln.

- Lange Texte automatisch umbrechen und Höhe anpassen.

**Nutzen:** Stabile Anzeige bei variierenden Inhalten.

## Empfohlene Reihenfolge

1. Lifecycle/TImer

2. Zustandsautomat

3. Positionierung

4. DPI-Rendering

5. Input-Härtung
