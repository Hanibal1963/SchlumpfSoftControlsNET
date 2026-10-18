Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.TransparentLabelControl

Namespace SchlumpfSoftControlsNET.Tests.TransparentLabelControl
    <TestClass>
    Public Class TransparentLabelTests
        <TestMethod>
        Public Sub Constructor_SetsExpectedDefaults()
            Using control As New TransparentLabel()
                Assert.AreEqual(NameOf(TransparentLabel), control.Name)
                Assert.AreEqual(Color.Transparent, control.BackColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub BackColor_AllowsTransparentAndEmpty()
            Using control As New TransparentLabel()
                control.BackColor = Color.Empty
                Assert.AreEqual(Color.Transparent, control.BackColor)

                control.BackColor = Color.Transparent
                Assert.AreEqual(Color.Transparent, control.BackColor)
            End Using
        End Sub

        <TestMethod>
        Public Sub BackColor_RejectsNonTransparentColor()
            Using control As New TransparentLabel()
                Try
                    control.BackColor = Color.Red
                    Assert.Fail("Es wurde keine ArgumentException ausgelöst.")
                Catch ex As ArgumentException
                    Assert.AreEqual("value", ex.ParamName)
#Disable Warning MSTEST0046 ' Verwenden Sie Assert anstelle von StringAssert
                    StringAssert.Contains(ex.Message, "nur Color.Transparent")
#Enable Warning MSTEST0046 ' Verwenden Sie Assert anstelle von StringAssert
                End Try
            End Using
        End Sub

        <TestMethod>
        Public Sub CreateParams_ContainsTransparentExtendedStyle()
            Using control As New TestTransparentLabel()
                Dim cp As CreateParams = control.ExposedCreateParams

                Assert.AreNotEqual(0, cp.ExStyle And &H20)
            End Using
        End Sub

        Private NotInheritable Class TestTransparentLabel
            Inherits TransparentLabel

            Public ReadOnly Property ExposedCreateParams As CreateParams
                Get
                    Return MyBase.CreateParams
                End Get
            End Property
        End Class
    End Class
End Namespace
