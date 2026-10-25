' --------------------------------------------------------------------------------------------------------
' Datei: AfterSwitchPagesEventArgs.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.ComponentModel

Namespace WizardControl

    ''' <summary>
    ''' Enthält die Indexwerte der Seiten nachdem die Seiten gewechselt wurden.
    ''' </summary>
    Public Class AfterSwitchPagesEventArgs

        Inherits EventArgs

        Private Protected _NewIndex As Int32
        Private Protected _OldIndex As Int32

        ''' <summary>
        ''' Index der alten Seite
        ''' </summary>
        Public ReadOnly Property OldIndex As Int32
            Get
                Return Me._OldIndex
            End Get
        End Property

        ''' <summary>
        ''' Index der neuen Seite
        ''' </summary>
        Public ReadOnly Property NewIndex As Int32
            Get
                Return Me._NewIndex
            End Get
        End Property

        Friend Sub New(OldIndex As Int32, NewIndex As Int32)
            Me._OldIndex = OldIndex
            Me._NewIndex = NewIndex
        End Sub

    End Class

End Namespace
