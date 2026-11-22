' --------------------------------------------------------------------------------------------------------
' Datei: AniGifDesigner.vb
' Author: Andreas Sauer
' Datum: 20.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel
#If NETFRAMEWORK Then
Imports System.ComponentModel.Design
Imports System.Windows.Forms.Design
#Else
Imports Microsoft.DotNet.DesignTools.Designers
Imports Microsoft.DotNet.DesignTools.Designers.Actions
#End If

Namespace AniGifControl

    <Designer("SchlumpfSoft.ControlsNET.AniGifControl.AniGifDesigner, SchlumpfSoftControlsNET")>
    Partial Public Class AniGif
    End Class

    ''' <summary>Stellt die Smart-Tag-Aktionen im Windows-Forms-Designer des jeweiligen Zielframeworks bereit.</summary>
    Friend NotInheritable Class AniGifDesigner
        Inherits ControlDesigner

        ''' <summary>Zwischengespeicherte Aktionsliste für diese Designerinstanz.</summary>
        Private _actions As DesignerActionListCollection

        ''' <summary>Gibt die Aktionen für das vom Designer verwaltete AniGif-Control zurück.</summary>
        Public Overrides ReadOnly Property ActionLists As DesignerActionListCollection
            Get
                If Me._actions Is Nothing Then
                    Me._actions = MyBase.ActionLists
#Disable Warning IDE0058 ' Der Ausdruckswert wird niemals verwendet.
                    Me._actions.Add(New AniGifActionList(DirectCast(Me.Component, AniGif)))
#Enable Warning IDE0058 ' Der Ausdruckswert wird niemals verwendet.
                End If
                Return Me._actions
            End Get
        End Property
    End Class

    ''' <summary>Leitet Smart-Tag-Einstellungen über den Designer an das AniGif-Control weiter.</summary>
    Friend NotInheritable Class AniGifActionList
        Inherits DesignerActionList

        ''' <summary>Das vom Smart-Tag bearbeitete Control.</summary>
        Private ReadOnly _control As AniGif

        ''' <summary>Erstellt die Aktionsliste für ein AniGif-Control.</summary>
        ''' <param name="control">Das im Designer bearbeitete Control.</param>
        Public Sub New(control As AniGif)
            MyBase.New(control)
            Me._control = control
        End Sub

        ''' <summary>Legt den automatischen Wiedergabestart fest.</summary>
        Public Property AutoPlay As Boolean
            Get
                Return Me._control.AutoPlay
            End Get
            Set(value As Boolean)
                Me.SetControlProperty(NameOf(AniGif.AutoPlay), value)
            End Set
        End Property

        ''' <summary>Wählt zwischen benutzerdefinierter Geschwindigkeit und GIF-Verzögerungen.</summary>
        Public Property CustomDisplaySpeed As Boolean
            Get
                Return Me._control.CustomDisplaySpeed
            End Get
            Set(value As Boolean)
                Me.SetControlProperty(NameOf(AniGif.CustomDisplaySpeed), value)
            End Set
        End Property

        ''' <summary>Legt die benutzerdefinierte Bildrate fest.</summary>
        Public Property FramesPerSecond As Decimal
            Get
                Return Me._control.FramesPerSecond
            End Get
            Set(value As Decimal)
                Me.SetControlProperty(NameOf(AniGif.FramesPerSecond), value)
            End Set
        End Property

        ''' <summary>Legt den Anzeigemodus der Grafik fest.</summary>
        Public Property GifSizeMode As ImageSizeMode
            Get
                Return Me._control.GifSizeMode
            End Get
            Set(value As ImageSizeMode)
                Me.SetControlProperty(NameOf(AniGif.GifSizeMode), value)
            End Set
        End Property

        ''' <summary>Erstellt die im Smart-Tag angebotenen Einstellmöglichkeiten.</summary>
        ''' <returns>Die sortierte Liste der Eigenschaften.</returns>
        Public Overrides Function GetSortedActionItems() As DesignerActionItemCollection
            Dim items As New DesignerActionItemCollection From {
                New DesignerActionHeaderItem("Wiedergabe"),
                New DesignerActionPropertyItem(NameOf(AutoPlay), "Automatisch starten", "Wiedergabe", "Startet die Animation automatisch."),
                New DesignerActionPropertyItem(NameOf(CustomDisplaySpeed), "Benutzerdefinierte Geschwindigkeit", "Wiedergabe", "Verwendet die eingestellte Bildrate statt der GIF-Verzögerungen."),
                New DesignerActionPropertyItem(NameOf(FramesPerSecond), "Bilder pro Sekunde", "Wiedergabe", "Legt die benutzerdefinierte Bildrate fest."),
                New DesignerActionHeaderItem("Darstellung"),
                New DesignerActionPropertyItem(NameOf(GifSizeMode), "Anzeigemodus", "Darstellung", "Legt die Skalierung und Ausrichtung fest.")
            }
            Return items
        End Function

        ''' <summary>Setzt eine Eigenschaft über ihren Descriptor für Designeränderungen und Serialisierung.</summary>
        ''' <param name="name">Name der Control-Eigenschaft.</param>
        ''' <param name="value">Neuer Wert für die Eigenschaft.</param>
        Private Sub SetControlProperty(name As String, value As Object)
            Dim descriptor As PropertyDescriptor = TypeDescriptor.GetProperties(Me._control)(name)
            If descriptor Is Nothing Then Throw New InvalidOperationException($"Die Eigenschaft '{name}' wurde nicht gefunden.")
            descriptor.SetValue(Me._control, value)
        End Sub
    End Class
End Namespace
