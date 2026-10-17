' --------------------------------------------------------------------------------------------------------
' Datei: DriveRemovedEventArgs.vb
' Author: Andreas Sauer
' Datum: 28.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Namespace DriveWatcherControl

    ''' <summary>
    ''' Stellt Ereignisdaten für ein entferntes Laufwerk bereit.
    ''' </summary>
    ''' <remarks>
    ''' Diese Argumente werden typischerweise mit einem Ereignis wie <c>DriveWatcher.DriveRemoved</c> übergeben<br/>
    ''' und enthalten mindestens den Namen des entfernten Laufwerks.
    ''' </remarks>
    Public Class DriveRemovedEventArgs : Inherits EventArgs

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Ruft den Namen des Laufwerks ab oder legt ihn fest, z. B. <c>C:\</c>.
        ''' </summary>
        Public Property DriveName As String

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine leere Instanz von <see cref="DriveRemovedEventArgs"/>.
        ''' </summary>
        Public Sub New()

            MyBase.New

        End Sub

        ''' <summary>
        ''' Initialisiert eine Instanz mit Laufwerksnamen.
        ''' </summary>
        ''' <param name="DriveName">Der Name des entfernten Laufwerks, z. B. <c>E:\</c>.</param>
        Public Sub New(DriveName As String)

            MyClass.New
            Me.DriveName = DriveName

        End Sub

#End Region

#Region "Überschriebene Methoden"

        ''' <summary>
        ''' Finalizer der Basisklasse; keine zusätzlichen Ressourcen in dieser Klasse.
        ''' </summary>
        Protected Overrides Sub Finalize()

            MyBase.Finalize()

        End Sub

#End Region

    End Class

End Namespace
