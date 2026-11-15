' --------------------------------------------------------------------------------------------------------
' Datei: NativeForm.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Windows.Forms
Imports System
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports Microsoft

Namespace DriveWatcherControl

    ''' <summary>
    ''' Repräsentiert ein unsichtbares natives Fenster zum Empfangen von Windows-Nachrichten für Geräteänderungen.
    ''' </summary>
    ''' <remarks>
    ''' Die Klasse verarbeitet <c> WM_DEVICECHANGE</c>, wertet relevante Broadcast-Strukturen aus und löst bei
    ''' Volume-Änderungen die Ereignisse <see cref="DriveAdded"/> und <see cref="DriveRemoved"/> aus.
    ''' Für Medienwechsel in Wechsel-/CD-/DVD-Laufwerken stehen zusätzlich
    ''' <see cref="MediaInserted"/> und <see cref="MediaRemoved"/> zur Verfügung.
    ''' Für Netzlaufwerke stehen zusätzlich <see cref="NetworkDriveAdded"/> und
    ''' <see cref="NetworkDriveRemoved"/> zur Verfügung.
    ''' </remarks>
#Disable Warning CA1852 ' Warnung: Interne Typen versiegeln
    Friend Class NativeForm
#Enable Warning CA1852

        Inherits NativeWindow
        Implements IDisposable

#Region "Variablen"

        Private _disposedValue As Boolean

#End Region

#Region " Konstanten"

        ' Windows-Nachricht für Geräteänderungen.
        ' Diese Nachricht wird vom Betriebssystem gesendet, wenn z. B. USB-Medien
        ' eingesteckt oder entfernt werden.
        Private Const WM_DEVICECHANGE As Int32 = &H219

        ' Ereigniscodes aus wParam von WM_DEVICECHANGE.
        ' Sie geben an, ob ein Gerät angekommen ist oder entfernt wurde.
        Private Const DBT_DEVICEARRIVAL As Int32 = &H8000
        Private Const DBT_DEVICEREMOVECOMPLETE As Int32 = &H8004

        ' Flag-Bits in DEV_BROADCAST_VOLUME.dbcv_flags.
        ' DBTF_MEDIA: Wechselmedium wurde eingelegt/entfernt (z. B. CD/DVD/SD-Karte).
        ' DBTF_NET: Das betroffene Laufwerk ist ein Netzlaufwerk.
        Private Const DBTF_MEDIA As Int16 = &H1
        Private Const DBTF_NET As Int16 = &H2

#End Region

#Region "Interne Auflistungen"

        ' Gerätetypen aus den DEV_BROADCAST_* Strukturen.
        ' In den Broadcast-Strukturen werden diese Werte als Integer geliefert.
        ' Für diese Klasse ist primär DBT_DEVTYP.VOLUME relevant.
        Private Enum DBT_DEVTYP
            OEM = 0 ' OEM-/IHV-definierte Geräteklasse (herstellerspezifische Meldungen)
            DEVNODE = 1 ' Geräteknoten (Device Node) im Plug-and-Play-Baum
            VOLUME = 2 ' Logisches Volume/Laufwerk (für diese Komponente relevant)
            PORT = 3 ' Portgerät (z. B. seriell oder parallel)
            NET = 4 ' Netzwerkressource
            DEVICEINTERFACE = 5 ' Geräteschnittstellenklasse (Interface-basiert)
            HANDLE = 6 ' Dateisystem-Handle
        End Enum

#End Region

#Region "Interne Strukturen"

        ' Die Struktur für den Header.
        ' https://learn.microsoft.com/de-de/windows/win32/api/dbt/ns-dbt-dev_broadcast_hdr
        Private Structure DEV_BROADCAST_HDR
            ' Gesamtgröße der Struktur in Bytes.
            Public dbch_size As Int32
            ' Gerätetyp der Meldung (Wert aus DBT_DEVTYP).
            Public dbch_devicetype As Int32
            ' Von Windows reserviert; nicht auswerten.
            Public dbch_reserved As Int32
        End Structure

        ' Die Struktur für Volumes.
        ' https://learn.microsoft.com/de-de/windows/win32/api/dbt/ns-dbt-dev_broadcast_volume
        Private Structure DEV_BROADCAST_VOLUME
            ' Gesamtgröße der Struktur in Bytes.
            Public dbch_size As Int32
            ' Gerätetyp der Meldung (für Volumes: DBT_DEVTYP.VOLUME).
            Public dbch_devicetype As Int32
            ' Von Windows reserviert; nicht auswerten.
            Public dbch_reserved As Int32
            ' Bitmaske der betroffenen Laufwerksbuchstaben (Bit 0=A ... Bit 25=Z).
            Public dbcv_unitmask As Int32
            ' Zusatzinformationen zur Meldung (Bitmaske):
            ' - DBTF_MEDIA (&H1): Medienwechsel
            ' - DBTF_NET (&H2): Netzlaufwerk
            Public dbcv_flags As Int16
        End Structure

#End Region

#Region "Öffentliche Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Volume vom Betriebssystem als hinzugefügt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event DriveAdded(sender As Object, e As String)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Volume vom Betriebssystem als entfernt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event DriveRemoved(sender As Object, e As String)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event MediaInserted(sender As Object, e As String)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event MediaRemoved(sender As Object, e As String)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als hinzugefügt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event NetworkDriveAdded(sender As Object, e As String)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als entfernt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event NetworkDriveRemoved(sender As Object, e As String)

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="NativeForm"/> und erstellt ein eigenes Fensterhandle.
        ''' </summary>
        Public Sub New()
            ' NativeWindow besitzt anfangs kein eigenes Fensterhandle.
            ' Durch CreateHandle erzeugen wir ein unsichtbares Fenster,
            ' an das Windows Nachrichten zustellen kann.
            Me.CreateHandle(New CreateParams)
        End Sub

        ''' <summary>
        ''' Erstellt das Empfangsfenster, solange die Instanz nicht freigegeben wurde.
        ''' </summary>
        ''' <param name="cp">Die Parameter für das native Fenster.</param>
        ''' <exception cref="ObjectDisposedException">Die Instanz wurde bereits freigegeben.</exception>
        ''' <exception cref="InvalidOperationException">Für diese Instanz existiert bereits ein Fensterhandle.</exception>
        Public Overrides Sub CreateHandle(cp As CreateParams)
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
            If Me._disposedValue Then Throw New ObjectDisposedException(NameOf(NativeForm))
