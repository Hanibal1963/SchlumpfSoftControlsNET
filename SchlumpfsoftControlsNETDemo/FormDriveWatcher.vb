' --------------------------------------------------------------------------------------------------------
' Datei: FormDriveWatcher.vb
' Author: Andreas Sauer
' Datum: 09.09.2026
' --------------------------------------------------------------------------------------------------------

Imports SchlumpfSoft.ControlsNET
Imports SchlumpfSoft.ControlsNET.DriveWatcherControl

Public Class FormDriveWatcher

    Public Sub New()
        Me.InitializeComponent()
        Me.Label_Info.Text = My.Resources.DriveWatcherInfoText
    End Sub

    Private Sub DriveWatcher_DriveAdded(sender As Object, e As DriveAddedEventArgs) Handles DriveWatcher.DriveAdded, DriveWatcher.NetworkDriveAdded
        Dim msg As String
        Dim name = e.DriveName
        Dim type = e.DriveType
        If e.IsReady Then
            Dim format = e.DriveFormat
            Dim label = e.VolumeLabel
            Dim totalSize = e.TotalSize
            Dim freeSpace = e.TotalFreeSpace
            Dim availableFreeSpace = e.AvailableFreeSpace
            msg = $"Das Laufwerk {name} vom Typ {type} wurde hinzugefügt und ist bereit." & Environment.NewLine &
                       $"Es ist {format} formatiert und hat die Bezeichnung {label}." & Environment.NewLine &
                        $"Die Gesamtkapazität beträgt {totalSize} Bytes und {freeSpace} Bytes Gesamtkapazität sind noch frei." & Environment.NewLine &
                        $"Für den aktuellen Benutzer sind noch {availableFreeSpace} Bytes freier Speicher verfügbar."
        Else
            msg = $"Das Laufwerk {name} vom Typ {type} wurde hinzugefügt, ist aber nicht bereit."
        End If
        Me.TextBox_DriveInfo.Text = msg
    End Sub

    Private Sub DriveWatcher_DriveRemoved(sender As Object, e As DriveRemovedEventArgs) Handles DriveWatcher.DriveRemoved, DriveWatcher.NetworkDriveRemoved
        Dim name = e.DriveName
        Dim msg As String = $"Das Laufwerk {name} wurde entfernt."
        Me.TextBox_DriveInfo.Text = msg
    End Sub

    Private Sub DriveWatcher_MediaInserted(sender As Object, e As DriveAddedEventArgs) Handles DriveWatcher.MediaInserted
        Dim name As String = e.DriveName
        Dim msg As String = $"In Laufwerk {name} wurde ein Medium eingelegt."
        Me.TextBox_DriveInfo.Text &= vbCrLf & msg
    End Sub

    Private Sub DriveWatcher_MediaRemoved(sender As Object, e As DriveRemovedEventArgs) Handles DriveWatcher.MediaRemoved
        Dim name As String = e.DriveName
        Dim msg As String = $"Aus Laufwerk {name} wurde ein Medium entfernt."
        Me.TextBox_DriveInfo.Text &= vbCrLf & msg
    End Sub

End Class