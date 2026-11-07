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
