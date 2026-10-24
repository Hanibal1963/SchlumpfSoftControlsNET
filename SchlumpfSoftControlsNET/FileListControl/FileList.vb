' --------------------------------------------------------------------------------------------------------
' Datei: FileList.vb
' Author: Andreas Sauer
' Datum: 29.04.2026
' --------------------------------------------------------------------------------------------------------

Imports System.Linq
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading.Tasks

Namespace FileListControl

    ''' <summary>
    ''' FileList ist ein benutzerdefiniertes Control, das Dateien und Verzeichnisse in einem ListView darstellt. Es
    ''' bietet Funktionalität zum Sortieren von Spalten, Anpassen der Spaltengröße sowie zum Speichern und Laden der
    ''' Spaltenreihenfolge.
    ''' </summary>
    <ToolboxItem(True)>
    <DesignTimeVisible(True)>
    <Description("Steuerelement zum Anzeigen der Dateien und Unterordner eines Ordners.")>
    <ToolboxBitmap(GetType(FileList), "FileListControl.FileList.bmp")>
    Public Class FileList

        Inherits UserControl

#Region "Konstanten"

        Private Const FolderImageKey As String = "__folder"
        Private Const DefaultFileImageKey As String = "__file"

#End Region

#Region "Variablen"

        Private _startFolder As String = String.Empty
        Private _sortColumn As System.Int32 = 1
        Private _sortOrder As SortOrder = SortOrder.Descending
        Private _autoResizeColumnsEnabled As Boolean = True
        ' Zwischenspeicher für den Fall, dass die Spaltenreihenfolge gesetzt wird, bevor das Handle existiert.
        Private _pendingColumnOrder As String = String.Empty
        Private ReadOnly _entryImageList As New ImageList()

#End Region

#Region "Ereignisse"

        Private Event StartFolderChanged As System.EventHandler

#End Region

#Region "Öffentliche Eigenschaften"

        ''' <summary>
        ''' Ruft den Pfad des Startordners ab oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Wenn ein neuer Ordner gesetzt wird, wird das <c>StartFolderChanged</c>-Event ausgelöst.
        ''' </remarks>
        ''' <value>
        ''' Der Pfad zum Startordner oder eine leere Zeichenfolge.
        ''' </value>
        <Description("Ruft den Pfad des Startordners ab oder legt ihn fest.")>
        <Category("Data")>
        <Browsable(True)>
        <DefaultValue("")>
        Public Property StartFolder As String
            Get
                Return Me._startFolder
            End Get
            Set(value As String)
                ' Normalisiere den Wert: Nothing wird zu leerer Zeichenfolge, führende/trailende Leerzeichen werden entfernt.
                Dim normalizedValue As String = If(value, String.Empty).Trim()
                ' Vergleich ist nicht case-sensitiv, um unnötige Reloads bei gleichem Pfad zu vermeiden.
                If String.Equals(Me._startFolder, normalizedValue, System.StringComparison.OrdinalIgnoreCase) Then
                    Return
                End If
                Me._startFolder = normalizedValue
                RaiseEvent StartFolderChanged(Me, System.EventArgs.Empty)
            End Set
        End Property

        ''' <summary>
        ''' Ruft einen Wert ab, der angibt, ob die automatische Größenanpassung von Spalten aktiviert ist, oder legt
        ''' diesen Wert fest.
        ''' </summary>
        ''' <value>True, wenn die Spaltengröße automatisch angepasst werden soll; andernfalls False.</value>
        <Description("Ruft einen Wert ab, der angibt, ob die automatische Größenanpassung von Spalten aktiviert ist, oder legt diesen Wert fest.")>
        <Category("Behavior")>
        <Browsable(True)>
        <DefaultValue(True)>
        Public Property AutoResizeColumnsEnabled As Boolean
            Get
                Return Me._autoResizeColumnsEnabled
            End Get
            Set(value As Boolean)
                If Me._autoResizeColumnsEnabled = value Then
                    Return
                End If
                Me._autoResizeColumnsEnabled = value
                ' Bei Aktivierung sofort die Spaltenbreiten anpassen
                If Me._autoResizeColumnsEnabled Then
                    Me.AdjustColumnWidths()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Ruft den aktuellen Zustand der Spaltenreihenfolge als Zeichenfolge ab oder legt ihn fest.
        ''' </summary>
        ''' <value>Ein String mit der Spaltenreihenfolge (z.B. "Name;Type;Size;...").</value>
        ''' <remarks>Das Format ist eine durch Semikolons getrennte Liste von Spaltenschlüsseln.</remarks>
        <Description("Ruft den aktuellen Zustand der Spaltenreihenfolge als Zeichenfolge ab oder legt ihn fest.")>
        <Category("Layout")>
        <Browsable(True)>
        <DefaultValue("Name;Type;Size;Created;LastAccess;LastWrite")>
        Public Property ColumnOrderState As String
            Get
                Return Me.GetColumnOrderState()
            End Get
            Set(value As String)
                Me._pendingColumnOrder = If(value, String.Empty).Trim()
                Me.ApplyColumnOrderState(Me._pendingColumnOrder)
            End Set
        End Property
#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Initialisiert eine neue Instanz der FileList-Klasse. Initialisiert die Benutzeroberfläche, die
        ''' Bildunterstützung und lädt die gespeicherte Spaltenreihenfolge.
        ''' </summary>
        Public Sub New()

            ' Initialisiert die vom Designer erzeugten Controls.
            Me.InitializeComponent()
            ' Verknüpft die ImageList mit dem ListView und lädt Standard-Icons.
            Me.InitializeImageSupport()
            ' Stellt ggf. die zuletzt gespeicherte Spaltenreihenfolge wieder her.
            Me.LoadColumnOrderFromSettings()

        End Sub

        ''' <summary>
        ''' Aktualisiert die Einträge im ListView durch erneutes Laden des Startordners.
        ''' </summary>
        Public Sub RefreshEntries()
            RaiseEvent StartFolderChanged(Me, System.EventArgs.Empty)
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Fügt einen neuen Eintrag (Datei oder Ordner) zum ListView hinzu.
        ''' </summary>
        ''' <param name="name">Der Name des Eintrags.</param>
        ''' <param name="created">Das Erstellungsdatum.</param>
        ''' <param name="lastAccess">Das Datum des letzten Zugriffs.</param>
        ''' <param name="lastWrite">Das Datum der letzten Änderung.</param>
        ''' <param name="sizeText">Die Größe als formatierter Text.</param>
        ''' <param name="typeText">Der Dateityp als Text.</param>
        ''' <param name="imageKey">Der Bildschlüssel aus der ImageList.</param>
        Private Sub AddItem(
                       name As String, created As System.DateTime, lastAccess As System.DateTime, lastWrite As System.DateTime,
                       sizeText As String, typeText As String, imageKey As String)

            ' Erstelle ein neues ListViewItem mit Name und zugehörigem Bild
            Dim item As New ListViewItem(name) With {.ImageKey = imageKey}

            ' Füge die weiteren Spalteneinträge hinzu: Erstellt, Letzter Zugriff, Letzte Änderung, Größe, Typ
            item.SubItems.AddRange(
            {typeText, sizeText,
             created.ToString("g", Globalization.CultureInfo.CurrentCulture),
             lastAccess.ToString("g", Globalization.CultureInfo.CurrentCulture),
             lastWrite.ToString("g", Globalization.CultureInfo.CurrentCulture)})

            ' Füge das Item zur ListView hinzu
            Dim unused = Me.listViewEntries.Items.Add(item)

        End Sub

        ''' <summary>
        ''' Behandelt das Disposed-Event des Controls. Speichert die aktuelle Spaltenreihenfolge in den Einstellungen.
        ''' </summary>
        Private Sub FileList_Disposed(sender As Object, e As System.EventArgs) Handles Me.Disposed

            Me.SaveColumnOrderToSettings()

        End Sub

        ''' <summary>
        ''' Wendet die aktuelle Sortierkonfiguration auf den ListView an.
        ''' </summary>
        Private Sub ApplyCurrentSort()

            ' Erstelle den Comparer mit der aktuellen Spalten- und Sortierreihenfolge
            Me.listViewEntries.ListViewItemSorter = New ListViewColumnComparer(Me._sortColumn, Me._sortOrder)

            ' Führe die Sortierung durch
            Me.listViewEntries.Sort()

        End Sub

        ''' <summary>
        ''' Gibt den aktuellen Zustand der Spaltenreihenfolge als durch Semikolons getrennte Zeichenfolge zurück.
        ''' </summary>
        ''' <returns>Ein String mit der Spaltenreihenfolge oder eine leere Zeichenfolge.</returns>
        Private Function GetColumnOrderState() As String

            ' Wenn keine Spalten vorhanden sind, gebe leere Zeichenfolge zurück
            If Me.listViewEntries.Columns.Count = 0 Then
                Return String.Empty
            End If

            ' Hole alle Spalten, sortiere sie nach ihrem DisplayIndex und erstelle einen String
            Return String.Join(";", Enumerable.Range(0, Me.listViewEntries.Columns.Count).Select(Function(i) Me.listViewEntries.Columns(i)).OrderBy(Function(column) column.DisplayIndex).Select(Function(column) Me.GetColumnKey(column)))

        End Function

        ''' <summary>
        ''' Wendet einen gespeicherten Zustand der Spaltenreihenfolge auf den ListView an. Validiert, dass alle Spalten
        ''' in der richtigen Anzahl vorhanden sind.
        ''' </summary>
        ''' <param name="orderState">Ein String mit der Spaltenreihenfolge (durch Semikolons getrennt).</param>
        Private Sub ApplyColumnOrderState(orderState As String)

            ' Prüfe auf leere Eingabe oder fehlende Spalten
            If String.IsNullOrWhiteSpace(orderState) OrElse Me.listViewEntries.Columns.Count = 0 Then
                Return
            End If

            ' Teile den String auf und entferne leere Einträge
            Dim tokens As String() = orderState.Split(";"c).Select(Function(token) token.Trim()).Where(Function(token) token.Length > 0).
            ToArray()

            ' Prüfe, ob die Anzahl der Tokens mit der Spaltenanzahl übereinstimmt
            If tokens.Length <> Me.listViewEntries.Columns.Count Then
                Return
            End If

            ' Erstelle eine Liste mit den Spalten in der neuen Reihenfolge
            Dim targetOrder As New System.Collections.Generic.List(Of ColumnHeader)(tokens.Length)
            For Each token As String In tokens

                ' Hole die Spalte anhand ihres Schlüssels
                Dim column As ColumnHeader = Me.GetColumnByKey(token)

                ' Wenn die Spalte nicht gefunden wird oder duplikat ist, abbrechen
                If column Is Nothing Then
                    Return
                End If

                If targetOrder.Contains(column) Then
                    Return
                End If

                targetOrder.Add(column)

            Next

            ' Setze die DisplayIndex-Werte in der neuen Reihenfolge
            For displayIndex As System.Int32 = 0 To targetOrder.Count - 1
                targetOrder(displayIndex).DisplayIndex = displayIndex
            Next

        End Sub

        ''' <summary>
        ''' Lädt die gespeicherte Spaltenreihenfolge aus den Anwendungseinstellungen.
        ''' </summary>
        Private Sub LoadColumnOrderFromSettings()

            Try

                ' Hole die gespeicherte Spaltenreihenfolge aus den Einstellungen
                Dim savedState As String = My.Settings.ListViewColumnOrder
                If String.IsNullOrWhiteSpace(savedState) Then
                    Return
                End If

                ' Wende die gespeicherte Reihenfolge an
                Me.ColumnOrderState = savedState

            Catch
                ' Fehlerbehandlung - still bei fehlerhaften Einstellungen
            End Try

        End Sub

        ''' <summary>
        ''' Speichert die aktuelle Spaltenreihenfolge in den Anwendungseinstellungen.
        ''' </summary>
        Private Sub SaveColumnOrderToSettings()

            If Me.listViewEntries.Columns.Count = 0 Then
                Return
            End If

            Try

                ' Hole den aktuellen Zustand und speichere ihn
                My.Settings.ListViewColumnOrder = Me.GetColumnOrderState()
                My.Settings.Save()

            Catch
                ' Fehlerbehandlung - still bei Speicherfehlern
            End Try

        End Sub

        ''' <summary>
        ''' Passt die Breite aller Spalten basierend auf dem Inhalt und der Kopfzeile an.
        ''' </summary>
        Private Sub AdjustColumnWidths()

            If Me.listViewEntries.Columns.Count = 0 Then
                Return
            End If

            ' Iteriere durch jede Spalte
            For columnIndex As System.Int32 = 0 To Me.listViewEntries.Columns.Count - 1

                ' Passe zuerst auf Inhalts-Größe an und speichere die Breite
                Me.listViewEntries.AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.ColumnContent)
                Dim contentWidth As System.Int32 = Me.listViewEntries.Columns(columnIndex).Width

                ' Passe dann auf Header-Größe an
                Me.listViewEntries.AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.HeaderSize)
                Dim headerWidth As System.Int32 = Me.listViewEntries.Columns(columnIndex).Width

                ' Setze die Spaltenbreite auf das Maximum von Inhalt und Header
                Me.listViewEntries.Columns(columnIndex).Width = System.Math.Max(contentWidth, headerWidth)

            Next

        End Sub

        ''' <summary>
        ''' Initialisiert die Bildunterstützung mit Ordner- und Dateisymbolen.
        ''' </summary>
        Private Sub InitializeImageSupport()

            ' Konfiguriere die ImageList mit 32-Bit Farben und 16x16 Pixel Größe
            Me._entryImageList.ColorDepth = ColorDepth.Depth32Bit
            Me._entryImageList.ImageSize = New Size(16, 16)

            ' Füge die ImageList zum ListView hinzu
            Me.listViewEntries.SmallImageList = Me._entryImageList

            ' Füge die Standard-Symbole hinzu
            Me.AddImageIfMissing(FolderImageKey, GetSmallSystemIcon("folder", NativeMethods.FILE_ATTRIBUTE_DIRECTORY))
            Me.AddImageIfMissing(DefaultFileImageKey, GetSmallSystemIcon("file", NativeMethods.FILE_ATTRIBUTE_NORMAL))

        End Sub

        ''' <summary>
        ''' Holt den Bildschlüssel für eine Datei basierend auf ihrer Erweiterung. Wenn das Symbol noch nicht in der
        ''' ImageList vorhanden ist, wird es hinzugefügt.
        ''' </summary>
        ''' <param name="file">Das <see cref="System.IO.FileInfo"/>-Objekt der Datei.</param>
        ''' <returns>Der Bildschlüssel aus der ImageList.</returns>
        Private Function GetFileImageKey(file As System.IO.FileInfo) As String

            ' Verwende die Dateierweiterung als Schlüssel, oder den Standard-Dateischlüssel
            Dim key As String = If(String.IsNullOrWhiteSpace(file.Extension), DefaultFileImageKey, file.Extension.ToLowerInvariant())

            ' Wenn das Bild bereits vorhanden ist, gebe den Schlüssel zurück
            If Me._entryImageList.Images.ContainsKey(key) Then
                Return key
            End If

            ' Hole das Symbol für den Dateityp vom System
            Dim icon = If(key = DefaultFileImageKey,
                GetSmallSystemIcon("file", NativeMethods.FILE_ATTRIBUTE_NORMAL),
                GetSmallSystemIcon("*" & key, NativeMethods.FILE_ATTRIBUTE_NORMAL))

            ' Füge das Symbol zur ImageList hinzu
            Me.AddImageIfMissing(key, icon)
            Return key

        End Function

        ''' <summary>
        ''' Fügt ein Bild zur ImageList hinzu, wenn es noch nicht vorhanden ist.
        ''' </summary>
        ''' <param name="key">Der Schlüssel für das Bild.</param>
        ''' <param name="icon">Das Icon-Objekt, das hinzugefügt werden soll.</param>
        <CodeAnalysis.SuppressMessage("Style", "IDE0031:NULL-Weitergabe verwenden", Justification:="<Ausstehend>")>
        Private Sub AddImageIfMissing(key As String, icon As Icon)

            ' Prüfe, ob das Bild bereits vorhanden ist
            If Me._entryImageList.Images.ContainsKey(key) Then
                ' Wenn ja, gebe den Speicher des Icons frei und kehre zurück
                If icon IsNot Nothing Then
                    icon.Dispose()
                End If
                Return
            End If

            ' Wenn das Icon null ist, verwende einen Fallback
            If icon Is Nothing Then
                Dim fallback As Bitmap = SystemIcons.Application.ToBitmap()
                Me._entryImageList.Images.Add(key, fallback)
                Return
            End If

            ' Konvertiere das Icon zu einer Bitmap und füge es hinzu
            Dim bitmap As Bitmap = icon.ToBitmap()
            Me._entryImageList.Images.Add(key, bitmap)
            icon.Dispose()

        End Sub

        ''' <summary>
        ''' Holt das Symbol einer Datei oder eines Ordners vom System. Verwendet Windows-API-Aufrufe
        ''' (<c>SHGetFileInfo</c>), um das Symbol zu beschaffen.
        ''' </summary>
        ''' <param name="path">Pfad, Dateiname oder Suchmuster (z. B. <c>*.txt</c>).</param>
        ''' <param name="fileAttributes">Die Dateiattribute (z. B. <c>FILE_ATTRIBUTE_DIRECTORY</c>).</param>
        ''' <returns>
        ''' Ein geklontes <see cref="Icon"/> oder <c>Nothing</c>, wenn kein Symbol ermittelt werden konnte.
        ''' </returns>
        Private Shared Function GetSmallSystemIcon(path As String, fileAttributes As System.UInt32) As Icon

            ' Erstelle eine SHFILEINFO-Struktur für die API-Aufrufe
            Dim fileInfo As New NativeMethods.SHFILEINFO()

            ' Rufe SHGetFileInfo auf um das Symbol zu erhalten
            Dim result As System.IntPtr = NativeMethods.SHGetFileInfo(
            path, fileAttributes, fileInfo,
            CUInt(System.Runtime.InteropServices.Marshal.SizeOf(fileInfo)),
            NativeMethods.SHGFI_ICON Or NativeMethods.SHGFI_SMALLICON Or NativeMethods.SHGFI_USEFILEATTRIBUTES)

            ' Prüfe auf Fehler
            If result = System.IntPtr.Zero OrElse fileInfo.hIcon = System.IntPtr.Zero Then
                Return Nothing
            End If

            Dim icon As Icon = Nothing

            Try

                ' Das Icon wird geklont, damit es nach dem Freigeben des nativen Handles weiter gültig bleibt.
                icon = DirectCast(Icon.FromHandle(fileInfo.hIcon).Clone(), Icon)

            Finally

                ' Gebe das Handle frei
                Dim unused = NativeMethods.DestroyIcon(fileInfo.hIcon)

            End Try

            Return icon

        End Function

        ''' <summary>
        ''' Holt die Größe eines Verzeichnisses sicher, ohne Fehler zu verursachen. Gibt 0 zurück, wenn ein Fehler
        ''' auftritt.
        ''' </summary>
        ''' <param name="directory">Das DirectoryInfo-Objekt des Verzeichnisses.</param>
        ''' <returns>Die Größe des Verzeichnisses in Bytes, oder 0 bei Fehler.</returns>
        Private Shared Function GetDirectorySizeSafe(directory As System.IO.DirectoryInfo) As System.Int64

            Try

                Return GetDirectorySize(directory)

            Catch

                ' Fehlerbehandlung - gebe 0 zurück
                Return 0

            End Try

        End Function

        ''' <summary>
        ''' Berechnet die Gesamtgröße eines Verzeichnisses rekursiv. Berücksichtigt alle Dateien und Unterverzeichnisse.
        ''' </summary>
        ''' <param name="directory">Das DirectoryInfo-Objekt des Verzeichnisses.</param>
        ''' <returns>Die Gesamtgröße in Bytes.</returns>
        Private Shared Function GetDirectorySize(directory As System.IO.DirectoryInfo) As System.Int64

            Dim total As System.Int64 = 0
            ' Hole alle Dateien im Verzeichnis
            Dim files As System.IO.FileInfo() = System.Array.Empty(Of System.IO.FileInfo)()

            Try
                files = directory.GetFiles()
            Catch
                ' Fehlerbehandlung - fehlerhafte Verzeichnisse überspringen
            End Try

            ' Addiere die Größe aller Dateien
            For Each file In files
                total += file.Length
            Next

            Return total

        End Function

        ''' <summary>
        ''' Gibt die Dateityp-Beschreibung für eine Datei zurück.
        ''' </summary>
        ''' <param name="file">Das FileInfo-Objekt der Datei.</param>
        ''' <returns>Eine Zeichenfolge mit der Dateityp-Beschreibung.</returns>
        Private Shared Function GetFileType(file As System.IO.FileInfo) As String

            ' Wenn keine Erweiterung vorhanden ist, gebe "Datei" zurück
            If String.IsNullOrWhiteSpace(file.Extension) Then
                Return "Datei"
            End If

            ' Gebe die Erweiterung in Großbuchstaben mit "-Datei" zurück
            Return file.Extension.ToUpperInvariant() & "-Datei"

        End Function

        ''' <summary>
        ''' Formatiert eine Byte-Größe in ein lesbares Format (B, KB, MB, GB, TB).
        ''' </summary>
        ''' <param name="bytes">Die Größe in Bytes.</param>
        ''' <returns>Die formatierte Größe als Zeichenfolge.</returns>
        Private Shared Function FormatSize(bytes As System.Int64) As String

            ' Definiere die Einheiten
            Dim units As String() = {"B", "KB", "MB", "GB", "TB"}
            Dim size As Double = bytes
            Dim unitIndex As System.Int32 = 0

            ' Konvertiere die Größe in die passende Einheit
            While size >= 1024 AndAlso unitIndex < units.Length - 1
                size /= 1024
                unitIndex += 1
            End While

            ' Gebe die formatierte Größe mit 2 Dezimalstellen zurück
            Return size.ToString("N2", Globalization.CultureInfo.CurrentCulture) & " " & units(unitIndex)

        End Function

        ''' <summary>
        ''' Gibt den Schlüssel einer Spalte basierend auf ihrem ColumnHeader-Objekt zurück.
        ''' </summary>
        ''' <param name="column">Das ColumnHeader-Objekt.</param>
        ''' <returns>Ein String-Schlüssel für die Spalte.</returns>
        Private Function GetColumnKey(column As ColumnHeader) As String

            ' Vergleiche das ColumnHeader-Objekt mit den bekannten Spalten
            Select Case True

                Case column Is Me.HeaderName
                    Return $"Name"

                Case column Is Me.HeaderType
                    Return "Type"

                Case column Is Me.HeaderSize
                    Return "Size"

                Case column Is Me.HeaderCreated
                    Return "Created"

                Case column Is Me.HeaderLastAccess
                    Return "LastAccess"

                Case column Is Me.HeaderLastWrite
                    Return "LastWrite"

                Case Else
                    Return String.Empty

            End Select

        End Function

        ''' <summary>
        ''' Gibt das ColumnHeader-Objekt für einen gegebenen Spaltenschlüssel zurück.
        ''' </summary>
        ''' <param name="key">Der Spaltenschlüssel (z.B. "Name", "Type").</param>
        ''' <returns>Das ColumnHeader-Objekt oder Nothing, wenn nicht gefunden.</returns>
        Private Function GetColumnByKey(key As String) As ColumnHeader

            ' Vergleiche den Schlüssel (ohne zusätzliche Normalisierung) mit den bekannten Spaltennamen.
            Select Case key.Trim()

                Case "Name"
                    Return Me.HeaderName

                Case "Type"
                    Return Me.HeaderType

                Case "Size"
                    Return Me.HeaderSize

                Case "Created"
                    Return Me.HeaderCreated

                Case "LastAccess"
                    Return Me.HeaderLastAccess

                Case "LastWrite"
                    Return Me.HeaderLastWrite

                Case Else
                    Return Nothing

            End Select

        End Function

