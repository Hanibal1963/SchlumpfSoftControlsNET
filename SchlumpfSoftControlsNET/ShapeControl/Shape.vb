' --------------------------------------------------------------------------------------------------------
' Datei: Shape.vb
' Author: Andreas Sauer
' Datum: 05.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace ShapeControl

    ''' <summary>
    ''' Steuerelement zum Darstellen von Linien, Rechtecken und Ellipsen (gefüllt oder ungefüllt).
    ''' </summary>
    <Description("Steuerelement zum Darstellen von Linien, Rechtecken und Ellipsen (gefüllt oder ungefüllt).")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(Shape), "ShapeControl.Shape.bmp")>
    Public Class Shape

        Inherits Control

#Region "Variablen"

        Private _ShapeModus As ShapeModes ' Speichert den aktuell gesetzten Modus (Formtyp), der gezeichnet werden soll.
        Private _LineWidth As Single ' Speichert die Linienbreite für Linie oder Rahmen.
        Private _LineColor As Color ' Speichert die Farbe der Linie oder Rahmenlinie.
        Private _FillColor As Color ' Speichert die Füllfarbe für Rechteck oder Ellipse, sofern gefüllte Formen gewählt wurden.
        Private _DiagonalLineModus As DiagonalLineModes ' Speichert die Richtung der diagonalen Linie.

#End Region

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Legt die anzuzeigende Form fest oder gibt diese zurück.
        ''' </summary>
        ''' <value>
        ''' Ein Wert aus <see cref="ShapeModes"/>, der die Form bestimmt.
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt die anzuzeigende Form fest oder gibt diese zurück.")>
        <DefaultValue(ShapeModes.HorizontalLine)>
        Public Property ShapeModus() As ShapeModes
            Get
                Return Me._ShapeModus
            End Get
            Set(value As ShapeModes)
                If Me._ShapeModus = value Then
                    Return
                End If

                Me._ShapeModus = value
                ' Durch das Neuerstellen des Handles wird das Steuerelement mit dem neuen Modus neu gezeichnet.
                Me.RecreateHandle()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Breite der Linie oder Rahmenlinie fest oder gibt diese zurück.
        ''' </summary>
        ''' <value>
        ''' Die Breite der Linie in Pixeln.
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt die Breite der Linie oder Rahmenlinie fest oder gibt diese zurück.")>
        <DefaultValue(2.0F)>
        Public Property LineWidth() As Single
            Get
                Return Me._LineWidth
            End Get
            Set(value As Single)
                ' Eine Linienbreite kleiner 1 Pixel ist für die Darstellung nicht sinnvoll.
                If value < 1.0F Then
                    value = 1.0F
                End If

                If Me._LineWidth = value Then
                    Return
                End If

                Me._LineWidth = value
                ' Geänderte Linienbreite erfordert eine Neuzeichnung des Steuerelements.
                Me.RecreateHandle()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Farbe der Linie oder Rahmenlinie fest oder gibt diese zurück.
        ''' </summary>
        ''' <value>
        ''' Eine <see cref="Color"/> Instanz für die Linienfarbe.
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt die Farbe der Linie oder Rahmenlinie fest oder gibt diese zurück.")>
        <DefaultValue(GetType(Color), "Black")>
        Public Property LineColor() As Color
            Get
                Return Me._LineColor
            End Get
            Set(value As Color)
                If Me._LineColor = value Then
                    Return
                End If

                Me._LineColor = value
                Me.RecreateHandle()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Füllfarbe für die Form fest oder gibt diese zurück.
        ''' </summary>
        ''' <value>
        ''' Eine <see cref="Color"/> Instanz für die Füllung.
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt die Füllfarbe für die Form fest oder gibt diese zurück.")>
        <DefaultValue(GetType(Color), "Gray")>
        Public Property FillColor() As Color
            Get
                Return Me._FillColor
            End Get
            Set(value As Color)
                If Me._FillColor = value Then
                    Return
                End If

                Me._FillColor = value
                Me.RecreateHandle()
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob eine diagonale Linie von links oben nach rechts unten oder umgekehrt verläuft, oder gibt
        ''' diesen Wert zurück.
        ''' </summary>
        ''' <value>
        ''' Ein Wert aus <see cref="DiagonalLineModes"/> zur Bestimmung der Richtung.
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt fest ob eine diagonale Linie von links oben nach rechts unten oder umgekehrt verläuft oder gibt dies zurück.")>
        <DefaultValue(GetType(DiagonalLineModes), "TopLeftToBottomRight")>
        Public Property DiagonalLineModus() As DiagonalLineModes
            Get
                Return Me._DiagonalLineModus
            End Get
            Set(value As DiagonalLineModes)
                If Me._DiagonalLineModus = value Then
                    Return
                End If

                Me._DiagonalLineModus = value
                Me.RecreateHandle()
            End Set
        End Property

        ''' <summary>
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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

        ''' <summary>
        ''' Wird im Designer ausgeblendet, da diese Eigenschaft für dieses Steuerelement nicht relevant ist.
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

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="Shape"/>.
        ''' </summary>
        ''' <remarks>
        ''' Richtet Standardwerte und Zeichenstile ein.
        ''' </remarks>
        Public Sub New()
            ' Dieser Aufruf ist für den Designer erforderlich.
            Me.InitializeComponent()
            Me.InitializeStyles()
            ' Eigene Standardwerte und Zeichenstile werden nach der Designer-Initialisierung gesetzt.
            Me.InitializeDefaults()
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Initialisiert die Standardwerte des Steuerelements.
        ''' </summary>
        Private Sub InitializeDefaults()
            Me._ShapeModus = ShapeModes.HorizontalLine ' Standardform: horizontale Linie
            Me._DiagonalLineModus = DiagonalLineModes.TopLeftToBottomRight ' Standardrichtung der diagonalen Linie
            Me._LineColor = Color.Black ' Standardfarbe für Linien und Rahmen
            Me._LineWidth = 2 ' Standard-Linienbreite in Pixeln
            Me._FillColor = Color.Transparent ' Standardfüllung für gefüllte Formen
        End Sub

        ''' <summary>
        ''' Initialisiert die Zeichenstile des Steuerelements.
        ''' </summary>
        Private Sub InitializeStyles()

            Me.SetStyle(ControlStyles.Opaque, True) ' Steuerung übernimmt die komplette Hintergrund-/Vordergrunddarstellung
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, False) ' Kein DoubleBuffer wegen Transparenzverhalten (WS_EX_TRANSPARENT)

        End Sub

        ''' <summary>
        ''' Zeichnet die aktuell gewählte Form anhand der gesetzten Eigenschaften.
        ''' </summary>
        ''' <param name="e">Enthält die Grafikoberfläche und Zustandsinformationen für den Zeichenvorgang.</param>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)
            Dim g As Graphics = e.Graphics ' Zeichenoberfläche des Steuerelements

            ' Maße werden mit Max(0) abgesichert, damit bei kleinen Control-Größen keine negativen Werte entstehen.
            Dim halfLineWidth As Single = Me._LineWidth / 2.0F ' Halbierte Linienbreite für zentrierte Rahmenzeichnung
            Dim rectWidth As Single = Math.Max(0.0F, Me.Width - Me._LineWidth) ' Außenmaß für Rahmen
            Dim rectHeight As Single = Math.Max(0.0F, Me.Height - Me._LineWidth) ' Außenmaß für Rahmen
            Dim fillWidth As Single = Math.Max(0.0F, Me.Width - (2.0F * Me._LineWidth)) ' Innenmaß für Füllung
            Dim fillHeight As Single = Math.Max(0.0F, Me.Height - (2.0F * Me._LineWidth)) ' Innenmaß für Füllung

            Using pen As New Pen(Me._LineColor, Me._LineWidth) ' Für Linien und Umrandungen

                Using brush As New SolidBrush(Me._FillColor) ' Für gefüllte Formen

                    Select Case Me._ShapeModus
                        Case ShapeModes.HorizontalLine  ' horizontale Linie zeichnen (mittig im Rahmen des Controls)
                            g.DrawLine(pen, 0, CInt(Me.Height / 2), Me.Width, CInt(Me.Height / 2))

                        Case ShapeModes.VerticalLine ' vertikale Linie zeichnen (mittig im Rahmen des Controls)
                            g.DrawLine(pen, CInt(Me.Width / 2), 0, CInt(Me.Width / 2), Me.Height)

                        Case ShapeModes.DiagonalLine ' diagonale Linie zeichnen

                            Select Case Me._DiagonalLineModus ' konkrete Richtung der Diagonale auswerten
                                Case DiagonalLineModes.BottomLeftToTopRight
                                    g.DrawLine(pen, 0, Me.Height, Me.Width, 0)

                                Case DiagonalLineModes.TopLeftToBottomRight   ' von links oben nach rechts unten
                                    g.DrawLine(pen, 0, 0, Me.Width, Me.Height)

                            End Select

                        Case ShapeModes.Rectangle ' einfaches Rechteck zeichnen
                            g.DrawRectangle(pen, halfLineWidth, halfLineWidth, rectWidth, rectHeight)

                        Case ShapeModes.FilledRectangle  ' einfaches Rechteck zeichnen und ausfüllen
                            g.DrawRectangle(pen, halfLineWidth, halfLineWidth, rectWidth, rectHeight)
                            g.FillRectangle(brush, Me._LineWidth, Me._LineWidth, fillWidth, fillHeight)

                        Case ShapeModes.Ellipse ' einfache Ellipse zeichnen
                            g.DrawEllipse(pen, halfLineWidth, halfLineWidth, rectWidth, rectHeight)

                        Case ShapeModes.FilledEllipse ' einfache Ellipse zeichnen und ausfüllen
                            g.DrawEllipse(pen, halfLineWidth, halfLineWidth, rectWidth, rectHeight)
                            g.FillEllipse(brush, Me._LineWidth, Me._LineWidth, fillWidth, fillHeight)

                    End Select

                End Using

            End Using

        End Sub

        ''' <summary>
        ''' Legt spezielle Parameter für das ShapeControl fest.
        ''' </summary>
        ''' <remarks>
        '''
        ''' <para>Das Setzen von <c>WS_EX_TRANSPARENT</c> sorgt dafür, dass der Hintergrund des Eltern-Steuerelements
        ''' durchscheint.</para>
        '''
        ''' <para>Weitere Informationen:
        ''' <see href="https://stackoverflow.com/questions/511320/transparent-control-backgrounds-on-a-vb-net-gradient-filled-form"/>
        ''' und <see href="https://learn.microsoft.com/de-de/windows/win32/winmsg/extended-window-styles"/> .</para>
        ''' </remarks>
        ''' <value>
        ''' Ein <see cref="CreateParams"/> Objekt mit erweiterten Stil-Flags.
        ''' </value>
        Protected Overrides ReadOnly Property CreateParams() As CreateParams
            Get
                Dim cp As CreateParams = MyBase.CreateParams
                ' Aktiviert den erweiterten Fenstilstil WS_EX_TRANSPARENT.
                cp.ExStyle = cp.ExStyle Or &H20
                Return cp
            End Get
        End Property

#End Region

    End Class

End Namespace
