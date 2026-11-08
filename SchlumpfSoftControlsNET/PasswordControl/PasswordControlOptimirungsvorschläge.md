# Optimierungsvorschläge – PasswordControl

## Zielbild

Sicheres Passwort-Control mit klarer API und minimierter Angriffsfläche.

## 1) Kryptoparameter zentral versionieren

**Umsetzung:**

- PBKDF2-Parameter (Iterationen, Salt-Länge, Hash-Länge, Algorithmus) als versioniertes Format speichern.

- Migrationspfad für alte Hashversionen vorsehen.

**Nutzen:** Zukunftssichere Passwortprüfung.

## 2) Sensitive Daten im Speicher minimieren

**Umsetzung:**

- Passwortstring-Nutzung zeitlich begrenzen.

- Temporäre Byte-Arrays nach Verwendung nullen.

**Nutzen:** Reduziert Risiko bei Memory-Dumps.

## 3) Fehlerbehandlung bei DPAPI härten

**Umsetzung:**

- `ProtectSecret`/`UnprotectSecret` differenziert behandeln (Format, Kontextwechsel, Benutzerprofil).

- Genaue Fehlermeldungsstrategie ohne Informationsleck.

**Nutzen:** Robustheit in Roaming/Service-Szenarien.

## 4) Eventfluss präzisieren

**Umsetzung:**

- `PasswortChanged` nur bei validem State auslösen.

- Optionales `PasswordValidationFailed`-Event ergänzen.

**Nutzen:** Eindeutigeres Verhalten für Konsumenten.

## 5) UI-Interaktion verbessern

**Umsetzung:**

- Icon-Klickfläche/Keyboard-Aktivierung vereinheitlichen.

- Caps-Lock-Hinweis optional integrieren.

**Nutzen:** Bessere Usability ohne Sicherheitsverlust.

## Empfohlene Reihenfolge

1. Kryptoversionierung

2. Memory-Härtung

3. DPAPI-Fehlerpfade

4. Eventfluss

5. UI-Feinschliff
