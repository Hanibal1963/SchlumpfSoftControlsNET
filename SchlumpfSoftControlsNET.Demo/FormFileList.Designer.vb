<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormFileList
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
        Me.FileList = New SchlumpfSoft.ControlsNET.FileListControl.FileList()
        Me.Button_SelectPath = New System.Windows.Forms.Button()
        Me.Label_SelectedPath = New System.Windows.Forms.Label()
        Me.TableLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.FolderBrowserDialog = New System.Windows.Forms.FolderBrowserDialog()
        Me.TableLayoutPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'FileList
        '
        Me.FileList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.TableLayoutPanel.SetColumnSpan(Me.FileList, 2)
        Me.FileList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FileList.Location = New System.Drawing.Point(4, 41)
        Me.FileList.Name = "FileList"
        Me.FileList.Size = New System.Drawing.Size(701, 408)
        Me.FileList.TabIndex = 0
        '
        'Button_SelectPath
        '
        Me.Button_SelectPath.Location = New System.Drawing.Point(4, 4)
        Me.Button_SelectPath.Name = "Button_SelectPath"
        Me.Button_SelectPath.Size = New System.Drawing.Size(114, 30)
        Me.Button_SelectPath.TabIndex = 1
        Me.Button_SelectPath.Text = "Pfad wählen ..."
        Me.Button_SelectPath.UseVisualStyleBackColor = True
        '
        'Label_SelectedPath
        '
        Me.Label_SelectedPath.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label_SelectedPath.Location = New System.Drawing.Point(125, 1)
        Me.Label_SelectedPath.Name = "Label_SelectedPath"
        Me.Label_SelectedPath.Size = New System.Drawing.Size(580, 36)
        Me.Label_SelectedPath.TabIndex = 2
        Me.Label_SelectedPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TableLayoutPanel
        '
        Me.TableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.[Single]
        Me.TableLayoutPanel.ColumnCount = 2
        Me.TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel.Controls.Add(Me.Button_SelectPath, 0, 0)
        Me.TableLayoutPanel.Controls.Add(Me.Label_SelectedPath, 1, 0)
        Me.TableLayoutPanel.Controls.Add(Me.FileList, 0, 1)
        Me.TableLayoutPanel.Location = New System.Drawing.Point(12, 12)
        Me.TableLayoutPanel.Name = "TableLayoutPanel"
        Me.TableLayoutPanel.RowCount = 2
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel.Size = New System.Drawing.Size(709, 453)
        Me.TableLayoutPanel.TabIndex = 3
        '
        'FolderBrowserDialog
        '
        Me.FolderBrowserDialog.Description = "Wähle einen Ordner"
        Me.FolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer
        Me.FolderBrowserDialog.SelectedPath = "d:\Dokumente"
        Me.FolderBrowserDialog.ShowNewFolderButton = False
        '
        'FormFileList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(735, 478)
        Me.Controls.Add(Me.TableLayoutPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormFileList"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "FileList Demo"
        Me.TableLayoutPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents FileList As SchlumpfSoft.ControlsNET.FileListControl.FileList
    Private WithEvents TableLayoutPanel As TableLayoutPanel
    Private WithEvents Button_SelectPath As Button
    Private WithEvents Label_SelectedPath As Label
    Private WithEvents FolderBrowserDialog As FolderBrowserDialog
End Class
