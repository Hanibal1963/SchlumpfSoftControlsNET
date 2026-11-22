' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBarThresholdCollection.vb
' Author: Andreas Sauer
' Datum: 22.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.ObjectModel

Namespace ColorProgressBarControl

    ''' <summary>Verwaltet die geordneten Schwellenwertbereiche eines Fortschrittsbalkens.</summary>
    Public NotInheritable Class ColorProgressBarThresholdCollection

        Inherits Collection(Of ColorProgressBarThreshold)

        ''' <summary>Benachrichtigt das zugehörige Control nach Änderungen.</summary>
        Private ReadOnly _changed As Action

        ''' <summary>Erstellt die Sammlung für das zugehörige Control.</summary>
        ''' <param name="changed">Benachrichtigung nach einer Änderung.</param>
        Friend Sub New(changed As Action)
            Me._changed = changed
        End Sub

        ''' <summary>Fügt einen nicht leeren Bereich ein.</summary>
        ''' <param name="index">Position des neuen Bereichs.</param>
        ''' <param name="item">Einzufügender Bereich.</param>
        ''' <exception cref="ArgumentNullException">Der Bereich ist Nothing.</exception>
        Protected Overrides Sub InsertItem(index As Integer, item As ColorProgressBarThreshold)
#If NETFRAMEWORK Then
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
#Else
            ArgumentNullException.ThrowIfNull(item)
#End If
            MyBase.InsertItem(index, item)
            Me._changed()
        End Sub

        ''' <summary>Ersetzt einen Bereich und aktualisiert das Control.</summary>
        ''' <param name="index">Position des zu ersetzenden Bereichs.</param>
        ''' <param name="item">Neuer Bereich.</param>
        ''' <exception cref="ArgumentNullException">Der Bereich ist Nothing.</exception>
        Protected Overrides Sub SetItem(index As Integer, item As ColorProgressBarThreshold)
#If NETFRAMEWORK Then
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
#Else
            ArgumentNullException.ThrowIfNull(item)
#End If
            MyBase.SetItem(index, item)
            Me._changed()
        End Sub

        ''' <summary>Entfernt einen Bereich und aktualisiert das Control.</summary>
        ''' <param name="index">Position des zu entfernenden Bereichs.</param>
        Protected Overrides Sub RemoveItem(index As Integer)
            MyBase.RemoveItem(index)
            Me._changed()
        End Sub

        ''' <summary>Entfernt alle Bereiche und aktualisiert das Control.</summary>
        Protected Overrides Sub ClearItems()
            If Me.Count = 0 Then Return
            MyBase.ClearItems()
            Me._changed()
        End Sub

    End Class

End Namespace