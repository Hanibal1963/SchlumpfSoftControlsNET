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

        ''' <summary>Ermittelt die Zeichenformatierung der aktuellen Auswahl.</summary>
        Friend Const EM_GETCHARFORMAT As Int32 = &H43A

        ''' <summary>Ermittelt die Absatzformatierung der aktuellen Auswahl.</summary>
        Friend Const EM_GETPARAFORMAT As Int32 = &H43D

        ''' <summary>Wählt die aktuelle Auswahl für eine Zeichenformatabfrage.</summary>
        Friend Const SCF_SELECTION As Int32 = 1

        ''' <summary>Kennzeichnet einen einheitlichen linken Absatzeinzug.</summary>
        Friend Const PFM_STARTINDENT As Int32 = 1

        ''' <summary>Kennzeichnet eine einheitliche Schriftgröße.</summary>
        Friend Const CFM_SIZE As Int32 = &H80000000

        ''' <summary>Kennzeichnet eine einheitliche Schriftfamilie.</summary>
        Friend Const CFM_FACE As Int32 = &H20000000

        ''' <summary>Kennzeichnet einen einheitlichen Zeichensatz.</summary>
        Friend Const CFM_CHARSET As Int32 = &H8000000

        ''' <summary>Kennzeichnet die vier von FontStyle unterstützten Stilattribute.</summary>
        Friend Const CFM_FONT_STYLES As Int32 = &HF

        ''' <summary>Relevante Felder der nativen Unicode-CHARFORMAT-Struktur.</summary>
        <StructLayout(LayoutKind.Explicit, Size:=92)>
        Friend Structure CharacterFormat

            ''' <summary>Größe der nativen Struktur in Byte.</summary>
            <FieldOffset(0)> Public Size As Int32

            ''' <summary>Kennzeichnet die in der Auswahl einheitlichen Attribute.</summary>
            <FieldOffset(4)> Public Mask As Int32

            ''' <summary>Enthält die gesetzten Zeichenattribute.</summary>
            <FieldOffset(8)> Public Effects As Int32

            ''' <summary>Enthält die Schriftgröße in Twips.</summary>
            <FieldOffset(12)> Public Height As Int32

        End Structure

        ''' <summary>Relevante Felder der nativen PARAFORMAT-Struktur.</summary>
        <StructLayout(LayoutKind.Explicit, Size:=156)>
        Friend Structure ParagraphFormat

            ''' <summary>Größe der nativen Struktur in Byte.</summary>
            <FieldOffset(0)> Public Size As Int32

            ''' <summary>Kennzeichnet die in der Auswahl einheitlichen Absatzattribute.</summary>
            <FieldOffset(4)> Public Mask As Int32

        End Structure

        ''' <summary>Fragt eine native Zeichenformatstruktur ab.</summary>
        ''' <param name="hWnd">Handle des Rich-Edit-Steuerelements.</param>
        ''' <param name="msg">Nachricht zur Zeichenformatabfrage.</param>
        ''' <param name="wParam">Bereich der Abfrage.</param>
        ''' <param name="format">Struktur für die abgefragten Attribute.</param>
        ''' <returns>Maske der einheitlichen Attribute.</returns>
        <DllImport("user32.dll", EntryPoint:="SendMessageW")>
        Friend Function GetCharacterFormat(hWnd As IntPtr, msg As Int32, wParam As IntPtr, ByRef format As CharacterFormat) As IntPtr
        End Function

        ''' <summary>Fragt eine native Absatzformatstruktur ab.</summary>
        ''' <param name="hWnd">Handle des Rich-Edit-Steuerelements.</param>
        ''' <param name="msg">Nachricht zur Absatzformatabfrage.</param>
        ''' <param name="wParam">Für diese Abfrage null.</param>
        ''' <param name="format">Struktur für die abgefragten Attribute.</param>
        ''' <returns>Maske der einheitlichen Attribute.</returns>
        <DllImport("user32.dll", EntryPoint:="SendMessageW")>
        Friend Function GetParagraphFormat(hWnd As IntPtr, msg As Int32, wParam As IntPtr, ByRef format As ParagraphFormat) As IntPtr
        End Function

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
