# Erweiterungsvorschläge – WizardControl

## 1) Bedingte Seitenflüsse

**Vorschlag:** Verzweigungen abhängig von Eingaben.
**Umsetzung:** Navigationsregeln als deklaratives Rule-Set.

## 2) Asynchrone Schrittverarbeitung

**Vorschlag:** Lange Prüfungen/Operationen im Seitenwechsel unterstützen.
**Umsetzung:** Async-Events (`BeforeSwitchPagesAsync`) mit Cancel/Progress.

## 3) Validierungsframework pro Seite

**Vorschlag:** Einheitliches Interface für Seitenvalidierung.
**Umsetzung:** `IWizardPageValidation` mit Ergebnisobjekt.

## 4) Persistenter Wizard-State

**Vorschlag:** Wiederaufnahme nach Abbruch.
**Umsetzung:** Serialisierung von Seitenindex + Eingabedaten.

## 5) Lokalisierungsunterstützung ausbauen

**Vorschlag:** Ressourcenbasierte Texte für Buttons und Seitenelemente.
**Umsetzung:** ResourceManager-Integration und Laufzeitwechsel der Kultur.
