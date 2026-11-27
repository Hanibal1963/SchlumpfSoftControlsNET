# Copilot Instructions

## Regeln für Code-Änderungen durch den Agent

- Die erste Zeile jeder Commitnachricht enthält eine einzeilige Zusammenfassung mit höchstens 50 Zeichen. Die zweite Zeile ist eine Leerzeile. Ab der dritten Zeile folgt eine vollständige Zusammenfassung der Änderungen.

- Bevorzuge kleine, rückverfolgbare Commits.

- Erhalte und erweitere vorhandene XML-Kommentare.

- Füge XML-Kommentare für alle öffentlichen und privaten Member sowie Inlinekommentare zum besseren Verständnis der Funktionsweise des Codes hinzu in die Quellcode-Dateien ein. Die Kommentare sollen den Zweck, die Funktionsweise und die Verwendung der Member beschreiben. Kommentare sollen nicht nur den Code wiederholen, sondern zusätzliche Informationen liefern.

- Verwende Tabs und die standardmäßigen Datei-Header-Optionen.

- Keine neuen NuGet-Abhängigkeiten ohne Rücksprache.

- Nach einem vom Agenten gestarteten Build darf die Versionsnummer nicht zurückgesetzt werden. Automatische Versionsänderungen durch den Build sind auch bei der Bereinigung des Arbeitsverzeichnisses zu erhalten; eine Rücksetzung ist nur auf ausdrückliche Anweisung des Nutzers zulässig.

- Vermeide bei Codeänderungen neue Compiler- und Analyzerwarnungen. Prüfe die betroffenen Zielplattformen mit dem passenden Build und behebe durch die Änderung verursachte Warnungen an ihrer Ursache, ohne bestehendes Verhalten unnötig zu ändern. Unterdrücke Warnungen nicht pauschal; falls eine Unterdrückung wegen fehlender plattformspezifischer APIs oder aus anderen Gründen unvermeidbar ist, begründe und begrenze sie ausdrücklich. Melde verbleibende oder nicht überprüfbare Warnungen.

- Vermeide MSTEST0037: Verwende `Assert.IsTrue` und `Assert.IsFalse` nur für echte boolesche Bedingungen. Nutze für Vergleiche `Assert.IsGreaterThan`, `Assert.IsLessThan` oder `Assert.IsInRange`, für Null-/String-Prüfungen die spezifischen `Assert`-Methoden und für Sammlungen `Assert.Contains`, `Assert.DoesNotContain`, `Assert.IsEmpty` oder `Assert.IsNotEmpty`, sofern sie die gleiche Bedingung ausdrücken.

- Arbeite bei größeren Änderungen schrittweise und gib nach jedem Schritt explizit frei, bevor der nächste Schritt ausgeführt wird. **Im Guided-Modus vor jedem größeren Schritt Rückfrage/Freigabe einholen.**

- Führe zusätzliche Detailprüfungen nur auf Nachfrage und fundweise mit vorheriger Rückfrage/Freigabe durch.

- Bei Entscheidungen im laufenden Szenario, wie der Installation eines Konvertierungstools, Task 01 erneut starten statt manueller Ausnahme.

## Projektrichtlinien

- Bei Markdown-Dokumenten sollen die Regeln MD022 und MD032 eingehalten werden: Überschriften müssen von Leerzeilen umgeben sein und Listen ebenfalls.

- Compilerwarnungen und Analyzerwarnungen sollen nicht unterdrückt werden, außer es ist ausdrücklich begründet und auf die betroffene Stelle beschränkt. Melde verbleibende oder nicht überprüfbare Warnungen. Auch Meldungen sollen möglichst vermieden werden, die auf fehlende plattformspezifische APIs zurückzuführen sind. Prüfe die betroffenen Zielplattformen mit dem passenden Build und behebe durch die Änderung verursachte Warnungen an ihrer Ursache, ohne bestehendes Verhalten unnötig zu ändern.

## Konventionen & Stil

- Einrückung: Tabs (keine Spaces).

- Naming: Klassen PascalCase; lokale Variablen camelCase; private Felder _camelCase.

