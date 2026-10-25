' --------------------------------------------------------------------------------------------------------
' Datei: PageFinish.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Definiert die Abschlußseite des Assistenten.
    ''' </summary>
    ''' <remarks>
    ''' Diese Seite wird typischerweise am Ende des Assistenten angezeigt, um Ergebnisse
    ''' zusammenzufassen oder eine Bestätigung bereitzustellen.
    ''' </remarks>
    <ToolboxItem(False)>
    Public Class PageFinish

        Inherits WizardPage

        ' Privates Feld zur Speicherung des Seitenstils.
        Private _Style As PageStyle = PageStyle.Finish

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Ruft den Stil der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Der Standardwert ist <see cref="PageStyle.Finish"/>.
        ''' </remarks>
        <DefaultValue(PageStyle.Finish)>
        <Category("Design")>
        <Description("Ruft den Stil der Assistentenseite ab oder legt diesen fest.")>
        Public Overrides Property Style As PageStyle
            Get
                Return Me._Style
            End Get
            Set(value As PageStyle)
                Me._Style = value
            End Set
        End Property

    End Class

End Namespace