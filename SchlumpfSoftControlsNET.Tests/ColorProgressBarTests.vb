Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.ColorProgressBarControl

Namespace SchlumpfSoftControlsNET.Tests.ColorProgressBarControl
    <STATestClass>
    Public Class ColorProgressBarTests
        <TestMethod>
        Public Sub Value_IsClampedToValidRange()
            Using control As New ColorProgressBar()
                control.ProgressMaximumValue = 10

                control.Value = -5
                Assert.AreEqual(0, control.Value)

                control.Value = 15
                Assert.AreEqual(10, control.Value)
            End Using
        End Sub

        <TestMethod>
        Public Sub ProgressMaximumValue_IsClampedAndAdjustsCurrentValue()
            Using control As New ColorProgressBar()
                control.Value = 8

                control.ProgressMaximumValue = 0

                Assert.AreEqual(1, control.ProgressMaximumValue)
                Assert.AreEqual(1, control.Value)
            End Using
        End Sub

        <TestMethod>
        Public Sub ShowBorder_UpdatesPadding()
            Using control As New ColorProgressBar()
                control.ShowBorder = True
                Assert.AreEqual(New Padding(1), control.Padding)

                control.ShowBorder = False
                Assert.AreEqual(New Padding(0), control.Padding)

                control.Padding = New Padding(3)
                Assert.AreEqual(New Padding(0), control.Padding)
            End Using
        End Sub

        <TestMethod>
        Public Sub IsGlossy_TogglesOverlayOnBothAreas()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 2)
                    AssertPixel(Color.Silver, image, 75, 2)
                End Using

                control.IsGlossy = True
                Using image As Bitmap = RenderControl(control)
                    Dim gloss As Color = Color.FromArgb(100, 255, 255, 255)
                    AssertPixel(BlendColors(Color.Blue, gloss), image, 10, 2)
                    AssertPixel(BlendColors(Color.Silver, gloss), image, 75, 2)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ColorProperties_UpdateRenderedAreas()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                control.BarColor = Color.Orange
                control.EmptyColor = Color.LightGray
                control.BorderColor = Color.DarkBlue

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Orange, image, 10, 10)
                    AssertPixel(Color.LightGray, image, 75, 10)
                    AssertPixel(Color.DarkBlue, image, 0, 10)
                    Assert.AreEqual(Color.DarkBlue, control.BackColor)
                End Using

                control.BackColor = Color.Magenta
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Magenta, image, 0, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub TransparentColors_AreDrawnOverTheBackground()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                control.BorderColor = Color.White
                control.BarColor = Color.FromArgb(128, Color.Red)
                control.EmptyColor = Color.FromArgb(128, Color.Blue)

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(BlendColors(Color.White, control.BarColor), image, 10, 10)
                    AssertPixel(BlendColors(Color.White, control.EmptyColor), image, 75, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub ShowBorder_TogglesRenderedBorder()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                control.BarColor = Color.Red

                control.ShowBorder = True
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Black, image, 0, 10)
                    AssertPixel(Color.Black, image, 99, 10)
                End Using

                control.ShowBorder = False
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Red, image, 0, 10)
                    AssertPixel(Color.Silver, image, 99, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Value_UpdatesProgressWidthProportionally()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.ProgressMaximumValue = 10
                control.IsGlossy = False

                control.Value = 0
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 10)
                    AssertPixel(Color.Silver, image, 98, 10)
                End Using

                control.Value = 5
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 50, 10)
                    AssertPixel(Color.Silver, image, 51, 10)
                End Using

                control.Value = 10
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 1, 10)
                    AssertPixel(Color.Blue, image, 98, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Resize_UpdatesFillAndGlossGeometry()
            Using control As New ColorProgressBar()
                control.Size = New Size(80, 20)
                control.Value = 5

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 40, 10)
                    AssertPixel(Color.Silver, image, 41, 10)
                End Using

                control.Size = New Size(100, 30)
                Using image As Bitmap = RenderControl(control)
                    Dim gloss As Color = Color.FromArgb(100, 255, 255, 255)
                    AssertPixel(BlendColors(Color.Blue, gloss), image, 25, 10)
                    AssertPixel(Color.Blue, image, 25, 11)
                    AssertPixel(Color.Blue, image, 50, 15)
                    AssertPixel(Color.Silver, image, 51, 15)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub SmallSizes_ArePaintedWithoutInvalidRectangles()
            Using control As New TestableColorProgressBar()
                control.Value = 10
                For Each size As Size In {New Size(0, 0), New Size(1, 1), New Size(2, 2), New Size(3, 3)}
                    control.Size = size
                    Using image As New Bitmap(3, 3)
                        Using graphics As Graphics = Graphics.FromImage(image)
                            Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                                control.PaintForTest(args)
                            End Using
                        End Using
                    End Using
                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub AppearanceChangesAndResize_InvalidateControl()
            Using control As New ColorProgressBar()
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Integer = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                Dim changes As Action() = {
                    Sub() control.Value = 5,
                    Sub() control.ProgressMaximumValue = 20,
                    Sub() control.BarColor = Color.Red,
                    Sub() control.EmptyColor = Color.White,
                    Sub() control.BorderColor = Color.Green,
                    Sub() control.ShowBorder = False,
                    Sub() control.IsGlossy = False,
                    Sub() control.Size = New Size(120, 25)
                }

                For Each change As Action In changes
                    invalidationCount = 0
                    change()
                    Assert.IsTrue(invalidationCount > 0)
                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub Paint_RaisesEventAfterDrawing()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                Dim paintCount As Integer = 0
                AddHandler control.Paint, Sub(sender, e)
                                              paintCount += 1
                                              e.Graphics.FillRectangle(Brushes.Yellow, 10, 10, 1, 1)
                                          End Sub

                Using image As Bitmap = RenderControl(control)
                    Assert.IsTrue(paintCount > 0)
                    AssertPixel(Color.Yellow, image, 10, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Click_UsesTheControlAsSenderWithoutChildPanels()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                Assert.AreEqual(0, control.Controls.Count)
                Assert.IsTrue(control.StandardClickEnabled)

                Dim clickCount As Integer = 0
                Dim eventSender As Object = Nothing
                AddHandler control.Click, Sub(sender, e)
                                              clickCount += 1
                                              eventSender = sender
                                          End Sub

                control.RaiseClickForTest()
                Assert.AreEqual(1, clickCount)
                Assert.AreSame(control, eventSender)
            End Using
        End Sub

        Private Shared Function RenderControl(control As ColorProgressBar) As Bitmap
            Dim image As New Bitmap(control.Width, control.Height)
            Try
                control.DrawToBitmap(image, New Rectangle(Point.Empty, image.Size))
                Return image
            Catch
                image.Dispose()
                Throw
            End Try
        End Function

        Private Shared Sub AssertPixel(expected As Color, image As Bitmap, x As Integer, y As Integer)
            Assert.AreEqual(expected.ToArgb(), image.GetPixel(x, y).ToArgb(), $"Unerwartete Farbe bei ({x}, {y}).")
        End Sub

        Private Shared Function BlendColors(background As Color, overlay As Color) As Color
            Using image As New Bitmap(1, 1)
                Using graphics As Graphics = Graphics.FromImage(image)
                    graphics.Clear(background)
                    Using brush As New SolidBrush(overlay)
                        graphics.FillRectangle(brush, 0, 0, 1, 1)
                    End Using
                End Using
                Return image.GetPixel(0, 0)
            End Using
        End Function

        Private Class TestableColorProgressBar
            Inherits ColorProgressBar

            Public ReadOnly Property StandardClickEnabled As Boolean
                Get
                    Return Me.GetStyle(ControlStyles.StandardClick)
                End Get
            End Property

            Public Sub PaintForTest(e As PaintEventArgs)
                MyBase.OnPaint(e)
            End Sub

            Public Sub RaiseClickForTest()
                Me.OnClick(EventArgs.Empty)
            End Sub
        End Class
    End Class
End Namespace
