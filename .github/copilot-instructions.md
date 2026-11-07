# Copilot Instructions

## Regeln für Code-Änderungen durch den Agent

- Bevorzuge kleine, rückverfolgbare Commits.
- Erhalte und erweitere vorhandene XML-Kommentare.
- Verwende Tabs und die standardmäßigen Datei-Header-Optionen.
- Keine neuen NuGet-Abhängigkeiten ohne Rücksprache.
- Arbeite bei größeren Änderungen schrittweise und gib nach jedem Schritt explizit frei, bevor der nächste Schritt ausgeführt wird. **Im Guided-Modus vor jedem größeren Schritt Rückfrage/Freigabe einholen.**
- Führe zusätzliche Detailprüfungen nur auf Nachfrage und fundweise mit vorheriger Rückfrage/Freigabe durch.
- Bei Entscheidungen im laufenden Szenario, wie der Installation eines Konvertierungstools, Task 01 erneut starten statt manueller Ausnahme.

## Projektrichtlinien

- Bei Markdown-Dokumenten sollen die Regeln MD022 und MD032 eingehalten werden: Überschriften müssen von Leerzeilen umgeben sein und Listen ebenfalls.

## Konventionen & Stil

- Einrückung: Tabs (keine Spaces).
- Naming: Klassen PascalCase; lokale Variablen camelCase; private Felder _camelCase.
- Öffentliche Member: XML-Kommentare (`<summary>`, `<param>`, `<returns>`) erhalten/erweitern. Verwende in den Texten der XML-Kommentare statt der Zeichen "<" und ">" die HTML-Entities "&lt;" und "&gt;".
- Kodierungsfehler bei Umlauten (Mojibake wie �/�/�) sind zu vermeiden.

## Dokumentationsdateien

- README.md: Enthält eine kurze Projektbeschreibung, Installationsanweisungen, Beispiele und Lizenzinformationen.
- Lage: Dokumentationsdateien liegen im Ordner `Doc`.
- Dateiname: Die Dateinamen der Dokumentationsdateien sollen den Namen der Klasse oder des Moduls widerspiegeln, auf das sie sich beziehen, und die Endung `.md` haben. Beispiel: `MyClass.md` für die Klasse `MyClass`.
- Struktur der Dokumentationsdateien:
  - Die erste Überschrift der Dokumentationsdateien soll den Namen der Klasse oder des Moduls widerspiegeln, auf das sie sich beziehen. Beispiel: `# MyClass` für die Klasse `MyClass` und soll eine Kurzbeschreibung (1-3 Sätze zum Zweck der Klasse/Funktion) enthalten.
  - Abschnitt `## Konstruktoren` mit allen öffentlichen Konstruktoren inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).
  - Abschnitt `## Ereignisse` mit allen öffentlichen Ereignissen inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).
  - Abschnitt `## Eigenschaften` mit allen öffentlichen Eigenschaften inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).
  - Abschnitt `## Funktionen` mit allen öffentlichen Methoden/Shared-Methoden inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).
  - Abschnitt `## Hilfstypen/Strukturen` mit allen öffentlichen Hilfstypen/Strukturen inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).
  - Abschnitt `## Hinweise` für numerische Grenzen, Genauigkeit, Abhängigkeiten (z. B. `System.Drawing`) und Namespace-Besonderheiten.
  - Optionale kurze Beispiele, wenn für die Nutzung hilfreich.
  - Die Reihenfolge der Überschriften bzw. Abschnitte soll wie oben beschrieben eingehalten werden. Zusätzliche Überschriften sind nur in Ausnahmefällen und auf Nachfrage erlaubt.
  
- Inhaltspflege: Bei Änderungen an API, Signaturen oder Guard-Clauses müssen die zugehörigen `.md`-Dateien im selben Änderungsumfang aktualisiert werden. Dokumentation und XML-Kommentare im Code müssen konsistent bleiben.
- Validierung: Nach Änderungen auf korrekte Methodennamen, konsistente Signaturen und lesbare Struktur prüfen. Vorhandene Dokumentationsdateien als Referenz verwenden.
- In .md-Dateien sollen die Zeichen '<' und '>' normal verwendet werden; HTML-Entities (&lt;, &gt;) nur in XML-Kommentaren im Code.

## Übersetzungen

- Bei der Auslagerung von Meldungstexten in Ressourcen gelten die deutschen Texte als maßgebliche Quelle für Übersetzungen; Englisch wird daraus abgeleitet.