#Enable Warning CA1513
            MyBase.CreateHandle(cp)
        End Sub

        ''' <summary>
        ''' Gibt Ressourcen der Instanz frei.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Implementiert das eigentliche Dispose-Muster.
        ''' </summary>
        ''' <param name="disposing">
        ''' <see langword="True"/>, wenn verwaltete Ressourcen freigegeben werden sollen; andernfalls
        ''' <see langword="False"/>.
        ''' </param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me._disposedValue Then
                If disposing Then
                    ' Das von CreateHandle erzeugte Fensterhandle freigeben.
                    ' Danach werden keine weiteren Nachrichten mehr empfangen.
                    Me.DestroyHandle()
                End If

                Me._disposedValue = True
            End If
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Liest den Broadcast-Header aus der Nachricht und delegiert an den passenden Handler.
        ''' </summary>
        ''' <param name="m">Die empfangene Windows-Nachricht.</param>
        Private Sub HandleHeader(ByRef m As Message)
            Dim header As DEV_BROADCAST_HDR
            If m.LParam = IntPtr.Zero Then
                Trace.TraceWarning("WM_DEVICECHANGE enthält keinen Geräteheader.")
                Return
            End If
            If Marshal.ReadInt32(m.LParam) < Marshal.SizeOf(Of DEV_BROADCAST_HDR)() Then
                Trace.TraceWarning("WM_DEVICECHANGE enthält einen zu kurzen Geräteheader.")
                Return
            End If
            header = DirectCast(m.GetLParam(header.GetType), DEV_BROADCAST_HDR)
            If header.dbch_devicetype = DBT_DEVTYP.VOLUME Then
                If header.dbch_size < Marshal.SizeOf(Of DEV_BROADCAST_VOLUME)() Then
                    Trace.TraceWarning("WM_DEVICECHANGE enthält eine zu kurze Volume-Struktur.")
                    Return
                End If
                Me.HandleVolume(m)
            End If
        End Sub

        ''' <summary>
        ''' Verarbeitet Volume-Benachrichtigungen und löst die entsprechenden .NET-Ereignisse aus.
        ''' </summary>
        ''' <param name="m">Die empfangene Windows-Nachricht.</param>
        Private Sub HandleVolume(ByRef m As Message)
            Dim volume As DEV_BROADCAST_VOLUME

            ' lParam als Volume-Struktur auslesen. Darin liegt u. a. die Unit-Maske,
            ' aus der der Laufwerksbuchstabe berechnet wird.
            Dim objVolume As Object = m.GetLParam(volume.GetType)
            If Not VisualBasic.IsNothing(objVolume) Then
                volume = DirectCast(objVolume, DEV_BROADCAST_VOLUME)

                ' Der Name ist bereits bekannt, auch wenn das Laufwerk nicht mehr lesbar ist.
                Dim driveLetter As Char = Me.DriveFromMask(volume.dbcv_unitmask)
                If driveLetter = ControlChars.NullChar Then
                    Trace.TraceWarning("WM_DEVICECHANGE enthält keine gültige Laufwerksmaske.")
                    Return
                End If
                Dim driveName As String = $"{driveLetter}:\"

                ' wParam enthält den konkreten Änderungstyp (Ankunft/Entfernen).
                ' dbcv_flags enthält Zusatzkontext zur Art des Volumes.
                Dim isMediaEvent As Boolean = (volume.dbcv_flags And DBTF_MEDIA) = DBTF_MEDIA
                Dim isNetworkVolume As Boolean = (volume.dbcv_flags And DBTF_NET) = DBTF_NET
                Select Case CInt(m.WParam)
                    Case DBT_DEVICEARRIVAL
                        ' Internes Ereignis „Laufwerk hinzugefügt" weitergeben.
                        RaiseEvent DriveAdded(Me, driveName)

                        ' Bei gesetztem DBTF_MEDIA handelt es sich um einen Medienwechsel
                        ' (z. B. CD eingelegt, SD-Karte eingesetzt) im vorhandenen Laufwerk.
                        If isMediaEvent AndAlso Not isNetworkVolume Then
                            RaiseEvent MediaInserted(Me, driveName)
                        End If

                        ' Bei gesetztem DBTF_NET handelt es sich um ein Netzlaufwerk.
                        If isNetworkVolume Then
                            RaiseEvent NetworkDriveAdded(Me, driveName)
                        End If
                    Case DBT_DEVICEREMOVECOMPLETE
                        ' Internes Ereignis „Laufwerk entfernt" weitergeben.
                        RaiseEvent DriveRemoved(Me, driveName)

                        ' Bei gesetztem DBTF_MEDIA handelt es sich um einen Medienwechsel
                        ' (z. B. CD entnommen, SD-Karte entfernt) im vorhandenen Laufwerk.
                        If isMediaEvent AndAlso Not isNetworkVolume Then
                            RaiseEvent MediaRemoved(Me, driveName)
                        End If

                        ' Bei gesetztem DBTF_NET handelt es sich um ein Netzlaufwerk.
                        If isNetworkVolume Then
                            RaiseEvent NetworkDriveRemoved(Me, driveName)
                        End If
                End Select
            End If
        End Sub

        ''' <summary>
        ''' Ermittelt aus der Unit-Maske den ersten enthaltenen Laufwerksbuchstaben.
        ''' </summary>
        ''' <param name="mask">Bitmaske aus <c>dbcv_unitmask</c>.</param>
        ''' <returns>Den Laufwerksbuchstaben von <c>A</c> bis <c>Z</c>.</returns>
        Private Function DriveFromMask(mask As Int32) As Char

            ' Standardwert für den Fall, dass kein Bit gesetzt ist.
            Dim result As Char = ControlChars.NullChar
            ' Jedes Bit der Maske steht für einen Buchstaben:
            ' Bit 0 = A, Bit 1 = B, ..., Bit 25 = Z.
            For b As Int32 = 0 To 25
                If (mask And CInt(2 ^ b)) <> 0 Then

                    ' ASCII 65 entspricht „A".
                    result = VisualBasic.Chr(65 + b)
                    Exit For
                End If
            Next b
            Return result
        End Function

