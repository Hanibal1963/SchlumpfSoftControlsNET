' --------------------------------------------------------------------------------------------------------
' Datei: MultiDigit.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Threading

Namespace SevenSegmentControl

    ''' <summary>
    ''' Stellt ein Control dar, das mehrere Siebensegmentanzeigen enthält.
    ''' </summary>
    <Description("Stellt ein Control dar, das mehrere Siebensegmentanzeigen enthält.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(MultiDigit), "SevenSegmentControl.MultiDigit.bmp")>
    Public Class MultiDigit

        Inherits Control

#Region "Definition der Variablen"

        Private _Digits As SingleDigit()        ' Array der untergeordneten Einzelanzeigen (Digits), welche die Zeichen darstellen.
        Private _DigitPadding As Padding        ' Innenabstand (Padding), der für jedes Digit angewendet wird.
        Private _Value As String                ' Der aktuell darzustellende Textwert.

        Private _SegmentWidth As Int32          ' Breite der LED-Segmente in Pixeln.
        Private _ItalicFactor As Single         ' Scherkoeffizient zur Simulation von Kursivschrift (negativ = nach links geneigt).
        Private _BackgroundColor As Color       ' Hintergrundfarbe des Controls.
        Private _InactiveColor As Color         ' Farbe, mit der inaktive Segmente gezeichnet werden.
        Private _ForeColor As Color             ' Farbe, mit der aktive Segmente gezeichnet werden.
        Private _ShowDecimalPoint As Boolean    ' Gibt an, ob der Dezimalpunkt pro Digit sichtbar sein kann.

#End Region

#Region "Definition der Eigenschaften"

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
                Me.UpdateSegments()
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
                Me.UpdateSegments()
            End Set
        End Property

        ''' <summary>
        ''' Scherkoeffizient für die Kursivschrift der Anzeige.
        ''' </summary>
        ''' <remarks>
        ''' Standardwert ist -0.1
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
                Me.UpdateSegments()
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
                Me.UpdateSegments()
            End Set
        End Property

        ''' <summary>
        ''' Anzahl der Digits in diesem Control.
        ''' </summary>
        <Category("Appearance")>
        <Description("Anzahl der Digits in diesem Control.")>
        <DefaultValue(1)>
        Public Property DigitCount As Int32
            Get
                Return Me._Digits.Length
            End Get
            Set(value As Int32)
                If value > 0 AndAlso value <= 100 Then Me.CreateSegments(value)
            End Set
        End Property

        ''' <summary>
        ''' Auffüllung, die für jedes Digit im Control gilt.
        ''' </summary>
        ''' <remarks>
        ''' Passen Sie diese Zahlen an, um das perfekte Erscheinungsbild für das Control Ihrer Größe zu erhalten.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Auffüllung, die für jedes Digit im Control gilt.")>
        <DefaultValue(GetType(Padding), "0,0,0,0")>
        Public Property DigitPadding As Padding
            Get
                Return Me._DigitPadding
            End Get
            Set(value As Padding)
                Me._DigitPadding = value
                Me.UpdateSegments()
            End Set
        End Property

        ''' <summary>
        ''' Der auf dem Control anzuzeigende Wert.
        ''' </summary>
        ''' <remarks>
        ''' Kann Zahlen, bestimmte Buchstaben und Dezimalpunkte enthalten.
        ''' </remarks>
        <Category("Appearance")>
        <Description("Der auf dem Control anzuzeigende Wert.")>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        Public Property Value As String
            Get
                Return Me._Value
            End Get
            Set(value As String)
                Me._Value = value
                ' Anzeigezustand aller Digits vor dem erneuten Parsen zurücksetzen.
                For i = 0 To Me._Digits.Length - 1
                    Me._Digits(i).CustomBitPattern = 0
                    Me._Digits(i).DecimalPointActive = False
                Next
                If Not Equals(Me._Value, Nothing) Then
                    Dim segmentIndex = 0
                    ' Wert wird von rechts nach links verarbeitet,
                    ' damit die letzte Zeichenposition auf dem rechten Digit landet.
                    For i = Me._Value.Length - 1 To 0 Step -1
                        If segmentIndex >= Me._Digits.Length Then Exit For
                        If Me._Value(i) = "."c Then
                            ' Dezimalpunkt gehört zum zuletzt gesetzten Digit.
                            Me._Digits(segmentIndex).DecimalPointActive = True
                        Else
                            Me._Digits(Math.Min(Interlocked.Increment(segmentIndex), segmentIndex - 1)).DigitValue = Me._Value(i).ToString()
                        End If
                    Next
                End If
            End Set
        End Property

        ''' <summary>
        ''' Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.
        ''' </summary>
        <Category("Appearance")>
        <Description("Legt die Hintergrundfarbe des Controls fest oder gibt diese zurück.")>
        Public Overrides Property BackColor As Color
            Get
                Return Me._BackgroundColor
            End Get
            Set(value As Color)
                Me._BackgroundColor = value
                Me.UpdateSegments()
            End Set
        End Property

        ''' <summary>
        ''' Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.
        ''' </summary>
        <Category("Appearance")>
        <Description("Legt die Vordergrundfarbe der Segmente des Controls fest oder gibt diese zurück.")>
        Public Overrides Property ForeColor As Color
            Get
                Return Me._ForeColor
            End Get
            Set(value As Color)
                Me._ForeColor = value
                Me.UpdateSegments()
            End Set
        End Property

        ''' <summary>
        ''' ausgeblendet da nicht relevant.
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
        ''' ausgeblendet da nicht relevant.
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
        ''' ausgeblendet da nicht relevant.
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
        ''' ausgeblendet da nicht relevant.
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
        ''' ausgeblendet da nicht relevant.
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

