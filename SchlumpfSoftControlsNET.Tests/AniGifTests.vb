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
        Public Sub ScalingGeometry_ZoomFactorAndSmallSizesAreRounded()
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Zoom, 50D, New Rectangle(175, 25, 50, 50))
            AssertDrawingRectangle(New Size(100, 100), New Size(400, 100), ImageSizeMode.Zoom, 1D, New Rectangle(200, 50, 1, 1))
            AssertDrawingRectangle(New Size(100, 100), New Size(200, 100), ImageSizeMode.Zoom, 33.3D, New Rectangle(84, 34, 33, 33))
            AssertDrawingRectangle(New Size(3, 6), New Size(5, 5), ImageSizeMode.Zoom, 60D, New Rectangle(2, 1, 2, 3))
            AssertDrawingRectangle(New Size(10000, 1), New Size(20, 1), ImageSizeMode.Zoom, 1D, New Rectangle(10, 0, 1, 1))
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

        Private Shared Function GetDrawingRectangle(control As AniGif) As Rectangle
            Dim sizeMethod As MethodInfo = GetGeometryMethod("GetRectStartSize")
            Dim pointMethod As MethodInfo = GetGeometryMethod("GetRectStartPoint")
            Dim size As Size = DirectCast(sizeMethod.Invoke(Nothing, New Object() {control.GifSizeMode, control, control.Gif, control.ZoomFactor / 100D}), Size)
            Dim point As Point = DirectCast(pointMethod.Invoke(Nothing, New Object() {control.GifSizeMode, control, control.Gif, size}), Point)
            Return New Rectangle(point, size)
        End Function

        Private Shared Function GetGeometryMethod(name As String) As MethodInfo
            Dim helperType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".FunctionDefinitions")
            Assert.IsNotNull(helperType)
            Dim method As MethodInfo = helperType.GetMethod(name, BindingFlags.Static Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Return method
        End Function

        Private Shared Sub AssertScaledPixels(image As Bitmap, clientSize As Size, first As Color, middle As Color, last As Color, message As String)
            Dim points As Point() = If(clientSize.Width > clientSize.Height,
                New Point() {New Point(20, 20), New Point(80, 20), New Point(140, 20)},
                New Point() {New Point(20, 20), New Point(20, 80), New Point(20, 140)})
            Dim expected As Color() = {first, middle, last}
            For index As Integer = 0 To points.Length - 1
                Assert.AreEqual(expected(index).ToArgb(), image.GetPixel(points(index).X, points(index).Y).ToArgb(), $"{message}, {clientSize}, Pixel {points(index)}")
            Next
        End Sub

        Private NotInheritable Class PaintProbeAniGif
            Inherits AniGif

            Public Sub PaintTo(graphics As Graphics, bounds As Rectangle)
                Using e As New PaintEventArgs(graphics, bounds)
                    Me.OnPaint(e)
                End Using
            End Sub
        End Class

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
