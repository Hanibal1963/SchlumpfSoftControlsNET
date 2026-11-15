' --------------------------------------------------------------------------------------------------------
' Datei: AniGifTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.AniGifControl

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Namespace SchlumpfSoftControlsNET.Tests.AniGifControl

    <TestClass>
    Partial Public Class AniGifTests

        Private NotInheritable Class FrameInvalidationProbe
            Inherits AniGif

            Public Sub DestroyHandleForFrameTest()
                Me.DestroyHandle()
            End Sub

            Public Overloads Sub PaintFrame(graphics As Graphics, clip As Rectangle)
                Dim state As GraphicsState = graphics.Save()
                Try
                    graphics.SetClip(clip, CombineMode.Intersect)
                    Using e As New PaintEventArgs(graphics, clip)
                        Me.OnPaintBackground(e)
                        Me.OnPaint(e)
                    End Using
                Finally
                    graphics.Restore(state)
                End Try
            End Sub

            Public Overloads Sub PaintFrame(surface As Bitmap, clip As Rectangle)
                Using graphics As Graphics = Graphics.FromImage(surface)
                    Me.PaintFrame(graphics, clip)
                End Using
            End Sub

        End Class

        Private NotInheritable Class FrameInvalidationScrollPanel
            Inherits Panel

            Public Sub RaiseScrollForFrameTest()
                Me.OnScroll(New ScrollEventArgs(ScrollEventType.ThumbPosition, 5))
            End Sub

        End Class

        Private NotInheritable Class FrameInvalidationTestForm
            Inherits Form

            Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
                Get
                    Return True
                End Get
            End Property

        End Class

        Private NotInheritable Class PaintProbeAniGif
            Inherits AniGif

            Public Sub DestroyHandleForTest()
                Me.DestroyHandle()
            End Sub

            Public Sub PaintTo(graphics As Graphics, bounds As Rectangle)
                Using e As New PaintEventArgs(graphics, bounds)
                    Me.OnPaint(e)
                End Using
            End Sub

        End Class

        Private NotInheritable Class ReentrantTimer
            Inherits System.Windows.Forms.Timer

            Private _onEnabledRead As Action

            Public Overrides Property Enabled As Boolean
                Get
                    Dim callback As Action = Me._onEnabledRead
                    Me._onEnabledRead = Nothing
                    If callback IsNot Nothing Then callback.Invoke()
                    Return MyBase.Enabled
                End Get
                Set(value As Boolean)
                    MyBase.Enabled = value
                End Set
            End Property

            Public Sub RunOnNextEnabledRead(callback As Action)
                Me._onEnabledRead = callback
            End Sub

        End Class

        Private NotInheritable Class TrackingTimer
            Inherits System.Windows.Forms.Timer

            Private _startCalls As Integer
            Private _stopCalls As Integer

            Public Overrides Property Enabled As Boolean
                Get
                    Return MyBase.Enabled
                End Get
                Set(value As Boolean)
                    If value Then
                        Me._startCalls += 1
                    Else
                        Me._stopCalls += 1
                    End If
                    MyBase.Enabled = value
                End Set
            End Property

            Public ReadOnly Property StartCalls As Integer
                Get
                    Return Me._startCalls
                End Get
            End Property

            Public ReadOnly Property StopCalls As Integer
                Get
                    Return Me._stopCalls
                End Get
            End Property

            Public Sub ResetCounts()
                Me._startCalls = 0
                Me._stopCalls = 0
            End Sub

        End Class

        <TestMethod>
        Public Sub AdaptiveTiming_DriftCorrectionUsesExistingBounds()
            Using control As New AniGif()
                control.AdaptiveTiming = True
                Assert.AreEqual(70, InvokeAdaptiveTiming(control, 100, 130, 100))
                Assert.AreEqual(160, InvokeAdaptiveTiming(control, 100, 40, 100))
                Assert.AreEqual(20, InvokeAdaptiveTiming(control, 100, 500, 100))
                Assert.AreEqual(500, InvokeAdaptiveTiming(control, 100, 1, 500))
                Assert.AreEqual(100, InvokeAdaptiveTiming(control, 100, 0, 100))
                Assert.AreEqual(91, InvokeAdaptiveTiming(control, 100, 10, 0))
                control.AdaptiveTiming = False
                Assert.AreEqual(100, InvokeAdaptiveTiming(control, 100, 130, 100))
                Assert.AreEqual(500, InvokeAdaptiveTiming(control, 700, 130, 100))
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub AdaptiveTiming_TimestampsFollowPlaybackAndDiagnosticsState()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each diagnostics As Boolean In New Boolean() {False, True}
                    Using timer As New TrackingTimer()
                        Using control As New AniGif()
                            InstallTrackingTimer(control, timer)
                            Dim handle As IntPtr = control.Handle
                            control.DiagnosticsEnabled = diagnostics
                            control.CustomDisplaySpeed = customSpeed
                            control.FramesPerSecond = 10D
                            Dim events As New List(Of String)()
                            AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                            AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                            control.StartAnimation()
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            InvokeTimerTick(control)
                            Assert.AreEqual(1, GetCurrentFrame(control))
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            control.SeekFrame(2)
                            control.MinimumFrameIntervalMs = 30
                            control.MaximumFrameIntervalMs = 400
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            control.PauseAnimation()
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            control.ResumeAnimation()
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))

                            control.AdaptiveTiming = True
                            Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))
                            SetLastFrameTimestamp(control, 1L)
                            control.SeekFrame(3)
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                            SetLastFrameTimestamp(control, 1L)
                            control.MinimumFrameIntervalMs = 130
                            Assert.AreEqual(130, timer.Interval)
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                            control.PauseAnimation()
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            control.ResumeAnimation()
                            Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))

                            control.MinimumFrameIntervalMs = 20
                            Assert.AreEqual(100, timer.Interval)
                            SetLastFrameTimestamp(control, Math.Max(1L, Stopwatch.GetTimestamp() - Stopwatch.Frequency))
                            InvokeTimerTick(control)
                            Assert.AreEqual(20, timer.Interval)
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))

                            timer.Interval = 37
                            control.AdaptiveTiming = False
                            Assert.AreEqual(37, timer.Interval)
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            InvokeTimerTick(control)
                            Assert.AreEqual(100, timer.Interval)
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            control.StopAnimation()
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                            CollectionAssert.AreEqual(New String() {"started", "stopped", "started", "stopped", "started", "stopped"}, events.ToArray())
                        End Using
                    End Using
                Next
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub AdaptiveTiming_TogglePreservesTheScheduledIntervalUntilTheNextTick()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.CustomDisplaySpeed = customSpeed
                        control.FramesPerSecond = 10D
                        control.AdaptiveTiming = True
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        timer.Interval = 80
                        timer.ResetCounts()

                        control.AdaptiveTiming = False
                        Assert.AreEqual(80, timer.Interval)
                        InvokeTimerTick(control)
                        Assert.AreEqual(100, timer.Interval)
                        control.AdaptiveTiming = True
                        Assert.AreEqual(100, timer.Interval)
                        Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub AdaptiveTiming_VisibilityAndPausedTransitionsRestoreTheCorrectClockState()
            Using parent As New Panel()
                Using control As New AniGif()
                    parent.Controls.Add(control)
                    control.CustomDisplaySpeed = True
                    control.AdaptiveTiming = True
                    control.StartAnimation()
                    Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))

                    control.Visible = False
                    Assert.IsFalse(GetTimer(control).Enabled)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    control.SeekFrame(1)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    control.Visible = True
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))

                    control.PauseAnimation()
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    control.AdaptiveTiming = False
                    control.Visible = False
                    control.ResumeAnimation()
                    Assert.IsFalse(GetTimer(control).Enabled)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    control.Visible = True
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    InvokeTimerTick(control)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    control.PauseAnimation()
                    control.AdaptiveTiming = True
                    Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))
                    SetLastFrameTimestamp(control, 1L)
                    control.ResumeAnimation()
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub AnimationStarted_HandlerCanChangeTimingAndPause()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    Dim events As New List(Of String)()
                    AddHandler control.AnimationStarted, Sub(sender, e)
                                                             events.Add("started")
                                                             If events.Count = 1 Then
                                                                 control.FramesPerSecond = 20D
                                                                 control.PauseAnimation()
                                                             End If
                                                         End Sub
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                    timer.ResetCounts()

                    control.StartAnimation()

                    Assert.AreEqual(20D, control.FramesPerSecond)
                    Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())
                    Assert.IsTrue(control.AutoPlay)
                    Assert.IsFalse(timer.Enabled)
                    Assert.AreEqual(1, timer.StartCalls)
                    Assert.AreEqual(1, timer.StopCalls)
                    CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub AnimationStarted_IsRaisedWhenAnimatedGifStarts()
            Using control As New AniGif()
                Using animatedGif As Bitmap = LoadAnimatedGif()
                    control.Gif = animatedGif

                    Dim startedRaised As Boolean = False
                    AddHandler control.AnimationStarted, Sub(sender, e) startedRaised = True

                    control.StartAnimation()

                    Assert.IsTrue(startedRaised)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub AnimationStopped_HandlerCanDisposeDuringVisibilityChange()
            Using control As New AniGif()
                Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                Dim stoppedCount As Integer = 0
                control.StartAnimation()
                AddHandler control.AnimationStopped, Sub(sender, e)
                                                         stoppedCount += 1
                                                         control.Dispose()
                                                     End Sub

                control.Visible = False

                Assert.IsTrue(control.IsDisposed)
                Assert.IsFalse(timer.Enabled)
                Assert.AreEqual(1, stoppedCount)
                Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
            End Using
        End Sub

        <TestMethod>
        Public Sub AnimationStopped_HandlerCanRestartWithoutDuplicateEvents()
            Using control As New AniGif()
                Dim startedCount As Integer = 0
                Dim stoppedCount As Integer = 0
                AddHandler control.AnimationStarted, Sub(sender, e) startedCount += 1
                AddHandler control.AnimationStopped, Sub(sender, e)
                                                         stoppedCount += 1
                                                         control.StartAnimation()
                                                     End Sub

                control.StartAnimation()
                control.StopAnimation()
                Assert.IsTrue(control.AutoPlay)
                Assert.IsTrue(GetTimer(control).Enabled)
                Assert.AreEqual(2, startedCount)
                Assert.AreEqual(1, stoppedCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub AnimationStopped_IsRaisedWhenRunningAnimationStops()
            Using control As New AniGif()
                Using animatedGif As Bitmap = LoadAnimatedGif()
                    control.Gif = animatedGif

                    Dim startedCount As Integer = 0
                    Dim stoppedCount As Integer = 0
                    AddHandler control.AnimationStarted, Sub(sender, e) startedCount += 1
                    AddHandler control.AnimationStopped, Sub(sender, e) stoppedCount += 1

                    control.StartAnimation()
                    control.StopAnimation()

                    Assert.AreEqual(1, startedCount)
                    Assert.AreEqual(1, stoppedCount)
                End Using
            End Using
        End Sub

        Private Shared Sub AssertDrawingRectangle(imageSize As Size, clientSize As Size, sizeMode As ImageSizeMode, zoom As Decimal, expected As Rectangle)
            Using source As New Bitmap(imageSize.Width, imageSize.Height)
                Using control As New AniGif()
                    control.ClientSize = clientSize
                    control.Gif = source
                    control.GifSizeMode = sizeMode
                    control.ZoomFactor = zoom
                    Assert.AreEqual(expected, GetDrawingRectangle(control), $"Bild {imageSize}, Client {clientSize}, {sizeMode}, Zoom {zoom}")
                End Using
            End Using
        End Sub

        Private Shared Sub AssertFrameSequenceMatchesFull(source As Bitmap, clientSize As Size, mode As ImageSizeMode, zoom As Decimal,
            quality As GifScalingQualityPreset, transparentBackground As Boolean, expectedInvalidation As Rectangle,
            Optional backgroundImage As Image = Nothing, Optional backgroundLayout As ImageLayout = ImageLayout.Tile)
            Using baselineParent As New Panel()
                Using optimizedParent As New Panel()
                    Using baseline As New FrameInvalidationProbe()
                        Using optimized As New FrameInvalidationProbe()
                            baselineParent.ClientSize = New Size(Math.Max(200, clientSize.Width + 20), Math.Max(160, clientSize.Height + 20))
                            optimizedParent.ClientSize = baselineParent.ClientSize
                            baselineParent.BackColor = Color.Navy
                            optimizedParent.BackColor = baselineParent.BackColor
                            baselineParent.Controls.Add(baseline)
                            optimizedParent.Controls.Add(optimized)
                            baseline.ClientSize = clientSize
                            optimized.ClientSize = clientSize
                            baseline.BackColor = If(transparentBackground, Color.Transparent, Color.Magenta)
                            optimized.BackColor = baseline.BackColor
                            baseline.BackgroundImage = backgroundImage
                            optimized.BackgroundImage = backgroundImage
                            baseline.BackgroundImageLayout = backgroundLayout
                            optimized.BackgroundImageLayout = backgroundLayout
                            baseline.Gif = source
                            optimized.Gif = source
                            baseline.GifSizeMode = mode
                            optimized.GifSizeMode = mode
                            baseline.ZoomFactor = zoom
                            optimized.ZoomFactor = zoom
                            baseline.ScalingQualityPreset = quality
                            optimized.ScalingQualityPreset = quality
                            optimized.UsePartialFrameInvalidation = True
                            Dim baselineHandle As IntPtr = baseline.Handle
                            Dim optimizedHandle As IntPtr = optimized.Handle
                            Assert.AreNotEqual(IntPtr.Zero, baselineHandle)
                            Assert.AreNotEqual(IntPtr.Zero, optimizedHandle)
                            Dim baselineInvalidations As New List(Of Rectangle)()
                            Dim optimizedInvalidations As New List(Of Rectangle)()
                            AddHandler baseline.Invalidated, Sub(sender, e) baselineInvalidations.Add(e.InvalidRect)
                            AddHandler optimized.Invalidated, Sub(sender, e) optimizedInvalidations.Add(e.InvalidRect)
                            baseline.StartAnimation()
                            optimized.StartAnimation()
                            InvokeTimerTick(baseline)
                            InvokeTimerTick(optimized)

                            Using baselineSurface As New Bitmap(clientSize.Width, clientSize.Height)
                                Using optimizedSurface As New Bitmap(clientSize.Width, clientSize.Height)
                                    If transparentBackground Then
                                        FillFrameBackground(baselineSurface, baseline.ClientRectangle, Color.Navy)
                                        FillFrameBackground(optimizedSurface, optimized.ClientRectangle, Color.Navy)
                                    End If
                                    baseline.PaintFrame(baselineSurface, baseline.ClientRectangle)
                                    optimized.PaintFrame(optimizedSurface, optimized.ClientRectangle)
                                    AssertSamePixels(baselineSurface, optimizedSurface)
                                    For frame As Integer = 1 To 4
                                        baselineInvalidations.Clear()
                                        optimizedInvalidations.Clear()
                                        InvokeTimerTick(baseline)
                                        InvokeTimerTick(optimized)
                                        Assert.HasCount(1, baselineInvalidations)
                                        Assert.HasCount(1, optimizedInvalidations)
                                        Assert.AreEqual(baseline.ClientRectangle, baselineInvalidations(0))
                                        Assert.AreEqual(expectedInvalidation, optimizedInvalidations(0), $"{mode}, {zoom}, {quality}, {transparentBackground}")
                                        If transparentBackground Then
                                            FillFrameBackground(baselineSurface, baselineInvalidations(0), Color.Navy)
                                            FillFrameBackground(optimizedSurface, optimizedInvalidations(0), Color.Navy)
                                        End If
                                        baseline.PaintFrame(baselineSurface, baselineInvalidations(0))
                                        optimized.PaintFrame(optimizedSurface, optimizedInvalidations(0))
                                        AssertSamePixels(baselineSurface, optimizedSurface)
                                    Next
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        Private Shared Sub AssertLastFrameInvalidation(invalidations As List(Of Rectangle), expected As Rectangle)
            Assert.IsNotEmpty(invalidations)
            Assert.AreEqual(expected, invalidations(invalidations.Count - 1))
        End Sub

        Private Shared Sub AssertSamePixels(expected As Bitmap, actual As Bitmap)
            Assert.AreEqual(expected.Size, actual.Size)
            For y As Integer = 0 To expected.Height - 1
                For x As Integer = 0 To expected.Width - 1
                    Assert.AreEqual(expected.GetPixel(x, y).ToArgb(), actual.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y})")
                Next
            Next
        End Sub

        Private Shared Sub AssertScaledPixels(image As Bitmap, clientSize As Size, first As Color, middle As Color, last As Color, message As String)
            Dim points As Point() = If(clientSize.Width > clientSize.Height,
                New Point() {New Point(20, 20), New Point(80, 20), New Point(140, 20)},
                New Point() {New Point(20, 20), New Point(20, 80), New Point(20, 140)})
            Dim expected As Color() = {first, middle, last}
            For index As Integer = 0 To points.Length - 1
                Assert.AreEqual(expected(index).ToArgb(), image.GetPixel(points(index).X, points(index).Y).ToArgb(), $"{message}, {clientSize}, Pixel {points(index)}")
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub AutoPlayChanged_HandlerCanChangeTimingWithoutDuplicateStart()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    Dim events As New List(Of String)()
                    AddHandler control.AutoPlayChanged, Sub(sender, e)
                                                            events.Add("autoplay")
                                                            If control.AutoPlay Then control.FramesPerSecond = 20D
                                                        End Sub
                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                    timer.ResetCounts()

                    control.StartAnimation()

                    Assert.IsTrue(timer.Enabled)
                    Assert.AreEqual(50, timer.Interval)
                    Assert.AreEqual(1, timer.StartCalls)
                    Assert.AreEqual(0, timer.StopCalls)
                    CollectionAssert.AreEqual(New String() {"autoplay", "started"}, events.ToArray())
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub AutoPlayChanged_IsRaisedWhenValueChanges()
            Using control As New AniGif()
                Dim eventCount As Integer = 0
                AddHandler control.AutoPlayChanged, Sub(sender, e) eventCount += 1

                control.AutoPlay = True
                control.AutoPlay = True
                control.AutoPlay = False

                Assert.AreEqual(2, eventCount)
            End Using
        End Sub

        Private Shared Function CreateCheckerboard(size As Integer) As Bitmap
            Dim image As New Bitmap(size, size)
            For y As Integer = 0 To size - 1
                For x As Integer = 0 To size - 1
                    image.SetPixel(x, y, If((x + y) Mod 2 = 0, Color.Black, Color.White))
                Next
            Next
            Return image
        End Function

        Private Shared Function CreateDelayBytes(ParamArray delays As Integer()) As Byte()
            Dim bytes((delays.Length * 4) - 1) As Byte
            For i As Integer = 0 To delays.Length - 1
                Buffer.BlockCopy(BitConverter.GetBytes(delays(i)), 0, bytes, i * 4, 4)
            Next
            Return bytes
        End Function

        <MethodImpl(MethodImplOptions.NoInlining)>
        Private Shared Function CreateDisposedFrameControl(parent As Control) As WeakReference
            Dim control As New AniGif()
            parent.Controls.Add(control)
            control.UsePartialFrameInvalidation = True
            Dim released As New WeakReference(control)
            control.Dispose()
            Return released
        End Function

        <TestMethod>
        Public Sub DefaultGif_IsIndependentForEachControlAndAfterReset()
            Using second As New AniGif()
                Using first As New AniGif()
                    Assert.AreNotSame(first.Gif, second.Gif)
                    Using expected As Bitmap = LoadAnimatedGif()
                        first.SeekFrame(1)
                        Assert.AreEqual(0, GetCurrentFrame(second))
                        AssertSamePixels(expected, second.Gif)
                    End Using

                    Dim oldImage As Bitmap = first.Gif
                    first.Gif = Nothing
                    Assert.AreNotSame(oldImage, first.Gif)
                    Assert.AreNotSame(first.Gif, second.Gif)
                    Assert.AreEqual(0, GetCurrentFrame(first))
                End Using

                second.SeekFrame(1)
                Assert.AreEqual(1, GetCurrentFrame(second))
            End Using
        End Sub

        <TestMethod>
        Public Sub DefaultGif_IsInitializedAndWrapsFromTheLastFrame()
            Using control As New AniGif()
                Dim dimension As New FrameDimension(control.Gif.FrameDimensionsList(0))
                Dim lastFrame As Integer = control.Gif.GetFrameCount(dimension) - 1
                Assert.IsGreaterThan(0, lastFrame)

                control.SeekFrame(lastFrame)
                Assert.AreEqual(lastFrame, GetCurrentFrame(control))
                control.StartAnimation()
                Assert.IsTrue(GetTimer(control).Enabled)
                InvokeTimerTick(control)
                Assert.AreEqual(0, GetCurrentFrame(control))
            End Using
        End Sub

        <TestMethod>
        Public Sub DiagnosticsCollector_CalculatesRatesDropsAndRenderTimesFromActiveIntervals()
            Dim collectorType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".DiagnosticsCollector")
            Assert.IsNotNull(collectorType)
            Dim collector As Object = Activator.CreateInstance(collectorType, True)
            Assert.IsNotNull(collector)
            Dim frequency As Long = Stopwatch.Frequency

            InvokeDiagnosticsCollector(collector, "Reset")
            InvokeDiagnosticsCollector(collector, "SetActive", True, 0L)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            Dim secondGeneration As Long = GetPendingDiagnosticsGeneration(collector)
            InvokeDiagnosticsCollector(collector, "RecordDraw", frequency \ 10, secondGeneration, True)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            Dim thirdGeneration As Long = GetPendingDiagnosticsGeneration(collector)
            InvokeDiagnosticsCollector(collector, "RecordDraw", frequency \ 5, thirdGeneration, True)
            InvokeDiagnosticsCollector(collector, "SetActive", False, frequency)

            Dim snapshot As AniGifDiagnostics = GetDiagnosticsCollectorSnapshot(collector, True, frequency)
            Assert.AreEqual(TimeSpan.FromSeconds(1), snapshot.ActiveDuration)
            Assert.AreEqual(2L, snapshot.RenderedFrames)
            Assert.AreEqual(1L, snapshot.DroppedFrames)
            Assert.AreEqual(2L, snapshot.RenderOperations)
            Assert.IsTrue(snapshot.ActualFps.HasValue)
            Assert.AreEqual(2.0R, snapshot.ActualFps.Value, 0.000001R)
            Assert.IsTrue(snapshot.AverageRenderTimeMs.HasValue)
            Assert.AreEqual(150.0R, snapshot.AverageRenderTimeMs.Value, 0.1R)
            Assert.IsTrue(snapshot.MaxRenderTimeMs.HasValue)
            Assert.AreEqual(200.0R, snapshot.MaxRenderTimeMs.Value, 0.1R)

            InvokeDiagnosticsCollector(collector, "SetActive", True, 3L * frequency)
            InvokeDiagnosticsCollector(collector, "SetActive", False, 4L * frequency)
            Dim resumed As AniGifDiagnostics = GetDiagnosticsCollectorSnapshot(collector, False, 5L * frequency)
            Assert.AreEqual(TimeSpan.FromSeconds(2), resumed.ActiveDuration)
            Assert.IsTrue(resumed.ActualFps.HasValue)
            Assert.AreEqual(1.0R, resumed.ActualFps.Value, 0.000001R)
            Assert.AreEqual(TimeSpan.FromSeconds(1), snapshot.ActiveDuration)
        End Sub

        <TestMethod>
        Public Sub DiagnosticsCollector_DoesNotCountStaleClippedOrCancelledFrames()
            Dim collectorType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".DiagnosticsCollector")
            Assert.IsNotNull(collectorType)
            Dim collector As Object = Activator.CreateInstance(collectorType, True)
            Assert.IsNotNull(collector)
            Dim frequency As Long = Stopwatch.Frequency

            InvokeDiagnosticsCollector(collector, "Reset")
            InvokeDiagnosticsCollector(collector, "SetActive", True, 0L)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            Dim firstGeneration As Long = GetPendingDiagnosticsGeneration(collector)
            InvokeDiagnosticsCollector(collector, "RecordDraw", 1L, firstGeneration, False)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            Dim secondGeneration As Long = GetPendingDiagnosticsGeneration(collector)
            InvokeDiagnosticsCollector(collector, "RecordDraw", 1L, firstGeneration, True)
            InvokeDiagnosticsCollector(collector, "RecordDraw", 1L, secondGeneration, True)
            InvokeDiagnosticsCollector(collector, "RecordDraw", 1L, secondGeneration, True)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            InvokeDiagnosticsCollector(collector, "SetActive", False, frequency)
            InvokeDiagnosticsCollector(collector, "SetActive", True, 2L * frequency)
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            InvokeDiagnosticsCollector(collector, "CancelPendingFrame")
            InvokeDiagnosticsCollector(collector, "QueueTimerFrame")
            InvokeDiagnosticsCollector(collector, "SetActive", False, 3L * frequency)

            Dim snapshot As AniGifDiagnostics = GetDiagnosticsCollectorSnapshot(collector, False, 3L * frequency)
            Assert.AreEqual(1L, snapshot.RenderedFrames)
            Assert.AreEqual(1L, snapshot.DroppedFrames)
            Assert.AreEqual(4L, snapshot.RenderOperations)
            Assert.AreEqual(TimeSpan.FromSeconds(2), snapshot.ActiveDuration)

            InvokeDiagnosticsCollector(collector, "Reset")
            Dim reset As AniGifDiagnostics = GetDiagnosticsCollectorSnapshot(collector, False, 0L)
            Assert.AreEqual(0L, reset.RenderedFrames)
            Assert.AreEqual(0L, reset.DroppedFrames)
            Assert.AreEqual(0L, reset.RenderOperations)
            Assert.IsFalse(reset.ActualFps.HasValue)
            Assert.IsFalse(reset.MaxRenderTimeMs.HasValue)
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_AncestorVisibilityHandlersObserveCurrentStateBeforeDiagnosticsHandler()
            Using parent As New Panel()
                Using nested As New Panel()
                    Using control As New PaintProbeAniGif()
                        parent.Controls.Add(nested)
                        nested.Controls.Add(control)
                        control.ClientSize = New Size(32, 32)
                        Dim handle As IntPtr = control.Handle
                        Assert.AreNotEqual(IntPtr.Zero, handle)
                        Dim hiddenBeforePaint As AniGifDiagnostics = Nothing
                        Dim hiddenAfterPaint As AniGifDiagnostics = Nothing
                        Dim shown As AniGifDiagnostics = Nothing

                        Using surface As New Bitmap(32, 32)
                            Using graphics As Graphics = Graphics.FromImage(surface)
                                Dim bounds As New Rectangle(Point.Empty, surface.Size)
                                Dim visibilityHandler As EventHandler = Sub(sender, e)
                                                                            If parent.Visible Then
                                                                                InvokeTimerTick(control)
                                                                                control.PaintTo(graphics, bounds)
                                                                                shown = control.Diagnostics
                                                                            Else
                                                                                hiddenBeforePaint = control.Diagnostics
                                                                                control.PaintTo(graphics, bounds)
                                                                                hiddenAfterPaint = control.Diagnostics
                                                                            End If
                                                                        End Sub
                                AddHandler parent.VisibleChanged, visibilityHandler
                                control.DiagnosticsEnabled = True
                                control.StartAnimation()
                                InvokeTimerTick(control)
                                parent.Visible = False
                                Assert.IsNotNull(hiddenBeforePaint)
                                Assert.IsNotNull(hiddenAfterPaint)
                                Assert.AreEqual(hiddenBeforePaint.ActiveDuration, hiddenAfterPaint.ActiveDuration)
                                Assert.AreEqual(0L, hiddenAfterPaint.RenderedFrames)
                                Assert.AreEqual(1L, hiddenAfterPaint.RenderOperations)

                                parent.Visible = True
                                Assert.IsNotNull(shown)
                                Assert.AreEqual(1L, shown.RenderedFrames)
                                Assert.AreEqual(0L, shown.DroppedFrames)
                                RemoveHandler parent.VisibleChanged, visibilityHandler
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_AutoPlayChangedHandlerObservesStoppedMeasurementImmediately()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()
                InvokeTimerTick(control)
                Dim beforePaint As AniGifDiagnostics = Nothing
                Dim afterPaint As AniGifDiagnostics = Nothing

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim autoPlayHandler As AniGif.AutoPlayChangedEventHandler = Sub(sender, e)
                                                                                        If Not control.AutoPlay Then
                                                                                            beforePaint = control.Diagnostics
                                                                                            control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                                                                                            afterPaint = control.Diagnostics
                                                                                        End If
                                                                                    End Sub
                        AddHandler control.AutoPlayChanged, autoPlayHandler
                        control.StopAnimation()
                        RemoveHandler control.AutoPlayChanged, autoPlayHandler
                    End Using
                End Using
                Assert.IsNotNull(beforePaint)
                Assert.IsNotNull(afterPaint)
                Assert.AreEqual(beforePaint.ActiveDuration, afterPaint.ActiveDuration)
                Assert.AreEqual(0L, afterPaint.RenderedFrames)
                Assert.AreEqual(0L, afterPaint.DroppedFrames)
                Assert.AreEqual(1L, afterPaint.RenderOperations)
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub Diagnostics_CountsTimerFramesInBothSpeedAndAdaptiveModes()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each adaptive As Boolean In New Boolean() {False, True}
                    Using control As New PaintProbeAniGif()
                        control.ClientSize = New Size(32, 32)
                        control.CustomDisplaySpeed = customSpeed
                        control.AdaptiveTiming = adaptive
                        control.DiagnosticsEnabled = True
                        Dim handle As IntPtr = control.Handle
                        Assert.AreNotEqual(IntPtr.Zero, handle)
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()

                        Using surface As New Bitmap(32, 32)
                            Using graphics As Graphics = Graphics.FromImage(surface)
                                Dim bounds As New Rectangle(Point.Empty, surface.Size)
                                InvokeTimerTick(control)
                                InvokeTimerTick(control)
                                control.PaintTo(graphics, bounds)
                                control.PaintTo(graphics, bounds)
                                InvokeTimerTick(control)
                                control.PaintTo(graphics, bounds)
                            End Using
                        End Using

                        Dim snapshot As AniGifDiagnostics = control.Diagnostics
                        Assert.AreEqual(2L, snapshot.RenderedFrames, $"custom={customSpeed}, adaptive={adaptive}")
                        Assert.AreEqual(1L, snapshot.DroppedFrames, $"custom={customSpeed}, adaptive={adaptive}")
                        Assert.AreEqual(3L, snapshot.RenderOperations)
                        Assert.IsTrue(snapshot.ActualFps.HasValue)
                        Assert.IsTrue(snapshot.AverageRenderTimeMs.HasValue)
                        Assert.IsTrue(snapshot.MaxRenderTimeMs.HasValue)
                        Assert.AreEqual(3, GetCurrentFrame(control))
                        Assert.IsTrue(GetTimer(control).Enabled)
                        control.StopAnimation()
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                    End Using
                Next
            Next
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_DefaultsAndFrozenSnapshotsAreIndependent()
            Using control As New PaintProbeAniGif()
                Dim initial As AniGifDiagnostics = control.Diagnostics
                Assert.IsFalse(control.DiagnosticsEnabled)
                Assert.IsFalse(initial.IsEnabled)
                Assert.AreEqual(TimeSpan.Zero, initial.ActiveDuration)
                Assert.AreEqual(0L, initial.RenderedFrames)
                Assert.AreEqual(0L, initial.DroppedFrames)
                Assert.AreEqual(0L, initial.RenderOperations)
                Assert.IsFalse(initial.ActualFps.HasValue)
                Assert.IsFalse(initial.AverageRenderTimeMs.HasValue)
                Assert.IsFalse(initial.MaxRenderTimeMs.HasValue)

                Using surface As New Bitmap(160, 160)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim bounds As New Rectangle(Point.Empty, control.ClientSize)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(0L, control.Diagnostics.RenderOperations)

                        control.DiagnosticsEnabled = True
                        control.PaintTo(graphics, bounds)
                        Dim measured As AniGifDiagnostics = control.Diagnostics
                        Assert.IsTrue(measured.IsEnabled)
                        Assert.AreEqual(1L, measured.RenderOperations)
                        Assert.AreEqual(0L, measured.RenderedFrames)
                        Assert.IsFalse(measured.ActualFps.HasValue)
                        Assert.IsTrue(measured.AverageRenderTimeMs.HasValue)
                        Assert.IsTrue(measured.MaxRenderTimeMs.HasValue)

                        control.DiagnosticsEnabled = False
                        control.PaintTo(graphics, bounds)
                        Assert.IsFalse(control.Diagnostics.IsEnabled)
                        Assert.AreEqual(1L, control.Diagnostics.RenderOperations)
                        Assert.AreEqual(0L, initial.RenderOperations)

                        control.ResetDiagnostics()
                        Assert.AreEqual(0L, control.Diagnostics.RenderOperations)
                        Assert.IsFalse(control.Diagnostics.AverageRenderTimeMs.HasValue)
                        Assert.AreEqual(1L, measured.RenderOperations)
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub Diagnostics_DoesNotChangePixelsTimerOrPlaybackEvents()
            For Each mode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Normal, ImageSizeMode.CenterImage, ImageSizeMode.Zoom, ImageSizeMode.Fill}
                For Each preset As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                    Using baseline As New PaintProbeAniGif()
                        Using measured As New PaintProbeAniGif()
                            baseline.ClientSize = New Size(32, 24)
                            measured.ClientSize = baseline.ClientSize
                            baseline.GifSizeMode = mode
                            measured.GifSizeMode = mode
                            baseline.ZoomFactor = 100D
                            measured.ZoomFactor = 100D
                            baseline.ScalingQualityPreset = preset
                            measured.ScalingQualityPreset = preset
                            measured.DiagnosticsEnabled = True
                            Dim baselineHandle As IntPtr = baseline.Handle
                            Dim measuredHandle As IntPtr = measured.Handle
                            Assert.AreNotEqual(IntPtr.Zero, baselineHandle)
                            Assert.AreNotEqual(IntPtr.Zero, measuredHandle)
                            Dim baselineEvents As New List(Of String)()
                            Dim measuredEvents As New List(Of String)()
                            AddHandler baseline.AnimationStarted, Sub(sender, e) baselineEvents.Add("started")
                            AddHandler baseline.AnimationStopped, Sub(sender, e) baselineEvents.Add("stopped")
                            AddHandler measured.AnimationStarted, Sub(sender, e) measuredEvents.Add("started")
                            AddHandler measured.AnimationStopped, Sub(sender, e) measuredEvents.Add("stopped")
                            baseline.StartAnimation()
                            measured.StartAnimation()
                            InvokeTimerTick(baseline)
                            InvokeTimerTick(measured)
                            Assert.AreEqual(GetCurrentFrame(baseline), GetCurrentFrame(measured))
                            Assert.AreEqual(GetTimer(baseline).Interval, GetTimer(measured).Interval)

                            Using baselineImage As New Bitmap(32, 24)
                                Using measuredImage As New Bitmap(32, 24)
                                    Using baselineGraphics As Graphics = Graphics.FromImage(baselineImage)
                                        Using measuredGraphics As Graphics = Graphics.FromImage(measuredImage)
                                            baselineGraphics.Clear(Color.Magenta)
                                            measuredGraphics.Clear(Color.Magenta)
                                            Dim bounds As New Rectangle(Point.Empty, baselineImage.Size)
                                            baseline.PaintTo(baselineGraphics, bounds)
                                            measured.PaintTo(measuredGraphics, bounds)
                                        End Using
                                    End Using
                                    AssertSamePixels(baselineImage, measuredImage)
                                End Using
                            End Using

                            baseline.StopAnimation()
                            measured.StopAnimation()
                            CollectionAssert.AreEqual(baselineEvents.ToArray(), measuredEvents.ToArray())
                            CollectionAssert.AreEqual(New String() {"started", "stopped"}, measuredEvents.ToArray())
                            Assert.AreEqual(0L, baseline.Diagnostics.RenderOperations)
                            Assert.AreEqual(1L, measured.Diagnostics.RenderOperations)
                        End Using
                    End Using
                Next
            Next
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_ExcludesPausedHiddenAndEmptyClientIntervals()
            Using parent As New Panel()
                Using nested As New Panel()
                    Using control As New PaintProbeAniGif()
                        parent.Controls.Add(nested)
                        nested.Controls.Add(control)
                        control.ClientSize = New Size(32, 32)
                        control.DiagnosticsEnabled = True
                        Dim handle As IntPtr = control.Handle
                        Assert.AreNotEqual(IntPtr.Zero, handle)
                        control.StartAnimation()
                        InvokeTimerTick(control)
                        control.PauseAnimation()
                        Dim paused As AniGifDiagnostics = control.Diagnostics
                        InvokeTimerTick(control)
                        Assert.AreEqual(paused.ActiveDuration, control.Diagnostics.ActiveDuration)

                        control.ResumeAnimation()
                        InvokeTimerTick(control)
                        parent.Visible = False
                        Dim hidden As AniGifDiagnostics = control.Diagnostics
                        InvokeTimerTick(control)
                        Assert.AreEqual(hidden.ActiveDuration, control.Diagnostics.ActiveDuration)

                        parent.Visible = True
                        Assert.IsTrue(GetTimer(control).Enabled)
                        Using surface As New Bitmap(32, 32)
                            Using graphics As Graphics = Graphics.FromImage(surface)
                                Dim bounds As New Rectangle(Point.Empty, surface.Size)
                                InvokeTimerTick(control)
                                control.PaintTo(graphics, bounds)
                                Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)

                                control.ClientSize = Size.Empty
                                Dim empty As AniGifDiagnostics = control.Diagnostics
                                InvokeTimerTick(control)
                                InvokeTimerTick(control)
                                Assert.AreEqual(empty.ActiveDuration, control.Diagnostics.ActiveDuration)
                                Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)

                                control.ClientSize = surface.Size
                                InvokeTimerTick(control)
                                control.PaintTo(graphics, bounds)
                                Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                                Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_ExcludesTimeWithoutAHandleAndResumesAfterRecreation()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim firstHandle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, firstHandle)
                control.StartAnimation()
                InvokeTimerTick(control)
                control.DestroyHandleForTest()
                Assert.IsFalse(control.IsHandleCreated)
                Dim withoutHandle As AniGifDiagnostics = control.Diagnostics
                InvokeTimerTick(control)
                InvokeTimerTick(control)
                Assert.AreEqual(withoutHandle.ActiveDuration, control.Diagnostics.ActiveDuration)
                Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)

                Dim recreatedHandle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, recreatedHandle)
                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                    End Using
                End Using
                Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_FollowsReparentedAncestorsAndDetachesOnDisable()
            Using firstParent As New Panel()
                Using secondParent As New Panel()
                    Using branch As New Panel()
                        Using control As New PaintProbeAniGif()
                            firstParent.Controls.Add(branch)
                            branch.Controls.Add(control)
                            control.ClientSize = New Size(32, 32)
                            control.DiagnosticsEnabled = True
                            Dim handle As IntPtr = control.Handle
                            Assert.AreNotEqual(IntPtr.Zero, handle)
                            control.StartAnimation()
                            InvokeTimerTick(control)

                            firstParent.Visible = False
                            Dim hidden As AniGifDiagnostics = control.Diagnostics
                            InvokeTimerTick(control)
                            Assert.AreEqual(hidden.ActiveDuration, control.Diagnostics.ActiveDuration)

                            secondParent.Controls.Add(branch)
                            Assert.IsTrue(control.Visible)
                            Using surface As New Bitmap(32, 32)
                                Using graphics As Graphics = Graphics.FromImage(surface)
                                    InvokeTimerTick(control)
                                    control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                                End Using
                            End Using
                            Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                            Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)

                            secondParent.Visible = False
                            Dim reparentedHidden As AniGifDiagnostics = control.Diagnostics
                            InvokeTimerTick(control)
                            Assert.AreEqual(reparentedHidden.ActiveDuration, control.Diagnostics.ActiveDuration)

                            control.DiagnosticsEnabled = False
                            Dim ancestorsField As FieldInfo = GetType(AniGif).GetField("_diagnosticsAncestors", BindingFlags.Instance Or BindingFlags.NonPublic)
                            Assert.IsNotNull(ancestorsField)
                            Dim ancestors As List(Of Control) = DirectCast(ancestorsField.GetValue(control), List(Of Control))
                            Assert.IsNotNull(ancestors)
                            Assert.IsEmpty(ancestors)
                            secondParent.Visible = True
                            Assert.AreEqual(reparentedHidden.ActiveDuration, control.Diagnostics.ActiveDuration)
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_HandleDestroyedHandlerCannotReactivateMeasurement()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()
                InvokeTimerTick(control)
                Dim beforePaint As AniGifDiagnostics = Nothing
                Dim afterPaint As AniGifDiagnostics = Nothing

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim handleDestroyedHandler As EventHandler = Sub(sender, e)
                                                                         beforePaint = control.Diagnostics
                                                                         control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                                                                         afterPaint = control.Diagnostics
                                                                     End Sub
                        AddHandler control.HandleDestroyed, handleDestroyedHandler
                        control.DestroyHandleForTest()
                        RemoveHandler control.HandleDestroyed, handleDestroyedHandler
                    End Using
                End Using
                Assert.IsNotNull(beforePaint)
                Assert.IsNotNull(afterPaint)
                Assert.AreEqual(beforePaint.ActiveDuration, afterPaint.ActiveDuration)
                Assert.AreEqual(0L, afterPaint.RenderedFrames)
                Assert.AreEqual(1L, afterPaint.RenderOperations)
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_PaintHandlerCanDisposeTheControl()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()
                InvokeTimerTick(control)
                AddHandler control.Paint, Sub(sender, e) control.Dispose()
                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                    End Using
                End Using
                Assert.IsTrue(control.IsDisposed)
                Assert.IsFalse(control.Diagnostics.IsEnabled)
                Assert.AreEqual(0L, control.Diagnostics.RenderedFrames)
                Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                Assert.AreEqual(0L, control.Diagnostics.RenderOperations)
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_RecordsRealTimerTicksInAWinFormsMessageLoop()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.CustomDisplaySpeed = True
                control.FramesPerSecond = 20D
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Using watchdog As New System.Windows.Forms.Timer()
                            Dim ticks As Integer = 0
                            Dim timedOut As Boolean = False
                            AddHandler GetTimer(control).Tick, Sub(sender, e)
                                                                   ticks += 1
                                                                   control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                                                                   If ticks = 2 Then Application.ExitThread()
                                                               End Sub
                            watchdog.Interval = 5000
                            AddHandler watchdog.Tick, Sub(sender, e)
                                                          timedOut = True
                                                          Application.ExitThread()
                                                      End Sub
                            control.StartAnimation()
                            watchdog.Start()
                            Try
                                Application.Run()
                            Finally
                                watchdog.Stop()
                                control.StopAnimation()
                            End Try

                            Assert.IsFalse(timedOut, "Die Diagnostik hat nicht zwei Timer-Ticks verarbeitet.")
                            Assert.AreEqual(2, ticks)
                            Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                            Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                            Assert.IsGreaterThan(1L, control.Diagnostics.RenderOperations)
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_ReentrantPaintAndInvalidatedHandlersDoNotMisattributeFrames()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()
                Dim advancedDuringPaint As Boolean = False
                Dim paintHandler As PaintEventHandler = Sub(sender, e)
                                                            If Not advancedDuringPaint Then
                                                                advancedDuringPaint = True
                                                                InvokeTimerTick(control)
                                                            End If
                                                        End Sub
                AddHandler control.Paint, paintHandler

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim bounds As New Rectangle(Point.Empty, surface.Size)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        RemoveHandler control.Paint, paintHandler
                        Assert.IsTrue(advancedDuringPaint)
                        Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(1L, control.Diagnostics.DroppedFrames)

                        Dim soughtDuringInvalidation As Boolean = False
                        Dim invalidatedHandler As InvalidateEventHandler = Sub(sender, e)
                                                                               If Not soughtDuringInvalidation Then
                                                                                   soughtDuringInvalidation = True
                                                                                   control.SeekFrame(4)
                                                                               End If
                                                                           End Sub
                        AddHandler control.Invalidated, invalidatedHandler
                        InvokeTimerTick(control)
                        RemoveHandler control.Invalidated, invalidatedHandler
                        Assert.IsTrue(soughtDuringInvalidation)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(1L, control.Diagnostics.DroppedFrames)
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_RequiresAnIntersectingClipAndTracksFrameGenerationsAcrossWrap()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim bounds As New Rectangle(Point.Empty, surface.Size)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, Rectangle.Empty)
                        graphics.SetClip(Rectangle.Empty)
                        control.PaintTo(graphics, bounds)
                        graphics.ResetClip()
                        Assert.AreEqual(0L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(2L, control.Diagnostics.RenderOperations)

                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                        Dim firstFrame As Integer = GetCurrentFrame(control)
                        Dim dimension As New FrameDimension(control.Gif.FrameDimensionsList(0))
                        Dim frameCount As Integer = control.Gif.GetFrameCount(dimension)
                        For index As Integer = 1 To frameCount
                            InvokeTimerTick(control)
                        Next
                        Assert.AreEqual(firstFrame, GetCurrentFrame(control))
                        Assert.AreEqual(CLng(frameCount - 1), control.Diagnostics.DroppedFrames)
                        control.PaintTo(graphics, bounds)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(CLng(frameCount - 1), control.Diagnostics.DroppedFrames)
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub Diagnostics_ResetWhilePlayingAndTimingChangesPreservePlayback()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim events As New List(Of String)()
                AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                control.StartAnimation()

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim bounds As New Rectangle(Point.Empty, surface.Size)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        InvokeTimerTick(control)
                        control.ResetDiagnostics()
                        Assert.AreEqual(0L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.RenderOperations)
                        Assert.IsTrue(GetTimer(control).Enabled)
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())

                        InvokeTimerTick(control)
                        control.FramesPerSecond = 15D
                        control.CustomDisplaySpeed = True
                        control.MinimumFrameIntervalMs = 15
                        control.MaximumFrameIntervalMs = 600
                        control.AdaptiveTiming = True
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                        Assert.AreEqual(1L, control.Diagnostics.RenderOperations)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
                control.StopAnimation()
                CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub Diagnostics_SeekImageChangesResetAndDisposePreserveTheirContracts()
            Using control As New PaintProbeAniGif()
                control.ClientSize = New Size(32, 32)
                control.DiagnosticsEnabled = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.StartAnimation()

                Using surface As New Bitmap(32, 32)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Dim bounds As New Rectangle(Point.Empty, surface.Size)
                        InvokeTimerTick(control)
                        control.SeekFrame(3)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)

                        InvokeTimerTick(control)
                        Using source As Bitmap = LoadAnimatedGif()
                            control.Gif = source
                        End Using
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)

                        control.DiagnosticsEnabled = False
                        Dim frozen As AniGifDiagnostics = control.Diagnostics
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        Assert.AreEqual(frozen.ActiveDuration, control.Diagnostics.ActiveDuration)
                        Assert.AreEqual(frozen.RenderOperations, control.Diagnostics.RenderOperations)
                        Assert.AreEqual(frozen.RenderedFrames, control.Diagnostics.RenderedFrames)

                        control.DiagnosticsEnabled = True
                        Assert.AreEqual(0L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.RenderOperations)
                        InvokeTimerTick(control)
                        control.PaintTo(graphics, bounds)
                        InvokeTimerTick(control)
                        control.Dispose()

                        Dim disposed As AniGifDiagnostics = control.Diagnostics
                        Assert.IsFalse(disposed.IsEnabled)
                        Assert.AreEqual(1L, disposed.RenderedFrames)
                        Assert.AreEqual(0L, disposed.DroppedFrames)
                        Assert.AreEqual(disposed.ActiveDuration, control.Diagnostics.ActiveDuration)
                        Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.DiagnosticsEnabled = True)
                        control.ResetDiagnostics()
                        Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(2L, frozen.RenderedFrames)
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub Diagnostics_StaticImageHasRenderTimesButNoPlaybackRate()
            Using control As New PaintProbeAniGif()
                Using stillImage As New Bitmap(8, 8)
                    control.Gif = stillImage
                    control.ClientSize = New Size(32, 32)
                    control.DiagnosticsEnabled = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Dim notifications As Integer = 0
                    AddHandler control.NoAnimation, Sub(sender, e) notifications += 1
                    control.StartAnimation()
                    InvokeTimerTick(control)
                    Using surface As New Bitmap(32, 32)
                        Using graphics As Graphics = Graphics.FromImage(surface)
                            control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                        End Using
                    End Using

                    Dim snapshot As AniGifDiagnostics = control.Diagnostics
                    Assert.AreEqual(1, notifications)
                    Assert.IsFalse(GetTimer(control).Enabled)
                    Assert.AreEqual(TimeSpan.Zero, snapshot.ActiveDuration)
                    Assert.IsFalse(snapshot.ActualFps.HasValue)
                    Assert.AreEqual(0L, snapshot.RenderedFrames)
                    Assert.AreEqual(0L, snapshot.DroppedFrames)
                    Assert.AreEqual(1L, snapshot.RenderOperations)
                    Assert.IsTrue(snapshot.AverageRenderTimeMs.HasValue)
                    Assert.IsTrue(snapshot.MaxRenderTimeMs.HasValue)
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub Dispose_StopsPlaybackAndReleasesResourcesInBothSpeedModes()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using control As New AniGif()
                    control.CustomDisplaySpeed = customSpeed
                    Dim ownedImage As Bitmap = control.Gif
                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                    Dim timerDisposedCount As Integer = 0
                    Dim events As New List(Of String)()
                    AddHandler timer.Disposed, Sub(sender, e) timerDisposedCount += 1
                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                    control.StartAnimation()
                    InvokeTimerTick(control)
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    control.Dispose()
                    Assert.IsTrue(control.IsDisposed)
                    Assert.IsFalse(timer.Enabled)
                    Assert.AreEqual(1, timerDisposedCount)
                    Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    Assert.IsNull(control.Gif)
                    Assert.ThrowsExactly(Of ArgumentException)(Sub() ownedImage.GetPixel(0, 0))

                    InvokeTimerTick(control)
                    control.StartAnimation()
                    control.StopAnimation()
                    control.Dispose()
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    Assert.AreEqual(1, timerDisposedCount)
                    CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                End Using
            Next
        End Sub

        Private Shared Function ExpectedFrameInvalidation(mode As ImageSizeMode) As Rectangle
            Select Case mode
                Case ImageSizeMode.Normal
                    Return New Rectangle(0, 0, 34, 34)
                Case ImageSizeMode.CenterImage
                    Return New Rectangle(62, 42, 36, 36)
                Case ImageSizeMode.Zoom
                    Return New Rectangle(48, 28, 64, 64)
                Case ImageSizeMode.Fill
                    Return New Rectangle(0, 0, 160, 120)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(mode))
            End Select
        End Function

        <TestMethod>
        Public Sub ExplicitPause_SurvivesVisibilityAndTimingChanges()
            Using parent As New Panel()
                Using control As New AniGif()
                    parent.Controls.Add(control)
                    control.StartAnimation()
                    control.SeekFrame(2)
                    control.PauseAnimation()

                    parent.Visible = False
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    control.MinimumFrameIntervalMs = 30
                    control.MaximumFrameIntervalMs = 300
                    control.AdaptiveTiming = True
                    parent.Visible = True
                    InvokeTimerTick(control)
                    Assert.AreEqual(2, GetCurrentFrame(control))
                    Assert.IsFalse(GetTimer(control).Enabled)

                    control.ResumeAnimation()
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.AreEqual(100, GetTimer(control).Interval)
                End Using
            End Using
        End Sub

        Private Shared Sub FillFrameBackground(surface As Bitmap, bounds As Rectangle, color As Color)
            Using graphics As Graphics = Graphics.FromImage(surface)
                Using brush As New SolidBrush(color)
                    graphics.FillRectangle(brush, bounds)
                End Using
            End Using
        End Sub

        Private Shared Function FindSolutionRoot(startDirectory As String) As String
            Dim current As New DirectoryInfo(startDirectory)

            While current IsNot Nothing
                Dim solutionPath As String = Path.Combine(current.FullName, "SchlumpfSoftControlsNET.sln")
                If File.Exists(solutionPath) Then
                    Return current.FullName
                End If

                current = current.Parent
            End While

            Assert.Fail("Die Solution konnte ausgehend vom Test-Ausgabeverzeichnis nicht gefunden werden.")
            Return String.Empty
        End Function

        <TestMethod>
        Public Sub FrameDelayCache_ImageReplacementAndDisposalResetCachedData()
            Using source As Bitmap = LoadAnimatedGif()
                Using stillImage As New Bitmap(2, 2)
                    Using control As New AniGif()
                        Dim exposedImage As Bitmap = control.Gif
                        exposedImage.RemovePropertyItem(&H5100)
                        control.FramesPerSecond = 20D
                        Assert.AreEqual(50, GetFrameDelays(control)(0))

                        control.Gif = source
                        Assert.AreEqual(100, GetFrameDelays(control)(0))
                        Dim sourceProperty As PropertyItem = source.GetPropertyItem(&H5100)
                        SetFirstFrameDelay(source, sourceProperty, 30)
                        Dim internalImage As Bitmap = GetGifWithoutExposure(control)
                        Assert.AreEqual(10, BitConverter.ToInt32(internalImage.GetPropertyItem(&H5100).Value, 0))
                        internalImage.RemovePropertyItem(&H5100)
                        control.FramesPerSecond = 25D
                        Assert.AreEqual(100, GetFrameDelays(control)(0))

                        control.Gif = stillImage
                        CollectionAssert.AreEqual(Array.Empty(Of Integer)(), GetFrameDelays(control))
                        CollectionAssert.AreEqual(Array.Empty(Of Integer)(), GetRawFrameDelays(control))
                        control.Gif = Nothing
                        Assert.AreEqual(100, GetFrameDelays(control)(0))
                        Assert.IsGreaterThan(0, GetRawFrameDelays(control).Length)

                        control.Dispose()
                        Assert.IsNull(control.Gif)
                        CollectionAssert.AreEqual(Array.Empty(Of Integer)(), GetFrameDelays(control))
                        CollectionAssert.AreEqual(Array.Empty(Of Integer)(), GetRawFrameDelays(control))
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub FrameDelayCache_MetadataChangeIsDetectedWhenRoundedFallbackIsUnchanged()
            Using control As New AniGif()
                Dim image As Bitmap = control.Gif
                Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                SetFirstFrameDelay(image, delayProperty, 20)
                control.FramesPerSecond = 30.1D
                Assert.AreEqual(200, GetFrameDelays(control)(0))
            End Using
        End Sub

        <TestMethod>
        Public Sub FrameDelayCache_ReentrantGetterAfterImageReplacementTracksExposure()
            Using source As Bitmap = LoadAnimatedGif()
                Using control As New AniGif()
                    Dim handle As IntPtr = control.Handle
                    Dim invalidationCount As Integer = 0
                    AddHandler control.Invalidated, Sub(sender, e)
                                                        invalidationCount += 1
                                                        Dim image As Bitmap = control.Gif
                                                        Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                                                        SetFirstFrameDelay(image, delayProperty, 20)
                                                    End Sub

                    control.Gif = source
                    Assert.IsGreaterThan(0, invalidationCount)
                    Assert.AreEqual(100, GetFrameDelays(control)(0))
                    control.FramesPerSecond = 25D
                    Assert.AreEqual(200, GetFrameDelays(control)(0))
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub FrameDelayCache_UnexposedAndExposedImagesReuseStorage()
            Using control As New AniGif()
                Dim normalizedDelays As Integer() = GetFrameDelays(control)
                Dim rawDelays As Integer() = GetRawFrameDelays(control)
                Dim internalImage As Bitmap = GetGifWithoutExposure(control)
                ' Der private Zugriff simuliert eine Änderung, ohne den öffentlichen Getter auszulösen.
                internalImage.RemovePropertyItem(&H5100)

                control.FramesPerSecond = 25D
                Assert.AreEqual(100, GetFrameDelays(control)(0))
                control.MinimumFrameIntervalMs = 120
                Assert.AreEqual(120, GetFrameDelays(control)(0))
                control.MaximumFrameIntervalMs = 300
                control.MinimumFrameIntervalMs = 20
                Assert.AreEqual(100, GetFrameDelays(control)(0))
                Assert.AreSame(normalizedDelays, GetFrameDelays(control))
                Assert.AreSame(rawDelays, GetRawFrameDelays(control))

                Assert.AreSame(internalImage, control.Gif)
                control.FramesPerSecond = 20D
                Assert.AreEqual(50, GetFrameDelays(control)(0))
                control.MaximumFrameIntervalMs = 400
                Assert.AreEqual(50, GetFrameDelays(control)(0))
                Assert.AreSame(normalizedDelays, GetFrameDelays(control))
                Assert.AreSame(rawDelays, GetRawFrameDelays(control))
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub FrameDelayMetadata_ExternalMutationsUseExistingRefreshPoints()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.CustomDisplaySpeed = customSpeed
                        control.FramesPerSecond = 10D
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        timer.ResetCounts()

                        Dim image As Bitmap = control.Gif
                        Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                        SetFirstFrameDelay(image, delayProperty, 20)
                        control.FramesPerSecond = 10D
                        control.MinimumFrameIntervalMs = 20
                        control.MaximumFrameIntervalMs = 500
                        control.CustomDisplaySpeed = Not customSpeed
                        control.CustomDisplaySpeed = customSpeed
                        Assert.AreEqual(100, GetFrameDelays(control)(0))

                        control.MinimumFrameIntervalMs = 21
                        Assert.AreEqual(200, GetFrameDelays(control)(0))
                        Assert.AreEqual(If(customSpeed, 100, 200), timer.Interval)

                        SetFirstFrameDelay(image, delayProperty, 30)
                        control.FramesPerSecond = 25D
                        Assert.AreEqual(300, GetFrameDelays(control)(0))
                        Assert.AreEqual(If(customSpeed, 40, 300), timer.Interval)

                        SetFirstFrameDelay(image, delayProperty, 0)
                        control.MaximumFrameIntervalMs = 400
                        Assert.AreEqual(40, GetFrameDelays(control)(0))
                        Assert.AreEqual(40, timer.Interval)

                        image.RemovePropertyItem(&H5100)
                        control.FramesPerSecond = 20D
                        Assert.AreEqual(50, GetFrameDelays(control)(0))
                        Assert.AreEqual(50, timer.Interval)

                        SetFirstFrameDelay(image, delayProperty, 40)
                        control.MinimumFrameIntervalMs = 22
                        Assert.AreEqual(400, GetFrameDelays(control)(0))
                        Assert.AreEqual(If(customSpeed, 50, 400), timer.Interval)
                        control.CustomDisplaySpeed = Not customSpeed
                        Assert.AreEqual(If(customSpeed, 400, 50), timer.Interval)
                        Assert.IsTrue(timer.Enabled)
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub FrameDelayMetadata_NormalizesRawValuesAndRestoresWidenedBounds()
            Using control As New AniGif()
                Dim image As Bitmap = control.Gif
                Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                Dim delayBytes As Byte() = CreateDelayBytes(10, 0, 50)
                Array.Resize(delayBytes, delayBytes.Length + 2)
                delayBytes(delayBytes.Length - 2) = 1
                delayBytes(delayBytes.Length - 1) = 2
                SetDelayBytes(image, delayProperty, delayBytes)

                control.FramesPerSecond = 25D
                Dim delays As Integer() = GetFrameDelays(control)
                Assert.IsGreaterThan(3, delays.Length)
                Assert.AreEqual(100, delays(0))
                Assert.AreEqual(40, delays(1))
                Assert.AreEqual(500, delays(2))
                For i As Integer = 3 To delays.Length - 1
                    Assert.AreEqual(40, delays(i))
                Next

                control.MinimumFrameIntervalMs = 50
                control.MaximumFrameIntervalMs = 300
                delays = GetFrameDelays(control)
                Assert.AreEqual(100, delays(0))
                Assert.AreEqual(50, delays(1))
                Assert.AreEqual(300, delays(2))

                control.MaximumFrameIntervalMs = 500
                control.MinimumFrameIntervalMs = 20
                delays = GetFrameDelays(control)
                Assert.AreEqual(100, delays(0))
                Assert.AreEqual(40, delays(1))
                Assert.AreEqual(500, delays(2))

                Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, delayBytes, 4, 4)
                SetDelayBytes(image, delayProperty, delayBytes)
                control.FramesPerSecond = 16D
                Assert.AreEqual(63, GetFrameDelays(control)(1))
            End Using
        End Sub

        <TestMethod>
        Public Sub FrameDelayMetadata_OverflowDoesNotPublishPartialValues()
            Using control As New AniGif()
                Dim image As Bitmap = control.Gif
                Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                Dim oldDelays As Integer() = GetFrameDelays(control)
                Dim oldValues As Integer() = DirectCast(oldDelays.Clone(), Integer())
                Buffer.BlockCopy(BitConverter.GetBytes(20), 0, delayProperty.Value, 0, 4)
                Buffer.BlockCopy(BitConverter.GetBytes(Integer.MaxValue), 0, delayProperty.Value, 4, 4)
                image.SetPropertyItem(delayProperty)

                Dim unused1 = Assert.ThrowsExactly(Of OverflowException)(Sub() control.FramesPerSecond = 20D)
                Assert.AreEqual(20D, control.FramesPerSecond)
                Assert.AreSame(oldDelays, GetFrameDelays(control))
                CollectionAssert.AreEqual(oldValues, GetFrameDelays(control))
                Dim unused = Assert.ThrowsExactly(Of OverflowException)(Sub() control.MaximumFrameIntervalMs = 400)
                Assert.AreSame(oldDelays, GetFrameDelays(control))
                CollectionAssert.AreEqual(oldValues, GetFrameDelays(control))

                Buffer.BlockCopy(BitConverter.GetBytes(30), 0, delayProperty.Value, 4, 4)
                image.SetPropertyItem(delayProperty)
                control.FramesPerSecond = 25D
                Assert.AreEqual(200, GetFrameDelays(control)(0))
                Assert.AreEqual(300, GetFrameDelays(control)(1))
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub FrameDelayMetadata_ShortBlocksUseFallbackAndExtraValuesAreIgnored()
            Using control As New AniGif()
                Dim image As Bitmap = control.Gif
                Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                For length As Integer = 1 To 3
                    SetDelayBytes(image, delayProperty, New Byte(length - 1) {})
                    control.FramesPerSecond = If(length = 2, 25D, 20D)
                    Dim expected As Integer = If(length = 2, 40, 50)
                    For Each delay As Integer In GetFrameDelays(control)
                        Assert.AreEqual(expected, delay)
                    Next
                Next

                Dim frameCount As Integer = GetFrameDelays(control).Length
                Dim values(frameCount) As Integer
                For i As Integer = 0 To frameCount - 1
                    values(i) = 10
                Next
                values(frameCount) = Integer.MaxValue
                SetDelayBytes(image, delayProperty, CreateDelayBytes(values))
                control.FramesPerSecond = 30D
                For Each delay As Integer In GetFrameDelays(control)
                    Assert.AreEqual(100, delay)
                Next
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub FramesPerSecond_EquivalentValuesDoNotRefreshOrRestart()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.CustomDisplaySpeed = customSpeed
                        control.FramesPerSecond = 50D
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()

                        For Each boundary As Decimal In New Decimal() {50D, 1D}
                            control.FramesPerSecond = boundary
                            Dim delays As Integer() = GetFrameDelays(control)
                            Dim interval As Integer = timer.Interval
                            Dim frame As Integer = GetCurrentFrame(control)
                            SetLastFrameTimestamp(control, 123L)
                            timer.ResetCounts()

                            control.FramesPerSecond = boundary
                            control.FramesPerSecond = If(boundary = 50D, 80D, 0D)
                            control.FramesPerSecond = If(boundary = 50D, 100D, -10D)

                            Assert.AreEqual(boundary, control.FramesPerSecond)
                            Assert.AreSame(delays, GetFrameDelays(control))
                            Assert.AreEqual(interval, timer.Interval)
                            Assert.AreEqual(frame, GetCurrentFrame(control))
                            Assert.AreEqual(123L, GetLastFrameTimestamp(control))
                            Assert.IsTrue(timer.Enabled)
                            Assert.AreEqual(0, timer.StartCalls)
                            Assert.AreEqual(0, timer.StopCalls)
                            CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                        Next
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub FramesPerSecond_IsClampedToValidRange()
            Using control As New AniGif()
                control.FramesPerSecond = 0D
                Assert.AreEqual(1D, control.FramesPerSecond)

                control.FramesPerSecond = 80D
                Assert.AreEqual(50D, control.FramesPerSecond)
            End Using
        End Sub

        Private Shared Function GetCurrentFrame(control As AniGif) As Integer
            Dim frameField As FieldInfo = GetType(AniGif).GetField("_Frame", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(frameField)

            Return CInt(frameField.GetValue(control))
        End Function

        Private Shared Function GetDiagnosticsCollectorSnapshot(collector As Object, isEnabled As Boolean, timestamp As Long) As AniGifDiagnostics
            Dim method As MethodInfo = collector.GetType().GetMethod("CreateSnapshot", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Return DirectCast(method.Invoke(collector, New Object() {isEnabled, timestamp}), AniGifDiagnostics)
        End Function

        Private Shared Function GetDrawingRectangle(control As AniGif) As Rectangle
            Dim sizeMethod As MethodInfo = GetGeometryMethod("GetRectStartSize")
            Dim pointMethod As MethodInfo = GetGeometryMethod("GetRectStartPoint")
            Dim size As Size = DirectCast(sizeMethod.Invoke(Nothing, New Object() {control.GifSizeMode, control, control.Gif, control.ZoomFactor / 100D}), Size)
            Dim point As Point = DirectCast(pointMethod.Invoke(Nothing, New Object() {control.GifSizeMode, control, control.Gif, size}), Point)
            Return New Rectangle(point, size)
        End Function

        Private Shared Function GetFrameDelays(control As AniGif) As Integer()
            Dim delayField As FieldInfo = GetType(AniGif).GetField("_NormalizedFrameDelaysMs", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(delayField)
            Return DirectCast(delayField.GetValue(control), Integer())
        End Function

        Private Shared Function GetGeometryMethod(name As String) As MethodInfo
            Dim helperType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".FunctionDefinitions")
            Assert.IsNotNull(helperType)
            Dim method As MethodInfo = helperType.GetMethod(name, BindingFlags.Static Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Return method
        End Function

        Private Shared Function GetGifWithoutExposure(control As AniGif) As Bitmap
            Dim imageField As FieldInfo = GetType(AniGif).GetField("_Gif", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(imageField)
            Return DirectCast(imageField.GetValue(control), Bitmap)
        End Function

        Private Shared Function GetLastFrameTimestamp(control As AniGif) As Long
            Dim timestampField As FieldInfo = GetType(AniGif).GetField("_LastFrameTickTimestamp", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timestampField)
            Return CLng(timestampField.GetValue(control))
        End Function

        Private Shared Function GetPendingDiagnosticsGeneration(collector As Object) As Long
            Dim propertyInfo As PropertyInfo = collector.GetType().GetProperty("PendingFrameGeneration", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(propertyInfo)
            Return CLng(propertyInfo.GetValue(collector))
        End Function

        <TestMethod>
        Public Sub GetPlaybackState_ReflectsPlaybackTransitions()
            Using control As New AniGif()
                Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())

                control.StartAnimation()
                Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())

                control.PauseAnimation()
                Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())

                control.ResumeAnimation()
                Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())

                control.StopAnimation()
                Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
            End Using
        End Sub

        <TestMethod>
        Public Sub GetPlaybackState_RemainsPlayingWhileHidden()
            Using control As New AniGif()
                control.StartAnimation()
                Assert.IsTrue(GetTimer(control).Enabled)

                ' Der gewünschte Wiedergabezustand bleibt trotz angehaltenem Timer erhalten.
                control.Visible = False
                Assert.IsFalse(GetTimer(control).Enabled)
                Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())

                control.Visible = True
                Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
            End Using
        End Sub

        <TestMethod>
        Public Sub GetPlaybackState_ReturnsStoppedAfterDisposal()
            Using control As New AniGif()
                control.StartAnimation()
                control.PauseAnimation()
                Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())

                control.Dispose()
                Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
            End Using
        End Sub

        Private Shared Function GetRawFrameDelays(control As AniGif) As Integer()
            Dim delayField As FieldInfo = GetType(AniGif).GetField("_rawFrameDelaysMs", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(delayField)
            Return DirectCast(delayField.GetValue(control), Integer())
        End Function

        Private Shared Function GetTimer(control As AniGif) As System.Windows.Forms.Timer
            Dim timerProperty As PropertyInfo = GetType(AniGif).GetProperty("Timer", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerProperty)
            Return DirectCast(timerProperty.GetValue(control), System.Windows.Forms.Timer)
        End Function

        <TestMethod>
        Public Sub GifReplacement_PreservesPauseAndResetsTheFrame()
            Using source As Bitmap = LoadAnimatedGif()
                Using control As New AniGif()
                    control.StartAnimation()
                    control.SeekFrame(3)
                    control.PauseAnimation()
                    control.Gif = source
                    Assert.AreEqual(0, GetCurrentFrame(control))
                    Assert.IsFalse(GetTimer(control).Enabled)

                    control.Gif = Nothing
                    InvokeTimerTick(control)
                    Assert.AreEqual(0, GetCurrentFrame(control))
                    Assert.IsFalse(GetTimer(control).Enabled)
                    control.ResumeAnimation()
                    Assert.IsTrue(GetTimer(control).Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub GifReplacement_PreservesPlaybackAndReleasesImagesInBothSpeedModes()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using source As Bitmap = LoadAnimatedGif()
                    Using control As New AniGif()
                        control.CustomDisplaySpeed = customSpeed
                        Dim sourceWidth As Integer = source.Width
                        Dim previousImage As Bitmap = control.Gif
                        Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                        control.StartAnimation()
                        InvokeTimerTick(control)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        control.Gif = source
                        Assert.ThrowsExactly(Of ArgumentException)(Sub() previousImage.GetPixel(0, 0))
                        Assert.AreNotSame(source, control.Gif)
                        Assert.AreEqual(sourceWidth, source.Width)
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                        Assert.IsTrue(timer.Enabled)
                        CollectionAssert.AreEqual(New String() {"started", "started"}, events.ToArray())

                        control.SeekFrame(1)
                        control.PauseAnimation()
                        previousImage = control.Gif
                        control.Gif = source
                        Assert.ThrowsExactly(Of ArgumentException)(Sub() previousImage.GetPixel(0, 0))
                        Assert.AreEqual(sourceWidth, source.Width)
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())
                        Assert.IsFalse(timer.Enabled)
                        CollectionAssert.AreEqual(New String() {"started", "started", "stopped"}, events.ToArray())

                        control.ResumeAnimation()
                        Assert.IsTrue(timer.Enabled)
                        InvokeTimerTick(control)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"started", "started", "stopped", "started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub Gif_AfterDisposalRejectsAssignmentsWithoutTakingOwnership()
            Using source As New Bitmap(8, 8)
                source.SetPixel(0, 0, Color.Red)

                Using control As New AniGif()
                    control.Gif = source
                    Assert.AreNotSame(source, control.Gif)
                    source.SetPixel(0, 0, Color.Blue)
                    Assert.AreEqual(Color.Red.ToArgb(), control.Gif.GetPixel(0, 0).ToArgb())

                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                    Dim eventCount As Integer = 0
                    AddHandler control.AnimationStarted, Sub(sender, e) eventCount += 1
                    AddHandler control.AnimationStopped, Sub(sender, e) eventCount += 1
                    AddHandler control.NoAnimation, Sub(sender, e) eventCount += 1
                    AddHandler control.AutoPlayChanged, Sub(sender, e) eventCount += 1
                    AddHandler control.Invalidated, Sub(sender, e) eventCount += 1

                    control.Dispose()
                    Assert.IsTrue(control.IsDisposed)
                    Assert.IsNull(control.Gif)
                    Dim eventCountAfterDispose As Integer = eventCount

                    For attempt As Integer = 1 To 2
                        Dim exception As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = source)
                        Assert.AreEqual(NameOf(AniGif), exception.ObjectName)
                        Assert.IsNull(control.Gif)
                        Dim defaultException As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = Nothing)
                        Assert.AreEqual(NameOf(AniGif), defaultException.ObjectName)
                        Assert.IsNull(control.Gif)
                    Next

                    Assert.AreEqual(Color.Blue.ToArgb(), source.GetPixel(0, 0).ToArgb())
                    Assert.AreEqual(eventCountAfterDispose, eventCount)
                    Assert.IsFalse(timer.Enabled)
                    control.Dispose()
                    Assert.IsNull(control.Gif)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Gif_AfterDisposalRejectsDisposedBitmapBeforeCloning()
            Using source As New Bitmap(8, 8)
                Using control As New AniGif()
                    source.Dispose()
                    control.Dispose()

                    Dim exception As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = source)
                    Assert.AreEqual(NameOf(AniGif), exception.ObjectName)
                    Assert.IsNull(control.Gif)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Gif_AfterDisposalRejectsNothingWithoutRecreatingTheDefaultImage()
            Using control As New AniGif()
                control.Dispose()
                Assert.IsNull(control.Gif)

                For attempt As Integer = 1 To 2
                    Dim exception As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = Nothing)
                    Assert.AreEqual(NameOf(AniGif), exception.ObjectName)
                    Assert.IsNull(control.Gif)
                Next

                control.Dispose()
                Assert.IsNull(control.Gif)
                Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
            End Using
        End Sub

        <TestMethod>
        Public Sub Gif_WhileDisposingRejectsAssignmentsBeforeTheBaseControlIsDisposed()
            Using source As New Bitmap(8, 8)
                Using control As New AniGif()
                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                    Dim timerDisposedCount As Integer = 0

                    AddHandler timer.Disposed, Sub(sender, e)
                                                   timerDisposedCount += 1
                                                   Assert.IsFalse(control.IsDisposed)
                                                   Assert.IsNull(control.Gif)

                                                   Dim bitmapException As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = source)
                                                   Assert.AreEqual(NameOf(AniGif), bitmapException.ObjectName)
                                                   Dim defaultException As ObjectDisposedException = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.Gif = Nothing)
                                                   Assert.AreEqual(NameOf(AniGif), defaultException.ObjectName)
                                                   Assert.IsNull(control.Gif)
                                               End Sub

                    control.Dispose()
                    Assert.AreEqual(1, timerDisposedCount)
                    Assert.IsTrue(control.IsDisposed)
                    Assert.IsNull(control.Gif)
                    Assert.AreEqual(8, source.Width)
                End Using
            End Using
        End Sub

        Private Shared Sub InstallTrackingTimer(control As AniGif, timer As System.Windows.Forms.Timer)
            Dim timerProperty As PropertyInfo = GetType(AniGif).GetProperty("Timer", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerProperty)
            timer.Interval = GetTimer(control).Interval
            timerProperty.SetValue(control, timer)
        End Sub

        Private Shared Function InvokeAdaptiveTiming(control As AniGif, target As Integer, elapsed As Integer, scheduled As Integer) As Integer
            Dim method As MethodInfo = GetType(AniGif).GetMethod("ApplyAdaptiveTiming", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Return CInt(method.Invoke(control, New Object() {target, elapsed, scheduled}))
        End Function

        Private Shared Sub InvokeDiagnosticsCollector(collector As Object, name As String, ParamArray arguments As Object())
            Dim method As MethodInfo = collector.GetType().GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Dim unused = method.Invoke(collector, arguments)
        End Sub

        Private Shared Sub InvokeTimerTick(control As AniGif)
            Dim timerTickMethod As MethodInfo = GetType(AniGif).GetMethod("Timer_Tick", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerTickMethod)

            Dim unused = timerTickMethod.Invoke(control, New Object() {control, EventArgs.Empty})
        End Sub

        Private Shared Function LoadAnimatedGif() As Bitmap
            Dim solutionRoot As String = FindSolutionRoot(AppContext.BaseDirectory)
            Dim gifPath As String = Path.Combine(solutionRoot, "SchlumpfSoftControlsNET", "AniGifControl", "AniGifControl.Standard.gif")

            Assert.IsTrue(File.Exists(gifPath), $"Die Testdatei wurde nicht gefunden: {gifPath}")
            Return New Bitmap(gifPath)
        End Function

        <TestMethod>
        Public Sub NoAnimation_EquivalentFpsDoesNotNotifyButRealChangesDo()
            Using control As New AniGif()
                Using stillImage As New Bitmap(8, 8)
                    control.Gif = stillImage
                    Dim notifications As Integer = 0
                    AddHandler control.NoAnimation, Sub(sender, e) notifications += 1
                    control.StartAnimation()
                    Assert.AreEqual(1, notifications)

                    control.FramesPerSecond = 30D
                    Assert.AreEqual(1, notifications)
                    control.FramesPerSecond = 31D
                    Assert.AreEqual(2, notifications)
                    control.Visible = False
                    control.FramesPerSecond = 32D
                    Assert.AreEqual(2, notifications)
                    control.Visible = True
                    Assert.AreEqual(3, notifications)
                    control.PauseAnimation()
                    control.FramesPerSecond = 33D
                    Assert.AreEqual(3, notifications)
                    control.ResumeAnimation()
                    Assert.AreEqual(4, notifications)
                    Assert.IsFalse(GetTimer(control).Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub NoAnimation_HandlerCanAssignFpsReentrantly()
            Using control As New AniGif()
                Using stillImage As New Bitmap(8, 8)
                    control.Gif = stillImage
                    Dim notifications As Integer = 0
                    AddHandler control.NoAnimation, Sub(sender, e)
                                                        notifications += 1
                                                        control.FramesPerSecond = 12D
                                                    End Sub

                    control.StartAnimation()

                    Assert.AreEqual(2, notifications)
                    Assert.AreEqual(12D, control.FramesPerSecond)
                    Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                    Assert.IsFalse(GetTimer(control).Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub NoAnimation_HandlerCanReplaceImageAndStartPlayback()
            Using source As Bitmap = LoadAnimatedGif()
                Using stillImage As New Bitmap(8, 8)
                    Using control As New AniGif()
                        control.Gif = stillImage
                        Dim events As New List(Of String)()
                        AddHandler control.NoAnimation, Sub(sender, e)
                                                            events.Add("no-animation")
                                                            control.Gif = source
                                                        End Sub
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                        control.StartAnimation()

                        Assert.IsTrue(GetTimer(control).Enabled)
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"no-animation", "started"}, events.ToArray())
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub NoAnimation_IsRaisedForNonAnimatedImage()
            Using control As New AniGif()
                Using stillImage As New Bitmap(8, 8)
                    control.Gif = stillImage

                    Dim noAnimationCount As Integer = 0
                    AddHandler control.NoAnimation, Sub(sender, e) noAnimationCount += 1

                    control.StartAnimation()

                    Assert.AreEqual(1, noAnimationCount)
                End Using
            End Using
        End Sub

        Private Shared Sub PaintFullFrame(control As FrameInvalidationProbe)
            Using surface As New Bitmap(control.ClientSize.Width, control.ClientSize.Height)
                control.PaintFrame(surface, control.ClientRectangle)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_AncestorSubscriptionsFollowReparentingAndDetach()
            Using firstParent As New Panel()
                Using secondParent As New Panel()
                    Using branch As New FrameInvalidationScrollPanel()
                        Using control As New FrameInvalidationProbe()
                            Using background As Bitmap = CreateCheckerboard(8)
                                firstParent.Controls.Add(branch)
                                branch.Controls.Add(control)
                                control.ClientSize = New Size(160, 120)
                                control.UsePartialFrameInvalidation = True
                                control.DiagnosticsEnabled = True
                                Dim handle As IntPtr = control.Handle
                                Assert.AreNotEqual(IntPtr.Zero, handle)
                                PrimeFrameInvalidation(control)
                                Dim field As FieldInfo = GetType(AniGif).GetField("_frameInvalidationAncestors", BindingFlags.Instance Or BindingFlags.NonPublic)
                                Assert.IsNotNull(field)
                                Dim ancestors As List(Of Control) = DirectCast(field.GetValue(control), List(Of Control))
                                Assert.IsNotNull(ancestors)
                                Assert.HasCount(2, ancestors)
                                Assert.Contains(branch, ancestors)
                                Assert.Contains(firstParent, ancestors)
                                Dim diagnosticsField As FieldInfo = GetType(AniGif).GetField("_diagnosticsAncestors", BindingFlags.Instance Or BindingFlags.NonPublic)
                                Assert.IsNotNull(diagnosticsField)
                                Dim diagnosticsAncestors As List(Of Control) = DirectCast(diagnosticsField.GetValue(control), List(Of Control))
                                Assert.IsNotNull(diagnosticsAncestors)
                                Assert.HasCount(2, diagnosticsAncestors)
                                Dim invalidations As New List(Of Rectangle)()
                                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)

                                firstParent.BackColor = Color.Brown
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.BackgroundImage = background
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.BackgroundImageLayout = ImageLayout.Center
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.Location = New Point(7, 9)
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.Size = New Size(210, 160)
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.PerformLayout()
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                invalidations.Clear()
                                branch.RaiseScrollForFrameTest()
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                branch.Visible = False
                                Assert.IsFalse(control.Visible)
                                invalidations.Clear()
                                branch.Visible = True
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)

                                secondParent.Controls.Add(branch)
                                Assert.HasCount(2, ancestors)
                                Assert.Contains(branch, ancestors)
                                Assert.Contains(secondParent, ancestors)
                                Assert.DoesNotContain(firstParent, ancestors)
                                Assert.HasCount(2, diagnosticsAncestors)
                                Assert.Contains(secondParent, diagnosticsAncestors)
                                Assert.DoesNotContain(firstParent, diagnosticsAncestors)
                                invalidations.Clear()
                                firstParent.BackColor = Color.Yellow
                                Assert.IsEmpty(invalidations)
                                secondParent.BackColor = Color.Purple
                                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                                control.UsePartialFrameInvalidation = False
                                Assert.IsEmpty(ancestors)
                                Assert.HasCount(2, diagnosticsAncestors)
                                invalidations.Clear()
                                branch.RaiseScrollForFrameTest()
                                Assert.IsEmpty(invalidations)
                                control.UsePartialFrameInvalidation = True
                                Assert.HasCount(2, ancestors)
                                control.Dispose()
                                Assert.IsEmpty(ancestors)
                                Assert.IsEmpty(diagnosticsAncestors)
                                branch.BackgroundImage = Nothing
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_BackgroundImagesMatchFullRepaint()
            Using source As Bitmap = LoadAnimatedGif()
                Using background As Bitmap = CreateCheckerboard(8)
                    For Each layout As ImageLayout In New ImageLayout() {ImageLayout.Tile, ImageLayout.Center, ImageLayout.Stretch, ImageLayout.Zoom}
                        AssertFrameSequenceMatchesFull(source, New Size(160, 120), ImageSizeMode.CenterImage, 50D,
                            GifScalingQualityPreset.Photo, False, New Rectangle(62, 42, 36, 36), background, layout)
                    Next
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_BorderUsesClientCoordinatesAndForcesFullPaint()
            Using control As New FrameInvalidationProbe()
                control.BorderStyle = BorderStyle.FixedSingle
                control.ClientSize = New Size(160, 120)
                control.GifSizeMode = ImageSizeMode.CenterImage
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Assert.AreNotEqual(control.Width, control.ClientSize.Width)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                PrimeFrameInvalidation(control)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, New Rectangle(62, 42, 36, 36))

                control.BorderStyle = BorderStyle.Fixed3D
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
            End Using
        End Sub

        <TestMethod>
        Public Sub PartialFrameInvalidation_BoundsHandleEmptyAndLargeCoordinates()
            Dim method As MethodInfo = GetType(AniGif).GetMethod("GetFrameInvalidationBounds", BindingFlags.Static Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Dim client As New Rectangle(0, 0, 100, 100)
            Assert.AreEqual(New Rectangle(8, 18, 24, 34), DirectCast(method.Invoke(Nothing, New Object() {New Rectangle(10, 20, 20, 30), client}), Rectangle))
            Assert.AreEqual(Rectangle.Empty, DirectCast(method.Invoke(Nothing, New Object() {New Rectangle(10, 20, 0, 30), client}), Rectangle))
            Assert.AreEqual(Rectangle.Empty, DirectCast(method.Invoke(Nothing, New Object() {New Rectangle(Integer.MaxValue - 1, 0, 10, 10), client}), Rectangle))
            Assert.AreEqual(New Rectangle(0, 0, 1, 12), DirectCast(method.Invoke(Nothing, New Object() {New Rectangle(Integer.MinValue, 0, Integer.MaxValue, 10), client}), Rectangle))
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_CoalescedGeometryChangesClearOldPixels()
            Using baseline As New FrameInvalidationProbe()
                Using optimized As New FrameInvalidationProbe()
                    baseline.ClientSize = New Size(160, 120)
                    optimized.ClientSize = baseline.ClientSize
                    baseline.BackColor = Color.Magenta
                    optimized.BackColor = baseline.BackColor
                    optimized.UsePartialFrameInvalidation = True
                    Dim baselineHandle As IntPtr = baseline.Handle
                    Dim optimizedHandle As IntPtr = optimized.Handle
                    Assert.AreNotEqual(IntPtr.Zero, baselineHandle)
                    Assert.AreNotEqual(IntPtr.Zero, optimizedHandle)
                    Dim optimizedInvalidations As New List(Of Rectangle)()
                    AddHandler optimized.Invalidated, Sub(sender, e) optimizedInvalidations.Add(e.InvalidRect)
                    baseline.StartAnimation()
                    optimized.StartAnimation()
                    InvokeTimerTick(baseline)
                    InvokeTimerTick(optimized)
                    Using baselineSurface As New Bitmap(160, 120)
                        Using optimizedSurface As New Bitmap(160, 120)
                            baseline.PaintFrame(baselineSurface, baseline.ClientRectangle)
                            optimized.PaintFrame(optimizedSurface, optimized.ClientRectangle)
                            InvokeTimerTick(baseline)
                            InvokeTimerTick(optimized)
                            baseline.PaintFrame(baselineSurface, baseline.ClientRectangle)
                            optimized.PaintFrame(optimizedSurface, optimizedInvalidations(optimizedInvalidations.Count - 1))
                            AssertSamePixels(baselineSurface, optimizedSurface)

                            Dim found As Boolean = False
                            Dim oldPixel As Point = Point.Empty
                            For y As Integer = 0 To 31
                                For x As Integer = 0 To 31
                                    If optimizedSurface.GetPixel(x, y).ToArgb() <> Color.Magenta.ToArgb() Then
                                        oldPixel = New Point(x, y)
                                        found = True
                                        Exit For
                                    End If
                                Next
                                If found Then Exit For
                            Next
                            Assert.IsTrue(found, "Der alte Bildbereich muss sichtbare Pixel enthalten.")

                            baseline.GifSizeMode = ImageSizeMode.CenterImage
                            optimized.GifSizeMode = ImageSizeMode.CenterImage
                            baseline.ZoomFactor = 75D
                            optimized.ZoomFactor = 75D
                            baseline.GifSizeMode = ImageSizeMode.Zoom
                            optimized.GifSizeMode = ImageSizeMode.Zoom
                            AssertLastFrameInvalidation(optimizedInvalidations, optimized.ClientRectangle)
                            optimizedInvalidations.Clear()
                            InvokeTimerTick(baseline)
                            InvokeTimerTick(optimized)
                            AssertLastFrameInvalidation(optimizedInvalidations, optimized.ClientRectangle)
                            baseline.PaintFrame(baselineSurface, baseline.ClientRectangle)
                            optimized.PaintFrame(optimizedSurface, optimized.ClientRectangle)
                            AssertSamePixels(baselineSurface, optimizedSurface)
                            Assert.AreEqual(Color.Magenta.ToArgb(), optimizedSurface.GetPixel(oldPixel.X, oldPixel.Y).ToArgb())
                            optimizedInvalidations.Clear()
                            InvokeTimerTick(optimized)
                            AssertLastFrameInvalidation(optimizedInvalidations, New Rectangle(33, 13, 94, 94))
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_DefaultMetadataAndSwitchInvalidateFullClient()
            Using control As New AniGif()
                Assert.IsFalse(control.UsePartialFrameInvalidation)
                Dim descriptor As PropertyDescriptor = TypeDescriptor.GetProperties(control)(NameOf(AniGif.UsePartialFrameInvalidation))
                Assert.IsNotNull(descriptor)
                Assert.IsTrue(descriptor.IsBrowsable)
                Dim defaultValue As DefaultValueAttribute = DirectCast(descriptor.Attributes(GetType(DefaultValueAttribute)), DefaultValueAttribute)
                Assert.IsNotNull(defaultValue)
                Assert.IsFalse(CBool(defaultValue.Value))
                Assert.IsFalse(descriptor.ShouldSerializeValue(control))

                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                control.UsePartialFrameInvalidation = True
                Assert.HasCount(1, invalidations)
                Assert.AreEqual(control.ClientRectangle, invalidations(0))
                Assert.IsTrue(descriptor.ShouldSerializeValue(control))
                control.UsePartialFrameInvalidation = True
                Assert.HasCount(1, invalidations)
                descriptor.ResetValue(control)
                Assert.IsFalse(control.UsePartialFrameInvalidation)
                Assert.HasCount(2, invalidations)
                Assert.AreEqual(control.ClientRectangle, invalidations(1))
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_DefaultPaintHandlersRetainFullClientArea()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                Dim clips As New List(Of Rectangle)()
                Dim outsideColor As Color = Color.Red
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                AddHandler control.Paint, Sub(sender, e)
                                              clips.Add(e.ClipRectangle)
                                              Using brush As New SolidBrush(outsideColor)
                                                  e.Graphics.FillRectangle(brush, New Rectangle(100, 80, 10, 10))
                                              End Using
                                          End Sub
                control.StartAnimation()
                Using surface As New Bitmap(160, 120)
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                    Assert.AreEqual(Color.Red.ToArgb(), surface.GetPixel(105, 85).ToArgb())
                    Assert.AreEqual(control.ClientRectangle, clips(clips.Count - 1))

                    outsideColor = Color.Blue
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                    Assert.AreEqual(Color.Blue.ToArgb(), surface.GetPixel(105, 85).ToArgb())

                    control.UsePartialFrameInvalidation = True
                    control.PaintFrame(surface, control.ClientRectangle)
                    outsideColor = Color.Green
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 34, 34))
                    control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                    Assert.AreEqual(Color.Blue.ToArgb(), surface.GetPixel(105, 85).ToArgb())
                    Assert.AreEqual(New Rectangle(0, 0, 34, 34), clips(clips.Count - 1))

                    control.UsePartialFrameInvalidation = False
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    control.PaintFrame(surface, control.ClientRectangle)
                    Assert.AreEqual(Color.Green.ToArgb(), surface.GetPixel(105, 85).ToArgb())
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_DisposalDoesNotLeaveAncestorEventReferences()
            Using parent As New Panel()
                Dim released As WeakReference = CreateDisposedFrameControl(parent)
                GC.Collect()
                GC.WaitForPendingFinalizers()
                GC.Collect()
                Assert.IsFalse(released.IsAlive)
                GC.KeepAlive(parent)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_EarlyGifExposureDoesNotInvalidateImmediately()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                Dim image As Bitmap = control.Gif
                Assert.IsNotNull(image)
                Assert.IsEmpty(invalidations)
                control.StartAnimation()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                PaintFullFrame(control)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ExposedBitmapForcesFullUntilReplacement()
            Dim root As String = FindSolutionRoot(AppContext.BaseDirectory)
            Dim gifPath As String = Path.Combine(root, "SchlumpfsoftControlsNETDemo", "Resources", "AniGif_Anim108.gif")
            Using source As New Bitmap(gifPath)
                Using control As New FrameInvalidationProbe()
                    control.ClientSize = New Size(160, 120)
                    control.Gif = source
                    control.UsePartialFrameInvalidation = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Dim invalidations As New List(Of Rectangle)()
                    AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                    PrimeFrameInvalidation(control)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 29, 19))

                    invalidations.Clear()
                    Dim exposed As Bitmap = control.Gif
                    Assert.IsNotNull(exposed)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    Dim countAfterExposure As Integer = invalidations.Count
                    Assert.AreSame(exposed, control.Gif)
                    Assert.HasCount(countAfterExposure, invalidations)
                    exposed.RotateFlip(RotateFlipType.Rotate90FlipNone)
                    Assert.AreEqual(New Size(17, 27), exposed.Size)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)

                    control.Gif = source
                    PaintFullFrame(control)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 29, 19))
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_GeometryAndLifecycleChangesRequireFullPaint()
            Using control As New FrameInvalidationProbe()
                Using source As Bitmap = LoadAnimatedGif()
                    control.ClientSize = New Size(160, 120)
                    control.UsePartialFrameInvalidation = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Dim invalidations As New List(Of Rectangle)()
                    AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                    PrimeFrameInvalidation(control)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 34, 34))

                    invalidations.Clear()
                    control.GifSizeMode = ImageSizeMode.CenterImage
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(62, 42, 36, 36))

                    control.ZoomFactor = 75D
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)

                    control.ClientSize = New Size(180, 120)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    control.BackColor = Color.Aqua
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    control.Location = New Point(7, 9)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)

                    control.Gif = source
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    control.DestroyHandleForFrameTest()
                    handle = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    PaintFullFrame(control)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(72, 42, 36, 36))
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_GifExposureDuringInvalidationForcesFullArea()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                PrimeFrameInvalidation(control)
                Dim exposed As Bitmap = Nothing
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e)
                                                    invalidations.Add(e.InvalidRect)
                                                    If exposed Is Nothing AndAlso e.InvalidRect.Width < control.ClientSize.Width Then exposed = control.Gif
                                                End Sub
                InvokeTimerTick(control)
                Assert.IsNotNull(exposed)
                Assert.IsGreaterThan(1, invalidations.Count)
                Assert.AreEqual(New Rectangle(0, 0, 34, 34), invalidations(0))
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_GifGetterObservesReentrantDisposal()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                PrimeFrameInvalidation(control)
                InvokeTimerTick(control)
                AddHandler control.Invalidated, Sub(sender, e)
                                                    If e.InvalidRect = control.ClientRectangle Then control.Dispose()
                                                End Sub
                Dim image As Bitmap = control.Gif
                Assert.IsNull(image)
                Assert.IsTrue(control.IsDisposed)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_OverflowRemainsInPaintRatherThanInvalidation()
            Using source As New Bitmap(40000, 1)
                Using control As New PaintProbeAniGif()
                    control.ClientSize = New Size(1, 65535)
                    control.Gif = source
                    control.GifSizeMode = ImageSizeMode.Fill
                    control.UsePartialFrameInvalidation = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Assert.AreEqual(New Size(1, 65535), control.ClientSize)
                    Dim method As MethodInfo = GetType(AniGif).GetMethod("InvalidateIfVisible", BindingFlags.Instance Or BindingFlags.NonPublic)
                    Assert.IsNotNull(method)
                    Dim unused = method.Invoke(control, New Object() {False})
                    Using surface As New Bitmap(1, 1)
                        Using graphics As Graphics = Graphics.FromImage(surface)
                            Assert.ThrowsExactly(Of OverflowException)(Sub() control.PaintTo(graphics, New Rectangle(0, 0, 1, 1)))
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ParentBackgroundChangeKeepsTransparentPixelsCorrect()
            Using baselineParent As New Panel()
                Using optimizedParent As New Panel()
                    Using baseline As New FrameInvalidationProbe()
                        Using optimized As New FrameInvalidationProbe()
                            baselineParent.ClientSize = New Size(200, 160)
                            optimizedParent.ClientSize = baselineParent.ClientSize
                            baselineParent.BackColor = Color.Navy
                            optimizedParent.BackColor = baselineParent.BackColor
                            baselineParent.Controls.Add(baseline)
                            optimizedParent.Controls.Add(optimized)
                            baseline.ClientSize = New Size(160, 120)
                            optimized.ClientSize = baseline.ClientSize
                            baseline.BackColor = Color.Transparent
                            optimized.BackColor = Color.Transparent
                            optimized.UsePartialFrameInvalidation = True
                            Dim baselineHandle As IntPtr = baseline.Handle
                            Dim optimizedHandle As IntPtr = optimized.Handle
                            Assert.AreNotEqual(IntPtr.Zero, baselineHandle)
                            Assert.AreNotEqual(IntPtr.Zero, optimizedHandle)
                            Assert.IsFalse(baselineParent.Created)
                            Assert.IsFalse(optimizedParent.Created)
                            Dim baselineInvalidations As New List(Of Rectangle)()
                            Dim optimizedInvalidations As New List(Of Rectangle)()
                            AddHandler baseline.Invalidated, Sub(sender, e) baselineInvalidations.Add(e.InvalidRect)
                            AddHandler optimized.Invalidated, Sub(sender, e) optimizedInvalidations.Add(e.InvalidRect)
                            baseline.StartAnimation()
                            optimized.StartAnimation()
                            InvokeTimerTick(baseline)
                            InvokeTimerTick(optimized)
                            Using baselineSurface As New Bitmap(160, 120)
                                Using optimizedSurface As New Bitmap(160, 120)
                                    FillFrameBackground(baselineSurface, baseline.ClientRectangle, Color.Navy)
                                    FillFrameBackground(optimizedSurface, optimized.ClientRectangle, Color.Navy)
                                    baseline.PaintFrame(baselineSurface, baseline.ClientRectangle)
                                    optimized.PaintFrame(optimizedSurface, optimized.ClientRectangle)
                                    baselineInvalidations.Clear()
                                    optimizedInvalidations.Clear()
                                    baselineParent.BackColor = Color.DarkGreen
                                    optimizedParent.BackColor = Color.DarkGreen
                                    InvokeTimerTick(baseline)
                                    InvokeTimerTick(optimized)
                                    Dim baselineRegion As Rectangle = UnionFrameInvalidations(baselineInvalidations)
                                    Dim optimizedRegion As Rectangle = UnionFrameInvalidations(optimizedInvalidations)
                                    Assert.AreEqual(optimized.ClientRectangle, optimizedRegion)
                                    FillFrameBackground(baselineSurface, baselineRegion, Color.DarkGreen)
                                    FillFrameBackground(optimizedSurface, optimizedRegion, Color.DarkGreen)
                                    baseline.PaintFrame(baselineSurface, baselineRegion)
                                    optimized.PaintFrame(optimizedSurface, optimizedRegion)
                                    AssertSamePixels(baselineSurface, optimizedSurface)
                                    Assert.AreEqual(Color.DarkGreen.ToArgb(), optimizedSurface.GetPixel(150, 110).ToArgb())
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_PreservesDiagnosticsAcrossTicksAndSeek()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each adaptive As Boolean In New Boolean() {False, True}
                    Using control As New FrameInvalidationProbe()
                        control.ClientSize = New Size(160, 120)
                        control.CustomDisplaySpeed = customSpeed
                        control.AdaptiveTiming = adaptive
                        control.DiagnosticsEnabled = True
                        control.UsePartialFrameInvalidation = True
                        Dim handle As IntPtr = control.Handle
                        Assert.AreNotEqual(IntPtr.Zero, handle)
                        Dim invalidations As New List(Of Rectangle)()
                        AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                        PrimeFrameInvalidation(control)
                        control.ResetDiagnostics()
                        Using surface As New Bitmap(160, 120)
                            InvokeTimerTick(control)
                            InvokeTimerTick(control)
                            AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 34, 34))
                            control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                            Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                            Assert.AreEqual(1L, control.Diagnostics.DroppedFrames)
                            Assert.AreEqual(1L, control.Diagnostics.RenderOperations)

                            InvokeTimerTick(control)
                            control.SeekFrame(5)
                            InvokeTimerTick(control)
                            control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                            Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                            Assert.AreEqual(1L, control.Diagnostics.DroppedFrames)
                            Assert.AreEqual(2L, control.Diagnostics.RenderOperations)
                        End Using
                    End Using
                Next
            Next
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_PreservesPixelsAcrossTransparentFrames()
            Using source As Bitmap = LoadAnimatedGif()
                For Each mode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Normal, ImageSizeMode.CenterImage, ImageSizeMode.Zoom, ImageSizeMode.Fill}
                    For Each quality As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                        For Each transparentBackground As Boolean In New Boolean() {False, True}
                            AssertFrameSequenceMatchesFull(source, New Size(160, 120), mode, 50D, quality, transparentBackground, ExpectedFrameInvalidation(mode))
                        Next
                    Next
                Next
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_RealTimerPaintsTheReducedArea()
            Using form As New FrameInvalidationTestForm()
                Using control As New AniGif()
                    Using watchdog As New System.Windows.Forms.Timer()
                        form.ShowInTaskbar = False
                        form.Opacity = 0
                        form.StartPosition = FormStartPosition.Manual
                        form.Location = New Point(-5000, -5000)
                        form.ClientSize = New Size(160, 120)
                        control.ClientSize = form.ClientSize
                        control.CustomDisplaySpeed = True
                        control.FramesPerSecond = 20D
                        control.UsePartialFrameInvalidation = True
                        form.Controls.Add(control)
                        form.Show()
                        control.Update()
                        control.DiagnosticsEnabled = True
                        Dim invalidations As New List(Of Rectangle)()
                        Dim clips As New List(Of Rectangle)()
                        Dim ticks As Integer = 0
                        Dim timedOut As Boolean = False
                        Dim capturingTickPaint As Boolean = False
                        AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                        AddHandler control.Paint, Sub(sender, e)
                                                      If capturingTickPaint Then clips.Add(e.ClipRectangle)
                                                  End Sub
                        AddHandler GetTimer(control).Tick, Sub(sender, e)
                                                               ticks += 1
                                                               capturingTickPaint = True
                                                               Try
                                                                   control.Update()
                                                               Finally
                                                                   capturingTickPaint = False
                                                               End Try
                                                               If ticks = 2 Then Application.ExitThread()
                                                           End Sub
                        watchdog.Interval = 5000
                        AddHandler watchdog.Tick, Sub(sender, e)
                                                      timedOut = True
                                                      Application.ExitThread()
                                                  End Sub
                        control.StartAnimation()
                        watchdog.Start()
                        Try
                            Application.Run()
                        Finally
                            watchdog.Stop()
                            control.StopAnimation()
                        End Try

                        Assert.IsFalse(timedOut, "Die zwei Timer-Frames wurden nicht gezeichnet.")
                        Assert.AreEqual(2, ticks)
                        Assert.HasCount(2, invalidations)
                        Assert.HasCount(2, clips)
                        Assert.AreEqual(control.ClientRectangle, invalidations(0))
                        Assert.AreEqual(New Rectangle(0, 0, 34, 34), invalidations(1))
                        Assert.AreEqual(control.ClientRectangle, clips(0))
                        Assert.AreEqual(New Rectangle(0, 0, 34, 34), clips(1))
                        Assert.AreEqual(2L, control.Diagnostics.RenderedFrames)
                        Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ReentrantChangesAndDisposeStaySafe()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                PrimeFrameInvalidation(control)
                Dim changed As Boolean = False
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e)
                                                    invalidations.Add(e.InvalidRect)
                                                    If Not changed AndAlso e.InvalidRect.Width < control.ClientSize.Width Then
                                                        changed = True
                                                        control.GifSizeMode = ImageSizeMode.CenterImage
                                                    End If
                                                End Sub
                InvokeTimerTick(control)
                Assert.IsTrue(changed)
                Assert.AreEqual(New Rectangle(0, 0, 34, 34), invalidations(0))
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                PaintFullFrame(control)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, New Rectangle(62, 42, 36, 36))
            End Using

            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                PrimeFrameInvalidation(control)
                Dim disposedFromHandler As Boolean = False
                AddHandler control.Invalidated, Sub(sender, e)
                                                    If e.InvalidRect.Width < control.ClientSize.Width Then
                                                        disposedFromHandler = True
                                                        control.Dispose()
                                                    End If
                                                End Sub
                InvokeTimerTick(control)
                Assert.IsTrue(disposedFromHandler)
                Assert.IsTrue(control.IsDisposed)
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ReentrantGifReplacementReturnsCurrentBitmap()
            Using source As Bitmap = LoadAnimatedGif()
                Using control As New FrameInvalidationProbe()
                    control.ClientSize = New Size(160, 120)
                    control.UsePartialFrameInvalidation = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    PrimeFrameInvalidation(control)
                    InvokeTimerTick(control)
                    Dim oldImage As Bitmap = GetGifWithoutExposure(control)
                    Dim replaced As Boolean = False
                    Dim invalidations As New List(Of Rectangle)()
                    AddHandler control.Invalidated, Sub(sender, e)
                                                        invalidations.Add(e.InvalidRect)
                                                        If Not replaced AndAlso e.InvalidRect = control.ClientRectangle Then
                                                            replaced = True
                                                            control.Gif = source
                                                        End If
                                                    End Sub

                    Dim returned As Bitmap = control.Gif
                    Assert.IsTrue(replaced)
                    Assert.IsNotNull(returned)
                    Assert.AreNotSame(oldImage, returned)
                    Assert.AreSame(GetGifWithoutExposure(control), returned)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ReentrantPaintDoesNotPublishStaleBounds()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                control.StartAnimation()
                InvokeTimerTick(control)
                Dim changed As Boolean = False
                Dim handler As PaintEventHandler = Sub(sender, e)
                                                       If Not changed Then
                                                           changed = True
                                                           control.GifSizeMode = ImageSizeMode.CenterImage
                                                       End If
                                                   End Sub
                AddHandler control.Paint, handler
                PaintFullFrame(control)
                RemoveHandler control.Paint, handler
                Assert.IsTrue(changed)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                PaintFullFrame(control)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, New Rectangle(62, 42, 36, 36))
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_ReentrantSeekCancelsPendingTimerFrame()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.DiagnosticsEnabled = True
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                PrimeFrameInvalidation(control)
                control.ResetDiagnostics()
                Dim sought As Boolean = False
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                Dim handler As InvalidateEventHandler = Sub(sender, e)
                                                            If Not sought Then
                                                                sought = True
                                                                control.SeekFrame(4)
                                                            End If
                                                        End Sub
                AddHandler control.Invalidated, handler
                InvokeTimerTick(control)
                RemoveHandler control.Invalidated, handler
                Assert.IsTrue(sought)
                Assert.HasCount(2, invalidations)
                Assert.AreEqual(New Rectangle(0, 0, 34, 34), invalidations(0))
                Assert.AreEqual(New Rectangle(0, 0, 34, 34), invalidations(1))
                Assert.AreEqual(4, GetCurrentFrame(control))
                Using surface As New Bitmap(160, 120)
                    control.PaintFrame(surface, invalidations(1))
                    Assert.AreEqual(0L, control.Diagnostics.RenderedFrames)
                    Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                    InvokeTimerTick(control)
                    control.PaintFrame(surface, invalidations(invalidations.Count - 1))
                    Assert.AreEqual(1L, control.Diagnostics.RenderedFrames)
                    Assert.AreEqual(0L, control.Diagnostics.DroppedFrames)
                    Assert.AreEqual(2L, control.Diagnostics.RenderOperations)
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_RequiresRequestedCompletePaint()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)

                Using surface As New Bitmap(160, 120)
                    Using graphics As Graphics = Graphics.FromImage(surface)
                        Using disjoint As New Region(New Rectangle(0, 0, 10, 120))
                            disjoint.Union(New Rectangle(150, 0, 10, 120))
                            graphics.SetClip(disjoint, CombineMode.Replace)
                            control.PaintFrame(graphics, control.ClientRectangle)
                        End Using
                    End Using

                    control.StartAnimation()
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    control.PaintFrame(surface, New Rectangle(0, 0, 16, 16))
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                    control.PaintFrame(surface, control.ClientRectangle)
                    invalidations.Clear()
                    InvokeTimerTick(control)
                    AssertLastFrameInvalidation(invalidations, New Rectangle(0, 0, 34, 34))
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_StaticAndUnchangedFramesDoNotInvalidate()
            Using source As New Bitmap(8, 8)
                Using control As New AniGif()
                    control.Gif = source
                    control.UsePartialFrameInvalidation = True
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    Dim invalidations As New List(Of Rectangle)()
                    AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                    control.SeekFrame(0)
                    control.StartAnimation()
                    InvokeTimerTick(control)
                    Assert.IsFalse(GetTimer(control).Enabled)
                    Assert.IsEmpty(invalidations)
                End Using
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_UsesImageBoundsForTimerAndSeek()
            For Each mode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Normal, ImageSizeMode.CenterImage, ImageSizeMode.Zoom, ImageSizeMode.Fill}
                For Each quality As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                    Using control As New FrameInvalidationProbe()
                        control.ClientSize = New Size(160, 120)
                        control.GifSizeMode = mode
                        control.ZoomFactor = 50D
                        control.ScalingQualityPreset = quality
                        control.UsePartialFrameInvalidation = True
                        Dim handle As IntPtr = control.Handle
                        Assert.AreNotEqual(IntPtr.Zero, handle)
                        Dim invalidations As New List(Of Rectangle)()
                        AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                        control.StartAnimation()
                        InvokeTimerTick(control)
                        AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
                        PaintFullFrame(control)

                        Dim expected As Rectangle = ExpectedFrameInvalidation(mode)
                        invalidations.Clear()
                        InvokeTimerTick(control)
                        Assert.HasCount(1, invalidations)
                        Assert.AreEqual(expected, invalidations(0), $"{mode}, {quality}, Timer")
                        invalidations.Clear()
                        control.SeekFrame(3)
                        Assert.HasCount(1, invalidations)
                        Assert.AreEqual(expected, invalidations(0), $"{mode}, {quality}, SeekFrame")
                        control.SeekFrame(3)
                        Assert.HasCount(1, invalidations)
                    End Using
                Next
            Next
        End Sub

        <STATestMethod>
        <DataRow("AniGif_Anim103.gif", 60, 40, ImageSizeMode.Normal, 100, 0, 0, 42, 40)>
        <DataRow("AniGif_Anim103.gif", 60, 40, ImageSizeMode.CenterImage, 100, 8, 0, 44, 40)>
        <DataRow("AniGif_Anim103.gif", 60, 40, ImageSizeMode.Zoom, 1, 28, 18, 5, 5)>
        <DataRow("AniGif_Anim103.gif", 60, 40, ImageSizeMode.Fill, 100, 0, 0, 60, 40)>
        <DataRow("AniGif_Anim105.gif", 160, 120, ImageSizeMode.CenterImage, 100, 0, 24, 160, 71)>
        <DataRow("AniGif_Anim108.gif", 160, 120, ImageSizeMode.Zoom, 100, 0, 8, 160, 105)>
        Public Sub PartialFrameInvalidation_VariedSizesAndZoomsPreservePixels(fileName As String, width As Integer, height As Integer,
            mode As ImageSizeMode, zoom As Integer, x As Integer, y As Integer, updateWidth As Integer, updateHeight As Integer)
            Dim root As String = FindSolutionRoot(AppContext.BaseDirectory)
            Dim gifPath As String = Path.Combine(root, "SchlumpfsoftControlsNETDemo", "Resources", fileName)
            Using source As New Bitmap(gifPath)
                For Each quality As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                    AssertFrameSequenceMatchesFull(source, New Size(width, height), mode, CDec(zoom), quality, False,
                        New Rectangle(x, y, updateWidth, updateHeight))
                Next
            End Using
        End Sub

        <STATestMethod>
        Public Sub PartialFrameInvalidation_VisibilityAndDrawabilityGuardsRemainUnchanged()
            Using control As New FrameInvalidationProbe()
                control.ClientSize = New Size(160, 120)
                control.DiagnosticsEnabled = True
                control.UsePartialFrameInvalidation = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidations As New List(Of Rectangle)()
                AddHandler control.Invalidated, Sub(sender, e) invalidations.Add(e.InvalidRect)
                PrimeFrameInvalidation(control)
                control.Visible = False
                Dim hiddenFrame As Integer = GetCurrentFrame(control)
                Dim hiddenCount As Integer = invalidations.Count
                InvokeTimerTick(control)
                Assert.AreEqual(hiddenFrame, GetCurrentFrame(control))
                Assert.HasCount(hiddenCount, invalidations)
                control.Visible = True
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)

                control.ClientSize = Size.Empty
                Dim emptyFrame As Integer = GetCurrentFrame(control)
                Dim emptyCount As Integer = invalidations.Count
                InvokeTimerTick(control)
                Assert.AreNotEqual(emptyFrame, GetCurrentFrame(control))
                Assert.HasCount(emptyCount, invalidations)
                control.ClientSize = New Size(160, 120)
                control.DestroyHandleForFrameTest()
                Dim frameWithoutHandle As Integer = GetCurrentFrame(control)
                Dim countWithoutHandle As Integer = invalidations.Count
                InvokeTimerTick(control)
                Assert.AreNotEqual(frameWithoutHandle, GetCurrentFrame(control))
                Assert.HasCount(countWithoutHandle, invalidations)
                handle = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                invalidations.Clear()
                InvokeTimerTick(control)
                AssertLastFrameInvalidation(invalidations, control.ClientRectangle)
            End Using
        End Sub

        <TestMethod>
        Public Sub PauseResume_RestartsTheFullIntervalWithoutChangingAutoPlay()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using control As New AniGif()
                    control.CustomDisplaySpeed = customSpeed
                    control.FramesPerSecond = 10D
                    control.AdaptiveTiming = True
                    GetFrameDelays(control)(1) = 160
                    Dim startedCount As Integer = 0
                    Dim stoppedCount As Integer = 0
                    Dim autoPlayCount As Integer = 0
                    AddHandler control.AnimationStarted, Sub(sender, e) startedCount += 1
                    AddHandler control.AnimationStopped, Sub(sender, e) stoppedCount += 1
                    AddHandler control.AutoPlayChanged, Sub(sender, e) autoPlayCount += 1

                    control.StartAnimation()
                    InvokeTimerTick(control)
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    control.PauseAnimation()
                    Assert.IsFalse(GetTimer(control).Enabled)
                    Assert.IsTrue(control.AutoPlay)
                    Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                    InvokeTimerTick(control)
                    Assert.AreEqual(1, GetCurrentFrame(control))

                    GetTimer(control).Interval = 1
                    control.ResumeAnimation()
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.AreEqual(If(customSpeed, 100, 160), GetTimer(control).Interval)
                    Assert.IsGreaterThan(0L, GetLastFrameTimestamp(control))
                    Assert.AreEqual(2, startedCount)
                    Assert.AreEqual(1, stoppedCount)
                    Assert.AreEqual(1, autoPlayCount)
                    InvokeTimerTick(control)
                    Assert.AreEqual(2, GetCurrentFrame(control))
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub PlaybackMethods_AreIdempotentAndStartCanResume()
            Using control As New AniGif()
                Dim startedCount As Integer = 0
                Dim stoppedCount As Integer = 0
                Dim autoPlayCount As Integer = 0
                AddHandler control.AnimationStarted, Sub(sender, e) startedCount += 1
                AddHandler control.AnimationStopped, Sub(sender, e) stoppedCount += 1
                AddHandler control.AutoPlayChanged, Sub(sender, e) autoPlayCount += 1

                control.PauseAnimation()
                control.ResumeAnimation()
                control.SeekFrame(3)
                control.StartAnimation()
                control.StartAnimation()
                control.PauseAnimation()
                control.PauseAnimation()
                Assert.IsTrue(control.AutoPlay)
                Assert.IsFalse(GetTimer(control).Enabled)

                control.StartAnimation()
                Assert.IsTrue(GetTimer(control).Enabled)
                control.PauseAnimation()
                control.StopAnimation()
                control.StopAnimation()
                control.ResumeAnimation()

                Assert.IsFalse(control.AutoPlay)
                Assert.IsFalse(GetTimer(control).Enabled)
                Assert.AreEqual(3, GetCurrentFrame(control))
                Assert.AreEqual(2, startedCount)
                Assert.AreEqual(2, stoppedCount)
                Assert.AreEqual(2, autoPlayCount)
            End Using
        End Sub

        Private Shared Sub PrimeFrameInvalidation(control As FrameInvalidationProbe)
            control.StartAnimation()
            InvokeTimerTick(control)
            PaintFullFrame(control)
        End Sub

        Private Shared Function RenderControl(control As AniGif) As Bitmap
            Dim image As New Bitmap(control.Width, control.Height)
            control.DrawToBitmap(image, New Rectangle(Point.Empty, image.Size))
            Return image
        End Function

        <TestMethod>
        Public Sub ResumeWhileHidden_WaitsUntilTheControlIsVisible()
            Using control As New AniGif()
                control.StartAnimation()
                control.PauseAnimation()
                control.Visible = False
                control.ResumeAnimation()
                Assert.IsFalse(GetTimer(control).Enabled)

                control.Visible = True
                Assert.IsTrue(GetTimer(control).Enabled)
                InvokeTimerTick(control)
                Assert.AreEqual(1, GetCurrentFrame(control))
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_EmptyClientAreaDoesNotDrawAnImage()
            Using source As New Bitmap(4, 4)
                Using sourceGraphics As Graphics = Graphics.FromImage(source)
                    sourceGraphics.Clear(Color.Lime)
                End Using

                Using control As New PaintProbeAniGif()
                    control.Gif = source
                    Dim paintCount As Integer = 0
                    AddHandler control.Paint, Sub(sender, e) paintCount += 1

                    For Each clientSize As Size In New Size() {Size.Empty, New Size(0, 10), New Size(10, 0)}
                        control.ClientSize = clientSize
                        Assert.AreEqual(clientSize, control.ClientSize)
                        For Each sizeMode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Normal, ImageSizeMode.CenterImage, ImageSizeMode.Zoom, ImageSizeMode.Fill}
                            control.GifSizeMode = sizeMode
                            Assert.AreEqual(Rectangle.Empty, GetDrawingRectangle(control), $"{clientSize}, {sizeMode}")

                            Using surface As New Bitmap(8, 8)
                                Using graphics As Graphics = Graphics.FromImage(surface)
                                    graphics.Clear(Color.Magenta)
                                    paintCount = 0
                                    control.PaintTo(graphics, New Rectangle(Point.Empty, surface.Size))
                                End Using
                                Assert.AreEqual(1, paintCount, $"{clientSize}, {sizeMode}: Paint-Ereignis")
                                Assert.AreEqual(Color.Magenta.ToArgb(), surface.GetPixel(0, 0).ToArgb(), $"{clientSize}, {sizeMode}: kein Bild")
                            End Using
                        Next
                    Next
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_FillCoversTheClientArea()
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Fill, 100D, New Rectangle(0, -150, 400, 400))
            AssertDrawingRectangle(New Size(100, 100), New Size(100, 400), ImageSizeMode.Fill, 100D, New Rectangle(-150, 0, 400, 400))
            AssertDrawingRectangle(New Size(100, 100), New Size(100, 400), ImageSizeMode.Fill, 1D, New Rectangle(-150, 0, 400, 400))
            AssertDrawingRectangle(New Size(200, 100), New Size(100, 200), ImageSizeMode.Fill, 100D, New Rectangle(-150, 0, 400, 200))
            AssertDrawingRectangle(New Size(100, 200), New Size(200, 100), ImageSizeMode.Fill, 100D, New Rectangle(0, -150, 200, 400))
            AssertDrawingRectangle(New Size(100, 100), New Size(100, 100), ImageSizeMode.Fill, 1D, New Rectangle(0, 0, 100, 100))
            AssertDrawingRectangle(New Size(2, 5), New Size(5, 5), ImageSizeMode.Fill, 100D, New Rectangle(0, -4, 5, 13))
            AssertDrawingRectangle(New Size(6, 3), New Size(5, 2), ImageSizeMode.Fill, 100D, New Rectangle(0, 0, 5, 3))
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub ScalingGeometry_InternalArgumentsAreValidated()
            Dim sizeMethod As MethodInfo = GetGeometryMethod("GetRectStartSize")
            Dim pointMethod As MethodInfo = GetGeometryMethod("GetRectStartPoint")

            Using source As New Bitmap(4, 4)
                Using control As New AniGif()
                    Dim sizeException As TargetInvocationException = Assert.ThrowsExactly(Of TargetInvocationException)(
                        Sub() sizeMethod.Invoke(Nothing, New Object() {ImageSizeMode.Zoom, Nothing, source, 1D}))
                    Dim nullSizeControl As ArgumentNullException = TryCast(sizeException.InnerException, ArgumentNullException)
                    Assert.IsNotNull(nullSizeControl)
                    Assert.AreEqual("Control", nullSizeControl.ParamName)

                    Dim pointException As TargetInvocationException = Assert.ThrowsExactly(Of TargetInvocationException)(
                        Sub() pointMethod.Invoke(Nothing, New Object() {ImageSizeMode.Zoom, Nothing, source, New Size(4, 4)}))
                    Dim nullPointControl As ArgumentNullException = TryCast(pointException.InnerException, ArgumentNullException)
                    Assert.IsNotNull(nullPointControl)
                    Assert.AreEqual("Control", nullPointControl.ParamName)

                    For Each invalidZoom As Decimal In New Decimal() {0D, -0.1D, 1.01D}
                        Dim zoomException As TargetInvocationException = Assert.ThrowsExactly(Of TargetInvocationException)(
                            Sub() sizeMethod.Invoke(Nothing, New Object() {ImageSizeMode.Zoom, control, source, invalidZoom}))
                        Dim outOfRange As ArgumentOutOfRangeException = TryCast(zoomException.InnerException, ArgumentOutOfRangeException)
                        Assert.IsNotNull(outOfRange)
                        Assert.AreEqual("Zoom", outOfRange.ParamName)
                    Next

                    Assert.AreEqual(Size.Empty, DirectCast(sizeMethod.Invoke(Nothing, New Object() {ImageSizeMode.Zoom, control, Nothing, 1D}), Size))
                    Assert.AreEqual(Point.Empty, DirectCast(pointMethod.Invoke(Nothing, New Object() {ImageSizeMode.Zoom, control, Nothing, Size.Empty}), Point))
                End Using
            End Using

        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_PreservesTransparentPixelsInBothQualityPresets()
            Using source As New Bitmap(40, 40)
                Using graphics As Graphics = Graphics.FromImage(source)
                    graphics.Clear(Color.Transparent)
                    Using brush As New SolidBrush(Color.Lime)
                        graphics.FillRectangle(brush, New Rectangle(10, 0, 20, 40))
                    End Using
                End Using

                Using control As New AniGif()
                    control.ClientSize = New Size(160, 40)
                    control.BackColor = Color.Magenta
                    control.Gif = source
                    control.ZoomFactor = 100D

                    For Each quality As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                        control.ScalingQualityPreset = quality
                        For Each mode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Zoom, ImageSizeMode.Fill}
                            control.GifSizeMode = mode
                            Using rendered As Bitmap = RenderControl(control)
                                Dim left As Integer = If(mode = ImageSizeMode.Zoom, 65, 20)
                                Dim right As Integer = If(mode = ImageSizeMode.Zoom, 95, 140)
                                Assert.AreEqual(Color.Magenta.ToArgb(), rendered.GetPixel(left, 20).ToArgb(), $"{mode}, {quality}: transparente linke Seite")
                                Assert.AreEqual(Color.Lime.ToArgb(), rendered.GetPixel(80, 20).ToArgb(), $"{mode}, {quality}: Bildmitte")
                                Assert.AreEqual(Color.Magenta.ToArgb(), rendered.GetPixel(right, 20).ToArgb(), $"{mode}, {quality}: transparente rechte Seite")
                            End Using
                        Next
                    Next
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_RendersZoomAndFillWithBothQualityPresetsAfterResize()
            Using source As New Bitmap(40, 40)
                Using graphics As Graphics = Graphics.FromImage(source)
                    graphics.Clear(Color.Lime)
                End Using

                For Each quality As GifScalingQualityPreset In New GifScalingQualityPreset() {GifScalingQualityPreset.Photo, GifScalingQualityPreset.PixelArt}
                    Using control As New AniGif()
                        control.BackColor = Color.Magenta
                        control.Gif = source
                        control.ZoomFactor = 100D
                        control.ScalingQualityPreset = quality

                        For Each clientSize As Size In New Size() {New Size(160, 40), New Size(40, 160)}
                            control.ClientSize = clientSize
                            control.GifSizeMode = ImageSizeMode.Zoom
                            Using rendered As Bitmap = RenderControl(control)
                                AssertScaledPixels(rendered, clientSize, Color.Magenta, Color.Lime, Color.Magenta, $"Zoom, {quality}")
                            End Using

                            control.GifSizeMode = ImageSizeMode.Fill
                            Using rendered As Bitmap = RenderControl(control)
                                AssertScaledPixels(rendered, clientSize, Color.Lime, Color.Lime, Color.Lime, $"Fill, {quality}")
                                For y As Integer = 0 To rendered.Height - 1
                                    For x As Integer = 0 To rendered.Width - 1
                                        Assert.AreNotEqual(Color.Magenta.ToArgb(), rendered.GetPixel(x, y).ToArgb(), $"Fill, {quality}, {clientSize}, Pixel ({x}, {y})")
                                    Next
                                Next
                            End Using
                        Next
                    End Using
                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_ResizeInvalidatesTheEntireClientArea()
            Using control As New AniGif()
                control.ClientSize = New Size(160, 40)
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidatedClient As Boolean = False
                AddHandler control.Invalidated, Sub(sender, e)
                                                    If e.InvalidRect.Contains(control.ClientRectangle) Then invalidatedClient = True
                                                End Sub

                For Each mode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Zoom, ImageSizeMode.Fill}
                    control.GifSizeMode = mode
                    invalidatedClient = False
                    control.ClientSize = New Size(40, 160)
                    Assert.IsTrue(invalidatedClient, $"{mode}: Clientfläche nach Größenänderung")

                    invalidatedClient = False
                    control.ClientSize = New Size(160, 40)
                    Assert.IsTrue(invalidatedClient, $"{mode}: Clientfläche nach erneuter Größenänderung")
                Next
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub ScalingGeometry_TargetDimensionsOutsideInt32ThrowOverflow()
            Using source As New Bitmap(10000, 1)
                Using control As New AniGif()
                    control.ClientSize = New Size(1, 214749)
                    Assert.AreEqual(New Size(1, 214749), control.ClientSize)
                    Dim sizeMethod As MethodInfo = GetGeometryMethod("GetRectStartSize")
                    Dim exception As TargetInvocationException = Assert.ThrowsExactly(Of TargetInvocationException)(
                        Sub() sizeMethod.Invoke(Nothing, New Object() {ImageSizeMode.Fill, control, source, 1D}))
                    Assert.IsNotNull(TryCast(exception.InnerException, OverflowException))
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_UsesClientSizeAndPreservesUnscaledModes()
            Using source As New Bitmap(100, 100)
                Using control As New AniGif()
                    control.BorderStyle = BorderStyle.FixedSingle
                    control.ClientSize = New Size(400, 100)
                    control.Gif = source
                    control.ZoomFactor = 100D
                    Assert.AreNotEqual(control.Width, control.ClientSize.Width)

                    control.GifSizeMode = ImageSizeMode.Zoom
                    Assert.AreEqual(New Rectangle(150, 0, 100, 100), GetDrawingRectangle(control))
                    control.GifSizeMode = ImageSizeMode.Fill
                    Assert.AreEqual(New Rectangle(0, -150, 400, 400), GetDrawingRectangle(control))

                    control.ClientSize = New Size(103, 105)
                    control.GifSizeMode = ImageSizeMode.CenterImage
                    Assert.AreEqual(New Rectangle(2, 2, 100, 100), GetDrawingRectangle(control))
                    control.GifSizeMode = ImageSizeMode.Normal
                    Assert.AreEqual(New Rectangle(0, 0, 100, 100), GetDrawingRectangle(control))
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_ZoomFactorAndSmallSizesAreRounded()
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Zoom, 50D, New Rectangle(175, 25, 50, 50))
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Zoom, 1D, New Rectangle(200, 50, 1, 1))
            AssertDrawingRectangle(New Size(100, 100), New Size(200, 100), ImageSizeMode.Zoom, 33.3D, New Rectangle(84, 34, 33, 33))
            AssertDrawingRectangle(New Size(3, 6), New Size(5, 5), ImageSizeMode.Zoom, 60D, New Rectangle(2, 1, 2, 3))
            AssertDrawingRectangle(New Size(10000, 1), New Size(20, 1), ImageSizeMode.Zoom, 1D, New Rectangle(10, 0, 1, 1))
        End Sub

        <TestMethod>
        Public Sub ScalingGeometry_ZoomFitsTheClientArea()
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Zoom, 100D, New Rectangle(150, 0, 100, 100))
            AssertDrawingRectangle(New Size(100, 100), New Size(100, 400), ImageSizeMode.Zoom, 100D, New Rectangle(0, 150, 100, 100))
            AssertDrawingRectangle(New Size(200, 100), New Size(100, 200), ImageSizeMode.Zoom, 100D, New Rectangle(0, 75, 100, 50))
            AssertDrawingRectangle(New Size(100, 200), New Size(200, 100), ImageSizeMode.Zoom, 100D, New Rectangle(75, 0, 50, 100))
            AssertDrawingRectangle(New Size(200, 100), New Size(400, 100), ImageSizeMode.Zoom, 100D, New Rectangle(100, 0, 200, 100))
            AssertDrawingRectangle(New Size(4, 1), New Size(10, 10), ImageSizeMode.Zoom, 100D, New Rectangle(0, 4, 10, 3))
            AssertDrawingRectangle(New Size(3, 6), New Size(5, 5), ImageSizeMode.Zoom, 100D, New Rectangle(1, 0, 3, 5))
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_DefaultAndDesignerMetadataArePhoto()
            Using control As New AniGif()
                Assert.AreEqual(GifScalingQualityPreset.Photo, control.ScalingQualityPreset)

                Dim descriptor As PropertyDescriptor = TypeDescriptor.GetProperties(control)(NameOf(AniGif.ScalingQualityPreset))
                Assert.IsNotNull(descriptor)
                Assert.IsTrue(descriptor.IsBrowsable)
                Dim defaultValue As DefaultValueAttribute = DirectCast(descriptor.Attributes(GetType(DefaultValueAttribute)), DefaultValueAttribute)
                Assert.IsNotNull(defaultValue)
                Assert.AreEqual(GifScalingQualityPreset.Photo, CType(defaultValue.Value, GifScalingQualityPreset))
                Assert.IsFalse(descriptor.ShouldSerializeValue(control))

                control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                Assert.IsTrue(descriptor.ShouldSerializeValue(control))
                descriptor.ResetValue(control)
                Assert.AreEqual(GifScalingQualityPreset.Photo, control.ScalingQualityPreset)
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_DoesNotChangeUnscaledModes()
            Using source As Bitmap = CreateCheckerboard(2)
                Using control As New AniGif()
                    control.Size = New Size(10, 10)
                    control.Gif = source

                    For Each sizeMode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Normal, ImageSizeMode.CenterImage}
                        control.GifSizeMode = sizeMode
                        control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                        Using photo As Bitmap = RenderControl(control)
                            control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                            Using pixelArt As Bitmap = RenderControl(control)
                                For y As Integer = 0 To control.Height - 1
                                    For x As Integer = 0 To control.Width - 1
                                        Assert.AreEqual(photo.GetPixel(x, y).ToArgb(), pixelArt.GetPixel(x, y).ToArgb(), $"{sizeMode}: Pixel ({x}, {y})")
                                    Next
                                Next
                            End Using
                        End Using
                    Next
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_InvalidatesOnlyWhenValueChanges()
            Using control As New AniGif()
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Integer = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                Assert.AreEqual(0, invalidationCount)
                control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                Assert.AreEqual(1, invalidationCount)
                control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                Assert.AreEqual(1, invalidationCount)
                control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                Assert.AreEqual(2, invalidationCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_ProducesSharpOrSmoothDownscaling()
            Using source As Bitmap = CreateCheckerboard(40)
                Using control As New AniGif()
                    control.Size = New Size(10, 10)
                    control.Gif = source
                    control.ZoomFactor = 100D

                    For Each sizeMode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Zoom, ImageSizeMode.Fill}
                        control.GifSizeMode = sizeMode
                        control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                        Using pixelArt As Bitmap = RenderControl(control)
                            Dim color As Color = pixelArt.GetPixel(5, 5)
                            Assert.Contains(color.ToArgb(), New Integer() {Color.Black.ToArgb(), Color.White.ToArgb()}, $"{sizeMode}: ungefilterter Pixel")
                        End Using

                        control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                        Using photo As Bitmap = RenderControl(control)
                            Dim color As Color = photo.GetPixel(5, 5)
                            Assert.IsInRange(1, 254, color.R, $"{sizeMode}: gefilterter Pixel")
                        End Using
                    Next
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_ProducesSharpOrSmoothUpscaling()
            Using source As Bitmap = CreateCheckerboard(2)
                Using control As New AniGif()
                    control.Size = New Size(40, 40)
                    control.Gif = source
                    control.ZoomFactor = 100D

                    For Each sizeMode As ImageSizeMode In New ImageSizeMode() {ImageSizeMode.Zoom, ImageSizeMode.Fill}
                        control.GifSizeMode = sizeMode
                        control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt
                        Using pixelArt As Bitmap = RenderControl(control)
                            Assert.AreEqual(Color.Black.ToArgb(), pixelArt.GetPixel(18, 10).ToArgb(), $"{sizeMode}: schwarze Pixelkante")
                            Assert.AreEqual(Color.White.ToArgb(), pixelArt.GetPixel(21, 10).ToArgb(), $"{sizeMode}: weiße Pixelkante")
                        End Using

                        control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                        Using photo As Bitmap = RenderControl(control)
                            Dim color As Color = photo.GetPixel(18, 10)
                            Assert.IsInRange(1, 254, color.R, $"{sizeMode}: geglättete Pixelkante")
                        End Using
                    Next
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ScalingQualityPreset_RejectsInvalidValuesWithoutChangingCurrentValue()
            Using control As New AniGif()
                control.ScalingQualityPreset = GifScalingQualityPreset.PixelArt

                For Each invalidValue As GifScalingQualityPreset In New GifScalingQualityPreset() {
                    CType(-1, GifScalingQualityPreset), CType(2, GifScalingQualityPreset), CType(Integer.MaxValue, GifScalingQualityPreset)}
                    Dim exception As InvalidEnumArgumentException = Assert.ThrowsExactly(Of InvalidEnumArgumentException)(Sub() control.ScalingQualityPreset = invalidValue)
                    Assert.AreEqual("value", exception.ParamName)
                    Assert.AreEqual(GifScalingQualityPreset.PixelArt, control.ScalingQualityPreset)
                Next
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub SeekFrame_HandlesStillImagesAndDisposedControls()
            Dim control As New AniGif()
            Using stillImage As New Bitmap(8, 8)
                control.Gif = stillImage
                control.SeekFrame(0)
                Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SeekFrame(-1))
                Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SeekFrame(1))
                control.StartAnimation()
                Assert.IsFalse(GetTimer(control).Enabled)
                control.PauseAnimation()
            End Using

            control.Dispose()
            control.PauseAnimation()
            control.ResumeAnimation()
            control.SeekFrame(-1)
            control.StartAnimation()
            control.StopAnimation()
            Assert.IsTrue(control.IsDisposed)
        End Sub

        <TestMethod>
        Public Sub SeekFrame_InvalidatesOnlyForVisibleFrameChanges()
            Using control As New AniGif()
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Integer = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                control.SeekFrame(1)
                control.SeekFrame(1)
                Assert.AreEqual(1, invalidationCount)
                control.Visible = False
                Dim beforeHiddenSeek As Integer = invalidationCount
                control.SeekFrame(2)
                Assert.AreEqual(beforeHiddenSeek, invalidationCount)
                control.Visible = True
                Assert.AreEqual(2, GetCurrentFrame(control))
            End Using
        End Sub

        <TestMethod>
        Public Sub SeekFrame_SelectsActualFramesAndRejectsInvalidIndices()
            Using source As Bitmap = LoadAnimatedGif()
                Using control As New AniGif()
                    control.Gif = source
                    Dim dimension As New FrameDimension(source.FrameDimensionsList(0))
                    Dim lastFrame As Integer = source.GetFrameCount(dimension) - 1

                    For Each index As Integer In New Integer() {lastFrame, 0, 1}
                        Dim unused = source.SelectActiveFrame(dimension, index)
                        control.SeekFrame(index)
                        Assert.AreEqual(index, GetCurrentFrame(control))
                        AssertSamePixels(source, control.Gif)
                    Next

                    For Each index As Integer In New Integer() {-1, lastFrame + 1, Integer.MaxValue}
                        Dim exception As ArgumentOutOfRangeException = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SeekFrame(index))
                        Assert.AreEqual("index", exception.ParamName)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                    Next
                    Assert.IsFalse(control.AutoPlay)
                    Assert.IsFalse(GetTimer(control).Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub SeekFrame_WhilePausedWaitsForResume()
            Using control As New AniGif()
                GetFrameDelays(control)(4) = 240
                control.StartAnimation()
                control.PauseAnimation()
                control.SeekFrame(4)
                Assert.AreEqual(4, GetCurrentFrame(control))
                Assert.IsFalse(GetTimer(control).Enabled)
                InvokeTimerTick(control)
                Assert.AreEqual(4, GetCurrentFrame(control))

                control.ResumeAnimation()
                Assert.IsTrue(GetTimer(control).Enabled)
                Assert.AreEqual(240, GetTimer(control).Interval)
            End Using
        End Sub

        <TestMethod>
        Public Sub SeekFrame_WhilePlayingRestartsTheSelectedIntervalWithoutEvents()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using control As New AniGif()
                    control.CustomDisplaySpeed = customSpeed
                    control.FramesPerSecond = 10D
                    control.AdaptiveTiming = True
                    GetFrameDelays(control)(2) = 320
                    Dim startedCount As Integer = 0
                    Dim stoppedCount As Integer = 0
                    AddHandler control.AnimationStarted, Sub(sender, e) startedCount += 1
                    AddHandler control.AnimationStopped, Sub(sender, e) stoppedCount += 1

                    control.StartAnimation()
                    SetLastFrameTimestamp(control, 1L)
                    control.SeekFrame(2)
                    Assert.AreEqual(2, GetCurrentFrame(control))
                    Assert.IsTrue(GetTimer(control).Enabled)
                    Assert.AreEqual(If(customSpeed, 100, 320), GetTimer(control).Interval)
                    Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                    Assert.AreEqual(1, startedCount)
                    Assert.AreEqual(0, stoppedCount)
                    InvokeTimerTick(control)
                    Assert.AreEqual(3, GetCurrentFrame(control))
                End Using
            Next
        End Sub

        Private Shared Sub SetDelayBytes(image As Bitmap, delayProperty As PropertyItem, bytes As Byte())
            delayProperty.Value = bytes
            delayProperty.Len = bytes.Length
            image.SetPropertyItem(delayProperty)
        End Sub

        Private Shared Sub SetFirstFrameDelay(image As Bitmap, delayProperty As PropertyItem, centiseconds As Integer)
            Buffer.BlockCopy(BitConverter.GetBytes(centiseconds), 0, delayProperty.Value, 0, 4)
            image.SetPropertyItem(delayProperty)
        End Sub

        Private Shared Sub SetLastFrameTimestamp(control As AniGif, value As Long)
            Dim timestampField As FieldInfo = GetType(AniGif).GetField("_LastFrameTickTimestamp", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timestampField)
            timestampField.SetValue(control, value)
        End Sub

        <TestMethod>
        Public Sub StartAnimation_SetsAutoPlayToTrue()
            Using control As New AniGif()
                control.AutoPlay = False

                control.StartAnimation()

                Assert.IsTrue(control.AutoPlay)
            End Using
        End Sub

        <TestMethod>
        Public Sub StopAnimation_SetsAutoPlayToFalse()
            Using control As New AniGif()
                control.AutoPlay = True

                control.StopAnimation()

                Assert.IsFalse(control.AutoPlay)
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimerPlayback_AdvancesAndStopsInBothSpeedModes()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using source As Bitmap = LoadAnimatedGif()
                    Using control As New AniGif()
                        control.Gif = source
                        control.CustomDisplaySpeed = customSpeed
                        control.FramesPerSecond = 10D
                        GetFrameDelays(control)(0) = 140
                        GetFrameDelays(control)(1) = 240
                        Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                        control.StartAnimation()
                        Assert.IsTrue(timer.Enabled)
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                        Assert.AreEqual(If(customSpeed, 100, 140), timer.Interval)

                        Dim dimension As New FrameDimension(source.FrameDimensionsList(0))
                        Dim unused = source.SelectActiveFrame(dimension, 1)
                        InvokeTimerTick(control)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        Assert.AreEqual(If(customSpeed, 100, 240), timer.Interval)
                        AssertSamePixels(source, control.Gif)

                        control.StopAnimation()
                        Assert.IsFalse(timer.Enabled)
                        Assert.IsFalse(control.AutoPlay)
                        Assert.AreEqual(PlaybackState.Stopped, control.GetPlaybackState())
                        Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        InvokeTimerTick(control)
                        control.StopAnimation()
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        AssertSamePixels(source, control.Gif)
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimerPlayback_EquivalentFpsAssignmentsAllowRealTicks()
            Using control As New AniGif()
                Using assignmentTimer As New System.Windows.Forms.Timer()
                    Using watchdog As New System.Windows.Forms.Timer()
                        control.CustomDisplaySpeed = True
                        control.FramesPerSecond = 2D
                        Dim tickCount As Integer = 0
                        Dim assignmentCount As Integer = 0
                        Dim timedOut As Boolean = False
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        AddHandler GetTimer(control).Tick, Sub(sender, e)
                                                               tickCount += 1
                                                               If tickCount = 2 Then Application.ExitThread()
                                                           End Sub
                        assignmentTimer.Interval = 25
                        AddHandler assignmentTimer.Tick, Sub(sender, e)
                                                             control.FramesPerSecond = 2D
                                                             assignmentCount += 1
                                                         End Sub
                        watchdog.Interval = 5000
                        AddHandler watchdog.Tick, Sub(sender, e)
                                                      timedOut = True
                                                      Application.ExitThread()
                                                  End Sub
                        control.StartAnimation()
                        assignmentTimer.Start()
                        watchdog.Start()
                        Try
                            Application.Run()
                        Finally
                            assignmentTimer.Stop()
                            watchdog.Stop()
                            control.StopAnimation()
                        End Try

                        Assert.IsFalse(timedOut, "Die Animation hat trotz wiederholter gleichwertiger FPS-Zuweisungen nicht fortgesetzt.")
                        Assert.AreEqual(2, tickCount)
                        Assert.IsGreaterThan(0, assignmentCount)
                        Assert.AreEqual(2, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimerPlayback_SpeedModeChangesPreserveFrameAndEventOrder()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using control As New AniGif()
                    control.CustomDisplaySpeed = customSpeed
                    control.FramesPerSecond = 10D
                    GetFrameDelays(control)(1) = 240
                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                    Dim events As New List(Of String)()
                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                    control.StartAnimation()
                    InvokeTimerTick(control)
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    Assert.AreEqual(If(customSpeed, 100, 240), timer.Interval)

                    control.CustomDisplaySpeed = Not customSpeed
                    Assert.IsTrue(timer.Enabled)
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    Assert.AreEqual(If(customSpeed, 240, 100), timer.Interval)

                    control.CustomDisplaySpeed = customSpeed
                    Assert.IsTrue(timer.Enabled)
                    Assert.IsTrue(control.AutoPlay)
                    Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    Assert.AreEqual(If(customSpeed, 100, 240), timer.Interval)
                    CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    control.StopAnimation()
                    CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                End Using
            Next
        End Sub

        <STATestMethod>
        Public Sub TimerTick_DrawabilityGuardsDoNotPreventFrameProgress()
            For Each diagnosticsEnabled As Boolean In New Boolean() {False, True}
                Using control As New AniGif()
                    control.ClientSize = New Size(32, 32)
                    control.DiagnosticsEnabled = diagnosticsEnabled
                    Dim invalidations As Integer = 0
                    AddHandler control.Invalidated, Sub(sender, e) invalidations += 1
                    control.StartAnimation()
                    Assert.IsFalse(control.IsHandleCreated)
                    InvokeTimerTick(control)
                    Assert.AreEqual(1, GetCurrentFrame(control))
                    Assert.AreEqual(0, invalidations)
                    Assert.AreEqual(TimeSpan.Zero, control.Diagnostics.ActiveDuration)

                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)
                    control.ClientSize = Size.Empty
                    Dim beforeTick As Integer = invalidations
                    Dim emptyDuration As TimeSpan = control.Diagnostics.ActiveDuration
                    InvokeTimerTick(control)
                    Assert.AreEqual(2, GetCurrentFrame(control))
                    Assert.AreEqual(beforeTick, invalidations)
                    Assert.AreEqual(emptyDuration, control.Diagnostics.ActiveDuration)

                    control.ClientSize = New Size(32, 32)
                    beforeTick = invalidations
                    InvokeTimerTick(control)
                    Assert.AreEqual(3, GetCurrentFrame(control))
                    Assert.AreEqual(beforeTick + 1, invalidations)
                    control.Dispose()
                    beforeTick = invalidations
                    InvokeTimerTick(control)
                    Assert.AreEqual(3, GetCurrentFrame(control))
                    Assert.AreEqual(beforeTick, invalidations)
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub TimerTick_PausesWhenControlIsInvisibleAndContinuesWhenVisible()
            Using control As New AniGif()
                Using animatedGif As Bitmap = LoadAnimatedGif()
                    control.Gif = animatedGif
                    control.CustomDisplaySpeed = True
                    control.StartAnimation()

                    control.Visible = False
                    Dim frameBeforeHiddenTick As Integer = GetCurrentFrame(control)
                    InvokeTimerTick(control)
                    Assert.AreEqual(frameBeforeHiddenTick, GetCurrentFrame(control))

                    control.Visible = True
                    InvokeTimerTick(control)
                    Assert.AreNotEqual(frameBeforeHiddenTick, GetCurrentFrame(control))
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub TimerTick_PausesWhenParentIsInvisible()
            Using parentPanel As New Panel()
                Using control As New AniGif()
                    parentPanel.Controls.Add(control)

                    Using animatedGif As Bitmap = LoadAnimatedGif()
                        control.Gif = animatedGif
                        control.CustomDisplaySpeed = True
                        control.StartAnimation()

                        parentPanel.Visible = False
                        Dim frameBeforeHiddenTick As Integer = GetCurrentFrame(control)
                        InvokeTimerTick(control)

                        Assert.AreEqual(frameBeforeHiddenTick, GetCurrentFrame(control))
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimerTick_RechecksVisibilityAfterTimerIntervalReentrancy()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each diagnosticsEnabled As Boolean In New Boolean() {False, True}
                    For Each reparent As Boolean In New Boolean() {False, True}
                        Using firstParent As New Panel()
                            Using hiddenParent As New Panel()
                                Using timer As New ReentrantTimer()
                                    Using control As New AniGif()
                                        firstParent.Controls.Add(control)
                                        hiddenParent.Visible = False
                                        InstallTrackingTimer(control, timer)
                                        control.ClientSize = New Size(32, 32)
                                        control.CustomDisplaySpeed = customSpeed
                                        control.FramesPerSecond = 10D
                                        control.DiagnosticsEnabled = diagnosticsEnabled
                                        Dim delays As Integer() = GetFrameDelays(control)
                                        delays(0) = 100
                                        delays(1) = 200
                                        Dim handle As IntPtr = control.Handle
                                        Assert.AreNotEqual(IntPtr.Zero, handle)
                                        Dim events As New List(Of String)()
                                        Dim invalidations As Integer = 0
                                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                                        AddHandler control.Invalidated, Sub(sender, e) invalidations += 1
                                        control.StartAnimation()
                                        If customSpeed Then timer.Interval = 200

                                        Dim invalidationsAfterTransition As Integer = -1
                                        timer.RunOnNextEnabledRead(Sub()
                                                                       If reparent Then
                                                                           hiddenParent.Controls.Add(control)
                                                                       Else
                                                                           firstParent.Visible = False
                                                                       End If
                                                                       invalidationsAfterTransition = invalidations
                                                                   End Sub)
                                        InvokeTimerTick(control)
                                        Assert.AreNotEqual(-1, invalidationsAfterTransition)
                                        Assert.AreEqual(invalidationsAfterTransition, invalidations)
                                        Assert.AreEqual(1, GetCurrentFrame(control))
                                        Assert.IsFalse(control.Visible)
                                        If reparent Then
                                            Assert.IsFalse(timer.Enabled)
                                        Else
                                            Assert.IsTrue(timer.Enabled)
                                        End If
                                        Dim hiddenDuration As TimeSpan = control.Diagnostics.ActiveDuration
                                        InvokeTimerTick(control)
                                        Assert.AreEqual(1, GetCurrentFrame(control))
                                        Assert.AreEqual(hiddenDuration, control.Diagnostics.ActiveDuration)

                                        If reparent Then
                                            hiddenParent.Visible = True
                                        Else
                                            firstParent.Visible = True
                                        End If
                                        Assert.IsTrue(timer.Enabled)
                                        InvokeTimerTick(control)
                                        Assert.AreEqual(2, GetCurrentFrame(control))
                                        If reparent Then
                                            CollectionAssert.AreEqual(New String() {"started", "stopped", "started", "stopped", "started"}, events.ToArray())
                                        Else
                                            CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                                        End If
                                    End Using
                                End Using
                            End Using
                        End Using
                    Next
                Next
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_DifferentFpsWithSameRoundedIntervalPreservesAdaptiveTiming()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.AdaptiveTiming = True
                    Dim events As New List(Of String)()
                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                    control.StartAnimation()
                    Assert.AreEqual(33, timer.Interval)
                    timer.Interval = 25
                    SetLastFrameTimestamp(control, 123L)
                    Dim delays As Integer() = GetFrameDelays(control)
                    timer.ResetCounts()

                    control.FramesPerSecond = 30.1D

                    Assert.AreEqual(30.1D, control.FramesPerSecond)
                    Assert.AreSame(delays, GetFrameDelays(control))
                    Assert.AreEqual(25, timer.Interval)
                    Assert.AreEqual(123L, GetLastFrameTimestamp(control))
                    Assert.AreEqual(0, timer.StartCalls)
                    Assert.AreEqual(0, timer.StopCalls)
                    CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_FailedMetadataRefreshKeepsFpsAndBoundsEffective()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    Dim events As New List(Of String)()
                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                    control.StartAnimation()
                    Assert.AreEqual(100, timer.Interval)

                    Dim image As Bitmap = control.Gif
                    Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                    Buffer.BlockCopy(BitConverter.GetBytes(Integer.MaxValue), 0, delayProperty.Value, 4, 4)
                    image.SetPropertyItem(delayProperty)
                    Dim unused2 = Assert.ThrowsExactly(Of OverflowException)(Sub() control.FramesPerSecond = 20D)
                    Assert.AreEqual(20D, control.FramesPerSecond)
                    Assert.AreEqual(100, timer.Interval)
                    control.SeekFrame(1)
                    Assert.AreEqual(50, timer.Interval)

                    ' GDI+ kann Metadaten bei der Frameauswahl zurücksetzen.
                    image.SetPropertyItem(delayProperty)
                    Dim unused1 = Assert.ThrowsExactly(Of OverflowException)(Sub() control.MinimumFrameIntervalMs = 70)
                    Assert.AreEqual(70, control.MinimumFrameIntervalMs)
                    Assert.AreEqual(50, timer.Interval)
                    control.SeekFrame(2)
                    Assert.AreEqual(70, timer.Interval)

                    image.SetPropertyItem(delayProperty)
                    Dim unused = Assert.ThrowsExactly(Of OverflowException)(Sub() control.MaximumFrameIntervalMs = 60)
                    Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                    Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                    Assert.AreEqual(70, timer.Interval)
                    control.SeekFrame(3)
                    Assert.AreEqual(60, timer.Interval)
                    CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub TimingChanges_FailedMetadataRefreshUsesUpdatedFpsAfterModeChanges()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.FramesPerSecond = 10D
                    control.StartAnimation()
                    Assert.AreEqual(100, timer.Interval)
                    timer.ResetCounts()

                    Dim image As Bitmap = control.Gif
                    Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                    Buffer.BlockCopy(BitConverter.GetBytes(Integer.MaxValue), 0, delayProperty.Value, 4, 4)
                    image.SetPropertyItem(delayProperty)
                    Dim unused2 = Assert.ThrowsExactly(Of OverflowException)(Sub() control.FramesPerSecond = 20D)
                    Assert.AreEqual(100, timer.Interval)
                    control.CustomDisplaySpeed = True
                    Assert.AreEqual(50, timer.Interval)

                    Dim unused1 = Assert.ThrowsExactly(Of OverflowException)(Sub() control.MinimumFrameIntervalMs = 70)
                    Assert.AreEqual(50, timer.Interval)
                    control.CustomDisplaySpeed = False
                    Assert.AreEqual(100, timer.Interval)
                    control.CustomDisplaySpeed = True
                    Assert.AreEqual(70, timer.Interval)

                    Dim unused = Assert.ThrowsExactly(Of OverflowException)(Sub() control.MaximumFrameIntervalMs = 60)
                    Assert.AreEqual(70, timer.Interval)
                    control.CustomDisplaySpeed = False
                    Assert.AreEqual(100, timer.Interval)
                    control.CustomDisplaySpeed = True
                    Assert.AreEqual(60, timer.Interval)
                    Assert.AreEqual(0, timer.StartCalls)
                    Assert.AreEqual(0, timer.StopCalls)
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_FpsFallbackUsesRoundingAndCoupledBounds()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.Gif.RemovePropertyItem(&H5100)
                        control.CustomDisplaySpeed = customSpeed
                        control.FramesPerSecond = 16D
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        Assert.AreEqual(63, timer.Interval)
                        Assert.AreEqual(63, GetFrameDelays(control)(0))
                        timer.ResetCounts()

                        Dim fpsValues As Decimal() = New Decimal() {30D, 30.1D, 1D, 50D, 0D, 80D}
                        Dim expectedFps As Decimal() = New Decimal() {30D, 30.1D, 1D, 50D, 1D, 50D}
                        Dim expectedIntervals As Integer() = New Integer() {33, 33, 500, 20, 500, 20}
                        For i As Integer = 0 To fpsValues.Length - 1
                            control.FramesPerSecond = fpsValues(i)
                            Assert.AreEqual(expectedFps(i), control.FramesPerSecond)
                            Assert.AreEqual(expectedIntervals(i), timer.Interval)
                            Assert.AreEqual(expectedIntervals(i), GetFrameDelays(control)(0))
                        Next

                        control.MinimumFrameIntervalMs = 70
                        Assert.AreEqual(70, timer.Interval)
                        Assert.AreEqual(70, GetFrameDelays(control)(0))
                        control.MaximumFrameIntervalMs = 60
                        Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                        Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                        Assert.AreEqual(60, timer.Interval)
                        Assert.AreEqual(60, GetFrameDelays(control)(0))
                        control.MaximumFrameIntervalMs = 500
                        Assert.AreEqual(60, timer.Interval)
                        control.MinimumFrameIntervalMs = 20
                        Assert.AreEqual(20, timer.Interval)
                        Assert.AreEqual(20, GetFrameDelays(control)(0))
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_MetadataModeAppliesBoundsWithoutRestartingPlayback()
            For Each adaptive As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.AdaptiveTiming = adaptive
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        Assert.AreEqual(100, timer.Interval)
                        timer.ResetCounts()

                        If adaptive Then SetLastFrameTimestamp(control, 1L)
                        control.MinimumFrameIntervalMs = 130
                        Assert.AreEqual(130, timer.Interval)
                        If adaptive Then
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        Else
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        End If
                        control.MaximumFrameIntervalMs = 300
                        If adaptive Then
                            timer.Interval = 160
                            SetLastFrameTimestamp(control, 123L)
                        End If

                        control.FramesPerSecond = 15D
                        Assert.AreEqual(If(adaptive, 160, 130), timer.Interval)
                        Assert.AreEqual(130, GetFrameDelays(control)(0))
                        Assert.AreEqual(If(adaptive, 123L, 0L), GetLastFrameTimestamp(control))

                        If adaptive Then SetLastFrameTimestamp(control, 1L)
                        control.MaximumFrameIntervalMs = 60
                        Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                        Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                        Assert.AreEqual(60, timer.Interval)
                        If adaptive Then
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        Else
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        End If
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        Assert.IsTrue(timer.Enabled)
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_MetadataModeUpdatesMissingOrInvalidDelayFallback()
            For Each missingDelay As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        If missingDelay Then
                            control.Gif.RemovePropertyItem(&H5100)
                        Else
                            Dim delayProperty As PropertyItem = control.Gif.GetPropertyItem(&H5100)
                            Array.Clear(delayProperty.Value, 0, 4)
                            control.Gif.SetPropertyItem(delayProperty)
                        End If
                        control.FramesPerSecond = 10D
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        Assert.AreEqual(100, timer.Interval)
                        timer.ResetCounts()

                        control.FramesPerSecond = 20D

                        Assert.AreEqual(50, timer.Interval)
                        Assert.AreEqual(50, GetFrameDelays(control)(0))
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        Public Sub TimingChanges_TargetAlreadyScheduledDoesNotResetTimestamp()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    control.AdaptiveTiming = True
                    control.StartAnimation()
                    timer.Interval = 50
                    SetLastFrameTimestamp(control, 123L)
                    timer.ResetCounts()

                    control.FramesPerSecond = 20D

                    Assert.AreEqual(50, timer.Interval)
                    Assert.AreEqual(123L, GetLastFrameTimestamp(control))
                    Assert.AreEqual(0, timer.StartCalls)
                    Assert.AreEqual(0, timer.StopCalls)
                    Assert.IsTrue(timer.Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub TimingChanges_UnchangedTargetPreservesAdaptiveIntervalAndTimestamp()
            Using timer As New TrackingTimer()
                Using control As New AniGif()
                    InstallTrackingTimer(control, timer)
                    control.CustomDisplaySpeed = True
                    control.FramesPerSecond = 10D
                    control.AdaptiveTiming = True
                    GetFrameDelays(control)(0) = 100
                    control.StartAnimation()
                    timer.Interval = 80
                    SetLastFrameTimestamp(control, 123L)
                    timer.ResetCounts()

                    control.MaximumFrameIntervalMs = 400
                    control.MinimumFrameIntervalMs = 30
                    ' Beide Geschwindigkeitsarten haben für diesen Frame dasselbe Sollintervall.
                    GetFrameDelays(control)(0) = 100
                    control.CustomDisplaySpeed = False
                    control.CustomDisplaySpeed = True

                    Assert.AreEqual(80, timer.Interval)
                    Assert.AreEqual(123L, GetLastFrameTimestamp(control))
                    Assert.AreEqual(0, GetCurrentFrame(control))
                    Assert.AreEqual(0, timer.StartCalls)
                    Assert.AreEqual(0, timer.StopCalls)
                    Assert.IsTrue(timer.Enabled)
                End Using
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_UpdateRunningIntervalWithoutRestartingPlayback()
            For Each adaptive As Boolean In New Boolean() {False, True}
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        control.CustomDisplaySpeed = True
                        control.FramesPerSecond = 10D
                        control.AdaptiveTiming = adaptive
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        Assert.AreEqual(100, timer.Interval)
                        timer.ResetCounts()

                        If adaptive Then SetLastFrameTimestamp(control, 1L)
                        control.FramesPerSecond = 20D
                        Assert.AreEqual(50, timer.Interval)
                        If adaptive Then
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        Else
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        End If

                        If adaptive Then SetLastFrameTimestamp(control, 1L)
                        control.MinimumFrameIntervalMs = 70
                        Assert.AreEqual(70, timer.Interval)
                        If adaptive Then
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        Else
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        End If

                        If adaptive Then SetLastFrameTimestamp(control, 1L)
                        control.MaximumFrameIntervalMs = 60
                        Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                        Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                        Assert.AreEqual(60, timer.Interval)
                        If adaptive Then
                            Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        Else
                            Assert.AreEqual(0L, GetLastFrameTimestamp(control))
                        End If
                        Assert.AreEqual(0, GetCurrentFrame(control))
                        Assert.IsTrue(timer.Enabled)
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started"}, events.ToArray())

                        control.StopAnimation()
                        Assert.AreEqual(1, timer.StopCalls)
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                    End Using
                End Using
            Next
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub TimingChanges_WhileHiddenOrPausedDoNotStartTheTimer()
            Using parent As New Panel()
                Using timer As New TrackingTimer()
                    Using control As New AniGif()
                        InstallTrackingTimer(control, timer)
                        parent.Controls.Add(control)
                        control.CustomDisplaySpeed = True
                        control.FramesPerSecond = 10D
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                        control.StartAnimation()
                        control.SeekFrame(1)
                        parent.Visible = False
                        control.FramesPerSecond = 20D
                        Assert.IsFalse(timer.Enabled)
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())
                        timer.ResetCounts()

                        Dim hiddenDelays As Integer() = GetFrameDelays(control)
                        control.FramesPerSecond = 20D
                        Assert.AreSame(hiddenDelays, GetFrameDelays(control))
                        control.MinimumFrameIntervalMs = 30
                        Assert.IsFalse(timer.Enabled)
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"started", "stopped"}, events.ToArray())

                        control.PauseAnimation()
                        timer.ResetCounts()
                        parent.Visible = True
                        control.FramesPerSecond = 20D
                        control.MaximumFrameIntervalMs = 300
                        Assert.IsFalse(timer.Enabled)
                        Assert.AreEqual(0, timer.StartCalls)
                        Assert.AreEqual(0, timer.StopCalls)
                        Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())

                        control.ResumeAnimation()
                        Assert.IsTrue(timer.Enabled)
                        Assert.AreEqual(50, timer.Interval)
                        Assert.AreEqual(1, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"started", "stopped", "started"}, events.ToArray())
                    End Using
                End Using
            End Using
        End Sub

        Private Shared Function UnionFrameInvalidations(invalidations As List(Of Rectangle)) As Rectangle
            Assert.IsNotEmpty(invalidations)
            Dim combined As Rectangle = invalidations(0)
            For index As Integer = 1 To invalidations.Count - 1
                combined = Rectangle.Union(combined, invalidations(index))
            Next
            Return combined
        End Function

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub VisibilityChanges_SuppressFramesInBothSpeedModes()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                Using parent As New Panel()
                    Using control As New AniGif()
                        parent.Controls.Add(control)
                        control.CustomDisplaySpeed = customSpeed
                        Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                        Dim events As New List(Of String)()
                        AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                        AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")

                        control.StartAnimation()
                        InvokeTimerTick(control)
                        Assert.AreEqual(1, GetCurrentFrame(control))

                        control.Visible = False
                        Assert.IsFalse(timer.Enabled)
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                        InvokeTimerTick(control)
                        Assert.AreEqual(1, GetCurrentFrame(control))

                        control.Visible = True
                        Assert.IsTrue(timer.Enabled)
                        InvokeTimerTick(control)
                        Assert.AreEqual(2, GetCurrentFrame(control))
                        CollectionAssert.AreEqual(New String() {"started", "stopped", "started"}, events.ToArray())

                        parent.Visible = False
                        Assert.IsFalse(control.Visible)
                        Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                        Dim eventCountWhileHidden As Integer = events.Count
                        InvokeTimerTick(control)
                        Assert.AreEqual(2, GetCurrentFrame(control))
                        Assert.HasCount(eventCountWhileHidden, events)

                        parent.Visible = True
                        Assert.IsTrue(timer.Enabled)
                        Assert.IsTrue(control.AutoPlay)
                        InvokeTimerTick(control)
                        Assert.AreEqual(3, GetCurrentFrame(control))
                    End Using
                End Using
            Next
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub VisibilityHierarchy_PreservesPlaybackAcrossNestedChanges()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each adaptive As Boolean In New Boolean() {False, True}
                    For Each diagnosticsEnabled As Boolean In New Boolean() {False, True}
                        Using outer As New Panel()
                            Using inner As New Panel()
                                Using control As New AniGif()
                                    outer.Controls.Add(inner)
                                    inner.Controls.Add(control)
                                    control.ClientSize = New Size(32, 32)
                                    control.CustomDisplaySpeed = customSpeed
                                    control.AdaptiveTiming = adaptive
                                    control.DiagnosticsEnabled = diagnosticsEnabled
                                    Dim handle As IntPtr = control.Handle
                                    Assert.AreNotEqual(IntPtr.Zero, handle)
                                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                                    Dim events As New List(Of String)()
                                    Dim invalidations As Integer = 0
                                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                                    AddHandler control.Invalidated, Sub(sender, e) invalidations += 1

                                    control.StartAnimation()
                                    Dim beforeTick As Integer = invalidations
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(1, GetCurrentFrame(control))
                                    Assert.AreEqual(beforeTick + 1, invalidations)

                                    outer.Visible = False
                                    ' Ein Elternwechsel hält den Timer nicht in jeder WinForms-Hierarchie an.
                                    Assert.IsTrue(timer.Enabled)
                                    Assert.IsTrue(control.AutoPlay)
                                    Assert.AreEqual(PlaybackState.Playing, control.GetPlaybackState())
                                    Dim hiddenDuration As TimeSpan = control.Diagnostics.ActiveDuration
                                    beforeTick = invalidations
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(1, GetCurrentFrame(control))
                                    Assert.AreEqual(beforeTick, invalidations)
                                    Assert.AreEqual(hiddenDuration, control.Diagnostics.ActiveDuration)

                                    inner.Visible = False
                                    outer.Visible = True
                                    Assert.IsFalse(control.Visible)
                                    Assert.IsTrue(timer.Enabled)
                                    inner.Visible = True
                                    Assert.IsTrue(timer.Enabled)
                                    beforeTick = invalidations
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(2, GetCurrentFrame(control))
                                    Assert.AreEqual(beforeTick + 1, invalidations)

                                    control.Visible = False
                                    inner.Visible = False
                                    inner.Visible = True
                                    Assert.IsFalse(control.Visible)
                                    Assert.IsFalse(timer.Enabled)
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(2, GetCurrentFrame(control))
                                    control.Visible = True
                                    Assert.IsTrue(timer.Enabled)
                                    control.PauseAnimation()
                                    outer.Visible = False
                                    outer.Visible = True
                                    Assert.AreEqual(PlaybackState.Paused, control.GetPlaybackState())
                                    Assert.IsFalse(timer.Enabled)
                                    control.ResumeAnimation()
                                    Assert.IsTrue(timer.Enabled)
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(3, GetCurrentFrame(control))
                                    CollectionAssert.AreEqual(New String() {"started", "stopped", "started", "stopped", "started", "stopped", "started"}, events.ToArray())
                                End Using
                            End Using
                        End Using
                    Next
                Next
            Next
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub VisibilityHierarchy_RealTimerSuppressesHiddenFramesAndResumes()
            Using outer As New Panel()
                Using inner As New Panel()
                    Using control As New AniGif()
                        Using showTimer As New System.Windows.Forms.Timer()
                            Using watchdog As New System.Windows.Forms.Timer()
                                outer.Controls.Add(inner)
                                inner.Controls.Add(control)
                                control.ClientSize = New Size(32, 32)
                                control.CustomDisplaySpeed = True
                                control.FramesPerSecond = 20D
                                control.DiagnosticsEnabled = True
                                Dim handle As IntPtr = control.Handle
                                Assert.AreNotEqual(IntPtr.Zero, handle)
                                Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                                Dim events As New List(Of String)()
                                Dim phase As Integer = 0
                                Dim firstFrame As Integer = -1
                                Dim hiddenFrame As Integer = -1
                                Dim resumedFrame As Integer = -1
                                Dim invalidations As Integer = 0
                                Dim invalidationsAfterHide As Integer = -1
                                Dim invalidationsBeforeShow As Integer = -1
                                Dim hiddenTimerEnabled As Boolean = False
                                Dim resumedTimerEnabled As Boolean = False
                                Dim hiddenStart As TimeSpan = TimeSpan.Zero
                                Dim hiddenEnd As TimeSpan = TimeSpan.Zero
                                Dim timedOut As Boolean = False
                                AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                                AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                                AddHandler control.Invalidated, Sub(sender, e) invalidations += 1
                                AddHandler timer.Tick, Sub(sender, e)
                                                           Select Case phase
                                                               Case 0
                                                                   firstFrame = GetCurrentFrame(control)
                                                                   phase = 1
                                                                   outer.Visible = False
                                                                   hiddenTimerEnabled = timer.Enabled
                                                                   invalidationsAfterHide = invalidations
                                                                   hiddenStart = control.Diagnostics.ActiveDuration
                                                                   showTimer.Start()
                                                               Case 2
                                                                   resumedFrame = GetCurrentFrame(control)
                                                                   phase = 3
                                                                   Application.ExitThread()
                                                           End Select
                                                       End Sub
                                showTimer.Interval = 100
                                AddHandler showTimer.Tick, Sub(sender, e)
                                                               showTimer.Stop()
                                                               hiddenFrame = GetCurrentFrame(control)
                                                               invalidationsBeforeShow = invalidations
                                                               hiddenEnd = control.Diagnostics.ActiveDuration
                                                               phase = 2
                                                               outer.Visible = True
                                                               resumedTimerEnabled = timer.Enabled
                                                           End Sub
                                watchdog.Interval = 5000
                                AddHandler watchdog.Tick, Sub(sender, e)
                                                              timedOut = True
                                                              Application.ExitThread()
                                                          End Sub
                                control.StartAnimation()
                                watchdog.Start()
                                Try
                                    Application.Run()
                                Finally
                                    showTimer.Stop()
                                    watchdog.Stop()
                                    control.StopAnimation()
                                End Try

                                Assert.IsFalse(timedOut, "Die Wiedergabe wurde nach dem Einblenden nicht fortgesetzt.")
                                Assert.AreEqual(3, phase)
                                Assert.AreEqual(1, firstFrame)
                                Assert.AreEqual(firstFrame, hiddenFrame)
                                Assert.AreEqual(2, resumedFrame)
                                Assert.AreNotEqual(-1, invalidationsAfterHide)
                                Assert.AreEqual(invalidationsAfterHide, invalidationsBeforeShow)
                                Assert.IsTrue(hiddenTimerEnabled)
                                Assert.IsTrue(resumedTimerEnabled)
                                Assert.AreEqual(hiddenStart, hiddenEnd)
                                CollectionAssert.AreEqual(New String() {"started", "stopped", "started", "stopped"}, events.ToArray(), String.Join(", ", events))
                            End Using
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <STATestMethod>
        <CodeAnalysis.SuppressMessage("Performance", "CA1861:Konstantenmatrizen als Argumente vermeiden", Justification:="<Ausstehend>")>
        Public Sub VisibilityHierarchy_ReparentingFollowsTheCurrentAncestors()
            For Each customSpeed As Boolean In New Boolean() {False, True}
                For Each diagnosticsEnabled As Boolean In New Boolean() {False, True}
                    Using firstParent As New Panel()
                        Using secondParent As New Panel()
                            Using branch As New Panel()
                                Using control As New AniGif()
                                    firstParent.Controls.Add(branch)
                                    branch.Controls.Add(control)
                                    control.ClientSize = New Size(32, 32)
                                    control.CustomDisplaySpeed = customSpeed
                                    control.DiagnosticsEnabled = diagnosticsEnabled
                                    Dim handle As IntPtr = control.Handle
                                    Assert.AreNotEqual(IntPtr.Zero, handle)
                                    Dim timer As System.Windows.Forms.Timer = GetTimer(control)
                                    Dim events As New List(Of String)()
                                    AddHandler control.AnimationStarted, Sub(sender, e) events.Add("started")
                                    AddHandler control.AnimationStopped, Sub(sender, e) events.Add("stopped")
                                    control.StartAnimation()
                                    InvokeTimerTick(control)

                                    firstParent.Visible = False
                                    Assert.IsTrue(timer.Enabled)
                                    secondParent.Controls.Add(branch)
                                    Assert.IsTrue(control.Visible)
                                    Assert.IsTrue(timer.Enabled)
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(2, GetCurrentFrame(control))
                                    Dim eventCount As Integer = events.Count
                                    firstParent.Visible = True
                                    firstParent.Visible = False
                                    Assert.HasCount(eventCount, events)
                                    Assert.IsTrue(timer.Enabled)

                                    secondParent.Visible = False
                                    firstParent.Controls.Add(branch)
                                    Assert.IsFalse(control.Visible)
                                    Assert.IsTrue(timer.Enabled)
                                    Dim hiddenDuration As TimeSpan = control.Diagnostics.ActiveDuration
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(2, GetCurrentFrame(control))
                                    Assert.AreEqual(hiddenDuration, control.Diagnostics.ActiveDuration)
                                    firstParent.Visible = True
                                    Assert.IsTrue(timer.Enabled)
                                    InvokeTimerTick(control)
                                    Assert.AreEqual(3, GetCurrentFrame(control))
                                    CollectionAssert.AreEqual(New String() {"started", "stopped", "started"}, events.ToArray())
                                End Using
                            End Using
                        End Using
                    End Using
                Next
            Next
        End Sub

        <TestMethod>
        Public Sub ZoomFactor_IsClampedToValidRange()
            Using control As New AniGif()
                control.ZoomFactor = 0D
                Assert.AreEqual(1D, control.ZoomFactor)

                control.ZoomFactor = 150D
                Assert.AreEqual(100D, control.ZoomFactor)

                control.ZoomFactor = 42D
                Assert.AreEqual(42D, control.ZoomFactor)
            End Using
        End Sub

    End Class

End Namespace
