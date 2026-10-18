# PasswordControl

Ein Control zum Eingeben, Maskieren und Validieren von Passwörtern.

## Überblick

Die Komponente `Password` kapselt die Eingabe eines Passworts in einem eigenen Steuerelement.

Beim Bestätigen der Eingabe wird aus dem Passwort ein PBKDF2-Hash erzeugt, zusätzlich geschützt und über ein Ereignis bereitgestellt.

## Enthaltene Dateien

- `Password.vb` Enthält das eigentliche Passwort-Steuerelement mit Anzeigeumschaltung, Größenlogik und Ereignisauslösung.
- `PasswordChangedEventArgs.vb` Definiert die Ereignisdaten für den erzeugten Passwort-Code.
- `SecurityService.vb` Kapselt das Erzeugen, Prüfen, Schützen und Entschützen von Passwortdaten.

## Klasse `Password`

### Zweck

Das Steuerelement dient zur Eingabe eines Passworts und zur Prüfung eines zuvor erzeugten Passwort-Codes.

Zusätzlich kann die Maskierung des Passworts über ein Symbol ein- oder ausgeschaltet werden.

### Öffentliche Methoden

- `New()` Initialisiert das Steuerelement, setzt Mindest- und Standardgröße, aktiviert die Passwortmaskierung und lädt das Standardsymbol.
- `VerifyPasswordCode(PasswordCode As String) As Boolean` Entschlüsselt einen zuvor erzeugten Passwort-Code und vergleicht ihn mit dem aktuell eingegebenen Passwort.
- `ClearPassword()` Löscht den Inhalt des Passwortfelds und setzt den Fokus auf das Steuerelement.

### Ereignisse

- `PasswortChanged(sender As Object, e As PasswordChangedEventArgs)` Tritt ein, wenn aus dem aktuell eingegebenen Passwort ein neuer geschützter Passwort-Code erzeugt wurde.

### Interne Funktionsweise

- Bei der Eingabetaste wird der aktuelle Inhalt des Textfelds ausgewertet.
- Für nichtleere Eingaben wird ein PBKDF2-Hash erzeugt.
- Der Hash wird anschließend mit DPAPI geschützt.
- Das Ergebnis wird über `PasswortChanged` an abonnierende Komponenten weitergegeben.
- Beim Klick auf das Symbol wird zwischen sichtbarer und maskierter Eingabe umgeschaltet.

## Klasse `PasswordChangedEventArgs`

### Zweck

Diese Klasse stellt die Daten für das Ereignis `PasswortChanged` bereit.

### Öffentliche Eigenschaften

- `PasswordCode As String` Enthält den geschützten Code des erzeugten Passwort-Hashs.

### Öffentliche Methoden

- `New(Code As String)` Erstellt die Ereignisdaten und speichert den übergebenen Passwort-Code.

## Klasse `SecurityService`

### Zweck

Der `SecurityService` übernimmt die sicherheitsrelevanten Operationen des Steuerelements.
Dazu gehören das Erzeugen eines Passwort-Hashs, das Prüfen eines Passworts sowie das Schützen und Entschützen sensibler Daten.

### Eigenschaften - SecurityService

Die Klasse stellt keine öffentlichen Eigenschaften bereit.

### Methoden - SecurityService

- `CreatePasswordHash(password As String) As String` Erzeugt einen speicherbaren Passwort-Hash im Format `PBKDF2$Iterationen$Salt$Hash`.
- `VerifyPassword(password As String, storedHash As String) As Boolean` Validiert ein eingegebenes Passwort gegen einen gespeicherten PBKDF2-Hash.
- `ProtectSecret(plainText As String) As String` Verschlüsselt Daten benutzergebunden mit DPAPI und liefert einen Base64-kodierten Ciphertext zurück.
- `UnprotectSecret(protectedBase64 As String) As String` Entschlüsselt benutzergebundene DPAPI-Daten und liefert den ursprünglichen Klartext zurück.

### Ereignisse - SecurityService

Die Klasse stellt keine öffentlichen Ereignisse bereit.

### Sicherheitsmerkmale

- PBKDF2 mit SHA-256
- Zufälliger Salt pro Passwort
- Konfigurierbare Iterationszahl im Dienst
- Konstanter Bytevergleich zur Verringerung von Timing-Angriffen
- DPAPI-Schutz für gespeicherte oder weitergegebene Hashwerte
