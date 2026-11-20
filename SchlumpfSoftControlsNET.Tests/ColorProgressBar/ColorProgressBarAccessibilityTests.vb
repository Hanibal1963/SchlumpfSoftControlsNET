' --------------------------------------------------------------------------------------------------------
' Datei: ColorProgressBarAccessibilityTests.vb
' Author: Andreas Sauer
' Datum: 15.11.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.ExceptionServices
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Automation
Imports System.Windows.Automation.Provider
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.ColorProgressBarControl

Namespace SchlumpfSoftControlsNET.Tests.ColorProgressBarControl

    <STATestClass>
    Public Class ColorProgressBarAccessibilityTests

        <TestMethod>
        <DataRow("de-DE", "Fortschrittsanzeige", "Zeigt den Fortschritt eines Vorgangs an.")>
        <DataRow("en-US", "Progress indicator", "Shows the progress of an operation.")>
        <DataRow("fr-FR", "Fortschrittsanzeige", "Zeigt den Fortschritt eines Vorgangs an.")>
        Public Sub AccessibleDefaults_UseTheUiCulture(cultureName As String, expectedName As String, expectedDescription As String)
            Dim previousCulture As CultureInfo = CultureInfo.CurrentUICulture
            Try
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
                Using control As New ColorProgressBar()
                    Assert.IsNull(control.AccessibleName)
                    Assert.IsNull(control.AccessibleDescription)
                    Assert.IsFalse(TypeDescriptor.GetProperties(control)(NameOf(control.AccessibleName)).ShouldSerializeValue(control))
                    Assert.IsFalse(TypeDescriptor.GetProperties(control)(NameOf(control.AccessibleDescription)).ShouldSerializeValue(control))
                    Assert.AreEqual(AccessibleRole.ProgressBar, control.AccessibleRole)
                    Assert.AreEqual(expectedName, control.AccessibilityObject.Name)
                    Assert.AreEqual(expectedDescription, control.AccessibilityObject.Description)
                    Assert.AreEqual(AccessibleRole.ProgressBar, control.AccessibilityObject.Role)
                End Using
            Finally
                CultureInfo.CurrentUICulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub AccessibleMetadata_CanBeCustomized()
            Using control As New ColorProgressBar()
                control.AccessibleName = "Dateiübertragung"
                control.AccessibleDescription = "Fortschritt des Downloads"
                control.AccessibleRole = AccessibleRole.Slider

                Assert.AreEqual("Dateiübertragung", control.AccessibilityObject.Name)
                Assert.AreEqual("Fortschritt des Downloads", control.AccessibilityObject.Description)
                Assert.AreEqual(AccessibleRole.Slider, control.AccessibilityObject.Role)

                control.AccessibleName = String.Empty
                control.AccessibleDescription = String.Empty
                control.AccessibleRole = AccessibleRole.Default
                Assert.AreEqual(String.Empty, control.AccessibilityObject.Name)
                Assert.AreEqual(String.Empty, control.AccessibilityObject.Description)
                Assert.AreEqual(AccessibleRole.ProgressBar, control.AccessibilityObject.Role)

                control.AccessibilityObject.Name = "Neuer Name"
                Assert.AreEqual("Neuer Name", control.AccessibleName)
            End Using
        End Sub

        <TestMethod>
        Public Sub AccessibleDefaults_FollowUiCultureWithoutRecreatingTheControl()
            Dim previousCulture As CultureInfo = CultureInfo.CurrentUICulture
            Try
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE")
                Using control As New ColorProgressBar()
                    Assert.AreEqual("Fortschrittsanzeige", control.AccessibilityObject.Name)
                    Assert.AreEqual("Zeigt den Fortschritt eines Vorgangs an.", control.AccessibilityObject.Description)

                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US")
                    Assert.AreEqual("Progress indicator", control.AccessibilityObject.Name)
                    Assert.AreEqual("Shows the progress of an operation.", control.AccessibilityObject.Description)
                End Using
            Finally
                CultureInfo.CurrentUICulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub AccessibleName_UsesAnAssociatedLabelUnlessExplicitlyOverridden()
            Using form As New Form(), label As New Label(), control As New ColorProgressBar()
                label.Text = "&Dateiübertragung"
                label.TabIndex = 0
                control.TabIndex = 1
                form.Controls.Add(label)
                form.Controls.Add(control)

                Assert.AreEqual("Dateiübertragung", control.AccessibilityObject.Name)
                control.AccessibleName = "Eigener Name"
                Assert.AreEqual("Eigener Name", control.AccessibilityObject.Name)
                control.AccessibleName = String.Empty
                Assert.AreEqual(String.Empty, control.AccessibilityObject.Name)
                control.AccessibleName = Nothing
                Assert.AreEqual("Dateiübertragung", control.AccessibilityObject.Name)
            End Using
        End Sub

        <TestMethod>
        <DataRow(0, 10, 0)>
        <DataRow(1, 3, 33)>
        <DataRow(2, 3, 66)>
        <DataRow(Integer.MaxValue - 1, Integer.MaxValue, 99)>
        <DataRow(Integer.MaxValue, Integer.MaxValue, 100)>
        Public Sub AccessibleValue_ReportsBoundedPercentage(value As Integer, maximum As Integer, expectedPercentage As Integer)
            Dim previousCulture As CultureInfo = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
                Using control As New ColorProgressBar()
                    control.ProgressMaximumValue = maximum
                    control.Value = value

                    Dim expected As String = (CDbl(expectedPercentage) / 100).ToString("P0", CultureInfo.CurrentCulture)
                    Assert.AreEqual(expected, control.AccessibilityObject.Value)
                    Assert.AreEqual(AccessibleStates.ReadOnly, control.AccessibilityObject.State And AccessibleStates.ReadOnly)
                End Using
            Finally
                CultureInfo.CurrentCulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub AccessibleValue_TracksValueAndMaximumAndIsReadOnly()
            Dim previousCulture As CultureInfo = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE")
                Using control As New ColorProgressBar()
                    control.Value = 8
                    Dim accessible As AccessibleObject = control.AccessibilityObject
                    Assert.AreEqual(0.8R.ToString("P0", CultureInfo.CurrentCulture), accessible.Value)

                    control.ProgressMaximumValue = 20
                    Assert.AreEqual(0.4R.ToString("P0", CultureInfo.CurrentCulture), accessible.Value)

                    control.ProgressMaximumValue = 4
                    Assert.AreEqual(1.0R.ToString("P0", CultureInfo.CurrentCulture), accessible.Value)
                    Dim unused = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() accessible.Value = "50%")
                    Assert.AreEqual(4, control.Value)
                End Using
            Finally
                CultureInfo.CurrentCulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub Accessibility_DoesNotCreateAWindowHandleForValueChanges()
            Using control As New ColorProgressBar()
                Assert.IsFalse(control.IsHandleCreated)
                control.Value = 5
                control.ProgressMaximumValue = 20
                Assert.IsFalse(control.IsHandleCreated)

                Assert.IsNotNull(control.AccessibilityObject.Value)
                Assert.IsFalse(control.IsHandleCreated)
            End Using
        End Sub

        <TestMethod>
        Public Sub MsaaClient_ReadsCurrentNameRoleAndProgress()
            Dim previousCulture As CultureInfo = CultureInfo.CurrentCulture
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
            Try
                Using form As New Form(), control As New ColorProgressBar()
                    control.AccessibleName = "File transfer"
                    control.AccessibleDescription = "Transfer progress"
                    control.Value = 5
                    form.Controls.Add(control)
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)

                    RunAutomationClient(Sub() AssertMsaaValues(handle, "50%"))
                    control.ProgressMaximumValue = 20
                    control.Value = 6
                    RunAutomationClient(Sub() AssertMsaaValues(handle, "30%"))
                End Using
            Finally
                CultureInfo.CurrentCulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub Accessibility_RaisesMsaaValueChangesForTheControl()
            Using control As New ColorProgressBar()
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)
                Dim events As New List(Of Tuple(Of Integer, Integer))()
                Dim callback As WinEventCallback = Sub(hook, eventType, window, objectId, childId, eventThread, eventTime)
                                                       If window = handle Then events.Add(Tuple.Create(objectId, childId))
                                                   End Sub
                Dim eventId As UInteger = CUInt(AccessibleEvents.ValueChange)
                Dim hookHandle As IntPtr = SetWinEventHook(eventId, eventId, IntPtr.Zero, callback, CUInt(Environment.ProcessId), 0UI, 0UI)
                If hookHandle = IntPtr.Zero Then Throw New Win32Exception(Marshal.GetLastWin32Error())

                Try
                    control.Value = 5
                    control.Value = 5
                    control.ProgressMaximumValue = 20
                    control.ProgressMaximumValue = 20
                    PumpUntil(Function() events.Count >= 2, "MSAA-Ereignisse")
                    Application.DoEvents()

                    Assert.HasCount(2, events)
                    For Each observed As Tuple(Of Integer, Integer) In events
                        Assert.AreEqual(-4, observed.Item1)
                        Assert.AreEqual(0, observed.Item2)
                    Next
                Finally
                    If Not UnhookWinEvent(hookHandle) Then
                        Throw New Win32Exception(Marshal.GetLastWin32Error())
                    End If
                    GC.KeepAlive(callback)
                End Try
            End Using
        End Sub

        Private Shared Sub AssertMsaaValues(handle As IntPtr, expectedValue As String)
            Dim interfaceId As Guid = GetType(Accessibility.IAccessible).GUID
            Dim native As Object = Nothing
            Marshal.ThrowExceptionForHR(AccessibleObjectFromWindow(handle, &HFFFFFFFCUI, interfaceId, native))
            Assert.IsNotNull(native)
            Try
                Dim accessible As Accessibility.IAccessible = DirectCast(native, Accessibility.IAccessible)
                Assert.AreEqual("File transfer", accessible.accName(0))
                Assert.AreEqual("Transfer progress", accessible.accDescription(0))
                Assert.AreEqual(CInt(AccessibleRole.ProgressBar), Convert.ToInt32(accessible.accRole(0), CultureInfo.InvariantCulture))
                Assert.AreEqual(expectedValue, accessible.accValue(0))
                Dim state As AccessibleStates = CType(Convert.ToInt32(accessible.accState(0), CultureInfo.InvariantCulture), AccessibleStates)
                Assert.AreEqual(AccessibleStates.ReadOnly, state And AccessibleStates.ReadOnly)
            Finally
                If Marshal.IsComObject(native) Then
                    Dim unused = Marshal.ReleaseComObject(native)
                End If
            End Try
        End Sub

        <TestMethod>
        Public Sub UiAutomation_ExposesReadOnlyProgressAndRaisesPropertyChanges()
            Dim previousCulture As CultureInfo = CultureInfo.CurrentCulture
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US")
            Try
                Using form As New Form(), control As New ColorProgressBar(), ready As New ManualResetEventSlim(),
                    changed As New ManualResetEventSlim(), received As New ManualResetEventSlim(), completed As New ManualResetEventSlim()
                    control.Name = "TransferProgress"
                    control.AccessibleName = "File transfer"
                    control.AccessibleDescription = "Transfer progress"
                    form.Controls.Add(control)
                    Dim handle As IntPtr = control.Handle
                    Assert.AreNotEqual(IntPtr.Zero, handle)

                    Dim events As New ConcurrentQueue(Of AutomationPropertyChangedEventArgs)()
                    Dim eventCount As Integer = 0
                    Dim clientError As Exception = Nothing
                    Dim client As New Thread(Sub()
                                                 Dim element As AutomationElement = Nothing
                                                 Dim registered As Boolean = False
                                                 Dim handler As AutomationPropertyChangedEventHandler = Sub(sender, e)
                                                                                                            events.Enqueue(e)
                                                                                                            If Interlocked.Increment(eventCount) = 7 Then received.Set()
                                                                                                        End Sub
                                                 Try
                                                     element = AutomationElement.FromHandle(handle)
                                                     Assert.IsNotNull(element)
                                                     Assert.AreEqual(ControlType.ProgressBar, element.Current.ControlType)
                                                     Assert.AreEqual("File transfer", element.Current.Name)
                                                     Assert.AreEqual("Transfer progress", element.Current.HelpText)
                                                     Assert.AreEqual("TransferProgress", element.Current.AutomationId)
                                                     Assert.IsTrue(element.Current.IsControlElement)
                                                     Assert.IsTrue(element.Current.IsContentElement)

                                                     Dim range As RangeValuePattern = DirectCast(element.GetCurrentPattern(RangeValuePattern.Pattern), RangeValuePattern)
                                                     Dim text As ValuePattern = DirectCast(element.GetCurrentPattern(ValuePattern.Pattern), ValuePattern)
                                                     Assert.AreEqual(0.0R, range.Current.Minimum)
                                                     Assert.AreEqual(10.0R, range.Current.Maximum)
                                                     Assert.AreEqual(1.0R, range.Current.Value)
                                                     Assert.IsTrue(range.Current.IsReadOnly)
                                                     Assert.IsTrue(Double.IsNaN(range.Current.LargeChange))
                                                     Assert.IsTrue(Double.IsNaN(range.Current.SmallChange))
                                                     Assert.AreEqual("10%", text.Current.Value)
                                                     Assert.IsTrue(text.Current.IsReadOnly)
                                                     Dim unused = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() range.SetValue(5))
                                                     Dim unused1 = Assert.ThrowsExactly(Of InvalidOperationException)(Sub() text.SetValue("50%"))

                                                     Automation.AddAutomationPropertyChangedEventHandler(element, TreeScope.Element, handler,
                                                         RangeValuePattern.ValueProperty, RangeValuePattern.MaximumProperty, ValuePattern.ValueProperty)
                                                     registered = True
                                                     ready.Set()
                                                     If Not changed.Wait(TimeSpan.FromSeconds(10)) Then Throw New TimeoutException("Fortschrittsänderungen fehlen.")
                                                     If Not received.Wait(TimeSpan.FromSeconds(10)) Then Throw New TimeoutException("UI-Automation-Ereignisse fehlen.")

                                                     Assert.AreEqual(4.0R, range.Current.Value)
                                                     Assert.AreEqual(4.0R, range.Current.Maximum)
                                                     Assert.AreEqual("100%", text.Current.Value)
                                                     Assert.AreEqual("Download", element.Current.Name)
                                                     Assert.AreEqual("Aktueller Download", element.Current.HelpText)
                                                     Assert.IsFalse(element.Current.IsEnabled)
                                                 Catch ex As Exception
                                                     clientError = ex
                                                 Finally
                                                     If registered Then
                                                         Try
                                                             Automation.RemoveAutomationPropertyChangedEventHandler(element, handler)
                                                         Catch ex As Exception
                                                             clientError = If(clientError Is Nothing, ex, New AggregateException(clientError, ex))
                                                         End Try
                                                     End If
                                                     ready.Set()
                                                     completed.Set()
                                                 End Try
                                             End Sub) With {
                        .IsBackground = True
                                             }
                    client.SetApartmentState(ApartmentState.MTA)
                    client.Start()

                    Try
                        PumpUntil(Function() ready.IsSet, "UI-Automation-Anmeldung")
                        If clientError IsNot Nothing Then ExceptionDispatchInfo.Capture(clientError).Throw()

                        control.Value = 5
                        control.Value = 5
                        control.ProgressMaximumValue = 20
                        control.ProgressMaximumValue = 20
                        control.ProgressMaximumValue = 4
                        control.AccessibleName = "Download"
                        control.AccessibleDescription = "Aktueller Download"
                        control.Enabled = False
                        changed.Set()
                        PumpUntil(Function() completed.IsSet, "UI-Automation-Prüfung")
                    Finally
                        changed.Set()
                        If Not completed.IsSet Then PumpUntil(Function() completed.IsSet, "UI-Automation-Abmeldung")
                        Assert.IsTrue(client.Join(TimeSpan.FromSeconds(1)))
                    End Try

                    If clientError IsNot Nothing Then ExceptionDispatchInfo.Capture(clientError).Throw()
                    Dim observed As AutomationPropertyChangedEventArgs() = events.ToArray()
                    Assert.HasCount(7, observed)
                    AssertAutomationEvent(observed, RangeValuePattern.ValueProperty, 1.0R, 5.0R)
                    AssertAutomationEvent(observed, RangeValuePattern.ValueProperty, 5.0R, 4.0R)
                    AssertAutomationEvent(observed, RangeValuePattern.MaximumProperty, 10.0R, 20.0R)
                    AssertAutomationEvent(observed, RangeValuePattern.MaximumProperty, 20.0R, 4.0R)
                    AssertAutomationEvent(observed, ValuePattern.ValueProperty, "10%", "50%")
                    AssertAutomationEvent(observed, ValuePattern.ValueProperty, "50%", "25%")
                    AssertAutomationEvent(observed, ValuePattern.ValueProperty, "25%", "100%")
                End Using
            Finally
                CultureInfo.CurrentCulture = previousCulture
            End Try
        End Sub

        <TestMethod>
        Public Sub UiAutomation_RespectsExplicitAccessibleRole()
            Using form As New Form(), control As New ColorProgressBar()
                control.AccessibleRole = AccessibleRole.Slider
                control.AccessibleName = "Custom role"
                form.Controls.Add(control)
                Dim handle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, handle)

                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(handle)
                                        Assert.IsNotNull(element)
                                        Assert.AreEqual(ControlType.Slider, element.Current.ControlType)
                                        Assert.AreEqual("Custom role", element.Current.Name)
                                    End Sub)

                control.AccessibleRole = AccessibleRole.Default
                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(handle)
                                        Assert.IsNotNull(element)
                                        Assert.AreEqual(ControlType.ProgressBar, element.Current.ControlType)
                                        Assert.IsNotNull(element.GetCurrentPattern(RangeValuePattern.Pattern))
                                    End Sub)
            End Using
        End Sub

        <TestMethod>
        <CodeAnalysis.SuppressMessage("Style", "IDE0058:Der Ausdruckswert wird niemals verwendet.", Justification:="<Ausstehend>")>
        Public Sub UiAutomation_RecreatesProviderAfterHandleRecreation()
            Using form As New Form(), control As New TestableColorProgressBar()
                control.Value = 5
                form.Controls.Add(control)
                Dim originalHandle As IntPtr = control.Handle
                Assert.AreNotEqual(IntPtr.Zero, originalHandle)

                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(originalHandle)
                                        Assert.IsNotNull(element)
                                        Dim range As RangeValuePattern = DirectCast(element.GetCurrentPattern(RangeValuePattern.Pattern), RangeValuePattern)
                                        Assert.AreEqual(5.0R, range.Current.Value)
                                    End Sub)

                Dim providerField As FieldInfo = GetType(ColorProgressBar).GetField("_uiaProvider", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(providerField)
                Dim originalProvider As IRawElementProviderSimple = DirectCast(providerField.GetValue(control), IRawElementProviderSimple)
                Assert.IsNotNull(originalProvider)
                Dim ownerField As FieldInfo = originalProvider.GetType().GetField("_owner", BindingFlags.Instance Or BindingFlags.NonPublic)
                Assert.IsNotNull(ownerField)
                Assert.AreSame(control, ownerField.GetValue(originalProvider))

                control.AccessibleRole = AccessibleRole.Slider
                Dim unused = Assert.ThrowsExactly(Of ElementNotAvailableException)(
                    Sub() originalProvider.GetPatternProvider(RangeValuePatternIdentifiers.Pattern.Id))
                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(originalHandle)
                                        Assert.IsNotNull(element)
                                        Assert.AreEqual(ControlType.Slider, element.Current.ControlType)
                                    End Sub)

                control.AccessibleRole = AccessibleRole.ProgressBar
                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(originalHandle)
                                        Assert.IsNotNull(element)
                                        Assert.AreEqual(ControlType.ProgressBar, element.Current.ControlType)
                                    End Sub)

                control.RecreateHandleForTest()
                Assert.IsTrue(control.IsHandleCreated)
                Dim unused1 = Assert.ThrowsExactly(Of ElementNotAvailableException)(
                    Sub() originalProvider.GetPatternProvider(RangeValuePatternIdentifiers.Pattern.Id))
                Assert.IsNull(providerField.GetValue(control))
                Assert.IsNull(ownerField.GetValue(originalProvider))

                Dim newHandle As IntPtr = control.Handle
                RunAutomationClient(Sub()
                                        Dim element As AutomationElement = AutomationElement.FromHandle(newHandle)
                                        Assert.IsNotNull(element)
                                        Assert.AreEqual(ControlType.ProgressBar, element.Current.ControlType)
                                        Dim range As RangeValuePattern = DirectCast(element.GetCurrentPattern(RangeValuePattern.Pattern), RangeValuePattern)
                                        Assert.AreEqual(5.0R, range.Current.Value)
                                    End Sub)
                Dim newProvider As IRawElementProviderSimple = DirectCast(providerField.GetValue(control), IRawElementProviderSimple)
                Assert.IsNotNull(newProvider)
                Assert.AreNotSame(originalProvider, newProvider)

                control.Dispose()
                Assert.IsNull(providerField.GetValue(control))
                Assert.IsNull(ownerField.GetValue(newProvider))
                Assert.ThrowsExactly(Of ElementNotAvailableException)(
                    Sub() newProvider.GetPatternProvider(RangeValuePatternIdentifiers.Pattern.Id))
            End Using
        End Sub

        Private Shared Sub RunAutomationClient(action As Action)
            Using completed As New ManualResetEventSlim()
                Dim clientError As Exception = Nothing
                Dim client As New Thread(Sub()
                                             Try
                                                 action()
                                             Catch ex As Exception
                                                 clientError = ex
                                             Finally
                                                 completed.Set()
                                             End Try
                                         End Sub) With {
                    .IsBackground = True
                                         }
                client.SetApartmentState(ApartmentState.MTA)
                client.Start()
                PumpUntil(Function() completed.IsSet, "UI-Automation-Client")
                Assert.IsTrue(client.Join(TimeSpan.FromSeconds(1)))
                If clientError IsNot Nothing Then ExceptionDispatchInfo.Capture(clientError).Throw()
            End Using
        End Sub

        Private Shared Sub AssertAutomationEvent(events As AutomationPropertyChangedEventArgs(), propertyId As AutomationProperty,
                                                 oldValue As Object, newValue As Object)
            Dim matches As Integer = 0
            For Each observed As AutomationPropertyChangedEventArgs In events
                If observed.Property.Id = propertyId.Id AndAlso Object.Equals(oldValue, observed.OldValue) AndAlso
                    Object.Equals(newValue, observed.NewValue) Then
                    matches += 1
                End If
            Next
            Assert.AreEqual(1, matches, $"UI-Automation-Ereignis {propertyId.ProgrammaticName}: {oldValue} -> {newValue}")
        End Sub

        Private Shared Sub PumpUntil(condition As Func(Of Boolean), operation As String)
            Dim timeout As Stopwatch = Stopwatch.StartNew()
            While Not condition()
                If timeout.Elapsed > TimeSpan.FromSeconds(10) Then Throw New TimeoutException(operation)
                Application.DoEvents()
                Thread.Sleep(10)
            End While
        End Sub

        <UnmanagedFunctionPointer(CallingConvention.Winapi)>
        Private Delegate Sub WinEventCallback(hook As IntPtr, eventType As UInteger, window As IntPtr, objectId As Integer,
                                              childId As Integer, eventThread As UInteger, eventTime As UInteger)

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function SetWinEventHook(eventMin As UInteger, eventMax As UInteger, moduleHandle As IntPtr,
                                                callback As WinEventCallback, processId As UInteger, threadId As UInteger,
                                                flags As UInteger) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function UnhookWinEvent(hook As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("oleacc.dll")>
        Private Shared Function AccessibleObjectFromWindow(handle As IntPtr, objectId As UInteger, ByRef interfaceId As Guid,
                                                           <MarshalAs(UnmanagedType.Interface)> ByRef result As Object) As Integer
        End Function

        Private Class TestableColorProgressBar

            Inherits ColorProgressBar

            Public Sub RecreateHandleForTest()
                Me.RecreateHandle()
            End Sub
        End Class

    End Class

End Namespace