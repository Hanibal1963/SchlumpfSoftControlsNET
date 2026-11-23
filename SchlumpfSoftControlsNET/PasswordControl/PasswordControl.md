# PasswordControl

Ein Control zum Eingeben, Maskieren und Validieren von Passwörtern.

Die Komponente `Password` kapselt die Eingabe eines Passworts in einem eigenen Steuerelement.

Beim Bestätigen mit der Eingabetaste wird für eine nicht leere Eingabe ein PBKDF2-Hash erzeugt, zusätzlich geschützt und über ein Ereignis bereitgestellt. Bei leerer oder nur aus Leerraum bestehender Eingabe wird stattdessen eine geschützte leere Zeichenfolge gemeldet.

## 1. Konstruktoren

- `Password.New()`: Initialisiert das Steuerelement, setzt Mindest- und Standardgröße, aktiviert die Passwortmaskierung und lädt das Standardsymbol.
- `PasswordChangedEventArgs.New(Code As String)`: Erstellt die Ereignisdaten und speichert den übergebenen Passwort-Code.

---

## 2. Ereignisse

- `Password.PasswortChanged(sender As Object, e As PasswordChangedEventArgs)`: Tritt ein, wenn aus dem aktuell eingegebenen Passwort ein neuer geschützter Passwort-Code erzeugt wurde.


---

## 3. Eigenschaften

- `PasswordChangedEventArgs.PasswordCode As String`: Enthält den geschützten Code des erzeugten Passwort-Hashs.


---

## 4. Funktionen

- `Password.VerifyPasswordCode(PasswordCode As String) As Boolean`: Entschlüsselt einen zuvor erzeugten Passwort-Code und vergleicht ihn mit dem aktuell eingegebenen Passwort. Leere Eingaben oder ein leerer entschlüsselter Hash verursachen `ArgumentException`, ungültiges Base64 `FormatException` und nicht entschlüsselbare Daten `CryptographicException`.
- `Password.ClearPassword()`: Löscht den Inhalt des Passwortfelds und setzt den Fokus auf das Steuerelement.
- Intern: `SecurityService.CreatePasswordHash(password As String) As String`: Erzeugt einen speicherbaren Passwort-Hash im Format `PBKDF2$Iterationen$Salt$Hash`.
- Intern: `SecurityService.VerifyPassword(password As String, storedHash As String) As Boolean`: Validiert ein eingegebenes Passwort gegen einen gespeicherten PBKDF2-Hash.
- Intern: `SecurityService.ProtectSecret(plainText As String) As String`: Verschlüsselt Daten benutzergebunden mit DPAPI und liefert einen Base64-kodierten Ciphertext zurück.
- Intern: `SecurityService.UnprotectSecret(protectedBase64 As String) As String`: Entschlüsselt benutzergebundene DPAPI-Daten und liefert den ursprünglichen Klartext zurück.

---

## 5. Hilfstypen/Strukturen

- `Password`: Passwort-Steuerelement mit Maskierungsumschaltung.
- `PasswordChangedEventArgs`: Ereignisdaten für den erzeugten Passwort-Code.
- `SecurityService` (projektintern): Sicherheitslogik für Hashing, Prüfung, Schutz und Entschlüsselung.

---

## 6. Hinweise

- Sicherheitsmerkmale: PBKDF2 mit SHA-256 ab .NET Framework 4.7.2 und bei den .NET-8-, -9- und -10-Zielen; die Ziele .NET Framework 4.6.2 bis 4.7.1 verwenden SHA-1. Da das Hashformat den Algorithmus nicht speichert, sind Hashes dieser beiden Gruppen nicht direkt austauschbar. Hinzu kommen ein zufälliger Salt pro Passwort, konstanter Bytevergleich zur Verringerung von Timing-Angriffen sowie benutzergebundener DPAPI-Schutz.

---

## 7. geplante Optimierungen

### 7.1. Zielbild

Sicheres Passwort-Control mit klarer API und minimierter Angriffsfläche.

---

### 7.2. Kryptoparameter zentral versionieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- PBKDF2-Parameter (Iterationen, Salt-Länge, Hash-Länge, Algorithmus) als versioniertes Format speichern.

- Migrationspfad für alte Hashversionen vorsehen.

**Nutzen:** Zukunftssichere Passwortprüfung.

---

### 7.3. Sensitive Daten im Speicher minimieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Passwortstring-Nutzung zeitlich begrenzen.

- Temporäre Byte-Arrays nach Verwendung nullen.

**Nutzen:** Reduziert Risiko bei Memory-Dumps.

---

### 7.4. Fehlerbehandlung bei DPAPI härten

**Status:** &#x1F4A1; offen

**Umsetzung:**

- `ProtectSecret`/`UnprotectSecret` differenziert behandeln (Format, Kontextwechsel, Benutzerprofil).

- Genaue Fehlermeldungsstrategie ohne Informationsleck.

**Nutzen:** Robustheit in Roaming/Service-Szenarien.

---

### 7.5. Eventfluss präzisieren

**Status:** &#x1F4A1; offen

**Umsetzung:**

- `PasswortChanged` nur bei validem State auslösen.

- Optionales `PasswordValidationFailed`-Event ergänzen.

**Nutzen:** Eindeutigeres Verhalten für Konsumenten.

---

### 7.6. UI-Interaktion verbessern

**Status:** &#x1F4A1; offen

**Umsetzung:**

- Icon-Klickfläche/Keyboard-Aktivierung vereinheitlichen.

- Caps-Lock-Hinweis optional integrieren.

**Nutzen:** Bessere Usability ohne Sicherheitsverlust.

---

### 7.7. Empfohlene Reihenfolge

1. Kryptoversionierung
2. Memory-Härtung
3. DPAPI-Fehlerpfade
4. Eventfluss
5. UI-Feinschliff

---

## 8. geplante Erweiterungen

### 8.1. Passwortregeln konfigurierbar

**Vorschlag:** Mindestlänge, Zeichenklassen, verbotene Muster.
**Umsetzung:** `PasswordPolicy`-Klasse und Validierungsereignis.

---

### 8.2. Stärkeanzeige

**Vorschlag:** Visuelle Bewertung (schwach/mittel/stark).
**Umsetzung:** Entropie- oder regelbasierte Berechnung mit Farbfeedback.

---

### 8.3. Bestätigungsmodus

**Vorschlag:** Optional zweites Eingabefeld zur Bestätigung.
**Umsetzung:** Modus-Property und Event `PasswordsMismatch`.

---

### 8.4. Provider-Modell für Kryptographie

**Vorschlag:** Austauschbarer Hash-/Schutzprovider.
**Umsetzung:** Interface (z. B. `IPasswordCryptoProvider`) und Standardimplementierung.

---

### 8.5. Sichere Zwischenablage

**Vorschlag:** Optionales, zeitlich begrenztes Kopieren des Passwortcodes.
**Umsetzung:** Clipboard-Timer mit automatischem Zurücksetzen.

---

### 8.6. Empfohlene Reihenfolge

1. Passwortregeln konfigurierbar
2. Stärkeanzeige
3. Bestätigungsmodus
4. Provider-Modell für Kryptographie
5. Sichere Zwischenablage
