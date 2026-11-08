# WizardControl

Ein Control zum Erstellen eines Assistenten.

Dieses Steuerelement wurde in Anlehnung an den [Wizard](https://marketplace.visualstudio.com/items?itemName=vs-publisher-106990.RuWizard) von [Klaus Rutkowski](https://marketplace.visualstudio.com/publishers/vs-publisher-106990) entwickelt.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz des Assistenten-Controls.

## 2. Ereignisse

- `BeforeSwitchPages`: Tritt auf, bevor die Seiten des Assistenten gewechselt werden, um dem Benutzer die Möglichkeit zur Validierung zu geben.
- `AfterSwitchPages`: Tritt auf, nachdem die Seiten des Assistenten gewechselt wurden, und gibt dem Benutzer die Möglichkeit, die neue Seite einzurichten.
- `Cancel`: Tritt auf, wenn der Benutzer auf „Abbrechen“ geklickt hat.
- `Finish`: Tritt auf, wenn der Assistent abgeschlossen ist, und gibt dem Benutzer die Möglichkeit, zusätzliche Aufgaben zu erledigen.
- `Help`: Tritt auf, wenn der Benutzer auf die Hilfeschaltfläche klickt.

## 3. Eigenschaften

- `VisibleHelp`: Ruft den Sichtbarkeitsstatus der Hilfeschaltfläche ab oder legt diesen fest.
- `Pages`: Ruft die Auflistung der Assistentenseiten in diesem Registerkartensteuerelement ab.
- `ImageHeader`: Ruft das in der Kopfzeile der Standardseiten angezeigte Bild ab oder legt dieses fest.
- `ImageWelcome`: Ruft das auf den Begrüßungs- und Abschlussseiten angezeigte Bild ab oder legt es fest.
- `Dock`: Ruft ab oder legt fest, an welcher Kante des übergeordneten Containers ein Steuerelement angedockt ist.
- `HeaderFont`: Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Standardseite verwendet wird, oder legt diese fest.
- `HeaderTitleFont`: Ruft die Schriftart ab, die zum Anzeigen des Titels einer Standardseite verwendet wird, oder legt diese fest.
- `WelcomeFont`: Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Begrüßungs- oder Abschlussseite verwendet wird, oder legt diese fest.
- `WelcomeTitleFont`: Ruft die Schriftart ab, die zum Anzeigen des Titels einer Begrüßungs- oder Abschlussseite verwendet wird, oder legt diese fest.
- `SelectedPage`: Gibt die aktuell aktive Assistentenseite zurück oder aktiviert eine übergebene Seite.
- `NextEnabled`: Aktiviert oder deaktiviert die Schaltfläche „Weiter“.
- `BackEnabled`: Aktiviert oder deaktiviert die Schaltfläche „Zurück“.
- `NextText`: Gibt den Text der Schaltfläche „Weiter“ zurück oder legt ihn fest.
- `BackText`: Gibt den Text der Schaltfläche „Zurück“ zurück oder legt ihn fest.
- `CancelText`: Gibt den Text der Schaltfläche „Abbrechen/Beenden“ zurück oder legt ihn fest.
- `HelpText`: Gibt den Text der Schaltfläche „Hilfe“ zurück oder legt ihn fest.

## 4. Funktionen

- `Next()`: Entspricht einem Klick auf die Schaltfläche „Weiter“.
- `Back()`: Entspricht einem Klick auf die Schaltfläche „Zurück“.

## 5. Hilfstypen/Strukturen

- `WizardPage`: Basistyp für Assistentenseiten.
- `PagesCollection`: Sammlung der Seiten innerhalb des Assistenten.
- `BeforeSwitchPagesEventArgs`: Ereignisdaten vor dem Seitenwechsel.
- `AfterSwitchPagesEventArgs`: Ereignisdaten nach dem Seitenwechsel.

## 6. Hinweise

- Seitenwechsel können über `BeforeSwitchPages` validiert und abgebrochen werden.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Vorhersagbarer Seitenfluss mit sauberer Validierung und performanter UI.

### 7.2. Navigationszustand zentralisieren

**Umsetzung:**

- Zustandsmaschine für `Back`, `Next`, `Finish`, `Cancel` einführen.

- Aktivierungslogik der Buttons aus einem zentralen Evaluator ableiten.

**Nutzen:** Keine inkonsistenten Navigationszustände.

### 7.3. Seitenwechsel robuster machen

**Umsetzung:**

- `BeforeSwitchPages` mit klarer Cancel-Semantik absichern.

- Reentrancy bei rekursiven Wechseln verhindern.

**Nutzen:** Stabiler Ablauf auch bei komplexer Validierung.

### 7.4. Rendering/Ladezeiten reduzieren

**Umsetzung:**

- Header-/Welcome-Bilder skaliert cachen.

- Nur aktive Seite vollständig rendern.

**Nutzen:** Schnellere Umschaltung bei großen Bildern.

### 7.5. Ressourcenverwaltung verbessern

**Umsetzung:**

- Dynamisch erzeugte Seiten und Event-Abos in `Dispose` lösen.

**Nutzen:** Keine Lecks bei wiederholter Wizard-Nutzung.

### 7.6. Fehlersichtbarkeit erhöhen

**Umsetzung:**

- Validierungsfehler an Feldern markieren.

- Optionales Fehlermodell pro Seite (`ValidationSummary`) anbieten.

**Nutzen:** Höhere Abschlussquote und bessere UX.

### 7.7. Empfohlene Reihenfolge

1. Navigationszustandsmaschine
2. Seitenwechsel-Härtung
3. Ressourcenmanagement
4. Rendering-Caches
5. Validierungsfeedback

## 8. Erweiterungsvorschläge

### 8.1. Bedingte Seitenflüsse

**Vorschlag:** Verzweigungen abhängig von Eingaben.
**Umsetzung:** Navigationsregeln als deklaratives Rule-Set.

### 8.2. Asynchrone Schrittverarbeitung

**Vorschlag:** Lange Prüfungen/Operationen im Seitenwechsel unterstützen.
**Umsetzung:** Async-Events (`BeforeSwitchPagesAsync`) mit Cancel/Progress.

### 8.3. Validierungsframework pro Seite

**Vorschlag:** Einheitliches Interface für Seitenvalidierung.
**Umsetzung:** `IWizardPageValidation` mit Ergebnisobjekt.

### 8.4. Persistenter Wizard-State

**Vorschlag:** Wiederaufnahme nach Abbruch.
**Umsetzung:** Serialisierung von Seitenindex + Eingabedaten.

### 8.5. Lokalisierungsunterstützung ausbauen

**Vorschlag:** Ressourcenbasierte Texte für Buttons und Seitenelemente.
**Umsetzung:** ResourceManager-Integration und Laufzeitwechsel der Kultur.
