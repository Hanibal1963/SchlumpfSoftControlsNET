' --------------------------------------------------------------------------------------------------------
' Datei: ImageComboBoxCollection.vb
' Author: Andreas Sauer
' Datum: 05.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Windows.Forms

Namespace ImageComboBoxControl

    ''' <summary>
    ''' Stellt eine Kollektion von <see cref="ImageComboBoxItem"/>-Elementen bereit.
    ''' </summary>
    ''' <typeparam name="TComboBoxItem">
    ''' Der Typ der verwalteten ComboBox-Elemente.
    ''' </typeparam>
    Public Class ImageComboBoxCollection(Of TComboBoxItem As ImageComboBoxItem)

        Inherits CollectionBase

        Implements IList(Of TComboBoxItem)
        Implements IReadOnlyList(Of TComboBoxItem)

        Private _itemsBase As ComboBox.ObjectCollection

        ''' <summary>
        ''' Tritt auf, wenn sich die Elemente der Kollektion geändert haben.
        ''' </summary>
        Public Event UpdateItems As EventHandler


        ''' <summary>
        ''' Ruft die zugrunde liegende <see cref="ComboBox.ObjectCollection"/> ab oder legt sie fest.
        ''' </summary>
        ''' <value>
        ''' Die zugrunde liegende Objektkollektion der ComboBox.
        ''' </value>
        Public Property ItemsBase As ComboBox.ObjectCollection
            Get
                Return Me._itemsBase
            End Get
            Set(value As ComboBox.ObjectCollection)
                Me._itemsBase = value
                If Me._itemsBase IsNot Nothing Then
                    ' Die Ziel-Collection wird vollständig neu aufgebaut, um die Reihenfolge konsistent zu halten.
                    Me._itemsBase.Clear()
                    For Each item As TComboBoxItem In Me.List
                        Dim unused = Me._itemsBase.Add(item)
                    Next
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft das Element am angegebenen Index ab oder legt es fest.
        ''' </summary>
        ''' <param name="index">
        ''' Der nullbasierte Index des Elements.
        ''' </param>
        ''' <value>
        ''' Das <see cref="ImageComboBoxItem"/> am angegebenen Index.
        ''' </value>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Der angegebene Index liegt außerhalb des gültigen Bereichs.
        ''' </exception>
        Default Public Property Item(index As Int32) As TComboBoxItem Implements IList(Of TComboBoxItem).Item, IReadOnlyList(Of TComboBoxItem).Item
            Get
                Return CType(Me.List(index), TComboBoxItem)
            End Get
            Set(value As TComboBoxItem)
                Me.List(index) = value
                If Me._itemsBase IsNot Nothing Then
                    Me._itemsBase(index) = value
                End If
                RaiseEvent UpdateItems(Me, EventArgs.Empty)
            End Set
        End Property

        Public Shadows ReadOnly Property Count As Integer Implements ICollection(Of TComboBoxItem).Count, IReadOnlyCollection(Of TComboBoxItem).Count
            Get
                Return Me.List.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of TComboBoxItem).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Fügt der Kollektion ein Element hinzu.
        ''' </summary>
        ''' <param name="value">
        ''' Das hinzuzufügende Element.
        ''' </param>
        ''' <returns>
        ''' Der nullbasierte Index des hinzugefügten Elements.
        ''' </returns>
        Public Function Add(value As TComboBoxItem) As Int32
            Dim index As Int32 = Me.List.Add(value)

            ' Der Eintrag wird direkt in die Basis-Items übernommen, damit sofort korrekt gezeichnet wird.
            Me._itemsBase?.Add(value)

            RaiseEvent UpdateItems(Me, EventArgs.Empty)
            Return index
        End Function

        Private Sub AddItem(value As TComboBoxItem) Implements ICollection(Of TComboBoxItem).Add
            Dim unused As Int32 = Me.Add(value)
        End Sub

        ''' <summary>
        ''' Ermittelt den Index eines bestimmten Elements in der Kollektion.
        ''' </summary>
        ''' <param name="value">
        ''' Das zu suchende Element.
        ''' </param>
        ''' <returns>
        ''' Der nullbasierte Index des Elements oder -1, wenn das Element nicht gefunden wurde.
        ''' </returns>
        Public Function IndexOf(value As TComboBoxItem) As Int32 Implements IList(Of TComboBoxItem).IndexOf
            Return Me.List.IndexOf(value)
        End Function

        ''' <summary>
        ''' Fügt ein Element an der angegebenen Position in die Kollektion ein.
        ''' </summary>
        ''' <param name="index">
        ''' Der nullbasierte Index, an dem das Element eingefügt werden soll.
        ''' </param>
        ''' <param name="value">
        ''' Das einzufügende Element.
        ''' </param>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Der angegebene Index liegt außerhalb des gültigen Bereichs.
        ''' </exception>
        Public Sub Insert(index As Int32, value As TComboBoxItem) Implements IList(Of TComboBoxItem).Insert
            Me.List.Insert(index, value)
            Me._itemsBase?.Insert(index, value)
            RaiseEvent UpdateItems(Me, EventArgs.Empty)
        End Sub

        Public Overloads Sub RemoveAt(index As Int32) Implements IList(Of TComboBoxItem).RemoveAt
            Me.List.RemoveAt(index)
            Me._itemsBase?.RemoveAt(index)
            RaiseEvent UpdateItems(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Entfernt das angegebene Element aus der Kollektion.
        ''' </summary>
        ''' <param name="value">
        ''' Das zu entfernende Element.
        ''' </param>
        Public Function Remove(value As TComboBoxItem) As Boolean Implements ICollection(Of TComboBoxItem).Remove
            Dim index As Int32 = Me.List.IndexOf(value)
            If index < 0 Then
                ' Früher Rückgabepunkt vermeidet unnötige Aktualisierung bei nicht vorhandenem Element.
                Return False
            End If

            Me.List.RemoveAt(index)
            Me._itemsBase?.RemoveAt(index)
            RaiseEvent UpdateItems(Me, EventArgs.Empty)
            Return True
        End Function

        ''' <summary>
        ''' Entfernt alle Elemente aus der Kollektion.
        ''' </summary>
        Public Overloads Sub Clear() Implements ICollection(Of TComboBoxItem).Clear
            Me.List.Clear()
            Me._itemsBase?.Clear()
            RaiseEvent UpdateItems(Me, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Prüft, ob ein bestimmtes Element in der Kollektion enthalten ist.
        ''' </summary>
        ''' <param name="value">
        ''' Das zu suchende Element.
        ''' </param>
        ''' <returns>
        ''' <c>True</c>, wenn das Element enthalten ist, andernfalls <c>False</c>.
        ''' </returns>
        Public Function Contains(value As TComboBoxItem) As Boolean Implements ICollection(Of TComboBoxItem).Contains
            Return Me.List.Contains(value)
        End Function

        Public Sub CopyTo(array() As TComboBoxItem, arrayIndex As Int32) Implements ICollection(Of TComboBoxItem).CopyTo
            For i As Int32 = 0 To Me.List.Count - 1
                array(arrayIndex + i) = CType(Me.List(i), TComboBoxItem)
            Next
        End Sub

        Private Iterator Function GetGenericEnumerator() As IEnumerator(Of TComboBoxItem) Implements IEnumerable(Of TComboBoxItem).GetEnumerator
            For Each item As TComboBoxItem In Me.List
                Yield item
            Next
        End Function

    End Class

End Namespace
