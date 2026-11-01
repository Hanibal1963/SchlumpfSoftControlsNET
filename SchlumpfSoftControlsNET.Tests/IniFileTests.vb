Imports System
Imports System.IO
Imports System.Windows.Forms

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports SchlumpfSoft.ControlsNET.IniFileControl

Namespace SchlumpfSoftControlsNET.Tests.IniFileControl
    <TestClass>
    Public Class IniFileTests
        Private _tempDirectory As String = String.Empty

        <TestInitialize>
        Public Sub CreateTemporaryDirectory()
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"IniFileTests_{Guid.NewGuid():N}")
            Dim unused = Directory.CreateDirectory(_tempDirectory)
        End Sub

        <TestCleanup>
        Public Sub DeleteTemporaryDirectory()
            If Directory.Exists(_tempDirectory) Then
                Directory.Delete(_tempDirectory, recursive:=True)
            End If
        End Sub

        <TestMethod>
        Public Sub Constructor_SetsExpectedDefaults()
            Using ini As New IniFile()
                Assert.IsFalse(ini.FileSaved)
                Assert.AreEqual(";"c, ini.CommentPrefix)
                Assert.AreEqual("neue Datei.ini", ini.FileName)
                Assert.AreEqual(String.Empty, ini.FilePath)
                Assert.IsFalse(ini.AutoSave)
            End Using
        End Sub

        <TestMethod>
        Public Sub CreateNewFile_UsesDefaultPrefixAndCreatesSampleContent()
            Using ini As New IniFile()
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub

                ini.CreateNewFile()

                Assert.AreEqual(";"c, ini.CommentPrefix)
                Assert.IsFalse(ini.FileSaved)
                Assert.AreEqual(1, changeCount)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"Allgemein", "Datenbank", "Logging"}, ini.GetSectionNames())
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.AreEqual("MeineApp", ini.GetEntryValue("Allgemein", "AppName"))
                Assert.AreEqual("3306", ini.GetEntryValue("Datenbank", "Port"))
                Assert.Contains("; INI - Datei Beispiel", ini.GetFileContent())
                Assert.Contains("AppName = MeineApp", ini.GetFileContent())
            End Using
        End Sub

        <TestMethod>
        Public Sub CreateNewFile_WithCustomPrefix_UsesPrefixForComments()
            Using ini As New IniFile()
                ini.CreateNewFile("#"c)

                Assert.AreEqual("#"c, ini.CommentPrefix)
                CollectionAssert.AreEqual(New String() {"INI - Datei Beispiel", "Diese Datei wurde von " & Application.ProductName & " erzeugt"}, ini.GetFileComment())

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"Anwendungsname und Version"}, ini.GetSectionComment("Allgemein"))
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.Contains("# INI - Datei Beispiel", ini.GetFileContent())
            End Using
        End Sub

        <TestMethod>
        Public Sub LoadFile_ParsesCommentsSectionsAndValuesIncludingEquals()
            Dim filePath As String = Path.Combine(_tempDirectory, "sample.ini")
            File.WriteAllLines(filePath, New String() {
                "; Header comment",
                "; Second header line",
                String.Empty,
                "[General]",
                "; Section comment",
                "Name = value",
                "Connection = server=local;port=1433",
                "Blank =",
                "[Other]",
                "Count = 42"
            })

            Using ini As New IniFile()
                ini.LoadFile(filePath)

                Assert.IsTrue(ini.FileSaved)
                Assert.AreEqual(_tempDirectory, ini.FilePath)
                Assert.AreEqual("sample.ini", ini.FileName)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"Header comment", "Second header line"}, ini.GetFileComment())
                CollectionAssert.AreEqual(New String() {"General", "Other"}, ini.GetSectionNames())
                CollectionAssert.AreEqual(New String() {"Section comment"}, ini.GetSectionComment("General"))
                CollectionAssert.AreEqual(New String() {"Name", "Connection", "Blank"}, ini.GetEntryNames("General"))
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.AreEqual("value", ini.GetEntryValue("General", "Name"))
                Assert.AreEqual("server=local;port=1433", ini.GetEntryValue("General", "Connection"))
                Assert.AreEqual(String.Empty, ini.GetEntryValue("General", "Blank"))
                Assert.AreEqual("42", ini.GetEntryValue("Other", "Count"))
                Assert.IsNull(ini.GetEntryNames("Missing"))
                Assert.IsNull(ini.GetSectionComment("Missing"))
            End Using
        End Sub

        <TestMethod>
        Public Sub LoadFile_ReplacesPreviouslyLoadedStateAndRaisesChangeEvent()
            Dim filePath As String = Path.Combine(_tempDirectory, "replacement.ini")

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
            File.WriteAllLines(filePath, New String() {"[Replacement]", "Key = new"})
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub

                ini.LoadFile(filePath)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"Replacement"}, ini.GetSectionNames())
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.AreEqual("new", ini.GetEntryValue("Replacement", "Key"))
                Assert.IsTrue(ini.FileSaved)
                Assert.AreEqual(1, changeCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub LoadFile_RejectsEmptyPathAndReportsMissingFile()
            Using ini As New IniFile()
                Dim unused1 = Assert.ThrowsExactly(Of ArgumentException)(Sub() ini.LoadFile("  "))
                Dim unused = Assert.ThrowsExactly(Of IOException)(Sub() ini.LoadFile(Path.Combine(_tempDirectory, "missing.ini")))
            End Using
        End Sub

        <TestMethod>
        Public Sub AddSection_RaisesChangeEventAndRejectsDuplicateName()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim changeCount As Integer
                Dim duplicateCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub
                AddHandler ini.SectionNameExist, Sub(sender, e)
                                                     duplicateCount += 1
                                                 End Sub

                ini.AddSection("Custom")
                ini.AddSection("Custom")

                Assert.AreEqual(1, changeCount)
                Assert.AreEqual(1, duplicateCount)
                Assert.Contains("Custom", ini.GetSectionNames())
                Assert.HasCount(4, ini.GetFileContent())
            End Using
        End Sub

        <TestMethod>
        Public Sub AddEntryAndSetEntryValue_UpdatesStructureAndSerializedContent()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub

                ini.AddEntry("Allgemein", "CustomValue")
                Assert.AreEqual(String.Empty, ini.GetEntryValue("Allgemein", "CustomValue"))

                ini.SetEntryValue("Allgemein", "CustomValue", "left=right")

                Assert.AreEqual("left=right", ini.GetEntryValue("Allgemein", "CustomValue"))
                Assert.Contains("CustomValue = left=right", ini.GetFileContent())
                Assert.AreEqual(2, changeCount)
                Assert.IsFalse(ini.FileSaved)
            End Using
        End Sub

        <TestMethod>
        Public Sub AddEntry_RaisesDuplicateEventWithoutChangingContent()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim changeCount As Integer
                Dim duplicateCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub
                AddHandler ini.EntryNameExist, Sub(sender, e)
                                                   duplicateCount += 1
                                               End Sub

                ini.AddEntry("Allgemein", "AppName")

                Assert.AreEqual(0, changeCount)
                Assert.AreEqual(1, duplicateCount)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"AppName", "Version"}, ini.GetEntryNames("Allgemein"))
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

            End Using
        End Sub

        <TestMethod>
        Public Sub RenameSection_PreservesEntriesAndComments()
            Using ini As New IniFile()
                ini.CreateNewFile()

                ini.RenameSection("Allgemein", "Application")

                Assert.IsNull(ini.GetEntryNames("Allgemein"))

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"Application", "Datenbank", "Logging"}, ini.GetSectionNames())
                CollectionAssert.AreEqual(New String() {"Anwendungsname und Version"}, ini.GetSectionComment("Application"))
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.AreEqual("MeineApp", ini.GetEntryValue("Application", "AppName"))
                Assert.Contains("[Application]", ini.GetFileContent())
            End Using
        End Sub

        <TestMethod>
        Public Sub RenameSection_RaisesDuplicateEventWithoutRenaming()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim duplicateCount As Integer
                AddHandler ini.SectionNameExist, Sub(sender, e)
                                                     duplicateCount += 1
                                                 End Sub

                ini.RenameSection("Allgemein", "Datenbank")

                Assert.AreEqual(1, duplicateCount)
                Assert.AreEqual("MeineApp", ini.GetEntryValue("Allgemein", "AppName"))
                Assert.AreEqual("3306", ini.GetEntryValue("Datenbank", "Port"))
            End Using
        End Sub

        <TestMethod>
        Public Sub RenameEntry_PreservesValueAndRejectsDuplicateName()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim duplicateCount As Integer
                AddHandler ini.EntryNameExist, Sub(sender, e)
                                                   duplicateCount += 1
                                               End Sub

                ini.RenameEntry("Allgemein", "AppName", "Version")
                Assert.AreEqual(1, duplicateCount)
                Assert.AreEqual("MeineApp", ini.GetEntryValue("Allgemein", "AppName"))

                ini.RenameEntry("Allgemein", "AppName", "ApplicationName")

                Assert.AreEqual("MeineApp", ini.GetEntryValue("Allgemein", "ApplicationName"))
                Assert.IsFalse(Array.Exists(ini.GetEntryNames("Allgemein"), Function(name) name = "AppName"))
            End Using
        End Sub

        <TestMethod>
        Public Sub DeleteSection_RemovesSectionAndItsComment()
            Using ini As New IniFile()
                ini.CreateNewFile()

                ini.DeleteSection("Allgemein")

                Assert.IsFalse(Array.Exists(ini.GetSectionNames(), Function(name) name = "Allgemein"))
                Assert.IsNull(ini.GetSectionComment("Allgemein"))
                Assert.IsFalse(Array.Exists(ini.GetFileContent(), Function(line) line = "[Allgemein]"))
            End Using
        End Sub

        <TestMethod>
        Public Sub DeleteEntry_RemovesEntryFromSectionAndContent()
            Using ini As New IniFile()
                ini.CreateNewFile()

                ini.DeleteEntry("Allgemein", "AppName")

                Assert.IsFalse(Array.Exists(ini.GetEntryNames("Allgemein"), Function(name) name = "AppName"))
                Assert.IsFalse(Array.Exists(ini.GetFileContent(), Function(line) line.StartsWith("AppName =", StringComparison.Ordinal)))
            End Using
        End Sub

        <TestMethod>
        Public Sub SetComments_ReplacesCommentsAndRegeneratesContent()
            Using ini As New IniFile()
                ini.CreateNewFile("#"c)
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                ini.SetFileComment(New String() {"Updated header", "Second line"})
                ini.SetSectionComment("Allgemein", New String() {"Updated section"})
                CollectionAssert.AreEqual(New String() {"Updated header", "Second line"}, ini.GetFileComment())
                CollectionAssert.AreEqual(New String() {"Updated section"}, ini.GetSectionComment("Allgemein"))
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.Contains("# Updated header", ini.GetFileContent())
                Assert.Contains("# Updated section", ini.GetFileContent())
                Assert.AreEqual(2, changeCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub SaveFileAs_WritesContentUpdatesPathAndRaisesChangeEvent()
            Using ini As New IniFile()
                ini.CreateNewFile()
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub
                Dim filePath As String = Path.Combine(_tempDirectory, "saved.ini")

                ini.SaveFileAs(filePath)

                Assert.IsTrue(ini.FileSaved)
                Assert.AreEqual(_tempDirectory, ini.FilePath)
                Assert.AreEqual("saved.ini", ini.FileName)
                Assert.IsTrue(File.Exists(filePath))
                Assert.Contains("AppName = MeineApp", File.ReadAllLines(filePath))
                Assert.AreEqual(1, changeCount)
            End Using
        End Sub

        <TestMethod>
        Public Sub SaveFileAs_RejectsEmptyPath()
            Using ini As New IniFile()
                Dim unused = Assert.ThrowsExactly(Of ArgumentException)(Sub() ini.SaveFileAs(String.Empty))
            End Using
        End Sub

        <TestMethod>
        Public Sub AutoSave_PersistsChangesAndRaisesOneChangeEvent()
            Using ini As New IniFile()
                ini.CreateNewFile()
                ini.FilePath = _tempDirectory
                ini.FileName = "autosave.ini"
                ini.SaveFile()
                ini.AutoSave = True
                Dim changeCount As Integer
                AddHandler ini.FileContentChanged, Sub(sender, e)
                                                       changeCount += 1
                                                   End Sub

                ini.SetEntryValue("Allgemein", "AppName", "AutoSaved")

                Assert.IsTrue(ini.FileSaved)
                Assert.AreEqual(1, changeCount)
                Assert.Contains("AppName = AutoSaved", File.ReadAllLines(Path.Combine(_tempDirectory, "autosave.ini")))
            End Using
        End Sub

        <TestMethod>
        Public Sub ContentView_PropertiesSynchronizeWithInnerControls()
            Using control As New ContentView()
                Dim groupBox As GroupBox = FindControl(Of GroupBox)(control, "GroupBox")
                Dim textBox As TextBox = FindControl(Of TextBox)(control, "TextBox")
                Dim lines As String() = {"first", "second"}

                control.TitelText = "INI-Inhalt"
                control.Lines = lines

                Assert.AreEqual("INI-Inhalt", control.TitelText)
                Assert.AreEqual("INI-Inhalt", groupBox.Text)
                Assert.AreSame(lines, control.Lines)
                CollectionAssert.AreEqual(lines, textBox.Lines)
                Assert.IsTrue(textBox.ReadOnly)
            End Using
        End Sub

        <TestMethod>
        Public Sub CommentEdit_CommitRaisesEventWithSectionAndLines()
            Using control As New CommentEdit()
                Dim textBox As TextBox = FindControl(Of TextBox)(control, "TextBox")
                Dim button As Button = FindControl(Of Button)(control, "Button")
                Dim eventArgs As CommentEditEventArgs = Nothing
                control.SectionName = "General"
                AddHandler control.CommentChanged, Sub(sender, e)
                                                       eventArgs = e
                                                   End Sub

                Assert.IsFalse(button.Enabled)
                textBox.Text = "first" & Environment.NewLine & "second"
                Assert.IsTrue(button.Enabled)
                button.PerformClick()

                Assert.IsNotNull(eventArgs)
                Assert.AreEqual("General", eventArgs.Section)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"first", "second"}, eventArgs.Comment)
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                CollectionAssert.AreEqual(eventArgs.Comment, control.Comment)
                Assert.IsFalse(button.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub CommentEdit_PropertiesUpdateEditorState()
            Using control As New CommentEdit()
                Dim groupBox As GroupBox = FindControl(Of GroupBox)(control, "GroupBox")
                Dim textBox As TextBox = FindControl(Of TextBox)(control, "TextBox")
                Dim button As Button = FindControl(Of Button)(control, "Button")
                Dim lines As String() = {"header", "detail"}

                control.TitelText = "Kommentar"
                control.Comment = lines
                control.SectionName = "General"

                Assert.AreEqual("Kommentar", groupBox.Text)
                Assert.AreEqual("General", control.SectionName)
                CollectionAssert.AreEqual(lines, control.Comment)
                CollectionAssert.AreEqual(lines, textBox.Lines)
                Assert.IsFalse(button.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub EntryValueEdit_CommitRaisesEventWithSelectionAndValue()
            Using control As New EntryValueEdit()
                Dim textBox As TextBox = FindControl(Of TextBox)(control, "TextBox")
                Dim button As Button = FindControl(Of Button)(control, "Button")
                Dim eventArgs As EntryValueEditEventArgs = Nothing
                control.SelectedSection = "General"
                control.SelectedEntry = "Name"
                control.Value = "before"
                AddHandler control.ValueChanged, Sub(sender, e)
                                                     eventArgs = e
                                                 End Sub

                textBox.Text = "after"
                Assert.IsTrue(button.Enabled)
                Assert.AreEqual("after", control.Value)
                button.PerformClick()

                Assert.IsNotNull(eventArgs)
                Assert.AreEqual("General", eventArgs.SelectedSection)
                Assert.AreEqual("Name", eventArgs.SelectedEntry)
                Assert.AreEqual("after", eventArgs.NewValue)
                Assert.IsFalse(button.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub EntryValueEdit_PropertiesSynchronizeWithInnerControls()
            Using control As New EntryValueEdit()
                Dim groupBox As GroupBox = FindControl(Of GroupBox)(control, "GroupBox")
                Dim textBox As TextBox = FindControl(Of TextBox)(control, "TextBox")
                Dim button As Button = FindControl(Of Button)(control, "Button")

                control.TitelText = "Wert"
                control.SelectedSection = "General"
                control.SelectedEntry = "Name"
                control.Value = "value"

                Assert.AreEqual("Wert", groupBox.Text)
                Assert.AreEqual("General", control.SelectedSection)
                Assert.AreEqual("Name", control.SelectedEntry)
                Assert.AreEqual("value", control.Value)
                Assert.AreEqual("value", textBox.Text)
                Assert.IsFalse(button.Enabled)
            End Using
        End Sub

        <TestMethod>
        Public Sub ListEdit_ListItemsAndSelectionSynchronizeWithListBox()
            Using control As New ListEdit()
                Dim listBox As ListBox = FindControl(Of ListBox)(control, "ListBox")
                Dim groupBox As GroupBox = FindControl(Of GroupBox)(control, "GroupBox")
                Dim selectedElement As String = Nothing
                AddHandler control.SelectedItemChanged, Sub(sender, e)
                                                            selectedElement = e.SelectedElement
                                                        End Sub

                control.TitelText = "Abschnitte"
                control.ListItems = New String() {"General", "Database"}

                Assert.AreEqual("Abschnitte", groupBox.Text)

#Disable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden
                CollectionAssert.AreEqual(New String() {"General", "Database"}, control.ListItems)
#Enable Warning CA1861 ' Konstantenmatrizen als Argumente vermeiden

                Assert.AreEqual(2, listBox.Items.Count)
                Assert.AreEqual(String.Empty, control.SelectedElement)

                listBox.SelectedIndex = 1
                Assert.AreEqual("Database", control.SelectedElement)
                Assert.AreEqual("Database", selectedElement)

                control.ListItems = New String() {"Logging"}
                Assert.AreEqual(-1, listBox.SelectedIndex)
                Assert.AreEqual(String.Empty, control.SelectedElement)
                Assert.AreEqual(String.Empty, selectedElement)
            End Using
        End Sub

        <TestMethod>
        Public Sub EventArgs_Constructors_PreservePayloads()
            Dim commentLines As String() = {"one", "two"}
            Dim commentArgs As New CommentEditEventArgs("General", commentLines)
            Dim valueArgs As New EntryValueEditEventArgs("General", "Name", "value")
            Dim addArgs As New ListEditAddEventArgs("New")
            Dim renameArgs As New ListEditRenameEventArgs("Old", "New")
            Dim removeArgs As New ListEditRemoveEventArgs("Remove")
            Dim selectedArgs As New ListEditSelectedElementChangedEventArgs("Selected")

            Assert.AreEqual("General", commentArgs.Section)
            Assert.AreSame(commentLines, commentArgs.Comment)
            Assert.AreEqual("General", valueArgs.SelectedSection)
            Assert.AreEqual("Name", valueArgs.SelectedEntry)
            Assert.AreEqual("value", valueArgs.NewValue)
            Assert.AreEqual("New", addArgs.ItemToAdd)
            Assert.AreEqual("Old", renameArgs.OldName)
            Assert.AreEqual("New", renameArgs.NewName)
            Assert.AreEqual("Remove", removeArgs.ItemToRemove)
            Assert.AreEqual("Selected", selectedArgs.SelectedElement)
        End Sub

        Private Shared Function FindControl(Of T As Control)(root As Control, controlName As String) As T
            For Each child As Control In root.Controls
                If TypeOf child Is T AndAlso String.Equals(child.Name, controlName, StringComparison.Ordinal) Then
                    Return DirectCast(child, T)
                End If

                Dim nested As T = TryFindControl(Of T)(child, controlName)
                If nested IsNot Nothing Then
                    Return nested
                End If
            Next

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
