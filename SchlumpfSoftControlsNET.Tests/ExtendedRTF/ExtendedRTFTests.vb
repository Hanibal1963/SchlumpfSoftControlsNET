' --------------------------------------------------------------------------------------------------------
' Datei: ExtendedRTFTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.ExtendedRTFControl

Namespace SchlumpfSoftControlsNET.Tests.ExtendedRTFControl
    <TestClass>
    Public Class ExtendedRTFTests
        <TestMethod>
        <DataRow(-1.0F)>
        <DataRow(0.0F)>
        <DataRow(5.0F)>
        Public Sub SelectionFontSize_RejectsValueBelowMinimum(size As Single)
            Using control As New ExtendedRTF()
                Try
                    control.SelectionFontSize = size
                    Assert.Fail("Es wurde keine ArgumentOutOfRangeException ausgelöst.")
                Catch ex As ArgumentOutOfRangeException
                    Assert.AreEqual("value", ex.ParamName)
                End Try
            End Using
        End Sub

        ''' <summary>Unveränderte Schriftwerte dürfen keine temporären Auswahländerungen auslösen.</summary>
        <TestMethod>
        Public Sub UnchangedFontProperties_DoNotChangeSelection()
            Using control As New ExtendedRTF()
                control.Text = "Beispiel"
                control.Select(0, control.TextLength)
                control.SelectionFontSize = 12.0F
                control.SelectionBold = True
                Dim eventCount = 0
                AddHandler control.SelectionChanged, Sub(sender, args) eventCount += 1

                control.SelectionFontSize = 12.0F
                control.SelectionBold = True

                Assert.AreEqual(0, eventCount)
                Assert.AreEqual(0, control.SelectionStart)
                Assert.AreEqual(control.TextLength, control.SelectionLength)
                Assert.AreEqual(12.0F, control.SelectionFontSize.Value)
                Assert.IsTrue(control.SelectionBold.Value)
            End Using
        End Sub

        ''' <summary>Schrifttransformationen erhalten andere Attribute auch bei gemischten Auswahlen.</summary>
        <TestMethod>
        Public Sub FontTransformation_PreservesOtherAttributes()
            Using control As New ExtendedRTF()
                control.Text = "abcd"
                control.Select(0, 2)
                control.SelectionFontSize = 12.0F
                control.SelectionItalic = True
                control.Select(2, 2)
                control.SelectionFontSize = 16.0F
                control.SelectionItalic = False
                control.Select(0, 4)
                control.SelectionBold = True

                Assert.AreEqual(0, control.SelectionStart)
                Assert.AreEqual(4, control.SelectionLength)
                control.Select(0, 2)
                Assert.AreEqual(12.0F, control.SelectionFontSize.Value)
                Assert.IsTrue(control.SelectionItalic.Value)
                Assert.IsTrue(control.SelectionBold.Value)
                control.Select(2, 2)
                Assert.AreEqual(16.0F, control.SelectionFontSize.Value)
                Assert.IsFalse(control.SelectionItalic.Value)
                Assert.IsTrue(control.SelectionBold.Value)

                control.Select(0, 4)
                control.SelectionFontSize = 14.0F
                control.Select(0, 2)
                Assert.AreEqual(14.0F, control.SelectionFontSize.Value)
                Assert.IsTrue(control.SelectionItalic.Value)
                control.Select(2, 2)
                Assert.AreEqual(14.0F, control.SelectionFontSize.Value)
                Assert.IsFalse(control.SelectionItalic.Value)
            End Using
        End Sub

        ''' <summary>Auch bei gemischter Auswahl setzt ClearFormatting den gesamten Font zurück.</summary>
        <TestMethod>
        Public Sub ClearFormatting_MixedFontsUsesSelectionFont()
            Using control As New ExtendedRTF()
                control.Text = "abcd"
                control.Select(0, 2)
                control.SelectionFontSize = 16.0F
                control.SelectionBold = True
                control.Select(0, 4)
                Dim expectedSize = If(control.SelectionFont, control.Font).Size

                control.ClearFormatting()

                Assert.IsTrue(control.SelectionFontSize.HasValue)
                Assert.AreEqual(expectedSize, control.SelectionFontSize.Value)
                Assert.IsFalse(control.SelectionBold.Value)
            End Using
        End Sub

        ''' <summary>Prüft Mischzustände unabhängig von weiteren Schriftattributen und Änderungen über die Basis-API.</summary>
        <TestMethod>
        Public Sub MixedFontProperties_ReflectCurrentSelection()
            Using control As New ExtendedRTF()
                control.Text = "abcd"
                control.Select(0, 4)
                control.SelectionFontSize = 12.0F
                control.SelectionBold = False
                control.Select(0, 2)
                control.SelectionBold = True
                control.Select(0, 4)

                Assert.IsFalse(control.SelectionBold.HasValue)
                Assert.AreEqual(12.0F, control.SelectionFontSize.Value)
                Assert.IsFalse(control.SelectionItalic.Value)

                control.Select(2, 2)
                Using changedFont As New Font(control.Font.FontFamily, 16.0F, FontStyle.Italic)
                    control.SelectionFont = changedFont
                End Using
                control.Select(0, 4)
                Assert.IsFalse(control.SelectionFontSize.HasValue)
                Assert.IsFalse(control.SelectionItalic.HasValue)

                control.Select(2, 2)
                Assert.AreEqual(16.0F, control.SelectionFontSize.Value)
                Assert.IsTrue(control.SelectionItalic.Value)
            End Using
        End Sub

        ''' <summary>Prüft einheitliche und unterschiedliche Einzüge mehrerer Absätze.</summary>
        <TestMethod>
        Public Sub SelectionLeftIndent_ReportsMixedParagraphs()
            Using control As New ExtendedRTF()
                control.Text = "eins" & vbLf & "zwei"
                control.Select(0, control.TextLength)
                control.SelectionLeftIndent = 20
                Assert.AreEqual(20, control.SelectionLeftIndent.Value)
                control.Select(5, 4)
                control.SelectionIndent = 40
                control.Select(0, control.TextLength)
                Assert.IsFalse(control.SelectionLeftIndent.HasValue)
                control.Select(5, 4)
                Assert.AreEqual(40, control.SelectionLeftIndent.Value)
            End Using
        End Sub

        ''' <summary>Abfragen verändern weder die Auswahl noch lösen sie Auswahlereignisse aus.</summary>
        <TestMethod>
        Public Sub MixedStateQueries_DoNotChangeSelectionOrRaiseEvents()
            Using control As New ExtendedRTF()
                control.Text = "eins" & vbLf & "zwei"
                control.Select(0, 2)
                control.SelectionBold = True
                control.Select(0, control.TextLength)
                Dim eventCount = 0
                AddHandler control.SelectionChanged, Sub(sender, args) eventCount += 1

                Dim bold = control.SelectionBold
                Dim size = control.SelectionFontSize
                Dim indent = control.SelectionLeftIndent

                Assert.IsFalse(bold.HasValue)
                Assert.IsTrue(size.HasValue)
                Assert.IsTrue(indent.HasValue)
                Assert.AreEqual(0, eventCount)
                Assert.AreEqual(0, control.SelectionStart)
                Assert.AreEqual(control.TextLength, control.SelectionLength)
            End Using
        End Sub

        ''' <summary>Formatierungen dürfen ein ausgeblendetes Steuerelement nicht sichtbar machen.</summary>
        <TestMethod>
        Public Sub Formatting_PreservesHiddenControlVisibility()
            Using control As New ExtendedRTF()
                control.Visible = False
                control.CreateControl()
                Dim handle = control.Handle
                control.Text = "Text"
                control.Select(0, 4)

                control.SelectionForeColor = Color.Blue
                control.SelectionBackColor = Color.Yellow
                control.SelectionLeftIndent = 10
                control.SelectionBold = True
                control.SetSelectionAlignment(HorizontalAlignment.Center)
                control.ToggleBullet()
                control.ClearFormatting()

                Assert.IsFalse(control.Visible)
                Assert.AreEqual(handle, control.Handle)
                Assert.AreEqual(HorizontalAlignment.Center, control.SelectionAlignment)
                Assert.IsFalse(control.SelectionBullet)
            End Using
        End Sub

        ''' <summary>Prüft, dass die kleinste zulässige Schriftgröße übernommen wird.</summary>
        <TestMethod>
        Public Sub SelectionFontSize_AcceptsMinimum()
            Using control As New ExtendedRTF()
                control.SelectionFontSize = 8.0F
                Assert.AreEqual(8.0F, control.SelectionFontSize.Value)
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
