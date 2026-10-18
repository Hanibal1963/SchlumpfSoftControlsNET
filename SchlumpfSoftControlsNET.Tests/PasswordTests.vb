Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.PasswordControl

Namespace SchlumpfSoftControlsNET.Tests.PasswordControl
    <TestClass>
    Public Class PasswordTests
        <TestMethod>
        Public Sub Constructor_SetsDefaultMaskingAndSizing()
            Using control As New Password()
                Dim tb As TextBox = FindControl(Of TextBox)(control, "TB")

                Assert.AreEqual(New Drawing.Size(100, 20), control.MinimumSize)
                Assert.AreEqual(New Drawing.Size(100, 20), control.Size)
                Assert.IsTrue(tb.UseSystemPasswordChar)
            End Using
        End Sub

        <TestMethod>
        Public Sub ToggleVisibility_ChangesPasswordMasking()
            Using control As New Password()
                Dim tb As TextBox = FindControl(Of TextBox)(control, "TB")
                Dim pb As PictureBox = FindControl(Of PictureBox)(control, "PB")

                RaiseClick(pb)
                Assert.IsFalse(tb.UseSystemPasswordChar)

                RaiseClick(pb)
                Assert.IsTrue(tb.UseSystemPasswordChar)
            End Using
        End Sub

        <TestMethod>
        Public Sub ClearPassword_EmptiesText()
            Using control As New Password()
                Dim tb As TextBox = FindControl(Of TextBox)(control, "TB")
                tb.Text = "geheim"

                control.ClearPassword()

                Assert.AreEqual(String.Empty, tb.Text)
            End Using
        End Sub

        <TestMethod>
        Public Sub PasswortChanged_ProvidesVerifiableCode()
            Using control As New Password()
                Dim tb As TextBox = FindControl(Of TextBox)(control, "TB")
                Dim raised As Integer = 0
                Dim passwordCode As String = Nothing

                AddHandler control.PasswortChanged,
                    Sub(sender, e)
                        raised += 1
                        passwordCode = e.PasswordCode
                    End Sub

                tb.Text = "meinPasswort123"
                RaiseKeyDownEnter(tb)

                Assert.AreEqual(1, raised)
                Assert.IsFalse(String.IsNullOrWhiteSpace(passwordCode))
                Assert.IsTrue(control.VerifyPasswordCode(passwordCode))

                tb.Text = "falsch"
                Assert.IsFalse(control.VerifyPasswordCode(passwordCode))
            End Using
        End Sub

        <TestMethod>
        Public Sub EnabledChanged_UpdatesInnerTextBoxEnabledState()
            Using control As New Password()
                Dim tb As TextBox = FindControl(Of TextBox)(control, "TB")

                control.Enabled = False
                Assert.IsFalse(tb.Enabled)

                control.Enabled = True
                Assert.IsTrue(tb.Enabled)
            End Using
        End Sub

        Private Shared Sub RaiseKeyDownEnter(tb As TextBox)
            Dim methodInfo As MethodInfo = GetType(TextBox).GetMethod("OnKeyDown", BindingFlags.Instance Or BindingFlags.NonPublic)
            methodInfo.Invoke(tb, New Object() {New KeyEventArgs(Keys.Enter)})
        End Sub

        Private Shared Sub RaiseClick(control As Control)
            Dim methodInfo As MethodInfo = GetType(Control).GetMethod("OnClick", BindingFlags.Instance Or BindingFlags.NonPublic)
            methodInfo.Invoke(control, New Object() {EventArgs.Empty})
        End Sub

        Private Shared Function FindControl(Of T As Control)(root As Control, controlName As String) As T
            Dim result As T = TryFindControl(Of T)(root, controlName)
            If result IsNot Nothing Then
                Return result
            End If

            Assert.Fail($"Control '{controlName}' wurde nicht gefunden.")
            Return Nothing
        End Function

        Private Shared Function TryFindControl(Of T As Control)(root As Control, controlName As String) As T
            For Each child As Control In root.Controls
                If TypeOf child Is T AndAlso String.Equals(child.Name, controlName, StringComparison.Ordinal) Then
                    Return DirectCast(child, T)
                End If

                Dim nested As T = TryFindControl(Of T)(child, controlName)
                If nested IsNot Nothing Then
                    Return nested
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace
