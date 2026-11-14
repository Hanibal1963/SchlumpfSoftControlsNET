# Erweiterungsvorschläge für den ExtendedRTFControl

## 1. Undo/Redo-Transaktionen

**Vorschlag:** Batch-Formatierungen als eine Undo-Operation gruppieren.
**Umsetzung:** Native Nachrichten oder internes Kommandomodell kapseln.

## 2. Stilvorlagen

**Vorschlag:** Wiederverwendbare Textstile (`Heading`, `Code`, `Note`).
**Umsetzung:** Style-Objekte definieren und auf Selektion anwenden.

## 3. Einfüge-APIs erweitern

**Vorschlag:** `InsertImage`, `InsertLink`, `InsertTableLikeText`.
**Umsetzung:** RTF-Snippet-Builder mit Validierung.

## 4. Exportoptionen

**Vorschlag:** Export nach PlainText/HTML/Markdown (Basis).
**Umsetzung:** Adapter-Layer je Zielformat.

## 5. Ereignisse für Editorintegration

**Vorschlag:** `FormattingChanged`, `SelectionStyleChanged`.
**Umsetzung:** Bestehende `SelectionChanged`-Ableitung mit Debounce ergänzen.