' --------------------------------------------------------------------------------------------------------
' Datei: PageStyle.vb
' Author: Andreas Sauer
' Datum: 25.05.2026
' --------------------------------------------------------------------------------------------------------

Namespace WizardControl

    ''' <summary>
    ''' Auflistung der verfügbaren Seitenstile
    ''' </summary>
    Public Enum PageStyle

        ''' <summary>
        ''' Stellt eine Standardseite dar.
        ''' </summary>
        Standard = 0

        ''' <summary>
        ''' Stellt die Willkommensseite dar.
        ''' </summary>
        Welcome = 1

        ''' <summary>
        ''' Stellt die Abschlußseite dar.
        ''' </summary>
        Finish = 2

        ''' <summary>
        ''' Stellt eine Benutzerdefinierte Seite dar.
        ''' </summary>
        Custom = 3

    End Enum

End Namespace