#End Region

#Region "Überschriebene Methoden"

        ''' <summary>
        ''' Verarbeitet eingehende Fensternachrichten und reagiert auf <c> WM_DEVICECHANGE</c>.
        ''' </summary>
        ''' <param name="m">Die zu verarbeitende Windows-Nachricht.</param>
        Protected Overrides Sub WndProc(ByRef m As Message)

            ' Zentrale Eintrittsstelle für alle Fensternachrichten dieses NativeWindow.
            ' Für dieses Control interessiert ausschließlich WM_DEVICECHANGE.
            If m.Msg = WM_DEVICECHANGE AndAlso
                (m.WParam = New IntPtr(DBT_DEVICEARRIVAL) OrElse m.WParam = New IntPtr(DBT_DEVICEREMOVECOMPLETE)) Then
                Me.HandleHeader(m)
            End If

            ' Nicht verarbeitete Nachrichten an die Basisklasse weiterreichen.
            MyBase.WndProc(m)
        End Sub

        ''' <summary>
        ''' Finalizer als Sicherheitsnetz für den Fall, dass <see cref="Dispose()"/> nicht aufgerufen wurde.
        ''' </summary>
        Protected Overrides Sub Finalize()

            ' Kein managed Cleanup mehr; nur das allgemeine Dispose-Muster abschließen.
            Me.Dispose(disposing:=False)
            MyBase.Finalize()
        End Sub

#End Region

    End Class

End Namespace
