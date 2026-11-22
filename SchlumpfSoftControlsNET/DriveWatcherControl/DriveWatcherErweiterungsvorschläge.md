# Erweiterungsvorschläge für DriveWatcherControl

## 1. Filterbare Events

**Status:** &#x1F4A1; offen

**Vorschlag:** Nur bestimmte DriveTypes oder Buchstaben melden.

**Umsetzung:** Filter-Properties (`IncludeDriveTypes`, `IncludeLetters`) vor Eventauslösung prüfen.

## 2. Snapshot-API

**Status:** &#x1F4A1; offen

**Vorschlag:** `GetCurrentDrives()` mit Metadaten (Typ, Label, Dateisystem, frei/belegt).

**Umsetzung:** Einheitliches DriveInfo-Modell definieren.

## 3. Erweiterte EventArgs

**Status:** &#x1F4A1; offen

**Vorschlag:** Seriennummer und IsNetwork. Dateisystem (`DriveFormat`) und `VolumeLabel` sind bereits in `DriveWatcherAddedEventArgs` enthalten.

**Umsetzung:** EventArgs-Klassen erweitern und Abwärtskompatibilität sichern.

## 4. Polling-Fallback

**Status:** &#x1F4A1; offen

**Vorschlag:** Optionaler Fallback-Modus bei fehlenden Systemevents.

**Umsetzung:** Intervallbasierten Vergleich gegen letzten Snapshot.

## 5. Diagnosemodus

**Status:** &#x1F4A1; offen

**Vorschlag:** `DiagnosticMode` mit Rohereignis-Feed.

**Umsetzung:** Separates Event `RawDeviceEventReceived` ergänzen.
