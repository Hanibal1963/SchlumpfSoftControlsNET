Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.ExtendedRTFControl

Namespace SchlumpfSoftControlsNET.Tests.ExtendedRTFControl
    <TestClass>
    Public Class ExtendedRTFTests
        <TestMethod>
        Public Sub SelectionFontSize_RejectsValueBelowMinimum()
            Using control As New ExtendedRTF()
                Try
                    control.SelectionFontSize = 5.0F
                    Assert.Fail("Es wurde keine ArgumentOutOfRangeException ausgelöst.")
                Catch ex As ArgumentOutOfRangeException
                    Assert.AreEqual("value", ex.ParamName)
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub SelectionLeftIndent_RejectsNegativeValue()
            Using control As New ExtendedRTF()
                Try
                    control.SelectionLeftIndent = -1
                    Assert.Fail("Es wurde keine ArgumentOutOfRangeException ausgelöst.")
                Catch ex As ArgumentOutOfRangeException
                    Assert.AreEqual("value", ex.ParamName)
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleBold_TogglesBoldState()
            Using control As New ExtendedRTF()
                control.Text = "Hello World"
                control.Select(0, 5)

                control.SelectionBold = False
                control.ToggleBold()
                Assert.IsTrue(control.SelectionBold.GetValueOrDefault())

                control.ToggleBold()
                Assert.IsFalse(control.SelectionBold.GetValueOrDefault())
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleItalic_TogglesItalicState()
            Using control As New ExtendedRTF()
                control.Text = "Hello World"
                control.Select(0, 5)

                control.SelectionItalic = False
                control.ToggleItalic()
                Assert.IsTrue(control.SelectionItalic.GetValueOrDefault())

                control.ToggleItalic()
                Assert.IsFalse(control.SelectionItalic.GetValueOrDefault())
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleUnderline_TogglesUnderlineState()
            Using control As New ExtendedRTF()
                control.Text = "Hello World"
                control.Select(0, 5)

                control.SelectionUnderline = False
                control.ToggleUnderline()
                Assert.IsTrue(control.SelectionUnderline.GetValueOrDefault())

                control.ToggleUnderline()
                Assert.IsFalse(control.SelectionUnderline.GetValueOrDefault())
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleStrikeout_TogglesStrikeoutState()
            Using control As New ExtendedRTF()
                control.Text = "Hello World"
                control.Select(0, 5)

                control.SelectionStrikeout = False
                control.ToggleStrikeout()
                Assert.IsTrue(control.SelectionStrikeout.GetValueOrDefault())

                control.ToggleStrikeout()
                Assert.IsFalse(control.SelectionStrikeout.GetValueOrDefault())
            End Using
        End Sub

        <TestMethod>
        Public Sub SetSelectionAlignment_UpdatesSelectionAlignment()
            Using control As New ExtendedRTF()
                control.Text = "Absatz Text"
                control.Select(0, 6)

                control.SetSelectionAlignment(HorizontalAlignment.Center)
                Assert.AreEqual(HorizontalAlignment.Center, control.SelectionAlignment)

                control.SetSelectionAlignment(HorizontalAlignment.Right)
                Assert.AreEqual(HorizontalAlignment.Right, control.SelectionAlignment)
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleBullet_TogglesBulletState()
            Using control As New ExtendedRTF()
                control.Text = "Punkt Eins"
                control.Select(0, 5)

                control.SelectionBullet = False
                control.ToggleBullet()
                Assert.IsTrue(control.SelectionBullet)

                control.ToggleBullet()
                Assert.IsFalse(control.SelectionBullet)
            End Using
        End Sub

        <TestMethod>
        Public Sub SelectionForeColor_SetsAndReadsColor()
            Using control As New ExtendedRTF()
                control.Text = "Farbiger Text"
                control.Select(0, 8)

                control.SelectionForeColor = Color.Blue
                Assert.AreEqual(Color.Blue.ToArgb(), control.SelectionForeColor.ToArgb())
            End Using
        End Sub

        <TestMethod>
        Public Sub ClearFormatting_ResetsFontStyles()
            Using control As New ExtendedRTF()
                control.Text = "Formatierter Text"
                control.Select(0, 11)

                control.SelectionBold = True
                control.SelectionItalic = True

                control.ClearFormatting()

                Assert.IsFalse(control.SelectionBold.GetValueOrDefault())
                Assert.IsFalse(control.SelectionItalic.GetValueOrDefault())
            End Using
        End Sub
    End Class
End Namespace
