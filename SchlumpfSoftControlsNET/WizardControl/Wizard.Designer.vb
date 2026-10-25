Imports System.ComponentModel

Namespace WizardControl

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class Wizard

        Inherits System.Windows.Forms.UserControl

        <DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(disposing As Boolean)
            Try
                If disposing AndAlso Me.components IsNot Nothing Then
                    Me.components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        Private ReadOnly components As IContainer

        'HINWEIS: Das folgende Verfahren ist für den Windows Form Designer erforderlich
        'Es kann mit dem Windows Form Designer geändert werden.
        'Ändern Sie es nicht mit dem Code-Editor.
        <DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.ButtonHelp = New System.Windows.Forms.Button()
            Me.ButtonBack = New System.Windows.Forms.Button()
            Me.ButtonNext = New System.Windows.Forms.Button()
            Me.ButtonCancel = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'ButtonHelp
            '
            Me.ButtonHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
            Me.ButtonHelp.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.ButtonHelp.Location = New System.Drawing.Point(8, 189)
            Me.ButtonHelp.Name = "ButtonHelp"
            Me.ButtonHelp.Size = New System.Drawing.Size(75, 23)
            Me.ButtonHelp.TabIndex = 9
            Me.ButtonHelp.Text = "&Hilfe"
            '
            'ButtonBack
            '
            Me.ButtonBack.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ButtonBack.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.ButtonBack.Location = New System.Drawing.Point(108, 189)
            Me.ButtonBack.Name = "ButtonBack"
            Me.ButtonBack.Size = New System.Drawing.Size(75, 23)
            Me.ButtonBack.TabIndex = 6
            Me.ButtonBack.Text = "< &Zurück"
            '
            'ButtonNext
            '
            Me.ButtonNext.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ButtonNext.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.ButtonNext.Location = New System.Drawing.Point(189, 189)
            Me.ButtonNext.Name = "ButtonNext"
            Me.ButtonNext.Size = New System.Drawing.Size(75, 23)
            Me.ButtonNext.TabIndex = 7
            Me.ButtonNext.Text = "&Weiter >"
            '
            'ButtonCancel
            '
            Me.ButtonCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
            Me.ButtonCancel.Location = New System.Drawing.Point(269, 189)
            Me.ButtonCancel.Name = "ButtonCancel"
            Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
            Me.ButtonCancel.TabIndex = 8
            Me.ButtonCancel.Text = "Abbruch"
            '
            'Wizard
            '
            Me.Controls.Add(Me.ButtonHelp)
            Me.Controls.Add(Me.ButtonBack)
            Me.Controls.Add(Me.ButtonNext)
            Me.Controls.Add(Me.ButtonCancel)
            Me.Name = "Wizard"
            Me.Size = New System.Drawing.Size(353, 221)
            Me.ResumeLayout(False)

        End Sub

        Public WithEvents ButtonHelp As Button
        Public WithEvents ButtonBack As Button
        Public WithEvents ButtonNext As Button
        Public WithEvents ButtonCancel As Button

    End Class

End Namespace
