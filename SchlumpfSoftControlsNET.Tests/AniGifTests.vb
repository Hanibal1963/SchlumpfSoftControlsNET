Imports System.ComponentModel
Imports System.Diagnostics
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
                            Assert.IsInRange(1, 254, color.R, $"{sizeMode}: geglättete Pixelkante")
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

                        SetLastFrameTimestamp(control, 1L)
                        control.FramesPerSecond = 20D
                        Assert.AreEqual(50, timer.Interval)
                        Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))

                        SetLastFrameTimestamp(control, 1L)
                        control.MinimumFrameIntervalMs = 70
                        Assert.AreEqual(70, timer.Interval)
                        Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))

                        SetLastFrameTimestamp(control, 1L)
                        control.MaximumFrameIntervalMs = 60
                        Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                        Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                        Assert.AreEqual(60, timer.Interval)
                        Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
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

                        SetLastFrameTimestamp(control, 1L)
                        control.MinimumFrameIntervalMs = 130
                        Assert.AreEqual(130, timer.Interval)
                        Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
                        control.MaximumFrameIntervalMs = 300
                        If adaptive Then timer.Interval = 160
                        SetLastFrameTimestamp(control, 123L)

                        control.FramesPerSecond = 15D
                        Assert.AreEqual(If(adaptive, 160, 130), timer.Interval)
                        Assert.AreEqual(130, GetFrameDelays(control)(0))
                        Assert.AreEqual(123L, GetLastFrameTimestamp(control))

                        SetLastFrameTimestamp(control, 1L)
                        control.MaximumFrameIntervalMs = 60
                        Assert.AreEqual(60, control.MinimumFrameIntervalMs)
                        Assert.AreEqual(60, control.MaximumFrameIntervalMs)
                        Assert.AreEqual(60, timer.Interval)
                        Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
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
        Public Sub FrameDelayMetadata_OverflowDoesNotPublishPartialValues()
            Using control As New AniGif()
                Dim image As Bitmap = control.Gif
                Dim delayProperty As PropertyItem = image.GetPropertyItem(&H5100)
                Dim oldDelays As Integer() = GetFrameDelays(control)
                Dim oldValues As Integer() = DirectCast(oldDelays.Clone(), Integer())
                Buffer.BlockCopy(BitConverter.GetBytes(20), 0, delayProperty.Value, 0, 4)
                Buffer.BlockCopy(BitConverter.GetBytes(Integer.MaxValue), 0, delayProperty.Value, 4, 4)
                image.SetPropertyItem(delayProperty)

                Assert.ThrowsExactly(Of OverflowException)(Sub() control.FramesPerSecond = 20D)
                Assert.AreEqual(20D, control.FramesPerSecond)
                Assert.AreSame(oldDelays, GetFrameDelays(control))
                CollectionAssert.AreEqual(oldValues, GetFrameDelays(control))
                Assert.ThrowsExactly(Of OverflowException)(Sub() control.MaximumFrameIntervalMs = 400)
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
                    Assert.IsGreaterThan(1L, GetLastFrameTimestamp(control))
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

        <TestMethod>
        Public Sub DiagnosticsCollector_CalculatesRatesDropsAndRenderTimesFromActiveIntervals()
            Dim collectorType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".AniGifDiagnosticsCollector")
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
            Dim collectorType As Type = GetType(AniGif).Assembly.GetType(GetType(AniGif).Namespace & ".AniGifDiagnosticsCollector")
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

        Private Shared Sub InvokeDiagnosticsCollector(collector As Object, name As String, ParamArray arguments As Object())
            Dim method As MethodInfo = collector.GetType().GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Dim unused = method.Invoke(collector, arguments)
        End Sub

        Private Shared Function GetPendingDiagnosticsGeneration(collector As Object) As Long
            Dim propertyInfo As PropertyInfo = collector.GetType().GetProperty("PendingFrameGeneration", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(propertyInfo)
            Return CLng(propertyInfo.GetValue(collector))
        End Function

        Private Shared Function GetDiagnosticsCollectorSnapshot(collector As Object, isEnabled As Boolean, timestamp As Long) As AniGifDiagnostics
            Dim method As MethodInfo = collector.GetType().GetMethod("CreateSnapshot", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(method)
            Return DirectCast(method.Invoke(collector, New Object() {isEnabled, timestamp}), AniGifDiagnostics)
        End Function

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

            Public Sub DestroyHandleForTest()
                Me.DestroyHandle()
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

        Private NotInheritable Class TrackingTimer
            Inherits System.Windows.Forms.Timer

            Private _startCalls As Integer
            Private _stopCalls As Integer

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

            Public Sub ResetCounts()
                Me._startCalls = 0
                Me._stopCalls = 0
            End Sub
        End Class

        Private Shared Sub InstallTrackingTimer(control As AniGif, timer As TrackingTimer)
            Dim timerProperty As PropertyInfo = GetType(AniGif).GetProperty("Timer", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(timerProperty)
            timer.Interval = GetTimer(control).Interval
            timerProperty.SetValue(control, timer)
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

        Private Shared Function GetRawFrameDelays(control As AniGif) As Integer()
            Dim delayField As FieldInfo = GetType(AniGif).GetField("_rawFrameDelaysMs", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(delayField)
            Return DirectCast(delayField.GetValue(control), Integer())
        End Function

        Private Shared Function GetGifWithoutExposure(control As AniGif) As Bitmap
            Dim imageField As FieldInfo = GetType(AniGif).GetField("_Gif", BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(imageField)
            Return DirectCast(imageField.GetValue(control), Bitmap)
        End Function

        Private Shared Function CreateDelayBytes(ParamArray delays As Integer()) As Byte()
            Dim bytes(delays.Length * 4 - 1) As Byte
            For i As Integer = 0 To delays.Length - 1
                Buffer.BlockCopy(BitConverter.GetBytes(delays(i)), 0, bytes, i * 4, 4)
            Next
            Return bytes
        End Function

        Private Shared Sub SetDelayBytes(image As Bitmap, delayProperty As PropertyItem, bytes As Byte())
            delayProperty.Value = bytes
            delayProperty.Len = bytes.Length
            image.SetPropertyItem(delayProperty)
        End Sub

        Private Shared Sub SetFirstFrameDelay(image As Bitmap, delayProperty As PropertyItem, centiseconds As Integer)
            Buffer.BlockCopy(BitConverter.GetBytes(centiseconds), 0, delayProperty.Value, 0, 4)
            image.SetPropertyItem(delayProperty)
        End Sub

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
