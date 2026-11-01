' --------------------------------------------------------------------------------------------------------
' Datei: ContentView.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Stellt ein Steuerelement zur schreibgeschützten Anzeige von Textinhalten bereit.
    ''' </summary>
    ''' <remarks>
    ''' Die Anzeige erfolgt zeilenbasiert über <see cref="Lines"/>. Änderungen an <see cref="TitelText"/> und
    ''' <see cref="Lines"/> werden intern auf die UI synchronisiert.
    ''' </remarks>
    <Description("Steuerelement zum Anzeigen des Dateiinhaltes.")> ' Beschreibt das Control im Designer (Eigenschaftenfenster/Toolbox).
    <ToolboxItem(True)> ' Markiert die Klasse als Toolbox-Element.
    <ToolboxBitmap(GetType(ContentView), "IniFileControl.ContentView.bmp")> ' Legt das Symbol in der Toolbox fest.
    Public Class ContentView : Inherits UserControl

#Region "Variablen"

        Private _Lines As String()
        Private _TitelText As String

#End Region

#Region "Ereignisse"

        Private Event PropertyLinesChanged()
        Private Event PropertyTitelTextChanged()

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt den Titeltext der umschließenden GroupBox zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Text, der in der Benutzeroberfläche als Überschrift angezeigt wird.</value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den Text der Titelzeile zurück oder legt diesen fest.")>
        <DefaultValue("Inhalt")>
        Public Property TitelText As String
            Set(value As String)
                If Me._TitelText <> value Then
                    Me._TitelText = value
                    ' Löst die Aktualisierung der GroupBox-Beschriftung aus.
                    RaiseEvent PropertyTitelTextChanged()
                End If
            End Set
            Get
                Return Me._TitelText
            End Get
        End Property

        ''' <summary>
        ''' Gibt den angezeigten Inhalt als Zeilenarray zurück oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Die Änderungsprüfung erfolgt über Referenzvergleich. Bei inhaltlichen Änderungen sollte daher ein neues
        ''' Array zugewiesen werden.
        ''' </remarks>
        ''' <value>Zeilenarray für die schreibgeschützte Textanzeige.</value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den Dateiinhalt zurück oder legt diesen fest.")>
        <DefaultValue(GetType(String()), Nothing)>
        Public Property Lines As String()
            Get
                Return Me._Lines
            End Get
            Set
                If Me._Lines IsNot Value Then
                    ' Übernimmt den neuen Modellzustand und triggert die UI-Synchronisierung.
                    Me._Lines = Value
                    RaiseEvent PropertyLinesChanged()
                End If
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="ContentView"/>.
        ''' </summary>
        ''' <remarks>
        ''' Erstellt die Designer-Komponenten und übernimmt den initialen Titel der GroupBox in das interne Titelfeld.
        ''' </remarks>
        Public Sub New()

            Me.InitializeComponent()
            ' Übernimmt den im Designer hinterlegten Starttitel.
            Me._TitelText = Me.GroupBox.Text

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Synchronisiert den internen Zeileninhalt mit dem Anzeigefeld.
        ''' </summary>
        Private Sub ContentView_LinesChanged() Handles Me.PropertyLinesChanged

            ' Spiegelt den internen Zeileninhalt im Anzeigefeld wider.
            Me.TextBox.Lines = Me._Lines

        End Sub

        ''' <summary>
        ''' Synchronisiert den internen Titeltext mit der sichtbaren Gruppenüberschrift.
        ''' </summary>
        Private Sub ContentView_TitelTextChanged() Handles Me.PropertyTitelTextChanged

            ' Aktualisiert die sichtbare Gruppenüberschrift.
            Me.GroupBox.Text = Me._TitelText

        End Sub

#End Region

    End Class

End Namespace