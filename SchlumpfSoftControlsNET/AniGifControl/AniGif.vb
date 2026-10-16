' --------------------------------------------------------------------------------------------------------
' Datei: AniGif.vb
' Author: Andreas Sauer
' Datum: 25.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Windows.Forms

Namespace AniGifControl

    ''' <summary>
    ''' Control zum Anzeigen von animierten Grafiken.
    ''' </summary>
    <Description("Control zum Anzeigen von animierten Grafiken.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(AniGif), "AniGifControl.AniGif.bmp")>
    Public Class AniGif

        Inherits UserControl
        Implements IDisposable

#Region "Variablen"

        ' Gemeinsamer Handler für ImageAnimator zum Stoppen/Neu-Registrieren
        Private ReadOnly _AnimationHandler As EventHandler = AddressOf Me.OnNextFrame

        Private _FramesPerSecond As Decimal ' Benutzerdefinierte Wiedergabegeschwindigkeit (nur aktiv bei CustomDisplaySpeed = True)
        Private _Dimension As FrameDimension ' GIF-Frame-Dimension (bei GIFs i. d. R. Time)
        Private _Frame As Int32 ' Aktueller Frame-Index für die Timer-basierte Wiedergabe
        Private _MaxFrame As Int32 ' Letzter gültiger Frame-Index des geladenen GIFs
        Private _Autoplay As Boolean ' Steuert, ob die Animation automatisch starten soll
        Private _ZoomFactor As Decimal ' Zoomfaktor in Prozent für den Zoom-Modus
        Private _CustomDisplaySpeed As Boolean ' True = Timer/FPS verwenden, False = GIF-interne Verzögerung verwenden
        Private _GifSizeMode As ImageSizeMode ' Art der Darstellung (Normal, Stretch, Zoom, ...)
        Private _Gif As Bitmap ' Aktuell angezeigtes Bild (intern immer als Bitmap gehalten)
        Private _OwnsGif As Boolean ' Kennzeichnet, ob _Gif vom Control geklont/erstellt wurde und damit freigegeben werden muss
        Private _IsAnimating As Boolean ' Interner Status, ob aktuell eine Animation aktiv läuft
        Private _IsDisposed As Boolean  ' Kennzeichnet, dass die Ressourcenfreigabe des Controls begonnen hat

#End Region

#Region "Öffentliche Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst wenn die Grafik nicht animiert werden kann.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn die Grafik nicht animiert werden kann.")>
        Public Event NoAnimation(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst wenn sich die Eigenschaft Autoplay geändert hat.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn sich die Eigenschaft Autoplay geändert hat.")>
        Public Event AutoPlayChanged(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst wenn die Animation gestartet wurde.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn die Animation gestartet wurde.")>
        Public Event AnimationStarted(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst wenn die Animation gestoppt wurde.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn die Animation gestoppt wurde.")>
        Public Event AnimationStopped(sender As Object, e As EventArgs)

#End Region

#Region "Interne Ereignisse"

        Private Event GifChanged()
        Private Event CustomDisplaySpeedChanged()
        Private Event FramesPerSecondChanged()

#End Region

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Steuert, ob die GIF‑Animation automatisch gestartet wird, sobald ein Bild vorhanden ist.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt fest ob die Animation sofort nach dem laden gestartet wird.")>
        <DefaultValue(False)>
        Public Property AutoPlay() As Boolean
            Get
                Return Me._Autoplay
            End Get
            Set(value As Boolean)
                If Me._Autoplay = value Then Return
                Me._Autoplay = value
                RaiseEvent AutoPlayChanged(Me, EventArgs.Empty)
                Me.UpdateTimerState()
            End Set
        End Property

        ''' <summary>
        ''' Gibt die animierte GIF‑Grafik zurück oder legt diese fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt die animierte Gif-Grafik zurück oder legt diese fest.")>
        <DefaultValue(GetType(Bitmap), "AniGifControl_Standard")>
        Public Property Gif() As Bitmap
            Get
                Return Me._Gif
            End Get
            Set(value As Bitmap)
                Me.SetGifImage(value)
            End Set
        End Property

        ''' <summary>
        ''' Gibt den Anzeigemodus (Skalierung/Ausrichtung) der GIF‑Grafik zurück oder legt ihn fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Gibt die Art wie die Grafik angezeigt wird zurück oder legt diese fest.")>
        <DefaultValue(ImageSizeMode.Normal)>
        Public Property GifSizeMode() As ImageSizeMode
            Get
                Return Me._GifSizeMode
            End Get
            Set(value As ImageSizeMode)
                Me.SetGifSizeMode(value)
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob die benutzerdefinierte Anzeigegeschwindigkeit (Timer/FPS) oder die im GIF hinterlegte
        ''' Bildfolge (ImageAnimator) verwendet wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt fest ob die benutzerdefinierte Anzeigegeschwindigkeit oder die in der Datei festgelegte Geschwindigkeit benutzt wird.")>
        <DefaultValue(False)>
        Public Property CustomDisplaySpeed As Boolean
            Get
                Return Me._CustomDisplaySpeed
            End Get
            Set(value As Boolean)
                Me.SetCustomDisplaySpeed(value)
            End Set
        End Property

        ''' <summary>
        ''' Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern pro Sekunde (FPS) fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern/Sekunde fest wenn CustomDisplaySpeed auf True festgelegt ist.")>
        <DefaultValue(30D)>
        Public Property FramesPerSecond As Decimal
            Get
                Return Me._FramesPerSecond
            End Get
            Set(value As Decimal)
                Me._FramesPerSecond = FunctionDefinitions.CheckFramesPerSecondValue(value)
                RaiseEvent FramesPerSecondChanged()
            End Set
        End Property

        ''' <summary>
        ''' Legt den Zoomfaktor in Prozent fest, mit dem das GIF skaliert wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt den Zoomfaktor fest wenn GifSizeMode auf Zoom festgelegt ist.")>
        <DefaultValue(50D)>
        Public Property ZoomFactor As Decimal
            Get
                Return Me._ZoomFactor
            End Get
            Set(value As Decimal)
                Me.SetZoomFactor(value)
            End Set
        End Property

