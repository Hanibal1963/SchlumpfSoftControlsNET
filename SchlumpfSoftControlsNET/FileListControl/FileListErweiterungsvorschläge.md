# Erweiterungsvorschläge für FileListControl

## 1. Spaltenkonfiguration erweitern

**Vorschlag:** Ein-/Ausblenden, Reihenfolge, benutzerdefinierte Spalten.
**Umsetzung:** Column-Descriptor-Modell mit Persistenz.

## 2. Dateivorschau

**Vorschlag:** Optionales Vorschaupanel für Bilder/Text/PDF-Metadaten.
**Umsetzung:** Ereignisbasiertes Preview-Provider-Interface.

## 3. Filter und Suche

**Vorschlag:** Live-Filter nach Name, Typ, Größe, Datum.
**Umsetzung:** In-Memory-Query oder serverseitige Provider-Schnittstelle.

## 4. Dateioperationen

**Vorschlag:** Copy/Move/Delete/Rename als optionales API.
**Umsetzung:** Kommandos mit Confirm-Callbacks und Fehlerkanal.

## 5. Watcher-basierte Aktualisierung

**Vorschlag:** Auto-Refresh bei Änderungen im Startordner.
**Umsetzung:** `FileSystemWatcher` mit Debounce und Teilupdates.