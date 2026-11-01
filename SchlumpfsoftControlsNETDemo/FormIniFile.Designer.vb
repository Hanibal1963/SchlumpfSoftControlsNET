<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormIniFile
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.MenuStrip = New System.Windows.Forms.MenuStrip()
        Me.DateiToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemOeffnen = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemSchliessen = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemBeenden = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemSpeichern = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemSpeichernUnter = New System.Windows.Forms.ToolStripMenuItem()
        Me.SplitContainer = New System.Windows.Forms.SplitContainer()
        Me.ContentView = New SchlumpfSoft.ControlsNET.IniFileControl.ContentView()
        Me.CommentEditFileComment = New SchlumpfSoft.ControlsNET.IniFileControl.CommentEdit()
        Me.ListEditSections = New SchlumpfSoft.ControlsNET.IniFileControl.ListEdit()
        Me.EntryValueEdit = New SchlumpfSoft.ControlsNET.IniFileControl.EntryValueEdit()
        Me.CommentEditSections = New SchlumpfSoft.ControlsNET.IniFileControl.CommentEdit()
        Me.ListEditEntrys = New SchlumpfSoft.ControlsNET.IniFileControl.ListEdit()
        Me.IniFile = New SchlumpfSoft.ControlsNET.IniFileControl.IniFile()
        Me.StatusStrip = New System.Windows.Forms.StatusStrip()
        Me.MenuStrip.SuspendLayout()
        CType(Me.SplitContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer.Panel1.SuspendLayout()
        Me.SplitContainer.Panel2.SuspendLayout()
        Me.SplitContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip
        '
        Me.MenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DateiToolStripMenuItem})
        Me.MenuStrip.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip.Name = "MenuStrip"
        Me.MenuStrip.Size = New System.Drawing.Size(739, 24)
        Me.MenuStrip.TabIndex = 5
        Me.MenuStrip.Text = "MenuStrip"
        '
        'DateiToolStripMenuItem
        '
        Me.DateiToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemOeffnen, Me.ToolStripMenuItemSchliessen, Me.ToolStripMenuItemBeenden, Me.ToolStripMenuItemSpeichern, Me.ToolStripMenuItemSpeichernUnter})
        Me.DateiToolStripMenuItem.Name = "DateiToolStripMenuItem"
        Me.DateiToolStripMenuItem.Size = New System.Drawing.Size(46, 20)
        Me.DateiToolStripMenuItem.Text = "Datei"
        '
        'ToolStripMenuItemOeffnen
        '
        Me.ToolStripMenuItemOeffnen.Name = "ToolStripMenuItemOeffnen"
        Me.ToolStripMenuItemOeffnen.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemOeffnen.Text = "öffnen ..."
        '
        'ToolStripMenuItemSchliessen
        '
        Me.ToolStripMenuItemSchliessen.Name = "ToolStripMenuItemSchliessen"
        Me.ToolStripMenuItemSchliessen.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemSchliessen.Text = "schliessen"
        '
        'ToolStripMenuItemBeenden
        '
        Me.ToolStripMenuItemBeenden.Name = "ToolStripMenuItemBeenden"
        Me.ToolStripMenuItemBeenden.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemBeenden.Text = "beenden"
        '
        'ToolStripMenuItemSpeichern
        '
        Me.ToolStripMenuItemSpeichern.Name = "ToolStripMenuItemSpeichern"
        Me.ToolStripMenuItemSpeichern.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemSpeichern.Text = "speichern"
        '
        'ToolStripMenuItemSpeichernUnter
        '
        Me.ToolStripMenuItemSpeichernUnter.Name = "ToolStripMenuItemSpeichernUnter"
        Me.ToolStripMenuItemSpeichernUnter.Size = New System.Drawing.Size(168, 22)
        Me.ToolStripMenuItemSpeichernUnter.Text = "speichern unter ..."
        '
        'SplitContainer
        '
        Me.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer.Location = New System.Drawing.Point(0, 24)
        Me.SplitContainer.Name = "SplitContainer"
        '
        'SplitContainer.Panel1
        '
        Me.SplitContainer.Panel1.Controls.Add(Me.ContentView)
        Me.SplitContainer.Panel1.Controls.Add(Me.CommentEditFileComment)
        '
        'SplitContainer.Panel2
        '
        Me.SplitContainer.Panel2.Controls.Add(Me.ListEditSections)
        Me.SplitContainer.Panel2.Controls.Add(Me.EntryValueEdit)
        Me.SplitContainer.Panel2.Controls.Add(Me.CommentEditSections)
        Me.SplitContainer.Panel2.Controls.Add(Me.ListEditEntrys)
        Me.SplitContainer.Size = New System.Drawing.Size(739, 527)
        Me.SplitContainer.SplitterDistance = 338
        Me.SplitContainer.TabIndex = 8
        '
        'ContentView
        '
        Me.ContentView.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ContentView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ContentView.Lines = Nothing
        Me.ContentView.Location = New System.Drawing.Point(0, 0)
        Me.ContentView.Name = "ContentView"
        Me.ContentView.Size = New System.Drawing.Size(333, 334)
        Me.ContentView.TabIndex = 0
        Me.ContentView.TitelText = "Dateiinhalt"
        '
        'CommentEditFileComment
        '
        Me.CommentEditFileComment.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CommentEditFileComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.CommentEditFileComment.Comment = New String() {""}
        Me.CommentEditFileComment.Location = New System.Drawing.Point(0, 340)
        Me.CommentEditFileComment.Name = "CommentEditFileComment"
        Me.CommentEditFileComment.SectionName = Nothing
        Me.CommentEditFileComment.Size = New System.Drawing.Size(333, 161)
        Me.CommentEditFileComment.TabIndex = 1
        Me.CommentEditFileComment.TitelText = "Dateikommentar bearbeiten"
        '
        'ListEditSections
        '
        Me.ListEditSections.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListEditSections.AutoSize = True
        Me.ListEditSections.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ListEditSections.ListItems = New String() {""}
        Me.ListEditSections.Location = New System.Drawing.Point(3, 3)
        Me.ListEditSections.Name = "ListEditSections"
        Me.ListEditSections.Size = New System.Drawing.Size(390, 150)
        Me.ListEditSections.TabIndex = 2
        Me.ListEditSections.TitelText = "Abschnitte bearbeiten"
        '
        'EntryValueEdit
        '
        Me.EntryValueEdit.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.EntryValueEdit.Location = New System.Drawing.Point(3, 422)
        Me.EntryValueEdit.Name = "EntryValueEdit"
        Me.EntryValueEdit.SelectedEntry = Nothing
        Me.EntryValueEdit.SelectedSection = ""
        Me.EntryValueEdit.Size = New System.Drawing.Size(390, 79)
        Me.EntryValueEdit.TabIndex = 7
        Me.EntryValueEdit.TitelText = "Eintragswert bearbeiten"
        Me.EntryValueEdit.Value = ""
        '
        'CommentEditSections
        '
        Me.CommentEditSections.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CommentEditSections.Comment = New String() {""}
        Me.CommentEditSections.Location = New System.Drawing.Point(3, 162)
        Me.CommentEditSections.Name = "CommentEditSections"
        Me.CommentEditSections.SectionName = Nothing
        Me.CommentEditSections.Size = New System.Drawing.Size(390, 107)
        Me.CommentEditSections.TabIndex = 6
        Me.CommentEditSections.TitelText = "Abschnittskommentar bearbeiten"
        '
        'ListEditEntrys
        '
        Me.ListEditEntrys.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListEditEntrys.ListItems = New String() {""}
        Me.ListEditEntrys.Location = New System.Drawing.Point(3, 275)
        Me.ListEditEntrys.Name = "ListEditEntrys"
        Me.ListEditEntrys.Size = New System.Drawing.Size(390, 141)
        Me.ListEditEntrys.TabIndex = 3
        Me.ListEditEntrys.TitelText = "Einträge bearbeiten"
        '
        'IniFile
        '
        Me.IniFile.AutoSave = False
        Me.IniFile.CommentPrefix = Global.Microsoft.VisualBasic.ChrW(59)
        Me.IniFile.FileName = "neue Datei.ini"
        Me.IniFile.FilePath = ""
        '
        'StatusStrip
        '
        Me.StatusStrip.Location = New System.Drawing.Point(0, 529)
        Me.StatusStrip.Name = "StatusStrip"
        Me.StatusStrip.Size = New System.Drawing.Size(739, 22)
        Me.StatusStrip.TabIndex = 9
        Me.StatusStrip.Text = "StatusStrip1"
        '
        'FormIniFile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(739, 551)
        Me.Controls.Add(Me.StatusStrip)
        Me.Controls.Add(Me.SplitContainer)
        Me.Controls.Add(Me.MenuStrip)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(610, 590)
        Me.Name = "FormIniFile"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "IniFile Demo"
        Me.MenuStrip.ResumeLayout(False)
        Me.MenuStrip.PerformLayout()
        Me.SplitContainer.Panel1.ResumeLayout(False)
        Me.SplitContainer.Panel2.ResumeLayout(False)
        Me.SplitContainer.Panel2.PerformLayout()
        CType(Me.SplitContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents IniFile As SchlumpfSoft.ControlsNET.IniFileControl.IniFile
    Private WithEvents ContentView As SchlumpfSoft.ControlsNET.IniFileControl.ContentView
    Private WithEvents CommentEditFileComment As SchlumpfSoft.ControlsNET.IniFileControl.CommentEdit
    Private WithEvents ListEditSections As SchlumpfSoft.ControlsNET.IniFileControl.ListEdit
    Private WithEvents ListEditEntrys As SchlumpfSoft.ControlsNET.IniFileControl.ListEdit
    Private WithEvents MenuStrip As MenuStrip
    Friend WithEvents DateiToolStripMenuItem As ToolStripMenuItem
    Private WithEvents ToolStripMenuItemOeffnen As ToolStripMenuItem
    Private WithEvents ToolStripMenuItemSchliessen As ToolStripMenuItem
    Private WithEvents ToolStripMenuItemBeenden As ToolStripMenuItem
    Private WithEvents ToolStripMenuItemSpeichern As ToolStripMenuItem
    Private WithEvents ToolStripMenuItemSpeichernUnter As ToolStripMenuItem
    Private WithEvents CommentEditSections As SchlumpfSoft.ControlsNET.IniFileControl.CommentEdit
    Private WithEvents EntryValueEdit As SchlumpfSoft.ControlsNET.IniFileControl.EntryValueEdit
    Private WithEvents SplitContainer As SplitContainer
    Private WithEvents StatusStrip As StatusStrip
End Class
