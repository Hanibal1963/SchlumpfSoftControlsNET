' --------------------------------------------------------------------------------------------------------
' Datei: DriveWatcherRawDeviceEventArgs.vb
' Author: Andreas Sauer
' Datum: 26.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace DriveWatcherControl

    ''' <summary>Stellt eine unveränderliche Kopie der verfügbaren Daten einer nativen Geräteänderung bereit.</summary>
    ''' <remarks>Ein nicht verfügbarer Strukturwert ist <see langword="Nothing"/>; native Zeiger werden nicht gespeichert.</remarks>
    Public NotInheritable Class DriveWatcherRawDeviceEventArgs

        Inherits EventArgs

        ''' <summary>Erstellt Ereignisdaten aus der synchron empfangenen Windows-Nachricht.</summary>
        ''' <param name="eventCode">Unveränderter numerischer Wert von wParam.</param>
        ''' <param name="deviceType">Gerätetyp aus einem gültigen Header oder Nothing.</param>
        ''' <param name="unitMask">Unveränderte Volume-Bitmaske oder Nothing.</param>
        ''' <param name="flags">Unveränderte Volume-Flags oder Nothing.</param>
        ''' <param name="timestampUtc">Zeitpunkt des Nachrichtenempfangs in UTC.</param>
        Friend Sub New(eventCode As Int64, deviceType As Int32?, unitMask As Int32?, flags As Int16?, timestampUtc As DateTimeOffset)
            Me.EventCode = eventCode
            Me.DeviceType = deviceType
            Me.UnitMask = unitMask
            Me.Flags = flags
            Me.TimestampUtc = timestampUtc
        End Sub

        ''' <summary>Ruft den numerischen Windows-Geräteereigniscode aus wParam ab.</summary>
        Public ReadOnly Property EventCode As Int64

        ''' <summary>Ruft den nativen Gerätetyp ab, sofern ein gültiger Header ausgewertet wurde.</summary>
        Public ReadOnly Property DeviceType As Int32?

        ''' <summary>Ruft die unveränderte 32-Bit-Volume-Laufwerksmaske ab, sofern verfügbar.</summary>
        Public ReadOnly Property UnitMask As Int32?

        ''' <summary>Ruft die unveränderten 16-Bit-Volume-Flags ab, sofern verfügbar.</summary>
        Public ReadOnly Property Flags As Int16?

        ''' <summary>Ruft den UTC-Zeitpunkt des Nachrichtenempfangs ab.</summary>
        Public ReadOnly Property TimestampUtc As DateTimeOffset

    End Class

End Namespace