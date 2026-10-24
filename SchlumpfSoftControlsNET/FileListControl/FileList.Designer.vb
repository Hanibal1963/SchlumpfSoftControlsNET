Namespace FileListControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class FileList
        Inherits System.Windows.Forms.UserControl

        'UserControl1 überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                    Me._entryImageList?.Dispose()
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
            Me.listViewEntries = New System.Windows.Forms.ListView()
            Me.HeaderName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.HeaderType = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.HeaderSize = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.HeaderCreated = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.HeaderLastAccess = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.HeaderLastWrite = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
            Me.SuspendLayout()
            '
            'listViewEntries
            '
            Me.listViewEntries.AllowColumnReorder = True
            Me.listViewEntries.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.listViewEntries.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.HeaderName, Me.HeaderType, Me.HeaderSize, Me.HeaderCreated, Me.HeaderLastAccess, Me.HeaderLastWrite})
            Me.listViewEntries.Dock = System.Windows.Forms.DockStyle.Fill
            Me.listViewEntries.FullRowSelect = True
            Me.listViewEntries.GridLines = True
            Me.listViewEntries.HideSelection = False
            Me.listViewEntries.Location = New System.Drawing.Point(0, 0)
            Me.listViewEntries.Name = "listViewEntries"
            Me.listViewEntries.Size = New System.Drawing.Size(556, 269)
            Me.listViewEntries.TabIndex = 0
            Me.listViewEntries.UseCompatibleStateImageBehavior = False
            Me.listViewEntries.View = System.Windows.Forms.View.Details
            '
            'HeaderName
            '
            Me.HeaderName.Text = "Name"
            Me.HeaderName.Width = 120
            '
            'HeaderType
            '
            Me.HeaderType.Text = "Typ"
            '
            'HeaderSize
            '
            Me.HeaderSize.Text = "Größe"
            '
            'HeaderCreated
            '
            Me.HeaderCreated.Text = "Erstellt"
            Me.HeaderCreated.Width = 100
            '
            'HeaderLastAccess
            '
            Me.HeaderLastAccess.Text = "Letzter Zugriff"
            Me.HeaderLastAccess.Width = 100
            '
            'HeaderLastWrite
            '
            Me.HeaderLastWrite.Text = "Letzte Änderung"
            Me.HeaderLastWrite.Width = 100
            '
            'FileList
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            Me.Controls.Add(Me.listViewEntries)
            Me.Name = "FileList"
            Me.Size = New System.Drawing.Size(556, 269)
            Me.ResumeLayout(False)

        End Sub
        Private WithEvents listViewEntries As System.Windows.Forms.ListView
        Private WithEvents HeaderName As System.Windows.Forms.ColumnHeader
        Private WithEvents HeaderCreated As System.Windows.Forms.ColumnHeader
        Private WithEvents HeaderLastAccess As System.Windows.Forms.ColumnHeader
        Private WithEvents HeaderLastWrite As System.Windows.Forms.ColumnHeader
        Private WithEvents HeaderSize As System.Windows.Forms.ColumnHeader
        Private WithEvents HeaderType As System.Windows.Forms.ColumnHeader
    End Class

End Namespace

