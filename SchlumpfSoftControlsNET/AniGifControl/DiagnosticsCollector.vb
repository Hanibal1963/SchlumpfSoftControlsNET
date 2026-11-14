' --------------------------------------------------------------------------------------------------------
' Datei: DiagnosticsCollector.vb
' Author: Andreas Sauer
' Datum: 14.11.2026
' --------------------------------------------------------------------------------------------------------

Namespace AniGifControl

	Friend NotInheritable Class DiagnosticsCollector

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
