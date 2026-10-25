' --------------------------------------------------------------------------------------------------------
' Datei: BeforeSwitchPagesEventArgs.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Enthält die Indexwerte der Seiten bevor die Seiten gewechselt werden.
    ''' </summary>
    Public Class BeforeSwitchPagesEventArgs

        Inherits AfterSwitchPagesEventArgs

        Public Property Cancel As Boolean

        ''' <summary>
        ''' Index der neuen Seite
        ''' </summary>
        Public Overloads Property NewIndex As Int32
            Get
                Return Me._NewIndex
            End Get
            Set(value As Int32)
                Me._NewIndex = value
            End Set
        End Property

        Friend Sub New(OldIndex As Int32, NewIndex As Int32)
            MyBase.New(OldIndex, NewIndex)
        End Sub

    End Class

End Namespace