' --------------------------------------------------------------------------------------------------------
' Datei: FormShape.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET

Public Class FormShape

    Public Sub New()
        Me.InitializeComponent()
        Me.InitializeComboBoxes()
        Me.InitializeShapeControl()
    End Sub

    ''' <summary>
    ''' Standardauswahl für die ComboBoxen festlegen.
    ''' </summary>
    Private Sub InitializeComboBoxes()
        Me.ComboBox_ShapeMode.SelectedIndex = 0
        Me.ComboBox_DiagonalLineMode.SelectedIndex = 0
    End Sub

    ''' <summary>
    ''' Anfangswerte aus den Steuerelementen in das Shape-Control übernehmen.
    ''' </summary>
    Private Sub InitializeShapeControl()
        Me.Shape.ShapeModus = CType(Me.ComboBox_ShapeMode.SelectedIndex, ShapeControl.ShapeModes)
        Me.Shape.DiagonalLineModus = CType(Me.ComboBox_DiagonalLineMode.SelectedIndex, ShapeControl.DiagonalLineModes)
        Me.Shape.LineWidth = Me.NumericUpDown_LineWidth.Value
    End Sub

    ''' <summary>
    ''' Reagiert auf Änderungen des Shape-Modus und aktiviert abhängig vom Modus zusätzliche Eingabesteuerelemente.
    ''' </summary>
    ''' <param name="sender">Die auslösende <see cref="ComboBox"/>.</param>
    ''' <param name="e">Ereignisdaten der Auswahländerung.</param>
    Private Sub ComboBox_ShapeMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_ShapeMode.SelectedIndexChanged
        ' Zusatzoptionen standardmäßig deaktivieren und nur bei Bedarf einschalten.
        Me.ComboBox_DiagonalLineMode.Enabled = False
        Me.Button_FillColor.Enabled = False
        Dim selindex As Int32 = CType(sender, ComboBox).SelectedIndex
        ' Ausgewählten Index auf den zugehörigen Shape-Modus abbilden.
        Select Case selindex
            Case 0 : Me.Shape.ShapeModus = ShapeControl.ShapeModes.HorizontalLine
            Case 1 : Me.Shape.ShapeModus = ShapeControl.ShapeModes.VerticalLine
            Case 2
                Me.Shape.ShapeModus = ShapeControl.ShapeModes.DiagonalLine
                Me.ComboBox_DiagonalLineMode.Enabled = True
            Case 3 : Me.Shape.ShapeModus = ShapeControl.ShapeModes.Rectangle
            Case 4
                Me.Shape.ShapeModus = ShapeControl.ShapeModes.FilledRectangle
                Me.Button_FillColor.Enabled = True
            Case 5 : Me.Shape.ShapeModus = ShapeControl.ShapeModes.Ellipse
            Case 6
                Me.Shape.ShapeModus = ShapeControl.ShapeModes.FilledEllipse
                Me.Button_FillColor.Enabled = True
        End Select
    End Sub

    ''' <summary>
    ''' Reagiert auf Änderungen des Diagonal-Line-Modus und setzt die gewünschte Linienrichtung.
    ''' </summary>
    ''' <param name="sender">Die auslösende <see cref="ComboBox"/>.</param>
    ''' <param name="e">Ereignisdaten der Auswahländerung.</param>
    Private Sub ComboBox_DiagonalLineMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox_DiagonalLineMode.SelectedIndexChanged
        Dim selindex As Int32 = CType(sender, ComboBox).SelectedIndex
        ' Gewählte Richtung für die diagonale Linie übernehmen.
        Select Case selindex
            Case 0 : Me.Shape.DiagonalLineModus = ShapeControl.DiagonalLineModes.TopLeftToBottomRight
            Case 1 : Me.Shape.DiagonalLineModus = ShapeControl.DiagonalLineModes.BottomLeftToTopRight
        End Select
    End Sub

    ''' <summary>
    ''' Reagiert auf Änderungen der Linienstärke und übergibt den Wert an das Shape-Control.
    ''' </summary>
    ''' <param name="sender">Das auslösende <see cref="NumericUpDown"/>-Steuerelement.</param>
    ''' <param name="e">Ereignisdaten der Wertänderung.</param>
    Private Sub NumericUpDown_LineWidth_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown_LineWidth.ValueChanged
        Dim value As Decimal = CType(sender, NumericUpDown).Value
        Me.Shape.LineWidth = value
    End Sub

    ''' <summary>
    ''' Öffnet den Farbdialog zur Auswahl der Linienfarbe und übernimmt die Auswahl bei Bestätigung.
    ''' </summary>
    ''' <param name="sender">Das auslösende Schaltflächen-Steuerelement.</param>
    ''' <param name="e">Ereignisdaten des Klicks.</param>
    Private Sub Button_LineColor_Click(sender As Object, e As EventArgs) Handles Button_LineColor.Click
        ' Aktuelle Linienfarbe vorbelegen, damit der Dialog den Ist-Zustand zeigt.
        Me.ColorDialog.Color = Me.Shape.LineColor
        Dim result As DialogResult = Me.ColorDialog.ShowDialog(Me)
        ' Nur bestätigte Auswahl übernehmen.
        If result = DialogResult.OK Then
            Me.Shape.LineColor = Me.ColorDialog.Color
        End If
    End Sub

    ''' <summary>
    ''' Öffnet den Farbdialog zur Auswahl der Füllfarbe und übernimmt die Auswahl bei Bestätigung.
    ''' </summary>
    ''' <param name="sender">Das auslösende Schaltflächen-Steuerelement.</param>
    ''' <param name="e">Ereignisdaten des Klicks.</param>
    Private Sub Button_FillColor_Click(sender As Object, e As EventArgs) Handles Button_FillColor.Click
        ' Aktuelle Füllfarbe vorbelegen, damit der Dialog den Ist-Zustand zeigt.
        Me.ColorDialog.Color = Me.Shape.FillColor
        Dim result As DialogResult = Me.ColorDialog.ShowDialog(Me)
        ' Nur bestätigte Auswahl übernehmen.
        If result = DialogResult.OK Then
            Me.Shape.FillColor = Me.ColorDialog.Color
        End If
    End Sub

End Class