' --------------------------------------------------------------------------------------------------------
' Datei: ListViewColumnComparer.vb
' Author: Andreas Sauer
' Datum: 30.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections
Imports System.Windows.Forms

Namespace FileListControl

    ''' <summary>
    ''' Vergleicht zwei <see cref="ListViewItem"/>-Objekte anhand einer definierten Spalte,
    ''' damit eine sortierte Anzeige im <see cref="ListView"/> möglich ist.
    ''' </summary>
    Friend NotInheritable Class ListViewColumnComparer

        Implements IComparer

#Region "Variablen"

        ''' <summary>
        ''' Index der Spalte, deren Textwerte für den Vergleich verwendet werden.
        ''' </summary>
        Private ReadOnly _column As Int32

        ''' <summary>
        ''' Gibt an, ob auf- oder absteigend sortiert wird.
        ''' </summary>
        Private ReadOnly _order As SortOrder

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert einen neuen Comparer mit Zielspalte und Sortierreihenfolge.
        ''' </summary>
        ''' <param name="column">Der Spaltenindex, nach dem verglichen werden soll.</param>
        ''' <param name="order">Die gewünschte Sortierreihenfolge.</param>
        Public Sub New(column As Int32, order As SortOrder)

            Me._column = column
            Me._order = order

        End Sub

        ''' <summary>
        ''' Vergleicht zwei Listeneinträge für den Sortiervorgang der ListView.
        ''' </summary>
        ''' <param name="x">Erstes Vergleichsobjekt (erwartet: <see cref="ListViewItem"/>).</param>
        ''' <param name="y">Zweites Vergleichsobjekt (erwartet: <see cref="ListViewItem"/>).</param>
        ''' <returns>
        ''' Kleiner 0, wenn <paramref name="x"/> vor <paramref name="y"/> sortiert wird;
        ''' größer 0 im umgekehrten Fall; 0 bei Gleichheit.
        ''' Bei <see cref="SortOrder.Descending"/> wird das Ergebnis invertiert.
        ''' </returns>
        Public Function Compare(x As Object, y As Object) As Int32 Implements IComparer.Compare

            ' Die übergebene IComparer-Signatur liefert Object; hier wird auf ListViewItem konkretisiert.
            Dim leftItem As ListViewItem = DirectCast(x, ListViewItem)
            Dim rightItem As ListViewItem = DirectCast(y, ListViewItem)

            ' Die zu vergleichenden Texte stammen aus derselben Zielspalte.
            Dim leftText As String = GetSubItemText(leftItem, Me._column)
            Dim rightText As String = GetSubItemText(rightItem, Me._column)

            ' Kulturabhängiger, nicht case-sensitiver Textvergleich für benutzerfreundliche Sortierung.
            Dim result As Int32 = StringComparer.CurrentCultureIgnoreCase.Compare(leftText, rightText)

            ' Gleichheit (0) bleibt unabhängig von der Sortierrichtung unverändert.
            ' Für absteigende Sortierung wird das Vergleichsergebnis invertiert.
            If Me._order = SortOrder.Descending Then
                result = -result
            End If

            Return result

        End Function

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Liefert den Text eines SubItems und schützt gegen ungültige Spaltenindizes.
        ''' </summary>
        ''' <param name="item">Der ListView-Eintrag, aus dem gelesen wird.</param>
        ''' <param name="index">Der gewünschte SubItem-Index.</param>
        ''' <returns>
        ''' Text des SubItems; bei ungültigem Index wird ein leerer String zurückgegeben.
        ''' </returns>
        Private Shared Function GetSubItemText(item As ListViewItem, index As Int32) As String

            ' Robuste Absicherung: Bei fehlender Spalte kein Fehler, sondern neutraler Vergleichswert.
            Return If(index < 0 OrElse index >= item.SubItems.Count, String.Empty, item.SubItems(index).Text)

        End Function

#End Region

    End Class

End Namespace

