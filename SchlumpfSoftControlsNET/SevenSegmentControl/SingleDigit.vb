' --------------------------------------------------------------------------------------------------------
' Datei: SingleDigit.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing

Namespace SevenSegmentControl

    ''' <summary>
    ''' <para>Dieses Steuerelement stellt ein einzelnes Siebensegment-LED-Display dar, </para>
    ''' <para>das eine Ziffer oder einen Buchstaben anzeigt.</para>
    ''' </summary>
    <Description("Dieses Steuerelement stellt ein einzelnes Siebensegment-LED-Display dar, das eine Ziffer oder einen Buchstaben anzeigt.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(SingleDigit), "SevenSegmentControl.SingleDigit.bmp")>
    Public Class SingleDigit

        Inherits Control

#Region "Variablen"

        Private ReadOnly _SegmentPoints As Point()()  ' Sammlung der Eckpunkte für jedes der 7 Segmente (jedes Segment als Polygon mit 6 Punkten).
        Private ReadOnly _DigitHeight As Int32 = 80 ' Interne, fixe Höhe (virtuell) der Ziffer für die Berechnung der Segmentkoordinaten.
        Private ReadOnly _DigitWidth As Int32 = 48 ' Interne, fixe Breite (virtuell) der Ziffer für die Berechnung der Segmentkoordinaten.

        Private _SegmentWidth As Int32          ' Aktuelle Segmentbreite (Dicke der LED-Balken) in Pixeln.
        Private _ItalicFactor As Single         ' Scherfaktor zur Erzeugung einer kursiven Darstellung (negativ neigt nach links).
        Private _BackgroundColor As Color       ' Zwischengespeicherte Hintergrundfarbe des Steuerelements.
        Private _InactiveColor As Color         ' Farbe für inaktive (nicht leuchtende) Segmente.
        Private _ForeColor As Color             ' Vordergrundfarbe für aktive (leuchtende) Segmente.
        Private _DigitValue As String           ' Zu darstellender Zeichenwert (Ziffer/Buchstabe/Sonderzeichen) als String.
        Private _ShowDecimalPoint As Boolean    ' Steuert, ob der Dezimalpunkt gezeichnet wird.
        Private _DecimalPointActive As Boolean  ' Status des Dezimalpunktes (aktiv = leuchtend).
        Private _ShowColon As Boolean           ' Steuert, ob der Doppelpunkt (zwei Punkte) gezeichnet wird.
        Private _ColonActive As Boolean         ' Status des Doppelpunkts (aktiv = beide Punkte leuchten).
        Private _CustomBitPattern As Int32      ' Bitmaske für die 7 Segmente (Bit0..Bit6); ermöglicht benutzerdefinierte Muster.

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.
        ''' </summary>
        <Category("Appearance")>
        <Description("Legt die Farbe inaktiver Segmente fest oder gibt diese zurück.")>
        <DefaultValue(GetType(Color), "DarkGray")>
        Public Property InactiveColor As Color
            Get
                Return Me._InactiveColor
            End Get
            Set(value As Color)
                Me._InactiveColor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Breite der LED-Segmente fest oder gibt diese zurück.
        ''' </summary>
        <Category("Appearance")>
        <Description("Legt die Breite der LED-Segmente fest oder gibt diese zurück.")>
        <DefaultValue(10)>
        Public Property SegmentWidth As Int32
            Get
                Return Me._SegmentWidth
            End Get
            Set(value As Int32)
                Me._SegmentWidth = value
                Me.CalculatePoints(Me._SegmentPoints, Me._DigitHeight, Me._DigitWidth, Me._SegmentWidth)
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Scherkoeffizient für die Kursivschrift der Anzeige.
        ''' </summary>
        ''' <remarks>
        ''' Standardwert ist -0,1.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Scherkoeffizient für die Kursivschrift der Anzeige.")>
        <DefaultValue(-0.1)>
        Public Property ItalicFactor As Single
            Get
                Return Me._ItalicFactor
            End Get
            Set(value As Single)
                Me._ItalicFactor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt das anzuzeigende Zeichen fest oder gibt dieses zurück.
        ''' </summary>
        ''' <remarks>
        ''' Unterstützte Zeichen sind Ziffern und die meisten Buchstaben.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Legt das anzuzeigende Zeichen fest oder gibt dieses zurück.")>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
        Public Property DigitValue As String
            Get
                Return Me._DigitValue
            End Get
            Set(value As String)
                Me._CustomBitPattern = 0
                Me._DigitValue = value
                Me.Invalidate()
                If Equals(value, Nothing) OrElse value.Length = 0 Then
                    Return
                End If
                Dim tempValue As Int32
                If Int32.TryParse(value, tempValue) Then
                    If tempValue > 9 Then tempValue = 9
                    If tempValue < 0 Then tempValue = 0
                    'ist es eine ganze Zahl?
                    Select Case tempValue
                        Case 0 : Me._CustomBitPattern = CharacterPattern.Zero
                        Case 1 : Me._CustomBitPattern = CharacterPattern.One
                        Case 2 : Me._CustomBitPattern = CharacterPattern.Two
                        Case 3 : Me._CustomBitPattern = CharacterPattern.Three
                        Case 4 : Me._CustomBitPattern = CharacterPattern.Four
                        Case 5 : Me._CustomBitPattern = CharacterPattern.Five
                        Case 6 : Me._CustomBitPattern = CharacterPattern.Six
                        Case 7 : Me._CustomBitPattern = CharacterPattern.Seven
                        Case 8 : Me._CustomBitPattern = CharacterPattern.Eight
                        Case 9 : Me._CustomBitPattern = CharacterPattern.Nine
                    End Select
                Else
                    'ist es ein Buchstabe?
                    Select Case value(0)
                        Case "A"c, "a"c : Me._CustomBitPattern = CharacterPattern.A
                        Case "B"c, "b"c : Me._CustomBitPattern = CharacterPattern.B
                        Case "C"c : Me._CustomBitPattern = CharacterPattern.C
                        Case "c"c : Me._CustomBitPattern = CharacterPattern.cField
                        Case "D"c, "d"c : Me._CustomBitPattern = CharacterPattern.D
                        Case "E"c, "e"c : Me._CustomBitPattern = CharacterPattern.E
                        Case "F"c, "f"c : Me._CustomBitPattern = CharacterPattern.F
                        Case "G"c, "g"c : Me._CustomBitPattern = CharacterPattern.G
                        Case "H"c : Me._CustomBitPattern = CharacterPattern.H
                        Case "h"c : Me._CustomBitPattern = CharacterPattern.hField
                        Case "I"c : Me._CustomBitPattern = CharacterPattern.One
                        Case "i"c : Me._CustomBitPattern = CharacterPattern.i
                        Case "J"c, "j"c : Me._CustomBitPattern = CharacterPattern.J
                        Case "L"c, "l"c : Me._CustomBitPattern = CharacterPattern.L
                        Case "N"c, "n"c : Me._CustomBitPattern = CharacterPattern.N
                        Case "O"c : Me._CustomBitPattern = CharacterPattern.Zero
                        Case "o"c : Me._CustomBitPattern = CharacterPattern.o
                        Case "P"c, "p"c : Me._CustomBitPattern = CharacterPattern.P
                        Case "Q"c, "q"c : Me._CustomBitPattern = CharacterPattern.Q
                        Case "R"c, "r"c : Me._CustomBitPattern = CharacterPattern.R
                        Case "S"c, "s"c : Me._CustomBitPattern = CharacterPattern.Five
                        Case "T"c, "t"c : Me._CustomBitPattern = CharacterPattern.T
                        Case "U"c : Me._CustomBitPattern = CharacterPattern.U
                        Case "u"c, "µ"c, "μ"c : Me._CustomBitPattern = CharacterPattern.uField
                        Case "Y"c, "y"c : Me._CustomBitPattern = CharacterPattern.Y
                        Case "-"c : Me._CustomBitPattern = CharacterPattern.Dash
                        Case "="c : Me._CustomBitPattern = CharacterPattern.Equals
                        Case "°"c : Me._CustomBitPattern = CharacterPattern.Degrees
                        Case "'"c : Me._CustomBitPattern = CharacterPattern.Apostrophe
                        Case """"c : Me._CustomBitPattern = CharacterPattern.Quote
                        Case "["c, "{"c : Me._CustomBitPattern = CharacterPattern.C
                        Case "]"c, "}"c : Me._CustomBitPattern = CharacterPattern.RBracket
                        Case "_"c : Me._CustomBitPattern = CharacterPattern.Underscore
                        Case "≡"c : Me._CustomBitPattern = CharacterPattern.Identical
                        Case "¬"c : Me._CustomBitPattern = CharacterPattern.Not
                    End Select
                End If
            End Set
        End Property

        ''' <summary>
        ''' <para>Legt ein benutzerdefiniertes Bitmuster fest, das in den sieben Segmenten angezeigt werden soll.</para>
        ''' <para>Dies ist ein ganzzahliger Wert, bei dem die Bits 0 bis 6 den jeweiligen LED-Segmenten entsprechen.</para>
        ''' </summary>
        <Category("Appearance")>
        <Description("Legt ein benutzerdefiniertes Bitmuster fest, das in den sieben Segmenten angezeigt werden soll.")>
        <DefaultValue(0)>
        Public Property CustomBitPattern As Int32
            Get
                Return Me._CustomBitPattern
            End Get
            Set(value As Int32)
                Me._CustomBitPattern = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob die Dezimalpunkt-LED angezeigt wird.
        ''' </summary>
        <Category("Appearance")>
        <Description("Gibt an, ob die Dezimalpunkt-LED angezeigt wird.")>
        <DefaultValue(True)>
        Public Property ShowDecimalPoint As Boolean
            Get
                Return Me._ShowDecimalPoint
            End Get
            Set(value As Boolean)
                Me._ShowDecimalPoint = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob die Dezimalpunkt-LED aktiv ist.
        ''' </summary>
        <Category("Appearance")>
        <Description("Gibt an, ob die Dezimalpunkt-LED aktiv ist.")>
        <DefaultValue(True)>
        Public Property DecimalPointActive As Boolean
            Get
                Return Me._DecimalPointActive
            End Get
            Set(value As Boolean)
                Me._DecimalPointActive = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob die Doppelpunkt-LEDs angezeigt werden.
        ''' </summary>
        <Category("Appearance")>
        <Description("Gibt an, ob die Doppelpunkt-LEDs angezeigt werden.")>
        <DefaultValue(True)>
        Public Property ShowColon As Boolean
            Get
                Return Me._ShowColon
            End Get
            Set(value As Boolean)
                Me._ShowColon = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt an, ob die Doppelpunkt-LEDs aktiv sind.
        ''' </summary>
        <Category("Appearance")>
        <Description("Gibt an, ob die Doppelpunkt-LEDs aktiv sind.")>
        <DefaultValue(True)>
        Public Property ColonActive As Boolean
            Get
                Return Me._ColonActive
            End Get
            Set(value As Boolean)
                Me._ColonActive = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
        ''' </summary>
        ''' <returns>Aktuelle Hintergrundfarbe.</returns>
        <Category("Appearance")>
        <Description("Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.")>
        Public Overrides Property BackColor As Color
            Get
                Return Me._BackgroundColor
            End Get
            Set(value As Color)
                Me._BackgroundColor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.
        ''' </summary>
        ''' <returns>Aktuelle Segment-Vordergrundfarbe.</returns>
        <Category("Appearance")>
        <Description("Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.")>
        Public Overrides Property ForeColor As Color
            Get
                Return Me._ForeColor
            End Get
            Set(value As Color)
                Me._ForeColor = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da nicht relevant für die Funktion der Anzeige.
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
        ''' Ausgeblendet da nicht relevant für die Funktion der Anzeige.
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
        ''' Ausgeblendet da nicht relevant für die Funktion der Anzeige.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Font As Font
            Get
                Return MyBase.Font
            End Get
            Set(value As Font)
                MyBase.Font = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da nicht relevant für die Funktion der Anzeige.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da nicht relevant für die Funktion der Anzeige.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property RightToLeft As RightToLeft
            Get
                Return MyBase.RightToLeft
            End Get
            Set(value As RightToLeft)
                MyBase.RightToLeft = value
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="SingleDigit"/> -Klasse.
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()
            Me.SuspendLayout()
            Me.InitializeDefaults()
            Me.Name = "SevSegSingleDigit"
            Me.Size = New Size(32, 64)
            Me.TabStop = False
            Me.Padding = New Padding(10, 4, 10, 4)
            MyBase.DoubleBuffered = True
            Me._SegmentPoints = New Point(6)() {}
            For i = 0 To 6
                Me._SegmentPoints(i) = New Point(5) {}
            Next
            Me.CalculatePoints(Me._SegmentPoints, Me._DigitHeight, Me._DigitWidth, Me._SegmentWidth)
            Me.ResumeLayout(False)
        End Sub

#End Region

#Region "Interne Methoden"

        Private Sub InitializeDefaults()
            Me._SegmentWidth = 10 ' Aktuelle Segmentbreite (Dicke der LED-Balken) in Pixeln.
            Me._ItalicFactor = -0.1F ' Scherfaktor zur Erzeugung einer kursiven Darstellung (negativ neigt nach links).
            Me._BackgroundColor = SystemColors.Control ' Zwischengespeicherte Hintergrundfarbe des Steuerelements.
            Me._InactiveColor = Color.DarkGray ' Farbe für inaktive (nicht leuchtende) Segmente.
            Me._ForeColor = Color.DarkGreen  ' Vordergrundfarbe für aktive (leuchtende) Segmente.
            Me._DigitValue = Nothing ' Zu darstellender Zeichenwert (Ziffer/Buchstabe/Sonderzeichen) als String.
            Me._ShowDecimalPoint = True ' Steuert, ob der Dezimalpunkt gezeichnet wird.
            Me._DecimalPointActive = False ' Status des Dezimalpunktes (aktiv = leuchtend).
            Me._ShowColon = False ' Steuert, ob der Doppelpunkt (zwei Punkte) gezeichnet wird.
            Me._ColonActive = False  ' Status des Doppelpunkts (aktiv = beide Punkte leuchten).
            Me._CustomBitPattern = 0 ' Bitmaske für die 7 Segmente (Bit0..Bit6); ermöglicht benutzerdefinierte Muster.
        End Sub

        ''' <summary>
        ''' Rendert das gesamte Siebensegment-Digit inklusive Segmenten, optionalem Dezimalpunkt und optionalem Doppelpunkt.
        ''' </summary>
        ''' <param name="sender">Quelle des Paint-Ereignisses.</param>
        ''' <param name="e">Paint-Ereignisdaten mit Grafikobjekt.</param>
        Private Sub SevSegsingleDigit_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
            Dim useValue = Me._CustomBitPattern
            Dim brushLight As Brush = New SolidBrush(Me._ForeColor)
            Dim brushDark As Brush = New SolidBrush(Me._InactiveColor)
            ' Definiert den Quellbereich für das virtuelle Koordinatensystem.
            Dim srcRect As RectangleF
            Dim colonWidth As Int32 = CInt(Me._DigitWidth / 4)
            srcRect = If(Me._ShowColon,
                    New RectangleF(0.0F, 0.0F, Me._DigitWidth + colonWidth, Me._DigitHeight),
                    New RectangleF(0.0F, 0.0F, Me._DigitWidth, Me._DigitHeight))
            Dim destRect As New RectangleF(Me.Padding.Left, Me.Padding.Top, Me.Width - Me.Padding.Left - Me.Padding.Right, Me.Height - Me.Padding.Top - Me.Padding.Bottom)
            ' Grafikcontainer, der die virtuellen Koordinaten auf den verfügbaren Zielbereich abbildet.
            Dim containerState = e.Graphics.BeginContainer(destRect, srcRect, GraphicsUnit.Pixel)
            Dim trans As New Drawing2D.Matrix()
            trans.Shear(Me._ItalicFactor, 0.0F)
            e.Graphics.Transform = trans
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Default
            ' Segmente entsprechend der Bitmaske zeichnen.
            PaintSegments(e, useValue, brushLight, brushDark, Me._SegmentPoints)
            If Me._ShowDecimalPoint Then
                e.Graphics.FillEllipse(If(Me._DecimalPointActive, brushLight, brushDark), Me._DigitWidth - 1, Me._DigitHeight - Me._SegmentWidth + 1, Me._SegmentWidth, Me._SegmentWidth)
            End If
            If Me._ShowColon Then
                e.Graphics.FillEllipse(If(Me._ColonActive, brushLight, brushDark), Me._DigitWidth + colonWidth - 4, CInt((Me._DigitHeight / 4) - Me._SegmentWidth + 8), Me._SegmentWidth, Me._SegmentWidth)
                e.Graphics.FillEllipse(If(Me._ColonActive, brushLight, brushDark), Me._DigitWidth + colonWidth - 4, CInt((Me._DigitHeight * 3 / 4) - Me._SegmentWidth + 4), Me._SegmentWidth, Me._SegmentWidth)
            End If
            e.Graphics.EndContainer(containerState)
        End Sub

        ''' <summary>
        ''' Zeichnet alle sieben Segmente auf Basis des übergebenen Bitmusters.
        ''' </summary>
        ''' <param name="e">Paint-Ereignisdaten mit Grafikobjekt.</param>
        ''' <param name="BitPattern">Bitmaske für Segmentzustände (Bit 0 bis Bit 6).</param>
        ''' <param name="BrushLight">Pinsel für aktive Segmente.</param>
        ''' <param name="BrushDark">Pinsel für inaktive Segmente.</param>
        ''' <param name="SegmentPoints">Polygonpunkte für alle Segmente.</param>
        Private Shared Sub PaintSegments(e As PaintEventArgs, BitPattern As Int32, BrushLight As Brush, BrushDark As Brush, ByRef SegmentPoints As Point()())
            e.Graphics.FillPolygon(If((BitPattern And &H1) = &H1, BrushLight, BrushDark), SegmentPoints(0))
            e.Graphics.FillPolygon(If((BitPattern And &H2) = &H2, BrushLight, BrushDark), SegmentPoints(1))
            e.Graphics.FillPolygon(If((BitPattern And &H4) = &H4, BrushLight, BrushDark), SegmentPoints(2))
            e.Graphics.FillPolygon(If((BitPattern And &H8) = &H8, BrushLight, BrushDark), SegmentPoints(3))
            e.Graphics.FillPolygon(If((BitPattern And &H10) = &H10, BrushLight, BrushDark), SegmentPoints(4))
            e.Graphics.FillPolygon(If((BitPattern And &H20) = &H20, BrushLight, BrushDark), SegmentPoints(5))
            e.Graphics.FillPolygon(If((BitPattern And &H40) = &H40, BrushLight, BrushDark), SegmentPoints(6))
        End Sub

        ''' <summary>
        ''' Berechnet die Polygonpunkte für alle Segmente im virtuellen Koordinatensystem der Anzeige.
        ''' </summary>
        ''' <param name="SegmentCornerPoints">Zielarray mit den Segment-Polygonen.</param>
        ''' <param name="DigitHeight">Virtuelle Höhe der Anzeige.</param>
        ''' <param name="DigitWidth">Virtuelle Breite der Anzeige.</param>
        ''' <param name="SegmentWidth">Dicke der einzelnen Segmente.</param>
        Private Sub CalculatePoints(ByRef SegmentCornerPoints As Point()(), DigitHeight As Int32, DigitWidth As Int32, SegmentWidth As Int32)
            Dim halfHeight As Int32 = CInt(DigitHeight / 2)
            Dim halfWidth As Int32 = CInt(SegmentWidth / 2)
            Dim p = 0
            ' Segment 0 (oben)
            SegmentCornerPoints(p)(0).X = SegmentWidth + 1
            SegmentCornerPoints(p)(0).Y = 0
            SegmentCornerPoints(p)(1).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(1).Y = 0
            SegmentCornerPoints(p)(2).X = DigitWidth - halfWidth - 1
            SegmentCornerPoints(p)(2).Y = halfWidth
            SegmentCornerPoints(p)(3).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(3).Y = SegmentWidth
            SegmentCornerPoints(p)(4).X = SegmentWidth + 1
            SegmentCornerPoints(p)(4).Y = SegmentWidth
            SegmentCornerPoints(p)(5).X = halfWidth + 1
            SegmentCornerPoints(p)(5).Y = halfWidth
            p += 1
            ' Segment 1 (oben links)
            SegmentCornerPoints(p)(0).X = 0
            SegmentCornerPoints(p)(0).Y = SegmentWidth + 1
            SegmentCornerPoints(p)(1).X = halfWidth
            SegmentCornerPoints(p)(1).Y = halfWidth + 1
            SegmentCornerPoints(p)(2).X = SegmentWidth
            SegmentCornerPoints(p)(2).Y = SegmentWidth + 1
            SegmentCornerPoints(p)(3).X = SegmentWidth
            SegmentCornerPoints(p)(3).Y = halfHeight - halfWidth - 1
            SegmentCornerPoints(p)(4).X = 4
            SegmentCornerPoints(p)(4).Y = halfHeight - 1
            SegmentCornerPoints(p)(5).X = 0
            SegmentCornerPoints(p)(5).Y = halfHeight - 1
            p += 1
            ' Segment 2 (oben rechts)
            SegmentCornerPoints(p)(0).X = DigitWidth - SegmentWidth
            SegmentCornerPoints(p)(0).Y = SegmentWidth + 1
            SegmentCornerPoints(p)(1).X = DigitWidth - halfWidth
            SegmentCornerPoints(p)(1).Y = halfWidth + 1
            SegmentCornerPoints(p)(2).X = DigitWidth
            SegmentCornerPoints(p)(2).Y = SegmentWidth + 1
            SegmentCornerPoints(p)(3).X = DigitWidth
            SegmentCornerPoints(p)(3).Y = halfHeight - 1
            SegmentCornerPoints(p)(4).X = DigitWidth - 4
            SegmentCornerPoints(p)(4).Y = halfHeight - 1
            SegmentCornerPoints(p)(5).X = DigitWidth - SegmentWidth
            SegmentCornerPoints(p)(5).Y = halfHeight - halfWidth - 1
            p += 1
            ' Segment 3 (Mitte)
            SegmentCornerPoints(p)(0).X = SegmentWidth + 1
            SegmentCornerPoints(p)(0).Y = halfHeight - halfWidth
            SegmentCornerPoints(p)(1).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(1).Y = halfHeight - halfWidth
            SegmentCornerPoints(p)(2).X = DigitWidth - 5
            SegmentCornerPoints(p)(2).Y = halfHeight
            SegmentCornerPoints(p)(3).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(3).Y = halfHeight + halfWidth
            SegmentCornerPoints(p)(4).X = SegmentWidth + 1
            SegmentCornerPoints(p)(4).Y = halfHeight + halfWidth
            SegmentCornerPoints(p)(5).X = 5
            SegmentCornerPoints(p)(5).Y = halfHeight
            p += 1
            ' Segment 4 (unten links)
            SegmentCornerPoints(p)(0).X = 0
            SegmentCornerPoints(p)(0).Y = halfHeight + 1
            SegmentCornerPoints(p)(1).X = 4
            SegmentCornerPoints(p)(1).Y = halfHeight + 1
            SegmentCornerPoints(p)(2).X = SegmentWidth
            SegmentCornerPoints(p)(2).Y = halfHeight + halfWidth + 1
            SegmentCornerPoints(p)(3).X = SegmentWidth
            SegmentCornerPoints(p)(3).Y = DigitHeight - SegmentWidth - 1
            SegmentCornerPoints(p)(4).X = halfWidth
            SegmentCornerPoints(p)(4).Y = DigitHeight - halfWidth - 1
            SegmentCornerPoints(p)(5).X = 0
            SegmentCornerPoints(p)(5).Y = DigitHeight - SegmentWidth - 1
            p += 1
            ' Segment 5 (unten rechts)
            SegmentCornerPoints(p)(0).X = DigitWidth - SegmentWidth
            SegmentCornerPoints(p)(0).Y = halfHeight + halfWidth + 1
            SegmentCornerPoints(p)(1).X = DigitWidth - 4
            SegmentCornerPoints(p)(1).Y = halfHeight + 1
            SegmentCornerPoints(p)(2).X = DigitWidth
            SegmentCornerPoints(p)(2).Y = halfHeight + 1
            SegmentCornerPoints(p)(3).X = DigitWidth
            SegmentCornerPoints(p)(3).Y = DigitHeight - SegmentWidth - 1
            SegmentCornerPoints(p)(4).X = DigitWidth - halfWidth
            SegmentCornerPoints(p)(4).Y = DigitHeight - halfWidth - 1
            SegmentCornerPoints(p)(5).X = DigitWidth - SegmentWidth
            SegmentCornerPoints(p)(5).Y = DigitHeight - SegmentWidth - 1
            p += 1
            ' Segment 6 (unten)
            SegmentCornerPoints(p)(0).X = SegmentWidth + 1
            SegmentCornerPoints(p)(0).Y = DigitHeight - SegmentWidth
            SegmentCornerPoints(p)(1).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(1).Y = DigitHeight - SegmentWidth
            SegmentCornerPoints(p)(2).X = DigitWidth - halfWidth - 1
            SegmentCornerPoints(p)(2).Y = DigitHeight - halfWidth
            SegmentCornerPoints(p)(3).X = DigitWidth - SegmentWidth - 1
            SegmentCornerPoints(p)(3).Y = DigitHeight
            SegmentCornerPoints(p)(4).X = SegmentWidth + 1
            SegmentCornerPoints(p)(4).Y = DigitHeight
            SegmentCornerPoints(p)(5).X = halfWidth + 1
            SegmentCornerPoints(p)(5).Y = DigitHeight - halfWidth
        End Sub

        ''' <summary>
        ''' Reagiert auf Größenänderungen und fordert ein Neuzeichnen an.
        ''' </summary>
        ''' <param name="sender">Quelle des Ereignisses.</param>
        ''' <param name="e">Ereignisdaten der Größenänderung.</param>
        Private Sub SevSegSingleDigit_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Reagiert auf Änderungen des Innenabstands und fordert ein Neuzeichnen an.
        ''' </summary>
        ''' <param name="e">Ereignisdaten der Padding-Änderung.</param>
        Protected Overrides Sub OnPaddingChanged(e As EventArgs)
            MyBase.OnPaddingChanged(e)
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Zeichnet den Hintergrund in der konfigurierten Hintergrundfarbe.
        ''' </summary>
        ''' <param name="pevent">Paint-Ereignisdaten für das Hintergrundzeichnen.</param>
        Protected Overrides Sub OnPaintBackground(pevent As PaintEventArgs)
            'MyBase.OnPaintBackground(e)
            pevent.Graphics.Clear(Me._BackgroundColor)
        End Sub

        ''' <summary>
        ''' <para>Gibt nicht verwaltete Ressourcen frei und führt weitere Bereinigungsvorgänge durch, </para>
        ''' <para>bevor <see cref="SingleDigit"/> durch die Garbage Collection
        ''' zurückgefordert wird.</para>
        ''' </summary>
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub

#End Region

    End Class

End Namespace
