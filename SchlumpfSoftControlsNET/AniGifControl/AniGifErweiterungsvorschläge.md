# Erweiterungsvorschläge für AniGifControl

## 1. Playback-API erweitern

**Status:** Umgesetzt.

**Umsetzung:** `PauseAnimation()`, `ResumeAnimation()` und `SeekFrame(index)` verwenden einen internen Wiedergabezustand und steuern den Timer entsprechend.

## 2 Loop- und Segmentsteuerung

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.
**Umsetzung:** Framezähler mit optionalem Abbruch bzw. Segmentwiedergabe.

## 3 Eventmodell ausbauen

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.
**Umsetzung:** Ereignisse nach erfolgreichem Framewechsel bzw. Fehlerpfad auslösen.

## 4 Quellenvielfalt erhöhen

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.
**Umsetzung:** Überladene `LoadGif(...)`-Methoden mit einheitlicher Validierung.

## 5 Design-Time-Unterstützung

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.
**Umsetzung:** DesignerActionList ergänzen.