- Öffentliche Member: XML-Kommentare (`<summary>`, `<param>`, `<returns>`) erhalten/erweitern. Verwende in den Texten der XML-Kommentare statt der Zeichen "<" und ">" die HTML-Entities "&lt;" und "&gt;".

- Kodierungsfehler bei Umlauten (Mojibake wie �/�/�) sind zu vermeiden.

## Dokumentationsdateien

- README.md: Enthält eine kurze Projektbeschreibung, Installationsanweisungen, Beispiele und Lizenzinformationen.

- Dateiname: Die Dateinamen der Dokumentationsdateien sollen den Namen der Klasse oder des Moduls widerspiegeln, auf das sie sich beziehen, und die Endung `.md` haben. Beispiel: `MyClass.md` für die Klasse `MyClass`.

- Lage: Dokumentationsdateien liegen im selben Ordner wie die Klasse oder des Moduls, auf das sie sich beziehen.

- Struktur der Dokumentationsdateien:

  - Die erste Überschrift der Dokumentationsdateien soll den Namen der Klasse oder des Moduls widerspiegeln, auf das sie sich beziehen. Beispiel: `# MyClass` für die Klasse `MyClass` und soll eine Kurzbeschreibung (1-3 Sätze zum Zweck der Klasse/Funktion) enthalten.

  - Abschnitt `## 1. Konstruktoren` mit allen öffentlichen Konstruktoren inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).

  - Abschnitt `## 2. Ereignisse` mit allen öffentlichen Ereignissen inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).

  - Abschnitt `## 3. Eigenschaften` mit allen öffentlichen Eigenschaften inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).

  - Abschnitt `## 4. Funktionen` mit allen öffentlichen Methoden/Shared-Methoden inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).

  - Abschnitt `## 5. Hilfstypen/Strukturen` mit allen öffentlichen Hilfstypen/Strukturen inkl. kurzer Beschreibung, Parameterbedingungen und Verhalten bei ungültigen Eingaben (Exceptions).

  - Abschnitt `## 6. Hinweise` für numerische Grenzen, Genauigkeit, Abhängigkeiten (z. B. `System.Drawing`) und Namespace-Besonderheiten.

  - Abschnitt `## 7. geplante Optimierungen` mit allen Vorschlägen zur Verbesserung der Leistung, Lesbarkeit oder Wartbarkeit (als Unterpunkte ### 7.x.) und dem letzten Abschnitt mit der Reihenfolge zur Durchführung.

  - Abschnitt `## 8. geplante Erweiterungen` mit allen Vorschlägen zur Erweiterung der Funktionalität (als Unterpunkte ### 8.x.) und dem letzten Abschnitt mit der Reihenfolge zur Durchführung.

  - Die Unterpunkte der abschnitte 7 und 8 sollen jeweils einen Punkt `**Status** &#x1F4A1; offen:` oder `**Status** &#x2705; Umgesetzt:` oder `**Status** &#x26A0; Teilweise umgesetzt:`, einen Punkt `**Umsetzung**` (mit einer Auflistung der Schritte) und einen Punkt `**Nutzen**` (mit der erwarteten Wirkung) haben

  - Optionale kurze Beispiele, wenn für die Nutzung hilfreich.

  - Die Reihenfolge der Überschriften bzw. Abschnitte soll wie oben beschrieben eingehalten werden. Zusätzliche Überschriften sind nur in Ausnahmefällen und auf Nachfrage erlaubt.

- Inhaltspflege: Bei Änderungen an API, Signaturen oder Guard-Clauses müssen die zugehörigen `.md`-Dateien im selben Änderungsumfang aktualisiert werden. Dokumentation und XML-Kommentare im Code müssen konsistent bleiben.

- Validierung: Nach Änderungen auf korrekte Methodennamen, konsistente Signaturen und lesbare Struktur prüfen. Vorhandene Dokumentationsdateien als Referenz verwenden.

- In .md-Dateien sollen die Zeichen '<' und '>' normal verwendet werden; HTML-Entities (&lt;, &gt;) nur in XML-Kommentaren im Code.

## Übersetzungen

- Bei der Auslagerung von Meldungstexten in Ressourcen gelten die deutschen Texte als maßgebliche Quelle für Übersetzungen; Englisch (und eventuelle weitere Sprachen) wird daraus abgeleitet.