#End Region

#Region "Ereignisbehandlungen"

        ''' <summary>
        ''' Behandelt das StartFolderChanged-Event und lädt die Einträge aus dem Startordner. Sortiert die Einträge und
        ''' passt die Spaltenbreiten an.
        ''' </summary>
        Private Sub FileList_StartFolderChanged(sender As Object, e As System.EventArgs) Handles Me.StartFolderChanged

            ' Prüfe, ob der Startordner gültig ist
            If String.IsNullOrWhiteSpace(Me._startFolder) OrElse Not System.IO.Directory.Exists(Me._startFolder) Then
                ' Leere UI auf UI-Thread
                If Me.listViewEntries.InvokeRequired Then
                    Me.listViewEntries.BeginInvoke(New MethodInvoker(Sub()
                                                                         Me.listViewEntries.Items.Clear()
                                                                     End Sub))
                Else
                    Me.listViewEntries.Items.Clear()
                End If
                Return
            End If

            Dim directoryInfo As New System.IO.DirectoryInfo(Me._startFolder)

            ' Enumeration im Hintergrund durchführen
            Dim unused = Task.Run(Sub()
                                      Dim directories As System.IO.DirectoryInfo() = System.Array.Empty(Of System.IO.DirectoryInfo)()
                                      Dim files As System.IO.FileInfo() = System.Array.Empty(Of System.IO.FileInfo)()
                                      Try
                                          directories = directoryInfo.GetDirectories().OrderBy(Function(d) d.Name, System.StringComparer.CurrentCultureIgnoreCase).ToArray()
                                      Catch
                                      End Try
                                      Try
                                          files = directoryInfo.GetFiles().OrderBy(Function(f) f.Name, System.StringComparer.CurrentCultureIgnoreCase).ToArray()
                                      Catch
                                      End Try

                                      ' Bereite die Einträge vor (leichte Struktur, um UI-Thread zu entlasten)
                                      Dim dirData = (From d In directories Select New With {d.Name, Key .Created = d.CreationTime, Key .LastAccess = d.LastAccessTime, Key .LastWrite = d.LastWriteTime, Key .Size = GetDirectorySizeSafe(d)}).ToArray()
                                      Dim fileData = (From f In files Select New With {f.Name, Key .Created = f.CreationTime, Key .LastAccess = f.LastAccessTime, Key .LastWrite = f.LastWriteTime, Key .Size = f.Length, Key .Type = GetFileType(f), Key .ImageKey = Me.GetFileImageKey(f)}).ToArray()

                                      ' Marshal zurück auf UI-Thread und aktualisiere ListView
                                      If Me.InvokeRequired Then
                                          Me.BeginInvoke(New MethodInvoker(Sub()
                                                                               Me.listViewEntries.BeginUpdate()
                                                                               Try
                                                                                   Me.listViewEntries.Items.Clear()
                                                                                   For Each d In dirData
                                                                                       Me.AddItem(d.Name, d.Created, d.LastAccess, d.LastWrite, FormatSize(d.Size), "Ordner", FolderImageKey)
                                                                                   Next
                                                                                   For Each f In fileData
                                                                                       Me.AddItem(f.Name, f.Created, f.LastAccess, f.LastWrite, FormatSize(f.Size), f.Type, f.ImageKey)
                                                                                   Next
                                                                                   Me.ApplyCurrentSort()
                                                                                   If Me._autoResizeColumnsEnabled Then Me.AdjustColumnWidths()
                                                                               Finally
                                                                                   Me.listViewEntries.EndUpdate()
                                                                               End Try
                                                                           End Sub))
                                      Else
                                          Me.listViewEntries.BeginUpdate()
                                          Try
                                              Me.listViewEntries.Items.Clear()
                                              For Each d In dirData
                                                  Me.AddItem(d.Name, d.Created, d.LastAccess, d.LastWrite, FormatSize(d.Size), "Ordner", FolderImageKey)
                                              Next
                                              For Each f In fileData
                                                  Me.AddItem(f.Name, f.Created, f.LastAccess, f.LastWrite, FormatSize(f.Size), f.Type, f.ImageKey)
                                              Next
                                              Me.ApplyCurrentSort()
                                              If Me._autoResizeColumnsEnabled Then Me.AdjustColumnWidths()
                                          Finally
                                              Me.listViewEntries.EndUpdate()
                                          End Try
                                      End If
                                  End Sub)

        End Sub

        ''' <summary>
        ''' Behandelt das ColumnClick-Event zum Sortieren nach Spalten. Wechselt zwischen aufsteigender und absteigender
        ''' Sortierung bei wiederholtem Klick.
        ''' </summary>
        Private Sub ListViewEntries_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles listViewEntries.ColumnClick

            ' Prüfe, ob auf dieselbe Spalte geklickt wurde
            If e.Column = Me._sortColumn Then

                ' Wechsle die Sortierreihenfolge
                Me._sortOrder = If(Me._sortOrder = System.Windows.Forms.SortOrder.Ascending, SortOrder.Descending, SortOrder.Ascending)

            Else

                ' Neue Spalte - setze auf aufsteigend
                Me._sortColumn = e.Column
                Me._sortOrder = SortOrder.Ascending

            End If

            ' Wende die neue Sortierung an
            Me.ApplyCurrentSort()

        End Sub

        ''' <summary>
        ''' Behandelt das HandleCreated-Event und wendet die ausstehende Spaltenreihenfolge an.
        ''' </summary>
        Private Sub ListViewEntries_HandleCreated(sender As Object, e As System.EventArgs) Handles listViewEntries.HandleCreated

            ' Dieser Schritt ist nötig, falls die Reihenfolge vor der Handle-Erzeugung gesetzt wurde.
            Me.ApplyColumnOrderState(Me._pendingColumnOrder)

        End Sub

        ''' <summary>
        ''' Behandelt das ColumnReordered-Event und speichert die neue Spaltenreihenfolge.
        ''' </summary>
        Private Sub ListViewEntries_ColumnReordered(sender As Object, e As ColumnReorderedEventArgs) Handles listViewEntries.ColumnReordered

            ' Speichere die neue Spaltenreihenfolge sofort
            Me.SaveColumnOrderToSettings()

        End Sub

#End Region

    End Class

End Namespace

