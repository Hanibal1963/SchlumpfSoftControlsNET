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

        Private _FramesPerSecond As Decimal ' Benutzerdefinierte Wiedergabegeschwindigkeit (nur aktiv bei CustomDisplaySpeed = True)
        Private _defaultFrameIntervalMs As Int32 ' Begrenztes FPS-Intervall und Fallback für GIF-Delays
        Private _Dimension As FrameDimension ' GIF-Frame-Dimension (bei GIFs i. d. R. Time)
        Private _Frame As Int32 ' Aktueller Frame-Index für die Timer-basierte Wiedergabe
        Private _MaxFrame As Int32 ' Letzter gültiger Frame-Index des geladenen GIFs
        Private _Autoplay As Boolean ' Steuert, ob die Animation automatisch starten soll
        Private _ZoomFactor As Decimal ' Zoomfaktor in Prozent für den Zoom-Modus
        Private _CustomDisplaySpeed As Boolean ' True = FPS-Intervall, False = GIF-Metadaten-Verzögerung verwenden
        Private _GifSizeMode As ImageSizeMode ' Art der Darstellung (Normal, Stretch, Zoom, ...)
        Private _scalingQualityPreset As GifScalingQualityPreset
        Private _Gif As Bitmap ' Aktuell angezeigtes Bild (intern immer als Bitmap gehalten)
        Private _OwnsGif As Boolean ' Kennzeichnet, ob _Gif vom Control geklont/erstellt wurde und damit freigegeben werden muss
        Private _IsAnimating As Boolean ' Interner Status, ob aktuell eine Animation aktiv läuft
        Private _playbackState As PlaybackState ' Gewünschter Wiedergabezustand unabhängig von der Sichtbarkeit
        Private _IsDisposed As Boolean  ' Kennzeichnet, dass die Ressourcenfreigabe des Controls begonnen hat
        Private _MinimumFrameIntervalMs As Int32 ' Untere Grenze für normalisierte Frame-Intervalle
        Private _MaximumFrameIntervalMs As Int32 ' Obere Grenze für normalisierte Frame-Intervalle
        Private _NormalizedFrameDelaysMs() As Int32 ' Normalisierte Delay-Werte je Frame
        Private _rawFrameDelaysMs() As Int32 ' Unnormalisierte Delay-Werte der aktuellen Bitmap
        Private _rawFrameDelayCount As Int32
        Private _rawFrameDelayCacheValid As Boolean
        Private _lastFrameDelayFallbackMs As Int32
        Private _gifWasExposed As Boolean
        Private _LastFrameTickTimestamp As Int64 ' Zeitstempel des letzten Timer-Ticks
        Private _AdaptiveTiming As Boolean ' Aktiviert Drift-Korrektur auf Basis der UI-Thread-Last

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
        ''' Wird ausgelöst, wenn die Animation gestartet oder nach einer Unterbrechung fortgesetzt wurde.
        ''' </summary>
        ''' <remarks>Änderungen der Geschwindigkeit oder Intervallgrenzen lösen das Ereignis nicht aus, solange die Wiedergabe weiterläuft.</remarks>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn die Animation gestartet oder nach einer Unterbrechung fortgesetzt wurde.")>
        Public Event AnimationStarted(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn die laufende Animation angehalten oder gestoppt wurde.
        ''' </summary>
        ''' <remarks>Änderungen der Geschwindigkeit oder Intervallgrenzen lösen das Ereignis nicht aus, solange die Wiedergabe weiterläuft.</remarks>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Wird ausgelöst wenn die laufende Animation angehalten oder gestoppt wurde.")>
        Public Event AnimationStopped(sender As Object, e As EventArgs)

#End Region

#Region "Interne Ereignisse"

        Private Event GifChanged()
        Private Event CustomDisplaySpeedChanged(previousIntervalMs As Int32)
        Private Event FramesPerSecondChanged(previousIntervalMs As Int32)

#End Region

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Steuert, ob die GIF‑Animation automatisch gestartet wird, sobald ein Bild vorhanden ist.
        ''' </summary>
        ''' <remarks>
        ''' Eine explizite Pause verändert diese Eigenschaft nicht.
        ''' </remarks>
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
                Me._playbackState = If(value, PlaybackState.Playing, PlaybackState.Stopped)
                RaiseEvent AutoPlayChanged(Me, EventArgs.Empty)
                Me.UpdateTimerState()
            End Set
        End Property

        ''' <summary>
        ''' Gibt die animierte GIF‑Grafik zurück oder legt diese fest.
        ''' </summary>
        ''' <remarks>
        ''' Bei der Zuweisung erstellt das Control eine eigene Bitmap-Kopie und gibt vom Aufrufer verwaltete Bilder nicht frei.
        ''' Der Getter liefert die interne Bitmap, die beim Bildwechsel oder bei der Ressourcenfreigabe freigegeben wird.
        ''' Bei animierten Bildern werden Änderungen ihrer Frame-Delay-Metadaten beim nächsten tatsächlichen Wechsel von FPS oder Intervallgrenzen eingelesen.
        ''' Nothing stellt eine Kopie des Standardbilds wieder her. Nach der Ressourcenfreigabe gibt der Getter Nothing zurück.
        ''' </remarks>
        ''' <exception cref="ObjectDisposedException">
        ''' Die Eigenschaft wird nach Beginn der Ressourcenfreigabe festgelegt.
        ''' </exception>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt die animierte Gif-Grafik zurück oder legt diese fest.")>
        <DefaultValue(GetType(Bitmap), "AniGifControl_Standard")>
        Public Property Gif() As Bitmap
            Get
                Dim image As Bitmap = Me._Gif
                If image IsNot Nothing Then Me._gifWasExposed = True
                Return image
            End Get
            Set(value As Bitmap)
                Me.SetGifImage(value)
            End Set
        End Property

        ''' <summary>
        ''' Gibt den Anzeigemodus (Skalierung/Ausrichtung) der GIF‑Grafik zurück oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Zoom passt die Grafik anhand von <see cref="ZoomFactor"/> in die Clientfläche ein.
        ''' Fill füllt die Clientfläche und kann die Grafik an den Rändern beschneiden.
        ''' Beide Modi erhalten das Seitenverhältnis bis auf die Rundung auf ganze Pixel.
        ''' </remarks>
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
        ''' Legt fest, ob die timerbasierte Wiedergabe die benutzerdefinierte Anzeigegeschwindigkeit (FPS)
        ''' oder die im GIF hinterlegten normalisierten Frame-Verzögerungen verwendet.
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
        ''' <remarks>
        ''' Werte außerhalb von 1 bis 50 werden begrenzt; gleichwertige Zuweisungen bleiben ohne Wirkung.
        ''' Die Bildrate dient auch als Fallback für fehlende oder ungültige GIF-Frame-Verzögerungen.
        ''' </remarks>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt die benutzerdefinierte Anzeigegeschwindigkeit in Bildern/Sekunde fest wenn CustomDisplaySpeed auf True festgelegt ist.")>
        <DefaultValue(30D)>
        Public Property FramesPerSecond As Decimal
            Get
                Return Me._FramesPerSecond
            End Get
            Set(value As Decimal)
                Dim validatedValue As Decimal = FunctionDefinitions.CheckFramesPerSecondValue(value)
                If Me._FramesPerSecond = validatedValue Then Return

                Dim previousIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
                Me._FramesPerSecond = validatedValue
                RaiseEvent FramesPerSecondChanged(previousIntervalMs)
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
        ''' <remarks>
        ''' Beim Aktivieren beginnt die Zeitmessung neu; beim Deaktivieren bleibt das aktuelle Timer-Intervall zunächst unverändert.
        ''' </remarks>
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
        ''' Legt den Zoomfaktor in Prozent für den Modus <see cref="ImageSizeMode.Zoom"/> fest.
        ''' Werte außerhalb von 1 bis 100 werden auf diesen Bereich begrenzt; der Standardwert ist 50.
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
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
            Me.UpdateStyles()
            Me.InitializeDefaults()
        End Sub

        ''' <summary>
        ''' Startet die Animation oder setzt eine pausierte Animation beim aktuellen Frame fort.
        ''' </summary>
        Public Sub StartAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Not Me.AutoPlay Then
                Me.AutoPlay = True
            ElseIf Me._playbackState <> PlaybackState.Playing Then
                Me._playbackState = PlaybackState.Playing
                Me.UpdateTimerState()
            End If
        End Sub

        ''' <summary>
        ''' Stoppt die Animation und hält den Timer an. Der aktuelle Frame bleibt erhalten.
        ''' </summary>
        Public Sub StopAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Me.AutoPlay Then
                Me.AutoPlay = False
            ElseIf Me._playbackState <> PlaybackState.Stopped Then
                Me._playbackState = PlaybackState.Stopped
                Me.UpdateTimerState()
            End If
        End Sub

        ''' <summary>
        ''' Hält die Wiedergabe beim aktuellen Frame an, ohne AutoPlay zu verändern.
        ''' Ist die Wiedergabe bereits pausiert oder gestoppt, bleibt der Aufruf ohne Wirkung.
        ''' </summary>
        Public Sub PauseAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed OrElse Me._playbackState <> PlaybackState.Playing Then Return

            Me._playbackState = PlaybackState.Paused
            Me.StopAnimationCore(True)
        End Sub

        ''' <summary>
        ''' Setzt eine pausierte Wiedergabe beim aktuellen Frame fort und beginnt dessen Intervall erneut.
        ''' Ist die Wiedergabe nicht pausiert, bleibt der Aufruf ohne Wirkung.
        ''' </summary>
        Public Sub ResumeAnimation()
            If Me._IsDisposed OrElse Me.IsDisposed OrElse Me._playbackState <> PlaybackState.Paused Then Return

            Me._playbackState = PlaybackState.Playing
            Me.UpdateTimerState()
        End Sub

        ''' <summary>
        ''' Wählt einen Frame aus, ohne den Wiedergabezustand zu verändern.
        ''' Bei laufender Wiedergabe beginnt das Intervall des gewählten Frames erneut.
        ''' </summary>
        ''' <param name="index">Nullbasierter Index des anzuzeigenden Frames.</param>
        ''' <exception cref="ArgumentOutOfRangeException">Der Index liegt außerhalb des gültigen Framebereichs.</exception>
        ''' <exception cref="InvalidOperationException">Es ist kein Bild vorhanden.</exception>
        Public Sub SeekFrame(index As Int32)
            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Me._Gif Is Nothing Then Throw New InvalidOperationException("Es ist kein Bild vorhanden.")
            If index < 0 OrElse index > Me._MaxFrame Then Throw New ArgumentOutOfRangeException(NameOf(index))
            If index = Me._Frame Then Return

            Dim unused = Me._Gif.SelectActiveFrame(Me._Dimension, index)
            Me._Frame = index
            If Me._diagnosticsEnabled Then Me._diagnosticsCollector.CancelPendingFrame()

            If Me._IsAnimating AndAlso Me._playbackState = PlaybackState.Playing Then
                ' Das neue Intervall beginnt am gewählten Frame, ohne Start-/Stoppereignisse auszulösen.
                Me.Timer.Stop()
                Me.Timer.Interval = Me.GetFrameIntervalForIndex(index)
                If Me._AdaptiveTiming Then Me._LastFrameTickTimestamp = Stopwatch.GetTimestamp()
                Me.Timer.Start()
            End If

            Me.InvalidateIfVisible()
        End Sub

        ''' <summary>
        ''' Gibt den aktuellen Wiedergabezustand zurück (Playing, Paused, Stopped).
        ''' </summary>
        ''' <returns>
        ''' Der aktuelle Wiedergabezustand.
        ''' </returns>
        Public Function GetPlaybackState() As PlaybackState
            Return Me._playbackState
        End Function

