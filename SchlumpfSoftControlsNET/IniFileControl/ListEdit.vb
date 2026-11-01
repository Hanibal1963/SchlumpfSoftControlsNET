' --------------------------------------------------------------------------------------------------------
' Datei: ListEdit.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace IniFileControl

    ''' <summary>
    ''' Stellt eine editierbare Liste für Abschnitte oder Einträge einer INI-Datei bereit.
    ''' </summary>
    ''' <remarks>
    '''
    ''' <para><b>Darstellung:</b></para>
    '''
    ''' <list type="bullet"><item><description>Eine GroupBox mit Titel (Eigenschaft <see cref="TitelText"/> ).
    ''' </description> </item> <item><description>Eine ListBox mit Einträgen (Eigenschaft <see cref="ListItems"/> ).
    ''' </description> </item> <item><description>Drei Buttons: Hinzufügen, Umbenennen, Löschen. Interaktion:
    ''' </description> </item> <item><description>Auswahländerung in der ListBox löst <see cref="SelectedItemChanged"/>
    ''' aus. </description> </item> <item><description>Button-Klicks lösen semantische Ereignisse aus (<see
    ''' cref="ItemAdd"/>, <see cref="ItemRename"/>, <see cref="ItemRemove"/> ), die vom Host verarbeitet
    ''' werden.</description> </item> </list>
    ''' </remarks>
    <Description("Steuerelement zum Anzeigen und Bearbeiten der Abschnitts- oder Eintrags- Liste einer INI - Datei.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ListEdit), "IniFileControl.ListEdit.bmp")>
    Public Class ListEdit : Inherits UserControl

#Region "Variablen"

        Private _SelectedItem As String = String.Empty
        Private _Items As String() = {""}
        Private _TitelText As String

#End Region

#Region "Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn ein neuer Eintrag angefordert wurde.
        ''' </summary>
        ''' <remarks>
        ''' Die tatsächliche Erstellung erfolgt im Host. Danach sollte der Host <see cref="ListItems"/> mit der
        ''' aktualisierten Datenquelle neu setzen.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Eintrag hinzugefügt werden soll.")>
        <Category("ListEdit")>
        Public Event ItemAdd(sender As Object, e As ListEditAddEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein bestehender Eintrag umbenannt werden soll.
        ''' </summary>
        ''' <remarks>
        ''' Alter und neuer Wert werden über <see cref="ListEditRenameEventArgs"/> transportiert.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Eintrag umbenannt werden soll.")>
        <Category("ListEdit")>
        Public Event ItemRename(sender As Object, e As ListEditRenameEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn der aktuell gewählte Eintrag gelöscht werden soll.
        ''' </summary>
        ''' <remarks>
        ''' Das Ereignis enthält den zum Löschzeitpunkt ausgewählten Eintrag.
        ''' </remarks>
        <Description("Wird ausgelöst wenn ein Eintrag gelöscht werden soll.")>
        <Category("ListEdit")>
        Public Event ItemRemove(sender As Object, e As ListEditRemoveEventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn sich die Auswahl in der ListBox geändert hat.
        ''' </summary>
        ''' <remarks>
        ''' Bei leerer Auswahl enthält <c>e.SelectedElement</c> einen leeren String.
        ''' </remarks>
        <Description("Wird ausgelöst wenn sich der gewählte Eintrag geändert hat.")>
        <Category("ListEdit")>
        Public Event SelectedItemChanged(sender As Object, e As ListEditSelectedElementChangedEventArgs)

        Private Event TitelTextChanged()
        Private Event ListItemsChanged()

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt den Titeltext der GroupBox zurück oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Beim Ändern des Werts wird <c>TitelTextChanged</c> ausgelöst und die Anzeige aktualisiert.
        ''' </remarks>
        <Browsable(True)>
        <Category("Appearance")>
        <Description("Gibt den Text der Titelzeile zurück oder legt diesen fest.")>
        <DefaultValue("Liste")>
        Public Property TitelText As String
            Set(value As String)
                ' Nur ändern, wenn sich der Wert wirklich unterscheidet, um unnötige UI-Updates zu vermeiden.
                If value <> Me._TitelText Then
                    Me._TitelText = value
                    RaiseEvent TitelTextChanged()
                End If
            End Set
            Get
                Return Me._TitelText
            End Get
        End Property

        ''' <summary>
        ''' Gibt die anzuzeigenden Listeneinträge zurück oder ersetzt diese vollständig.
        ''' </summary>
        ''' <remarks>
        ''' Bei Zuweisung einer neuen Array-Instanz wird die ListBox neu aufgebaut.
        ''' </remarks>
        <Browsable(True)>
        <Category("Data")>
        <Description("Setzt die Elemente der Listbox oder gibt diese zurück.")>
        <DefaultValue(GetType(String()), Nothing)>
        Public Property ListItems() As String()
            Set
                If Me._Items IsNot Value Then
                    Me._Items = Value
                    RaiseEvent ListItemsChanged()
                End If
            End Set
            Get
                Return Me._Items
            End Get
        End Property

        ''' <summary>
        ''' Gibt den aktuell zugehörigen Abschnittsnamen zurück.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property SelectedElement As String
            Get
                Return Me._SelectedItem
            End Get
        End Property

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert das Steuerelement und übernimmt den aktuellen GroupBox-Titel als Startwert.
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()
            Me._TitelText = Me.GroupBox.Text
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Wird ausgelöst, wenn sich die Auswahl in der ListBox geändert hat. Die interne Property
        ''' <see cref="_SelectedItem"/> wird aktualisiert und das Ereignis <see cref="SelectedItemChanged"/> ausgelöst.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ListBox_SelectedIndex_Changed(sender As Object, e As EventArgs) Handles ListBox.SelectedIndexChanged

            If Me.ListBox.SelectedIndex = -1 Then
                Me.ClearPropertySelectedItem()
            Else
                Me.SetPropertySelectedItem()
            End If

            RaiseEvent SelectedItemChanged(Me, New ListEditSelectedElementChangedEventArgs(Me._SelectedItem))

        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn der Button "Hinzufügen" geklickt wird. Ein
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ButtonAdd_Click(sender As Object, e As EventArgs) Handles ButtonAdd.Click

            ' Dialog zur Eingabe eines neuen Elementnamens anzeigen.
            Dim newitemdlg As New AddItemDialog
            Dim result As DialogResult = newitemdlg.ShowDialog(Me)

            ' Nur bei bestätigter Eingabe wird das semantische Add-Ereignis ausgelöst.
            If result = DialogResult.OK Then
                RaiseEvent ItemAdd(Me, New ListEditAddEventArgs(newitemdlg.NewItemValue))
            End If

        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn der Button "Umbenennen" geklickt wird. Ein Dialog zur Eingabe des neuen Namens wird
        ''' angezeigt.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ButtonRename_Click(sender As Object, e As EventArgs) Handles ButtonRename.Click

            ' Umbenennen-Dialog mit aktuellem Element vorbelegen.
            Dim renamedlg As New RenameItemDialog With {.OldItemValue = Me._SelectedItem}
            Dim result As DialogResult = renamedlg.ShowDialog(Me)

            ' Nur bei Bestätigung mit "Yes" wird das Rename-Ereignis an den Host gemeldet.
            If result = DialogResult.Yes Then
                RaiseEvent ItemRename(Me, New ListEditRenameEventArgs(Me._SelectedItem, renamedlg.NewItemValue))
            End If

        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn der Button "Löschen" geklickt wird. Ein Dialog zur Bestätigung des Löschens wird
        ''' angezeigt.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ButtonDelete_Click(sender As Object, e As EventArgs) Handles ButtonDelete.Click

            ' Löschdialog mit aktuellem Element anzeigen.
            Dim deldlg As New DeleteItemDialog With {.ItemValue = Me._SelectedItem}
            Dim result As DialogResult = deldlg.ShowDialog(Me)

            ' Nur bei Bestätigung wird der Löschwunsch an den Host signalisiert.
            If result = System.Windows.Forms.DialogResult.OK Then
                RaiseEvent ItemRemove(Me, New ListEditRemoveEventArgs(Me._SelectedItem))
            End If

        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn die Datenquelle der ListBox geändert wurde. Die ListBox wird neu aufgebaut.
        ''' </summary>
        Private Sub IniFileListEdit_ListItemsChanged() Handles Me.ListItemsChanged
            ' Bei neuer Datenquelle die komplette ListBox neu aufbauen.
            Me.FillListbox()
        End Sub

        ''' <summary>
        ''' Wird ausgelöst, wenn sich der Titeltext geändert hat. Die GroupBox wird mit dem neuen Text aktualisiert.
        ''' </summary>
        Private Sub IniFileListEdit_TitelTextChanged() Handles Me.TitelTextChanged
            ' Geänderten Titel direkt auf die GroupBox übertragen.
            Me.GroupBox.Text = Me._TitelText
        End Sub

        ''' <summary>
        ''' Setzt die interne Property <see cref="_SelectedItem"/> auf den aktuell in der ListBox gewählten Eintrag.
        ''' </summary>
        Private Sub SetPropertySelectedItem()
            Me._SelectedItem = CStr(Me.ListBox.SelectedItem)
            Me.ButtonDelete.Enabled = True
            Me.ButtonRename.Enabled = True
        End Sub

        ''' <summary>
        ''' Setzt die interne Property <see cref="_SelectedItem"/> auf einen leeren String und deaktiviert die Buttons
        ''' "Löschen" und "Umbenennen".
        ''' </summary>
        Private Sub ClearPropertySelectedItem()
            ' Auswahl zurücksetzen und Aktionen deaktivieren.
            Me._SelectedItem = String.Empty
            Me.ButtonDelete.Enabled = False
            Me.ButtonRename.Enabled = False
        End Sub

        ''' <summary>
        ''' Füllt die ListBox mit den aktuellen Einträgen aus <see cref="_Items"/> und setzt die Auswahl zurück.
        ''' </summary>
        Private Sub FillListbox()

            Me.ListBox.Items.Clear()

            If Me._Items IsNot Nothing Then
                Me.ListBox.Items.AddRange(Me._Items)
            End If

            Me.ListBox.SelectedIndex = -1
            Me._SelectedItem = ""
            Me.ButtonAdd.Enabled = True
            Me.ButtonDelete.Enabled = False
            Me.ButtonRename.Enabled = False

            RaiseEvent SelectedItemChanged(Me, New ListEditSelectedElementChangedEventArgs(Me._SelectedItem))

        End Sub

#End Region

    End Class

End Namespace