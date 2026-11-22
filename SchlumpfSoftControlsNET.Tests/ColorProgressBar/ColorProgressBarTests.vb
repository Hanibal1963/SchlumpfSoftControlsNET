' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBarTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.ColorProgressBarControl

Namespace SchlumpfSoftControlsNET.Tests.ColorProgressBarControl

    <STATestClass>
    Public Class ColorProgressBarTests

        ''' <summary>Prüft Bereiche, Priorität, Begrenzungen und Änderungen der Sammlung.</summary>
        <TestMethod>
        Public Sub Thresholds_RenderRangesAndReactToCollectionChanges()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 5
                control.Thresholds.Add(New ColorProgressBarThreshold(0, 5, Color.Red))
                control.Thresholds.Add(New ColorProgressBarThreshold(5, 10, Color.Lime))
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Red, image, 10, 10)
                End Using
                control.Value = 6
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Lime, image, 10, 10)
                End Using
                control.Thresholds(1) = New ColorProgressBarThreshold(6, 8, Color.Yellow)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Yellow, image, 10, 10)
                End Using
                control.Thresholds.RemoveAt(1)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                control.Value = 5
                control.Thresholds.Clear()
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                Dim unused2 = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() New ColorProgressBarThreshold(-1, 5, Color.Red))
                Dim unused1 = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Function() New ColorProgressBarThreshold(5, 4, Color.Red))
                Dim unused = Assert.ThrowsExactly(Of ArgumentNullException)(Sub() control.Thresholds.Add(Nothing))
            End Using
        End Sub

        ''' <summary>Prüft Invalidierungen und die Aktualisierung von Systemfarben.</summary>
        <TestMethod>
        Public Sub Thresholds_InvalidateAndRefreshSystemColorBrushes()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 5
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim changes As Int32 = 0
                AddHandler control.Invalidated, Sub(sender, e) changes += 1
                control.Thresholds.Add(New ColorProgressBarThreshold(0, 10, SystemColors.Highlight))
                Assert.AreEqual(1, changes)
                Dim brush As SolidBrush
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(SystemColors.Highlight, image, 10, 10)
                    brush = CachedBrush(control, "_barBrush")
                End Using
                control.RaiseSystemColorsChangedForTest()
                AssertBrushDisposed(brush)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(SystemColors.Highlight, image, 10, 10)
                End Using
                control.Thresholds.Clear()
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft Schwellenfarben bei animierten Werten und kombinierten Darstellungsmodi.</summary>
        <TestMethod>
        Public Sub Thresholds_FollowDisplayedValueAcrossStylesAndAnimation()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 0
                control.Thresholds.Add(New ColorProgressBarThreshold(0, 4, Color.Red))
                control.Thresholds.Add(New ColorProgressBarThreshold(5, 10, Color.Lime))
                control.AnimateValueChanges = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.Value = 10
                Dim advance As MethodInfo = GetType(ColorProgressBar).GetMethod("UpdateAnimatedValue", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(advance)
                Dim unused1 = advance.Invoke(control, New Object() {150.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Lime, image, 10, 10)
                End Using
                Dim unused = advance.Invoke(control, New Object() {300.0})
                control.ShowSegments = True
                control.ProgressStyle = ColorProgressBarStyle.Striped
                Using image As Bitmap = RenderControl(control)
                    Assert.AreNotEqual(Color.Blue.ToArgb(), image.GetPixel(10, 10).ToArgb())
                End Using
                control.ProgressStyle = ColorProgressBarStyle.Marquee
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Lime, image, 8, 10)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft die gleichmäßige Aufteilung und kontinuierliche Füllung über Segmentgrenzen.</summary>
        <TestMethod>
        Public Sub Segments_RenderFillAndGapsWithoutChangingProgress()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.ShowBorder = False
                control.IsGlossy = False
                control.Value = 5
                Assert.IsFalse(control.ShowSegments)
                Assert.AreEqual(10, control.SegmentCount)
                Assert.AreEqual(2, control.SegmentSpacing)
                control.ShowSegments = True
                control.SegmentCount = 4
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Black, image, 23, 10)
                    AssertPixel(Color.Blue, image, 25, 10)
                    AssertPixel(Color.Black, image, 49, 10)
                    AssertPixel(Color.Silver, image, 51, 10)
                    AssertPixel(Color.Black, image, 74, 10)
                    AssertPixel(Color.Silver, image, 80, 10)
                End Using
                Assert.AreEqual(5, control.Value)
                control.ShowSegments = False
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 23, 10)
                    AssertPixel(Color.Silver, image, 74, 10)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft Text, Glanz, Übergang und die Wiederherstellung der Zeichenbeschränkung für Paint-Handler.</summary>
        <TestMethod>
        Public Sub Segments_InteractWithAnimationTextGlossAndPaintEvent()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 30)
                control.ShowBorder = False
                control.ShowSegments = True
                control.SegmentCount = 4
                control.IsGlossy = True
                control.ShowText = True
                control.Value = 0
                control.AnimateValueChanges = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.Value = 10
                Dim advance As MethodInfo = GetType(ColorProgressBar).GetMethod("UpdateAnimatedValue", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(advance)
                Dim unused = advance.Invoke(control, New Object() {150.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Black, image, 23, 3)
                    AssertPixel(Color.Black, image, 49, 15)
                    AssertPixel(Color.Blue, image, 80, 25)
                    AssertPixel(Color.Silver, image, 95, 25)
                End Using
                AddHandler control.Paint, Sub(sender, e) e.Graphics.FillRectangle(Brushes.Red, 23, 15, 1, 1)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Red, image, 23, 15)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft Segmentierung bei allen Stilen, Null und Maximum sowie engen Zeichenflächen.</summary>
        <TestMethod>
        Public Sub Segments_SupportStylesProgressAndNarrowSizes()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.ShowBorder = False
                control.IsGlossy = False
                control.ShowSegments = True
                control.SegmentCount = 4
                For Each style As ColorProgressBarStyle In {ColorProgressBarStyle.Solid, ColorProgressBarStyle.Gradient,
                        ColorProgressBarStyle.Striped, ColorProgressBarStyle.Marquee}
                    control.ProgressStyle = style
                    For Each value As Int32 In {0, 10}
                        control.Value = value
                        Using image As Bitmap = RenderControl(control)
                            AssertPixel(Color.Black, image, 23, 10)
                            AssertPixel(Color.Black, image, 49, 10)
                            If style <> ColorProgressBarStyle.Marquee AndAlso value = 0 Then AssertPixel(Color.Silver, image, 10, 10)
                            If style = ColorProgressBarStyle.Solid AndAlso value = 10 Then AssertPixel(Color.Blue, image, 80, 10)
                        End Using
                    Next
                Next
                control.ProgressStyle = ColorProgressBarStyle.Solid
                control.SegmentCount = 1000
                control.SegmentSpacing = 1000
                control.Size = New Size(3, 5)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 0, 2)
                    AssertPixel(Color.Blue, image, 2, 2)
                End Using
                control.SegmentCount = 1
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 1, 2)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft die Grenzen und Designer-Standardwerte der Segmenteinstellungen.</summary>
        <TestMethod>
        Public Sub Segments_ValidatePropertiesAndDesignerDefaults()
            Using control As New ColorProgressBar()
                Dim properties As PropertyDescriptorCollection = TypeDescriptor.GetProperties(control)
                For Each name As String In {NameOf(ColorProgressBar.ShowSegments), NameOf(ColorProgressBar.SegmentCount), NameOf(ColorProgressBar.SegmentSpacing)}
                    Assert.IsFalse(properties(name).ShouldSerializeValue(control), name)
                Next
                Dim unused3 = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SegmentCount = 0)
                Dim unused2 = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SegmentCount = 1001)
                Dim unused1 = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SegmentSpacing = -1)
                Dim unused = Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() control.SegmentSpacing = 1001)
                Assert.AreEqual(10, control.SegmentCount)
                Assert.AreEqual(2, control.SegmentSpacing)
                control.SegmentSpacing = 0
                Assert.IsTrue(properties(NameOf(ColorProgressBar.SegmentSpacing)).ShouldSerializeValue(control))
            End Using
        End Sub

        ''' <summary>Prüft Zwischenstand, Zielwert und Abschluss des zeitbasierten Übergangs.</summary>
        <TestMethod>
        Public Sub ValueAnimation_InterpolatesDrawingWithoutChangingTheReportedValue()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 0
                Assert.IsFalse(control.AnimateValueChanges)
                control.AnimateValueChanges = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.Value = 10
                Assert.AreEqual(10, control.Value)
                Assert.AreEqual(1.0.ToString("P0", Globalization.CultureInfo.CurrentCulture), control.AccessibilityObject.Value)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 80, 10)
                End Using
                Dim advance As MethodInfo = GetType(ColorProgressBar).GetMethod("UpdateAnimatedValue", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(advance)
                Dim unused = advance.Invoke(control, New Object() {150.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 80, 10)
                    AssertPixel(Color.Silver, image, 95, 10)
                End Using
                unused = advance.Invoke(control, New Object() {300.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 98, 10)
                End Using
                Assert.AreEqual(10, control.Value)
                control.Value = 0
                unused = advance.Invoke(control, New Object() {150.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 20, 10)
                End Using
                unused = advance.Invoke(control, New Object() {300.0})
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 10)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft Timer-Lebensdauer, Richtungswechsel und Synchronisierung bei Zustandsänderungen.</summary>
        <TestMethod>
        Public Sub ValueAnimation_StopsAndSynchronizesAcrossLifecycleChanges()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 0
                control.AnimateValueChanges = True
                Dim timerField As FieldInfo = GetType(ColorProgressBar).GetField("_valueAnimationTimer", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(timerField)
                control.Value = 5
                Assert.IsNull(timerField.GetValue(control))
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.Value = 10
                Dim timer As Timer = DirectCast(timerField.GetValue(control), Timer)
                Assert.IsNotNull(timer)
                Assert.IsTrue(timer.Enabled)
                control.Value = 2
                Assert.AreEqual(2, control.Value)
                Assert.IsTrue(timer.Enabled)
                control.Visible = False
                Assert.IsFalse(timer.Enabled)
                control.Visible = True
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 80, 10)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                control.Value = 8
                Assert.IsTrue(timer.Enabled)
                control.ProgressMaximumValue = 5
                Assert.IsFalse(timer.Enabled)
                Assert.AreEqual(5, control.Value)
                control.Value = 2
                Assert.IsTrue(timer.Enabled)
                control.ProgressStyle = ColorProgressBarStyle.Marquee
                Assert.IsFalse(timer.Enabled)
                control.ProgressStyle = ColorProgressBarStyle.Solid
                control.Value = 4
                Assert.IsTrue(timer.Enabled)
                control.RecreateHandleForTest()
                Assert.IsFalse(timer.Enabled)
                control.Value = 1
                Assert.IsTrue(timer.Enabled)
                control.AnimateValueChanges = False
                Assert.IsFalse(timer.Enabled)
                control.AnimateValueChanges = True
                control.Value = 4
                Assert.IsTrue(timer.Enabled)
                control.Dispose()
                Assert.IsFalse(timer.Enabled)
            End Using
        End Sub

        ''' <summary>Prüft, dass der echte WinForms-Timer den Übergang in der Nachrichtenverarbeitung abschließt.</summary>
        <TestMethod>
        Public Sub ValueAnimation_TimerCompletesTransition()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 0
                control.AnimateValueChanges = True
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                control.Value = 10
                Dim timerField As FieldInfo = GetType(ColorProgressBar).GetField("_valueAnimationTimer", BindingFlags.Instance Or BindingFlags.NonPublic)
                Dim timer As Timer = DirectCast(timerField.GetValue(control), Timer)
                Dim timeout As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
                While timer.Enabled AndAlso timeout.ElapsedMilliseconds < 3000
                    Application.DoEvents()
                    Threading.Thread.Sleep(10)
                End While
                Assert.IsFalse(timer.Enabled, "Der Übergang wurde nicht rechtzeitig abgeschlossen.")
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 98, 10)
                End Using
            End Using
        End Sub

        ''' <summary>Prüft einen deutlich sichtbaren Verlauf unterhalb des optionalen Glanzeffekts.</summary>
        <TestMethod>
        Public Sub Gradient_ShowsVisibleVerticalVariationWithAndWithoutGloss()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 23)
                control.Value = control.ProgressMaximumValue
                control.ProgressStyle = ColorProgressBarStyle.Gradient
                For Each color As Color In {Color.Blue, Color.Black, Color.White}
                    control.BarColor = color
                    For Each glossy As Boolean In {False, True}
                        control.IsGlossy = glossy
                        Using image As Bitmap = RenderControl(control)
                            Dim upper As Color = image.GetPixel(20, 9)
                            Dim lower As Color = image.GetPixel(20, 20)
                            Dim difference As Int32 = Math.Max(Math.Abs(CInt(upper.R) - CInt(lower.R)),
                                Math.Max(Math.Abs(CInt(upper.G) - CInt(lower.G)), Math.Abs(CInt(upper.B) - CInt(lower.B))))
                            Assert.IsGreaterThan(45, difference, $"Farbe {color}, Glanz {glossy}")
                        End Using
                    Next
                Next
            End Using
        End Sub

        ''' <summary>Prüft die Stilwahl und die unterschiedliche Darstellung innerhalb der Fortschrittsfläche.</summary>
        <TestMethod>
        Public Sub ProgressStyle_RendersDistinctStylesAndValidatesInput()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 30)
                control.IsGlossy = False
                control.Value = 5
                Assert.AreEqual(ColorProgressBarStyle.Solid, control.ProgressStyle)
                Dim descriptor As PropertyDescriptor = TypeDescriptor.GetProperties(control)(NameOf(ColorProgressBar.ProgressStyle))
                Dim defaultValue As DefaultValueAttribute = DirectCast(descriptor.Attributes(GetType(DefaultValueAttribute)), DefaultValueAttribute)
                Assert.AreEqual(ColorProgressBarStyle.Solid, defaultValue.Value)
                Assert.IsFalse(descriptor.ShouldSerializeValue(control))
                Using solid As Bitmap = RenderControl(control)
                    For Each style As ColorProgressBarStyle In {ColorProgressBarStyle.Gradient, ColorProgressBarStyle.Striped}
                        control.ProgressStyle = style
                        Using styled As Bitmap = RenderControl(control)
                            Dim differences As Int32 = 0
                            For y As Int32 = 1 To 28
                                For x As Int32 = 1 To 50
                                    If solid.GetPixel(x, y) <> styled.GetPixel(x, y) Then differences += 1
                                Next
                            Next
                            Assert.IsGreaterThan(0, differences, style.ToString())
                            AssertPixel(Color.Silver, styled, 80, 15)
                        End Using
                    Next
                End Using
                Dim unused = Assert.ThrowsExactly(Of InvalidEnumArgumentException)(Sub() control.ProgressStyle = CType(-1, ColorProgressBarStyle))
                Assert.AreEqual(ColorProgressBarStyle.Striped, control.ProgressStyle)
                Assert.IsTrue(descriptor.ShouldSerializeValue(control))
            End Using
        End Sub

        ''' <summary>Prüft kleine Zeichenflächen und die Extremfarben in allen Darstellungsmodi.</summary>
        <TestMethod>
        Public Sub ProgressStyle_RendersSmallSizesAndExtremeColors()
            Using control As New ColorProgressBar()
                control.IsGlossy = False
                control.ShowText = True
                For Each style As ColorProgressBarStyle In {ColorProgressBarStyle.Solid, ColorProgressBarStyle.Gradient,
                        ColorProgressBarStyle.Striped, ColorProgressBarStyle.Marquee}
                    control.ProgressStyle = style
                    For Each size As Size In {New Size(1, 1), New Size(3, 3), New Size(60, 20)}
                        control.Size = size
                        For Each color As Color In {Color.Black, Color.White, Color.Transparent}
                            control.BarColor = color
                            For Each value As Int32 In {0, 10}
                                control.Value = value
                                Using image As Bitmap = RenderControl(control)
                                    Assert.AreEqual(size, image.Size)
                                End Using
                            Next
                        Next
                    Next
                Next
            End Using
        End Sub

        ''' <summary>Prüft Animation, unveränderte Fortschrittswerte und den Lebenszyklus des Timers.</summary>
        <TestMethod>
        Public Sub Marquee_AnimatesAndStopsWhenNotVisibleOrNotSelected()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 30)
                control.IsGlossy = False
                control.Value = 5
                control.ProgressStyle = ColorProgressBarStyle.Marquee
                Dim timerField As FieldInfo = GetType(ColorProgressBar).GetField("_marqueeTimer", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(timerField)
                Assert.IsNull(timerField.GetValue(control))
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim timer As Timer = DirectCast(timerField.GetValue(control), Timer)
                Assert.IsNotNull(timer)
                Assert.IsTrue(timer.Enabled)
                Dim tick As MethodInfo = GetType(ColorProgressBar).GetMethod("MarqueeTick", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(tick)
                Using before As Bitmap = RenderControl(control)
                    Dim unused = tick.Invoke(control, New Object() {timer, EventArgs.Empty})
                    Using after As Bitmap = RenderControl(control)
                        Assert.AreNotEqual(before.GetPixel(7, 15).ToArgb(), after.GetPixel(7, 15).ToArgb())
                    End Using
                End Using
                Assert.AreEqual(5, control.Value)
                control.Visible = False
                Assert.IsFalse(timer.Enabled)
                control.Visible = True
                Assert.IsTrue(timer.Enabled)
                control.RecreateHandleForTest()
                Assert.IsTrue(timer.Enabled)
                control.ProgressStyle = ColorProgressBarStyle.Solid
                Assert.IsFalse(timer.Enabled)
                control.ProgressStyle = ColorProgressBarStyle.Marquee
                Assert.IsTrue(timer.Enabled)
                control.Dispose()
                Assert.IsFalse(timer.Enabled)
            End Using
        End Sub

        ''' <summary>Prüft Standardwerte, Formatvalidierung und die Zeichnung der optionalen Beschriftung.</summary>
        <TestMethod>
        Public Sub ProgressText_FormatsAndVisibilityChangeRendering()
            Using control As New ColorProgressBar()
                control.Size = New Size(220, 40)
                control.IsGlossy = False
                control.Value = 5
                Assert.IsFalse(control.ShowText)
                Assert.AreEqual(ColorProgressBarTextFormat.Percentage, control.TextFormat)
                Using withoutText As Bitmap = RenderControl(control)
                    control.ShowText = True
                    Using percentage As Bitmap = RenderControl(control)
                        control.TextFormat = ColorProgressBarTextFormat.Value
                        Using value As Bitmap = RenderControl(control)
                            Dim visiblePixels As Int32 = 0
                            Dim differentPixels As Int32 = 0
                            For y As Int32 = 1 To control.Height - 2
                                For x As Int32 = 1 To control.Width - 2
                                    If withoutText.GetPixel(x, y) <> percentage.GetPixel(x, y) Then visiblePixels += 1
                                    If percentage.GetPixel(x, y) <> value.GetPixel(x, y) Then differentPixels += 1
                                Next
                            Next
                            Assert.IsGreaterThan(0, visiblePixels)
                            Assert.IsGreaterThan(0, differentPixels)
                        End Using
                    End Using
                End Using
                Dim unused = Assert.ThrowsExactly(Of InvalidEnumArgumentException)(Sub() control.TextFormat = CType(-1, ColorProgressBarTextFormat))
                Assert.AreEqual(ColorProgressBarTextFormat.Value, control.TextFormat)
            End Using
        End Sub

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
        Public Sub Progress_DefaultValuesMatchDesignerAttributes()
            Using control As New ColorProgressBar()
                Dim properties As PropertyDescriptorCollection = TypeDescriptor.GetProperties(control)
                Dim valueProperty As PropertyDescriptor = properties(NameOf(ColorProgressBar.Value))
                Dim maximumProperty As PropertyDescriptor = properties(NameOf(ColorProgressBar.ProgressMaximumValue))
                Assert.IsNotNull(valueProperty)
                Assert.IsNotNull(maximumProperty)

                Dim valueDefault As DefaultValueAttribute = DirectCast(valueProperty.Attributes(GetType(DefaultValueAttribute)), DefaultValueAttribute)
                Dim maximumDefault As DefaultValueAttribute = DirectCast(maximumProperty.Attributes(GetType(DefaultValueAttribute)), DefaultValueAttribute)
                Assert.IsNotNull(valueDefault)
                Assert.IsNotNull(maximumDefault)
                Assert.AreEqual(1, control.Value)
                Assert.AreEqual(10, control.ProgressMaximumValue)
                Assert.AreEqual(control.Value, DirectCast(valueDefault.Value, Int32))
                Assert.AreEqual(control.ProgressMaximumValue, DirectCast(maximumDefault.Value, Int32))
            End Using
        End Sub

        <TestMethod>
        <DataRow(Int32.MinValue, Int32.MinValue, 1, 0)>
        <DataRow(0, 0, 1, 0)>
        <DataRow(1, 1, 1, 1)>
        <DataRow(-5, Int32.MaxValue, 1, 1)>
        <DataRow(10, -1, 10, 0)>
        <DataRow(10, 5, 10, 5)>
        <DataRow(10, 11, 10, 10)>
        <DataRow(Int32.MaxValue, Int32.MinValue, Int32.MaxValue, 0)>
        <DataRow(Int32.MaxValue, Int32.MaxValue - 1, Int32.MaxValue, Int32.MaxValue - 1)>
        <DataRow(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue)>
        Public Sub Progress_BoundsInt32Inputs(maximum As Int32, value As Int32, expectedMaximum As Int32, expectedValue As Int32)
            Using control As New ColorProgressBar()
                control.ProgressMaximumValue = maximum
                control.Value = value

                Assert.AreEqual(expectedMaximum, control.ProgressMaximumValue)
                Assert.AreEqual(expectedValue, control.Value)
            End Using
        End Sub

        <TestMethod>
        Public Sub Progress_Int32LimitsRenderCorrectly()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.ProgressMaximumValue = Int32.MaxValue
                control.Value = Int32.MinValue

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 10)
                    AssertPixel(Color.Silver, image, 98, 10)
                End Using

                control.Value = Int32.MaxValue \ 2
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 50, 10)
                    AssertPixel(Color.Silver, image, 51, 10)
                End Using

                control.Value = Int32.MaxValue
                control.ProgressMaximumValue = Int32.MinValue
                Assert.AreEqual(1, control.ProgressMaximumValue)
                Assert.AreEqual(1, control.Value)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 1, 10)
                    AssertPixel(Color.Blue, image, 98, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Progress_IncreasingMaximumDoesNotRestorePreviouslyClampedValue()
            Using control As New ColorProgressBar()
                control.Value = 20
                Assert.AreEqual(10, control.Value)

                control.ProgressMaximumValue = 20
                Assert.AreEqual(20, control.ProgressMaximumValue)
                Assert.AreEqual(10, control.Value)

                control.Value = 20
                Assert.AreEqual(20, control.Value)
            End Using
        End Sub

        <TestMethod>
        Public Sub ProgressMaximumValue_ClampsBeforeReentrantInvalidation()

            Using control As New TestableColorProgressBar()

                control.Size = New Size(100, 20)
                control.IsGlossy = False
                control.Value = 8

                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)

                Dim observedValues As New List(Of Int32)()
                Dim observedMaximums As New List(Of Int32)()

                Using image As New Bitmap(100, 20)

                    Using graphics As Graphics = Graphics.FromImage(image)

                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))

                            control.PaintForTest(args)
                            Dim onInvalidated As InvalidateEventHandler = Sub(sender, e)
                                                                              observedValues.Add(control.Value)
                                                                              observedMaximums.Add(control.ProgressMaximumValue)
                                                                              If observedValues.Count = 1 Then
                                                                                  graphics.Clear(Color.Magenta)
                                                                                  control.PaintForTest(args)
                                                                                  AssertPixel(Color.Blue, image, 98, 10)
                                                                                  control.ProgressMaximumValue = 20
                                                                                  control.Value = 7
                                                                              End If
                                                                          End Sub
                            AddHandler control.Invalidated, onInvalidated

                            Try
                                control.ProgressMaximumValue = 4
                            Finally
                                RemoveHandler control.Invalidated, onInvalidated
                            End Try

                            Dim intvar1 As Int32() = New Int32() {4, 4, 7}
                            CollectionAssert.AreEqual(intvar1, observedValues.ToArray())

                            Dim intvar2 As Int32() = New Int32() {4, 20, 20}
                            CollectionAssert.AreEqual(intvar2, observedMaximums.ToArray())

                            Assert.AreEqual(7, control.Value)
                            Assert.AreEqual(20, control.ProgressMaximumValue)

                        End Using

                    End Using

                End Using

            End Using

        End Sub

        <TestMethod>
        Public Sub Value_OnlyInvalidatesWhenTheBoundedValueChanges()
            Using control As New ColorProgressBar()
                control.Value = 5
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Int32 = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                control.Value = 5
                Assert.AreEqual(0, invalidationCount)

                control.Value = Int32.MinValue
                Assert.AreEqual(0, control.Value)
                Assert.AreEqual(1, invalidationCount)

                control.Value = -1
                control.Value = 0
                Assert.AreEqual(1, invalidationCount)

                control.Value = Int32.MaxValue
                Assert.AreEqual(10, control.Value)
                Assert.AreEqual(2, invalidationCount)

                control.Value = 11
                control.Value = 10
                Assert.AreEqual(2, invalidationCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub ProgressMaximumValue_OnlyInvalidatesWhenTheBoundedMaximumChanges()
            Using control As New ColorProgressBar()
                control.Value = 8
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Int32 = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                control.ProgressMaximumValue = 10
                Assert.AreEqual(0, invalidationCount)

                control.ProgressMaximumValue = 0
                Assert.AreEqual(1, control.ProgressMaximumValue)
                Assert.AreEqual(1, control.Value)
                Assert.AreEqual(1, invalidationCount)

                control.ProgressMaximumValue = Int32.MinValue
                control.ProgressMaximumValue = 1
                Assert.AreEqual(1, invalidationCount)

                control.ProgressMaximumValue = Int32.MaxValue
                Assert.AreEqual(Int32.MaxValue, control.ProgressMaximumValue)
                Assert.AreEqual(1, control.Value)
                Assert.AreEqual(2, invalidationCount)

                control.ProgressMaximumValue = Int32.MaxValue
                Assert.AreEqual(2, invalidationCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub StateChanges_InvalidateEvenWhenRoundedFillWidthIsUnchanged()
            Using control As New ColorProgressBar()
                control.Size = New Size(10, 10)
                control.ProgressMaximumValue = 1000
                control.Value = 1
                control.IsGlossy = False
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Int32 = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 5)
                End Using

                invalidationCount = 0
                control.Value = 2
                Assert.AreEqual(2, control.Value)
                Assert.AreEqual(1, invalidationCount)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 5)
                End Using

                invalidationCount = 0
                control.ProgressMaximumValue = 2000
                Assert.AreEqual(2, control.Value)
                Assert.AreEqual(1, invalidationCount)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 5)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub SystemColorChange_ReplacesOnlyBrushesUsingSystemColors()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim systemColor As Color = SystemColors.Highlight
                Dim fixedColor As Color = Color.FromArgb(systemColor.ToArgb())
                control.BarColor = systemColor
                control.EmptyColor = fixedColor

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(systemColor, image, 10, 10)
                    AssertPixel(fixedColor, image, 75, 10)
                End Using
                Dim systemBar As SolidBrush = CachedBrush(control, "_barBrush")
                Dim fixedEmpty As SolidBrush = CachedBrush(control, "_emptyBrush")
                Assert.IsNotNull(systemBar)
                Assert.IsNotNull(fixedEmpty)

                control.RaiseSystemColorsChangedForTest()
                Assert.IsNull(CachedBrush(control, "_barBrush"))
                Assert.AreSame(fixedEmpty, CachedBrush(control, "_emptyBrush"))
                AssertBrushDisposed(systemBar)

                control.BarColor = fixedColor
                control.EmptyColor = systemColor
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(fixedColor, image, 10, 10)
                    AssertPixel(systemColor, image, 75, 10)
                End Using
                Dim fixedBar As SolidBrush = CachedBrush(control, "_barBrush")
                Dim systemEmpty As SolidBrush = CachedBrush(control, "_emptyBrush")
                Assert.IsNotNull(fixedBar)
                Assert.IsNotNull(systemEmpty)

                control.RaiseSystemColorsChangedForTest()
                Assert.AreSame(fixedBar, CachedBrush(control, "_barBrush"))
                Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                AssertBrushDisposed(systemEmpty)
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
        Public Sub TransparentColorChanges_AfterPaintingUseCurrentColors()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using

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
        Public Sub Brushes_AreCreatedLazilyAndReusedAcrossPaints()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 0
                control.IsGlossy = False

                Assert.IsNull(CachedBrush(control, "_barBrush"))
                Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                Assert.IsNull(CachedBrush(control, "_glossBrush"))

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 10, 10)
                End Using
                Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                Assert.IsNotNull(emptyBrush)
                Assert.IsNull(CachedBrush(control, "_barBrush"))
                Assert.IsNull(CachedBrush(control, "_glossBrush"))

                control.Value = 5
                control.IsGlossy = True
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using
                Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                Dim glossBrush As SolidBrush = CachedBrush(control, "_glossBrush")
                Assert.IsNotNull(barBrush)
                Assert.IsNotNull(glossBrush)
                Assert.AreSame(emptyBrush, CachedBrush(control, "_emptyBrush"))

                For index As Int32 = 1 To 20
                    Using image As Bitmap = RenderControl(control)
                        AssertPixel(Color.Blue, image, 10, 10)
                        AssertPixel(Color.Silver, image, 75, 10)
                    End Using
                Next

                control.IsGlossy = False
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 2)
                End Using
                control.IsGlossy = True
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(BlendColors(Color.Blue, Color.FromArgb(100, 255, 255, 255)), image, 10, 2)
                End Using

                Assert.AreSame(barBrush, CachedBrush(control, "_barBrush"))
                Assert.AreSame(emptyBrush, CachedBrush(control, "_emptyBrush"))
                Assert.AreSame(glossBrush, CachedBrush(control, "_glossBrush"))
            End Using
        End Sub

        <TestMethod>
        Public Sub ColorSetters_InvalidateAndReplaceBrushesOnlyAfterARealChange()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using
                Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                Assert.IsNotNull(barBrush)
                Assert.IsNotNull(emptyBrush)
                Dim invalidationCount As Int32 = 0
                AddHandler control.Invalidated, Sub(sender, e) invalidationCount += 1

                control.BarColor = Color.Blue
                control.EmptyColor = Color.Silver
                Assert.AreEqual(2, invalidationCount)
                Assert.AreSame(barBrush, CachedBrush(control, "_barBrush"))
                Assert.AreSame(emptyBrush, CachedBrush(control, "_emptyBrush"))

                control.BarColor = Color.Red
                control.BarColor = Color.Orange
                control.EmptyColor = Color.Green
                control.EmptyColor = Color.White
                Assert.AreEqual(6, invalidationCount)
                Assert.IsNull(CachedBrush(control, "_barBrush"))
                Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                AssertBrushDisposed(barBrush)
                AssertBrushDisposed(emptyBrush)

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Orange, image, 10, 10)
                    AssertPixel(Color.White, image, 75, 10)
                End Using
                Assert.AreNotSame(barBrush, CachedBrush(control, "_barBrush"))
                Assert.AreNotSame(emptyBrush, CachedBrush(control, "_emptyBrush"))
            End Using
        End Sub

        <TestMethod>
        Public Sub ColorChanges_AreVisibleDuringSynchronousInvalidation()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Int32 = 0

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            graphics.Clear(Color.White)
                            control.PaintForTest(args)
                            Dim repaint As InvalidateEventHandler = Sub(sender, e)
                                                                        invalidationCount += 1
                                                                        graphics.Clear(Color.White)
                                                                        control.PaintForTest(args)
                                                                    End Sub
                            AddHandler control.Invalidated, repaint
                            Try
                                control.BarColor = Color.FromArgb(128, Color.Red)
                                Assert.AreEqual(1, invalidationCount)
                                AssertPixel(BlendColors(Color.White, control.BarColor), image, 10, 10)

                                control.EmptyColor = Color.Empty
                                Assert.AreEqual(2, invalidationCount)
                                Assert.AreEqual(Color.Empty, control.EmptyColor)
                                AssertPixel(Color.White, image, 75, 10)

                                control.EmptyColor = Color.FromArgb(96, Color.Blue)
                                Assert.AreEqual(3, invalidationCount)
                                AssertPixel(BlendColors(Color.White, control.EmptyColor), image, 75, 10)

                                control.BarColor = Color.Empty
                                Assert.AreEqual(4, invalidationCount)
                                AssertPixel(Color.White, image, 10, 10)
                            Finally
                                RemoveHandler control.Invalidated, repaint
                            End Try
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub SystemColors_RefreshCachedBrushesBeforeRaisingTheEvent()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim systemColor As Color = SystemColors.Highlight
                Dim fixedColor As Color = Color.FromArgb(systemColor.ToArgb())
                Assert.AreNotEqual(systemColor, fixedColor)
                control.BarColor = fixedColor
                control.EmptyColor = fixedColor
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            control.PaintForTest(args)
                            Dim fixedBar As SolidBrush = CachedBrush(control, "_barBrush")
                            Dim fixedEmpty As SolidBrush = CachedBrush(control, "_emptyBrush")
                            Assert.IsNotNull(fixedBar)
                            Assert.IsNotNull(fixedEmpty)

                            control.BarColor = systemColor
                            control.EmptyColor = systemColor
                            Assert.IsNull(CachedBrush(control, "_barBrush"))
                            Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                            AssertBrushDisposed(fixedBar)
                            AssertBrushDisposed(fixedEmpty)
                            control.PaintForTest(args)
                            Dim systemBar As SolidBrush = CachedBrush(control, "_barBrush")
                            Dim systemEmpty As SolidBrush = CachedBrush(control, "_emptyBrush")
                            Assert.IsNotNull(systemBar)
                            Assert.IsNotNull(systemEmpty)
                            Assert.AreEqual(systemColor, systemBar.Color)
                            Assert.AreEqual(systemColor, systemEmpty.Color)

                            Dim systemEventCount As Int32 = 0
                            Dim invalidationCount As Int32 = 0
                            Dim onSystemColorsChanged As EventHandler = Sub(sender, e)
                                                                            systemEventCount += 1
                                                                            control.PaintForTest(args)
                                                                            Assert.AreNotSame(systemBar, CachedBrush(control, "_barBrush"))
                                                                            Assert.AreNotSame(systemEmpty, CachedBrush(control, "_emptyBrush"))
                                                                        End Sub
                            Dim onInvalidated As InvalidateEventHandler = Sub(sender, e) invalidationCount += 1
                            AddHandler control.SystemColorsChanged, onSystemColorsChanged
                            AddHandler control.Invalidated, onInvalidated
                            Try
                                control.RaiseSystemColorsChangedForTest()
                                Assert.AreEqual(1, systemEventCount)
                                Assert.IsGreaterThan(0, invalidationCount)
                                AssertPixel(systemColor, image, 10, 10)
                                AssertPixel(systemColor, image, 75, 10)
                                AssertBrushDisposed(systemBar)
                                AssertBrushDisposed(systemEmpty)
                            Finally
                                RemoveHandler control.SystemColorsChanged, onSystemColorsChanged
                                RemoveHandler control.Invalidated, onInvalidated
                            End Try

                            control.BarColor = fixedColor
                            control.EmptyColor = fixedColor
                            control.PaintForTest(args)
                            fixedBar = CachedBrush(control, "_barBrush")
                            fixedEmpty = CachedBrush(control, "_emptyBrush")
                            control.RaiseSystemColorsChangedForTest()
                            control.PaintForTest(args)
                            Assert.AreSame(fixedBar, CachedBrush(control, "_barBrush"))
                            Assert.AreSame(fixedEmpty, CachedBrush(control, "_emptyBrush"))
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Dispose_ReleasesOwnedBrushesWithoutAffectingOtherControls()
            Using first As New TestableColorProgressBar(), second As New TestableColorProgressBar()
                first.Size = New Size(100, 20)
                first.Value = 5
                second.Size = New Size(100, 20)
                second.Value = 5

                Using image As Bitmap = RenderControl(first)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                Using image As Bitmap = RenderControl(second)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using

                Dim firstBar As SolidBrush = CachedBrush(first, "_barBrush")
                Dim firstEmpty As SolidBrush = CachedBrush(first, "_emptyBrush")
                Dim firstGloss As SolidBrush = CachedBrush(first, "_glossBrush")
                Dim secondBar As SolidBrush = CachedBrush(second, "_barBrush")
                Dim secondEmpty As SolidBrush = CachedBrush(second, "_emptyBrush")
                Dim secondGloss As SolidBrush = CachedBrush(second, "_glossBrush")
                Assert.IsNotNull(firstBar)
                Assert.IsNotNull(firstEmpty)
                Assert.IsNotNull(firstGloss)
                Assert.IsNotNull(secondBar)
                Assert.IsNotNull(secondEmpty)
                Assert.IsNotNull(secondGloss)
                Assert.AreNotSame(firstBar, secondBar)
                Assert.AreNotSame(firstEmpty, secondEmpty)
                Assert.AreNotSame(firstGloss, secondGloss)

                first.Dispose()
                first.Dispose()
                Assert.IsNull(CachedBrush(first, "_barBrush"))
                Assert.IsNull(CachedBrush(first, "_emptyBrush"))
                Assert.IsNull(CachedBrush(first, "_glossBrush"))
                AssertBrushDisposed(firstBar)
                AssertBrushDisposed(firstEmpty)
                AssertBrushDisposed(firstGloss)

                Using image As Bitmap = RenderControl(second)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using
                Assert.AreSame(secondBar, CachedBrush(second, "_barBrush"))
                Assert.AreSame(secondEmpty, CachedBrush(second, "_emptyBrush"))
                Assert.AreSame(secondGloss, CachedBrush(second, "_glossBrush"))

                first.BarColor = Color.Red
                first.EmptyColor = Color.White
                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            Dim unused = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() first.PaintForTest(args))
                        End Using
                    End Using
                End Using
                Assert.IsNull(CachedBrush(first, "_barBrush"))
                Assert.IsNull(CachedBrush(first, "_emptyBrush"))
                Assert.IsNull(CachedBrush(first, "_glossBrush"))
            End Using
        End Sub

        <TestMethod>
        Public Sub Dispose_BeforePaintingAndWithPartiallyCreatedBrushes()
            Using unpainted As New ColorProgressBar()
                unpainted.Dispose()
                unpainted.Dispose()
                Assert.IsNull(CachedBrush(unpainted, "_barBrush"))
                Assert.IsNull(CachedBrush(unpainted, "_emptyBrush"))
                Assert.IsNull(CachedBrush(unpainted, "_glossBrush"))
            End Using

            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 0
                control.IsGlossy = False
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 10, 10)
                End Using
                Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                Assert.IsNotNull(emptyBrush)
                Assert.IsNull(CachedBrush(control, "_barBrush"))
                Assert.IsNull(CachedBrush(control, "_glossBrush"))
                control.Dispose()
                AssertBrushDisposed(emptyBrush)
                Assert.IsNull(CachedBrush(control, "_emptyBrush"))
            End Using

            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 10
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                Dim glossBrush As SolidBrush = CachedBrush(control, "_glossBrush")
                Assert.IsNotNull(barBrush)
                Assert.IsNotNull(glossBrush)
                Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                control.Dispose()
                AssertBrushDisposed(barBrush)
                AssertBrushDisposed(glossBrush)
            End Using
        End Sub

        <TestMethod>
        Public Sub DisposedEvent_CannotRecreateBrushesByPaintingSynchronously()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                Dim disposedEventCount As Int32 = 0

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            control.PaintForTest(args)
                            Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                            Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                            Dim glossBrush As SolidBrush = CachedBrush(control, "_glossBrush")
                            Dim onDisposed As EventHandler = Sub(sender, e)
                                                                 disposedEventCount += 1
                                                                 Dim unused = Assert.ThrowsExactly(Of ObjectDisposedException)(Sub() control.PaintForTest(args))
                                                                 Assert.IsNull(CachedBrush(control, "_barBrush"))
                                                                 Assert.IsNull(CachedBrush(control, "_emptyBrush"))
                                                                 Assert.IsNull(CachedBrush(control, "_glossBrush"))
                                                             End Sub
                            AddHandler control.Disposed, onDisposed
                            Try
                                control.Dispose()
                                Assert.IsGreaterThan(0, disposedEventCount)
                                control.Dispose()
                            Finally
                                RemoveHandler control.Disposed, onDisposed
                            End Try

                            AssertBrushDisposed(barBrush)
                            AssertBrushDisposed(emptyBrush)
                            AssertBrushDisposed(glossBrush)
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub HandleRecreation_PreservesLiveBrushes()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                End Using
                Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                Dim glossBrush As SolidBrush = CachedBrush(control, "_glossBrush")
                Assert.IsNotNull(barBrush)
                Assert.IsNotNull(emptyBrush)
                Assert.IsNotNull(glossBrush)

                control.RecreateHandleForTest()
                Assert.IsTrue(control.IsHandleCreated)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 10, 10)
                    AssertPixel(Color.Silver, image, 75, 10)
                End Using
                Assert.AreSame(barBrush, CachedBrush(control, "_barBrush"))
                Assert.AreSame(emptyBrush, CachedBrush(control, "_emptyBrush"))
                Assert.AreSame(glossBrush, CachedBrush(control, "_glossBrush"))
            End Using
        End Sub

        <TestMethod>
        Public Sub RepeatedControlDisposal_ReleasesEveryCreatedBrush()
            For index As Int32 = 1 To 25
                Using control As New ColorProgressBar()
                    control.Size = New Size(100, 20)
                    control.Value = 5
                    Using image As Bitmap = RenderControl(control)
                        AssertPixel(Color.Blue, image, 10, 10)
                    End Using
                    Dim barBrush As SolidBrush = CachedBrush(control, "_barBrush")
                    Dim emptyBrush As SolidBrush = CachedBrush(control, "_emptyBrush")
                    Dim glossBrush As SolidBrush = CachedBrush(control, "_glossBrush")
                    Assert.IsNotNull(barBrush)
                    Assert.IsNotNull(emptyBrush)
                    Assert.IsNotNull(glossBrush)
                    control.Dispose()
                    AssertBrushDisposed(barBrush)
                    AssertBrushDisposed(emptyBrush)
                    AssertBrushDisposed(glossBrush)
                End Using
            Next
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
        Public Sub ExternalPaddingChanges_KeepGeometryConsistentWithBorder()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Black, image, 0, 10)
                End Using

                control.ShowBorder = False
                control.Padding = New Padding(3)
                Assert.AreEqual(New Padding(0), control.Padding)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 0, 10)
                    AssertPixel(Color.Silver, image, 99, 10)
                End Using

                control.ShowBorder = True
                control.Padding = New Padding(3)
                Assert.AreEqual(New Padding(1), control.Padding)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Black, image, 0, 10)
                    AssertPixel(Color.Black, image, 99, 10)
                    AssertPixel(Color.Blue, image, 1, 10)
                    AssertPixel(Color.Silver, image, 98, 10)
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
        Public Sub ProgressMaximumValue_UpdatesCachedFillAndClampsValue()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 50, 10)
                    AssertPixel(Color.Silver, image, 51, 10)
                End Using

                control.ProgressMaximumValue = 20
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 25, 10)
                    AssertPixel(Color.Silver, image, 26, 10)
                End Using

                control.ProgressMaximumValue = 4
                Assert.AreEqual(4, control.Value)
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 1, 10)
                    AssertPixel(Color.Blue, image, 98, 10)
                End Using

                control.ProgressMaximumValue = 8
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 50, 10)
                    AssertPixel(Color.Silver, image, 51, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Value_PreservesIntermediateWidthRoundingAndClipping()
            Using control As New ColorProgressBar()
                control.Size = New Size(5, 9)
                control.IsGlossy = False
                control.Value = 5

                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Black, image, 0, 4)
                    AssertPixel(Color.Blue, image, 1, 4)
                    AssertPixel(Color.Blue, image, 2, 4)
                    AssertPixel(Color.Silver, image, 3, 4)
                    AssertPixel(Color.Black, image, 4, 4)
                End Using

                control.Value = 6
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Blue, image, 3, 4)
                End Using

                control.Value = 0
                Using image As Bitmap = RenderControl(control)
                    AssertPixel(Color.Silver, image, 1, 4)
                    AssertPixel(Color.Silver, image, 3, 4)
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
        Public Sub BorderStyleChange_UpdatesGeometryWhenOuterSizeIsUnchanged()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 10
                control.IsGlossy = False
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim originalSize As Size = control.Size
                Dim originalClientWidth As Int32 = control.ClientSize.Width

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Blue, image, originalClientWidth - 2, 5)

                            control.BorderStyle = BorderStyle.FixedSingle
                            Assert.AreEqual(originalSize, control.Size)
                            Assert.IsLessThan(originalClientWidth, control.ClientSize.Width)

                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Blue, image, control.ClientSize.Width - 2, 5)
                            AssertPixel(Color.Magenta, image, control.ClientSize.Width - 1, 5)
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Resize_FromAndToEmptyContent_UpdatesCachedGeometry()
            Using control As New TestableColorProgressBar()
                control.Value = 10
                control.IsGlossy = False

                Using image As New Bitmap(3, 3)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            control.Size = New Size(3, 3)
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Blue, image, 1, 1)

                            control.Size = New Size(2, 2)
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Magenta, image, 1, 1)

                            control.Size = Size.Empty
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Magenta, image, 1, 1)

                            control.Size = New Size(3, 3)
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            AssertPixel(Color.Blue, image, 1, 1)
                        End Using
                    End Using
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
                Dim invalidationCount As Int32 = 0
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
                    Assert.IsGreaterThan(0, invalidationCount)
                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub Paint_RaisesEventAfterDrawing()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                Dim paintCount As Int32 = 0
                AddHandler control.Paint, Sub(sender, e)
                                              paintCount += 1
                                              e.Graphics.FillRectangle(Brushes.Yellow, 10, 10, 1, 1)
                                          End Sub

                Using image As Bitmap = RenderControl(control)
                    Assert.IsGreaterThan(0, paintCount)
                    AssertPixel(Color.Yellow, image, 10, 10)
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Paint_ExplicitRepaintStillDrawsAndRaisesEventWhenValuesAreUnchanged()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim paintCount As Int32 = 0
                AddHandler control.Paint, Sub(sender, e)
                                              paintCount += 1
                                              e.Graphics.FillRectangle(Brushes.Yellow, 10, 10, 1, 1)
                                          End Sub

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            control.PaintForTest(args)
                            Assert.AreEqual(1, paintCount)
                            AssertPixel(Color.Blue, image, 25, 10)
                            AssertPixel(Color.Yellow, image, 10, 10)

                            control.Value = 5
                            control.ProgressMaximumValue = 10
                            graphics.Clear(Color.Magenta)
                            control.PaintForTest(args)
                            Assert.AreEqual(2, paintCount)
                            AssertPixel(Color.Blue, image, 25, 10)
                            AssertPixel(Color.Yellow, image, 10, 10)
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub InvalidatedHandler_SeesUpdatedGeometryWhenPaintingSynchronously()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                control.Value = 5
                control.IsGlossy = False
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim invalidationCount As Int32 = 0

                Using image As New Bitmap(100, 20)
                    Using graphics As Graphics = Graphics.FromImage(image)
                        Using args As New PaintEventArgs(graphics, New Rectangle(Point.Empty, image.Size))
                            control.PaintForTest(args)
                            AssertPixel(Color.Blue, image, 50, 10)

                            Dim repaintOnInvalidated As InvalidateEventHandler = Sub(sender, e)
                                                                                     invalidationCount += 1
                                                                                     graphics.Clear(Color.Magenta)
                                                                                     control.PaintForTest(args)
                                                                                 End Sub
                            AddHandler control.Invalidated, repaintOnInvalidated

                            control.Value = 7
                            Assert.AreEqual(1, invalidationCount)
                            AssertPixel(Color.Blue, image, 70, 10)
                            AssertPixel(Color.Silver, image, 71, 10)

                            control.ProgressMaximumValue = 14
                            Assert.AreEqual(2, invalidationCount)
                            AssertPixel(Color.Blue, image, 50, 10)
                            AssertPixel(Color.Silver, image, 51, 10)
                            RemoveHandler control.Invalidated, repaintOnInvalidated
                        End Using
                    End Using
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub Click_UsesTheControlAsSenderWithoutChildPanels()
            Using control As New TestableColorProgressBar()
                control.Size = New Size(100, 20)
                Assert.AreEqual(0, control.Controls.Count)
                Assert.IsTrue(control.StandardClickEnabled)

                Dim clickCount As Int32 = 0
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

        Private Shared Sub AssertPixel(expected As Color, image As Bitmap, x As Int32, y As Int32)
            Assert.AreEqual(expected.ToArgb(), image.GetPixel(x, y).ToArgb(), $"Unerwartete Farbe bei ({x}, {y}).")
        End Sub

        Private Shared Function CachedBrush(control As ColorProgressBar, fieldName As String) As SolidBrush
            Dim field As FieldInfo = GetType(ColorProgressBar).GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic)
            Assert.IsNotNull(field, $"Der Brush-Cache {fieldName} wurde nicht gefunden.")
            Return DirectCast(field.GetValue(control), SolidBrush)
        End Function

        Private Shared Sub AssertBrushDisposed(brush As SolidBrush)
            Assert.IsNotNull(brush)
            Using image As New Bitmap(1, 1)
                Using graphics As Graphics = Graphics.FromImage(image)
                    Dim unused = Assert.ThrowsExactly(Of ArgumentException)(Sub() graphics.FillRectangle(brush, 0, 0, 1, 1))
                End Using
            End Using
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

            Public Sub RaiseSystemColorsChangedForTest()
                Me.OnSystemColorsChanged(EventArgs.Empty)
            End Sub

            Public Sub RecreateHandleForTest()
                Me.RecreateHandle()
            End Sub

        End Class

    End Class

End Namespace
