# Erweiterungsvorschläge für AniGifControl

## 1. Playback-API erweitern

**Status:** &#x2705; Umgesetzt.

**Umsetzung:** `PauseAnimation()`, `ResumeAnimation()` und `SeekFrame(index)` verwenden einen internen Wiedergabezustand und steuern den Timer entsprechend.

## 2 Loop- und Segmentsteuerung

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Eigenschaften `LoopCount`, `PlayRangeStart`, `PlayRangeEnd`.

**Umsetzung:** `LoopCount`, `PlayRangeStart` und `PlayRangeEnd` steuern die begrenzte oder endlose Segmentwiedergabe. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

## 3 Eventmodell ausbauen

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** `FrameChanged`, `PlaybackCompleted`, `PlaybackError`.

**Umsetzung:** Ereignisse melden Framewechsel, den regulären Abschluss begrenzter Wiedergaben und Fehler bei der Frameauswahl. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

## 4 Quellenvielfalt erhöhen

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Laden aus Stream/ByteArray/Dateipfad.

**Umsetzung:** `LoadGif(...)` lädt GIF-Daten aus Stream, Bytearray oder Datei. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).

## 5 Design-Time-Unterstützung

**Status:** &#x2705; Umgesetzt.

**Vorschlag:** Smart-Tag für AutoPlay/Speed/SizeMode.

**Umsetzung:** Eine `DesignerActionList` stellt `AutoPlay`, `CustomDisplaySpeed`, `FramesPerSecond` und `GifSizeMode` im klassischen Designer und im Out-of-Process-Designer für .NET 8 bis .NET 10 bereit. Einzelheiten stehen in [AniGifControl.md](AniGifControl.md).
