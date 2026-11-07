# PasswordControl

Ein Control zum Eingeben, Maskieren und Validieren von Passwörtern.

Die Komponente `Password` kapselt die Eingabe eines Passworts in einem eigenen Steuerelement.

Beim Bestätigen der Eingabe wird aus dem Passwort ein PBKDF2-Hash erzeugt, zusätzlich geschützt und über ein Ereignis bereitgestellt.

## Konstruktoren

- `Password.New()`: Initialisiert das Steuerelement, setzt Mindest- und Standardgröße, aktiviert die Passwortmaskierung und lädt das Standardsymbol.
- `PasswordChangedEventArgs.New(Code As String)`: Erstellt die Ereignisdaten und speichert den übergebenen Passwort-Code.

## Ereignisse

- `Password.PasswortChanged(sender As Object, e As PasswordChangedEventArgs)`: Tritt ein, wenn aus dem aktuell eingegebenen Passwort ein neuer geschützter Passwort-Code erzeugt wurde.
- `SecurityService`: Keine öffentlichen Ereignisse dokumentiert.

## Eigenschaften

- `PasswordChangedEventArgs.PasswordCode As String`: Enthält den geschützten Code des erzeugten Passwort-Hashs.
- `SecurityService`: Keine öffentlichen Eigenschaften dokumentiert.

## Funktionen

- `Password.VerifyPasswordCode(PasswordCode As String) As Boolean`: Entschlüsselt einen zuvor erzeugten Passwort-Code und vergleicht ihn mit dem aktuell eingegebenen Passwort.
- `Password.ClearPassword()`: Löscht den Inhalt des Passwortfelds und setzt den Fokus auf das Steuerelement.
- `SecurityService.CreatePasswordHash(password As String) As String`: Erzeugt einen speicherbaren Passwort-Hash im Format `PBKDF2$Iterationen$Salt$Hash`.
- `SecurityService.VerifyPassword(password As String, storedHash As String) As Boolean`: Validiert ein eingegebenes Passwort gegen einen gespeicherten PBKDF2-Hash.
- `SecurityService.ProtectSecret(plainText As String) As String`: Verschlüsselt Daten benutzergebunden mit DPAPI und liefert einen Base64-kodierten Ciphertext zurück.
- `SecurityService.UnprotectSecret(protectedBase64 As String) As String`: Entschlüsselt benutzergebundene DPAPI-Daten und liefert den ursprünglichen Klartext zurück.

## Hilfstypen/Strukturen

- `Password`: Passwort-Steuerelement mit Maskierungsumschaltung.
- `PasswordChangedEventArgs`: Ereignisdaten für den erzeugten Passwort-Code.
- `SecurityService`: Sicherheitslogik für Hashing, Prüfung, Schutz und Entschlüsselung.

## Hinweise

- Sicherheitsmerkmale: PBKDF2 mit SHA-256, zufälliger Salt pro Passwort, konstanter Bytevergleich zur Verringerung von Timing-Angriffen sowie DPAPI-Schutz für gespeicherte oder weitergegebene Hashwerte.

## Optimierungs und Verbesserungsmöglichkeiten

- [Optimierungsvorschläge](PasswordControlOptimirungsvorschläge.md)
- [Erweiterungsvorschläge](PasswordControlErweiterunsvorschläge.md)
