# NotifyForm Control

Ein Control zum Anzeigen von Benachrichtigungsfenstern.

Der ursprüngliche Code zu diesem Projekt stammt aus den Tiefen des Internets.

Leider ist die Quelle offensichtlich nicht mehr verfügbar.

Falls jemand die ursprüngliche Quelle kennt oder finden sollte, dann bitte eine Nachricht an mich damit ich diese hier benennen kann.

## Eigenschaften

- `Design`: Legt das Aussehen des Benachrichtigungsfensters fest (`Bright`, `Colorful`, `Dark`).
- `Message`: Legt den Benachrichtigungstext fest, der angezeigt werden soll.
- `ShowTime`: Legt die Anzeigedauer des Benachrichtigungsfensters in Millisekunden fest (`0` = kein automatisches Schließen).
- `Style`: Legt das anzuzeigende Symbol im Benachrichtigungsfenster fest (`Information`, `Question`, `CriticalError`, `Exclamation`).
- `Title`: Legt den Text der Titelzeile des Benachrichtigungsfensters fest.

## Methoden

- `New()`: Initialisiert eine neue Instanz von `NotifyForm` mit Standardwerten.
- `Show()`: Zeigt das Meldungsfenster mit den aktuellen Einstellungen an.

## Ereignisse

- Keine öffentlichen Ereignisse vorhanden.
