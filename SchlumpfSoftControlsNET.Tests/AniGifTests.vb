Imports System.ComponentModel
Imports System.Drawing
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

            timerTickMethod.Invoke(control, New Object() {control, EventArgs.Empty})
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
