# ImageComboBoxControl

Eine erweiterte ComboBox mit Symbolen.

Anregungen zu diesem Control hatte ich durch folgende Webseiten:

- [Bildsymbol im Eingabefeld der ComboBox anzeigen](https://www.vbarchiv.net/tipps/tipp_1948-bildsymbol-im-eingabefeld-der-combobox-anzeigen.html)
- [ComboBox mit Icons](https://www.vbarchiv.net/forum/read.php?id=22&t=96156&i=96156&v=f)
- [Combo-/ListBox mit ItemData-Eigenschaft erweitern](https://www.vbarchiv.net/tipps/tipp_1468-combo-listbox-mit-itemdata-eigenschaft-erweitern-net.html)
- [How to Display Images in ComboBox in 5 Minutes](https://web.archive.org/web/20120222054732/<http://www.codeproject.com/Articles/106467/How-to-Display-Images-in-ComboBox-in-5-Minutes>)
- [Image ComboBox Control](https://web.archive.org/web/20250819110747/<https://www.codeproject.com/Articles/10670/Image-ComboBox-Control>)

---

## Eigenschaften ImageComboBox

- `Items`: Ruft die Elemente der ComboBox ab.
- `Elements`: Ruft die designbare Elementekollektion der ComboBox ab.

## Methoden ImageComboBox

- `New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBox`.
- `CreateControlsInstance() As ControlCollection`: Erstellt die Steuerelementsammlung und stellt sicher, dass die Elementekollektion initialisiert ist.

## Ereignisse ImageComboBox

- Keine öffentlichen, spezifischen Ereignisse im Control dokumentiert.

### Eigenschaften - ImageComboBoxItem

- `Value`: Ruft den anzuzeigenden Textwert des Elements ab oder legt ihn fest.
- `Image`: Ruft das dem Element zugeordnete Bild ab oder legt es fest.

### Methoden - ImageComboBoxItem

- `New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem`.
- `New(value As String)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit einem Textwert.
- `New(value As String, image As System.Drawing.Image)`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxItem` mit Textwert und Bild.
- `ToString() As String`: Gibt den Textwert des Elements zurück.

### Ereignisse - ImageComboBoxItem

- Keine öffentlichen Ereignisse in `ImageComboBoxItem` dokumentiert.

### Eigenschaften - ImageComboBoxCollection

- `ItemsBase`: Ruft die zugrunde liegende `System.Windows.Forms.ComboBox.ObjectCollection` ab oder legt sie fest.
- `Item(index As Integer)`: Ruft das `ImageComboBoxItem` am angegebenen Index ab oder legt es fest.

### Methoden - ImageComboBoxCollection

- `Add(value As ImageComboBoxItem) As Integer`: Fügt der Kollektion ein Element hinzu und gibt den Index zurück.
- `IndexOf(value As ImageComboBoxItem) As Integer`: Ermittelt den Index eines bestimmten Elements in der Kollektion.
- `Insert(index As Integer, value As ImageComboBoxItem)`: Fügt ein Element an der angegebenen Position in die Kollektion ein.
- `Remove(value As ImageComboBoxItem)`: Entfernt das angegebene Element aus der Kollektion.
- `Clear()`: Entfernt alle Elemente aus der Kollektion.
- `Contains(value As ImageComboBoxItem) As Boolean`: Prüft, ob ein bestimmtes Element in der Kollektion enthalten ist.

### Ereignisse - ImageComboBoxCollection

- `UpdateItems`: Tritt auf, wenn sich die Elemente der Kollektion geändert haben.

### Eigenschaften - ImageComboBoxCollectionEditor

- Keine öffentlichen oder geschützten Eigenschaften in `ImageComboBoxCollectionEditor` dokumentiert.

### Methoden - ImageComboBoxCollectionEditor

- `New()`: Initialisiert eine neue Instanz der Klasse `ImageComboBoxCollectionEditor`.
- `CreateCollectionItemType() As Type`: Gibt den Typ der bearbeitbaren Sammlungselemente zurück.
- `CreateNewItemTypes() As Type()`: Gibt die im Collection-Editor zulässigen neuen Elementtypen zurück.

### Ereignisse - ImageComboBoxCollectionEditor

- Keine öffentlichen Ereignisse in `ImageComboBoxCollectionEditor` dokumentiert.
