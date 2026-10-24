' --------------------------------------------------------------------------------------------------------
' Datei: NotifyFormDesign.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel

Namespace NotifyFormControl

    ''' <summary>
    ''' Definiert die verfügbaren Farbschemata des Benachrichtigungsfensters.
    ''' </summary>
    Public Enum NotifyFormDesign As Int32

        ''' <summary>
        ''' Helles Design mit neutralen Farben.
        ''' </summary>
        Bright = 0

        ''' <summary>
        ''' Farbiges Design mit höherem Kontrast.
        ''' </summary>
        Colorful = 1

        ''' <summary>
        ''' Dunkles Design für zurückhaltende Darstellung.
        ''' </summary>
        Dark = 2

    End Enum

End Namespace
