' --------------------------------------------------------------------------------------------------------
' Datei: NotifyForm.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing

Namespace NotifyFormControl

    ''' <summary>
    ''' Control zum Anzeigen von Benachrichtigungsfenstern.
    ''' </summary>
    <Description("Control zum Anzeigen von Benachrichtigungsfenstern.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(NotifyForm), "NotifyFormControl.NotifyForm.bmp")>
    Public Class NotifyForm : Inherits Component

#Region "Variablen"

        Private _Title As String
        Private _Style As NotifyFormStyle
        Private _ShowTime As Int32
        Private _Message As String
        Private _Design As NotifyFormDesign

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Legt das Aussehen des Benachrichtigungsfensters fest.
        ''' </summary>
        ''' <value>
        ''' Ein Wert aus <see cref="NotifyFormDesign"/>
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt das Aussehen des Benachrichtigungsfensters fest.")>
        <DefaultValue(GetType(NotifyFormDesign), "Bright")>
        Public Property Design As NotifyFormDesign
            Get
                Return Me._Design
            End Get
            Set
                Me._Design = Value
            End Set
        End Property

        ''' <summary>
        ''' Legt den Benachrichtigungstext fest der angezeigt werden soll.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt den Benachrichtigungstext fest der angezeigt werden soll.")>
        <DefaultValue("Mitteilung")>
        Public Property Message As String
            Get
                Return Me._Message
            End Get
            Set
                Me._Message = Value
            End Set
        End Property

        ''' <summary>
        ''' Legt die Anzeigedauer des Benachrichtigungsfensters in ms fest.
        ''' </summary>
        ''' <remarks>
        ''' Der Wert 0 bewirkt das kein automatisches schließen des Fensters erfolgt.
        ''' </remarks>
        <Browsable(True)>
        <Category("Behavior")>
        <Description("Legt die Anzeigedauer des Benachrichtigungsfensters in ms fest.")>
        <DefaultValue(5000)>
        Public Property ShowTime As Int32
            Get
                Return Me._ShowTime
            End Get
            Set
                Me._ShowTime = Value
            End Set
        End Property

        ''' <summary>
        ''' Legt das anzuzeigende Symbol im Benachrichtigungsfensters fest.
        ''' </summary>
        ''' <value>
        ''' Ein Wert aus <see cref="NotifyFormStyle"/>
        ''' </value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt das anzuzeigende Symbol im Benachrichtigungsfensters fest.")>
        <DefaultValue(GetType(NotifyFormStyle), "Information")>
        Public Property Style As NotifyFormStyle
            Get
                Return Me._Style
            End Get
            Set
                Me._Style = Value
            End Set
        End Property

        ''' <summary>
        ''' Legt den Text der Titelzeile des Benachrichtigungsfensters fest.
        ''' </summary>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Legt den Text der Titelzeile des Benachrichtigungsfensters fest.")>
        <DefaultValue("Titel")>
        Public Property Title As String
            Get
                Return Me._Title
            End Get
            Set
                Me._Title = Value
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="NotifyForm"/> Klasse mit Standardwerten.
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()
            Me.Title = $"Titel"
            Me.Message = $"Mitteilung"
            Me.Design = NotifyFormDesign.Bright
            Me.Style = NotifyFormStyle.Information
            Me.ShowTime = 5000
        End Sub

        ''' <summary>
        ''' Zeigt das Meldungsfenster an.
        ''' </summary>
        <Description("Zeigt das Meldungsfenster an.")>
        Public Sub Show()
            ' Überträgt die aktuellen Einstellungen in die statischen Anzeigeparameter der Formularvorlage.
            FormTemplate.Image = Me.SetFormImage()
            FormTemplate.Title = Me.Title
            FormTemplate.Message = Me.Message
            FormTemplate.ShowTime = Me.ShowTime

            ' Wendet das gewählte Farbschema an und zeigt anschließend das Formular an.
            Me.SetFormDesign()

            ShowForm()

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Erstellt eine Instanz des internen Popup-Formulars und zeigt sie an.
        ''' </summary>
        Private Shared Sub ShowForm()
            Dim frm As New FormTemplate
            frm.Initialize()
        End Sub

        ''' <summary>
        ''' Wählt anhand der konfigurierten <see cref="Design"/> -Eigenschaft das Farbschema aus.
        ''' </summary>
        Private Sub SetFormDesign()
            Select Case Me.Design
                Case NotifyFormDesign.Bright : SetFormDesignBright()
                Case NotifyFormDesign.Colorful : SetFormDesignColorful()
                Case NotifyFormDesign.Dark : SetFormDesignDark()
            End Select
        End Sub

        ''' <summary>
        ''' Setzt das helle Design für das Benachrichtigungsformular.
        ''' </summary>
        Private Shared Sub SetFormDesignBright()
            FormTemplate.BackgroundColor = Color.White
            FormTemplate.TextFieldColor = Color.White
            FormTemplate.TitleBarColor = Color.Gray
            FormTemplate.FontColor = Color.Black
        End Sub

        ''' <summary>
        ''' Setzt das farbige Design für das Benachrichtigungsformular.
        ''' </summary>
        Private Shared Sub SetFormDesignColorful()
            FormTemplate.BackgroundColor = Color.LightBlue
            FormTemplate.TextFieldColor = Color.LightBlue
            FormTemplate.TitleBarColor = Color.LightSeaGreen
            FormTemplate.FontColor = Color.White
        End Sub

        ''' <summary>
        ''' Setzt das dunkle Design für das Benachrichtigungsformular.
        ''' </summary>
        Private Shared Sub SetFormDesignDark()
            FormTemplate.BackgroundColor = Color.FromArgb(83, 79, 75)
            FormTemplate.TextFieldColor = Color.FromArgb(83, 79, 75)
            FormTemplate.TitleBarColor = Color.FromArgb(60, 57, 54)
            FormTemplate.FontColor = Color.White
        End Sub

        ''' <summary>
        ''' Ermittelt das anzuzeigende Symbol entsprechend dem eingestellten Stil.
        ''' </summary>
        ''' <returns>
        ''' Ein Bild aus den Ressourcen, das dem aktuell gewählten <see cref="Style"/> entspricht.
        ''' </returns>
        Private Function SetFormImage() As Image
            Dim result As Bitmap = Nothing
            Select Case Me.Style
            ' Ordnet den ausgewählten Stil dem passenden Ressourcensymbol zu.
                Case NotifyFormStyle.CriticalError : result = My.Resources.NotifyFormControl_CriticalError
                Case NotifyFormStyle.Exclamation : result = My.Resources.NotifyFormControl_Warning
                Case NotifyFormStyle.Information : result = My.Resources.NotifyFormControl_Information
                Case NotifyFormStyle.Question : result = My.Resources.NotifyFormControl_Question
            End Select
            Return result
        End Function

#End Region

    End Class

End Namespace
