' --------------------------------------------------------------------------------------------------------
' Datei: PageStandard.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Definiert eine Standardseite.
    ''' </summary>
    ''' <remarks>
    ''' Diese Seite verwendet standardmäßig den Stil <see cref="PageStyle.Standard"/>.
    ''' </remarks>
    <ToolboxItem(False)>
    Public Class PageStandard

        Inherits WizardPage

        ' Privates Feld zur Speicherung des Seitenstils.
        Private _Style As PageStyle = PageStyle.Standard

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Ruft den Stil der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Der Standardwert ist <see cref="PageStyle.Standard"/>.
        ''' </remarks>
        ''' <value>
        ''' Der aktuell konfigurierte Stil der Seite als <see cref="PageStyle"/>.
        ''' </value>
        <DefaultValue(PageStyle.Standard)>
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