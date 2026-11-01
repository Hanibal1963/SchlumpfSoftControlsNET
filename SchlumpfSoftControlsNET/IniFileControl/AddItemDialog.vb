' --------------------------------------------------------------------------------------------------------
' Datei: AddItemDialog.vb
' Author: Andreas Sauer
' Datum: 31.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Dialogfenster zur Eingabe eines neuen Textwertes (z. B. für einen neuen
    ''' INI-Schlüssel oder einen neuen INI-Eintrag).
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet">
    '''  <item>
    '''   <description>Der OK-Button ist anfangs deaktiviert und wird erst bei gültiger Eingabe aktiviert.</description>
    '''  </item>
    '''  <item>
    '''   <description>Bei Änderungen im Textfeld wird die Eingabe auf Leer- bzw. Whitespace-Inhalt geprüft.</description>
    '''  </item>
    '''  <item>
    '''   <description>Beim Bestätigen wird der Text in <see cref="NewItemValue"/> übernommen und der Dialog mit einem passenden <see cref="Form.DialogResult"/> geschlossen.</description>
    '''  </item>
    ''' </list>
    ''' </remarks>
    Friend NotInheritable Class AddItemDialog

        Inherits Form

#Region "Eigenschaften"

        ''' <summary>
        ''' Speichert den vom Benutzer bestätigten Textwert des Dialogs.
        ''' </summary>
        ''' <remarks>
        ''' Dieser Wert wird nur dann aus dem Textfeld übernommen, wenn der Benutzer
        ''' den Dialog mit dem OK-Button bestätigt. Bei Abbruch bleibt der zuletzt
        ''' gesetzte Wert unverändert.
        '''
        ''' Die Eigenschaft kann vor dem Anzeigen des Dialogs gesetzt werden, um einen
        ''' Startwert vorzubelegen oder einen bereits vorhandenen Wert erneut anzuzeigen.
        ''' </remarks>
        ''' <value>
        ''' Der zu speichernde Textwert des Elements.
        ''' </value>
        <Browsable(True)>
        <DefaultValue("")>
        Public Property NewItemValue As String = $""

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz des Dialogs und setzt den Startzustand.
        ''' </summary>
        ''' <remarks>
        ''' Nach dem Erzeugen der Oberfläche wird der OK-Button zunächst deaktiviert,
        ''' damit keine leere oder nur aus Leerzeichen bestehende Eingabe bestätigt
        ''' werden kann. Die Aktivierung erfolgt erst durch eine gültige Eingabe in
        ''' das Textfeld.
        ''' </remarks>
        Public Sub New()

            ' Erstellt alle im Designer definierten Steuerelemente und verdrahtet Events.
            Me.InitializeComponent()

            ' Sicherheitszustand beim Öffnen:
            ' Ohne gültigen Text darf der Benutzer den Dialog nicht mit OK schließen.
            Me.ButtonOK.Enabled = False

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Verarbeitet Klicks auf OK und Abbrechen.
        ''' </summary>
        ''' <param name="sender">Das auslösende Steuerelement.</param>
        ''' <param name="e">Ereignisdaten des Klick-Ereignisses.</param>
        Private Sub Button_Click(sender As Object, e As EventArgs) Handles ButtonOK.Click, ButtonCancel.Click

            ' Ein gemeinsamer Handler für beide Buttons:
            ' Wir unterscheiden anhand des auslösenden Steuerelements.
            Select Case True
                Case sender Is Me.ButtonOK
                    ' Nur bei OK wird der aktuelle Text als Ergebnis übernommen.
                    Me.SetNewItemValue()
                    ' Signalisiert dem aufrufenden Code: Eingabe wurde bestätigt.
                    Me.DialogResult = DialogResult.OK

                Case sender Is Me.ButtonCancel
                    ' Signalisiert dem aufrufenden Code: Vorgang wurde abgebrochen.
                    Me.DialogResult = DialogResult.Cancel

            End Select

            ' Schließt den Dialog nach der Auswahl.
            Me.Close()

        End Sub

        ''' <summary>
        ''' Überträgt den aktuellen Inhalt des Textfelds in <see cref="NewItemValue"/>.
        ''' </summary>
        Private Sub SetNewItemValue()

            ' Übernimmt den unveränderten Inhalt des Textfelds in die öffentliche Eigenschaft,
            ' damit der aufrufende Code nach Dialogende darauf zugreifen kann.
            Me.NewItemValue = Me.TextBox.Text

        End Sub

        ''' <summary>
        ''' Prüft bei jeder Textänderung die Eingabe und aktiviert/deaktiviert den OK-Button.
        ''' </summary>
        ''' <param name="sender">Das Textfeld, das das Ereignis ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten der Textänderung.</param>
        Private Sub TextBox_TextChanged(sender As Object, e As EventArgs) Handles TextBox.TextChanged

            ' Jede Texteingabe wird sofort validiert.
            ' Leere oder reine Whitespace-Eingaben sind nicht zulässig.
            If String.IsNullOrWhiteSpace(CType(sender, TextBox).Text) Then

                ' Ungültige Eingabe: Bestätigung deaktivieren.
                Me.ButtonOK.Enabled = False

            Else

                ' Gültige Eingabe: Bestätigung erlauben.
                Me.ButtonOK.Enabled = True

            End If

        End Sub

#End Region

    End Class

End Namespace