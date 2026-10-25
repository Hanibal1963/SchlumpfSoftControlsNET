' --------------------------------------------------------------------------------------------------------
' Datei: Wizard.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing

Namespace WizardControl

    ''' <summary>
    ''' Ein Control zum Erstellen eines Assistenten.
    ''' </summary>
    <Description("Ein Control zum erstellen eines Assistenen")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(Wizard), "WizardControl.Wizard.bmp")>
    <Designer(GetType(WizardDesigner))>
    Public Class Wizard

        Inherits UserControl

#Region "Variablen"

        Friend _ImageHeader As Image               ' Bild, das in der Kopfzeile von Standardseiten angezeigt wird
        Friend _ImageWelcome As Image              ' Bild, das auf Begrüßungs- und Abschlussseiten angezeigt wird
        Friend _WelcomeFont As Font                ' Schriftart für die Beschreibung auf Begrüßungs- und Abschlussseiten (optional, fällt sonst auf Font zurück)
        Friend _WelcomeTitleFont As Font           ' Schriftart für den Titel auf Begrüßungs- und Abschlussseiten (optional; Standard: größer und fett)
        Friend _HeaderFont As Font                 ' Schriftart für die Beschreibung in der Kopfzeile von Standardseiten (optional, fällt sonst auf Font zurück)
        Friend _HeaderTitleFont As Font            ' Schriftart für den Titel in der Kopfzeile von Standardseiten (optional; Standard: +2pt, fett)
        Friend _SelectedPage As WizardPage         ' Aktuell aktive Assistentenseite
        Friend _Pages As PagesCollection           ' Sammlung aller Assistentenseiten
        Friend _ButtonHelpVisible As Boolean       ' Steuert die Sichtbarkeit der Hilfeschaltfläche

        Friend ReadOnly _OffsetCancel As New Point(84, 36) ' Abstand für Positionierung der Abbrechen/Beenden-Schaltfläche (X- und Y-Offset)
        Friend ReadOnly _OffsetNext As New Point(164, 36)  ' Abstand für Positionierung der Weiter-Schaltfläche (X- und Y-Offset)
        Friend ReadOnly _OffsetBack As New Point(244, 36)  ' Abstand für Positionierung der Zurück-Schaltfläche (X- und Y-Offset)

#End Region

        ''' <summary>
        ''' Delegattyp für das Ereignis, das vor dem Wechsel der Assistentenseiten ausgelöst wird. Ermöglicht
        ''' Validierung und das ggf. Verhindern des Seitenwechsels.
        ''' </summary>
        ''' <remarks>
        ''' Setzen Sie <c>e.Cancel = True</c>, um den Seitenwechsel zu verhindern.
        ''' </remarks>
        ''' <param name="sender">
        ''' Das auslösende Objekt (typischerweise die <see cref="Wizard"/> -Instanz).
        ''' </param>
        ''' <param name="e">
        ''' Ein <see cref="BeforeSwitchPagesEventArgs"/>, das die Ereignisdaten enthält (Alter/Neuer Index,
        ''' Abbruchflag).
        ''' </param>
        Public Delegate Sub BeforeSwitchPagesEventHandler(sender As Object, e As BeforeSwitchPagesEventArgs)

        ''' <summary>
        ''' Delegattyp für das Ereignis, das nach dem Wechsel der Assistentenseiten ausgelöst wird. Ermöglicht das
        ''' Initialisieren und Aktualisieren der neu aktiven Seite.
        ''' </summary>
        ''' <remarks>
        ''' Verwenden Sie dieses Ereignis, um Fokus zu setzen, Daten zu laden oder UI-Elemente anzupassen.
        ''' </remarks>
        ''' <param name="sender">
        ''' Das auslösende Objekt (typischerweise die <see cref="Wizard"/> -Instanz).
        ''' </param>
        ''' <param name="e">
        ''' Ein <see cref="AfterSwitchPagesEventArgs"/>, das die Ereignisdaten enthält (Alter/Neuer Index).
        ''' </param>
        Public Delegate Sub AfterSwitchPagesEventHandler(sender As Object, e As AfterSwitchPagesEventArgs)

        ''' <summary>
        ''' Tritt auf, bevor die Seiten des Assistenten gewechselt werden, um dem Benutzer die Möglichkeit zur
        ''' Validierung zu geben.
        ''' </summary>
        <Category("Behavior")>
        <Description("Tritt auf, bevor die Seiten des Assistenten gewechselt werden, um dem Benutzer die Möglichkeit zur Validierung zu geben.")>
        Public Event BeforeSwitchPages As BeforeSwitchPagesEventHandler

        ''' <summary>
        ''' Tritt auf, nachdem die Seiten des Assistenten gewechselt wurden, und gibt dem Benutzer die Möglichkeit, die
        ''' neue Seite einzurichten.
        ''' </summary>
        <Category("Behavior")>
        <Description("Tritt auf, nachdem die Seiten des Assistenten gewechselt wurden, und gibt dem Benutzer die Möglichkeit, die neue Seite einzurichten.")>
        Public Event AfterSwitchPages As AfterSwitchPagesEventHandler

        ''' <summary>
        ''' Tritt auf wenn der Benutzer auf Abbrechen geklickt hat.
        ''' </summary>
        <Category("Behavior")>
        <Description("Tritt auf wenn der Benutzer auf Abbrechen geklickt hat.")>
        Public Event Cancel As CancelEventHandler

        ''' <summary>
        ''' Tritt auf, wenn der Assistent abgeschlossen ist, und gibt dem Benutzer die Möglichkeit, zusätzliche Aufgaben
        ''' zu erledigen.
        ''' </summary>
        <Category("Behavior")>
        <Description("Tritt auf, wenn der Assistent abgeschlossen ist, und gibt dem Benutzer die Möglichkeit, zusätzliche Aufgaben zu erledigen.")>
        Public Event Finish As EventHandler

        ''' <summary>
        ''' Tritt auf, wenn der Benutzer auf die Hilfeschaltfläche klickt.
        ''' </summary>
        <Category("Behavior")>
        <Description("Tritt auf, wenn der Benutzer auf die Hilfeschaltfläche klickt.")>
        Public Event Help As EventHandler

        ''' <summary>
        ''' Ruft die Sichtbarkeit Status der Hilfeschaltfläche ab oder legt diesen fest.
        ''' </summary>
        ''' <value>
        ''' True, wenn die Hilfeschaltfläche sichtbar ist, andernfalls False.
        ''' </value>
        <Browsable(True)>
        <Category("Design")>
        <Description("Ruft die Sichtbarkeit Status der Hilfeschaltfläche ab oder legt diesen fest.")>
        <DefaultValue(True)>
        Public Property VisibleHelp As Boolean
            Get
                Return Me._ButtonHelpVisible
            End Get
            Set(value As Boolean)
                Me._ButtonHelpVisible = value
                Try
                    Dim e As EventArgs = Nothing
                    If Not Me._ButtonHelpVisible Then
                        Me.Controls.Remove(Me.ButtonHelp)
                        Me.OnResize(e)
                    Else
                        Me.Controls.Add(Me.ButtonHelp)
                        Me.OnResize(e)
                    End If
                Catch
                End Try
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Auflistung der Assistentenseiten in diesem Registerkartensteuerelement ab.
        ''' </summary>
        ''' <value>
        ''' Die Auflistung der Seiten als <see cref="PagesCollection"/>.
        ''' </value>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        <Category("Design")>
        <Description("Ruft die Auflistung der Assistentenseiten in diesem Registerkartensteuerelement ab.")>
        <Editor(GetType(PagesCollectionEditor), GetType(Design.UITypeEditor))>
        Public ReadOnly Property Pages As PagesCollection
            Get
                Return Me._Pages
            End Get
        End Property

        ''' <summary>
        ''' Ruft das in der Kopfzeile der Standardseiten angezeigte Bild ab oder legt dieses fest.
        ''' </summary>
        ''' <value>
        ''' Das Bild für die Kopfzeile.
        ''' </value>
        <Browsable(True)>
        <Category("Design")>
        <Description("Ruft das in der Kopfzeile der Standardseiten angezeigte Bild ab oder legt dieses fest.")>
        <DefaultValue(GetType(Image), "WizardControl_HeaderImage")>
        Public Property ImageHeader As Image
            Get
                Return Me._ImageHeader
            End Get
            Set(value As Image)
                If Me._ImageHeader IsNot value Then
                    Me._ImageHeader = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft das auf den Begrüßungs- und Abschlussseiten angezeigte Bild ab oder legt es fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Design")>
        <Description("Ruft das auf den Begrüßungs- und Abschlussseiten angezeigte Bild ab oder legt es fest.")>
        <DefaultValue(GetType(Image), "WizardControl_WelcomeImage")>
        Public Property ImageWelcome As Image
            Get
                Return Me._ImageWelcome
            End Get
            Set(value As Image)
                If Me._ImageWelcome IsNot value Then
                    Me._ImageWelcome = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft ab oder legt fest, an welcher Kante des übergeordneten Containers ein Steuerelement angedockt ist.
        ''' </summary>
        <Category("Layout")>
        <Description("Ruft ab oder legt fest, an welcher Kante des übergeordneten Containers ein Steuerelement angedockt ist.")>
        <DefaultValue(DockStyle.Fill)>
        Public Overloads Property Dock As DockStyle
            Get
                Return MyBase.Dock
            End Get
            Set(value As DockStyle)
                MyBase.Dock = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectedPage As WizardPage
            Get
                Return Me._SelectedPage
            End Get
            Set(value As WizardPage)
                Me.ActivatePage(value)
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Friend Property SelectedIndex As Int32
            Get
                Return Me._Pages.IndexOf(Me._SelectedPage)
            End Get
            Set(value As Int32)
                If Me._Pages.Count = 0 Then
                    Me.ActivatePage(-1)
                    Return
                End If
                If value < -1 OrElse value >= Me._Pages.Count Then
                    Throw New ArgumentOutOfRangeException($"SelectedIndex", value, $"Der Seitenindex muss zwischen 0 und {Me._Pages.Count - 1} liegen ")
                End If
                Me.ActivatePage(value)
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Standardseite verwendet wird, oder legt
        ''' diese fest.
        ''' </summary>
        <Category("Appearance")>
        <Description("Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Standardseite verwendet wird, oder legt diese fest.")>
        <DefaultValue(GetType(Font), "Font")>
        Public Property HeaderFont As Font
            Get
                Return If(Me._HeaderFont, MyBase.Font)
            End Get
            Set(value As Font)
                If Me._HeaderFont IsNot value Then
                    Me._HeaderFont = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Schriftart ab, die zum Anzeigen des Titels einer Standardseite verwendet wird, oder legt diese
        ''' fest.
        ''' </summary>
        ''' <returns></returns>
        <Category("Appearance")>
        <Description("Ruft die Schriftart ab, die zum Anzeigen des Titels einer Standardseite verwendet wird, oder legt diese fest.")>
        <DefaultValue(GetType(Font), "Font")>
        Public Property HeaderTitleFont As Font
            Get
                Return If(
                    Me._HeaderTitleFont,
                    New Font(MyBase.Font.FontFamily, MyBase.Font.Size + 2.0F, FontStyle.Bold))
            End Get
            Set(value As Font)
                If Me._HeaderTitleFont IsNot value Then
                    Me._HeaderTitleFont = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Begrüßungs- oder Abschlussseite verwendet
        ''' wird, oder legt diese fest.
        ''' </summary>
        ''' <returns></returns>
        <Category("Appearance")>
        <Description("Ruft die Schriftart ab, die zum Anzeigen der Beschreibung einer Begrüßungs- oder Abschlussseite verwendet wird, oder legt diese fest.")>
        <DefaultValue(GetType(Font), "Font")>
        Public Property WelcomeFont As Font
            Get
                Return If(Me._WelcomeFont, MyBase.Font)
            End Get
            Set(value As Font)
                If Me._WelcomeFont IsNot value Then
                    Me._WelcomeFont = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Schriftart ab, die zum Anzeigen des Titels einer Begrüßungs- oder Abschlussseite verwendet wird,
        ''' oder legt diese fest.
        ''' </summary>
        ''' <returns></returns>
        <Category("Appearance")>
        <Description("Ruft die Schriftart ab, die zum Anzeigen des Titels einer Begrüßungs- oder Abschlussseite verwendet wird, oder legt diese fest.")>
        <DefaultValue(GetType(Font), "Font")>
        Public Property WelcomeTitleFont As Font
            Get
                Return If(
                    Me._WelcomeTitleFont,
                    New Font(MyBase.Font.FontFamily, MyBase.Font.Size + 10.0F, FontStyle.Bold))
            End Get
            Set(value As Font)
                If Me._WelcomeTitleFont IsNot value Then
                    Me._WelcomeTitleFont = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property NextEnabled As Boolean
            Get
                Return Me.ButtonNext.Enabled
            End Get
            Set(value As Boolean)
                Me.ButtonNext.Enabled = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property BackEnabled As Boolean
            Get
                Return Me.ButtonBack.Enabled
            End Get
            Set(value As Boolean)
                Me.ButtonBack.Enabled = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property NextText As String
            Get
                Return Me.ButtonNext.Text
            End Get
            Set(value As String)
                Me.ButtonNext.Text = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property BackText As String
            Get
                Return Me.ButtonBack.Text
            End Get
            Set(value As String)
                Me.ButtonBack.Text = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property CancelText As String
            Get
                Return Me.ButtonCancel.Text
            End Get
            Set(value As String)
                Me.ButtonCancel.Text = value
            End Set
        End Property

        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property HelpText As String
            Get
                Return Me.ButtonHelp.Text
            End Get
            Set(value As String)
                Me.ButtonHelp.Text = value
            End Set
        End Property

        Public Sub New()
            Me.InitializeComponent()
            Me.InitializeVariables()
            Me.InitializeStyles()
            MyBase.Dock = DockStyle.Fill
            Me._Pages = New PagesCollection(Me)
        End Sub

        Private Sub InitializeVariables()
            Me._ImageHeader = My.Resources.WizardControl_HeaderImage
            Me._ImageWelcome = My.Resources.WizardControl_WelcomeImage
            Me._WelcomeFont = Nothing
            Me._WelcomeTitleFont = Nothing
            Me._HeaderFont = Nothing
            Me._HeaderTitleFont = Nothing
            Me._SelectedPage = Nothing
            Me._Pages = Nothing
            Me._ButtonHelpVisible = True
        End Sub

        Private Sub InitializeStyles()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.DoubleBuffer, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
            Me.SetStyle(ControlStyles.UserPaint, True)
        End Sub

        ''' <summary>
        ''' Entspricht einem Klick auf die Schaltfläche "weiter".
        ''' </summary>
        Public Sub [Next]()
            If Me.SelectedIndex = Me._Pages.Count - 1 Then
                Me.ButtonNext.Enabled = False
            Else
                Me.OnBeforeSwitchPages(New BeforeSwitchPagesEventArgs(Me.SelectedIndex, Me.SelectedIndex + 1))
            End If
        End Sub

        ''' <summary>
        ''' Entspricht einem Klick auf die Schaltfläche "zurück".
        ''' </summary>
        Public Sub Back()
            If Me.SelectedIndex = 0 Then
                Me.ButtonBack.Enabled = False
            Else
                Me.OnBeforeSwitchPages(New BeforeSwitchPagesEventArgs(Me.SelectedIndex, Me.SelectedIndex - 1))
            End If
        End Sub

        ''' <summary>
        ''' Setzt den Index der aktuellen Seite
        ''' </summary>
        Private Sub ActivatePage(index As Int32)
            If index >= 0 AndAlso index < Me._Pages.Count Then
                Dim page As WizardPage = Me._Pages(index)
                Me.ActivatePage(page)
            End If
        End Sub

        ''' <summary>
        ''' setzt eine Wizardseite als aktuelle Seite
        ''' </summary>
        Private Sub ActivatePage(page As WizardPage)
            If page Is Nothing Then
                If Me._SelectedPage IsNot Nothing Then
                    Me._SelectedPage.Visible = False
                End If
                Me._SelectedPage = Nothing
                Me.ButtonBack.Enabled = False
                Me.ButtonNext.Enabled = False
                Me.Invalidate()
                Return
            End If
            If Not Me._Pages.Contains(page) Then
                Return
            End If
            If Me._SelectedPage IsNot Nothing Then
                Me._SelectedPage.Visible = False
            End If
            Me._SelectedPage = page
            If Me._SelectedPage IsNot Nothing Then
                Me._SelectedPage.Parent = Me
                If Not Me.Contains(Me._SelectedPage) Then
                    Me.Container.Add(Me._SelectedPage)
                End If
                Select Case Me._SelectedPage.Style
                    Case PageStyle.Finish
                        Me.ButtonCancel.Text = $"Beenden"
                        Me.ButtonCancel.DialogResult = DialogResult.OK
                    Case Else
                        Me.ButtonCancel.Text = $"Abbruch"
                        Me.ButtonCancel.DialogResult = DialogResult.Cancel
                End Select
                If Me._SelectedPage.Style = PageStyle.Custom And Me._SelectedPage Is Me._Pages(Me._Pages.Count - 1) Then
                    Me.ButtonCancel.Text = $"Weiter"
                    Me.ButtonCancel.DialogResult = DialogResult.OK
                End If
                Me._SelectedPage.SetBounds(0, 0, Me.Width, Me.Height - 48)
                Me._SelectedPage.Visible = True
                Me._SelectedPage.BringToFront()
                FocusFirstTabIndex(Me._SelectedPage)
            End If
            Me.ButtonBack.Enabled = Me.SelectedIndex > 0
            If Me.SelectedIndex < Me._Pages.Count - 1 Then
                Me.ButtonNext.Enabled = True
            Else
                If Not Me.DesignMode Then
                End If
                Me.ButtonNext.Enabled = False
            End If
            If Me._SelectedPage IsNot Nothing Then
                Me._SelectedPage.Invalidate()
            Else
                Me.Invalidate()
            End If
        End Sub

        Private Shared Sub FocusFirstTabIndex(container As Control)
            Dim control As Control = Nothing
            For Each control2 As Control In container.Controls
                If control2.CanFocus AndAlso (control Is Nothing OrElse control2.TabIndex < control.TabIndex) Then
                    control = control2
                End If
            Next
            If control IsNot Nothing Then
                Dim unused = control.Focus()
            Else
                Dim unused1 = container.Focus()
            End If
        End Sub

        Protected Overridable Sub OnBeforeSwitchPages(e As BeforeSwitchPagesEventArgs)
            RaiseEvent BeforeSwitchPages(Me, e)
            If Not e.Cancel Then
                Me.ActivatePage(e.NewIndex)
                Me.OnAfterSwitchPages(e)
            End If
        End Sub

        Protected Overridable Sub OnAfterSwitchPages(e As AfterSwitchPagesEventArgs)
            RaiseEvent AfterSwitchPages(Me, e)
        End Sub

        Protected Overridable Sub OnCancel(e As CancelEventArgs)
            RaiseEvent Cancel(Me, e)
            If e.Cancel Then
                Me.ParentForm.DialogResult = DialogResult.None
            Else
                Me.ParentForm.Close()
            End If
        End Sub

        Protected Overridable Sub OnFinish(e As EventArgs)
            RaiseEvent Finish(Me, e)
            Me.ParentForm.Close()
        End Sub

        Protected Overridable Sub OnHelp(e As EventArgs)
            RaiseEvent Help(Me, e)
        End Sub

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)
            If Me._Pages.Count > 0 Then
                Me.ActivatePage(0)
            End If
        End Sub

        Protected Overrides Sub OnResize(e As EventArgs)
            MyBase.OnResize(e)
            Me._SelectedPage?.SetBounds(0, 0, Me.Width, Me.Height - 48)
            Me.ButtonHelp.Location = New Point(Me.ButtonHelp.Location.X, Me.Height - Me._OffsetBack.Y)
            Me.ButtonBack.Location = New Point(Me.Width - Me._OffsetBack.X, Me.Height - Me._OffsetBack.Y)
            Me.ButtonNext.Location = New Point(Me.Width - Me._OffsetNext.X, Me.Height - Me._OffsetNext.Y)
            Me.ButtonCancel.Location = New Point(Me.Width - Me._OffsetCancel.X, Me.Height - Me._OffsetCancel.Y)
            MyBase.Refresh()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim clientRectangle = MyBase.ClientRectangle
            clientRectangle.Y = Me.Height - 48
            clientRectangle.Height = 48
            ControlPaint.DrawBorder3D(e.Graphics, clientRectangle, Border3DStyle.Etched, Border3DSide.Top)
        End Sub

        Protected Overrides Sub OnControlAdded(e As ControlEventArgs)
            If Not (TypeOf e.Control Is WizardPage) AndAlso e.Control IsNot Me.ButtonCancel AndAlso e.Control IsNot Me.ButtonNext AndAlso e.Control IsNot Me.ButtonBack Then
                Me._SelectedPage?.Controls.Add(e.Control)
            Else
                MyBase.OnControlAdded(e)
            End If
        End Sub

        Private Sub ButtonHelp_Click(sender As Object, e As EventArgs) Handles ButtonHelp.Click
            Me.OnHelp(EventArgs.Empty)
        End Sub

        Private Sub ButtonBack_Click(sender As Object, e As EventArgs) Handles ButtonBack.Click
            Me.Back()
        End Sub

        Private Sub ButtonNext_Click(sender As Object, e As EventArgs) Handles ButtonNext.Click
            Me.Next()
        End Sub

        Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
            If Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel Then
                Me.OnCancel(New CancelEventArgs())
            ElseIf Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.OK Then
                Me.OnFinish(EventArgs.Empty)
            End If
        End Sub


    End Class


End Namespace