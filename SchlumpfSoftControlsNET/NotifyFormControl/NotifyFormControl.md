# NotifyForm Control

Ein Control zum Anzeigen von Benachrichtigungsfenstern.

Der ursprüngliche Code zu diesem Projekt stammt aus einer nicht mehr verfügbaren Quelle.

## 1. Konstruktoren

- `New()`: Initialisiert eine neue Instanz von `NotifyForm` mit Standardwerten.

## 2. Ereignisse

- Keine öffentlichen Ereignisse vorhanden.

## 3. Eigenschaften

- `Design`: Legt das Aussehen des Benachrichtigungsfensters fest (`Bright`, `Colorful`, `Dark`).
- `Message`: Legt den Benachrichtigungstext fest, der angezeigt werden soll.
- `ShowTime`: Legt die Anzeigedauer des Benachrichtigungsfensters in Millisekunden fest (`0` = kein automatisches Schließen).
- `Style`: Legt das anzuzeigende Symbol im Benachrichtigungsfenster fest (`Information`, `Question`, `CriticalError`, `Exclamation`).
- `Title`: Legt den Text der Titelzeile des Benachrichtigungsfensters fest.

## 4. Funktionen

- `Show()`: Zeigt das Meldungsfenster mit den aktuellen Einstellungen an.

## 5. Hilfstypen/Strukturen

- `NotifyformStyle`: Definiert die unterstützten Symboltypen der Benachrichtigung.
- `NotifyFormDesign`: Definiert die verfügbaren Designvarianten.

## 6. Hinweise

- `ShowTime = 0` deaktiviert das automatische Schließen des Benachrichtigungsfensters.

## [7. Optimierungsvorschläge](NotifyFormOptimierungsvorschläge.md)

## [8. Erweiterungsvorschläge](NotifyFormErweiterungsvorschläge.md)