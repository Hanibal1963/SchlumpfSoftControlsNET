' --------------------------------------------------------------------------------------------------------
' Datei: ImageComboBoxCollectionEditor.vb
' Author: Andreas Sauer
' Datum: 05.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel.Design

Namespace ImageComboBoxControl

    ''' <summary>
    ''' Stellt den Designer-Editor für die <see cref="ImageComboBoxCollection(Of TComboBoxItem)"/> bereit.
    ''' </summary>
    Public Class ImageComboBoxCollectionEditor

        Inherits CollectionEditor

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ImageComboBoxCollectionEditor"/>.
        ''' </summary>
        Public Sub New()
            MyBase.New(GetType(ImageComboBoxCollection(Of ImageComboBoxItem)))
        End Sub

        ''' <summary>
        ''' Gibt den Typ der bearbeitbaren Sammlungselemente zurück.
        ''' </summary>
        ''' <returns>
        ''' Den Typ <see cref="ImageComboBoxItem"/>.
        ''' </returns>
        Protected Overrides Function CreateCollectionItemType() As System.Type
            Return GetType(ImageComboBoxItem)
        End Function

        ''' <summary>
        ''' Gibt die im Collection-Editor zulässigen neuen Elementtypen zurück.
        ''' </summary>
        ''' <returns>
        ''' Ein Array mit dem Typ <see cref="ImageComboBoxItem"/>.
        ''' </returns>
        Protected Overrides Function CreateNewItemTypes() As System.Type()
            Return New System.Type() {GetType(ImageComboBoxItem)}
        End Function

    End Class

End Namespace