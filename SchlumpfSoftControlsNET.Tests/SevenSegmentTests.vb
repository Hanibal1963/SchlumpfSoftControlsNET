Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.SevenSegmentControl

Namespace SchlumpfSoftControlsNET.Tests.SevenSegmentControl
    <TestClass>
    Public Class SevenSegmentTests
        <TestMethod>
        Public Sub SingleDigit_Constructor_SetsExpectedDefaults()
            Using digit As New SingleDigit()
                Assert.AreEqual(10, digit.SegmentWidth)
                Assert.AreEqual(-0.1F, digit.ItalicFactor)
                Assert.AreEqual(SystemColors.Control, digit.BackColor)
                Assert.AreEqual(Color.DarkGray, digit.InactiveColor)
                Assert.AreEqual(Color.DarkGreen, digit.ForeColor)
                Assert.IsNull(digit.DigitValue)
                Assert.AreEqual(0, digit.CustomBitPattern)
                Assert.IsTrue(digit.ShowDecimalPoint)
                Assert.IsFalse(digit.DecimalPointActive)
                Assert.IsFalse(digit.ShowColon)
                Assert.IsFalse(digit.ColonActive)
            End Using
        End Sub

        <TestMethod>
        Public Sub SingleDigit_DigitValue_NumericCharactersMapToSegmentPatterns()
            Using digit As New SingleDigit()
                digit.DigitValue = "0"
                Assert.AreEqual(&H77, digit.CustomBitPattern)

                digit.DigitValue = "1"
                Assert.AreEqual(&H24, digit.CustomBitPattern)

                digit.DigitValue = "8"
                Assert.AreEqual(&H7F, digit.CustomBitPattern)

                digit.DigitValue = "12"
                Assert.AreEqual(&H6F, digit.CustomBitPattern)

                digit.DigitValue = "-1"
                Assert.AreEqual(&H77, digit.CustomBitPattern)
            End Using
        End Sub

        <TestMethod>
        Public Sub SingleDigit_DigitValue_LettersAndSymbolsMapToExpectedPatterns()
            Using digit As New SingleDigit()
                digit.DigitValue = "A"
                Assert.AreEqual(&H3F, digit.CustomBitPattern)

                digit.DigitValue = "c"
                Assert.AreEqual(&H58, digit.CustomBitPattern)

                digit.DigitValue = "C"
                Assert.AreEqual(&H53, digit.CustomBitPattern)

                digit.DigitValue = "u"
                Assert.AreEqual(&H70, digit.CustomBitPattern)

                digit.DigitValue = "-"
                Assert.AreEqual(&H8, digit.CustomBitPattern)

                digit.DigitValue = "?"
                Assert.AreEqual(0, digit.CustomBitPattern)
            End Using
        End Sub

        <TestMethod>
        Public Sub SingleDigit_DigitValue_NullAndEmptyResetPattern()
            Using digit As New SingleDigit()
                digit.DigitValue = "8"
                digit.DigitValue = Nothing
                Assert.IsNull(digit.DigitValue)
                Assert.AreEqual(0, digit.CustomBitPattern)

                digit.DigitValue = "5"
                digit.DigitValue = String.Empty
                Assert.AreEqual(String.Empty, digit.DigitValue)
                Assert.AreEqual(0, digit.CustomBitPattern)
            End Using
        End Sub

        <TestMethod>
        Public Sub SingleDigit_CustomBitPattern_CanBeSetAndReplacedByDigitValue()
            Using digit As New SingleDigit()
                digit.CustomBitPattern = &H55
                Assert.AreEqual(&H55, digit.CustomBitPattern)

                digit.DigitValue = "8"
                Assert.AreEqual(&H7F, digit.CustomBitPattern)
            End Using
        End Sub

        <TestMethod>
        Public Sub SingleDigit_AppearanceProperties_CanBeSetAndReadBack()
            Using digit As New SingleDigit()
                digit.SegmentWidth = 7
                digit.ItalicFactor = 0.25F
                digit.BackColor = Color.Navy
                digit.ForeColor = Color.Lime
                digit.InactiveColor = Color.SlateGray
                digit.ShowDecimalPoint = False
                digit.DecimalPointActive = True
                digit.ShowColon = True
                digit.ColonActive = True

                Assert.AreEqual(7, digit.SegmentWidth)
                Assert.AreEqual(0.25F, digit.ItalicFactor)
                Assert.AreEqual(Color.Navy, digit.BackColor)
                Assert.AreEqual(Color.Lime, digit.ForeColor)
                Assert.AreEqual(Color.SlateGray, digit.InactiveColor)
                Assert.IsFalse(digit.ShowDecimalPoint)
                Assert.IsTrue(digit.DecimalPointActive)
                Assert.IsTrue(digit.ShowColon)
                Assert.IsTrue(digit.ColonActive)
            End Using
        End Sub

        <TestMethod>
        Public Sub MultiDigit_Constructor_SetsExpectedDefaultsAndCreatesFourDigits()
            Using display As New MultiDigit()
                Assert.AreEqual(10, display.SegmentWidth)
                Assert.AreEqual(-0.1F, display.ItalicFactor)
                Assert.AreEqual(SystemColors.Control, display.BackColor)
                Assert.AreEqual(Color.DarkGray, display.InactiveColor)
                Assert.AreEqual(Color.DarkGreen, display.ForeColor)
                Assert.IsTrue(display.ShowDecimalPoint)
                Assert.AreEqual(4, display.DigitCount)
                Assert.AreEqual(New Padding(10, 4, 10, 4), display.DigitPadding)
                Assert.IsNull(display.Value)
                Assert.AreEqual(4, display.Controls.Count)
                Assert.AreEqual(4, GetDigits(display).Count)
            End Using
        End Sub

        <TestMethod>
        Public Sub MultiDigit_DigitCount_ResizesDisplayAndIgnoresOutOfRangeValues()
            Using display As New MultiDigit()
                display.DigitCount = 2
                Assert.AreEqual(2, display.DigitCount)
                Assert.AreEqual(2, display.Controls.Count)

                display.DigitCount = 0
                Assert.AreEqual(2, display.DigitCount)

                display.DigitCount = 101
                Assert.AreEqual(2, display.DigitCount)
                Assert.AreEqual(2, display.Controls.Count)
            End Using
        End Sub

        <TestMethod>
        Public Sub MultiDigit_Value_MapsDigitsRightToLeftAndAppliesDecimalPoint()
            Using display As New MultiDigit()
                display.Value = "12.3"
                Dim digits As List(Of SingleDigit) = GetDigits(display)

                Assert.AreEqual("12.3", display.Value)
                Assert.AreEqual(4, digits.Count)
                Assert.AreEqual(&H6D, digits(0).CustomBitPattern)
                Assert.IsFalse(digits(0).DecimalPointActive)
                Assert.AreEqual(&H5D, digits(1).CustomBitPattern)
                Assert.IsTrue(digits(1).DecimalPointActive)
                Assert.AreEqual(&H24, digits(2).CustomBitPattern)
                Assert.IsFalse(digits(2).DecimalPointActive)
                Assert.AreEqual(0, digits(3).CustomBitPattern)
            End Using
        End Sub

        <TestMethod>
        Public Sub MultiDigit_Value_TruncatesToAvailableDigitsAndResetsOldPatterns()
            Using display As New MultiDigit()
                display.Value = "12345"
                Dim digits As List(Of SingleDigit) = GetDigits(display)

                Assert.AreEqual(&H6B, digits(0).CustomBitPattern)
                Assert.AreEqual(&H2E, digits(1).CustomBitPattern)
                Assert.AreEqual(&H6D, digits(2).CustomBitPattern)
                Assert.AreEqual(&H5D, digits(3).CustomBitPattern)

                display.Value = "1"

                Assert.AreEqual(&H24, digits(0).CustomBitPattern)
                Assert.AreEqual(0, digits(1).CustomBitPattern)
                Assert.AreEqual(0, digits(2).CustomBitPattern)
                Assert.AreEqual(0, digits(3).CustomBitPattern)
                For Each digit As SingleDigit In digits
                    Assert.IsFalse(digit.DecimalPointActive)
                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub MultiDigit_AppearanceProperties_AreAppliedToEachDigit()
            Using display As New MultiDigit()
                Dim expectedPadding As New Padding(1, 2, 3, 4)
                display.SegmentWidth = 6
                display.ItalicFactor = 0.2F
                display.BackColor = Color.MidnightBlue
                display.ForeColor = Color.OrangeRed
                display.InactiveColor = Color.Gray
                display.ShowDecimalPoint = False
                display.DigitPadding = expectedPadding

                For Each digit As SingleDigit In GetDigits(display)
                    Assert.AreEqual(6, digit.SegmentWidth)
                    Assert.AreEqual(0.2F, digit.ItalicFactor)
                    Assert.AreEqual(Color.MidnightBlue, digit.BackColor)
                    Assert.AreEqual(Color.OrangeRed, digit.ForeColor)
                    Assert.AreEqual(Color.Gray, digit.InactiveColor)
                    Assert.IsFalse(digit.ShowDecimalPoint)
                    Assert.AreEqual(expectedPadding, digit.Padding)
                Next
            End Using
        End Sub

        Private Shared Function GetDigits(display As MultiDigit) As List(Of SingleDigit)
            Dim digits As New List(Of SingleDigit)
            For Each child As Control In display.Controls
                Dim digit As SingleDigit = TryCast(child, SingleDigit)
                If digit IsNot Nothing Then digits.Add(digit)
            Next
            digits.Sort(Function(left, right) right.Left.CompareTo(left.Left))
            Return digits
        End Function
    End Class
End Namespace
