' --------------------------------------------------------------------------------------------------------
' Datei: PagesCollection.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Definiert die Auflistung der Seiten des Assistenten
    ''' </summary>
    Public Class PagesCollection

        Inherits CollectionBase
        Implements IList(Of WizardPage)
        Implements IReadOnlyList(Of WizardPage)

        Private ReadOnly _Owner As Wizard

        Default Public Property Item(index As Int32) As WizardPage Implements IList(Of WizardPage).Item, IReadOnlyList(Of WizardPage).Item
            Get
                Return CType(Me.List(index), WizardPage)
            End Get
            Set(value As WizardPage)
                Me.List(index) = value
            End Set
        End Property

        Private ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of WizardPage).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Private ReadOnly Property CountGeneric As Integer Implements ICollection(Of WizardPage).Count, IReadOnlyCollection(Of WizardPage).Count
            Get
                Return Me.List.Count
            End Get
        End Property

        Friend Sub New(Owner As Wizard)
            Me._Owner = Owner
        End Sub

        Public Function Add(Page As WizardPage) As Int32
            Return Me.List.Add(Page)
        End Function

        Public Sub AddRange(Pages As WizardPage())
            For Each value As WizardPage In Pages
                Dim unused = Me.Add(value)
            Next
        End Sub

        Public Function IndexOf(Page As WizardPage) As Int32 Implements IList(Of WizardPage).IndexOf
            Return Me.List.IndexOf(Page)
        End Function

        Public Sub Insert(index As Int32, Page As WizardPage) Implements IList(Of WizardPage).Insert
            Me.List.Insert(index, Page)
        End Sub

        Public Sub Remove(Page As WizardPage)
            Me.List.Remove(Page)
        End Sub

        Public Function Contains(Page As WizardPage) As Boolean Implements ICollection(Of WizardPage).Contains
            Return Me.List.Contains(Page)
        End Function

        Public Overloads Sub RemoveAt(index As Integer) Implements IList(Of WizardPage).RemoveAt
            Me.List.RemoveAt(index)
        End Sub

        Public Overloads Sub Clear() Implements ICollection(Of WizardPage).Clear
            Me.List.Clear()
        End Sub

        Private Sub AddGeneric(item As WizardPage) Implements ICollection(Of WizardPage).Add
            Me.Add(item)
        End Sub

        Private Function RemoveGeneric(item As WizardPage) As Boolean Implements ICollection(Of WizardPage).Remove
            If Me.List.Contains(item) Then
                Me.List.Remove(item)
                Return True
            End If

            Return False
        End Function

        Public Sub CopyTo(array() As WizardPage, arrayIndex As Integer) Implements ICollection(Of WizardPage).CopyTo
            For index As Integer = 0 To Me.List.Count - 1
                array(arrayIndex + index) = CType(Me.List(index), WizardPage)
            Next
        End Sub

        Private Iterator Function GetEnumeratorGeneric() As IEnumerator(Of WizardPage) Implements IEnumerable(Of WizardPage).GetEnumerator
            For Each page As WizardPage In Me.List
                Yield page
            Next
        End Function

        Protected Overrides Sub OnInsertComplete(index As Int32, value As Object)
            MyBase.OnInsertComplete(index, value)
            Me._Owner.SelectedIndex = index
        End Sub

        Protected Overrides Sub OnRemoveComplete(index As Int32, value As Object)
            MyBase.OnRemoveComplete(index, value)
            If Object.ReferenceEquals(Me._Owner.SelectedPage, value) Then
                If Me.InnerList.Count = 0 Then
                    Me._Owner.SelectedPage = Nothing
                Else
                    Me._Owner.SelectedIndex = If(index < Me.InnerList.Count, index, Me.InnerList.Count - 1)
                End If
            End If
        End Sub

    End Class

End Namespace