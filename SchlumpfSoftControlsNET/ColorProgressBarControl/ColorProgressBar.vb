' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBar.vb
' Author: Andreas Sauer
' Datum: 29.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Globalization
Imports System.Resources
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Automation
Imports System.Windows.Automation.Provider

Namespace ColorProgressBarControl

    ''' <summary>Ein benutzerdefiniertes Windows Forms-Steuerelement zur Anzeige eines farbigen Fortschrittsbalkens mit
    ''' optionalem Rahmen und Glanzeffekt.</summary>
    <Description("Control zum Anzeigen eines farbigen Fortschrittbalkens.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ColorProgressBar), "ColorProgressBarControl.ColorProgressBar.bmp")>
    Public Class ColorProgressBar

        Inherits UserControl

#Region "Variablen"

        ''' <summary>Vorgesehene Fortschrittseinheit des Steuerelements.</summary>
        Private ReadOnly _ProgressUnit As Integer = 20

        ''' <summary>Verwaltet die lokalisierten Accessibility-Ressourcen.</summary>
        Private Shared ReadOnly _accessibilityResources As New ResourceManager("SchlumpfSoft.ControlsNET.ColorProgressBarAccessibility", GetType(ColorProgressBar).Assembly)

        ''' <summary>UI-Automation-Provider für das aktuelle Fensterhandle, sofern erstellt.</summary>
        Private _uiaProvider As ProgressAutomationProvider

        ''' <summary>Aktueller begrenzter Fortschrittswert.</summary>
        Private _ProgressValue As Integer = 1

        ''' <summary>Aktueller Maximalwert des Fortschritts.</summary>
        Private _MaxValue As Integer = 10

        ''' <summary>Gibt an, ob der Rahmen angezeigt wird.</summary>
        Private _ShowBorder As Boolean = True

        ''' <summary>Gibt an, ob der Glanzeffekt angezeigt wird.</summary>
        Private _IsGlossy As Boolean = True

        ''' <summary>Farbe des gefüllten Bereichs.</summary>
        Private _BarColor As System.Drawing.Color = Color.Blue

        ''' <summary>Farbe des leeren Bereichs.</summary>
        Private _EmptyColor As System.Drawing.Color = Color.Silver

        ''' <summary>Farbe des Rahmens.</summary>
        Private _BorderColor As System.Drawing.Color = Color.Black

        ''' <summary>Wiederverwendeter Pinsel für den gefüllten Bereich.</summary>
        Private _barBrush As SolidBrush

        ''' <summary>Wiederverwendeter Pinsel für den leeren Bereich.</summary>
        Private _emptyBrush As SolidBrush

        ''' <summary>Wiederverwendeter Pinsel für den Glanzeffekt.</summary>
        Private _glossBrush As SolidBrush

        ''' <summary>Gibt an, ob bereits Geometriedaten berechnet wurden.</summary>
        Private _isGeometryCached As Boolean

        ''' <summary>Clientfläche bei der letzten Geometrieberechnung.</summary>
        Private _cachedClientBounds As Rectangle

        ''' <summary>Steuerelementgröße bei der letzten Geometrieberechnung.</summary>
        Private _cachedControlSize As Size

        ''' <summary>Innenabstand bei der letzten Geometrieberechnung.</summary>
        Private _cachedPadding As Padding

        ''' <summary>Fortschrittswert bei der letzten Berechnung der Bereichsrechtecke.</summary>
        Private _cachedProgressValue As Int32

        ''' <summary>Maximum bei der letzten Berechnung der Bereichsrechtecke.</summary>
        Private _cachedMaximumValue As Int32

        ''' <summary>Zwischengespeicherte Innenfläche ohne Rahmen.</summary>
        Private _cachedContentBounds As Rectangle

        ''' <summary>Zwischengespeicherte Fläche des gefüllten Bereichs.</summary>
        Private _cachedFillBounds As Rectangle

        ''' <summary>Zwischengespeicherte Fläche des leeren Bereichs.</summary>
        Private _cachedEmptyBounds As Rectangle

        ''' <summary>Zwischengespeicherte Fläche des Glanzeffekts.</summary>
        Private _cachedGlossBounds As Rectangle

#End Region

#Region "öffentliche Eigenschaften"

        ''' <summary>
        ''' Gibt den aktuellen Fortschrittswert zurück oder legt diesen fest (Bereich: 0 bis
        ''' <see cref="ProgressMaximumValue"/>; Standardwert: 1). Werte außerhalb des Bereichs werden begrenzt.
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
                Me.UpdateProgress(value, Me._MaxValue)
            End Set
        End Property

        ''' <summary>
        ''' Gibt den Maximalwert des Fortschrittsbalkens zurück oder legt diesen fest (Standardwert: 10).
        ''' Werte kleiner als 1 werden auf 1 begrenzt und ein darüber liegender Fortschrittswert wird angepasst.
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
                Me.UpdateProgress(Me._ProgressValue, value)
            End Set
        End Property

        ''' <summary>
        ''' Gibt die Farbe des gefüllten Fortschrittsbereichs zurück oder legt diese fest.
        ''' Transparente Farben und Systemfarben werden unterstützt.
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
                Dim colorChanged As Boolean = Me._BarColor <> value
                Me._BarColor = value
                If colorChanged Then
                    DisposeCachedBrush(Me._barBrush)
                End If
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gibt die Farbe des leeren Fortschrittsbereichs zurück oder legt diese fest.
        ''' Transparente Farben und Systemfarben werden unterstützt.
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
                Dim colorChanged As Boolean = Me._EmptyColor <> value
                Me._EmptyColor = value
                If colorChanged Then
                    DisposeCachedBrush(Me._emptyBrush)
                End If
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

#Region "öffentliche Methoden"

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

#End Region

#Region "interne Methoden"

        ''' <summary>
        ''' Begrenzt Maximum und Fortschrittswert und aktualisiert sie gemeinsam.
        ''' </summary>
        ''' <param name="value">Der gewünschte Fortschrittswert.</param>
        ''' <param name="maximum">Der gewünschte Maximalwert.</param>
        Private Sub UpdateProgress(value As Int32, maximum As Int32)

            ' Das Maximum zuerst begrenzen, damit der Wert stets innerhalb des gültigen Bereichs liegt.
            Dim boundedMaximum As Int32 = Math.Max(1, maximum)
            Dim boundedValue As Int32 = Math.Max(0, Math.Min(value, boundedMaximum))

            If boundedMaximum <> Me._MaxValue OrElse boundedValue <> Me._ProgressValue Then
                Dim previousMaximum As Int32 = Me._MaxValue
                Dim previousValue As Int32 = Me._ProgressValue
                Me._MaxValue = boundedMaximum
                Me._ProgressValue = boundedValue
                ' Auch bei einem Fehler der Accessibility-Benachrichtigung muss neu gezeichnet werden.
                Try
                    Me.NotifyProgressAccessibility(previousValue, previousMaximum, boundedValue, boundedMaximum)
                Finally
                    Me.Invalidate()
                End Try
            End If

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
        ''' Setzt die initiale Rahmenfarbe und Accessibility-Eigenschaften des Steuerelements.
        ''' </summary>
        Private Sub InitializeDefaults()

            Me.BackColor = Me._BorderColor
            Me.InitializeAccessibility()

        End Sub

        ''' <summary>
        ''' Setzt die Accessibility-Rolle, die von der Anwendung überschrieben werden kann.
        ''' </summary>
        Private Sub InitializeAccessibility()

            Me.AccessibleRole = AccessibleRole.ProgressBar

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
        ''' <param name="sender">Das auslösende Steuerelement.</param>
        ''' <param name="e">Die Ereignisdaten.</param>
        Private Sub ColorProgressBar_PaddingChanged(sender As Object, e As EventArgs) Handles Me.PaddingChanged

            Me.UpdatePadding()

        End Sub

        ''' <summary>
        ''' Gibt die eigenen Zeichenressourcen beim Entsorgen des Steuerelements frei.
        ''' </summary>
        ''' <param name="sender">Das entsorgte Steuerelement.</param>
        ''' <param name="e">Die Ereignisdaten.</param>
        Private Sub ColorProgressBar_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed

            ' Jeder Pinsel wird auch dann freigegeben, wenn ein vorheriges Dispose fehlschlägt.
            Try
                DisposeCachedBrush(Me._barBrush)
            Finally
                Try
                    DisposeCachedBrush(Me._emptyBrush)
                Finally
                    DisposeCachedBrush(Me._glossBrush)
                End Try
            End Try

        End Sub

        ''' <summary>
        ''' Benachrichtigt Accessibility-Clients über einen geänderten Fortschritt.
        ''' </summary>
        ''' <param name="previousValue">Der vorherige Fortschrittswert.</param>
        ''' <param name="previousMaximum">Das vorherige Maximum.</param>
        ''' <param name="value">Der neue Fortschrittswert.</param>
        ''' <param name="maximum">Das neue Maximum.</param>
        Private Sub NotifyProgressAccessibility(previousValue As Int32, previousMaximum As Int32, value As Int32, maximum As Int32)

            If Not Me.IsHandleCreated Then Return

            Dim provider As ProgressAutomationProvider = Me._uiaProvider
            ' UIA-Ereignisse nur für einen vorhandenen Provider und aktive Clients auslösen.
            If provider IsNot Nothing AndAlso Me.HasProgressAccessibleRole() AndAlso AutomationInteropProvider.ClientsAreListening Then
                If previousValue <> value Then
                    AutomationInteropProvider.RaiseAutomationPropertyChangedEvent(provider,
                        New AutomationPropertyChangedEventArgs(RangeValuePatternIdentifiers.ValueProperty, CDbl(previousValue), CDbl(value)))
                End If
                If previousMaximum <> maximum Then
                    AutomationInteropProvider.RaiseAutomationPropertyChangedEvent(provider,
                        New AutomationPropertyChangedEventArgs(RangeValuePatternIdentifiers.MaximumProperty, CDbl(previousMaximum), CDbl(maximum)))
                End If

                Dim previousText As String = FormatAccessibleProgress(previousValue, previousMaximum)
                Dim currentText As String = FormatAccessibleProgress(value, maximum)
                If Not String.Equals(previousText, currentText, StringComparison.Ordinal) Then
                    AutomationInteropProvider.RaiseAutomationPropertyChangedEvent(provider,
                        New AutomationPropertyChangedEventArgs(ValuePatternIdentifiers.ValueProperty, previousText, currentText))
                End If
            End If

            ' Die MSAA-Kind-ID -1 bezeichnet das Steuerelement selbst.
            Me.AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1)

        End Sub

        ''' <summary>
        ''' Entfernt einen zwischengespeicherten Brush und gibt seine Ressourcen frei.
        ''' </summary>
        ''' <param name="brush">Der zu entfernende Brush; anschließend <see langword="Nothing"/>.</param>
        Private Shared Sub DisposeCachedBrush(ByRef brush As SolidBrush)

            If brush IsNot Nothing Then
                Dim oldBrush As SolidBrush = brush
                brush = Nothing
                oldBrush.Dispose()
            End If

        End Sub

        ''' <summary>
        ''' Prüft, ob die Accessibility-Rolle dem Fortschrittsbalken entspricht.
        ''' </summary>
        ''' <returns><see langword="True"/>, wenn die Standard- oder Fortschrittsrolle gesetzt ist.</returns>
        Private Function HasProgressAccessibleRole() As Boolean

            Return Me.AccessibleRole = AccessibleRole.Default OrElse Me.AccessibleRole = AccessibleRole.ProgressBar

        End Function

        ''' <summary>
        ''' Liest einen lokalisierten Accessibility-Text aus den Control-Ressourcen.
        ''' </summary>
        ''' <param name="name">Der Ressourcenschlüssel.</param>
        ''' <returns>Der lokalisierte Text.</returns>
        Private Shared Function GetAccessibilityText(name As String) As String

            Dim text As String = _accessibilityResources.GetString(name, CultureInfo.CurrentUICulture)
            If text Is Nothing Then
                Throw New MissingManifestResourceException($"Die Accessibility-Ressource '{name}' fehlt.")
            End If
            Return text

        End Function

        ''' <summary>
        ''' Berechnet einen lokalisierten, abgerundeten Prozentwert für Hilfstechnologien.
        ''' </summary>
        ''' <param name="value">Der begrenzte Fortschrittswert.</param>
        ''' <param name="maximum">Der begrenzte Maximalwert.</param>
        ''' <returns>Der Prozentwert als lokalisierter Text.</returns>
        Private Shared Function FormatAccessibleProgress(value As Int32, maximum As Int32) As String

            Dim percentage As Int64 = (CLng(value) * 100L) \ maximum
            Return (CDbl(percentage) / 100).ToString("P0", CultureInfo.CurrentCulture)

        End Function

        ''' <summary>
        ''' Zeichnet den Fortschritt, den leeren Bereich und den optionalen Glanzeffekt.
        ''' </summary>
        ''' <param name="e">Stellt die Zeichenfläche und den zu aktualisierenden Bereich bereit.</param>
        ''' <exception cref="ObjectDisposedException">Das Steuerelement wird oder wurde bereits freigegeben.</exception>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)

#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
            If Me.IsDisposed OrElse Me.Disposing Then
                Throw New ObjectDisposedException(NameOf(ColorProgressBar))
            End If
#Enable Warning CA1513

            Me.UpdateCachedGeometry()

            ' Pinsel werden bei Bedarf erzeugt und bis zur Farbänderung wiederverwendet.
            If Me._cachedFillBounds.Width > 0 AndAlso Me._cachedFillBounds.Height > 0 Then
                If Me._barBrush Is Nothing Then
                    Me._barBrush = New SolidBrush(Me._BarColor)
                End If
                e.Graphics.FillRectangle(Me._barBrush, Me._cachedFillBounds)
            End If

            If Me._cachedEmptyBounds.Width > 0 AndAlso Me._cachedEmptyBounds.Height > 0 Then
                If Me._emptyBrush Is Nothing Then
                    Me._emptyBrush = New SolidBrush(Me._EmptyColor)
                End If
                e.Graphics.FillRectangle(Me._emptyBrush, Me._cachedEmptyBounds)
            End If

            If Me._IsGlossy AndAlso Me._cachedGlossBounds.Width > 0 AndAlso Me._cachedGlossBounds.Height > 0 Then
                If Me._glossBrush Is Nothing Then
                    Me._glossBrush = New SolidBrush(Color.FromArgb(100, 255, 255, 255))
                End If
                e.Graphics.FillRectangle(Me._glossBrush, Me._cachedGlossBounds)
            End If

            MyBase.OnPaint(e)
        End Sub

        ''' <summary>
        ''' Aktualisiert zwischengespeicherte Systemfarben und löst das zugehörige Ereignis aus.
        ''' </summary>
        ''' <param name="e">Stellt die Ereignisdaten bereit.</param>
        Protected Overrides Sub OnSystemColorsChanged(e As EventArgs)

            Dim usesSystemColor As Boolean = Me._BarColor.IsSystemColor OrElse Me._EmptyColor.IsSystemColor

            ' Ereignishandler können bereits synchron mit den neuen Systemfarben zeichnen.
            If Me._BarColor.IsSystemColor Then
                DisposeCachedBrush(Me._barBrush)
            End If
            If Me._EmptyColor.IsSystemColor Then
                DisposeCachedBrush(Me._emptyBrush)
            End If

            MyBase.OnSystemColorsChanged(e)

            If usesSystemColor AndAlso Not Me.IsDisposed AndAlso Not Me.Disposing Then
                Me.Invalidate()
            End If
        End Sub

        ''' <summary>
        ''' Erstellt das MSAA-Objekt für den Fortschrittsbalken.
        ''' </summary>
        ''' <returns>Das Accessibility-Objekt des Steuerelements.</returns>
        Protected Overrides Function CreateAccessibilityInstance() As AccessibleObject

            Return New ProgressAccessibleObject(Me)

        End Function

        ''' <summary>
        ''' Stellt den UI-Automation-Provider für das Fenster des Steuerelements bereit.
        ''' </summary>
        ''' <param name="m">Die zu verarbeitende Fensternachricht.</param>
        Protected Overrides Sub WndProc(ByRef m As Message)

            Const wmGetObject As Int32 = &H3D
            ' Nur die UIA-Root-Anfrage für eine Fortschrittsrolle selbst beantworten.
            If m.Msg = wmGetObject AndAlso m.LParam.ToInt64() = AutomationInteropProvider.RootObjectId AndAlso
                Me.HasProgressAccessibleRole() AndAlso Not Me.IsDisposed AndAlso Not Me.Disposing Then
                If Me._uiaProvider Is Nothing Then
                    Me._uiaProvider = New ProgressAutomationProvider(Me, m.HWnd)
                End If
                m.Result = AutomationInteropProvider.ReturnRawElementProvider(m.HWnd, m.WParam, m.LParam, Me._uiaProvider)
                Return
            End If

            MyBase.WndProc(m)

        End Sub

        ''' <summary>
        ''' Trennt den UI-Automation-Provider vom alten Fenster-Handle.
        ''' </summary>
        ''' <param name="e">Stellt die Ereignisdaten bereit.</param>
        Protected Overrides Sub OnHandleDestroyed(e As EventArgs)

            Dim provider As ProgressAutomationProvider = Me._uiaProvider
            Me._uiaProvider = Nothing
            If provider IsNot Nothing Then
                provider.Disconnect()
            End If
            MyBase.OnHandleDestroyed(e)

        End Sub

#End Region

#Region "interne Klassen"

        ''' <summary>
        ''' Stellt lokalisierte MSAA-Vorgaben, Rolle und schreibgeschützten Fortschrittswert bereit.
        ''' </summary>
        Private NotInheritable Class ProgressAccessibleObject

            Inherits Control.ControlAccessibleObject

            ''' <summary>Steuerelement, dessen Accessibility-Daten bereitgestellt werden.</summary>
            Private ReadOnly _owner As ColorProgressBar

            ''' <summary>Erstellt das MSAA-Objekt für das angegebene Steuerelement.</summary>
            ''' <param name="owner">Zugehöriges Fortschrittssteuerelement.</param>
            Public Sub New(owner As ColorProgressBar)
                MyBase.New(owner)
                Me._owner = owner
            End Sub

            ''' <summary>Verwendet den expliziten Namen oder den lokalisierten Standardnamen.</summary>
            Public Overrides Property Name As String
                Get
                    Dim accessibleName As String = MyBase.Name
                    If accessibleName IsNot Nothing Then Return accessibleName
                    Return GetAccessibilityText("AccessibleName")
                End Get
                Set(value As String)
                    MyBase.Name = value
                End Set
            End Property

            ''' <summary>Verwendet die explizite Beschreibung oder den lokalisierten Standardtext.</summary>
            Public Overrides ReadOnly Property Description As String
                Get
                    Dim accessibleDescription As String = MyBase.Description
                    If accessibleDescription IsNot Nothing Then Return accessibleDescription
                    Return GetAccessibilityText("AccessibleDescription")
                End Get
            End Property

            ''' <summary>Gibt die konfigurierte Rolle beziehungsweise die Fortschrittsrolle zurück.</summary>
            Public Overrides ReadOnly Property Role As AccessibleRole
                Get
                    If Me._owner.AccessibleRole = AccessibleRole.Default Then Return AccessibleRole.ProgressBar
                    Return Me._owner.AccessibleRole
                End Get
            End Property

            ''' <summary>Ergänzt den Basiszustand um die schreibgeschützte Kennzeichnung.</summary>
            Public Overrides ReadOnly Property State As AccessibleStates
                Get
                    Return MyBase.State Or AccessibleStates.ReadOnly
                End Get
            End Property

            ''' <summary>Gibt den lokalisierten Fortschritt zurück; Änderungen sind nicht zulässig.</summary>
            ''' <exception cref="InvalidOperationException">Beim Versuch, den Wert zu setzen.</exception>
            Public Overrides Property Value As String
                Get
                    Return FormatAccessibleProgress(Me._owner.Value, Me._owner.ProgressMaximumValue)
                End Get
                Set(value As String)
                    Throw New InvalidOperationException()
                End Set
            End Property
        End Class

        ''' <summary>
        ''' Stellt die schreibgeschützten UI-Automation-Muster für den Fortschritt bereit.
        ''' </summary>
        <ComVisible(True)>
        <ClassInterface(ClassInterfaceType.None)>
        Private NotInheritable Class ProgressAutomationProvider

            Implements IRawElementProviderSimple
            Implements IRangeValueProvider
            Implements IValueProvider

            ''' <summary>Zugehöriges Steuerelement, bis der Provider getrennt wird.</summary>
            Private _owner As ColorProgressBar
            ''' <summary>Fensterhandle, für das dieser Provider erstellt wurde.</summary>
            Private ReadOnly _windowHandle As IntPtr
            ''' <summary>Gibt an, ob der Provider noch mit seinem Fenster verbunden ist.</summary>
            Private _connected As Boolean = True

            ''' <summary>Erstellt einen UI-Automation-Provider für ein Fenster.</summary>
            ''' <param name="owner">Zugehöriges Fortschrittssteuerelement.</param>
            ''' <param name="windowHandle">Handle des zugehörigen Fensters.</param>
            Public Sub New(owner As ColorProgressBar, windowHandle As IntPtr)
                Me._owner = owner
                Me._windowHandle = windowHandle
            End Sub

            ''' <summary>Trennt den Provider vom Steuerelement und seinem bisherigen Fenster.</summary>
            Public Sub Disconnect()
                Me._connected = False
                Me._owner = Nothing
            End Sub

            ''' <summary>Prüft, ob der Provider weiterhin für die Fortschrittsrolle verfügbar ist.</summary>
            ''' <exception cref="ElementNotAvailableException">Der Provider ist getrennt, das Control wird entsorgt oder besitzt eine andere Rolle.</exception>
            Private Sub EnsureAvailable()
                ' Der Kurzschluss verhindert den Zugriff auf den nach Disconnect gelösten Owner.
                If Not Me._connected OrElse Me._owner.IsDisposed OrElse Me._owner.Disposing OrElse
                    Not Me._owner.HasProgressAccessibleRole() Then
                    Throw New ElementNotAvailableException()
                End If
            End Sub

            ''' <summary>Gibt die serverseitigen Provideroptionen einschließlich COM-Threading zurück.</summary>
            Private ReadOnly Property Options As ProviderOptions Implements IRawElementProviderSimple.ProviderOptions
                Get
                    Me.EnsureAvailable()
                    Return ProviderOptions.ServerSideProvider Or ProviderOptions.UseComThreading
                End Get
            End Property

            ''' <summary>Gibt den UI-Automation-Hostprovider des Fensters zurück.</summary>
            Private ReadOnly Property HostProvider As IRawElementProviderSimple Implements IRawElementProviderSimple.HostRawElementProvider
                Get
                    Me.EnsureAvailable()
                    Return AutomationInteropProvider.HostProviderFromHandle(Me._windowHandle)
                End Get
            End Property

            ''' <summary>Stellt die unterstützten Bereichswert- und Textwertmuster bereit.</summary>
            ''' <param name="patternId">Kennung des angeforderten Musters.</param>
            ''' <returns>Dieser Provider für unterstützte Muster, andernfalls <see langword="Nothing"/>.</returns>
            Private Function GetPatternProvider(patternId As Int32) As Object Implements IRawElementProviderSimple.GetPatternProvider
                Me.EnsureAvailable()
                If patternId = RangeValuePatternIdentifiers.Pattern.Id OrElse patternId = ValuePatternIdentifiers.Pattern.Id Then
                    Return Me
                End If
                Return Nothing
            End Function

            ''' <summary>Ermittelt den Wert einer unterstützten UI-Automation-Eigenschaft.</summary>
            ''' <param name="propertyId">Kennung der angeforderten Eigenschaft.</param>
            ''' <returns>Der Eigenschaftswert oder <see langword="Nothing"/> für unbekannte Kennungen.</returns>
            Private Function GetPropertyValue(propertyId As Int32) As Object Implements IRawElementProviderSimple.GetPropertyValue
                Me.EnsureAvailable()
                ' Nicht explizit behandelte Eigenschaften werden dem UIA-Standard überlassen.
                Select Case propertyId
                    Case AutomationElementIdentifiers.ControlTypeProperty.Id
                        Return ControlType.ProgressBar.Id
                    Case AutomationElementIdentifiers.NameProperty.Id
                        Return Me._owner.AccessibilityObject.Name
                    Case AutomationElementIdentifiers.HelpTextProperty.Id
                        Return Me._owner.AccessibilityObject.Description
                    Case AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id
                        Return Me._owner.CanSelect
                    Case AutomationElementIdentifiers.IsControlElementProperty.Id, AutomationElementIdentifiers.IsContentElementProperty.Id
                        Return True
                End Select
                Return Nothing
            End Function

            ''' <summary>Kennzeichnet beide Wertmuster als schreibgeschützt.</summary>
            Private ReadOnly Property IsReadOnly As Boolean Implements IRangeValueProvider.IsReadOnly, IValueProvider.IsReadOnly
                Get
                    Me.EnsureAvailable()
                    Return True
                End Get
            End Property

            ''' <summary>Gibt die untere Grenze des Fortschrittsbereichs zurück.</summary>
            Private ReadOnly Property Minimum As Double Implements IRangeValueProvider.Minimum
                Get
                    Me.EnsureAvailable()
                    Return 0
                End Get
            End Property

            ''' <summary>Gibt das aktuelle Fortschrittsmaximum zurück.</summary>
            Private ReadOnly Property Maximum As Double Implements IRangeValueProvider.Maximum
                Get
                    Me.EnsureAvailable()
                    Return CDbl(Me._owner.ProgressMaximumValue)
                End Get
            End Property

            ''' <summary>Gibt den aktuellen numerischen Fortschrittswert zurück.</summary>
            Private ReadOnly Property NumericValue As Double Implements IRangeValueProvider.Value
                Get
                    Me.EnsureAvailable()
                    Return CDbl(Me._owner.Value)
                End Get
            End Property

            ''' <summary>Gibt an, dass keine große Änderungsschrittweite unterstützt wird.</summary>
            Private ReadOnly Property LargeChange As Double Implements IRangeValueProvider.LargeChange
                Get
                    Me.EnsureAvailable()
                    Return Double.NaN
                End Get
            End Property

            ''' <summary>Gibt an, dass keine kleine Änderungsschrittweite unterstützt wird.</summary>
            Private ReadOnly Property SmallChange As Double Implements IRangeValueProvider.SmallChange
                Get
                    Me.EnsureAvailable()
                    Return Double.NaN
                End Get
            End Property

            ''' <summary>Gibt den Fortschritt als lokalisierten Prozenttext zurück.</summary>
            Private ReadOnly Property TextValue As String Implements IValueProvider.Value
                Get
                    Me.EnsureAvailable()
                    Return FormatAccessibleProgress(Me._owner.Value, Me._owner.ProgressMaximumValue)
                End Get
            End Property

            ''' <summary>Lehnt Änderungen am schreibgeschützten numerischen Wert ab.</summary>
            ''' <param name="value">Der angeforderte numerische Wert.</param>
            ''' <exception cref="ElementNotAvailableException">Der Provider ist nicht mehr verfügbar.</exception>
            ''' <exception cref="InvalidOperationException">Der Provider ist verfügbar, aber der Wert ist schreibgeschützt.</exception>
            Private Sub SetRangeValue(value As Double) Implements IRangeValueProvider.SetValue
                Me.EnsureAvailable()
                Throw New InvalidOperationException()
            End Sub

            ''' <summary>Lehnt Änderungen am schreibgeschützten Textwert ab.</summary>
            ''' <param name="value">Der angeforderte Textwert.</param>
            ''' <exception cref="ElementNotAvailableException">Der Provider ist nicht mehr verfügbar.</exception>
            ''' <exception cref="InvalidOperationException">Der Provider ist verfügbar, aber der Wert ist schreibgeschützt.</exception>
            Private Sub SetTextValue(value As String) Implements IValueProvider.SetValue
                Me.EnsureAvailable()
                Throw New InvalidOperationException()
            End Sub

        End Class

#End Region

    End Class

End Namespace
