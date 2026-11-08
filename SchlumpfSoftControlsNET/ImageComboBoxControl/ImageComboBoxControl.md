# ImageComboBoxControl

Eine erweiterte ComboBox mit Symbolen.

Anregungen zu diesem Control stammen unter anderem aus folgenden Webseiten:

- [Bildsymbol im Eingabefeld der ComboBox anzeigen](https://www.vbarchiv.net/tipps/tipp_1948-bildsymbol-im-eingabefeld-der-combobox-anzeigen.html)
- [ComboBox mit Icons](https://www.vbarchiv.net/forum/read.php?id=22&t=96156&i=96156&v=f)
- [Combo-/ListBox mit ItemData-Eigenschaft erweitern](https://www.vbarchiv.net/tipps/tipp_1468-combo-listbox-mit-itemdata-eigenschaft-erweitern-net.html)
- [How to Display Images in ComboBox in 5 Minutes](https://web.archive.org/web/20120222054732/<http://www.codeproject.com/Articles/106467/How-to-Display-Images-in-ComboBox-in-5-Minutes>)
- [Image ComboBox Control](https://web.archive.org/web/20250819110747/<https://www.codeproject.com/Articles/10670/Image-ComboBox-Control>)

## 1. Konstruktoren

- `ImageComboBox.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBox`.
- `ImageComboBoxItem.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem`.
- `ImageComboBoxItem.New(value As String)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit einem Textwert.
- `ImageComboBoxItem.New(value As String, image As System.Drawing.Image)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit Textwert und Bild.
- `ImageComboBoxCollectionEditor.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxCollectionEditor`.

## 2. Ereignisse

- `ImageComboBox`: Keine öffentlichen, spezifischen Ereignisse im Control dokumentiert.
- `ImageComboBoxItem`: Keine öffentlichen Ereignisse dokumentiert.
- `ImageComboBoxCollection.UpdateItems`: Tritt auf, wenn sich die Elemente der Kollektion geändert haben.
- `ImageComboBoxCollectionEditor`: Keine öffentlichen Ereignisse dokumentiert.

## 3. Eigenschaften

- `ImageComboBox.Items`: Ruft die Elemente der ComboBox ab.
- `ImageComboBox.Elements`: Ruft die designbare Elementekollektion der ComboBox ab.
- `ImageComboBoxItem.Value`: Ruft den anzuzeigenden Textwert des Elements ab oder legt ihn fest.
- `ImageComboBoxItem.Image`: Ruft das dem Element zugeordnete Bild ab oder legt es fest.
- `ImageComboBoxCollection.ItemsBase`: Ruft die zugrunde liegende `System.Windows.Forms.ComboBox.ObjectCollection` ab oder legt sie fest.
- `ImageComboBoxCollection.Item(index As Integer)`: Ruft das `ImageComboBoxItem` am angegebenen Index ab oder legt es fest.
- `ImageComboBoxCollectionEditor`: Keine öffentlichen oder geschützten Eigenschaften dokumentiert.

## 4. Funktionen

- `ImageComboBox.CreateControlsInstance() As ControlCollection`: Erstellt die Steuerelementsammlung und stellt sicher, dass die Elementekollektion initialisiert ist.
- `ImageComboBoxItem.ToString() As String`: Gibt den Textwert des Elements zurück.
- `ImageComboBoxCollection.Add(value As ImageComboBoxItem) As Integer`: Fügt der Kollektion ein Element hinzu und gibt den Index zurück.
- `ImageComboBoxCollection.IndexOf(value As ImageComboBoxItem) As Integer`: Ermittelt den Index eines bestimmten Elements in der Kollektion.
- `ImageComboBoxCollection.Insert(index As Integer, value As ImageComboBoxItem)`: Fügt ein Element an der angegebenen Position in die Kollektion ein.
- `ImageComboBoxCollection.Remove(value As ImageComboBoxItem)`: Entfernt das angegebene Element aus der Kollektion.
- `ImageComboBoxCollection.Clear()`: Entfernt alle Elemente aus der Kollektion.
- `ImageComboBoxCollection.Contains(value As ImageComboBoxItem) As Boolean`: Prüft, ob ein bestimmtes Element in der Kollektion enthalten ist.
- `ImageComboBoxCollectionEditor.CreateCollectionItemType() As Type`: Gibt den Typ der bearbeitbaren Sammlungselemente zurück.
- `ImageComboBoxCollectionEditor.CreateNewItemTypes() As Type()`: Gibt die im Collection-Editor zulässigen neuen Elementtypen zurück.

## 5. Hilfstypen/Strukturen

- `ImageComboBoxItem`: Datenträger für Textwert und optionales Bild.
- `ImageComboBoxCollection`: Verwaltungsklasse für `ImageComboBoxItem`-Elemente.
- `ImageComboBoxCollectionEditor`: Design-Time-Editor für die Elementekollektion.

## 6. Hinweise

- Die Dokumentation fasst mehrere öffentliche Typen (`ImageComboBox`, `ImageComboBoxItem`, `ImageComboBoxCollection`, `ImageComboBoxCollectionEditor`) in einer Datei zusammen.

## 7. Optimierungsvorschläge

### 7.1. Zielbild

Schnelle Darstellung vieler Einträge mit konsistenter Bildqualität.

### 7.2. Draw-/Measure-Logik vereinheitlichen

**Umsetzung:**

- Itemhöhe aus Font und Bildgröße zentral berechnen.

- Trunkierung/Alignment konsistent in DrawItem anwenden.

**Nutzen:** Vorhersagbares Layout bei unterschiedlichen DPI.

### 7.3. Bildhandling optimieren

**Umsetzung:**

- Thumbnails bzw. skalierte Caches pro DPI-Stufe erzeugen.

- Null-Bilder mit Platzhalter behandeln.

**Nutzen:** Weniger Rendering-Artefakte und bessere Performance.

### 7.4. Collectionsynchronisation härten

**Umsetzung:**

- `Elements` und `Items` bidirektional konsistent halten.

- Bei Batch-Änderungen Update-Events bündeln.

**Nutzen:** Verhindert Inkonsistenzen im Designer und zur Laufzeit.

### 7.5. Ressourcenbereinigung verbessern

**Umsetzung:**

- Eigene Image-Ressourcen optional durch Control entsorgen lassen (Ownership-Flag).

**Nutzen:** Reduziert Memory-Leaks bei dynamischen Bildern.

### 7.6. Keyboard/Accessibility optimieren

**Umsetzung:**

- Tastaturnavigation testen und ggf. nachziehen.

- AccessibleName/Description pro Item ableiten.

**Nutzen:** Höhere Usability und Barrierefreiheit.

### 7.7. Empfohlene Reihenfolge

1. Draw-/Measure-Konsistenz
2. Collection-Synchronisierung
3. Bildcache
4. Cleanup
5. Accessibility

## 8. Erweiterungsvorschläge

### 8.1. Datenbindung verbessern

**Vorschlag:** Starke Binding-Unterstützung (`DataSource`, `DisplayMember`, `ValueMember`) mit Bildzuordnung.
**Umsetzung:** Interne Mapping-Schicht zwischen Datenobjekt und `ImageComboBoxItem`.

### 8.2. Async-Bildladen

**Vorschlag:** Bilder aus Pfad/URL asynchron laden.
**Umsetzung:** Platzhalterbild, CancellationToken und Cache.

### 8.3. Gruppierte Einträge

**Vorschlag:** Kategorien/Separatoren in Dropdownliste.
**Umsetzung:** Virtuelle Gruppeneinträge mit eigener Zeichenlogik.

### 8.4. AutoComplete mit Symbolen

**Vorschlag:** Symbolunterstützte Vorschlagsliste beim Tippen.
**Umsetzung:** Benutzerdefiniertes Popup mit gefilterten Items.

### 8.5. Theme-Unterstützung

**Vorschlag:** Light/Dark-Renderingprofile.
**Umsetzung:** Theme-Objekt für Farben, Padding und Highlight.
