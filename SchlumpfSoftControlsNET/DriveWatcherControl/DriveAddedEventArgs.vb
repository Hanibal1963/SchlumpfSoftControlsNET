' --------------------------------------------------------------------------------------------------------
' Datei: DriveAddedEventArgs.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace DriveWatcherControl

    ''' <summary>
    ''' Stellt Ereignisdaten für ein hinzugefügtes Laufwerk bereit.
    ''' </summary>
    ''' <remarks>
    ''' Diese Argumente werden typischerweise mit einem Ereignis wie <c>DriveWatcher.DriveAdded</c>
    ''' übergeben und enthalten<br/>
    ''' grundlegende Metadaten des erkannten Laufwerks<br/>
    ''' (z. B. Name, Volumebezeichnung, Dateisystem, Typ und Speichergrößen in Bytes).
    ''' </remarks>
    Public Class DriveAddedEventArgs : Inherits EventArgs

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Ruft den Namen eines Laufwerks ab oder legt ihn fest, z. B. <c>C:\</c>.
        ''' </summary>
        ''' <remarks>
        ''' Der Wert entspricht typischerweise <see cref="IO.DriveInfo.Name"/>.
        ''' </remarks>
        Public Property DriveName As String

        ''' <summary>
        ''' Ruft die Volumebezeichnung eines Laufwerks ab oder legt diese fest.
        ''' </summary>
        ''' <remarks>
        ''' Entspricht der vom Betriebssystem gemeldeten Bezeichnung (z. B. „System“,
        ''' „USB_STICK“).
        ''' </remarks>
        Public Property VolumeLabel As String

        ''' <summary>
        ''' Ruft die Gesamtmenge an verfügbarem freiem Speicherplatz in Bytes ab<br/>
        ''' oder legt sie fest, die für den aktuellen Benutzer auf dem Laufwerk verfügbar
        ''' ist.
        ''' </summary>
        ''' <remarks>
        ''' Dieser Wert kann von <see cref="TotalFreeSpace"/> abweichen, wenn Kontingente
        ''' gelten.
        ''' </remarks>
        Public Property AvailableFreeSpace As Int64

        ''' <summary>
        ''' Ruft die Gesamtmenge an freiem Speicherplatz in Bytes ab<br/>
        ''' oder legt sie fest, die auf dem Laufwerk verfügbar ist (unabhängig von
        ''' Benutzerkontingenten).
        ''' </summary>
        Public Property TotalFreeSpace As Int64

        ''' <summary>
        ''' Ruft die Gesamtgröße des Speicherplatzes in Bytes auf einem Laufwerk ab<br/>
        ''' oder legt sie fest.
        ''' </summary>
        Public Property TotalSize As Int64

        ''' <summary>
        ''' Ruft den Namen des Dateisystems ab oder legt ihn fest, z. B. <c>NTFS</c> oder <c>FAT32</c>.
        ''' </summary>
        Public Property DriveFormat As String

        ''' <summary>
        ''' Ruft den Laufwerkstyp ab oder legt ihn fest,<br/>
        ''' z. B. <see cref="IO.DriveType.CDRom"/>, <see
        ''' cref="IO.DriveType.Removable"/>, <see
        ''' cref="IO.DriveType.Network"/> oder <see
        ''' cref="IO.DriveType.Fixed"/>.
        ''' </summary>
        Public Property DriveType As IO.DriveType

        ''' <summary>
        ''' Ruft einen Wert ab oder legt diesen fest, der angibt, ob ein Laufwerk bereit
        ''' ist<br/>
        ''' (Medien vorhanden, lesbar).
        ''' </summary>
        Public Property IsReady As Boolean

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine leere Instanz von <see cref="DriveAddedEventArgs"/>.
        ''' </summary>
        Public Sub New()

            MyBase.New

        End Sub

        ''' <summary>
        ''' Initialisiert eine Instanz mit Laufwerksnamen.
        ''' </summary>
        ''' <param name="DriveName">Der Name des Laufwerks, z. B. <c>E:\</c>.</param>
        Public Sub New(DriveName As String)

            MyClass.New
            Me.DriveName = DriveName

        End Sub

        ''' <summary>
        ''' Initialisiert eine Instanz mit Laufwerksnamen und Laufwerkstyp.
        ''' </summary>
        ''' <param name="DriveName">Der Name des Laufwerks, z. B. <c>E:\</c>.</param>
        ''' <param name="DriveType">Der erkannte Typ des Laufwerks.</param>
        Public Sub New(DriveName As String, DriveType As IO.DriveType)

            MyClass.New
            Me.DriveName = DriveName
            Me.DriveType = DriveType

        End Sub

        ''' <summary>
        ''' Initialisiert eine Instanz mit Laufwerksnamen, Typ und Bereitschaftsstatus.
        ''' </summary>
        ''' <param name="DriveName">Der Name des Laufwerks, z. B. <c>E:\</c>.</param>
        ''' <param name="DriveType">Der erkannte Typ des Laufwerks.</param>
        ''' <param name="IsReady">Gibt an, ob das Laufwerk direkt lesbar ist.</param>
        Public Sub New(DriveName As String, DriveType As IO.DriveType, IsReady As Boolean)

            MyClass.New
            Me.DriveName = DriveName
            Me.DriveType = DriveType
            Me.IsReady = IsReady

        End Sub

#End Region

#Region "überschriebene Methoden"

        ''' <summary>
        ''' Finalizer der Basisklasse; keine zusätzlichen Ressourcen in dieser Klasse.
        ''' </summary>
        Protected Overrides Sub Finalize()

            MyBase.Finalize()

        End Sub

#End Region

    End Class

End Namespace
