' --------------------------------------------------------------------------------------------------------
' Datei: TransparentLabel.vb
' Author: Andreas Sauer
' Datum: 05.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace TransparentLabelControl

    ''' <summary>
    ''' Ein Steuerelement zum Anzeigen eines Textes mit durchscheinendem Hintergrund.
    ''' </summary>
    <Description("Ein Steuerelement zum Anzeigen eines Textes mit durchscheinendem Hintergrund.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(TransparentLabel), "TransparentLabelControl.TransparentLabel.bmp")>
    Public Class TransparentLabel : Inherits Label

        ' Konstante für den erweiterten Fensterstil WS_EX_TRANSPARENT, um die Transparenz zu ermöglichen.
        Private Const WS_EX_TRANSPARENT As Int32 = &H20

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der <see cref="TransparentLabel"/> -Klasse.
        ''' </summary>
        Public Sub New()

            ' Eigene Stil-Einstellungen werden direkt in der Konstruktion gesetzt,
            ' damit Transparenz und Zeichenverhalten korrekt konfiguriert sind.
            Me.Name = NameOf(TransparentLabel)
            Me.InitializeStyles()
            Me.BackColor = Color.Transparent

        End Sub

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DefaultValue(GetType(Color), "Transparent")>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overrides Property BackColor As Color
            Get
                Return MyBase.BackColor
            End Get
            Set(value As Color)
                If value <> Color.Transparent AndAlso value <> Color.Empty Then
                    Throw New ArgumentException("Für dieses Steuerelement ist nur Color.Transparent zulässig.", NameOf(value))
                End If

                ' Für dieses Steuerelement wird BackColor immer transparent erzwungen.
                MyBase.BackColor = Color.Transparent
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overrides Property BackgroundImage As Image
            Get
                Return MyBase.BackgroundImage
            End Get
            Set(value As Image)
                MyBase.BackgroundImage = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overrides Property BackgroundImageLayout As ImageLayout
            Get
                Return MyBase.BackgroundImageLayout
            End Get
            Set(value As ImageLayout)
                MyBase.BackgroundImageLayout = value
            End Set
        End Property

        ''' <summary>
        ''' Ausgeblendet da für dieses Control nicht relevant.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property FlatStyle As FlatStyle
            Get
                Return MyBase.FlatStyle
            End Get
            Set(value As FlatStyle)
                MyBase.FlatStyle = value
            End Set
        End Property

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Konfiguriert die relevanten <see cref="ControlStyles"/> für das Label.
        ''' </summary>
        ''' <remarks>
        '''
        ''' <para>Die Kombination der Styles sorgt dafür, dass das Steuerelement seinen Hintergrund nicht selbst opak
        ''' zeichnet und transparente Hintergründe unterstützt. </para>
        '''
        ''' <para>Das Deaktivieren von <c>OptimizedDoubleBuffer</c> hilft in diesem Szenario, unerwünschte
        ''' Übermal-Effekte bei transparenter Darstellung zu vermeiden. </para>
        ''' </remarks>
        Private Sub InitializeStyles()

            ' Relevante Transparenz-Styles in einem Schritt setzen.
            Me.SetStyle(ControlStyles.Opaque Or ControlStyles.SupportsTransparentBackColor, True)

            ' Doppelbuffering hier bewusst deaktivieren, damit die Transparenzdarstellung
            ' konsistent mit dem Eltern-Steuerelement funktioniert.
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, False)

            ' Style-Änderungen auf das Handle anwenden.
            Me.UpdateStyles()

        End Sub

        ''' <summary>
        ''' Liefert angepasste Erstellungsparameter für das Fensterhandle des Steuerelements.
        ''' </summary>
        ''' <remarks>
        '''
        ''' <para>Das Setzen von <c>WS_EX_TRANSPARENT</c> sorgt dafür, dass zuerst das Eltern-Steuerelement gezeichnet
        ''' wird und der Hintergrund dadurch durchscheinen kann. </para>
        '''
        ''' <para><b>Weitere Infos unter:</b><br/>
        ''' <see href="https://stackoverflow.com/questions/511320/transparent-control-backgrounds-on-a-vb-net-gradient-filled-form"/>
        ''' <br/> und<br/> <see href="https://learn.microsoft.com/de-de/windows/win32/winmsg/extended-window-styles"/>
        ''' </para>
        ''' </remarks>
        ''' <value>
        ''' Die angepassten <see cref="CreateParams"/> mit aktiviertem WS_EX_TRANSPARENT-Stil.
        ''' </value>
        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                ' Ausgangswerte vom Basistyp übernehmen.
                Dim cp As CreateParams = MyBase.CreateParams

                ' Erweiterten Fensterstil WS_EX_TRANSPARENT hinzufügen.
                ' Quelle: https://learn.microsoft.com/de-de/windows/win32/winmsg/extended-window-styles
                cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT

                ' Angepasste Erstellungsparameter an WinForms zurückgeben.
                Return cp
            End Get
        End Property

#End Region

    End Class

End Namespace
