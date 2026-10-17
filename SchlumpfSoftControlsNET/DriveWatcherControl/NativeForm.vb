' --------------------------------------------------------------------------------------------------------
' Datei: NativeForm.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Windows.Forms
Imports System
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

        Private disposedValue As Boolean

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

        ' Die Struktur für OEM.
        ' https://learn.microsoft.com/de-de/windows/win32/api/dbt/ns-dbt-dev_broadcast_oem
        Private Structure DEV_BROADCAST_OEM
            ' Größe der gesamten DEV_BROADCAST_OEM-Struktur in Bytes.
            Public dbco_size As Int32
            ' Gerätetyp der Meldung (z. B. DBT_DEVTYP.OEM).
            Public dbco_devicetype As Int32
            ' Reserviert durch Windows, normalerweise nicht auswerten.
            Public dbco_reserved As Int32
            ' OEM-/Herstellerdefinierter Identifier zur Klassifizierung des gemeldeten Geräts/Ereignisses.
            Public dbco_identifier As Int32
            ' OEM-/Herstellerdefinierter „Supplemental Function“-Wert (zusätzliche Funktions- bzw. Ereignisinformation).
            Public dbco_suppfunc As Int32
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
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event DriveAdded(sender As Object, e As IO.DriveInfo)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein logisches Volume vom Betriebssystem als entfernt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event DriveRemoved(sender As Object, e As IO.DriveInfo)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium in einem bestehenden Wechsel-/CD-/DVD-Laufwerk eingelegt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event MediaInserted(sender As Object, e As IO.DriveInfo)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Medium aus einem bestehenden Wechsel-/CD-/DVD-Laufwerk entfernt wurde.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event MediaRemoved(sender As Object, e As IO.DriveInfo)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als hinzugefügt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event NetworkDriveAdded(sender As Object, e As IO.DriveInfo)

        ''' <summary>
        ''' Wird ausgelöst, wenn ein Netzlaufwerk vom Betriebssystem als entfernt gemeldet wird.
        ''' </summary>
        ''' <param name="sender">Die auslösende Instanz von <see cref="NativeForm"/>.</param>
        ''' <param name="e">Die Laufwerksinformationen aus <see cref="IO.DriveInfo"/>.</param>
        Public Event NetworkDriveRemoved(sender As Object, e As IO.DriveInfo)

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
            If Not Me.disposedValue Then
                If disposing Then
                    ' Das von CreateHandle erzeugte Fensterhandle freigeben.
                    ' Danach werden keine weiteren Nachrichten mehr empfangen.
                    Me.DestroyHandle()
                End If

                Me.disposedValue = True
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

            ' lParam enthält bei WM_DEVICECHANGE einen Zeiger auf eine
            ' strukturierte Geräteinformation (beginnend mit DEV_BROADCAST_HDR).
            Dim objHeader As Object = m.GetLParam(header.GetType)
            If Not VisualBasic.IsNothing(objHeader) Then
                ' Nur nach Gerätetyp verzweigen; die eigentlichen Daten werden
                ' in den spezialisierten Handlern gelesen.
                Select Case header.dbch_devicetype
                    Case DBT_DEVTYP.OEM
                        Me.HandleOEM(m)
                    Case DBT_DEVTYP.DEVNODE
                    Case DBT_DEVTYP.VOLUME
                        Me.HandleVolume(m)
                    Case DBT_DEVTYP.PORT
                    Case DBT_DEVTYP.NET
                    Case DBT_DEVTYP.DEVICEINTERFACE
                    Case DBT_DEVTYP.HANDLE
                        ' Diese Typen sind für die Laufwerksüberwachung hier nicht relevant.
                End Select
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

                ' Aus der Bitmaske den ersten passenden Laufwerksbuchstaben ableiten
                ' und in eine DriveInfo-Instanz überführen.
                Dim di As New IO.DriveInfo(Me.DriveFromMask(volume.dbcv_unitmask))

                ' wParam enthält den konkreten Änderungstyp (Ankunft/Entfernen).
                ' dbcv_flags enthält Zusatzkontext zur Art des Volumes.
                ' Aktuell wird dieses Flag nicht zur Filterung verwendet, kann aber
                ' bei Bedarf für differenziertere Ereignislogik genutzt werden.
                Dim isMediaEvent As Boolean = (volume.dbcv_flags And DBTF_MEDIA) = DBTF_MEDIA
                Dim isNetworkVolume As Boolean = (volume.dbcv_flags And DBTF_NET) = DBTF_NET

                ' Aktuell erfolgt keine abweichende Behandlung; die Auswertung dient
                ' nur als vorbereitete Stelle für spätere, feinere Filterlogik.
                If isMediaEvent OrElse isNetworkVolume Then
                End If
                Select Case CInt(m.WParam)
                    Case DBT_DEVICEARRIVAL
                        ' Internes Ereignis „Laufwerk hinzugefügt" weitergeben.
                        RaiseEvent DriveAdded(Me, di)

                        ' Bei gesetztem DBTF_MEDIA handelt es sich um einen Medienwechsel
                        ' (z. B. CD eingelegt, SD-Karte eingesetzt) im vorhandenen Laufwerk.
                        If isMediaEvent AndAlso Not isNetworkVolume Then
                            RaiseEvent MediaInserted(Me, di)
                        End If

                        ' Bei gesetztem DBTF_NET handelt es sich um ein Netzlaufwerk.
                        If isNetworkVolume Then
                            RaiseEvent NetworkDriveAdded(Me, di)
                        End If
                    Case DBT_DEVICEREMOVECOMPLETE
                        ' Internes Ereignis „Laufwerk entfernt" weitergeben.
                        RaiseEvent DriveRemoved(Me, di)

                        ' Bei gesetztem DBTF_MEDIA handelt es sich um einen Medienwechsel
                        ' (z. B. CD entnommen, SD-Karte entfernt) im vorhandenen Laufwerk.
                        If isMediaEvent AndAlso Not isNetworkVolume Then
                            RaiseEvent MediaRemoved(Me, di)
                        End If

                        ' Bei gesetztem DBTF_NET handelt es sich um ein Netzlaufwerk.
                        If isNetworkVolume Then
                            RaiseEvent NetworkDriveRemoved(Me, di)
                        End If
                End Select
            End If
        End Sub

        ''' <summary>
        ''' Verarbeitet OEM-Broadcasts und leitet nur Volume-relevante Meldungen weiter.
        ''' </summary>
        ''' <param name="m">Die empfangene Windows-Nachricht.</param>
        Private Sub HandleOEM(ByRef m As Message)
            Dim oem As DEV_BROADCAST_OEM

            ' Manche Treiber senden OEM-Broadcasts. Nur wenn darin als Gerätetyp
            ' wiederum ein Volume steckt, behandeln wir die Nachricht wie ein Volume-Event.
            Dim objOem As Object = m.GetLParam(oem.GetType)
            If Not VisualBasic.IsNothing(objOem) Then
                oem = DirectCast(objOem, DEV_BROADCAST_OEM)
                If oem.dbco_devicetype = DBT_DEVTYP.VOLUME Then Me.HandleVolume(m)
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
            If m.Msg = WM_DEVICECHANGE Then Me.HandleHeader(m)

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
