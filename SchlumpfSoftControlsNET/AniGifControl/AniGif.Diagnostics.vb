' --------------------------------------------------------------------------------------------------------
' Datei: AniGif.Diagnostics.vb
' Author: Andreas Sauer
' Datum: 13.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Windows.Forms

Namespace AniGifControl

	Partial Public Class AniGif

		Private _diagnosticsEnabled As Boolean
		Private _diagnosticsCollector As AniGifDiagnosticsCollector
		Private _diagnosticsAncestors As List(Of Control)
		Private _diagnosticsHandleAvailable As Boolean

		''' <summary>
		''' Aktiviert oder deaktiviert die optionale Erfassung der Wiedergabe- und Zeichenmetriken.
		''' Der Standardwert ist <see langword="False"/>.
		''' </summary>
		''' <remarks>
		''' Das Aktivieren beginnt eine neue Messung; das Deaktivieren friert die bisherigen Werte ein.
		''' Die Eigenschaft muss auf dem UI-Thread des Controls verwendet werden.
		''' </remarks>
		''' <exception cref="ObjectDisposedException">Die Diagnostik wird nach Beginn der Ressourcenfreigabe aktiviert.</exception>
		<Browsable(False)>
		<DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
		<DefaultValue(False)>
		Public Property DiagnosticsEnabled As Boolean
			Get
				Return Me._diagnosticsEnabled
			End Get
			Set(value As Boolean)
#Disable Warning CA1513 ' ObjectDisposedException.ThrowIf ist unter .NET Framework nicht verfügbar.
				If value AndAlso (Me._IsDisposed OrElse Me.IsDisposed) Then Throw New ObjectDisposedException(NameOf(AniGif))
#Enable Warning CA1513
				If Me._diagnosticsEnabled = value Then Return

				If value Then
					If Me._diagnosticsCollector Is Nothing Then Me._diagnosticsCollector = New AniGifDiagnosticsCollector()
					Me._diagnosticsCollector.Reset()
					Me._diagnosticsEnabled = True
					Me.RefreshDiagnosticsAncestors()
					Me.UpdateDiagnosticsActivity()
				Else
					Me.SuspendDiagnosticsActivity()
					Me.DetachDiagnosticsAncestors()
					Me._diagnosticsEnabled = False
				End If
			End Set
		End Property

		''' <summary>
		''' Gibt eine unveränderliche Momentaufnahme der Diagnostik zurück.
		''' </summary>
		''' <remarks>
		''' Eingefrorene Werte bleiben nach dem Deaktivieren und nach der Ressourcenfreigabe lesbar.
		''' Die Eigenschaft muss auf dem UI-Thread des Controls gelesen werden.
		''' </remarks>
		<Browsable(False)>
		<DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
		Public ReadOnly Property Diagnostics As AniGifDiagnostics
			Get
				If Me._diagnosticsCollector Is Nothing Then Return New AniGifDiagnostics(False, 0, 0, 0, 0, 0, 0)
				If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
				Dim timestamp As Int64 = If(Me._diagnosticsCollector.IsActive, Stopwatch.GetTimestamp(), 0L)
				Return Me._diagnosticsCollector.CreateSnapshot(Me._diagnosticsEnabled, timestamp)
			End Get
		End Property

		''' <summary>
		''' Setzt die Diagnosewerte zurück, ohne den Wiedergabezustand zu ändern.
		''' </summary>
		''' <remarks>
		''' Bei aktivierter Diagnostik beginnt eine neue Messung. Nach der Ressourcenfreigabe bleibt der Aufruf ohne Wirkung.
		''' Die Methode muss auf dem UI-Thread des Controls aufgerufen werden.
		''' </remarks>
		Public Sub ResetDiagnostics()
			If Me._IsDisposed OrElse Me.IsDisposed OrElse Me._diagnosticsCollector Is Nothing Then Return
			Me._diagnosticsCollector.Reset()
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
		End Sub

		Protected Overrides Sub OnHandleCreated(e As EventArgs)
			Me._diagnosticsHandleAvailable = True
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
			MyBase.OnHandleCreated(e)
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
		End Sub

		Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
			Me._diagnosticsHandleAvailable = False
			Me.SuspendDiagnosticsActivity()
			MyBase.OnHandleDestroyed(e)
		End Sub

		Protected Overrides Sub OnClientSizeChanged(e As EventArgs)
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
			MyBase.OnClientSizeChanged(e)
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
		End Sub

		Private Sub UpdateDiagnosticsActivity(Optional isVisible As Boolean? = Nothing)
			If Not Me._diagnosticsEnabled Then Return

			Dim isActive As Boolean = Not Me._IsDisposed AndAlso Not Me.IsDisposed AndAlso Me._IsAnimating AndAlso
				Me._Autoplay AndAlso Me._playbackState = PlaybackState.Playing AndAlso
				Me._diagnosticsHandleAvailable AndAlso Me.IsHandleCreated AndAlso
				Me.ClientSize.Width > 0 AndAlso Me.ClientSize.Height > 0 AndAlso
				If(isVisible, Me.IsControlTreeVisible())
			If isActive <> Me._diagnosticsCollector.IsActive Then
				Me._diagnosticsCollector.SetActive(isActive, Stopwatch.GetTimestamp())
			End If
		End Sub

		Private Sub SuspendDiagnosticsActivity()
			If Not Me._diagnosticsEnabled Then Return
			If Me._diagnosticsCollector.IsActive Then
				Me._diagnosticsCollector.SetActive(False, Stopwatch.GetTimestamp())
			Else
				Me._diagnosticsCollector.CancelPendingFrame()
			End If
		End Sub

		Private Sub RefreshDiagnosticsAncestors()
			Me.DetachDiagnosticsAncestors()
			Dim ancestor As Control = Me.Parent
			If ancestor Is Nothing Then Return
			If Me._diagnosticsAncestors Is Nothing Then Me._diagnosticsAncestors = New List(Of Control)()

			While ancestor IsNot Nothing
				Me._diagnosticsAncestors.Add(ancestor)
				AddHandler ancestor.VisibleChanged, AddressOf Me.DiagnosticsAncestorVisibleChanged
				AddHandler ancestor.ParentChanged, AddressOf Me.DiagnosticsAncestorParentChanged
				ancestor = ancestor.Parent
			End While
		End Sub

		Private Sub DetachDiagnosticsAncestors()
			If Me._diagnosticsAncestors Is Nothing Then Return
			For Each ancestor As Control In Me._diagnosticsAncestors
				RemoveHandler ancestor.VisibleChanged, AddressOf Me.DiagnosticsAncestorVisibleChanged
				RemoveHandler ancestor.ParentChanged, AddressOf Me.DiagnosticsAncestorParentChanged
			Next
			Me._diagnosticsAncestors.Clear()
		End Sub

		Private Sub DiagnosticsAncestorVisibleChanged(sender As Object, e As EventArgs)
			If Me._diagnosticsEnabled Then Me.UpdateDiagnosticsActivity()
		End Sub

		Private Sub DiagnosticsAncestorParentChanged(sender As Object, e As EventArgs)
			If Not Me._diagnosticsEnabled Then Return
			Me.RefreshDiagnosticsAncestors()
			Me.UpdateDiagnosticsActivity()
		End Sub

	End Class

	Friend NotInheritable Class AniGifDiagnosticsCollector

		Private _isActive As Boolean
		Private _activeStartTimestamp As Int64
		Private _activeTicks As Int64
		Private _frameGeneration As Int64
		Private _pendingFrameGeneration As Int64
		Private _sessionVersion As Int64
		Private _renderedFrames As Int64
		Private _droppedFrames As Int64
		Private _renderOperations As Int64
		Private _totalRenderTicks As Int64
		Private _maximumRenderTicks As Int64

		Friend ReadOnly Property IsActive As Boolean
			Get
				Return Me._isActive
			End Get
		End Property

		Friend ReadOnly Property PendingFrameGeneration As Int64
			Get
				Return Me._pendingFrameGeneration
			End Get
		End Property

		Friend ReadOnly Property SessionVersion As Int64
			Get
				Return Me._sessionVersion
			End Get
		End Property

		Friend Sub Reset()
			Me._sessionVersion += 1
			Me._isActive = False
			Me._activeStartTimestamp = 0
			Me._activeTicks = 0
			Me._pendingFrameGeneration = 0
			Me._renderedFrames = 0
			Me._droppedFrames = 0
			Me._renderOperations = 0
			Me._totalRenderTicks = 0
			Me._maximumRenderTicks = 0
		End Sub

		Friend Sub SetActive(value As Boolean, timestamp As Int64)
			If Me._isActive = value Then Return
			If value Then
				Me._activeStartTimestamp = timestamp
			Else
				Me._activeTicks += timestamp - Me._activeStartTimestamp
				Me._pendingFrameGeneration = 0
			End If
			Me._isActive = value
		End Sub

		Friend Sub QueueTimerFrame()
			If Not Me._isActive Then Return
			If Me._pendingFrameGeneration <> 0 Then Me._droppedFrames += 1
			Me._frameGeneration += 1
			Me._pendingFrameGeneration = Me._frameGeneration
		End Sub

		Friend Sub CancelPendingFrame()
			Me._pendingFrameGeneration = 0
		End Sub

		Friend Sub RecordDraw(elapsedTicks As Int64, frameGeneration As Int64, intersectsClip As Boolean)
			Me._renderOperations += 1
			Me._totalRenderTicks += elapsedTicks
			Me._maximumRenderTicks = Math.Max(Me._maximumRenderTicks, elapsedTicks)

			If Me._isActive AndAlso intersectsClip AndAlso frameGeneration <> 0 AndAlso frameGeneration = Me._pendingFrameGeneration Then
				Me._renderedFrames += 1
				Me._pendingFrameGeneration = 0
			End If
		End Sub

		Friend Function CreateSnapshot(isEnabled As Boolean, timestamp As Int64) As AniGifDiagnostics
			Dim activeTicks As Int64 = Me._activeTicks
			If Me._isActive Then activeTicks += timestamp - Me._activeStartTimestamp
			Return New AniGifDiagnostics(isEnabled, activeTicks, Me._renderedFrames, Me._droppedFrames,
				Me._renderOperations, Me._totalRenderTicks, Me._maximumRenderTicks)
		End Function

	End Class

End Namespace