#End Region

#Region "Interne Methoden"

        Private Sub InitializeDefaults()
            Me._FramesPerSecond = 30D
            Me._ZoomFactor = 50D
            Me._GifSizeMode = ImageSizeMode.Normal
            Me._scalingQualityPreset = GifScalingQualityPreset.Photo
            Me._Gif = DirectCast(My.Resources.AniGifControl_Standard.Clone(), Bitmap)
            Me._OwnsGif = True
            Me._CustomDisplaySpeed = False
            Me._Autoplay = False
            Me._IsAnimating = False
            Me._playbackState = PlaybackState.Stopped
            Me._IsDisposed = False
            Me._MinimumFrameIntervalMs = 20
            Me._MaximumFrameIntervalMs = 500
            Me.UpdateDefaultFrameIntervalMs()
            Me._NormalizedFrameDelaysMs = Array.Empty(Of Int32)()
            Me._rawFrameDelaysMs = Array.Empty(Of Int32)()
            Me._rawFrameDelayCount = 0
            Me._rawFrameDelayCacheValid = False
            Me._lastFrameDelayFallbackMs = 0
            Me._gifWasExposed = False
            Me._LastFrameTickTimestamp = 0
            Me._AdaptiveTiming = False
            Me.InitializeGifFrames()
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
            If Me._diagnosticsEnabled Then
                Me.RefreshDiagnosticsAncestors()
                Me.UpdateDiagnosticsActivity()
            End If
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)

            MyBase.OnPaint(e)

            ' Null-Schutz
            If Me._Gif Is Nothing Then Return

            Dim diagnostics As AniGifDiagnosticsCollector = Nothing
            Dim diagnosticsVersion As Int64 = 0
            Dim frameGeneration As Int64 = 0
            Dim renderStartTimestamp As Int64 = 0
            If Me._diagnosticsEnabled Then
                Me.UpdateDiagnosticsActivity()
                diagnostics = Me._diagnosticsCollector
                diagnosticsVersion = diagnostics.SessionVersion
                frameGeneration = diagnostics.PendingFrameGeneration
                renderStartTimestamp = Stopwatch.GetTimestamp()
            End If

            ' Variable für Zeichenfläche
            Dim g As Graphics = e.Graphics

            ' Größe der Zeichenfläche berechnen
            Dim rectstartsize As Size = FunctionDefinitions.GetRectStartSize(Me._GifSizeMode, Me, Me._Gif, Me._ZoomFactor / 100)
            If rectstartsize.Width <= 0 OrElse rectstartsize.Height <= 0 Then Return

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
            Dim imageRectangle As New Rectangle(rectstartpoint, rectstartsize)
            g.DrawImage(Me._Gif, imageRectangle)

            If diagnostics IsNot Nothing AndAlso Me._diagnosticsEnabled AndAlso
                Me._diagnosticsCollector Is diagnostics AndAlso diagnostics.SessionVersion = diagnosticsVersion Then
                Dim renderTicks As Int64 = Stopwatch.GetTimestamp() - renderStartTimestamp
                Dim intersectsClip As Boolean = False
                If frameGeneration <> 0 Then
                    Dim clippedRectangle As Rectangle = Rectangle.Intersect(imageRectangle, e.ClipRectangle)
                    clippedRectangle = Rectangle.Intersect(clippedRectangle, Me.ClientRectangle)
                    intersectsClip = clippedRectangle.Width > 0 AndAlso clippedRectangle.Height > 0 AndAlso g.IsVisible(clippedRectangle)
                End If
                diagnostics.RecordDraw(renderTicks, frameGeneration, intersectsClip)
            End If

        End Sub

        Private Sub InitializeGifFrames()
            Me._Dimension = Nothing
            Me._MaxFrame = 0
            Me._Frame = 0
            Me._rawFrameDelayCacheValid = False

            If Me._Gif Is Nothing Then
                Me.ClearFrameDelayCache()
                Return
            End If

            ' Frameauswahl und Metadaten für Standard- und zugewiesene Bilder identisch vorbereiten.
            Me._Dimension = New FrameDimension(Me._Gif.FrameDimensionsList(0))
            Dim frameCount As Int32 = Me._Gif.GetFrameCount(Me._Dimension)
            Me._MaxFrame = Math.Max(0, frameCount - 1)
            If Me._MaxFrame > 0 Then
                Dim unused = Me._Gif.SelectActiveFrame(Me._Dimension, 0)
            End If
            Me.RefreshFrameDelayCache()
        End Sub

        Private Sub AniGif_GifChange() Handles Me.GifChanged
            If Me._IsDisposed OrElse Me.IsDisposed Then Return

            Me.InitializeGifFrames()
            Me.Invalidate()
            Me.UpdateTimerState()
        End Sub

        Private Sub AniGif_CustomDisplaySpeedChanged(previousIntervalMs As Int32) Handles Me.CustomDisplaySpeedChanged

            ' Nach einem Wechsel zwischen FPS und GIF-Verzögerungen den Timer neu konfigurieren.
            Me.UpdateTimerState(previousIntervalMs)

        End Sub

        Private Sub AniGif_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed

            Me.DisposeAnimationResources()

        End Sub

        Private Sub DisposeAnimationResources()

            If Me._IsDisposed Then Return
            Me._IsDisposed = True
            Me._playbackState = PlaybackState.Stopped

            ' Den Timer vor dem Freigeben von Bitmap und Komponenten anhalten.
            Me.StopAnimationCore(False)
            If Me._diagnosticsEnabled Then Me.DiagnosticsEnabled = False

            ' Nur intern erzeugte Bitmaps freigeben (fremde Instanzen nicht ungefragt disposen).
            If Me._OwnsGif AndAlso Me._Gif IsNot Nothing Then
                Me._Gif.Dispose()
                Me._Gif = Nothing
                Me._OwnsGif = False
            End If
            Me.ClearFrameDelayCache()
            Me._gifWasExposed = False

        End Sub

        Private Sub AniGif_FramesPerSecondChanged(previousIntervalMs As Int32) Handles Me.FramesPerSecondChanged

            ' Sicherheitsprüfung
            If Me._FramesPerSecond < 1D Then Me._FramesPerSecond = 1D

            Me.UpdateDefaultFrameIntervalMs()
            Me.RefreshFrameDelayCache(True)
            Me.UpdateTimerState(previousIntervalMs)

        End Sub

        Private Function NormalizeFrameIntervalMs(value As Int32) As Int32

            Dim minInterval As Int32 = Math.Max(1, Me._MinimumFrameIntervalMs)
            Dim maxInterval As Int32 = Math.Max(minInterval, Me._MaximumFrameIntervalMs)
            Return Math.Min(maxInterval, Math.Max(minInterval, value))

        End Function

        Private Function GetDefaultFrameIntervalMs() As Int32
            Return Me._defaultFrameIntervalMs
        End Function

        Private Sub UpdateDefaultFrameIntervalMs()
            Dim fpsInterval As Int32 = CInt(Math.Round(1000D / Math.Max(Me._FramesPerSecond, 1D), MidpointRounding.AwayFromZero))
            Me._defaultFrameIntervalMs = Me.NormalizeFrameIntervalMs(Math.Max(1, fpsInterval))
        End Sub

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

        Private Sub ClearFrameDelayCache()
            Me._rawFrameDelaysMs = Array.Empty(Of Int32)()
            Me._rawFrameDelayCount = 0
            Me._rawFrameDelayCacheValid = False
            Me._lastFrameDelayFallbackMs = 0
            Me._NormalizedFrameDelaysMs = Array.Empty(Of Int32)()
        End Sub

        Private Function ReadFrameDelayMetadata(frameCount As Int32) As Boolean
            Dim metadataChanged As Boolean = Not Me._rawFrameDelayCacheValid
            Dim previousCount As Int32 = Me._rawFrameDelayCount
            Me._rawFrameDelayCacheValid = False
            Dim delayBytes As Byte() = Nothing

            Try
                Dim delayProperty As PropertyItem = Me._Gif.GetPropertyItem(FrameDelayPropertyTag)
                If delayProperty IsNot Nothing Then delayBytes = delayProperty.Value
            Catch ex As ArgumentException
                ' Ohne Delay-Metadaten werden für alle Frames Fallbacks verwendet.
            End Try

            Dim availableCount As Int32 = If(delayBytes Is Nothing, 0, Math.Min(frameCount, delayBytes.Length \ 4))
            If Me._rawFrameDelaysMs.Length <> frameCount Then
                Me._rawFrameDelaysMs = New Int32(frameCount - 1) {}
                metadataChanged = True
            End If
            If availableCount <> previousCount Then metadataChanged = True

            For i As Int32 = 0 To availableCount - 1
                Dim delayInCentiseconds As Int32 = BitConverter.ToInt32(delayBytes, i * 4)
                Dim delayInMs As Int32 = delayInCentiseconds * 10
                If Me._rawFrameDelaysMs(i) <> delayInMs Then metadataChanged = True
                Me._rawFrameDelaysMs(i) = delayInMs
            Next

            Me._rawFrameDelayCount = availableCount
            Me._rawFrameDelayCacheValid = True
            Return metadataChanged
        End Function

        Private Sub RefreshFrameDelayCache(Optional fallbackOnly As Boolean = False)
            If Me._Gif Is Nothing OrElse Me._MaxFrame <= 0 Then
                Me.ClearFrameDelayCache()
                Return
            End If

            Dim frameCount As Int32 = Me._MaxFrame + 1
            Dim metadataChanged As Boolean = False
            ' Nach der Herausgabe kann die Bitmap auch später noch extern verändert werden.
            If Not Me._rawFrameDelayCacheValid OrElse Me._gifWasExposed Then
                metadataChanged = Me.ReadFrameDelayMetadata(frameCount)
            End If

            If Me._NormalizedFrameDelaysMs.Length <> frameCount Then
                Me._NormalizedFrameDelaysMs = New Int32(frameCount - 1) {}
                metadataChanged = True
            End If

            Dim fallbackDelay As Int32 = Me.GetDefaultFrameIntervalMs()
            If fallbackOnly AndAlso Not metadataChanged AndAlso Me._lastFrameDelayFallbackMs = fallbackDelay Then Return

            For i As Int32 = 0 To frameCount - 1
                If i < Me._rawFrameDelayCount AndAlso Me._rawFrameDelaysMs(i) > 0 Then
                    If Not fallbackOnly OrElse metadataChanged Then
                        Me._NormalizedFrameDelaysMs(i) = Me.NormalizeFrameIntervalMs(Me._rawFrameDelaysMs(i))
                    End If
                Else
                    Me._NormalizedFrameDelaysMs(i) = fallbackDelay
                End If
            Next
            Me._lastFrameDelayFallbackMs = fallbackDelay
        End Sub

        Private Sub InvalidateIfVisible(Optional isTimerFrame As Boolean = False)

            If Me._IsDisposed OrElse Me.IsDisposed Then Return
            If Not Me.IsHandleCreated Then Return
            If Not Me.IsControlTreeVisible() Then Return
            If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

            ' Vor Invalidate markieren, da dessen Ereignishandler erneut in das Control eintreten können.
            If isTimerFrame AndAlso Me._diagnosticsEnabled Then Me._diagnosticsCollector.QueueTimerFrame()
            Me.Invalidate()

        End Sub

        Private Function IsControlTreeVisible() As Boolean

            ' Der WinForms-Getter berücksichtigt bereits die Sichtbarkeit der Elternkette.
            Return Me.Visible

        End Function

        Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick

            If Me._IsDisposed OrElse Me.IsDisposed OrElse Me.DesignMode OrElse Not Me.AutoPlay OrElse Me._playbackState <> PlaybackState.Playing Then Exit Sub
            Dim isVisible As Boolean? = Nothing
            If Me._diagnosticsEnabled Then
                If Me._Gif IsNot Nothing Then isVisible = Me.IsControlTreeVisible()
                ' Die Diagnoseprüfung löst vor der Timerprüfung keine Ereignishandler aus.
                Me.UpdateDiagnosticsActivity(isVisible)
            End If
            If Me._Gif Is Nothing Then Exit Sub
            If Not If(isVisible, Me.IsControlTreeVisible()) Then Exit Sub

            ' wenn Frames = 0 ist das Bild nicht animiert -> Ende
            If Me._MaxFrame = 0 Then Exit Sub

            Dim elapsedMs As Int32 = 0
            If Me._AdaptiveTiming Then
                Dim currentTimestamp As Int64 = Stopwatch.GetTimestamp()
                If Me._LastFrameTickTimestamp > 0 Then
                    elapsedMs = CInt(Math.Max(0, (currentTimestamp - Me._LastFrameTickTimestamp) * 1000L \ Stopwatch.Frequency))
                End If
                Me._LastFrameTickTimestamp = currentTimestamp
            End If

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
            Me.InvalidateIfVisible(True)

        End Sub

        Private Sub SetGifImage(value As Bitmap)

