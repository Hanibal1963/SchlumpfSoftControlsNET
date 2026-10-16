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

        ''' <summary>
        ''' Initialisiert das Steuerelement, aktiviert flimmerfreies Zeichnen und setzt die Standarddarstellung.
        ''' </summary>
        Public Sub New()

            ' Dieser Aufruf ist für den Designer erforderlich.
            Me.InitializeComponent()

            Me.DoubleBuffered = True
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
            Me.UpdateStyles()

            ' Standardwerte setzen
            Me.InitializeDefaults()

        End Sub

#Region "öffentliche Eigenschaften"

        ''' <summary>
        ''' Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest (Bereich: 0 bis
        ''' <see cref="ProgressMaximumValue"/> ).
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
                Me._ProgressValue = Math.Max(0, Math.Min(value, Me._MaxValue))
                Me.UpdateProgress()
            End Set
        End Property

        ''' <summary>
        ''' Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest.
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

                Me._MaxValue = boundedValue

                ' Falls der aktuelle Wert über dem neuen Maximum liegt, wird er automatisch reduziert.
                Me._ProgressValue = Math.Min(Me._ProgressValue, Me._MaxValue)
                Me.UpdateProgress()
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
                Me.ProgressFull.BackColor = Me._BarColor
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
                Me.ProgressEmpty.BackColor = Me._EmptyColor
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
                Me.UpdateProgress()
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
                Me.UpdateProgress()
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
        ''' Setzt die initialen Standardfarben und den Ausgangszustand der enthaltenen Panels.
        ''' </summary>
        Private Sub InitializeDefaults()

            Me.GlossLeft.BackColor = Color.FromArgb(100, 255, 255, 255)
            Me.GlossRight.BackColor = Color.FromArgb(100, 255, 255, 255)
            Me.BackColor = Me._BorderColor
            Me.ProgressEmpty.BackColor = Me._EmptyColor
            Me.ProgressFull.BackColor = Me._BarColor

        End Sub

        ''' <summary>
        ''' Aktualisiert die Höhe der Glanz-Overlays in Abhängigkeit von der aktuellen Steuerelementhöhe.
        ''' </summary>
        Private Sub UpdateGloss()

            ' Der Glanz belegt das obere Drittel der Höhe und skaliert dynamisch bei Größenänderungen.
            Dim glossHeight As Int32 = Math.Max(0, Me.Height \ 3)

            ' Die berechnete Höhe auf beide Glanzflächen anwenden.
            Me.GlossLeft.Height = glossHeight
            Me.GlossRight.Height = glossHeight

        End Sub

        ''' <summary>
        ''' Berechnet die sichtbare Fortschrittsbreite neu und aktualisiert Glanz- sowie Rahmendarstellung.
        ''' </summary>
        Private Sub UpdateProgress()

            ' Sicherheitsnetz: Maximalwert darf niemals 0 oder negativ sein.
            If Me._MaxValue <= 0 Then
                Me._MaxValue = 1
            End If

            ' Füllbreite proportional aus dem Verhältnis Value/Maximum berechnen.
            Dim fillWidth As Int32 = 0
            If Me._ProgressValue > 0 Then
                Dim ratio As Double = CDbl(Me._ProgressValue) / CDbl(Me._MaxValue)
                fillWidth = CInt(Math.Round(ratio * CDbl(Me.Width)))

                ' Ergebnis auf die Control-Breite begrenzen.
                fillWidth = Math.Max(0, Math.Min(Me.Width, fillWidth))
            End If

            Me.ProgressFull.Width = fillWidth

            ' Glanz-Overlays abhängig von IsGlossy ein-/ausblenden.
            If Me._IsGlossy Then
                Me.GlossLeft.Visible = True
                Me.GlossRight.Visible = True
            Else
                Me.GlossLeft.Visible = False
                Me.GlossRight.Visible = False
            End If

            ' Sonderfall: Bei vollem Fortschritt bis zur Innenkante (mit Rahmen) bzw. Vollbreite (ohne Rahmen) füllen.
            If Me._ProgressValue = Me._MaxValue Then
                Me.ProgressFull.Width = If(Me._ShowBorder, Me.Width - 2, Me.Width)
            End If

            ' Innenabstand steuert die Rahmendarstellung: 1px mit Rahmen, 0px ohne Rahmen.
            Dim expectedPadding As Padding = If(Me._ShowBorder, New Padding(1), New Padding(0))

            If Not MyBase.Padding.Equals(expectedPadding) Then
                MyBase.Padding = expectedPadding
            End If

        End Sub

        ''' <summary>
        ''' Erzwingt bei externen Padding-Änderungen den für die Rahmenoption korrekten Padding-Wert.
        ''' </summary>
        Private Sub ColorProgressBar_PaddingChanged(sender As Object, e As EventArgs) Handles Me.PaddingChanged

            Dim expectedPadding As Padding = If(Me._ShowBorder, New Padding(1), New Padding(0))

            If Not MyBase.Padding.Equals(expectedPadding) Then
                MyBase.Padding = expectedPadding
            End If

        End Sub

        ''' <summary>
        ''' Reagiert auf Größenänderungen und berechnet Fortschritt sowie Glanzdarstellung neu.
        ''' </summary>
        Private Sub ColorProgressBar_Resize(sender As Object, e As EventArgs) Handles Me.Resize

            If Me.Value <= Me._MaxValue Then
                Me.UpdateProgress()
                Me.UpdateGloss()
            End If

        End Sub

        ''' <summary>
        ''' Leitet Klicks von inneren Panels an das Steuerelement weiter, damit ein einheitliches Click-Ereignis
        ''' entsteht.
        ''' </summary>
        Private Sub Panel_Click(sender As Object, e As EventArgs) Handles GlossLeft.Click, GlossRight.Click, ProgressFull.Click, ProgressEmpty.Click
            Me.OnClick(e)
        End Sub

#End Region

    End Class

End Namespace