#End Region

#Region "ausgeblendete Eigenschaften"

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property MaximumSize As Size
            Get
                Return MyBase.MaximumSize
            End Get
            Set(value As Size)
                MyBase.MaximumSize = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property MinimumSize As Size
            Get
                Return MyBase.MinimumSize
            End Get
            Set(value As Size)
                MyBase.MinimumSize = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property RightToLeft() As RightToLeft
            Get
                Return MyBase.RightToLeft
            End Get
            Set(value As RightToLeft)
                MyBase.RightToLeft = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Text() As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property AllowDrop() As Boolean
            Get
                Return MyBase.AllowDrop
            End Get
            Set(value As Boolean)
                MyBase.AllowDrop = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property AutoScrollOffset As Point
            Get
                Return MyBase.AutoScrollOffset
            End Get
            Set(value As Point)
                MyBase.AutoScrollOffset = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property AutoSize As Boolean
            Get
                Return MyBase.AutoSize
            End Get
            Set(value As Boolean)
                MyBase.AutoSize = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImage() As Image
            Get
                Return MyBase.BackgroundImage
            End Get
            Set(value As Image)
                MyBase.BackgroundImage = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property BackgroundImageLayout() As ImageLayout
            Get
                Return MyBase.BackgroundImageLayout
            End Get
            Set(value As ImageLayout)
                MyBase.BackgroundImageLayout = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property ContextMenuStrip() As ContextMenuStrip
            Get
                Return MyBase.ContextMenuStrip
            End Get
            Set(value As ContextMenuStrip)
                MyBase.ContextMenuStrip = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Dock() As DockStyle
            Get
                Return MyBase.Dock
            End Get
            Set(value As DockStyle)
                MyBase.Dock = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property Font() As Font
            Get
                Return MyBase.Font
            End Get
            Set(value As Font)
                MyBase.Font = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Property ForeColor() As Color
            Get
                Return MyBase.ForeColor
            End Get
            Set(value As Color)
                MyBase.ForeColor = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DefaultValue(GetType(Padding), "0")>
        Public Overloads Property Padding As Padding
            Get
                Return MyBase.Padding
            End Get
            Set(value As Padding)
                MyBase.Padding = value
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        Public Sub New()
            Me.InitializeComponent() 'Designer-Initialisierung
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
            Me.UpdateStyles()
        End Sub

        ''' <summary>
        ''' Startet die Animation (falls noch nicht aktiv).
        ''' </summary>
        Public Sub StartAnimation()
            If Not Me.AutoPlay Then
                Me.AutoPlay = True
            End If
        End Sub

        ''' <summary>
        ''' Stoppt die Animation und beendet Timer sowie ImageAnimator.
        ''' </summary>
        Public Sub StopAnimation()
            If Me.AutoPlay Then
                Me.AutoPlay = False
            End If
        End Sub

#End Region

