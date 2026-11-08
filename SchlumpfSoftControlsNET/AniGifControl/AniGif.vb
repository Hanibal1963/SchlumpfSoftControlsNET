' --------------------------------------------------------------------------------------------------------
' Datei: AniGif.vb
' Author: Andreas Sauer
' Datum: 25.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Diagnostics
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
        Private _CustomDisplaySpeed As Boolean ' True = Timer/FPS verwenden, False = GIF-Metadaten-Verzögerung verwenden
        Private _GifSizeMode As ImageSizeMode ' Art der Darstellung (Normal, Stretch, Zoom, ...)
        Private _scalingQualityPreset As GifScalingQualityPreset
        Private _Gif As Bitmap ' Aktuell angezeigtes Bild (intern immer als Bitmap gehalten)
        Private _OwnsGif As Boolean ' Kennzeichnet, ob _Gif vom Control geklont/erstellt wurde und damit freigegeben werden muss
        Private _IsAnimating As Boolean ' Interner Status, ob aktuell eine Animation aktiv läuft
        Private _IsDisposed As Boolean  ' Kennzeichnet, dass die Ressourcenfreigabe des Controls begonnen hat
        Private _MinimumFrameIntervalMs As Int32 ' Untere Grenze für normalisierte Frame-Intervalle
        Private _MaximumFrameIntervalMs As Int32 ' Obere Grenze für normalisierte Frame-Intervalle
        Private _NormalizedFrameDelaysMs() As Int32 ' Normalisierte Delay-Werte je Frame
        Private _LastFrameTickTimestamp As Int64 ' Zeitstempel des letzten Timer-Ticks
        Private _AdaptiveTiming As Boolean ' Aktiviert Drift-Korrektur auf Basis der UI-Thread-Last

        Private Const DefaultFrameDelayMs As Int32 = 100
        Private Const FrameDelayPropertyTag As Int32 = &H5100

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
        ''' Legt das Qualitäts-Preset für die GIF-Skalierung in den Modi Zoom und Fill fest. Der Standardwert ist Photo.
        ''' </summary>
        ''' <exception cref="InvalidEnumArgumentException">
        ''' Der Wert ist kein gültiges Qualitäts-Preset.
        ''' </exception>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt die Qualität der GIF-Skalierung für Zoom und Fill fest.")>
        <DefaultValue(GifScalingQualityPreset.Photo)>
        Public Property ScalingQualityPreset As GifScalingQualityPreset
            Get
                Return Me._scalingQualityPreset
            End Get
            Set(value As GifScalingQualityPreset)
                Me.SetScalingQualityPreset(value)
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
        ''' Legt das minimale Frame-Intervall in Millisekunden fest, das nach der Delay-Normalisierung verwendet wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt das minimale Frame-Intervall in Millisekunden fest.")>
        <DefaultValue(20)>
        Public Property MinimumFrameIntervalMs As Int32
            Get
                Return Me._MinimumFrameIntervalMs
            End Get
            Set(value As Int32)
                Me.SetMinimumFrameIntervalMs(value)
            End Set
        End Property

        ''' <summary>
        ''' Legt das maximale Frame-Intervall in Millisekunden fest, das nach der Delay-Normalisierung verwendet wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt das maximale Frame-Intervall in Millisekunden fest.")>
        <DefaultValue(500)>
        Public Property MaximumFrameIntervalMs As Int32
            Get
                Return Me._MaximumFrameIntervalMs
            End Get
            Set(value As Int32)
                Me.SetMaximumFrameIntervalMs(value)
            End Set
        End Property

        ''' <summary>
        ''' Legt fest, ob das Timer-Intervall adaptiv an erkannte UI-Thread-Last angepasst wird.
        ''' </summary>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Aktiviert adaptive Drift-Korrektur für die Frame-Wiedergabe.")>
        <DefaultValue(False)>
        Public Property AdaptiveTiming As Boolean
            Get
                Return Me._AdaptiveTiming
            End Get
            Set(value As Boolean)
                Me.SetAdaptiveTiming(value)
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
            Me.InitializeDefaults()
        End Sub

        ''' <summary>
        ''' Startet die Animation (falls noch nicht aktiv).
        ''' </summary>
        Public Sub StartAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Not Me.AutoPlay Then
                Me.AutoPlay = True
            End If
        End Sub

        ''' <summary>
        ''' Stoppt die Animation und beendet Timer sowie ImageAnimator.
        ''' </summary>
        Public Sub StopAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Me.AutoPlay Then
                Me.AutoPlay = False
            End If
        End Sub

#End Region

#Region "Interne Methoden"

        Private Sub InitializeDefaults()
            Me._FramesPerSecond = 30D
            Me._ZoomFactor = 50D
            Me._GifSizeMode = ImageSizeMode.Normal
            Me._scalingQualityPreset = GifScalingQualityPreset.Photo
            Me._Gif = My.Resources.AniGifControl_Standard
            Me._CustomDisplaySpeed = False
            Me._Autoplay = False
            Me._IsAnimating = False
            Me._IsDisposed = False
            Me._MinimumFrameIntervalMs = 20
            Me._MaximumFrameIntervalMs = 500
            Me._NormalizedFrameDelaysMs = Array.Empty(Of Int32)()
            Me._LastFrameTickTimestamp = 0
            Me._AdaptiveTiming = False
        End Sub

        Protected Overloads Overrides Sub InitLayout()
            MyBase.InitLayout()
            ' Nach Layout-Initialisierung den korrekten Animationsmodus setzen
            Me.UpdateTimerState()
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            Me.UpdateTimerState()
        End Sub

        Protected Overrides Sub OnParentChanged(e As EventArgs)
            MyBase.OnParentChanged(e)
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
                If Me._scalingQualityPreset = GifScalingQualityPreset.PixelArt Then
                    g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                    g.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
                    g.SmoothingMode = Drawing2D.SmoothingMode.None
                Else
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                    g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                End If
            End If

            ' Zeichenfläche festlegen und Bild zeichnen
            g.DrawImage(Me._Gif, New Rectangle(rectstartpoint, rectstartsize))

        End Sub

        Private Sub AniGif_GifChange() Handles Me.GifChanged

            If Me._IsDisposed OrElse Me.IsDisposed Then Return

            If Me._Gif Is Nothing Then
                ' Kein Bild vorhanden -> Animationszähler zurücksetzen und sauber stoppen
                Me._MaxFrame = 0
                Me._Frame = 0
                Me._NormalizedFrameDelaysMs = Array.Empty(Of Int32)()
                Me.UpdateTimerState()
                Me.Invalidate()
                Exit Sub
            End If

            ' Werte für die Timer-Wiedergabe vorbereiten
            Me._Dimension = New FrameDimension(Me._Gif.FrameDimensionsList(0))
            Dim frameCount As Int32 = Me._Gif.GetFrameCount(Me._Dimension)
            Me._MaxFrame = Math.Max(0, frameCount - 1)
            Me._Frame = 0

            Me.RefreshFrameDelayCache()

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

            Me.RefreshFrameDelayCache()
            Me.UpdateTimerState()

        End Sub

        Private Function NormalizeFrameIntervalMs(value As Int32) As Int32

            Dim minInterval As Int32 = Math.Max(1, Me._MinimumFrameIntervalMs)
            Dim maxInterval As Int32 = Math.Max(minInterval, Me._MaximumFrameIntervalMs)
            Return Math.Min(maxInterval, Math.Max(minInterval, value))

        End Function

        Private Function GetDefaultFrameIntervalMs() As Int32

            Dim fpsInterval As Int32 = CInt(Math.Round(1000D / Math.Max(Me._FramesPerSecond, 1D), MidpointRounding.AwayFromZero))
            Return Me.NormalizeFrameIntervalMs(Math.Max(1, fpsInterval))

        End Function

        Private Function GetFrameIntervalForIndex(frameIndex As Int32) As Int32

            If Me._CustomDisplaySpeed OrElse Me._NormalizedFrameDelaysMs Is Nothing OrElse Me._NormalizedFrameDelaysMs.Length = 0 Then
                Return Me.GetDefaultFrameIntervalMs()
            End If

            Dim safeIndex As Int32 = Math.Min(Math.Max(frameIndex, 0), Me._NormalizedFrameDelaysMs.Length - 1)
            Return Me._NormalizedFrameDelaysMs(safeIndex)

        End Function

        Private Function ApplyAdaptiveTiming(targetIntervalMs As Int32, elapsedMs As Int32, lastScheduledIntervalMs As Int32) As Int32

            If Not Me._AdaptiveTiming OrElse elapsedMs <= 0 Then
                Return Me.NormalizeFrameIntervalMs(targetIntervalMs)
            End If

            Dim driftMs As Int32 = elapsedMs - Math.Max(1, lastScheduledIntervalMs)
            Dim adjustedInterval As Int32 = targetIntervalMs - driftMs
            Return Me.NormalizeFrameIntervalMs(adjustedInterval)

        End Function

        Private Sub RefreshFrameDelayCache()

            If Me._Gif Is Nothing OrElse Me._MaxFrame <= 0 Then
                Me._NormalizedFrameDelaysMs = Array.Empty(Of Int32)()
                Return
            End If

            Dim frameCount As Int32 = Me._MaxFrame + 1
            Dim normalizedDelays(frameCount - 1) As Int32
            Dim fallbackDelay As Int32 = Me.GetDefaultFrameIntervalMs()

            For i As Int32 = 0 To normalizedDelays.Length - 1
                normalizedDelays(i) = fallbackDelay
            Next

            Try
                Dim delayProperty As PropertyItem = Me._Gif.GetPropertyItem(FrameDelayPropertyTag)
                If delayProperty IsNot Nothing AndAlso delayProperty.Value IsNot Nothing AndAlso delayProperty.Value.Length >= 4 Then
                    Dim availableCount As Int32 = Math.Min(frameCount, delayProperty.Value.Length \ 4)
                    For i As Int32 = 0 To availableCount - 1
                        Dim delayInCentiseconds As Int32 = BitConverter.ToInt32(delayProperty.Value, i * 4)
                        Dim delayInMs As Int32 = delayInCentiseconds * 10
                        If delayInMs <= 0 Then delayInMs = fallbackDelay
                        normalizedDelays(i) = Me.NormalizeFrameIntervalMs(delayInMs)
                    Next
                End If
            Catch ex As ArgumentException
                ' Kein Delay-Metadatenblock vorhanden -> Fallback-Delays bleiben aktiv.
            End Try

            Me._NormalizedFrameDelaysMs = normalizedDelays

        End Sub

        Private Sub OnNextFrame(o As Object, e As EventArgs)

            If Me._IsDisposed OrElse Me.IsDisposed OrElse Not Me.AutoPlay OrElse Me.DesignMode Then Return
            If Me._Gif Is Nothing OrElse Me._MaxFrame <= 0 Then Return
            If Not Me.IsControlTreeVisible() Then Return

            Dim nextFrame As Int32 = Me._Frame + 1
            If nextFrame > Me._MaxFrame Then nextFrame = 0
            If nextFrame = Me._Frame Then Return

            ' Callback von ImageAnimator: Frame berechnen und danach explizit neu zeichnen.
            Me._Frame = nextFrame
            ImageAnimator.UpdateFrames(Me._Gif)
            Me.InvalidateIfVisible()

        End Sub

        Private Sub InvalidateIfVisible()

            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Not Me.IsHandleCreated Then Return
            If Not Me.IsControlTreeVisible() Then Return
            If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

            Me.Invalidate()

        End Sub

        Private Function IsControlTreeVisible() As Boolean

            If Not Me.Visible Then Return False

            Dim currentParent As Control = Me.Parent
            While currentParent IsNot Nothing
                If Not currentParent.Visible Then Return False
                currentParent = currentParent.Parent
            End While

            Return True

        End Function

        Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick

            If Me._IsDisposed OrElse Me.IsDisposed OrElse Me.DesignMode OrElse Not Me.AutoPlay Then Exit Sub
            If Me._Gif Is Nothing Then Exit Sub
            If Not Me.IsControlTreeVisible() Then Exit Sub

            ' wenn Frames = 0 ist das Bild nicht animiert -> Ende
            If Me._MaxFrame = 0 Then Exit Sub

            Dim currentTimestamp As Int64 = Stopwatch.GetTimestamp()
            Dim elapsedMs As Int32 = 0
            If Me._LastFrameTickTimestamp > 0 Then
                elapsedMs = CInt(Math.Max(0, (currentTimestamp - Me._LastFrameTickTimestamp) * 1000L \ Stopwatch.Frequency))
            End If
            Me._LastFrameTickTimestamp = currentTimestamp

            ' Nur bei echtem Framewechsel umschalten/zeichnen
            Dim nextFrame As Int32 = Me._Frame + 1
            If nextFrame > Me._MaxFrame Then nextFrame = 0
            If nextFrame = Me._Frame Then Exit Sub

            Dim unused = Me._Gif.SelectActiveFrame(Me._Dimension, nextFrame)
            Me._Frame = nextFrame

            Dim targetIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
            Dim adjustedIntervalMs As Int32 = Me.ApplyAdaptiveTiming(targetIntervalMs, elapsedMs, Me.Timer.Interval)
            Me.Timer.Interval = adjustedIntervalMs

            ' Nur neu zeichnen wenn das Control sichtbar ist
            Me.InvalidateIfVisible()

        End Sub

        Private Sub SetGifImage(value As Bitmap)

            ' Vorherige Animation stoppen bevor neues Bild gesetzt wird
            Me.StopAnimationCore(False)

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

        Private Sub SetScalingQualityPreset(value As GifScalingQualityPreset)

            If value <> GifScalingQualityPreset.Photo AndAlso value <> GifScalingQualityPreset.PixelArt Then
                Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(GifScalingQualityPreset))
            End If

            If Me._scalingQualityPreset = value Then Return
            Me._scalingQualityPreset = value
            Me.Invalidate()

        End Sub

        Private Sub SetCustomDisplaySpeed(value As Boolean)

            If Me._CustomDisplaySpeed = value Then Return
            Me._CustomDisplaySpeed = value
            RaiseEvent CustomDisplaySpeedChanged()

        End Sub

        Private Sub SetMinimumFrameIntervalMs(value As Int32)

            Dim validatedValue As Int32 = Math.Max(1, value)
            If Me._MinimumFrameIntervalMs = validatedValue Then Return

            Me._MinimumFrameIntervalMs = validatedValue
            If Me._MaximumFrameIntervalMs < Me._MinimumFrameIntervalMs Then
                Me._MaximumFrameIntervalMs = Me._MinimumFrameIntervalMs
            End If

            Me.RefreshFrameDelayCache()
            Me.UpdateTimerState()

        End Sub

        Private Sub SetMaximumFrameIntervalMs(value As Int32)

            Dim validatedValue As Int32 = Math.Max(1, value)
            If Me._MaximumFrameIntervalMs = validatedValue Then Return

            Me._MaximumFrameIntervalMs = validatedValue
            If Me._MaximumFrameIntervalMs < Me._MinimumFrameIntervalMs Then
                Me._MinimumFrameIntervalMs = Me._MaximumFrameIntervalMs
            End If

            Me.RefreshFrameDelayCache()
            Me.UpdateTimerState()

        End Sub

        Private Sub SetAdaptiveTiming(value As Boolean)

            If Me._AdaptiveTiming = value Then Return
            Me._AdaptiveTiming = value
            Me._LastFrameTickTimestamp = Stopwatch.GetTimestamp()

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
            If Not Me.IsControlTreeVisible() Then Return

            If Me._MaxFrame > 0 Then
                Me.Timer.Interval = Me.GetFrameIntervalForIndex(Me._Frame)
                Me._LastFrameTickTimestamp = Stopwatch.GetTimestamp()
                Me.Timer.Start()
                Me._IsAnimating = True
                RaiseEvent AnimationStarted(Me, EventArgs.Empty)
            Else
                RaiseEvent NoAnimation(Me, EventArgs.Empty)
            End If

        End Sub

        Private Sub StopAnimationCore(raiseAnimationStopped As Boolean)

            Dim wasAnimating As Boolean = Me._IsAnimating

            Me.Timer.Stop()
            If Me._Gif IsNot Nothing Then
                ImageAnimator.StopAnimate(Me._Gif, Me._AnimationHandler)
            End If
            Me._LastFrameTickTimestamp = 0
            Me._IsAnimating = False

            If raiseAnimationStopped AndAlso wasAnimating Then
                RaiseEvent AnimationStopped(Me, EventArgs.Empty)
            End If

        End Sub

#End Region

    End Class

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class AniGif

        Inherits System.Windows.Forms.UserControl

        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing Then
                    Me.DisposeAnimationResources()

                    If components IsNot Nothing Then
                        components.Dispose()
                    End If
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Wird vom Windows Form-Designer benötigt.
        Private components As System.ComponentModel.IContainer

        'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.Timer = New System.Windows.Forms.Timer(Me.components)
            Me.SuspendLayout()
            '
            'Timer
            '
            Me.Timer.Interval = 200
            '
            'AniGif
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.DoubleBuffered = True
            Me.Name = "AniGif"
            Me.Size = New System.Drawing.Size(137, 114)
            Me.ResumeLayout(False)

        End Sub

        Private WithEvents Timer As System.Windows.Forms.Timer
    End Class

    ''' <summary>
    ''' Hilfsklasse mit gemeinsam genutzten Berechnungsfunktionen für das AniGif-Control.<br/> Berechnet
    ''' Zeichenpositionen und -größen abhängig vom gewählten <see cref="ImageSizeMode"/> <br/> sowie
    ''' Validierungsfunktionen für Zoom- und FPS-Werte.
    ''' </summary>
    Friend NotInheritable Class FunctionDefinitions

        ''' <summary>
        ''' Berechnet den Startpunkt (linke obere Ecke des Zeichenrechtecks) auf dem Control<br/> in Abhängigkeit vom
        ''' gewählten <see cref="ImageSizeMode"/>.
        ''' </summary>
        ''' <param name="Mode">
        ''' Der Anzeigemodus, der bestimmt wie das Bild positioniert wird.
        ''' </param>
        ''' <param name="Control">
        ''' Das AniGif-Control, auf dem das Bild gezeichnet wird.
        ''' </param>
        ''' <param name="Gif">
        ''' Das anzuzeigende Bitmap (wird für Originalgröße bei CenterImage benötigt).
        ''' </param>
        ''' <param name="RectStartSize">
        ''' Die bereits berechnete Zielgröße des Zeichenrechtecks (relevant bei Zoom und Fill).
        ''' </param>
        ''' <returns>
        ''' Der Punkt, an dem das Zeichnen beginnt (linke obere Ecke).
        ''' </returns>
        Friend Shared Function GetRectStartPoint(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, RectStartSize As Size) As Point

            ' Bestimmt den Startpunkt (linke obere Ecke) des Zeichenrechtecks auf dem Control
            Select Case Mode

                Case ImageSizeMode.Normal
                    ' Bild beginnt ungeskalt an der linken oberen Ecke des Controls (Koordinatenursprung)
                    Return New Point(0, 0)

                Case ImageSizeMode.CenterImage
                    ' Bild wird in Originalgröße mittig im Control platziert.
                    ' Ein negativer Wert ist möglich, wenn das Bild größer als das Control ist
                    ' (dann wird das Bild außerhalb des sichtbaren Bereichs begonnen und erscheint abgeschnitten).
                    ' X = (Control-Breite  - Bild-Breite)  / 2
                    ' Y = (Control-Höhe - Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - Gif.Size.Width) / 2), CInt((Control.Height - Gif.Size.Height) / 2))

                Case ImageSizeMode.Zoom
                    ' Das skalierte Bild wird mittig im Control ausgerichtet.
                    ' RectStartSize enthält die bereits berechnete Größe des skalierten Bildes.
                    ' X = (Control-Breite  - skalierte Bild-Breite)  / 2
                    ' Y = (Control-Höhe - skalierte Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - RectStartSize.Width) / 2), CInt((Control.Height - RectStartSize.Height) / 2))

                Case ImageSizeMode.Fill
                    ' Das auf Control-Größe gestrekte Bild wird mittig ausgerichtet.
                    ' Da beim Fill-Modus das Bild mindestens eine Dimension vollständig füllt,
                    ' kann ein Wert 0 oder negativ sein (kein sichtbarer Versatz bzw. Überlappung).
                    ' X = (Control-Breite  - gestreckte Bild-Breite)  / 2
                    ' Y = (Control-Höhe - gestreckte Bild-Höhe) / 2
                    Return New Point(CInt((Control.Width - RectStartSize.Width) / 2), CInt((Control.Height - RectStartSize.Height) / 2))

                Case Else
                    ' Fallback: Zeichnen ab der linken oberen Ecke
                    Return New Point(0, 0)

            End Select

        End Function

        ''' <summary>
        ''' Berechnet die Zielgröße des Zeichenrechtecks in Abhängigkeit vom gewählten <see cref="ImageSizeMode"/>
        ''' .<br/> Das Ergebnis wird als Parameter an <see cref="GetRectStartPoint"/> weitergegeben.
        ''' </summary>
        ''' <param name="Mode">
        ''' Der Anzeigemodus, der bestimmt wie das Bild skaliert wird.
        ''' </param>
        ''' <param name="Control">
        ''' Das AniGif-Control, das die verfügbare Zeichenfläche vorgibt.
        ''' </param>
        ''' <param name="Gif">
        ''' Das anzuzeigende Bitmap mit der Originalgröße als Berechnungsgrundlage.
        ''' </param>
        ''' <param name="Zoom">
        ''' Der Zoomfaktor (nur im Modus <see cref="ImageSizeMode.Zoom"/> wirksam).
        ''' </param>
        ''' <returns>
        ''' Die berechnete Größe des Zeichenrechtecks.
        ''' </returns>
        Friend Shared Function GetRectStartSize(Mode As ImageSizeMode, Control As AniGif, Gif As Bitmap, Zoom As Decimal) As Size

            ' Null-Schutz: Wenn kein Bild vorhanden ist, leere Größe zurückgeben
            If Gif Is Nothing Then Return Size.Empty

            Select Case Mode

                Case ImageSizeMode.Normal
                    ' Bild wird ungeskalt in Originalgröße dargestellt
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

                Case ImageSizeMode.CenterImage
                    ' Bild wird ebenfalls in Originalgröße dargestellt, nur die Position ist zentriert
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

                Case ImageSizeMode.Zoom
                    ' Bild wird unter Beibehaltung des Seitenverhältnisses mit dem Zoomfaktor skaliert.
                    ' Je nach Ausrichtung (Hochformat/Querformat) wird eine andere Control-Dimension als Basis verwendet.
                    If Gif.Size.Width < Gif.Size.Height Then
                        ' Hochformat: Bild ist höher als breit → Höhe des Controls als Skalierungsbasis.
                        ' Breite  = Control-Höhe / Seitenverhältnis (Höhe/Breite) * Zoom
                        '         = Control-Höhe * (Bild-Breite / Bild-Höhe) * Zoom
                        ' Höhe = Control-Höhe * Zoom
                        Return New Size(CInt(Control.Height / CDec(Gif.Size.Height / Gif.Size.Width) * Zoom), CInt(Control.Height * Zoom))
                    Else
                        ' Querformat: Bild ist breiter als hoch (oder quadratisch) → Breite des Controls als Skalierungsbasis.
                        ' Breite = Control-Breite * Zoom
                        ' Höhe  = Control-Breite * Seitenverhältnis (Höhe/Breite) * Zoom
                        Return New Size(CInt(Control.Width * Zoom), CInt(Control.Width * CDec(Gif.Size.Height / Gif.Size.Width) * Zoom))
                    End If

                Case ImageSizeMode.Fill
                    ' Bild wird so skaliert, dass es das Control in einer Dimension vollständig ausfüllt.
                    ' Das Seitenverhältnis bleibt erhalten; die andere Dimension kann das Control überschreiten.
                    If Gif.Size.Width < Gif.Size.Height Then
                        ' Hochformat: Höhe des Controls als Basis, Breite wird proportional berechnet.
                        ' Breite = Control-Höhe / Seitenverhältnis (Höhe/Breite)
                        '        = Control-Höhe * (Bild-Breite / Bild-Höhe)
                        ' Höhe  = Control-Höhe (füllt das Control vollständig in der Höhe)
                        Return New Size(CInt(Control.Height / CDec(Gif.Size.Height / Gif.Size.Width)), Control.Height)
                    Else
                        ' Querformat: Breite des Controls als Basis, Höhe wird proportional berechnet.
                        ' Breite = Control-Breite (füllt das Control vollständig in der Breite)
                        ' Höhe  = Control-Breite * Seitenverhältnis (Höhe/Breite)
                        Return New Size(Control.Width, CInt(Control.Width * CDec(Gif.Size.Height / Gif.Size.Width)))
                    End If

                Case Else
                    ' Fallback: Originalgröße zurückgeben
                    Return New Size(Gif.Size.Width, Gif.Size.Height)

            End Select

        End Function

        ''' <summary>
        ''' Überprüft, ob der angegebene Zoomfaktor im zulässigen Bereich liegt, und korrigiert ihn gegebenenfalls.<br/>
        ''' Der gültige Bereich ist 1 bis 100 (entspricht 1 % bis 100 % Zoom).
        ''' </summary>
        ''' <param name="ZoomFactor">Der zu prüfende Zoomfaktor.</param>
        ''' <returns>
        ''' Den korrigierten Zoomfaktor innerhalb des Bereichs [1, 100].
        ''' </returns>
        Friend Shared Function CheckZoomFactorValue(ZoomFactor As Decimal) As Decimal

            Select Case ZoomFactor

                Case Is < 1
                    ' Unterschreitung: Mindestzoom (1) zurückgeben
                    Return 1

                Case Is > 100
                    ' Überschreitung: Maximalzoom (100) zurückgeben
                    Return 100

                Case Else
                    ' Wert liegt im gültigen Bereich → unverändert zurückgeben
                    Return ZoomFactor

            End Select

        End Function

        ''' <summary>
        ''' Überprüft, ob die angegebene Bildrate im zulässigen Bereich liegt, und korrigiert sie gegebenenfalls.<br/>
        ''' Der gültige Bereich ist 1 bis 50 Frames pro Sekunde.
        ''' </summary>
        ''' <param name="Frames">
        ''' Die zu prüfende Bildrate in Frames pro Sekunde (FPS).
        ''' </param>
        ''' <returns>
        ''' Die korrigierte Bildrate innerhalb des Bereichs [1, 50].
        ''' </returns>
        Friend Shared Function CheckFramesPerSecondValue(Frames As Decimal) As Decimal

            ' Stellt sicher, dass der FPS-Wert im zulässigen Bereich liegt (1 bis 50)
            Select Case Frames

                Case Is < 1
                    ' Unterschreitung: Mindest-FPS (1) zurückgeben
                    Return 1

                Case Is > 50
                    ' Überschreitung: Maximal-FPS (50) zurückgeben
                    Return 50

                Case Else
                    ' Wert liegt im gültigen Bereich → unverändert zurückgeben
                    Return Frames

            End Select

        End Function

    End Class

    ''' <summary>
    ''' Legt die Darstellungsqualität für die Skalierung einer GIF-Grafik fest.
    ''' </summary>
    Public Enum GifScalingQualityPreset

        ''' <summary>
        ''' Glättet Foto-Inhalte mit hochwertiger bikubischer Interpolation.
        ''' </summary>
        Photo = 0

        ''' <summary>
        ''' Erhält bei Pixel-Art harte Kanten durch Nächster-Nachbar-Interpolation.
        ''' </summary>
        PixelArt = 1

    End Enum

    ''' <summary>
    ''' Legt fest, wie eine Grafik innerhalb der verfügbaren Client-Fläche eines Controls angezeigt wird.
    ''' </summary>
    Public Enum ImageSizeMode

        ''' <summary>
        ''' Die Grafik wird in Originalgröße angezeigt; Ausrichtung erfolgt oben links.
        ''' </summary>
        Normal = 0

        ''' <summary>
        ''' Die Grafik wird in Originalgröße zentriert angezeigt.
        ''' </summary>
        CenterImage = 1

        ''' <summary>
        ''' Die Größe der Grafik wird einheitlich skaliert, sodass sie in den verfügbaren Bereich des Controls passt;
        ''' zentrierte Ausrichtung (1–100%).
        ''' </summary>
        Zoom = 2

        ''' <summary>
        ''' Die Grafik wird so skaliert, dass der verfügbare Bereich vollständig gefüllt wird; zentrierte Ausrichtung.
        ''' </summary>
        Fill = 3

    End Enum

End Namespace
