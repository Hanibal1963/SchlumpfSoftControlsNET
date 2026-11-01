Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.ShapeControl

Namespace SchlumpfSoftControlsNET.Tests.ShapeControl
    <TestClass>
    Public Class ShapeTests

        <TestMethod>
        Public Sub LineWidth_IsClampedToMinimumOne()
            Using control As New Shape()
                control.LineWidth = 0.25F

                Assert.AreEqual(1.0F, control.LineWidth)
            End Using
        End Sub

        <TestMethod>
        Public Sub AppearanceProperties_CanBeSetAndReadBack()
            Using control As New Shape()
                control.ShapeModus = ShapeModes.FilledEllipse
                control.DiagonalLineModus = DiagonalLineModes.BottomLeftToTopRight
                control.LineColor = Color.Crimson
                control.FillColor = Color.Gold
                control.LineWidth = 3.5F

                Assert.AreEqual(ShapeModes.FilledEllipse, control.ShapeModus)
                Assert.AreEqual(DiagonalLineModes.BottomLeftToTopRight, control.DiagonalLineModus)
                Assert.AreEqual(Color.Crimson, control.LineColor)
                Assert.AreEqual(Color.Gold, control.FillColor)
                Assert.AreEqual(3.5F, control.LineWidth)
            End Using
        End Sub

        <TestMethod>
        Public Sub CreateParams_ContainsTransparentExtendedStyle()
            Using control As New TestShape()
                Dim cp As CreateParams = control.ExposedCreateParams

                Assert.AreNotEqual(0, cp.ExStyle And &H20)
            End Using
        End Sub

        Private NotInheritable Class TestShape
            Inherits Shape

            Public ReadOnly Property ExposedCreateParams As CreateParams
                Get
                    Return MyBase.CreateParams
                End Get
            End Property
        End Class
    End Class
End Namespace
