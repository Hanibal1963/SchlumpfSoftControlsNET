# NotifyFormControl

Ein Control zum Anzeigen von Benachrichtigungsfenstern.

Der ursprüngliche Code zu diesem Projekt stammt aus einer nicht mehr verfügbaren Quelle.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `NotifyForm` mit Standardwerten.

---

## 2. Ereignisse

- Keine öffentlichen Ereignisse vorhanden.

---

## 3. Eigenschaften

- `Design`: Legt das Aussehen des Benachrichtigungsfensters fest (`Bright`, `Colorful`, `Dark`).
- `Message`: Legt den Benachrichtigungstext fest, der angezeigt werden soll.
- `ShowTime`: Legt die Anzeigedauer des Benachrichtigungsfensters in Millisekunden fest (`0` = kein automatisches Schließen).
- `Style`: Legt das anzuzeigende Symbol im Benachrichtigungsfenster fest (`Information`, `Question`, `CriticalError`, `Exclamation`).
- `Title`: Legt den Text der Titelzeile des Benachrichtigungsfensters fest.

---

## 4. Funktionen

- `Show()`: Zeigt das Meldungsfenster mit den aktuellen Einstellungen an.

---

## 5. Hilfstypen/Strukturen

- `NotifyFormStyle`: Definiert die unterstützten Symboltypen der Benachrichtigung.
- `NotifyFormDesign`: Definiert die verfügbaren Designvarianten.

---

## 6. Hinweise

- `ShowTime = 0` deaktiviert das automatische Schließen des Benachrichtigungsfensters.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Ressourcenschonende, flüssige Benachrichtigungen mit konsistentem Verhalten.

---

### 7.2. Animationslogik zentralisieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Einblend-/Ausblendanimation in klaren Zustandsautomaten überführen.

- Reentrancy-Schutz für mehrfaches `Show()` ergänzen.

**Nutzen:** Keine flackernden oder überlappenden Zustände.

---

### 7.3. Timer-/Handle-Management absichern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Alle Timer in `Dispose` sicher stoppen und freigeben.

- Fensterlebenszyklus bei schnellem Öffnen/Schließen testen.

**Nutzen:** Keine Lecks bei häufiger Nutzung.

---

### 7.4. Multi-Monitor/Taskbar-Kollisionen verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Positionierung auf Arbeitsbereich des Zielmonitors.

- Stapelung mehrerer Notifications mit Abstandskalkulation.

**Nutzen:** Zuverlässige Sichtbarkeit auf allen Setups.

---

### 7.5. Rendering-Qualität erhöhen

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Textmessung DPI-sicher gestalten.

- Icons vorab skalieren und cachen.

**Nutzen:** Schärfere Darstellung bei High-DPI.

---

### 7.6. Fehlertoleranz im Message-Input

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Null/leer-Werte robust behandeln.

- Lange Texte automatisch umbrechen und Höhe anpassen.

**Nutzen:** Stabile Anzeige bei variierenden Inhalten.

---

### 7.7. Empfohlene Reihenfolge

1. Lifecycle/TImer
2. Zustandsautomat
3. Positionierung
4. DPI-Rendering
5. Input-Härtung

---

## 8. geplante Erweiterungen

### 8.1. Queue-Manager für Benachrichtigungen

**Vorschlag:** Mehrere Meldungen seriell/parallel verwalten.
**Umsetzung:** `NotificationManager` mit Priorität und Ablaufsteuerung.

---

### 8.2. Interaktive Aktionen

**Vorschlag:** Buttons/Links im Notification-Fenster.
**Umsetzung:** Action-Collection mit Callback-Ereignissen.

---

### 8.3. Positionierungsoptionen

**Vorschlag:** Ecke/Monitor frei wählbar (`TopRight`, `BottomLeft`, ...).
**Umsetzung:** Enum + Berechnung relativ zur WorkingArea.

---

### 8.4. Vorlagen und Themes

**Vorschlag:** Wiederverwendbare Design-Presets.
**Umsetzung:** Theme-Objekte für Farben, Schrift, Icon-Sets.

---

### 8.5. Fortschritts-Notification

**Vorschlag:** Optionaler Progressbalken innerhalb der Benachrichtigung.
**Umsetzung:** Property `ProgressValue` + animiertes Repaint.

---

### 8.6. Empfohlene Reihenfolge

1. Queue-Manager für Benachrichtigungen
2. Interaktive Aktionen
3. Positionierungsoptionen
4. Vorlagen und Themes
5. Fortschritts-Notification
