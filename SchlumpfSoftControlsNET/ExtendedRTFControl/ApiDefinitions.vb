' --------------------------------------------------------------------------------------------------------
' Datei: ApiDefinitions.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports System

Namespace ExtendedRTFControl

    Friend Module ApiDefinitions

        ' Win32-Konstante zum De-/Aktivieren des Redraws eines Fenster-Handles.
        Friend Const WM_SETREDRAW As Int32 = &HB

        ' Mindest-Schriftgröße.
        ' Kann bei Bedarf angepasst werden.
        Public Const MIN_FONT_SIZE As Single = 8.0F

        ' Sendet ein Windows-Message direkt an ein Fenster .
        ' Die Anwendung sollte 0 zurückgeben, wenn sie diese Nachricht verarbeitet.
        <DllImport("user32.dll")>
        Friend Function SendMessage(hWnd As IntPtr, msg As Int32, wParam As Boolean, lParam As IntPtr) As IntPtr
        End Function

    End Module

End Namespace
