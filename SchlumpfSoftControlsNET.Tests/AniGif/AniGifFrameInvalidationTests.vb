' --------------------------------------------------------------------------------------------------------
' Datei: AniGifFrameInvalidationTests.vb
' Author: Andreas Sauer
' Datum: 14.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.AniGifControl

Namespace SchlumpfSoftControlsNET.Tests.AniGifControl
    Partial Public Class AniGifTests

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

        <MethodImpl(MethodImplOptions.NoInlining)>
        Private Shared Function CreateDisposedFrameControl(parent As Control) As WeakReference
            Dim control As New AniGif()
            parent.Controls.Add(control)
            control.UsePartialFrameInvalidation = True
            Dim released As New WeakReference(control)
            control.Dispose()
            Return released
        End Function

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

        Private Shared Sub FillFrameBackground(surface As Bitmap, bounds As Rectangle, color As Color)
            Using graphics As Graphics = Graphics.FromImage(surface)
                Using brush As New SolidBrush(color)
                    graphics.FillRectangle(brush, bounds)
                End Using
            End Using
        End Sub

        Private Shared Sub AssertLastFrameInvalidation(invalidations As List(Of Rectangle), expected As Rectangle)
            Assert.IsNotEmpty(invalidations)
            Assert.AreEqual(expected, invalidations(invalidations.Count - 1))
        End Sub

        Private Shared Function UnionFrameInvalidations(invalidations As List(Of Rectangle)) As Rectangle
            Assert.IsNotEmpty(invalidations)
            Dim combined As Rectangle = invalidations(0)
            For index As Integer = 1 To invalidations.Count - 1
                combined = Rectangle.Union(combined, invalidations(index))
            Next
            Return combined
        End Function

        Private Shared Sub PrimeFrameInvalidation(control As FrameInvalidationProbe)
            control.StartAnimation()
            InvokeTimerTick(control)
            PaintFullFrame(control)
        End Sub

        Private Shared Sub PaintFullFrame(control As FrameInvalidationProbe)
            Using surface As New Bitmap(control.ClientSize.Width, control.ClientSize.Height)
                control.PaintFrame(surface, control.ClientRectangle)
            End Using
        End Sub

        Private NotInheritable Class FrameInvalidationProbe
            Inherits AniGif

            Public Overloads Sub PaintFrame(surface As Bitmap, clip As Rectangle)
                Using graphics As Graphics = Graphics.FromImage(surface)
                    Me.PaintFrame(graphics, clip)
                End Using
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

            Public Sub DestroyHandleForFrameTest()
                Me.DestroyHandle()
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

    End Class
End Namespace