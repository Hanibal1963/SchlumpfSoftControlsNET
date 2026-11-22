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
    Friend NotInheritable Class DriveWatcherNativeForm

        Inherits NativeWindow
        Implements IDisposable

#Region "Variablen"

        ''' <summary>Gibt an, ob die Instanz bereits freigegeben wurde.</summary>
        Private _disposedValue As Boolean

#End Region

#Region " Konstanten"

        ''' <summary>Windows-Nachricht für Geräteänderungen.</summary>
        ''' <remarks>Wird vom Betriebssystem gesendet, wenn z. B. USB-Medien eingesteckt oder entfernt werden.</remarks>
        Private Const WM_DEVICECHANGE As Int32 = &H219

        ''' <summary>Ereigniscode für die Ankunft eines Geräts.</summary>
        ''' <remarks>Sie geben an, ob ein Gerät angekommen ist oder entfernt wurde.</remarks>
        Private Const DBT_DEVICEARRIVAL As Int32 = &H8000

        ''' <summary>Ereigniscode für die abgeschlossene Entfernung eines Geräts.</summary>
        Private Const DBT_DEVICEREMOVECOMPLETE As Int32 = &H8004

        ''' <summary>Kennzeichnet einen Medienwechsel.</summary>
        ''' <remarks>
        ''' Flag-Bits in DEV_BROADCAST_VOLUME.dbcv_flags.
        ''' DBTF_MEDIA: Wechselmedium wurde eingelegt/entfernt (z. B. CD/DVD/SD-Karte).
        ''' DBTF_NET: Das betroffene Laufwerk ist ein Netzlaufwerk.
        ''' </remarks>
        Private Const DBTF_MEDIA As Int16 = &H1

        ''' <summary>Kennzeichnet ein Netzlaufwerk.</summary>
        Private Const DBTF_NET As Int16 = &H2

#End Region

#Region "Interne Auflistungen"

        ''' <summary>Gerätetypen in Windows-Broadcast-Strukturen.</summary>
        ''' <remarks>
        ''' Die Werte werden in den Broadcast-Strukturen als Integer geliefert.
        ''' Für diese Klasse ist primär DBT_DEVTYP.VOLUME relevant.
        ''' </remarks>
        Private Enum DBT_DEVTYP

            ''' <summary>Herstellerspezifische Geräteklasse.</summary>
            OEM = 0

            ''' <summary>Geräteknoten im Plug-and-Play-Baum.</summary>
            DEVNODE = 1

            ''' <summary>Logisches Volume beziehungsweise Laufwerk.</summary>
            VOLUME = 2

            ''' <summary>Portgerät.</summary>
            PORT = 3

            ''' <summary>Netzwerkressource.</summary>
            NET = 4

            ''' <summary>Geräteschnittstellenklasse.</summary>
            DEVICEINTERFACE = 5

            ''' <summary>Dateisystem-Handle.</summary>
            HANDLE = 6

        End Enum

#End Region

#Region "Interne Strukturen"

        ''' <summary>Gemeinsamer Header einer Geräte-Broadcast-Meldung.</summary>
        ''' <remarks>
        ''' Die Struktur wird in <c>WM_DEVICECHANGE</c>-Nachrichten übergeben.
        ''' Sie enthält die Gesamtgröße der Meldung, den Gerätetyp und ein reserviertes Feld
        ''' https://learn.microsoft.com/de-de/windows/win32/api/dbt/ns-dbt-dev_broadcast_hdr
        ''' </remarks>
        Private Structure DEV_BROADCAST_HDR

            ''' <summary>Gesamtgröße der Struktur in Bytes.</summary>
            Public dbch_size As Int32

            ''' <summary>Gerätetyp der Meldung als Wert von <see cref="DBT_DEVTYP"/>.</summary>
            Public dbch_devicetype As Int32

            ''' <summary>Von Windows reserviertes Feld.</summary>
            Public dbch_reserved As Int32

        End Structure

        ''' <summary>Broadcast-Daten für eine Volume-Änderung.</summary>
        ''' <remarks>
        ''' Die Struktur wird in <c>WM_DEVICECHANGE</c>-Nachrichten übergeben. Sie enthält die Gesamtgröße der Meldung,
        ''' den Gerätetyp, ein reserviertes Feld, die Laufwerksmaske und Zusatzinformationen.
        ''' https://learn.microsoft.com/de-de/windows/win32/api/dbt/ns-dbt-dev_broadcast_volume
        ''' </remarks>
        Private Structure DEV_BROADCAST_VOLUME

            ''' <summary>Gesamtgröße der Struktur in Bytes.</summary>
            Public dbch_size As Int32

            ''' <summary>Gerätetyp der Meldung für ein Volume. (für Volumes: DBT_DEVTYP.VOLUME)</summary>
            Public dbch_devicetype As Int32

            ''' <summary>Von Windows reserviertes Feld.</summary>
            Public dbch_reserved As Int32

            ''' <summary>Bitmaske der betroffenen Laufwerksbuchstaben von A bis Z. (Bit 0=A ... Bit 25=Z)</summary>
            Public dbcv_unitmask As Int32

            ''' <summary>Zusatzinformationen zum Medienwechsel oder Netzlaufwerk als Bitmaske.</summary>
            ''' <remarks> Die Werte sind in den Konstanten DBTF_MEDIA  (&amp;H1 - Medienwechsel) und DBTF_NET (&amp;H2 -Netzlaufwerk) definiert. </remarks>
            Public dbcv_flags As Int16

        End Structure

