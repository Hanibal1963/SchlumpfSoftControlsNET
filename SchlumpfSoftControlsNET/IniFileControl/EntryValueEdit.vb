' --------------------------------------------------------------------------------------------------------
' Datei: EntryValueEdit.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Stellt ein Steuerelement zum Anzeigen und Bearbeiten eines INI-Eintragswerts innerhalb einer ausgewählten
    ''' Sektion bereit.
    ''' </summary>
    ''' <remarks>
    ''' Änderungen werden nicht sofort gemeldet, sondern erst nach expliziter Bestätigung über den Übernehmen-Button per
    ''' <see cref="ValueChanged"/>.
    ''' </remarks>
    <Description("Steuerelement zum Anzeigen und Bearbeiten der Einträge eines Abschnitts einer INI - Datei.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(EntryValueEdit), "IniFileControl.EntryValueEdit.bmp")> ' Hinweis: Das Bitmap "EntryValueEdit.bmp" muss als eingebettete Ressource vorliegen (BuildAction: Embedded Resource).
    Public Class EntryValueEdit : Inherits UserControl

#Region "Variablen"

        Private _TitelText As String
        Private _Value As String = String.Empty

#End Region

#Region "Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn der aktuell bearbeitete Wert übernommen wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="EntryValueEdit"/>.</param>
        ''' <param name="e">Enthält Sektion, Eintrag und den bestätigten Wert.</param>
        <Description("Wird ausgelöst wenn sich der Wert geändert hat.")>
        Public Event ValueChanged(sender As Object, e As EntryValueEditEventArgs)

        Private Event TitelTextChanged()
        Private Event PropertyValueChanged()

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt den Titeltext der umschließenden GroupBox zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Der in der Benutzeroberfläche angezeigte Titeltext.</value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den Text der Titelzeile zurück oder legt diesen fest.")>
        <DefaultValue("Eintrag")>
        Public Property TitelText As String
            Set(value As String)
                If Me._TitelText <> value Then
                    Me._TitelText = value
                    ' Löst die UI-Synchronisierung für den GroupBox-Titel aus.
                    RaiseEvent TitelTextChanged()
                End If
            End Set
            Get
                Return Me._TitelText
            End Get
        End Property

        ''' <summary>
        ''' Gibt die aktuell ausgewählte INI-Sektion zurück oder legt sie fest.
        ''' </summary>
        ''' <value>Abschnittsname, der im Ereignis mit übertragen wird.</value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den aktuell ausgewählten Abschnitt zurück oder legt diesen fest.")>
        <DefaultValue("")>
        Public Property SelectedSection As String = String.Empty

        ''' <summary>
        ''' Gibt den aktuell ausgewählten Eintrag innerhalb der Sektion zurück oder legt ihn fest.
        ''' </summary>
        ''' <value>Schlüsselname, der im Ereignis mit übertragen wird.</value>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den aktuell ausgewählten Eintrag zurück oder legt diesen fest.")>
        <DefaultValue("")>
        Public Property SelectedEntry As String

        ''' <summary>
        ''' Gibt den aktuell bearbeiteten Eintragswert zurück oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Beim Setzen wird der TextBox-Inhalt synchronisiert und der Übernehmen-Button deaktiviert, da der neue Wert
        ''' als aktueller Ausgangszustand gilt.
        ''' </remarks>
        ''' <value>Der bearbeitete Wert des aktuell ausgewählten INI-Eintrags.</value>
        <Description("Gibt den Eintragswert zurück oder legt diesen fest.")>
        <DefaultValue("")>
        Public Property Value As String
            Get
                Return Me._Value
            End Get
            Set
                If Me._Value <> Value Then
                    ' Übernimmt den neuen Modellwert und synchronisiert danach die UI.
                    Me._Value = Value
                    RaiseEvent PropertyValueChanged()
                End If
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der Klasse <see cref="EntryValueEdit"/>.
        ''' </summary>
        ''' <remarks>
        ''' Nach dem Erstellen der Designer-Elemente wird der initiale GroupBox-Titel in das interne Feld übernommen.
        ''' </remarks>
        Public Sub New()
            Me.InitializeComponent()
            ' Übernimmt den im Designer definierten Starttitel.
            Me._TitelText = Me.GroupBox.Text
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Wird ausgelöst, wenn der Benutzer den Übernehmen-Button klickt. Meldet den neuen Wert an den aufrufenden
        ''' Code und deaktiviert danach den Button, bis eine weitere Änderung erfolgt.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub Button_Click(sender As Object, e As EventArgs) Handles Button.Click

            ' Meldet den bestätigten Wert an den aufrufenden Code.
            RaiseEvent ValueChanged(Me, New EntryValueEditEventArgs(Me.SelectedSection, Me.SelectedEntry, Me._Value))
            ' Nach dem Commit bleibt der Button bis zur nächsten Änderung deaktiviert.
            Me.Button.Enabled = False

        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn der Benutzer den Text im Eingabefeld ändert. Aktiviert den Übernehmen-Button, wenn der
        ''' neue Wert vom gespeicherten Wert abweicht. Deaktiviert den Button, wenn der neue Wert wieder dem
        ''' gespeicherten Wert entspricht.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub TextBox_TextChanged(sender As Object, e As EventArgs) Handles TextBox.TextChanged

            If Me._Value <> Me.TextBox.Text Then
                ' Benutzereingabe weicht vom gespeicherten Wert ab: Commit möglich.
                Me.Button.Enabled = True
                ' Führt den internen Wert fortlaufend mit dem Eingabefeld mit.
                Me._Value = Me.TextBox.Text
            Else
                ' Kein Unterschied zum gespeicherten Wert: kein Commit erforderlich.
                Me.Button.Enabled = False
            End If

        End Sub

        ''' <summary>
        ''' Synchronisiert den GroupBox-Titel mit der Property <see cref="TitelText"/>, wenn diese programmatisch
        ''' geändert wurde.
        ''' </summary>
        Private Sub IniFileCommentEdit_TitelTextChanged() Handles Me.TitelTextChanged

            ' Spiegelt den Titel im UI-Container wider.
            Me.GroupBox.Text = Me._TitelText

        End Sub

        ''' <summary>
        ''' Synchronisiert den internen Wert mit dem Textfeld, wenn die Property <see cref="Value"/> programmatisch
        ''' geändert wurde. Deaktiviert danach den Übernehmen-Button, da der neue Wert als aktueller Ausgangszustand
        ''' gilt.
        ''' </summary>
        Private Sub IniFileEntryValueEdit_PropertyValueChanged() Handles Me.PropertyValueChanged

            ' Schreibt programmatische Wertänderungen in das Textfeld.
            Me.TextBox.Text = Me._Value
            ' Programmatische Änderungen gelten als aktueller Basiszustand.
            Me.Button.Enabled = False

        End Sub

#End Region

    End Class

End Namespace