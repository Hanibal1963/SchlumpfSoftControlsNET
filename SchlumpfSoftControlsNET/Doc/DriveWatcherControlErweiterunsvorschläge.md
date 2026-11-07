# Erweiterungsvorschläge – DriveWatcherControl

## 1) Filterbare Events

**Vorschlag:** Nur bestimmte DriveTypes oder Buchstaben melden.
**Umsetzung:** Filter-Properties (`IncludeDriveTypes`, `IncludeLetters`) vor Eventauslösung prüfen.

## 2) Snapshot-API

**Vorschlag:** `GetCurrentDrives()` mit Metadaten (Typ, Label, Dateisystem, frei/belegt).
**Umsetzung:** Einheitliches DriveInfo-Modell definieren.

## 3) Erweiterte EventArgs

**Vorschlag:** Seriennummer, Dateisystem, VolumeLabel, IsNetwork.
**Umsetzung:** EventArgs-Klassen erweitern und Abwärtskompatibilität sichern.

## 4) Polling-Fallback

**Vorschlag:** Optionaler Fallback-Modus bei fehlenden Systemevents.
**Umsetzung:** Intervallbasierten Vergleich gegen letzten Snapshot.

## 5) Diagnosemodus

**Vorschlag:** `DiagnosticMode` mit Rohereignis-Feed.
**Umsetzung:** Separates Event `RawDeviceEventReceived` ergänzen.
