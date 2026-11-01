' --------------------------------------------------------------------------------------------------------
' Datei: RenameItemDialog.vb
' Author: Andreas Sauer
' Datum: 31.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Dialog zur Eingabe eines neuen Namens für ein vorhandenes Element.
    ''' </summary>
    ''' <remarks>
    ''' Der Dialog zeigt den alten Namen im Hinweistext an und akzeptiert die Eingabe
    ''' nur, wenn sie nicht leer oder nur aus Leerzeichen besteht.
    ''' </remarks>
    Friend NotInheritable Class RenameItemDialog

        Inherits Form

#Region "Variablen"

        Private _OldItemValue As String = $""

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Enthält den bisherigen Namen des umzubenennenden Elements.
        ''' </summary>
        ''' <remarks>
        ''' Beim Setzen dieser Eigenschaft wird die Hinweiszeile des Dialogs direkt aktualisiert. Dadurch ist für den
        ''' Benutzer jederzeit klar ersichtlich, welcher vorhandene Name ersetzt werden soll.
        ''' </remarks>
        ''' <value>
        ''' Der aktuelle Elementname vor der Umbenennung.
        ''' </value>
        <Browsable(True)>
        <DefaultValue("")>
        Public Property OldItemValue As String
            Get
                Return Me._OldItemValue
            End Get
            Set
                ' Speichert den alten Wert intern für spätere Referenz.
                Me._OldItemValue = Value

                ' Aktualisiert den Hinweistext, damit der Kontext der Umbenennung eindeutig bleibt.
                Me.Label.Text = $"Element '{Me._OldItemValue}' umbenennen in:"
            End Set
        End Property

        ''' <summary>
        ''' Speichert den neuen Namen, den der Benutzer bestätigt hat.
        ''' </summary>
        ''' <remarks>
        ''' Der Wert wird erst beim Klick auf den Ja-Button aus dem Textfeld in diese
        ''' Eigenschaft übernommen. Bei Auswahl von Nein bleibt der zuletzt gesetzte
        ''' Wert unverändert.
        ''' </remarks>
        ''' <value>
        ''' Der bestätigte neue Elementname.
        ''' </value>
        <Browsable(True)>
        <DefaultValue("")>
        Public Property NewItemValue As String = $""

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert den Dialog und setzt den sicheren Initialzustand.
        ''' </summary>
        ''' <remarks>
        ''' Direkt nach dem Aufbau der Oberfläche ist der Ja-Button deaktiviert, damit
        ''' keine leere oder ungültige Umbenennung bestätigt werden kann. Erst eine
        ''' gültige Eingabe aktiviert die Bestätigung.
        ''' </remarks>
        Public Sub New()
            ' Erstellt alle Steuerelemente und Event-Verknüpfungen aus dem Designer.
            Me.InitializeComponent()

            ' Initial darf keine Umbenennung ohne Benutzereingabe bestätigt werden.
            Me.ButtonYes.Enabled = False
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Überträgt den aktuellen Inhalt des Textfelds in <see cref="NewItemValue"/>.
        ''' </summary>
        Private Sub SetNewItemValue()
            ' Übernimmt den aktuellen Eingabetext als neues Umbenennungsergebnis.
            Me.NewItemValue = Me.TextBox.Text
        End Sub

        ''' <summary>
        ''' Verarbeitet Klicks auf die Buttons "Ja" und "Nein".
        ''' </summary>
        ''' <param name="sender">Das Steuerelement, das den Klick ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten des Klick-Ereignisses.</param>
        Private Sub Button_Click(sender As Object, e As EventArgs) Handles ButtonYes.Click, ButtonNo.Click
            If sender Is Me.ButtonYes Then
                ' Bei Bestätigung den neuen Namen sichern.
                Me.SetNewItemValue()

                ' Ergebnis für den aufrufenden Code: Umbenennung bestätigt.
                Me.DialogResult = DialogResult.Yes
            ElseIf sender Is Me.ButtonNo Then
                ' Ergebnis für den aufrufenden Code: Umbenennung verworfen.
                Me.DialogResult = DialogResult.No
            End If

            ' Dialog nach der Entscheidung schließen.
            Me.Close()
        End Sub

        ''' <summary>
        ''' Aktiviert oder deaktiviert den Ja-Button abhängig vom Eingabetext.
        ''' </summary>
        ''' <param name="sender">Die TextBox, deren Inhalt geprüft wird.</param>
        ''' <param name="e">Ereignisdaten der Textänderung.</param>
        Private Sub TextBox_TextChanged(sender As Object, e As EventArgs) Handles TextBox.TextChanged
            ' Aktiviert die Bestätigung nur bei sinnvoller Eingabe.
            If String.IsNullOrWhiteSpace(CType(sender, TextBox).Text) Then
                ' Ungültige Eingabe (leer/Whitespace): Bestätigen sperren.
                Me.ButtonYes.Enabled = False
            Else
                ' Gültige Eingabe: Bestätigen erlauben.
                Me.ButtonYes.Enabled = True
            End If
        End Sub


#End Region

    End Class

End Namespace