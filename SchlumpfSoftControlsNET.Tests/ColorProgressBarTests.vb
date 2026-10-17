Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.ColorProgressBarControl

Namespace SchlumpfSoftControlsNET.Tests.ColorProgressBarControl
    <TestClass>
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
            End Using
        End Sub

        <TestMethod>
        Public Sub IsGlossy_TogglesGlossPanelsVisibility()
            Using control As New ColorProgressBar()
                Dim glossLeft As Panel = FindPanel(control, "GlossLeft")
                Dim glossRight As Panel = FindPanel(control, "GlossRight")

                control.IsGlossy = False
                Assert.IsFalse(glossLeft.Visible)
                Assert.IsFalse(glossRight.Visible)

                control.IsGlossy = True
                Assert.IsTrue(glossLeft.Visible)
                Assert.IsTrue(glossRight.Visible)
            End Using
        End Sub

        <TestMethod>
        Public Sub ColorProperties_UpdateVisualElements()
            Using control As New ColorProgressBar()
                Dim progressFull As Panel = FindPanel(control, "ProgressFull")
                Dim progressEmpty As Panel = FindPanel(control, "ProgressEmpty")

                Dim barColor As Color = Color.Orange
                Dim emptyColor As Color = Color.LightGray
                Dim borderColor As Color = Color.DarkBlue

                control.BarColor = barColor
                control.EmptyColor = emptyColor
                control.BorderColor = borderColor

                Assert.AreEqual(barColor, progressFull.BackColor)
                Assert.AreEqual(emptyColor, progressEmpty.BackColor)
                Assert.AreEqual(borderColor, control.BackColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub Value_UpdatesProgressWidthProportionally()
            Using control As New ColorProgressBar()
                control.Size = New Size(100, 20)
                control.ProgressMaximumValue = 10

                control.Value = 5

                Dim progressFull As Panel = FindPanel(control, "ProgressFull")
                Assert.AreEqual(50, progressFull.Width)
            End Using
        End Sub

        Private Shared Function FindPanel(root As Control, panelName As String) As Panel
            Dim match As Panel = TryFindPanel(root, panelName)
            If match IsNot Nothing Then
                Return match
            End If

            Assert.Fail($"Panel '{panelName}' wurde nicht gefunden.")
            Return Nothing
        End Function

        Private Shared Function TryFindPanel(root As Control, panelName As String) As Panel
            For Each child As Control In root.Controls
                If String.Equals(child.Name, panelName, StringComparison.Ordinal) Then
                    Return DirectCast(child, Panel)
                End If

                Dim match As Panel = TryFindPanel(child, panelName)
                If match IsNot Nothing Then
                    Return match
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace
