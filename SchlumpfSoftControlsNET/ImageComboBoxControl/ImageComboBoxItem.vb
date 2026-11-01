' --------------------------------------------------------------------------------------------------------
' Datei: ImageComboBoxItem.vb
' Author: Andreas Sauer
' Datum: 05.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing

Namespace ImageComboBoxControl

    ''' <summary>
    ''' Repräsentiert ein einzelnes Element für die <see cref="ImageComboBox"/>.
    ''' </summary>
    <Serializable>
    Public Class ImageComboBoxItem

        Implements IDisposable

        Private _value As String
        Private _image As Image
        Private _disposed As Boolean

        ''' <summary>
        ''' Ruft den anzuzeigenden Textwert des Elements ab oder legt ihn fest.
        ''' </summary>
        ''' <value>
        ''' Der Textwert des Elements.
        ''' </value>
        Public Property Value As String
            Get
                Return Me._value
            End Get
            Set(value As String)
                Me._value = value
            End Set
        End Property

        ''' <summary>
        ''' Ruft das dem Element zugeordnete Bild ab oder legt es fest.
        ''' </summary>
        ''' <value>
        ''' Das Bild des Elements.
        ''' </value>
        Public Property Image As Image
            Get
                Return Me._image
            End Get
            Set(value As Image)
                Me._image = value
            End Set
        End Property

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ImageComboBoxItem"/>.
        ''' </summary>
        Public Sub New()
            Me._value = String.Empty
            ' Ein minimales Platzhalterbild vermeidet Nullprüfungen in Zeichenroutinen.
            Me._image = New Bitmap(1, 1)
        End Sub

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ImageComboBoxItem"/> mit einem Textwert.
        ''' </summary>
        ''' <param name="value">
        ''' Der Textwert des Elements.
        ''' </param>
        Public Sub New(value As String)
            Me._value = value
            ' Ein minimales Platzhalterbild vereinfacht die Verarbeitung in OwnerDraw-Szenarien.
            Me._image = New Bitmap(1, 1)
        End Sub

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ImageComboBoxItem"/> mit Textwert und Bild.
        ''' </summary>
        ''' <param name="value">
        ''' Der Textwert des Elements.
        ''' </param>
        ''' <param name="image">
        ''' Das Bild des Elements.
        ''' </param>
        Public Sub New(value As String, image As Image)
            Me._value = value
            Me._image = image
        End Sub

        ''' <summary>
        ''' Gibt den Textwert des Elements zurück.
        ''' </summary>
        ''' <returns>
        ''' Den Textwert des Elements oder eine leere Zeichenfolge, wenn kein Wert gesetzt ist.
        ''' </returns>
        Public Overrides Function ToString() As String
            Return If(Me._value, String.Empty)
        End Function

        ''' <summary>
        ''' Gibt die Ressourcen frei, die von der <see cref="ImageComboBoxItem"/> -Instanz verwendet werden.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Gibt die Ressourcen frei, die von der <see cref="ImageComboBoxItem"/> -Instanz verwendet werden.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me._disposed Then
                If disposing Then
                    ' Verwaltete Ressourcen freigeben
                    Me._image?.Dispose()
                End If
                Me._disposed = True
            End If
        End Sub

    End Class

End Namespace