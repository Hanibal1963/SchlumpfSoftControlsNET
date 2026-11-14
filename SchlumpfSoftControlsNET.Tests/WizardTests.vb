Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SchlumpfSoft.ControlsNET.WizardControl

Namespace SchlumpfSoftControlsNET.Tests.WizardControl

    <TestClass>
    Public Class WizardTests

        <TestMethod>
        Public Sub PageTypes_DefaultStyles_MatchExpectedValues()
            Using standardPage As New PageStandard(),
                  welcomePage As New PageWelcome(),
                  finishPage As New PageFinish(),
                  customPage As New PageCustom()

                Assert.AreEqual(PageStyle.Standard, standardPage.Style)
                Assert.AreEqual(PageStyle.Welcome, welcomePage.Style)
                Assert.AreEqual(PageStyle.Finish, finishPage.Style)
                Assert.AreEqual(PageStyle.Custom, customPage.Style)
            End Using
        End Sub

        <TestMethod>
        Public Sub WizardPage_TitleAndDescription_HandleNullAsEmpty()
            Using page As New WizardPage()
                page.Title = "Start"
                page.Description = "Beschreibung"
                Assert.AreEqual("Start", page.Title)
                Assert.AreEqual("Beschreibung", page.Description)

                page.Title = Nothing
                page.Description = Nothing
                Assert.IsEmpty(page.Title)
                Assert.IsEmpty(page.Description)
            End Using
        End Sub

        <TestMethod>
        Public Sub Constructor_SetsExpectedDefaults()
            Using wizard As New Wizard()
                Assert.AreEqual(DockStyle.Fill, wizard.Dock)
                Assert.IsTrue(wizard.VisibleHelp)
                Assert.IsNotNull(wizard.Pages)
                Assert.HasCount(0, wizard.Pages)

                Assert.IsNull(wizard.SelectedPage)
                Assert.AreEqual(-1, GetSelectedIndex(wizard))

                Assert.IsNotNull(wizard.ButtonHelp)
                Assert.IsNotNull(wizard.ButtonBack)
                Assert.IsNotNull(wizard.ButtonNext)
                Assert.IsNotNull(wizard.ButtonCancel)

                CollectionAssert.Contains(wizard.Controls, wizard.ButtonHelp)
                CollectionAssert.Contains(wizard.Controls, wizard.ButtonBack)
                CollectionAssert.Contains(wizard.Controls, wizard.ButtonNext)
                CollectionAssert.Contains(wizard.Controls, wizard.ButtonCancel)
            End Using
        End Sub

        <TestMethod>
        Public Sub VisibleHelp_TogglesControlInControlsCollection()
            Using wizard As New Wizard()
                Assert.IsTrue(wizard.VisibleHelp)
                CollectionAssert.Contains(wizard.Controls, wizard.ButtonHelp)

                wizard.VisibleHelp = False
                Assert.IsFalse(wizard.VisibleHelp)
                CollectionAssert.DoesNotContain(wizard.Controls, wizard.ButtonHelp)

                wizard.VisibleHelp = True
                Assert.IsTrue(wizard.VisibleHelp)
                CollectionAssert.Contains(wizard.Controls, wizard.ButtonHelp)
            End Using
        End Sub

        <TestMethod>
        Public Sub ButtonTextsAndEnabled_CanBeSetAndReadBack()
            Using wizard As New Wizard()
                wizard.BackText = "Zurück Schritt"
                wizard.NextText = "Weiter Schritt"
                wizard.CancelText = "Abbrechen Schritt"
                wizard.HelpText = "Hilfe Schritt"
                wizard.BackEnabled = True
                wizard.NextEnabled = False

                Assert.AreEqual("Zurück Schritt", wizard.BackText)
                Assert.AreEqual("Weiter Schritt", wizard.NextText)
                Assert.AreEqual("Abbrechen Schritt", wizard.CancelText)
                Assert.AreEqual("Hilfe Schritt", wizard.HelpText)
                Assert.IsTrue(wizard.BackEnabled)
                Assert.IsFalse(wizard.NextEnabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub PagesCollection_AddAndRemove_UpdatesSelectedIndexAndSelectedPage()
            Using wizard As New Wizard()
                Dim page1 As New PageWelcome()
                Dim page2 As New PageStandard()

                Dim unused = wizard.Pages.Add(page1)
                Assert.HasCount(1, wizard.Pages)

                Assert.AreSame(page1, wizard.SelectedPage)
                Assert.AreEqual(0, GetSelectedIndex(wizard))
                Assert.IsFalse(wizard.ButtonBack.Enabled)
                Assert.IsFalse(wizard.ButtonNext.Enabled)

                Dim unused2 = wizard.Pages.Add(page2)
                Assert.HasCount(2, wizard.Pages)

                Assert.AreSame(page2, wizard.SelectedPage)
                Assert.AreEqual(1, GetSelectedIndex(wizard))
                Assert.IsTrue(wizard.ButtonBack.Enabled)
                Assert.IsFalse(wizard.ButtonNext.Enabled)

                wizard.Pages.Remove(page2)
                Assert.HasCount(1, wizard.Pages)

                Assert.AreSame(page1, wizard.SelectedPage)
                Assert.AreEqual(0, GetSelectedIndex(wizard))
                wizard.Pages.Remove(page1)
                Assert.HasCount(0, wizard.Pages)

                Assert.IsNull(wizard.SelectedPage)
                Assert.AreEqual(-1, GetSelectedIndex(wizard))
                Assert.IsFalse(wizard.ButtonBack.Enabled)
                Assert.IsFalse(wizard.ButtonNext.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub NextAndBack_NavigateBetweenPages()
            Using wizard As New Wizard()
                Dim page1 As New PageWelcome()
                Dim page2 As New PageStandard()
                Dim page3 As New PageFinish()

                wizard.Pages.AddRange(New WizardPage() {page1, page2, page3})

                SetSelectedIndex(wizard, 0)
                Assert.AreEqual(0, GetSelectedIndex(wizard))
                Assert.AreSame(page1, wizard.SelectedPage)
                Assert.IsFalse(wizard.ButtonBack.Enabled)
                Assert.IsTrue(wizard.ButtonNext.Enabled)

                wizard.Next()
                Assert.AreEqual(1, GetSelectedIndex(wizard))
                Assert.AreSame(page2, wizard.SelectedPage)
                Assert.IsTrue(wizard.ButtonBack.Enabled)
                Assert.IsTrue(wizard.ButtonNext.Enabled)

                wizard.Next()
                Assert.AreEqual(2, GetSelectedIndex(wizard))
                Assert.AreSame(page3, wizard.SelectedPage)
                Assert.IsTrue(wizard.ButtonBack.Enabled)
                Assert.IsFalse(wizard.ButtonNext.Enabled)

                wizard.Back()
                Assert.AreEqual(1, GetSelectedIndex(wizard))
                Assert.AreSame(page2, wizard.SelectedPage)
                Assert.IsTrue(wizard.ButtonBack.Enabled)
                Assert.IsTrue(wizard.ButtonNext.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub BeforeSwitchPages_CanCancelNavigation()
            Using wizard As New Wizard()
                Dim page1 As New PageWelcome()
                Dim page2 As New PageStandard()

                wizard.Pages.AddRange(New WizardPage() {page1, page2})
                SetSelectedIndex(wizard, 0)

                Dim eventFired As Boolean = False
                AddHandler wizard.BeforeSwitchPages, Sub(s, e)
                                                         eventFired = True
                                                         Dim args As BeforeSwitchPagesEventArgs = TryCast(e, BeforeSwitchPagesEventArgs)
                                                         If args IsNot Nothing Then
                                                             args.Cancel = True
                                                         End If
                                                     End Sub

                wizard.Next()

                Assert.IsTrue(eventFired)
                Assert.AreEqual(0, GetSelectedIndex(wizard))
                Assert.AreSame(page1, wizard.SelectedPage)
            End Using
        End Sub

        <TestMethod>
        Public Sub AfterSwitchPages_IsRaisedWhenNavigationCompletes()
            Using wizard As New Wizard()
                Dim page1 As New PageWelcome()
                Dim page2 As New PageStandard()

                wizard.Pages.AddRange(New WizardPage() {page1, page2})
                SetSelectedIndex(wizard, 0)

                Dim observedOldIndex As Integer = -1
                Dim observedNewIndex As Integer = -1

                AddHandler wizard.AfterSwitchPages, Sub(s, e)
                                                        Dim args As AfterSwitchPagesEventArgs = TryCast(e, AfterSwitchPagesEventArgs)
                                                        If args IsNot Nothing Then
                                                            observedOldIndex = args.OldIndex
                                                            observedNewIndex = args.NewIndex
                                                        End If
                                                    End Sub

                wizard.Next()

                Assert.AreEqual(0, observedOldIndex)
                Assert.AreEqual(1, observedNewIndex)
                Assert.AreEqual(1, GetSelectedIndex(wizard))
            End Using
        End Sub

        <TestMethod>
        Public Sub SelectedIndex_ThrowsWhenOutOfRange()
            Using wizard As New Wizard()
                Dim page As New PageStandard()
                Dim unused = wizard.Pages.Add(page)

                Dim thrown As Boolean = False
                Try
                    SetSelectedIndex(wizard, 5)
                Catch ex As TargetInvocationException When TypeOf ex.InnerException Is ArgumentOutOfRangeException
                    thrown = True
                Catch ex As ArgumentOutOfRangeException
                    thrown = True
                End Try

                Assert.IsTrue(thrown)
            End Using
        End Sub

        <TestMethod>
        Public Sub ActivateFinishPage_UpdatesCancelButtonToBeendenAndDialogResultOK()
            Using wizard As New Wizard()
                Dim page1 As New PageWelcome()
                Dim page2 As New PageFinish()

                wizard.Pages.AddRange(New WizardPage() {page1, page2})
                SetSelectedIndex(wizard, 0)
                Assert.AreEqual("Abbruch", wizard.ButtonCancel.Text)
                Assert.AreEqual(DialogResult.Cancel, wizard.ButtonCancel.DialogResult)

                SetSelectedIndex(wizard, 1)
                Assert.AreEqual("Beenden", wizard.ButtonCancel.Text)
                Assert.AreEqual(DialogResult.OK, wizard.ButtonCancel.DialogResult)
            End Using
        End Sub

        <TestMethod>
        Public Sub FontProperties_FallBackToControlFontWhenNotSet()
            Using wizard As New Wizard()
                Assert.AreEqual(wizard.Font, wizard.HeaderFont)
                Assert.AreEqual(wizard.Font, wizard.WelcomeFont)

                Assert.AreEqual(wizard.Font.Size + 2.0F, wizard.HeaderTitleFont.Size)
                Assert.IsTrue(wizard.HeaderTitleFont.Bold)

                Assert.AreEqual(wizard.Font.Size + 10.0F, wizard.WelcomeTitleFont.Size)
                Assert.IsTrue(wizard.WelcomeTitleFont.Bold)
            End Using
        End Sub

        Private Shared Function GetSelectedIndex(wizard As Wizard) As Integer
            Dim prop = GetType(Wizard).GetProperty("SelectedIndex", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.NonPublic)
            Assert.IsNotNull(prop, "Eigenschaft 'SelectedIndex' nicht gefunden.")
            Return CInt(prop.GetValue(wizard))
        End Function

        Private Shared Sub SetSelectedIndex(wizard As Wizard, index As Integer)
            Dim prop = GetType(Wizard).GetProperty("SelectedIndex", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.NonPublic)
            Assert.IsNotNull(prop, "Eigenschaft 'SelectedIndex' nicht gefunden.")
            prop.SetValue(wizard, index)
        End Sub

    End Class

End Namespace
