' --------------------------------------------------------------------------------------------------------
' Datei: DeleteItemDialog.vb
' Author: Andreas Sauer
' Datum: 31.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Dialog zur Bestätigung des Löschens eines Elements.
    ''' </summary>
    ''' <remarks>
    ''' Der Dialog verwendet <see cref="DialogResult.OK"/> für eine Bestätigung und
    ''' <see cref="DialogResult.Cancel"/> für einen Abbruch.
    ''' </remarks>
    Friend NotInheritable Class DeleteItemDialog

        Inherits Form

#Region "Variablen"

        Private _ItemValue As String = $""

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Enthält den Wert des Elements, das gelöscht werden soll.
        ''' </summary>
        ''' <remarks>
        ''' Beim Setzen der Eigenschaft wird der Bestätigungstext im Dialog unmittelbar aktualisiert, damit der Benutzer
        ''' genau sieht, welches Element betroffen ist. Dadurch bleibt die Anzeige konsistent, auch wenn der Wert kurz
        ''' vor dem Anzeigen des Dialogs noch geändert wird.
        ''' </remarks>
        ''' <value>
        ''' Der Elementwert, der in der Löschabfrage angezeigt wird.
        ''' </value>
        <DefaultValue("")>
        Public Property ItemValue As String
            Get
                Return Me._ItemValue
            End Get
            Set
                ' Speichert den intern verwendeten Wert des zu löschenden Elements.
                Me._ItemValue = Value

                ' Aktualisiert den Hinweistext im UI, damit die Rückfrage eindeutig ist.
                Me.Label.Text = $"Möchten Sie das Element '{Me._ItemValue}' wirklich löschen?"
            End Set
        End Property

#End Region

#Region "öffentliche Methoden"

        ''' <summary>
        ''' Erstellt eine neue Instanz des <see cref="DeleteItemDialog"/>.
        ''' </summary>
        ''' <remarks>
        ''' In <see cref="InitializeComponent"/> werden alle im Designer definierten Steuerelemente erzeugt,
        ''' initialisiert und den Ereignissen zugeordnet.
        ''' </remarks>
        Public Sub New()

            ' Baut die Oberfläche des Dialogs gemäß Designer-Datei auf.
            Me.InitializeComponent()

        End Sub

#End Region

#Region "interne Methoden"

        ''' <summary>
        ''' Verarbeitet Klicks auf die Buttons "Ja" und "Nein".
        ''' </summary>
        ''' <param name="sender">Das Steuerelement, das das Klick-Ereignis ausgelöst hat.</param>
        ''' <param name="e">Ereignisdaten des Klick-Ereignisses.</param>
        Private Sub Button_Click(sender As Object, e As EventArgs) Handles ButtonYes.Click, ButtonNo.Click

            If sender Is Me.ButtonYes Then
                ' Benutzer bestätigt das Löschen.
                Me.DialogResult = DialogResult.OK
            ElseIf sender Is Me.ButtonNo Then
                ' Benutzer bricht den Vorgang explizit ab.
                Me.DialogResult = DialogResult.Cancel
            End If

            ' Dialog unabhängig von der Auswahl schließen.
            Me.Close()

        End Sub

#End Region

    End Class

End Namespace