' --------------------------------------------------------------------------------------------------------
' Datei: PageDesigner.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Windows.Forms
Imports System.Windows.Forms.Design
Imports System.ComponentModel

Namespace WizardControl

    Friend NotInheritable Class PageDesigner

        Inherits ParentControlDesigner

        Public Overrides ReadOnly Property SelectionRules As SelectionRules
            Get
                Return SelectionRules.Locked Or SelectionRules.Visible
            End Get
        End Property

    End Class

End Namespace