#Region "Interne Methoden"

        Protected Overloads Overrides Sub InitLayout()
            MyBase.InitLayout()
            ' Nach Layout-Initialisierung den korrekten Animationsmodus setzen
            Me.UpdateTimerState()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)

            ' Null-Schutz
            If Me._Gif Is Nothing Then Return

            ' Variable für Zeichenfläche
            Dim g As Graphics = e.Graphics

            ' Größe der Zeichenfläche berechnen
            Dim rectstartsize As Size = FunctionDefinitions.GetRectStartSize(Me._GifSizeMode, Me, Me._Gif, Me._ZoomFactor / 100)

            ' Startpunkt der Zeichenfläche berechnen
            Dim rectstartpoint As Point = FunctionDefinitions.GetRectStartPoint(Me._GifSizeMode, Me, Me._Gif, rectstartsize)

            ' Qualitätsverbesserung nur bei Skalierung
            If Me._GifSizeMode = ImageSizeMode.Zoom OrElse Me._GifSizeMode = ImageSizeMode.Fill Then
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            End If

            ' Zeichenfläche festlegen und Bild zeichnen
            g.DrawImage(Me._Gif, New Rectangle(rectstartpoint, rectstartsize))

            ' Bild animieren wenn AutoPlay aktiv und Benutzerdefinierte Geschwindigkeit deaktiviert
            If Not Me.DesignMode AndAlso Me._Autoplay AndAlso Not Me._CustomDisplaySpeed Then
                ' im Bild gespeicherte Geschwindigkeit verwenden
                ImageAnimator.UpdateFrames()
            End If

        End Sub

        Private Sub AniGif_GifChange() Handles Me.GifChanged

            If Me._IsDisposed OrElse Me.IsDisposed Then Return

            If Me._Gif Is Nothing Then
                ' Kein Bild vorhanden -> Animationszähler zurücksetzen und sauber stoppen
                Me._MaxFrame = 0
                Me._Frame = 0
                Me.UpdateTimerState()
                Me.Invalidate()
                Exit Sub
            End If

            ' prüfen ob das Bild animiert werden kann
            If ImageAnimator.CanAnimate(Me._Gif) = False AndAlso Me._Autoplay = True Then

                ' Anzahl der Frames auf 0 setzen (für nicht animiertes bild)
                Me._MaxFrame = 0
                Me._Frame = 0

            Else

                ' Werte für Benutzerdefinierte Geschwindigkeit speichern
                ' (FrameDimension + Anzahl Frames werden für Timer_Tick benötigt)
                Me._Dimension = New FrameDimension(Me._Gif.FrameDimensionsList(0))
                Me._MaxFrame = Me._Gif.GetFrameCount(Me._Dimension) - 1
                Me._Frame = 0

            End If

            ' neu zeichnen
            Me.Invalidate()

            ' Animation starten
            Me.UpdateTimerState()

        End Sub

        Private Sub AniGif_CustomDisplaySpeedChanged() Handles Me.CustomDisplaySpeedChanged

            ' Bei Moduswechsel zwischen Timer/ImageAnimator sofort neu konfigurieren
            Me.UpdateTimerState()

        End Sub

        Private Sub AniGif_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed

            Me.DisposeAnimationResources()

        End Sub

        Private Sub DisposeAnimationResources()

            If Me._IsDisposed Then Return
            Me._IsDisposed = True

            ' Animation vor dem Freigeben von Timer und Bitmap vollständig abmelden.
            Me.StopAnimationCore(False)

            ' Nur intern erzeugte Bitmaps freigeben (fremde Instanzen nicht ungefragt disposen).
            If Me._OwnsGif AndAlso Me._Gif IsNot Nothing Then
                Me._Gif.Dispose()
                Me._Gif = Nothing
                Me._OwnsGif = False
            End If

        End Sub

        Private Sub AniGif_FramesPerSecondChanged() Handles Me.FramesPerSecondChanged

            ' Sicherheitsprüfung
            If Me._FramesPerSecond < 1D Then Me._FramesPerSecond = 1D

            Me.UpdateTimerState()

        End Sub

        Private Sub OnNextFrame(o As Object, e As EventArgs)

            ' Callback von ImageAnimator: nur neu zeichnen, Frame-Umschaltung macht ImageAnimator selbst
            If Not Me._IsDisposed AndAlso Not Me.IsDisposed AndAlso Me.AutoPlay AndAlso Not Me.DesignMode Then
                Me.InvalidateIfVisible() 'neu zeichnen nur wenn sichtbar
            End If

        End Sub

        Private Sub InvalidateIfVisible()

            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Not Me.IsHandleCreated Then Return
            If Not Me.Visible Then Return
            If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

            Me.Invalidate()

        End Sub

        Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick

            ' Bild animieren wenn AutoPlay und Benutzerdefinierte Geschwindigkeit aktiv
            If Not Me._IsDisposed AndAlso Not Me.IsDisposed AndAlso Not Me.DesignMode AndAlso Me.AutoPlay Then

                If Me._Gif Is Nothing Then Exit Sub

                ' wenn Frames = 0 ist das Bild nicht animiert -> Ende
                If Me._MaxFrame = 0 Then Exit Sub

                ' Nur bei echtem Framewechsel umschalten/zeichnen
                Dim nextFrame As Int32 = Me._Frame + 1
                If nextFrame > Me._MaxFrame Then nextFrame = 0
                If nextFrame = Me._Frame Then Exit Sub

                Dim unused = Me._Gif.SelectActiveFrame(Me._Dimension, nextFrame)
                Me._Frame = nextFrame

                ' Nur neu zeichnen wenn das Control sichtbar ist
                Me.InvalidateIfVisible()

            End If

        End Sub

        Private Sub SetGifImage(value As Bitmap)

            ' Vorherige Animation stoppen bevor neues Bild gesetzt wird
            If Me._Gif IsNot Nothing Then
                ImageAnimator.StopAnimate(Me._Gif, Me._AnimationHandler)
            End If

            If Me._OwnsGif AndAlso Me._Gif IsNot Nothing Then
                ' Vorheriges, intern besessenes Bild freigeben
                Me._Gif.Dispose()
            End If

            ' Standardanimation verwenden wenn keine Auswahl erfolgte
            If value Is Nothing Then
                Me._Gif = My.Resources.AniGifControl_Standard


                Me._OwnsGif = False
            Else
                ' Defensive Kopie: externe Änderungen/Dispose am Original beeinflussen das Control nicht
                Me._Gif = DirectCast(value.Clone(), Bitmap)
                Me._OwnsGif = True
            End If

            RaiseEvent GifChanged()

        End Sub

        Private Sub SetGifSizeMode(value As ImageSizeMode)

            If Me._GifSizeMode = value Then Return
            Me._GifSizeMode = value
            Me.Invalidate()

        End Sub

        Private Sub SetCustomDisplaySpeed(value As Boolean)

            If Me._CustomDisplaySpeed = value Then Return
            Me._CustomDisplaySpeed = value
            RaiseEvent CustomDisplaySpeedChanged()

        End Sub

        Private Sub SetZoomFactor(value As Decimal)

            Dim validatedZoomFactor As Decimal = FunctionDefinitions.CheckZoomFactorValue(value)
            If Me._ZoomFactor = validatedZoomFactor Then Return

            Me._ZoomFactor = validatedZoomFactor
            ' neu zeichnen
            Me.Invalidate()

        End Sub

        Private Sub UpdateTimerState()

            If Me._IsDisposed OrElse Me.IsDisposed Then Return

            ' Immer zuerst beide Mechanismen stoppen, danach den benötigten Modus aktivieren
            Me.StopAnimationCore(True)

            If Me.DesignMode OrElse Not Me._Autoplay OrElse Me._Gif Is Nothing Then Return

            If Me._CustomDisplaySpeed Then

                ' Timer-basierte Wiedergabe mit fixer FPS
                If Me._MaxFrame > 0 Then
                    Me.Timer.Interval = CInt(Math.Max(1D, 1000D / Math.Max(Me._FramesPerSecond, 1D)))
                    Me.Timer.Start()
                    Me._IsAnimating = True
                    RaiseEvent AnimationStarted(Me, EventArgs.Empty)
                Else
                    RaiseEvent NoAnimation(Me, EventArgs.Empty)

                End If

            Else

                ' GIF-interne Frame-Delays über ImageAnimator verwenden
                If ImageAnimator.CanAnimate(Me._Gif) Then
                    ImageAnimator.Animate(Me._Gif, Me._AnimationHandler)
                    Me._IsAnimating = True
                    RaiseEvent AnimationStarted(Me, EventArgs.Empty)
                Else
                    RaiseEvent NoAnimation(Me, EventArgs.Empty)
                End If

            End If

        End Sub

        Private Sub StopAnimationCore(raiseAnimationStopped As Boolean)

            Dim wasAnimating As Boolean = Me._IsAnimating

            Me.Timer.Stop()
            If Me._Gif IsNot Nothing Then
                ImageAnimator.StopAnimate(Me._Gif, Me._AnimationHandler)
            End If
            Me._IsAnimating = False

            If raiseAnimationStopped AndAlso wasAnimating Then
                RaiseEvent AnimationStopped(Me, EventArgs.Empty)
            End If

        End Sub

#End Region

    End Class

End Namespace
