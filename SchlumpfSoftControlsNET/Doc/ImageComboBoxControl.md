# ImageComboBoxControl

Eine erweiterte ComboBox mit Symbolen.

Anregungen zu diesem Control stammen unter anderem aus folgenden Webseiten:

- [Bildsymbol im Eingabefeld der ComboBox anzeigen](https://www.vbarchiv.net/tipps/tipp_1948-bildsymbol-im-eingabefeld-der-combobox-anzeigen.html)
- [ComboBox mit Icons](https://www.vbarchiv.net/forum/read.php?id=22&t=96156&i=96156&v=f)
- [Combo-/ListBox mit ItemData-Eigenschaft erweitern](https://www.vbarchiv.net/tipps/tipp_1468-combo-listbox-mit-itemdata-eigenschaft-erweitern-net.html)
- [How to Display Images in ComboBox in 5 Minutes](https://web.archive.org/web/20120222054732/<http://www.codeproject.com/Articles/106467/How-to-Display-Images-in-ComboBox-in-5-Minutes>)
- [Image ComboBox Control](https://web.archive.org/web/20250819110747/<https://www.codeproject.com/Articles/10670/Image-ComboBox-Control>)

## Konstruktoren

- `ImageComboBox.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBox`.
- `ImageComboBoxItem.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem`.
- `ImageComboBoxItem.New(value As String)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit einem Textwert.
- `ImageComboBoxItem.New(value As String, image As System.Drawing.Image)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit Textwert und Bild.
- `ImageComboBoxCollectionEditor.New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxCollectionEditor`.

## Ereignisse

- `ImageComboBox`: Keine öffentlichen, spezifischen Ereignisse im Control dokumentiert.
- `ImageComboBoxItem`: Keine öffentlichen Ereignisse dokumentiert.
- `ImageComboBoxCollection.UpdateItems`: Tritt auf, wenn sich die Elemente der Kollektion geändert haben.
- `ImageComboBoxCollectionEditor`: Keine öffentlichen Ereignisse dokumentiert.

## Eigenschaften

- `ImageComboBox.Items`: Ruft die Elemente der ComboBox ab.
- `ImageComboBox.Elements`: Ruft die designbare Elementekollektion der ComboBox ab.
- `ImageComboBoxItem.Value`: Ruft den anzuzeigenden Textwert des Elements ab oder legt ihn fest.
- `ImageComboBoxItem.Image`: Ruft das dem Element zugeordnete Bild ab oder legt es fest.
- `ImageComboBoxCollection.ItemsBase`: Ruft die zugrunde liegende `System.Windows.Forms.ComboBox.ObjectCollection` ab oder legt sie fest.
- `ImageComboBoxCollection.Item(index As Integer)`: Ruft das `ImageComboBoxItem` am angegebenen Index ab oder legt es fest.
- `ImageComboBoxCollectionEditor`: Keine öffentlichen oder geschützten Eigenschaften dokumentiert.

## Funktionen

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

## Hilfstypen/Strukturen

- `ImageComboBoxItem`: Datenträger für Textwert und optionales Bild.
- `ImageComboBoxCollection`: Verwaltungsklasse für `ImageComboBoxItem`-Elemente.
- `ImageComboBoxCollectionEditor`: Design-Time-Editor für die Elementekollektion.

## Hinweise

- Die Dokumentation fasst mehrere öffentliche Typen (`ImageComboBox`, `ImageComboBoxItem`, `ImageComboBoxCollection`, `ImageComboBoxCollectionEditor`) in einer Datei zusammen.

## Optimierungs und Verbesserungsmöglichkeiten

- [Optimierungsvorschläge](ImageComboBoxControlOptimirungsvorschläge.md)
- [Erweiterungsvorschläge](ImageComboBoxControlErweiterunsvorschläge.md)
