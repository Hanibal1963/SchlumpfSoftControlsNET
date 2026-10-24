' --------------------------------------------------------------------------------------------------------
' Datei: FormTemplate.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.ComponentModel

Namespace NotifyFormControl

    ''' <summary>
    ''' Formularvorlage für die Anzeige eines Benachrichtigungsfensters (Popup) mit Titel, Symbol und Nachricht.
    ''' </summary>
    Friend NotInheritable Class FormTemplate

        Inherits Form

#Region "Variablen"

        Public Shared BackgroundColor As Color  ' Hintergrundfarbe des Formulars.
        Public Shared FontColor As Color ' Schriftfarbe für Titel und Nachricht.
        Public Shared Image As Image ' Anzuzeigendes Symbolbild.
        Public Shared Message As String ' Textnachricht die im Fenster dargestellt wird.
        Public Shared ShowTime As Int32 ' Zeit in Millisekunden bis zum automatischen Schließen (0 = kein Autoclose).
        Public Shared TextFieldColor As Color ' Hintergrundfarbe des Nachrichtentextfeldes.
        Public Shared Title As String ' Titeltext für die Titelleiste.
        Public Shared TitleBarColor As Color ' Hintergrundfarbe der Titelleiste.
        Private ReadOnly LabelClose As New Label  ' Label-Steuerelement zum Schließen des Fensters.
        Private ReadOnly LabelTitle As New Label ' Label-Steuerelement zur Anzeige des Titels.
        Private ReadOnly PanelSpacer As New Panel ' Trenn-Panel zwischen Symbol und Text.
        Private ReadOnly PanelTitle As New Panel ' Panel für die Titelleiste, enthält Titel und Schließen-Label.
        Private ReadOnly PictureBoxImage As New PictureBox ' Bildanzeige für das Symbol.
        Private ReadOnly RichTextBoxMessage As New RichTextBox ' RichTextBox zur Anzeige der Nachricht.
        ' Timer-basierte Animationen und Cancellation für Auto-Close
        Private ReadOnly _slideTimer As New System.Windows.Forms.Timer()
        Private ReadOnly _fadeOutTimer As New System.Windows.Forms.Timer()
        Private ReadOnly _fadeInTimer As New System.Windows.Forms.Timer()
        Private _slideTargetX As Integer
        Private _autoCloseCts As Threading.CancellationTokenSource

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert die Form-Eigenschaften und zeigt das Popup-Fenster an.
        ''' </summary>
        Public Sub Initialize()
            With Me
                .MaximizeBox = False
                .MinimizeBox = False
                .BackColor = BackgroundColor
                .ForeColor = FontColor
                .Size = New Size(504, 138)
                .FormBorderStyle = FormBorderStyle.None
                .StartPosition = FormStartPosition.Manual
            End With
            Me.Show()
        End Sub

        ''' <summary>
        ''' Gibt verwendete Ressourcen frei und ruft die Basisimplementierung auf.
        ''' </summary>
        ''' <param name="disposing">
        ''' <see langword="True"/>, wenn verwaltete Ressourcen freigegeben werden sollen; andernfalls
        ''' <see langword="False"/>.
        ''' </param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            'Bereinigung durchführen
            Me.LabelClose?.Dispose()
            Me.LabelTitle?.Dispose()
            Me.PanelSpacer?.Dispose()
            Me.PanelTitle?.Dispose()
            Me.PictureBoxImage?.Dispose()
            Me.RichTextBoxMessage?.Dispose()
            MyBase.Dispose(disposing)
        End Sub

        ''' <summary>
        ''' Finalizer der Klasse; delegiert an den Basis-Finalizer.
        ''' </summary>
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Schließt das Formular threadsicher und verwendet bei Bedarf
        ''' <see cref="Control.Invoke([Delegate])"/>.
        ''' </summary>
        Private Sub CloseForm()
            If Me.InvokeRequired Then
                Dim unused = Me.Invoke(New MethodInvoker(AddressOf Me.CloseForm))
            Else
                Me.Close()
            End If
        End Sub

        ''' <summary>
        ''' Fügt alle benötigten Steuerelemente zur Formularoberfläche hinzu.
        ''' </summary>
        Private Sub AddControls()
            With Me.Controls
                .Add(Me.LabelTitle)
                .Add(Me.LabelClose)
                .Add(Me.PanelSpacer)
                .Add(Me.PanelTitle)
                .Add(Me.RichTextBoxMessage)
                .Add(Me.PictureBoxImage)
            End With
        End Sub

        ''' <summary>
        ''' Führt den automatischen Schließvorgang nach Ablauf der eingestellten Zeit aus.
        ''' </summary>
        Private Async Sub StartAutoCloseAsync()
            _autoCloseCts?.Cancel()
            _autoCloseCts = New Threading.CancellationTokenSource()
            Dim ct = _autoCloseCts.Token
            If ShowTime > 0 Then
                Try
                    Await Task.Delay(ShowTime, ct)
                    If Not ct.IsCancellationRequested Then
                        BeginFadeOut()
                    End If
                Catch ex As TaskCanceledException
                    ' Abbruch gewünscht -> nichts tun
                End Try
            End If
        End Sub

        ''' <summary>
        ''' Führt beim Schließen eine kurze Ausblendanimation aus.
        ''' </summary>
        ''' <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten für den Schließvorgang.</param>
        Private Sub Form_Closing(sender As Object, e As CancelEventArgs) Handles Me.FormClosing
            ' Kein blockierendes Sleep oder Busy-Wait im Closing-Event.
            ' Die Ausblendanimation wird über BeginFadeOut / _fadeOutTimer gesteuert.
            ' Falls Close manuell aufgerufen wird, die Timer beenden und aufräumen.
            Try
                _slideTimer.Stop()
                _fadeOutTimer.Stop()
            Catch
            End Try
        End Sub

        ''' <summary>
        ''' Initialisiert beim Laden alle Steuerelemente, positioniert das Fenster und startet den Auto-Close-Thread.
        ''' </summary>
        ''' <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten zum Ladevorgang des Formulars.</param>
        Private Sub Form_Load(sender As Object, e As EventArgs) Handles Me.Load
            With Me
                .Opacity = 0.1
                .SetPropertys_Lbl_Close()
                .SetPropertys_Panel_Spacer()
                .SetPropertys_Pb_Image()
                .SetPropertys_Panel_Title()
                .SetPropertys_Lbl_Title()
                .SetPropertys_Txt_Msg()
                .AddControls()
                .ActiveControl = .Controls.Item(1)
            End With
            'Ändern Sie die Position in die untere rechte Ecke
            Dim y As Int32 = Screen.PrimaryScreen.WorkingArea.Height - Me.Height - 50
            ' Position zunächst außerhalb des sichtbaren Bereichs (rechts)
            Me.Location = New Point(Screen.PrimaryScreen.WorkingArea.Width, y)
            AddHandler Me.LabelClose.Click, AddressOf Me.Lbl_Close_Click

            ' Start nicht-blockierende Animationen
            StartSlideIn()
            StartFadeIn()

            ' Auto-Close asynchron starten
            StartAutoCloseAsync()
        End Sub

        ''' <summary>
        ''' Führt die Einblendanimation des Formulars aus.
        ''' </summary>
        Private Sub FormFadeIn()
            ' Backward compatible stub: startet den Timer-basierten Fade-In
            StartFadeIn()
        End Sub

        Private Sub StartFadeIn()
            Me.Opacity = 0.1
            _fadeInTimer.Interval = 40
            AddHandler _fadeInTimer.Tick, AddressOf FadeInTimer_Tick
            _fadeInTimer.Start()
        End Sub

        Private Sub FadeInTimer_Tick(sender As Object, e As EventArgs)
            Try
                Dim newOpacity = Math.Min(1.0, Me.Opacity + 0.1)
                Me.Opacity = newOpacity
                If Me.Opacity >= 1.0 Then
                    _fadeInTimer.Stop()
                    RemoveHandler _fadeInTimer.Tick, AddressOf FadeInTimer_Tick
                End If
            Catch
            End Try
        End Sub

        Private Sub StartSlideIn()
            _slideTargetX = Screen.PrimaryScreen.WorkingArea.Width - Me.Width
            _slideTimer.Interval = 8
            AddHandler _slideTimer.Tick, AddressOf SlideTimer_Tick
            _slideTimer.Start()
        End Sub

        Private Sub SlideTimer_Tick(sender As Object, e As EventArgs)
            Try
                Dim stepSize As Integer = 20
                Dim x As Integer = Me.Location.X - stepSize
                If x <= _slideTargetX Then
                    x = _slideTargetX
                    _slideTimer.Stop()
                    RemoveHandler _slideTimer.Tick, AddressOf SlideTimer_Tick
                End If
                Me.Location = New Point(x, Me.Location.Y)
            Catch
            End Try
        End Sub

        Private Sub BeginFadeOut()
            _fadeOutTimer.Interval = 60
            AddHandler _fadeOutTimer.Tick, AddressOf FadeOutTimer_Tick
            _fadeOutTimer.Start()
        End Sub

        Private Sub FadeOutTimer_Tick(sender As Object, e As EventArgs)
            Try
                Dim newOpacity = Math.Max(0.0, Me.Opacity - 0.1)
                Me.Opacity = newOpacity
                If Me.Opacity <= 0.0 Then
                    _fadeOutTimer.Stop()
                    RemoveHandler _fadeOutTimer.Tick, AddressOf FadeOutTimer_Tick
                    Me.Close()
                End If
            Catch
            End Try
        End Sub

        ''' <summary>
        ''' Behandelt den Klick auf das Schließen-Label und beendet das Popup sofort.
        ''' </summary>
        ''' <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten des Klickereignisses.</param>
        Private Sub Lbl_Close_Click(sender As Object, e As EventArgs)
            ' Bei manuellem Schließen: AutoClose abbrechen und Fade-Out starten
            Try
                _autoCloseCts?.Cancel()
            Catch
            End Try
            BeginFadeOut()
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften des Schließen-Labels.
        ''' </summary>
        Private Sub SetPropertys_Lbl_Close()
            With Me.LabelClose
                .AutoSize = True
                .BackColor = TitleBarColor
                .ForeColor = FontColor
                .Location = New Point(478, 9)
                .Name = "LblClose"
                .Size = New Size(14, 13)
                .TabIndex = 1
                .Text = "X"
            End With
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften des Titel-Labels.
        ''' </summary>
        Private Sub SetPropertys_Lbl_Title()
            With Me.LabelTitle
                .AutoSize = True
                .BackColor = TitleBarColor
                .ForeColor = FontColor
                .Location = New Point(11, 9)
                .Name = "LblTitle"
                .Size = New Size(27, 13)
                .TabIndex = 0
                .Text = Title
            End With
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften des Trenn-Panels zwischen Symbol- und Textbereich.
        ''' </summary>
        Private Sub SetPropertys_Panel_Spacer()
            With Me.PanelSpacer
                .BackColor = TitleBarColor
                .Location = New Point(119, 36)
                .Name = "PanelSpacer"
                .Size = New Size(1, 92)
                .TabIndex = 2
            End With
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften des Titel-Panels.
        ''' </summary>
        Private Sub SetPropertys_Panel_Title()
            With Me.PanelTitle
                .BackColor = TitleBarColor
                .Controls.Add(Me.LabelClose)
                .Controls.Add(Me.LabelTitle)
                .Location = New Point(0, -1)
                .Name = "PanelTitle"
                .Size = New Size(505, 29)
                .TabIndex = 0
            End With
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften der PictureBox für das Benachrichtigungssymbol.
        ''' </summary>
        Private Sub SetPropertys_Pb_Image()
            With Me.PictureBoxImage
                .BackColor = Color.Transparent
                .Location = New Point(12, 36)
                .Name = "PicBoxImage"
                .Size = New Size(92, 92)
                .TabIndex = 1
                .TabStop = False
                .Image = Image
                .SizeMode = PictureBoxSizeMode.StretchImage
                .Refresh()
            End With
        End Sub

        ''' <summary>
        ''' Setzt die Eigenschaften der RichTextBox für den Nachrichtentext.
        ''' </summary>
        Private Sub SetPropertys_Txt_Msg()
            With Me.RichTextBoxMessage
                .BackColor = TextFieldColor
                .ForeColor = FontColor
                .BorderStyle = BorderStyle.None
                .Location = New Point(133, 37)
                .Multiline = True
                .Name = "RiTxtMsg"
                .Size = New Size(361, 90)
                .TabIndex = 3
                .ReadOnly = True
                .Text = Message
                .SelectionProtected = True
                .Cursor = Cursors.Arrow
            End With
        End Sub

        ''' <summary>
        ''' Initialisiert die Designer-Komponenten des Formulars.
        ''' </summary>
        Private Sub InitializeComponent()
            Me.SuspendLayout()
            '
            'FormTemplate
            '
            Me.ClientSize = New Size(284, 261)
            Me.Name = "FormTemplate"
            Me.ResumeLayout(False)
        End Sub

#End Region

    End Class

End Namespace