#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
            If Me._IsDisposed OrElse Me.IsDisposed Then Throw New ObjectDisposedException(NameOf(AniGif))
#Enable Warning CA1513

            ' Auch die Standardressource gehört jeder Instanz als eigene Bitmap-Kopie.
            Dim source As Bitmap = If(value, My.Resources.AniGifControl_Standard)
            Dim imageCopy As Bitmap = DirectCast(source.Clone(), Bitmap)

            ' Vorherige Animation stoppen bevor neues Bild gesetzt wird
            Me.StopAnimationCore(False)

            If Me._OwnsGif AndAlso Me._Gif IsNot Nothing Then
                ' Vorheriges, intern besessenes Bild freigeben
                Me._Gif.Dispose()
            End If

            Me._gifWasExposed = False
            Me._rawFrameDelayCacheValid = False
            Me._Gif = imageCopy
            Me._OwnsGif = True

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
            Dim previousIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
            Me._CustomDisplaySpeed = value
            RaiseEvent CustomDisplaySpeedChanged(previousIntervalMs)

        End Sub

        Private Sub SetMinimumFrameIntervalMs(value As Int32)

            Dim validatedValue As Int32 = Math.Max(1, value)
            If Me._MinimumFrameIntervalMs = validatedValue Then Return

            Dim previousIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
            Me._MinimumFrameIntervalMs = validatedValue
            If Me._MaximumFrameIntervalMs < Me._MinimumFrameIntervalMs Then
                Me._MaximumFrameIntervalMs = Me._MinimumFrameIntervalMs
            End If

            Me.UpdateDefaultFrameIntervalMs()
            Me.RefreshFrameDelayCache()
            Me.UpdateTimerState(previousIntervalMs)

        End Sub

        Private Sub SetMaximumFrameIntervalMs(value As Int32)

            Dim validatedValue As Int32 = Math.Max(1, value)
            If Me._MaximumFrameIntervalMs = validatedValue Then Return

            Dim previousIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
            Me._MaximumFrameIntervalMs = validatedValue
            If Me._MaximumFrameIntervalMs < Me._MinimumFrameIntervalMs Then
                Me._MinimumFrameIntervalMs = Me._MaximumFrameIntervalMs
            End If

            Me.UpdateDefaultFrameIntervalMs()
            Me.RefreshFrameDelayCache()
            Me.UpdateTimerState(previousIntervalMs)

        End Sub

        Private Sub SetAdaptiveTiming(value As Boolean)

            If Me._AdaptiveTiming = value Then Return
            Me._AdaptiveTiming = value
            Me._LastFrameTickTimestamp = If(value, Stopwatch.GetTimestamp(), 0L)

        End Sub

        Private Sub SetZoomFactor(value As Decimal)

            Dim validatedZoomFactor As Decimal = FunctionDefinitions.CheckZoomFactorValue(value)
            If Me._ZoomFactor = validatedZoomFactor Then Return

            Me._ZoomFactor = validatedZoomFactor
            ' neu zeichnen
            Me.Invalidate()

        End Sub

        Private Sub UpdateTimerState(Optional previousIntervalMs As Int32? = Nothing)

            If Me._IsDisposed OrElse Me.IsDisposed Then Return

            Dim playbackRequested As Boolean = Not Me.DesignMode AndAlso Me._Autoplay AndAlso
                Me._playbackState = PlaybackState.Playing AndAlso Me._Gif IsNot Nothing AndAlso
                Me.IsControlTreeVisible()

            If Me._IsAnimating AndAlso (Not playbackRequested OrElse Me._MaxFrame <= 0) Then
                Me.StopAnimationCore(True)
                ' Ein Ereignishandler kann den Zustand bereits erneut geändert haben.
                Return
            End If

            If Not playbackRequested Then Return
            If Me._MaxFrame <= 0 Then
                RaiseEvent NoAnimation(Me, EventArgs.Empty)
                Return
            End If

            Dim targetIntervalMs As Int32 = Me.GetFrameIntervalForIndex(Me._Frame)
            If Me._IsAnimating Then
                ' Das Sollintervall kann vom adaptiv korrigierten Timerintervall abweichen.
                If previousIntervalMs.HasValue AndAlso previousIntervalMs.Value <> targetIntervalMs AndAlso Me.Timer.Interval <> targetIntervalMs Then
                    Me.Timer.Interval = targetIntervalMs
                    If Me._AdaptiveTiming Then Me._LastFrameTickTimestamp = Stopwatch.GetTimestamp()
                End If
                Return
            End If

            Me.Timer.Interval = targetIntervalMs
            If Me._AdaptiveTiming Then Me._LastFrameTickTimestamp = Stopwatch.GetTimestamp()
            Me.Timer.Start()
            Me._IsAnimating = True
            If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
            RaiseEvent AnimationStarted(Me, EventArgs.Empty)

        End Sub

        Private Sub StopAnimationCore(raiseAnimationStopped As Boolean)

            Dim wasAnimating As Boolean = Me._IsAnimating

            Me.Timer.Stop()
            Me._LastFrameTickTimestamp = 0
            Me._IsAnimating = False
            If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()

            If raiseAnimationStopped AndAlso wasAnimating Then
                RaiseEvent AnimationStopped(Me, EventArgs.Empty)
            End If

        End Sub

#End Region

    End Class

End Namespace
