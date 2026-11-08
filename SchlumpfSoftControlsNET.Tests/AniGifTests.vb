Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.AniGifControl

Namespace SchlumpfSoftControlsNET.Tests.AniGifControl
    <TestClass>
    Public Class AniGifTests

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
        Public Sub FramesPerSecond_IsClampedToValidRange()
            Using control As New AniGif()
                control.FramesPerSecond = 0D
                Assert.AreEqual(1D, control.FramesPerSecond)

                control.FramesPerSecond = 80D
                Assert.AreEqual(50D, control.FramesPerSecond)
            End Using
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
                            Assert.IsTrue(color.R > 0 AndAlso color.R < 255, $"{sizeMode}: geglättete Pixelkante")
                        End Using
                    Next
                End Using
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
                            Assert.IsTrue(color.ToArgb() = Color.Black.ToArgb() OrElse color.ToArgb() = Color.White.ToArgb(), $"{sizeMode}: ungefilterter Pixel")
                        End Using

                        control.ScalingQualityPreset = GifScalingQualityPreset.Photo
                        Using photo As Bitmap = RenderControl(control)
                            Dim color As Color = photo.GetPixel(5, 5)
                            Assert.IsTrue(color.R > 0 AndAlso color.R < 255, $"{sizeMode}: gefilterter Pixel")
                        End Using
                    Next
                End Using
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

        <TestMethod>
        Public Sub DefaultGif_IsInitializedAndWrapsFromTheLastFrame()
            Using control As New AniGif()
                Dim dimension As New FrameDimension(control.Gif.FrameDimensionsList(0))
                Dim lastFrame As Integer = control.Gif.GetFrameCount(dimension) - 1
                Assert.IsTrue(lastFrame > 0)

                control.SeekFrame(lastFrame)
                Assert.AreEqual(lastFrame, GetCurrentFrame(control))
                control.StartAnimation()
                Assert.IsTrue(GetTimer(control).Enabled)
                InvokeTimerTick(control)
                Assert.AreEqual(0, GetCurrentFrame(control))
            End Using
        End Sub

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
                    Assert.IsTrue(GetLastFrameTimestamp(control) > 0)
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
                    Assert.IsTrue(GetLastFrameTimestamp(control) > 1L)
                    Assert.AreEqual(1, startedCount)
                    Assert.AreEqual(0, stoppedCount)
                    InvokeTimerTick(control)
                    Assert.AreEqual(3, GetCurrentFrame(control))
                End Using
            Next
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

        Private Shared Sub AssertSamePixels(expected As Bitmap, actual As Bitmap)
            Assert.AreEqual(expected.Size, actual.Size)
            For y As Integer = 0 To expected.Height - 1
                For x As Integer = 0 To expected.Width - 1
                    Assert.AreEqual(expected.GetPixel(x, y).ToArgb(), actual.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y})")
                Next
            Next
        End Sub

        Private Shared Function GetTimer(control As AniGif) As System.Windows.Forms.Timer
            Dim timerProperty As PropertyInfo = GetType(AniGif).GetProperty("Timer", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerProperty)
            Return DirectCast(timerProperty.GetValue(control), System.Windows.Forms.Timer)
        End Function

        Private Shared Function GetFrameDelays(control As AniGif) As Integer()
            Dim delayField As FieldInfo = GetType(AniGif).GetField("_NormalizedFrameDelaysMs", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(delayField)
            Return DirectCast(delayField.GetValue(control), Integer())
        End Function

        Private Shared Function GetLastFrameTimestamp(control As AniGif) As Long
            Dim timestampField As FieldInfo = GetType(AniGif).GetField("_LastFrameTickTimestamp", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timestampField)
            Return CLng(timestampField.GetValue(control))
        End Function

        Private Shared Sub SetLastFrameTimestamp(control As AniGif, value As Long)
            Dim timestampField As FieldInfo = GetType(AniGif).GetField("_LastFrameTickTimestamp", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timestampField)
            timestampField.SetValue(control, value)
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

		Private Shared Function RenderControl(control As AniGif) As Bitmap
			Dim image As New Bitmap(control.Width, control.Height)
			control.DrawToBitmap(image, New Rectangle(Point.Empty, image.Size))
			Return image
		End Function

        Private Shared Sub InvokeTimerTick(control As AniGif)
            Dim timerTickMethod As MethodInfo = GetType(AniGif).GetMethod("Timer_Tick", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerTickMethod)

            Dim unused = timerTickMethod.Invoke(control, New Object() {control, EventArgs.Empty})
        End Sub

        Private Shared Function GetCurrentFrame(control As AniGif) As Integer
            Dim frameField As FieldInfo = GetType(AniGif).GetField("_Frame", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(frameField)

            Return CInt(frameField.GetValue(control))
        End Function

        Private Shared Function LoadAnimatedGif() As Bitmap
            Dim solutionRoot As String = FindSolutionRoot(AppContext.BaseDirectory)
            Dim gifPath As String = Path.Combine(solutionRoot, "SchlumpfSoftControlsNET", "AniGifControl", "AniGifControl.Standard.gif")

            Assert.IsTrue(File.Exists(gifPath), $"Die Testdatei wurde nicht gefunden: {gifPath}")
            Return New Bitmap(gifPath)
        End Function

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
    End Class
End Namespace