#End Region

#Region "Öffentliche Ereignisse"

        ''' <summary>Wird vor der Verarbeitung einer gültigen Volume-Meldung ausgelöst.</summary>
        ''' <param name="sender">Das Empfangsfenster.</param>
        ''' <param name="eventCode">Der Windows-Ereigniscode.</param>
        ''' <param name="driveName">Der erste gemeldete Laufwerksname.</param>
        ''' <param name="unitMask">Die unveränderte Laufwerksmaske.</param>
        ''' <param name="flags">Die unveränderten Volume-Flags.</param>
        Public Event VolumeObserved(sender As Object, eventCode As Integer, driveName As String, unitMask As Integer, flags As Short)

        ''' <summary>Wird ausgelöst, wenn ein logisches Volume vom Betriebssystem als hinzugefügt gemeldet wird. </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event DriveAdded(sender As Object, e As String)

        ''' <summary>Wird ausgelöst, wenn ein logisches Volume vom Betriebssystem als entfernt gemeldet wird.</summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event DriveRemoved(sender As Object, e As String)

        ''' <summary>Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.</summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event MediaInserted(sender As Object, e As String)

        ''' <summary>Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.</summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event MediaRemoved(sender As Object, e As String)

        ''' <summary>Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als hinzugefügt gemeldet wird.</summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event NetworkDriveAdded(sender As Object, e As String)

        ''' <summary>Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als entfernt gemeldet wird.</summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="DriveWatcherNativeForm"/>.</param>
        ''' <param name="e">Der gemeldete Laufwerksname.</param>
        Public Event NetworkDriveRemoved(sender As Object, e As String)

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz von <see cref="DriveWatcherNativeForm"/> und erstellt ein eigenes Fensterhandle.
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
#If NET8_0_OR_GREATER Then
            ObjectDisposedException.ThrowIf(Me._disposedValue, Me)
#Else
            If Me._disposedValue Then Throw New ObjectDisposedException(NameOf(DriveWatcherNativeForm))
#End If
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
        Protected Sub Dispose(disposing As Boolean)
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

            ' Vor dem Auslesen den Zeiger und die gemeldete Mindestgröße prüfen.
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

                ' Die Volume-Struktur benötigt mehr Platz als der gemeinsame Header.
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
                RaiseEvent VolumeObserved(Me, CInt(m.WParam), driveName, volume.dbcv_unitmask, volume.dbcv_flags)

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
		''' <remarks>Wenn kein Laufwerksbuchstabe gesetzt ist, wird das Nullzeichen zurückgegeben.</remarks>
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
