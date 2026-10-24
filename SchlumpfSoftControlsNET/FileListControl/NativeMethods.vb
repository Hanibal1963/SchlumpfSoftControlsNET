' --------------------------------------------------------------------------------------------------------
' Datei: NativeMethods.vb
' Author: Andreas Sauer
' Datum: 30.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Runtime.InteropServices

Namespace FileListControl

    ''' <summary>
    ''' Kapselt die benötigten nativen Win32-/Shell-Interop-Aufrufe für das Ermitteln und Verwalten von Dateisymbolen
    ''' (Icons).
    ''' </summary>
    Friend NotInheritable Class NativeMethods

        ' Utility-Klasse: keine Instanzierung vorgesehen.
        Private Sub New()
        End Sub

#Region "Konstanten"

        ' SHGetFileInfo-Flag: Icon-Handle im Rückgabestrukturfeld hIcon bereitstellen.
        Public Const SHGFI_ICON As UInt32 = &H100

        ' SHGetFileInfo-Flag: Kleines Icon (typisch 16x16) anfordern.
        Public Const SHGFI_SMALLICON As UInt32 = &H1

        ' SHGetFileInfo-Flag: Dateiattribute verwenden, auch wenn die Datei nicht existiert.
        Public Const SHGFI_USEFILEATTRIBUTES As UInt32 = &H10

        ' Win32-Dateiattribut: Verzeichnis.
        Public Const FILE_ATTRIBUTE_DIRECTORY As UInt32 = &H10

        ' Win32-Dateiattribut: Normale Datei.
        Public Const FILE_ATTRIBUTE_NORMAL As UInt32 = &H80

#End Region

#Region "Strukturen"

        ''' <summary>
        ''' Entspricht der nativen <c>SHFILEINFO</c>-Struktur aus der Windows-Shell-API. Enthält u. a. Icon-Handle sowie
        ''' Anzeige- und Typinformationen.
        ''' </summary>
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
        Public Structure SHFILEINFO

            ''' <summary>
            ''' Handle auf das gelieferte Icon. Muss nach Verwendung freigegeben werden.
            ''' </summary>
            Public hIcon As IntPtr

            ''' <summary>
            ''' Interner Icon-Index in der System-Image-Liste.
            ''' </summary>
            Public iIcon As Int32

            ''' <summary>
            ''' Dateiattribute entsprechend der Shell-Abfrage.
            ''' </summary>
            Public dwAttributes As UInt32

            ''' <summary>
            ''' Anzeigename (z. B. Dateiname) aus der Shell.
            ''' </summary>
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)>
            Public szDisplayName As String

            ''' <summary>
            ''' Typbezeichnung (z. B. "Textdokument") aus der Shell.
            ''' </summary>
            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)>
            Public szTypeName As String

        End Structure

#End Region

#Region "API-Funktionen"

        ''' <summary>
        ''' Gibt ein Icon-Handle frei, das zuvor von der Windows-API bereitgestellt wurde.
        ''' </summary>
        ''' <param name="hIcon">Das freizugebende Icon-Handle.</param>
        ''' <returns><c>True</c>, wenn das Handle erfolgreich zerstört wurde; andernfalls <c>False</c>.</returns>
        <DllImport("user32.dll", SetLastError:=True)>
        Public Shared Function DestroyIcon(hIcon As IntPtr) As Boolean
        End Function

        ''' <summary>
        ''' Ruft Datei-/Ordnerinformationen (insbesondere Icon-Daten) über die Shell-API ab.
        ''' </summary>
        ''' <param name="pszPath">Pfad, Dateiname oder Platzhalter (z. B. <c>*.txt</c>) des Zielobjekts.</param>
        ''' <param name="dwFileAttributes">
        ''' Dateiattribute (z. B. Verzeichnis oder normale Datei), relevant bei <c>SHGFI_USEFILEATTRIBUTES</c>.
        ''' </param>
        ''' <param name="psfi">
        ''' Ausgabestruktur mit den ermittelten Informationen, inklusive optionalem Icon-Handle.
        ''' </param>
        ''' <param name="cbFileInfo">Größe der Struktur <paramref name="psfi"/> in Bytes.</param>
        ''' <param name="uFlags">Kombination aus SHGFI-Flags zur Steuerung der Abfrage.</param>
        ''' <returns>
        ''' Ein von der API zurückgegebener Ergebniswert. Bei Erfolg ungleich <see cref="IntPtr.Zero"/>, bei Fehler
        ''' gleich <see cref="IntPtr.Zero"/>.
        ''' </returns>
        <DllImport("shell32.dll", CharSet:=CharSet.Unicode)>
        Public Shared Function SHGetFileInfo(pszPath As String, dwFileAttributes As UInt32, ByRef psfi As SHFILEINFO, cbFileInfo As UInt32, uFlags As UInt32) As IntPtr
        End Function

#End Region

    End Class

End Namespace

