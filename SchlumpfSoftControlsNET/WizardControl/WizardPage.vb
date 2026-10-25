' --------------------------------------------------------------------------------------------------------
' Datei: WizardPage.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing

Namespace WizardControl

    ''' <summary>
    ''' Definiert eine Seite des Controls.
    ''' </summary>
    ''' <remarks>
    ''' Eine Assistentenseite stellt Titel, Beschreibung und einen Darstellungsstil bereit.
    ''' </remarks>
    <ToolboxItem(False)>
    Public Class WizardPage : Inherits Panel

        Private _Style As PageStyle = PageStyle.Standard  ' Privates Feld: Darstellungsstil der Seite
        Private _Title As String = String.Empty  ' Privates Feld: Titel der Seite
        Private _Description As String = String.Empty  ' Privates Feld: Beschreibung der Seite

        ''' <summary>
        ''' Ruft den Stil der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Bei Änderung wird die Seite neu gezeichnet bzw. der Wizard aktualisiert.
        ''' </remarks>
        ''' <value>
        ''' Ein Wert der Enumeration <see cref="PageStyle"/>.
        ''' </value>
        <Category("Design")>
        <Description("Ruft den Stil der Assistentenseite ab oder legt diesen fest.")>
        <DefaultValue(PageStyle.Standard)>
        Public Overridable Property Style As PageStyle
            Get
                Return Me._Style
            End Get
            Set(value As PageStyle)
                If Me._Style = value Then
                    Return
                End If
                Me._Style = value
                If Me.Parent IsNot Nothing AndAlso TypeOf Me.Parent Is Wizard Then
                    Dim wizard As Wizard = CType(Me.Parent, Wizard)
                    If wizard.SelectedPage Is Me Then
                        wizard.SelectedPage = Me
                    End If
                Else
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft den Titel der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Bei Änderung wird die Seite neu gezeichnet.
        ''' </remarks>
        ''' <value>
        ''' Der anzuzeigende Titeltext.
        ''' </value>
        <DefaultValue("")>
        <Category("Design")>
        <Description("Ruft den Titel der Assistentenseite ab oder legt diesen fest.")>
        Public Overridable Property Title As String
            Get
                Return Me._Title
            End Get
            Set(value As String)
                If Equals(value, Nothing) Then
                    value = String.Empty
                End If
                If Not Equals(Me._Title, value) Then
                    Me._Title = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft die Beschreibung der Assistentenseite ab oder legt diese fest.
        ''' </summary>
        ''' <remarks>
        ''' Bei Änderung wird die Seite neu gezeichnet.
        ''' </remarks>
        ''' <value>
        ''' Der anzuzeigende Beschreibungstext.
        ''' </value>
        <DefaultValue("")>
        <Category("Design")>
        <Description("Ruft die Beschreibung der Assistentenseite ab oder legt diese fest.")>
        Public Overridable Property Description As String
            Get
                Return Me._Description
            End Get
            Set(value As String)
                If Equals(value, Nothing) Then
                    value = String.Empty
                End If
                If Not Equals(Me._Description, value) Then
                    Me._Description = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="WizardPage"/>.
        ''' </summary>
        ''' <remarks>
        ''' Setzt ControlStyles für flackerfreies Zeichnen.
        ''' </remarks>
        Public Sub New()
            Me.InitializeStyles()
        End Sub

        ' Initialisiert Zeichen- und Puffer-Styles.
        Private Sub InitializeStyles()
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.DoubleBuffer, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)
            Me.SetStyle(ControlStyles.UserPaint, True)
        End Sub

        ' Zeichnet die Seite abhängig vom Stil. Geschützter Override für benutzerdefiniertes Rendern.
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            If Me._Style = PageStyle.Custom Then Return

            Dim clientRectangle = MyBase.ClientRectangle
            Dim empty = Rectangle.Empty
            Dim empty2 = Rectangle.Empty
            Dim empty3 = Rectangle.Empty
            Dim genericDefault = StringFormat.GenericDefault

            genericDefault.LineAlignment = StringAlignment.Near
            genericDefault.Alignment = StringAlignment.Near
            genericDefault.Trimming = StringTrimming.EllipsisCharacter

            Select Case Me.Style

                Case PageStyle.Standard
                    clientRectangle.Height = 64
                    ControlPaint.DrawBorder3D(e.Graphics, clientRectangle, Border3DStyle.Etched, Border3DSide.Bottom)
                    clientRectangle.Height -= SystemInformation.Border3DSize.Height
                    e.Graphics.FillRectangle(SystemBrushes.Window, clientRectangle)
                    Dim num2 As Int32 = CInt(Math.Floor(8.0))
                    empty.Location = New Point(Me.Width - 48 - num2, num2)
                    empty.Size = New Size(48, 48)

                    Dim image2 As Image = Nothing
                    Dim font3 = MyBase.Font
                    Dim font4 = MyBase.Font

                    If Me.Parent IsNot Nothing AndAlso TypeOf Me.Parent Is Wizard Then
                        Dim wizard2 As Wizard = CType(Me.Parent, Wizard)
                        image2 = wizard2.ImageHeader
                        If image2 Is Nothing Then
                            empty.Size = New Size(0, 0)
                        End If
                        font3 = wizard2.HeaderFont
                        font4 = wizard2.HeaderTitleFont
                    End If

                    If image2 Is Nothing Then
                        ControlPaint.DrawFocusRectangle(e.Graphics, empty)
                    Else
                        e.Graphics.DrawImage(image2, empty)
                    End If

                    Dim num3 As Int32 = CInt(Math.Ceiling(e.Graphics.MeasureString(Me._Title, font4, 0, genericDefault).Height))

                    empty2.Location = New Point(8, 8)
                    empty2.Size = New Size(empty.Left - 8, num3)
                    empty3.Location = empty2.Location
                    empty3.Y += num3 + 4
                    empty3.Size = New Size(empty2.Width, 64 - empty3.Y)

                    e.Graphics.DrawString(Me._Title, font4, SystemBrushes.WindowText, empty2, genericDefault)
                    e.Graphics.DrawString(Me._Description, font3, SystemBrushes.WindowText, empty3, genericDefault)

                    Exit Select

                Case PageStyle.Welcome, PageStyle.Finish
                    e.Graphics.FillRectangle(SystemBrushes.Window, clientRectangle)

                    empty.Location = Point.Empty
                    empty.Size = New Size(164, Me.Height)

                    Dim image As Image = Nothing
                    Dim font = MyBase.Font
                    Dim font2 = MyBase.Font

                    If Me.Parent IsNot Nothing AndAlso TypeOf Me.Parent Is Wizard Then
                        Dim wizard As Wizard = CType(Me.Parent, Wizard)
                        image = wizard.ImageWelcome
                        font = wizard.WelcomeFont
                        font2 = wizard.WelcomeTitleFont
                    End If

                    If image Is Nothing Then
                        ControlPaint.DrawFocusRectangle(e.Graphics, empty)
                    Else
                        e.Graphics.DrawImage(image, empty)
                    End If

                    empty2.Location = New Point(172, 8)
                    empty2.Width = Me.Width - empty2.Left - 8

                    Dim num As Int32 = CInt(Math.Ceiling(e.Graphics.MeasureString(Me._Title, font2, empty2.Width, genericDefault).Height))

                    empty3.Location = empty2.Location
                    empty3.Y += num + 8
                    empty3.Size = New Size(Me.Width - empty3.Left - 8, Me.Height - empty3.Y)

                    e.Graphics.DrawString(Me._Title, font2, SystemBrushes.WindowText, empty2, genericDefault)
                    e.Graphics.DrawString(Me._Description, font, SystemBrushes.WindowText, empty3, genericDefault)

                    Exit Select

            End Select

        End Sub

    End Class

End Namespace