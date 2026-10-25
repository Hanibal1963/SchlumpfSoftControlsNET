' --------------------------------------------------------------------------------------------------------
' Datei: PageCustom.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Definiert eine benutzerdefinierte Assistentenseite.
    ''' </summary>
    ''' <remarks>
    ''' Verwenden Sie diese Klasse, um eigene Inhalte und Verhalten innerhalb eines
    ''' Assistenten bereitzustellen.
    ''' </remarks>
    <ToolboxItem(False)>
    Public Class PageCustom

        Inherits WizardPage

        ' Interner Speicher für den Seitenstil (Standard: Custom)
        Private _Style As PageStyle = PageStyle.Custom

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Ruft den Stil der Assistentenseite ab oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Der Stil steuert das Erscheinungsbild und Verhalten der Seite innerhalb des
        ''' Assistenten.<br/>
        ''' Der Standardwert ist <see cref="PageStyle.Custom"/>.
        ''' </remarks>
        <DefaultValue(PageStyle.Custom)>
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