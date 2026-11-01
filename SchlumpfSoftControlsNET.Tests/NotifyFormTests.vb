Imports System
Imports System.Drawing
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.NotifyFormControl

Namespace SchlumpfSoftControlsNET.Tests.NotifyFormControl
    <TestClass>
    Public Class NotifyFormTests

        <TestMethod>
        Public Sub Constructor_SetsExpectedDefaults()
            Using notify As New NotifyForm()
                Assert.AreEqual("Titel", notify.Title)
                Assert.AreEqual("Mitteilung", notify.Message)
                Assert.AreEqual(NotifyFormDesign.Bright, notify.Design)
                Assert.AreEqual(NotifyFormStyle.Information, notify.Style)
                Assert.AreEqual(5000, notify.ShowTime)
            End Using
        End Sub

        <TestMethod>
        Public Sub Properties_CanBeSetAndReadBack()
            Using notify As New NotifyForm()
                notify.Title = "Mein Titel"
                notify.Message = "Meine Mitteilung"
                notify.Design = NotifyFormDesign.Colorful
                notify.Style = NotifyFormStyle.CriticalError
                notify.ShowTime = 2500

                Assert.AreEqual("Mein Titel", notify.Title)
                Assert.AreEqual("Meine Mitteilung", notify.Message)
                Assert.AreEqual(NotifyFormDesign.Colorful, notify.Design)
                Assert.AreEqual(NotifyFormStyle.CriticalError, notify.Style)
                Assert.AreEqual(2500, notify.ShowTime)
            End Using
        End Sub

        <TestMethod>
        Public Sub SetFormDesign_Bright_AppliesExpectedColorsToTemplate()
            Using notify As New NotifyForm()
                notify.Design = NotifyFormDesign.Bright
                Dim unused = InvokePrivateMethod(notify, "SetFormDesign")

                Assert.AreEqual(Color.White, GetTemplateField(Of Color)("BackgroundColor"))
                Assert.AreEqual(Color.White, GetTemplateField(Of Color)("TextFieldColor"))
                Assert.AreEqual(Color.Gray, GetTemplateField(Of Color)("TitleBarColor"))
                Assert.AreEqual(Color.Black, GetTemplateField(Of Color)("FontColor"))
            End Using
        End Sub

        <TestMethod>
        Public Sub SetFormDesign_Colorful_AppliesExpectedColorsToTemplate()
            Using notify As New NotifyForm()
                notify.Design = NotifyFormDesign.Colorful
                Dim unused = InvokePrivateMethod(notify, "SetFormDesign")

                Assert.AreEqual(Color.LightBlue, GetTemplateField(Of Color)("BackgroundColor"))
                Assert.AreEqual(Color.LightBlue, GetTemplateField(Of Color)("TextFieldColor"))
                Assert.AreEqual(Color.LightSeaGreen, GetTemplateField(Of Color)("TitleBarColor"))
                Assert.AreEqual(Color.White, GetTemplateField(Of Color)("FontColor"))
            End Using
        End Sub

        <TestMethod>
        Public Sub SetFormDesign_Dark_AppliesExpectedColorsToTemplate()
            Using notify As New NotifyForm()
                notify.Design = NotifyFormDesign.Dark
                Dim unused = InvokePrivateMethod(notify, "SetFormDesign")

                Assert.AreEqual(Color.FromArgb(83, 79, 75), GetTemplateField(Of Color)("BackgroundColor"))
                Assert.AreEqual(Color.FromArgb(83, 79, 75), GetTemplateField(Of Color)("TextFieldColor"))
                Assert.AreEqual(Color.FromArgb(60, 57, 54), GetTemplateField(Of Color)("TitleBarColor"))
                Assert.AreEqual(Color.White, GetTemplateField(Of Color)("FontColor"))
            End Using
        End Sub

        <TestMethod>
        Public Sub SetFormImage_ReturnsExpectedImageForEachStyle()
            Using notify As New NotifyForm()
                notify.Style = NotifyFormStyle.Information
                Dim infoImage As Image = CType(InvokePrivateMethod(notify, "SetFormImage"), Image)
                Assert.IsNotNull(infoImage)

                notify.Style = NotifyFormStyle.Question
                Dim questionImage As Image = CType(InvokePrivateMethod(notify, "SetFormImage"), Image)
                Assert.IsNotNull(questionImage)

                notify.Style = NotifyFormStyle.CriticalError
                Dim errorImage As Image = CType(InvokePrivateMethod(notify, "SetFormImage"), Image)
                Assert.IsNotNull(errorImage)

                notify.Style = NotifyFormStyle.Exclamation
                Dim warningImage As Image = CType(InvokePrivateMethod(notify, "SetFormImage"), Image)
                Assert.IsNotNull(warningImage)
            End Using
        End Sub

        Private Shared Function GetTemplateField(Of T)(fieldName As String) As T
            Dim targetType As Type = GetType(NotifyForm).Assembly.GetType("SchlumpfSoft.ControlsNET.NotifyFormControl.FormTemplate")
            Assert.IsNotNull(targetType, "Typ 'FormTemplate' wurde nicht gefunden.")
            Dim field As FieldInfo = targetType.GetField(fieldName, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Assert.IsNotNull(field, $"Feld '{fieldName}' in FormTemplate wurde nicht gefunden.")
            Return CType(field.GetValue(Nothing), T)
        End Function

        Private Shared Function InvokePrivateMethod(target As Object, methodName As String, ParamArray args As Object()) As Object
            Dim flags As BindingFlags = BindingFlags.Instance Or BindingFlags.NonPublic
            Dim m As MethodInfo = target.GetType().GetMethod(methodName, flags)
            Assert.IsNotNull(m, $"Methode '{methodName}' wurde nicht gefunden.")
            Return m.Invoke(target, args)
        End Function
    End Class
End Namespace
