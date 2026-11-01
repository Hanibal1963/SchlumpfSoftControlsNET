Namespace IniFileControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class ListEdit

        Inherits System.Windows.Forms.UserControl

        'UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            Me.GroupBox = New System.Windows.Forms.GroupBox()
            Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
            Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
            Me.ButtonAdd = New System.Windows.Forms.Button()
            Me.ButtonRename = New System.Windows.Forms.Button()
            Me.ButtonDelete = New System.Windows.Forms.Button()
            Me.ListBox = New System.Windows.Forms.ListBox()
            Me.GroupBox.SuspendLayout()
            Me.TableLayoutPanel1.SuspendLayout()
            Me.TableLayoutPanel2.SuspendLayout()
            Me.SuspendLayout()
            '
            'GroupBox
            '
            Me.GroupBox.Controls.Add(Me.TableLayoutPanel1)
            Me.GroupBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.GroupBox.Location = New System.Drawing.Point(0, 0)
            Me.GroupBox.Name = "GroupBox"
            Me.GroupBox.Size = New System.Drawing.Size(327, 94)
            Me.GroupBox.TabIndex = 0
            Me.GroupBox.TabStop = False
            Me.GroupBox.Text = "ListEdit"
            '
            'TableLayoutPanel1
            '
            Me.TableLayoutPanel1.ColumnCount = 1
            Dim unused = Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel2, 0, 1)
            Me.TableLayoutPanel1.Controls.Add(Me.ListBox, 0, 0)
            Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 16)
            Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
            Me.TableLayoutPanel1.RowCount = 2
            Dim unused6 = Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Dim unused5 = Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.TableLayoutPanel1.Size = New System.Drawing.Size(321, 75)
            Me.TableLayoutPanel1.TabIndex = 4
            '
            'TableLayoutPanel2
            '
            Me.TableLayoutPanel2.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
            Me.TableLayoutPanel2.AutoSize = True
            Me.TableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.TableLayoutPanel2.ColumnCount = 3
            Dim unused4 = Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Dim unused3 = Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Dim unused2 = Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.TableLayoutPanel2.Controls.Add(Me.ButtonAdd, 0, 0)
            Me.TableLayoutPanel2.Controls.Add(Me.ButtonRename, 1, 0)
            Me.TableLayoutPanel2.Controls.Add(Me.ButtonDelete, 2, 0)
            Me.TableLayoutPanel2.Location = New System.Drawing.Point(6, 39)
            Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
            Me.TableLayoutPanel2.RowCount = 1
            Dim unused1 = Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.TableLayoutPanel2.Size = New System.Drawing.Size(312, 33)
            Me.TableLayoutPanel2.TabIndex = 5
            '
            'ButtonAdd
            '
            Me.ButtonAdd.Location = New System.Drawing.Point(3, 3)
            Me.ButtonAdd.Name = "ButtonAdd"
            Me.ButtonAdd.Size = New System.Drawing.Size(98, 27)
            Me.ButtonAdd.TabIndex = 1
            Me.ButtonAdd.Text = "hinzufügen"
            Me.ButtonAdd.UseVisualStyleBackColor = True
            '
            'ButtonRename
            '
            Me.ButtonRename.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.ButtonRename.Location = New System.Drawing.Point(107, 3)
            Me.ButtonRename.Name = "ButtonRename"
            Me.ButtonRename.Size = New System.Drawing.Size(98, 27)
            Me.ButtonRename.TabIndex = 2
            Me.ButtonRename.Text = "umbenennen"
            Me.ButtonRename.UseVisualStyleBackColor = True
            '
            'ButtonDelete
            '
            Me.ButtonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl
            Me.ButtonDelete.Location = New System.Drawing.Point(211, 3)
            Me.ButtonDelete.Name = "ButtonDelete"
            Me.ButtonDelete.Size = New System.Drawing.Size(98, 27)
            Me.ButtonDelete.TabIndex = 3
            Me.ButtonDelete.Text = "löschen"
            Me.ButtonDelete.UseVisualStyleBackColor = True
            '
            'ListBox
            '
            Me.ListBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ListBox.FormattingEnabled = True
            Me.ListBox.Location = New System.Drawing.Point(3, 3)
            Me.ListBox.Name = "ListBox"
            Me.ListBox.Size = New System.Drawing.Size(315, 30)
            Me.ListBox.TabIndex = 0
            '
            'ListEdit
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.GroupBox)
            Me.Name = "ListEdit"
            Me.Size = New System.Drawing.Size(327, 94)
            Me.GroupBox.ResumeLayout(False)
            Me.TableLayoutPanel1.ResumeLayout(False)
            Me.TableLayoutPanel1.PerformLayout()
            Me.TableLayoutPanel2.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents ButtonDelete As System.Windows.Forms.Button
        Private WithEvents ButtonRename As System.Windows.Forms.Button
        Private WithEvents ButtonAdd As System.Windows.Forms.Button
        Private WithEvents ListBox As System.Windows.Forms.ListBox
        Private WithEvents GroupBox As System.Windows.Forms.GroupBox
        Private WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
        Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel

    End Class

End Namespace

