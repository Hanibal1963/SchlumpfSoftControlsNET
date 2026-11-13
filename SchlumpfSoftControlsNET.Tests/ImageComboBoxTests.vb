Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.ImageComboBoxControl

Namespace SchlumpfSoftControlsNET.Tests.ImageComboBoxControl

    <TestClass>
    Public Class ImageComboBoxTests

        <TestMethod>
        Public Sub ImageComboBox_Constructor_SetsExpectedDefaults()
            Using control As New ImageComboBox()
                Assert.AreEqual(ComboBoxStyle.DropDownList, control.DropDownStyle)
                Assert.AreEqual(DrawMode.OwnerDrawVariable, control.DrawMode)
                Assert.IsNotNull(control.Elements)
                Assert.AreSame(control.Elements, control.Items)
                Assert.HasCount(0, control.Elements)
                Assert.AreEqual(0, BaseItemsCount(control))
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxItem_ConstructorsAndToString_WorkAsExpected()
            Using defaultItem As New ImageComboBoxItem(),
                  valueItem As New ImageComboBoxItem("Text"),
                  customImage As New Bitmap(3, 2),
                  imageItem As New ImageComboBoxItem("Bild", customImage)

                Assert.AreEqual(String.Empty, defaultItem.Value)
                Assert.IsNotNull(defaultItem.Image)
                Assert.AreEqual(1, defaultItem.Image.Width)
                Assert.AreEqual(1, defaultItem.Image.Height)

                Assert.AreEqual("Text", valueItem.Value)
                Assert.IsNotNull(valueItem.Image)
                Assert.AreEqual(1, valueItem.Image.Width)
                Assert.AreEqual(1, valueItem.Image.Height)

                Assert.AreEqual("Bild", imageItem.Value)
                Assert.AreSame(customImage, imageItem.Image)

                defaultItem.Value = Nothing
                Assert.AreEqual(String.Empty, defaultItem.ToString())

                valueItem.Value = "Neu"
                Assert.AreEqual("Neu", valueItem.ToString())
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxCollection_AddInsertAndIndexerSet_SynchronizeWithBaseItems()
            Using control As New ImageComboBox()
                Dim collection As ImageComboBoxCollection(Of ImageComboBoxItem) = control.Elements
                Dim first As New ImageComboBoxItem("A")
                Dim second As New ImageComboBoxItem("B")
                Dim inserted As New ImageComboBoxItem("X")
                Dim replacement As New ImageComboBoxItem("R")
                Dim eventCount As Integer

                AddHandler collection.UpdateItems, Sub(sender, e)
                                                       eventCount += 1
                                                   End Sub

                Dim firstIndex As Integer = collection.Add(first)
                Dim secondIndex As Integer = collection.Add(second)
                collection.Insert(1, inserted)
                collection(0) = replacement

                Assert.AreEqual(0, firstIndex)
                Assert.AreEqual(1, secondIndex)
                Assert.HasCount(3, collection)
                Assert.AreEqual(3, BaseItemsCount(control))
                Assert.AreEqual(4, eventCount)

                Assert.AreSame(replacement, collection(0))
                Assert.AreSame(inserted, collection(1))
                Assert.AreSame(second, collection(2))
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxCollection_Remove_RemoveAt_Clear_WorkAsExpected()
            Using control As New ImageComboBox()
                Dim collection As ImageComboBoxCollection(Of ImageComboBoxItem) = control.Elements
                Dim first As New ImageComboBoxItem("1")
                Dim second As New ImageComboBoxItem("2")
                Dim third As New ImageComboBoxItem("3")
                Dim missing As New ImageComboBoxItem("Fehlt")
                Dim eventCount As Integer

                Dim unused2 = collection.Add(first)
                Dim unused1 = collection.Add(second)
                Dim unused = collection.Add(third)

                AddHandler collection.UpdateItems, Sub(sender, e)
                                                       eventCount += 1
                                                   End Sub

                Dim removedExisting As Boolean = collection.Remove(second)
                Dim removedMissing As Boolean = collection.Remove(missing)
                collection.RemoveAt(0)
                collection.Clear()

                Assert.IsTrue(removedExisting)
                Assert.IsFalse(removedMissing)
                Assert.HasCount(0, collection)
                Assert.AreEqual(0, BaseItemsCount(control))
                Assert.AreEqual(3, eventCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxCollection_ContainsIndexOfCopyToAndEnumeration_WorkAsExpected()
            Using control As New ImageComboBox()
                Dim collection As ImageComboBoxCollection(Of ImageComboBoxItem) = control.Elements
                Dim first As New ImageComboBoxItem("Erstes")
                Dim second As New ImageComboBoxItem("Zweites")

                Dim unused1 = collection.Add(first)
                Dim unused = collection.Add(second)

                Assert.Contains(first, collection)
                Assert.AreEqual(0, collection.IndexOf(first))
                Assert.AreEqual(1, collection.IndexOf(second))
                Assert.IsFalse(collection.IsReadOnly)

                Dim copyTarget(2) As ImageComboBoxItem
                collection.CopyTo(copyTarget, 1)

                Assert.IsNull(copyTarget(0))
                Assert.AreSame(first, copyTarget(1))
                Assert.AreSame(second, copyTarget(2))

                Dim enumerated As New List(Of ImageComboBoxItem)
                For Each item As ImageComboBoxItem In collection
                    enumerated.Add(item)
                Next

                Assert.HasCount(2, enumerated)
                Assert.AreSame(first, enumerated(0))
                Assert.AreSame(second, enumerated(1))
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxCollection_ItemsBaseSetter_RebuildsTargetCollection()
            Using sourceControl As New ImageComboBox(),
                  targetCombo As New ComboBox()

                Dim collection As ImageComboBoxCollection(Of ImageComboBoxItem) = sourceControl.Elements
                Dim first As New ImageComboBoxItem("One")
                Dim second As New ImageComboBoxItem("Two")

                Dim unused1 = collection.Add(first)
                Dim unused = collection.Add(second)

                collection.ItemsBase = targetCombo.Items

                Assert.HasCount(2, targetCombo.Items)
                Assert.AreSame(first, DirectCast(targetCombo.Items(0), ImageComboBoxItem))
                Assert.AreSame(second, DirectCast(targetCombo.Items(1), ImageComboBoxItem))
            End Using
        End Sub

        <TestMethod>
        Public Sub ImageComboBoxCollectionEditor_ExposesExpectedTypes()
            Dim editor As New TestImageComboBoxCollectionEditor()

            Assert.AreEqual(GetType(ImageComboBoxItem), editor.ExposeCollectionItemType())

            Dim allowedTypes As Type() = editor.ExposeNewItemTypes()
            Assert.HasCount(1, allowedTypes)
            Assert.AreEqual(GetType(ImageComboBoxItem), allowedTypes(0))
        End Sub

        Private Shared Function BaseItemsCount(control As ImageComboBox) As Integer
            Return DirectCast(control, ComboBox).Items.Count
        End Function

        Private NotInheritable Class TestImageComboBoxCollectionEditor
            Inherits ImageComboBoxCollectionEditor

            Public Function ExposeCollectionItemType() As Type
                Return MyBase.CreateCollectionItemType()
            End Function

            Public Function ExposeNewItemTypes() As Type()
                Return MyBase.CreateNewItemTypes()
            End Function
        End Class

    End Class

End Namespace
