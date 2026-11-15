# Erweiterungsvorschläge für AniGifControl

## 1. Playback-API erweitern

**Status:** &#x2705; Umgesetzt.

**Umsetzung:** `PauseAnimation()`, `ResumeAnimation()` und `SeekFrame(index)` verwenden einen internen Wiedergabezustand und steuern den Timer entsprechend.

## 2 Loop- und Segmentsteuerung

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.

**Umsetzung:** `LoopCount`, `PlayRangeStart` und `PlayRangeEnd` steuern die begrenzte oder endlose Segmentwiedergabe. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

## 3 Eventmodell ausbauen

**Status:** &#x1F4A1; offen

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.

**Umsetzung:** Ereignisse nach erfolgreichem Framewechsel bzw. Fehlerpfad auslösen.

## 4 Quellenvielfalt erhöhen

**Status:** &#x1F4A1; offen

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.

**Umsetzung:** Überladene `LoadGif(...)`-Methoden mit einheitlicher Validierung.

## 5 Design-Time-Unterstützung

**Status:** &#x1F4A1; offen

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.

**Umsetzung:** DesignerActionList ergänzen.
