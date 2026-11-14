' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBar.vb
' Author: Andreas Sauer
' Datum: 29.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing

Namespace ColorProgressBarControl

    ''' <summary>
    ''' Ein benutzerdefiniertes Windows Forms-Steuerelement zur Anzeige eines farbigen Fortschrittsbalkens mit
    ''' optionalem Rahmen und Glanzeffekt.
    ''' </summary>
    <Description("Control zum Anzeigen eines farbigen Fortschrittbalkens.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ColorProgressBar), "ColorProgressBarControl.ColorProgressBar.bmp")>
    Public Class ColorProgressBar

        Inherits UserControl

        Private ReadOnly _ProgressUnit As Integer = 20
        Private _ProgressValue As Integer = 1
        Private _MaxValue As Integer = 10
        Private _ShowBorder As Boolean = True
        Private _IsGlossy As Boolean = True
        Private _BarColor As System.Drawing.Color = Color.Blue
        Private _EmptyColor As System.Drawing.Color = Color.Silver
        Private _BorderColor As System.Drawing.Color = Color.Black
        Private _isGeometryCached As Boolean
        Private _cachedClientBounds As Rectangle
        Private _cachedControlSize As Size
        Private _cachedPadding As Padding
        Private _cachedProgressValue As Int32
        Private _cachedMaximumValue As Int32
        Private _cachedContentBounds As Rectangle
        Private _cachedFillBounds As Rectangle
        Private _cachedEmptyBounds As Rectangle
        Private _cachedGlossBounds As Rectangle

        ''' <summary>
        ''' Initialisiert das Steuerelement, aktiviert flimmerfreies Zeichnen und setzt die Standarddarstellung.
        ''' </summary>
        Public Sub New()

            ' Dieser Aufruf ist für den Designer erforderlich.
            Me.InitializeComponent()

            Me.DoubleBuffered = True
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.StandardClick, True)
            Me.UpdateStyles()

            ' Standardwerte setzen
            Me.InitializeDefaults()

        End Sub

#Region "öffentliche Eigenschaften"

        ''' <summary>
        ''' Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest (Bereich: 0 bis
        ''' <see cref="ProgressMaximumValue"/>). Werte außerhalb des Bereichs werden begrenzt.
        ''' Bei unverändertem begrenztem Wert wird keine erneute Invalidierung ausgelöst.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Gibt den Gesamtfortschritt des Fortschrittsbalkens zurück oder legt diesen fest.")>
        <DefaultValue(1)>
        Public Property Value() As Int32
            Get
                Return Me._ProgressValue
            End Get
            Set(value As Int32)
                ' Wert auf den gültigen Bereich begrenzen, damit keine ungültigen Zustände entstehen.
                Dim boundedValue As Int32 = Math.Max(0, Math.Min(value, Me._MaxValue))
                If boundedValue <> Me._ProgressValue Then
                    Me._ProgressValue = boundedValue
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest. Werte kleiner als 1
        ''' werden auf 1 begrenzt und ein darüber liegender Fortschrittswert wird angepasst.
        ''' Bei unverändertem begrenztem Maximum wird keine erneute Invalidierung ausgelöst.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest.")>
        <DefaultValue(10)>
        Public Property ProgressMaximumValue() As Int32
            Get
                Return Me._MaxValue
            End Get
            Set(value As Int32)
                ' Der Maximalwert darf nie kleiner als 1 sein (Vermeidung Division durch 0).
                Dim minValue As Int32 = 1
                Dim boundedValue As Int32 = Math.Max(minValue, value)

                If boundedValue <> Me._MaxValue Then
                    Me._MaxValue = boundedValue

                    ' Falls der aktuelle Wert über dem neuen Maximum liegt, wird er automatisch reduziert.
                    Me._ProgressValue = Math.Min(Me._ProgressValue, Me._MaxValue)
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt die Farbe des Fortschrittsbalkens zurück oder legt diese fest.")>
        <DefaultValue(GetType(Color), "Blue")>
        Public Property BarColor As Color
            Get
                Return Me._BarColor
            End Get
            Set(value As Color)
                Me._BarColor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt die Farbe des leeren Fortschrittsbalkens zurück oder legt diese fest.")>
        <DefaultValue(GetType(Color), "Silver")>
        Public Property EmptyColor As Color
            Get
                Return Me._EmptyColor
            End Get
            Set(value As Color)
                Me._EmptyColor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt die Farbe des Rahmens zurück oder legt diese fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt die Farbe des Rahmens zurück oder legt diese fest.")>
        <DefaultValue(GetType(Color), "Black")>
        Public Property BorderColor As Color
            Get
                Return Me._BorderColor
            End Get
            Set(value As Color)
                Me._BorderColor = value
                Me.BackColor = Me._BorderColor
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob ein Rahmen um die Fortschrittsanzeige angezeigt wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt an, ob der Rahmen auf der Fortschrittsanzeige aktiviert ist.")>
        <DefaultValue(True)>
        Public Property ShowBorder As Boolean
            Get
                Return Me._ShowBorder
            End Get
            Set(value As Boolean)
                Me._ShowBorder = value
                Me.UpdatePadding()
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob ein Glanzeffekt auf der Fortschrittsleiste angezeigt wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt an, ob der Glanz auf der Fortschrittsleiste angezeigt wird.")>
        <DefaultValue(True)>
        Public Property IsGlossy As Boolean
            Get
                Return Me._IsGlossy
            End Get
            Set(value As Boolean)
                Me._IsGlossy = value
                Me.Invalidate()
            End Set
        End Property

#End Region

#Region "ausgeblendete Eigenschaften"

        ''' <summary>
        ''' Ausgeblendet, da die Hintergrundfarbe intern als Rahmenfarbe verwendet wird.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackColor As Color
            Get
                Return MyBase.BackColor
            End Get
            Set(value As Color)
                MyBase.BackColor = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet, da das Steuerelement keine Hintergrundgrafik unterstützt.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImage As Image
            Get
                Return MyBase.BackgroundImage
            End Get
            Set(value As Image)
                MyBase.BackgroundImage = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet, da das Steuerelement keine Hintergrundgrafik unterstützt.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImageLayout As ImageLayout
            Get
                Return MyBase.BackgroundImageLayout
            End Get
            Set(value As ImageLayout)
                MyBase.BackgroundImageLayout = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet, da der Rahmen über <see cref="ShowBorder"/> und <see cref="BorderColor"/> gesteuert wird.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DefaultValue(GetType(BorderStyle), "None")>
        Public Shadows Property BorderStyle As BorderStyle
            Get
                Return MyBase.BorderStyle
            End Get
            Set(value As BorderStyle)
                MyBase.BorderStyle = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet, da dieses Steuerelement keine Vordergrundfarbe verwendet.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property ForeColor As Color
            Get
                Return MyBase.ForeColor
            End Get
            Set(value As Color)
                MyBase.ForeColor = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet, da das innere Padding intern zur Rahmendarstellung verwaltet wird.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DefaultValue(GetType(Padding), "0, 0, 0, 0")>
        Public Overloads Property Padding As Padding
            Get
                Return MyBase.Padding
            End Get
            Set(value As Padding)
                MyBase.Padding = value
            End Set
        End Property

#End Region

#Region "interne Methoden"

        ''' <summary>
        ''' Zeichnet den Fortschritt, den leeren Bereich und den optionalen Glanzeffekt.
        ''' </summary>
        ''' <param name="e">Stellt die Zeichenfläche und den zu aktualisierenden Bereich bereit.</param>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            Me.UpdateCachedGeometry()

            If Me._cachedFillBounds.Width > 0 AndAlso Me._cachedFillBounds.Height > 0 Then
                Using barBrush As New SolidBrush(Me._BarColor)
                    e.Graphics.FillRectangle(barBrush, Me._cachedFillBounds)
                End Using
            End If

            If Me._cachedEmptyBounds.Width > 0 AndAlso Me._cachedEmptyBounds.Height > 0 Then
                Using emptyBrush As New SolidBrush(Me._EmptyColor)
                    e.Graphics.FillRectangle(emptyBrush, Me._cachedEmptyBounds)
                End Using
            End If

            If Me._IsGlossy AndAlso Me._cachedGlossBounds.Width > 0 AndAlso Me._cachedGlossBounds.Height > 0 Then
                Using glossBrush As New SolidBrush(Color.FromArgb(100, 255, 255, 255))
                    e.Graphics.FillRectangle(glossBrush, Me._cachedGlossBounds)
                End Using
            End If

            MyBase.OnPaint(e)
        End Sub

        ''' <summary>
        ''' Aktualisiert die Zeichenrechtecke nur bei geänderter Geometrie oder geändertem Fortschritt.
        ''' </summary>
        Private Sub UpdateCachedGeometry()

            Dim clientBounds As Rectangle = Me.ClientRectangle
            Dim controlSize As Size = Me.Size
            Dim currentPadding As Padding = Me.Padding
            ' Auch die Clientfläche kann sich unabhängig von der äußeren Größe ändern.
            Dim layoutChanged As Boolean = Not Me._isGeometryCached OrElse
                Not clientBounds.Equals(Me._cachedClientBounds) OrElse
                Not controlSize.Equals(Me._cachedControlSize) OrElse
                Not currentPadding.Equals(Me._cachedPadding)

            If layoutChanged Then
                ' Das Padding lässt den von der Hintergrundfarbe gezeichneten Rahmen frei.
                Dim contentWidth As Int32 = Math.Max(0, clientBounds.Width - currentPadding.Horizontal)
                Dim contentHeight As Int32 = Math.Max(0, clientBounds.Height - currentPadding.Vertical)
                Dim contentX As Int32 = clientBounds.X + currentPadding.Left
                Dim contentY As Int32 = clientBounds.Y + currentPadding.Top
                Dim glossHeight As Int32 = Math.Min(contentHeight, controlSize.Height \ 3)

                Me._cachedContentBounds = New Rectangle(contentX, contentY, contentWidth, contentHeight)
                Me._cachedGlossBounds = New Rectangle(contentX, contentY, contentWidth, glossHeight)
                Me._cachedClientBounds = clientBounds
                Me._cachedControlSize = controlSize
                Me._cachedPadding = currentPadding
            End If

            If layoutChanged OrElse Me._cachedProgressValue <> Me._ProgressValue OrElse
                Me._cachedMaximumValue <> Me._MaxValue Then
                Dim fillWidth As Int32 = 0
                Dim contentWidth As Int32 = Me._cachedContentBounds.Width
                Dim contentHeight As Int32 = Me._cachedContentBounds.Height

                If contentWidth > 0 AndAlso contentHeight > 0 Then
                    ' Zwischenwerte verwenden weiterhin die Außenbreite; bei Maximum wird die Innenfläche gefüllt.
                    If Me._ProgressValue = Me._MaxValue Then
                        fillWidth = contentWidth
                    Else
                        Dim ratio As Double = CDbl(Me._ProgressValue) / CDbl(Me._MaxValue)
                        fillWidth = CInt(Math.Round(ratio * CDbl(controlSize.Width)))
                        fillWidth = Math.Max(0, Math.Min(contentWidth, fillWidth))
                    End If
                End If

                Me._cachedFillBounds = New Rectangle(Me._cachedContentBounds.X, Me._cachedContentBounds.Y, fillWidth, contentHeight)
                Me._cachedEmptyBounds = New Rectangle(Me._cachedContentBounds.X + fillWidth, Me._cachedContentBounds.Y,
                                                      contentWidth - fillWidth, contentHeight)
                Me._cachedProgressValue = Me._ProgressValue
                Me._cachedMaximumValue = Me._MaxValue
            End If

            Me._isGeometryCached = True
        End Sub

        ''' <summary>
        ''' Setzt die initiale Rahmenfarbe des Steuerelements.
        ''' </summary>
        Private Sub InitializeDefaults()

            Me.BackColor = Me._BorderColor

        End Sub

        ''' <summary>
        ''' Aktualisiert den Innenabstand entsprechend der Rahmenoption.
        ''' </summary>
        Private Sub UpdatePadding()

            Dim expectedPadding As Padding = If(Me._ShowBorder, New Padding(1), New Padding(0))

            If Not MyBase.Padding.Equals(expectedPadding) Then
                MyBase.Padding = expectedPadding
            End If

        End Sub

        ''' <summary>
        ''' Erzwingt bei externen Padding-Änderungen den für die Rahmenoption korrekten Padding-Wert.
        ''' </summary>
        Private Sub ColorProgressBar_PaddingChanged(sender As Object, e As EventArgs) Handles Me.PaddingChanged

            Me.UpdatePadding()

        End Sub

#End Region

    End Class

End Namespace