#Region "Definition der öffentlichen Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="MultiDigit"/>.
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()
            Me.InitializeDefaults()
            Me.SuspendLayout()
            Me.Name = "SevSegMultiDigit"
            Me.Size = New Size(100, 25)
            AddHandler Resize, New EventHandler(AddressOf Me.SevSegMultiDigit_Resize)
            Me.TabStop = False
            Me._DigitPadding = New Padding(10, 4, 10, 4)
            Me.CreateSegments(4)
            Me.ResumeLayout(False)
        End Sub

        Private Sub InitializeDefaults()
            Me._SegmentWidth = 10 ' Breite der LED-Segmente in Pixeln.
            Me._ItalicFactor = -0.1F ' Scherkoeffizient zur Simulation von Kursivschrift (negativ = nach links geneigt).
            Me._BackgroundColor = SystemColors.Control ' Hintergrundfarbe des Controls.
            Me._InactiveColor = Color.DarkGray ' Farbe, mit der inaktive Segmente gezeichnet werden.
            Me._ForeColor = Color.DarkGreen ' Farbe, mit der aktive Segmente gezeichnet werden.
            Me._ShowDecimalPoint = True ' Gibt an, ob der Dezimalpunkt pro Digit sichtbar sein kann.
        End Sub

#End Region

#Region "Definition der internen Methoden"

        ''' <summary>
        ''' Erstellt die angegebene Anzahl von <see cref="SingleDigit"/> -Instanzen und bindet sie als untergeordnete
        ''' Steuerelemente an.
        ''' </summary>
        ''' <param name="count">Anzahl der zu erzeugenden Digits.</param>
        Private Sub CreateSegments(count As Int32)
            If Me._Digits IsNot Nothing Then
                For i = 0 To Me._Digits.Length - 1
                    ' Bestehende Digits sauber aus dem visuellen Baum entfernen und freigeben.
                    Me._Digits(i).Parent = Nothing
                    Me._Digits(i).Dispose()
                Next
            End If
            If count <= 0 Then Return
            Me._Digits = New SingleDigit(count - 1) {}
            For i = 0 To count - 1
                Me._Digits(i) = New SingleDigit With {
                        .Parent = Me,
                        .Top = 0,
                        .Height = Me.Height,
                        .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom,
                        .Visible = True
                    }
            Next
            Me.ResizeSegments()
            Me.UpdateSegments()
            Me.Value = Me._Value
        End Sub

        ''' <summary>
        ''' Berechnet Breite und Position aller Digits neu, damit die Anzeige gleichmäßig über die verfügbare Breite
        ''' verteilt ist.
        ''' </summary>
        Private Sub ResizeSegments()
            Dim segWidth As Int32 = CInt(Me.Width / Me._Digits.Length)
            For i = 0 To Me._Digits.Length - 1
                ' Digits werden von rechts nach links positioniert,
                ' damit der niedrigstwertige Stellenwert rechts liegt.
                Me._Digits(i).Left = CInt(Me.Width * (Me._Digits.Length - 1 - i) / Me._Digits.Length)
                Me._Digits(i).Width = segWidth
            Next
        End Sub

        ''' <summary>
        ''' Überträgt die aktuell gesetzten Anzeigeeigenschaften auf alle enthaltenen Digits.
        ''' </summary>
        Private Sub UpdateSegments()
            For i = 0 To Me._Digits.Length - 1
                Me._Digits(i).BackColor = Me._BackgroundColor
                Me._Digits(i).InactiveColor = Me._InactiveColor
                Me._Digits(i).ForeColor = Me._ForeColor
                Me._Digits(i).SegmentWidth = Me._SegmentWidth
                Me._Digits(i).ItalicFactor = Me._ItalicFactor
                Me._Digits(i).ShowDecimalPoint = Me._ShowDecimalPoint
                Me._Digits(i).Padding = Me._DigitPadding
            Next
        End Sub

        ''' <summary>
        ''' Reagiert auf Größenänderungen des Steuerelements und passt die Digit-Geometrie an.
        ''' </summary>
        ''' <param name="sender">Quelle des Ereignisses.</param>
        ''' <param name="e">Ereignisdaten der Größenänderung.</param>
        Private Sub SevSegMultiDigit_Resize(sender As Object, e As EventArgs)
            Me.ResizeSegments()
        End Sub

        ''' <summary>
        ''' Zeichnet den Hintergrund des Steuerelements in der konfigurierten Hintergrundfarbe.
        ''' </summary>
        ''' <param name="pevent">Zeichenkontext für den Hintergrund.</param>
        Protected Overrides Sub OnPaintBackground(pevent As PaintEventArgs)
            pevent.Graphics.Clear(Me._BackgroundColor)
        End Sub

#End Region

    End Class

End Namespace
