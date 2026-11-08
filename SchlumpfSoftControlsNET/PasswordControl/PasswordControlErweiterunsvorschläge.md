# Erweiterungsvorschläge – PasswordControl

## 1) Passwortregeln konfigurierbar

**Vorschlag:** Mindestlänge, Zeichenklassen, verbotene Muster.
**Umsetzung:** `PasswordPolicy`-Klasse und Validierungsereignis.

## 2) Stärkeanzeige

**Vorschlag:** Visuelle Bewertung (schwach/mittel/stark).
**Umsetzung:** Entropie- oder regelbasierte Berechnung mit Farbfeedback.

## 3) Bestätigungsmodus

**Vorschlag:** Optional zweites Eingabefeld zur Bestätigung.
**Umsetzung:** Modus-Property und Event `PasswordsMismatch`.

## 4) Provider-Modell für Kryptographie

**Vorschlag:** Austauschbarer Hash-/Schutzprovider.
**Umsetzung:** Interface (z. B. `IPasswordCryptoProvider`) und Standardimplementierung.

## 5) Sichere Zwischenablage

**Vorschlag:** Optionales, zeitlich begrenztes Kopieren des Passwortcodes.
**Umsetzung:** Clipboard-Timer mit automatischem Zurücksetzen.
