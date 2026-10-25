' --------------------------------------------------------------------------------------------------------
' Datei: PageWelcome.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Definiert die Willkommenseite für den Assistenten.
    ''' </summary>
    ''' <remarks>
    ''' Diese Seite dient als Einstiegsseite und verwendet standardmäßig den Stil <see
    ''' cref="PageStyle.Welcome"/>.
    ''' </remarks>
    <ToolboxItem(False)>
    Public Class PageWelcome

        Inherits WizardPage

        ' Privates Feld zur Speicherung des Seitenstils.
        Private _Style As PageStyle = PageStyle.Welcome

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Ruft den Stil der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Standardwert ist <see cref="PageStyle.Welcome"/>. Die Änderung des Stils kann
        ''' das Layout und die Darstellung der Seite beeinflussen.
        ''' </remarks>
        ''' <value>
        ''' Der aktuell konfigurierte Seitenstil des Assistenten.
        ''' </value>
        <DefaultValue(PageStyle.Welcome)>
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