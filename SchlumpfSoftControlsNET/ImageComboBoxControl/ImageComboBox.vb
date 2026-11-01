' --------------------------------------------------------------------------------------------------------
' Datei: ImageComboBox.vb
' Author: Andreas Sauer
' Datum: 05.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace ImageComboBoxControl

    ''' <summary>
    ''' Eine erweiterte ComboBox mit Symbolen.
    ''' </summary>
    <Description("Eine erweiterte ComboBox mit Symbolen.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ImageComboBox), "ImageComboBoxControl.ImageComboBox.bmp")>
    Public Class ImageComboBox

        Inherits ComboBox

        Private _items As ImageComboBoxCollection(Of ImageComboBoxItem)

        ''' <summary>
        ''' Ruft die Elemente der ComboBox ab.
        ''' </summary>
        ''' <value>
        ''' Die Kollektion der Elemente.
        ''' </value>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows ReadOnly Property Items As ImageComboBoxCollection(Of ImageComboBoxItem)
            Get
                Return Me.Elements
            End Get
        End Property

        ''' <summary>
        ''' Ruft die designbare Elementekollektion der ComboBox ab.
        ''' </summary>
        ''' <value>
        ''' Die Elemente der ComboBox.
        ''' </value>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        <Editor(GetType(ImageComboBoxCollectionEditor), GetType(UITypeEditor))>
        <MergableProperty(False)>
        <Description("Die Elemente der ComboBox.")>
        <Category("Data")>
        Public ReadOnly Property Elements As ImageComboBoxCollection(Of ImageComboBoxItem)
            Get
                If Me._items Is Nothing Then
                    Me.InitializeItemsCollection()
                End If

                Return Me._items
            End Get
        End Property

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ImageComboBox"/>.
        ''' </summary>
        Public Sub New()

            Me.InitializeComponent()
            Me.DropDownStyle = ComboBoxStyle.DropDownList
            ' OwnerDrawVariable ist erforderlich, damit Bild und Text pro Eintrag gemeinsam gezeichnet werden können.
            Me.DrawMode = DrawMode.OwnerDrawVariable

            Me.InitializeItemsCollection()

            AddHandler DrawItem, AddressOf Me.ComboBoxDrawItemEvent
            AddHandler MeasureItem, AddressOf Me.ComboBoxMeasureItem

        End Sub

        ''' <summary>
        ''' Erstellt die Steuerelementsammlung und stellt sicher, dass die Elementekollektion initialisiert ist.
        ''' </summary>
        ''' <returns>
        ''' Die neu erstellte <see cref="ControlCollection"/>.
        ''' </returns>
        Protected Overrides Function CreateControlsInstance() As ControlCollection

            Dim result As ControlCollection = MyBase.CreateControlsInstance()
            Me.InitializeItemsCollection()
            Return result

        End Function

        ''' <summary>
        ''' Initialisiert die Elementekollektion der ComboBox.
        ''' </summary>
        Private Sub InitializeItemsCollection()

            If Me._items Is Nothing Then
                Me._items = New ImageComboBoxCollection(Of ImageComboBoxItem)()
                AddHandler Me._items.UpdateItems, AddressOf Me.UpdateItems
            End If

            ' Die Basis-Items werden bei jeder Initialisierung neu zugewiesen, damit Designer und Laufzeit synchron bleiben.
            Me._items.ItemsBase = MyBase.Items

        End Sub

        ''' <summary>
        ''' Aktualisiert die Anzeige der Elemente in der ComboBox.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub UpdateItems(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Ermittelt die Höhe und Breite der Einträge in der ComboBox, um die Anzeige korrekt zu gestalten.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ComboBoxMeasureItem(sender As Object, e As MeasureItemEventArgs)

            Using g As Graphics = Me.CreateGraphics()

                Dim maxWidth As Int32 = 0

                ' Die maximale Breite wird vorab ermittelt, damit Einträge mit Bild nicht abgeschnitten werden.
                For Each comboItem As ImageComboBoxItem In Me.Elements

                    Dim itemText As String = String.Empty
                    Dim imageWidth As Int32 = 0

                    If comboItem IsNot Nothing Then
                        itemText = If(comboItem.Value, String.Empty)

                        If comboItem.Image IsNot Nothing Then
                            imageWidth = Me.ItemHeight
                        End If
                    End If

                    Dim textWidth As Int32 = CInt(g.MeasureString(itemText, Me.Font).Width)
                    Dim totalWidth As Int32 = imageWidth + textWidth + 8

                    If totalWidth > maxWidth Then
                        maxWidth = totalWidth
                    End If

                Next

                Me.DropDownWidth = Math.Max(maxWidth, Me.Width)
                e.ItemHeight = Me.ItemHeight

            End Using

        End Sub

        ''' <summary>
        ''' Zeichnet die Einträge in der ComboBox mit Bild und Text.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ComboBoxDrawItemEvent(sender As Object, e As DrawItemEventArgs)

            e.DrawBackground()

            If e.Index >= 0 AndAlso e.Index < Me.Elements.Count Then
                Dim comboboxItem As ImageComboBoxItem = Me.Elements(e.Index)
                If comboboxItem IsNot Nothing Then

                    Dim imageWidth As Int32 = Me.ItemHeight
                    Dim textLeft As Int32 = e.Bounds.X

                    If comboboxItem.Image IsNot Nothing Then

                        e.Graphics.DrawImage(comboboxItem.Image, e.Bounds.X, e.Bounds.Y, imageWidth, Me.ItemHeight)
                        ' Text wird nach rechts verschoben, damit Bild und Beschriftung nicht überlappen.
                        textLeft += imageWidth

                    End If

                    Dim text As String = If(comboboxItem.Value, String.Empty)

                    e.Graphics.DrawString(text, Me.Font, Brushes.Black,
                                          New RectangleF(textLeft, e.Bounds.Y, Me.DropDownWidth, Me.ItemHeight))
                End If

            End If

            e.DrawFocusRectangle()

        End Sub

    End Class

End Namespace