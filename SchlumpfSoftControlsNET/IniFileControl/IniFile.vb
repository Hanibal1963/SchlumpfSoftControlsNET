' --------------------------------------------------------------------------------------------------------
' Datei: IniFile.vb
' Author: Andreas Sauer
' Datum: 29.05.2026
' --------------------------------------------------------------------------------------------------------

Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Collections.Generic
Imports System.IO

Namespace IniFileControl

    ''' <summary>
    ''' Stellt Funktionen zum Laden, Analysieren, Bearbeiten und Speichern von INI-Dateien bereit.
    ''' </summary>
    ''' <remarks>
    ''' Die Komponente hält den Inhalt intern in einer Abschnitts-/Eintragsstruktur und erzeugt daraus bei Bedarf den
    ''' serialisierten Dateiinhalt. Änderungen an Abschnitten, Einträgen und Kommentaren werden über
    ''' <see cref="FileContentChanged"/> signalisiert.
    ''' </remarks>
    <Description("Ein Set von Controls zum Verwalten und bearbeiten von INI - Dateien.")>
    <ToolboxBitmap(GetType(IniFile), "IniFileControl.IniFile.bmp")>
    <ToolboxItem(True)>
    Public Class IniFile : Inherits Component

#Region "Variablen"

        Private _FileContent() As String = {$""} ' Aktueller Dateiinhalt als Zeilenpuffer (so, wie er gespeichert/geladen wird)
        Private _FileComment As New List(Of String) ' Kommentarzeilen am Anfang der Datei (ohne Prefixzeichen)
        Private _Sections As New Dictionary(Of String, Dictionary(Of String, String)) ' Abschnitte mit Einträgen: Abschnittsname -> (Eintragsname -> Wert)
        Private _SectionsComments As New Dictionary(Of String, List(Of String)) ' Abschnittskommentare: Abschnittsname -> Liste der Kommentarzeilen (ohne Prefix)
        Private _CurrentSectionName As String = $"" ' Name des Abschnitts, der beim Parsen gerade verarbeitet wird (Parserzustand)
        Private _FileSaved As Boolean  ' Status, ob der aktuelle Zustand auf Datenträger gespeichert ist

        Public Sub New()
            'Me.InitializeComponent()
            Me._FileSaved = False
        End Sub

#End Region

#Region "Ereignisse"

        ''' <summary>
        ''' Wird ausgelöst, wenn sich der Dateiinhalt geändert hat.
        ''' </summary>
        ''' <remarks>
        ''' Dieses Ereignis wird nach jeder Änderung an der internen Struktur (Add/Rename/Delete/Set) ausgelöst,
        ''' unabhängig davon, ob <see cref="AutoSave"/> aktiv ist.
        ''' </remarks>
        <Description("Wird ausgelöst wenn sich der Dateiinhalt geändert hat.")>
        Public Event FileContentChanged(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn beim Anlegen oder Umbenennen eines Abschnitts der Name bereits vorhanden ist.
        ''' </summary>
        <Description("Wird ausgelöst wenn beim anlegen eines neuen Abschnitts oder umbnennen eines Abschnitts der Name bereits vorhanden ist.")>
        Public Event SectionNameExist(sender As Object, e As EventArgs)

        ''' <summary>
        ''' Wird ausgelöst, wenn beim Anlegen oder Umbenennen eines Eintrags der Zielname bereits existiert.
        ''' </summary>
        <Description("Wird ausgelöst wenn beim anlegen eines neuen Eintrags oder umbenennen eines Eintrags der Name bereits vorhanden ist.")>
        Public Event EntryNameExist(sender As Object, e As EventArgs)

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Gibt an, ob der aktuelle Zustand bereits gespeichert wurde.
        ''' </summary>
        ''' <remarks>
        ''' True bedeutet, dass der aktuelle Zustand auf Datenträger geschrieben wurde (entweder durch expliziten Aufruf
        ''' von <see cref="SaveFile()"/> oder automatisch, wenn <see cref="AutoSave"/> = True).
        ''' </remarks>
        <Browsable(False)>
        Public ReadOnly Property FileSaved As Boolean
            Get
                Return Me._FileSaved
            End Get
        End Property

        ''' <summary>
        ''' Gibt das Prefixzeichen für Kommentare zurück oder legt es fest.
        ''' </summary>
        ''' <remarks>
        ''' Wird beim Erzeugen/Analysieren der Datei verwendet.<br/> Änderungen wirken sich auf die Ausgabe in <c>
        ''' CreateFileContent</c> aus.<br/> Beim Parsen wird das jeweils aktuell gesetzte Prefix zur Erkennung von
        ''' Kommentarzeilen herangezogen.
        ''' </remarks>
        <Browsable(True)>
        <Category("Design")>
        <Description("Gibt das Prefixzeichen für Kommentare zurück oder legt dieses fest.")>
        <DefaultValue("';'")>
        Public Property CommentPrefix As Char = ";"c ' Prefixzeichen für Kommentarzeilen (typisch ';', alternativ denkbar '#')

        ''' <summary>
        ''' Gibt den aktuellen Dateinamen zurück oder legt ihn fest.
        ''' </summary>
        ''' <remarks>
        ''' Der Name wird beim Speichern/Laden mit <see cref="FilePath"/> kombiniert.
        ''' </remarks>
        <Browsable(True)>
        <Category("Design")>
        <Description("Gibt den aktuellen Dateiname zurück oder legt diesen fest")>
        <DefaultValue("neue Datei.ini")>
        Public Property FileName As String = $"neue Datei.ini" ' Name der Datei (nur der Dateiname, ohne Pfad)

        ''' <summary>
        ''' Gibt den Pfad zur INI-Datei zurück oder legt diesen fest.
        ''' </summary>
        ''' <remarks>
        ''' Beim Speichern/Laden wird der Pfad mit dem Dateinamen kombiniert.
        ''' </remarks>
        <Browsable(True)>
        <Category("Design")>
        <Description("Gibt den Pfad zur INI-Datei zurück oder legt diesen fest.")>
        <DefaultValue("")>
        Public Property FilePath As String = String.Empty ' Verzeichnis, in dem die INI-Datei liegt (ohne Dateiname)

        ''' <summary>
        ''' Legt das Speicherverhalten der Komponente fest.
        ''' </summary>
        ''' <remarks>
        ''' True legt fest, dass Änderungen automatisch gespeichert werden.<br/> Bei False bleibt der Status
        ''' ungespeichert, bis <see cref="SaveFile()"/> explizit aufgerufen wird.
        ''' </remarks>
        <Browsable(True)>
        <Category("Design")>
        <Description("Legt das Speicherverhalten der Klasse fest.")>
        <DefaultValue(False)>
        Public Property AutoSave As Boolean = False ' Wenn True, werden Änderungen an den internen Strukturen automatisch auf die Datei geschrieben

#End Region

#Region "Öffentliche Methoden"

        ''' <summary>
        ''' Erzeugt eine neue INI-Datei mit Beispielinhalt und verwendet das Standard-Präfix für Kommentare.
        ''' </summary>
        ''' <remarks>
        ''' Diese Überladung delegiert an <see cref="CreateNewFile(Char)"/> und übergibt den Standardwert.
        ''' </remarks>
        Public Sub CreateNewFile()
            Me.CreateNewFile(Nothing)
        End Sub

        ''' <summary>
        ''' Erzeugt eine neue INI-Datei mit Beispielinhalt.
        ''' </summary>
        ''' <remarks>
        ''' Wenn kein Präfixzeichen angegeben wird, wird standardmäßig ein Semikolon verwendet.<br/> Nach dem Erzeugen
        ''' wird der Inhalt direkt geparst, sodass alle internen Datenstrukturen konsistent gefüllt sind.
        ''' </remarks>
        ''' <param name="CommentPrefix">Präfixzeichen für Kommentare oder <c>Nothing</c> für den Standardwert.</param>
        Public Sub CreateNewFile(CommentPrefix As Char)

            Me.CommentPrefix = If(CommentPrefix = Nothing, ";"c, CommentPrefix)

            ' Beispiel-Inhalt als zusammenhängenden Rohtext aufbauen.
            Dim content As String =
                $"{Me.CommentPrefix} INI - Datei Beispiel{vbCrLf}" &
                $"{Me.CommentPrefix} Diese Datei wurde von {Application.ProductName} erzeugt{vbCrLf}{vbCrLf}" &
                $"[Allgemein]{vbCrLf}" &
                $"{Me.CommentPrefix} Anwendungsname und Version{vbCrLf}" &
                $"AppName = MeineApp{vbCrLf}" &
                $"Version = 1.0.0{vbCrLf}{vbCrLf}" &
                $"[Datenbank]{vbCrLf}" &
                $"{Me.CommentPrefix} Einstellungen zur Datenbank{vbCrLf}" &
                $"Server = localhost{vbCrLf}" &
                $"Port = 3306{vbCrLf}" &
                $"Benutzername = admin{vbCrLf}" &
                $"Passwort = geheim{vbCrLf}{vbCrLf}" &
                $"[Logging]{vbCrLf}" &
                $"{Me.CommentPrefix} Einstellungen zum Logging{vbCrLf}" &
                $"LogLevel = Debug{vbCrLf}" &
                $"LogDatei = logs / app.log{vbCrLf}"

            ' Zeilenpuffer erzeugen und direkt in die internen Strukturen einlesen.
            Me._FileContent = content.Split({vbCrLf}, StringSplitOptions.None)
            Me.ParseFileContent()

            ' Neue Datei wurde erzeugt, aber noch nicht persistiert.
            Me._FileSaved = False

            RaiseEvent FileContentChanged(Me, EventArgs.Empty)

        End Sub

        ''' <summary>
        ''' Lädt die angegebene INI-Datei über einen vollständigen Pfad.
        ''' </summary>
        ''' <remarks>
        ''' Diese Überladung zerlegt den übergebenen Pfad in Verzeichnis und Dateiname und delegiert an
        ''' <see cref="LoadFile()"/>.
        ''' </remarks>
        ''' <param name="FilePathAndName">
        ''' Name und Pfad der Datei die geladen werden soll.
        ''' </param>
        Public Sub LoadFile(FilePathAndName As String)

            If String.IsNullOrWhiteSpace(FilePathAndName) Then
                Throw New ArgumentException("Der Parameter FilePathAndName darf nicht NULL oder ein Leerraumzeichen sein.", NameOf(FilePathAndName))
            End If

            ' Eingabepfad in die von der Komponente verwendeten Design-Eigenschaften aufteilen.
            Me.FilePath = Path.GetDirectoryName(FilePathAndName)
            Me.FileName = Path.GetFileName(FilePathAndName)
            Me.LoadFile()

        End Sub

        ''' <summary>
        ''' Lädt die Datei, die in <see cref="FilePath"/> und <see cref="FileName"/> angegeben wurde.
        ''' </summary>
        ''' <remarks>
        ''' Liest alle Zeilen, parst sie in die internen Strukturen, markiert den Zustand als gespeichert und löst das
        ''' FileContentChanged-Ereignis aus.
        ''' </remarks>
        Public Sub LoadFile()

            ' Vollständigen Dateipfad aus Pfad- und Dateiname zusammensetzen.
            Dim filepathandname As String = Path.Combine(Me.FilePath, Me.FileName)

            Try
                ' Datei vollständig lesen und in die Arbeitsstruktur übernehmen.
                Me._FileContent = File.ReadAllLines(filepathandname)
                Me.ParseFileContent()

                ' Erfolgreich geladen bedeutet: aktueller Zustand entspricht Datenträgerstand.
                Me._FileSaved = True
                RaiseEvent FileContentChanged(Me, EventArgs.Empty)
            Catch ex As IOException
                ' Aufrufer erhält eine domänenspezifische IO-Fehlermeldung.
                Throw New IOException($"Fehler beim laden der Datei {filepathandname}.")
            End Try

        End Sub

        ''' <summary>
        ''' Speichert den aktuellen Inhalt in die angegebene INI-Datei.
        ''' </summary>
        ''' <remarks>
        ''' Setzt Pfad und Dateiname und ruft SaveFile() ohne Parameter auf.
        ''' </remarks>
        ''' <param name="FilePathAndName">
        ''' Name und Pfad der Datei die gespeichert werden soll.
        ''' </param>
        Public Sub SaveFileAs(FilePathAndName As String)

            If String.IsNullOrWhiteSpace(FilePathAndName) Then
                Throw New ArgumentException("Der Parameter FilePathAndName darf nicht NULL oder ein Leerraumzeichen sein.", NameOf(FilePathAndName))
            End If

            ' Zielpfad in die Komponenten-Eigenschaften übernehmen.
            Me.FilePath = Path.GetDirectoryName(FilePathAndName)
            Me.FileName = Path.GetFileName(FilePathAndName)
            Me.SaveFile()

        End Sub

        ''' <summary>
        ''' Speichert den aktuellen Inhalt in die INI-Datei, die durch <see cref="FilePath"/> und <see cref="FileName"/>
        ''' definiert ist.
        ''' </summary>
        ''' <remarks>
        ''' Schreibt den aktuellen Zeilenpuffer in die Datei, markiert den Zustand als gespeichert und löst das
        ''' FileContentChanged-Ereignis aus.
        ''' </remarks>
        Public Sub SaveFile()

            ' Vollständigen Zielpfad bilden und aktuellen Zeilenpuffer schreiben.
            Dim filepathandname As String = Path.Combine(Me.FilePath, Me.FileName)
            File.WriteAllLines(filepathandname, Me._FileContent)

            ' Nach erfolgreichem Schreiben ist der Zustand synchron zum Datenträger.
            Me._FileSaved = True
            RaiseEvent FileContentChanged(Me, EventArgs.Empty)

        End Sub

        ''' <summary>
        ''' Gibt den aktuell erzeugten Dateiinhalt als Zeilenarray zurück.
        ''' </summary>
        ''' <remarks>
        ''' Dies ist der aktuelle, generierte Rohinhalt (Zeilen), so wie er gespeichert werden würde.
        ''' </remarks>
        Public Function GetFileContent() As String()

            Return Me._FileContent

        End Function

        ''' <summary>
        ''' Gibt die Kommentarzeilen im Dateikopf zurück.
        ''' </summary>
        ''' <remarks>
        ''' Die Kommentarzeilen werden ohne Prefixzeichen zurückgegeben. Beim Erzeugen des Datei-Inhalts wird das Prefix
        ''' automatisch vorangestellt.
        ''' </remarks>
        Public Function GetFileComment() As String()

            Return Me._FileComment.ToArray

        End Function

        ''' <summary>
        ''' Ersetzt den Dateikopf-Kommentar vollständig.
        ''' </summary>
        ''' <remarks>
        ''' Die übergebenen Zeilen sollten keine Prefixzeichen enthalten. Nach dem Setzen wird der Dateiinhalt neu
        ''' aufgebaut (und ggf. gespeichert, wenn AutoSave=True).
        ''' </remarks>
        ''' <param name="CommentLines">Die Zeilen des Dateikommentars.</param>
        Public Sub SetFileComment(CommentLines() As String)

            ' Vorhandene Kopfkommentare verwerfen und neue Zeilen übernehmen.
            Me._FileComment.Clear()
            Me._FileComment.AddRange(CommentLines)

            ' Serialisierten Inhalt und Status aktualisieren.
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Ruft alle Abschnittsnamen in der aktuellen INI-Struktur ab.
        ''' </summary>
        ''' <returns>Array mit Abschnittsnamen.</returns>
        Public Function GetSectionNames() As String()

            ' Keys der Abschnittsverwaltung in ein stabiles Array kopieren.
            Dim names As New List(Of String)
            For Each name As String In Me._Sections.Keys
                names.Add(name)
            Next
            Return names.ToArray

        End Function

        ''' <summary>
        ''' Gibt alle Eintragsnamen eines Abschnitts zurück.
        ''' </summary>
        ''' <param name="SectionName">Abschnittsname</param>
        ''' <returns>
        ''' Eintragsliste oder Nothing falls <paramref name="SectionName"/> nicht existiert.
        ''' </returns>
        Public Function GetEntryNames(SectionName As String) As String()

            ' Standard: Abschnitt nicht vorhanden -> Nothing.
            Dim result() As String = Nothing
            Dim sectionEntries As Dictionary(Of String, String) = Nothing

            If Me._Sections.TryGetValue(SectionName, sectionEntries) Then

                ' Eintragsnamen in ein neues Array materialisieren.
                Dim names As New List(Of String)

                For Each name As String In sectionEntries.Keys
                    names.Add(name)
                Next

                result = names.ToArray

            End If

            Return result

        End Function

        ''' <summary>
        ''' Fügt einen neuen Abschnitt hinzu.
        ''' </summary>
        ''' <remarks>
        ''' Löst SectionNameExist aus und bricht ab, wenn der Abschnitt bereits existiert.
        ''' </remarks>
        ''' <param name="Name">Name des neuen Abschnitts</param>
        Public Sub AddSection(Name As String)

            ' Doppelte Abschnittsnamen sind nicht erlaubt.
            If Me._Sections.ContainsKey(Name) Then
                RaiseEvent SectionNameExist(Me, EventArgs.Empty)
                Exit Sub
            End If

            ' Abschnitt anlegen und Dateiabbild neu erzeugen.
            Me.AddNewSection(Name)
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Fügt einen neuen Eintrag in einem Abschnitt hinzu.
        ''' </summary>
        ''' <remarks>
        ''' Der Abschnitt muss existieren, andernfalls kommt es zu einer Ausnahme.<br/> Bei Namenskonflikt wird
        ''' EntryNameExist ausgelöst und abgebrochen.
        ''' </remarks>
        ''' <param name="Section">
        ''' Abschnitt in den der Eintrag eingefügt werden soll.
        ''' </param>
        ''' <param name="Name">Name des Eintrags.</param>
        Public Sub AddEntry(Section As String, Name As String)

            ' Doppelte Eintragsnamen innerhalb des Abschnitts verhindern.
            If Me._Sections.Item(Section).ContainsKey(Name) Then
                RaiseEvent EntryNameExist(Me, EventArgs.Empty)
                Exit Sub
            End If

            ' Eintrag anlegen und Dateiabbild neu erzeugen.
            Me.AddNewEntry(Section, Name)
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Benennt einen Abschnitt um.
        ''' </summary>
        ''' <remarks>
        ''' Es werden sowohl der Abschnitt (Werte) als auch sein Kommentar umgehängt.<br/> Bei Namenskonflikt wird
        ''' SectionNameExist ausgelöst.
        ''' </remarks>
        ''' <param name="OldName">Alter Name des Abschnitts.</param>
        ''' <param name="NewName">Neuer Name des Abschnitts.</param>
        Public Sub RenameSection(OldName As String, NewName As String)

            ' Zielname darf nicht bereits existieren.
            If Me._Sections.ContainsKey(NewName) Then
                RaiseEvent SectionNameExist(Me, EventArgs.Empty)
                Exit Sub
            End If

            ' Abschnittsinhalt und Abschnittskommentar unter neuem Namen ablegen.
            Me.RenameSectionValue(OldName, NewName)
            Me.RenameSectionComment(OldName, NewName)
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Benennt einen Eintrag in einem Abschnitt um.
        ''' </summary>
        ''' <remarks>
        ''' Der Abschnitt muss existieren.<br/> Bei Namenskonflikt wird EntryNameExist ausgelöst.
        ''' </remarks>
        ''' <param name="Section">Abschnitt der den Eintrag enthält.</param>
        ''' <param name="Oldname">Alter Name des Eintrags.</param>
        ''' <param name="NewName">Neuer Name des Eintrags.</param>
        Public Sub RenameEntry(Section As String, Oldname As String, NewName As String)

            ' Zielname darf im Abschnitt noch nicht belegt sein.
            If Me._Sections.Item(Section).ContainsKey(NewName) Then
                RaiseEvent EntryNameExist(Me, EventArgs.Empty)
                Exit Sub
            End If

            ' Eintragsname ändern und Dateiabbild neu erzeugen.
            Me.RenameEntryvalue(Section, Oldname, NewName)
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Löscht einen Abschnitt samt zugehörigem Abschnittskommentar.
        ''' </summary>
        ''' <remarks>
        ''' Entfernt auch den dazugehörigen Abschnittskommentar.
        ''' </remarks>
        ''' <param name="Name">Name des Abschnittes</param>
        Public Sub DeleteSection(Name As String)

            ' Abschnittsdaten und Kommentarcontainer unabhängig voneinander entfernen.
            Dim unused = Me._Sections.Remove(Name)
            Dim unused1 = Me._SectionsComments.Remove(Name)

            ' Persistierbares Dateiabbild aktualisieren.
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Löscht einen Eintrag aus einem Abschnitt.
        ''' </summary>
        ''' <param name="Section">
        ''' Abschnitt aus dem der Eintrag gelöscht werden soll.
        ''' </param>
        ''' <param name="Entry">Eintrag der gelöscht werden soll.</param>
        Public Sub DeleteEntry(Section As String, Entry As String)

            ' Eintrag aus dem Zielabschnitt entfernen.
            Dim unused = Me._Sections.Item(Section).Remove(Entry)

            ' Persistierbares Dateiabbild aktualisieren.
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Gibt die Kommentarzeilen für einen Abschnitt zurück.
        ''' </summary>
        ''' <param name="SectionName">Name des Abschnitts</param>
        ''' <returns>
        ''' Kommentar für <paramref name="SectionName"/> oder Nothing wenn kein Kommentar existiert.
        ''' </returns>
        Public Function GetSectionComment(SectionName As String) As String()

            ' Standard: kein Kommentar vorhanden.
            Dim result() As String = Nothing
            Dim sectionComments As List(Of String) = Nothing

            If Me._SectionsComments.TryGetValue(SectionName, sectionComments) Then
                result = sectionComments.ToArray
            End If

            Return result

        End Function

        ''' <summary>
        ''' Gibt den Wert eines Eintrags aus einem Abschnitt zurück.
        ''' </summary>
        ''' <remarks>
        ''' Erwartet, dass Abschnitt und Eintrag existieren.<br/> Andernfalls kann eine Ausnahme geworfen werden.<br/>
        ''' Bei leerem Abschnitts- und Eintragsnamen wird ein leerer String zurückgegeben.
        ''' </remarks>
        ''' <param name="Section">
        ''' Abschnitt aus dem der Wert des Eintrags gelesen werden soll.
        ''' </param>
        ''' <param name="Entry">Eintrag dessen Wert gelesen werden soll.</param>
        ''' <returns>
        ''' Wert des Eintrags.
        ''' </returns>
        Public Function GetEntryValue(Section As String, Entry As String) As String

            ' Sonderfall: ungesetzte Auswahl im Host liefert bewusst einen leeren String.
            Dim result = If(
                String.IsNullOrEmpty(Section) AndAlso String.IsNullOrEmpty(Entry),
                String.Empty,
                Me._Sections.Item(Section).Item(Entry))
            Return result

        End Function

        ''' <summary>
        ''' Ersetzt den Kommentar eines Abschnitts vollständig.
        ''' </summary>
        ''' <remarks>
        ''' Die übergebenen Zeilen sollten ohne Prefixzeichen sein.
        ''' </remarks>
        ''' <param name="Name">Name des Abschnitts.</param>
        ''' <param name="CommentLines">Kommentarzeilen</param>
        Public Sub SetSectionComment(Name As String, CommentLines() As String)

            ' Ohne Abschnittsname ist keine Zuordnung der Kommentarzeilen möglich.
            If String.IsNullOrEmpty(Name) Then
                Dim unused = MessageBox.Show(
                    $"Es wurde kein Abschnitt ausgewählt!",
                    $"Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Exit Sub
            End If

            ' Vorhandene Kommentarzeilen ersetzen.
            Me._SectionsComments.Item(Name).Clear()
            Me._SectionsComments.Item(Name).AddRange(CommentLines)

            ' Persistierbares Dateiabbild aktualisieren.
            Me.ChangeFileContent()

        End Sub

        ''' <summary>
        ''' Setzt den Wert eines Eintrags in einem Abschnitt.
        ''' </summary>
        ''' <remarks>
        ''' Der Abschnitt und der Eintrag müssen existieren.
        ''' </remarks>
        ''' <param name="Section">
        ''' Abschnitt in dem der Wert eines Eintrags geändert werden soll.
        ''' </param>
        ''' <param name="Entry">Eintrag dessen Wert geändert werden soll.</param>
        ''' <param name="Value">Der geänderte Wert.</param>
        Public Sub SetEntryValue(Section As String, Entry As String, Value As String)

            ' Ohne Abschnitt kann kein Eintrag adressiert werden.
            If String.IsNullOrEmpty(Section) Then
                Dim unused = MessageBox.Show(
                    $"Es wurde kein Eintrag ausgewählt!",
                    $"Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Exit Sub
            End If

            ' Neuen Wert schreiben und Dateiabbild aktualisieren.
            Me._Sections.Item(Section).Item(Entry) = Value

            Me.ChangeFileContent()

        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Erzeugt den aktuellen Dateiinhalt aus der internen Struktur und speichert ihn ggf. auf Datenträger.
        ''' </summary>
        Private Sub ChangeFileContent()

            ' Aus der internen Struktur immer zuerst den aktuellen Zeilenpuffer erzeugen.
            Me.CreateFileContent()

            ' Abhängig von AutoSave sofort schreiben oder als "ungespeichert" markieren.
            If Me.AutoSave Then
                Me.SaveFile()
            Else
                Me._FileSaved = False
                ' Auch ohne automatisches Speichern müssen Änderungen an den Host gemeldet werden.
                RaiseEvent FileContentChanged(Me, EventArgs.Empty)
            End If

        End Sub

        ''' <summary>
        ''' Initialisiert die Parserzustandsvariablen vor jeder Analyse.
        ''' </summary>
        ''' <param name="Name"></param>
        Private Sub AddNewSection(Name As String)

            ' Abschnittscontainer für Werte und Kommentare parallel anlegen.
            Me._Sections.Add(Name, New Dictionary(Of String, String))
            Me._SectionsComments.Add(Name, New List(Of String))

        End Sub

        ''' <summary>
        ''' Fügt einen neuen Eintrag in einem Abschnitt hinzu.
        ''' </summary>
        ''' <param name="Section"></param>
        ''' <param name="Name"></param>
        Private Sub AddNewEntry(Section As String, Name As String)

            ' Neue Einträge werden mit leerem Wert initialisiert.
            Me._Sections.Item(Section).Add(Name, $"")

        End Sub

        ''' <summary>
        ''' Ändert den Schlüssel eines Abschnittskommentars.
        ''' </summary>
        ''' <param name="OldName"></param>
        ''' <param name="newName"></param>
        Private Sub RenameSectionComment(OldName As String, newName As String)

            ' Kommentar-Liste vom alten Namen lösen und unter neuem Namen registrieren.
            Dim oldcomment = Me._SectionsComments.Item(OldName)
            Dim unused1 = Me._SectionsComments.Remove(OldName)
            Me._SectionsComments.Add(newName, oldcomment)

        End Sub

        ''' <summary>
        ''' Ändert den Schlüssel eines Abschnittswerts.
        ''' </summary>
        ''' <param name="OldName"></param>
        ''' <param name="NewName"></param>
        Private Sub RenameSectionValue(OldName As String, NewName As String)

            ' Abschnittswerte vom alten Namen lösen und unter neuem Namen registrieren.
            Dim oldvalue = Me._Sections.Item(OldName)
            Dim unused = Me._Sections.Remove(OldName)
            Me._Sections.Add(NewName, oldvalue)

        End Sub

        Private Sub RenameEntryvalue(Section As String, OldName As String, NewName As String)

            ' Bestehenden Wert merken, alten Schlüssel entfernen, neuen Schlüssel mit altem Wert anlegen.
            Dim oldvalue = Me._Sections.Item(Section).Item(OldName)
            Dim unused = Me._Sections.Item(Section).Remove(OldName)
            Me._Sections.Item(Section).Add(NewName, oldvalue)

        End Sub

        ''' <summary>
        ''' Erzeugt den aktuellen Dateiinhalt aus der internen Abschnitts-/Eintragsstruktur.
        ''' </summary>
        Private Sub CreateFileContent()

            ' Zeilenliste als serialisierbares Abbild der internen Struktur aufbauen.
            Dim filecontent As New List(Of String)

            ' Dateikommentare am Anfang der Datei ausgeben.
            For Each line As String In Me._FileComment
                filecontent.Add(Me.CommentPrefix & $" " & line)
            Next

            ' Leerzeile zwischen Dateikopf und erstem Abschnitt.
            filecontent.Add($"")

            ' Alle Abschnitte mit Kommentaren und Einträgen in Datei-Reihenfolge schreiben.
            For Each sectionname As String In Me._Sections.Keys

                filecontent.Add($"[" & sectionname & $"]")

                For Each commentline As String In Me._SectionsComments.Item(sectionname) ' Zeilen des Abschnittskommentars durchlaufen
                    filecontent.Add(Me.CommentPrefix & $" " & commentline) ' Kommentarzeile einfügen
                Next

                Dim entryline As String

                For Each entryname As String In Me._Sections.Item(sectionname).Keys ' alle Eintragszeilen durchlaufen
                    entryline = entryname & $" = " & Me._Sections.Item(sectionname).Item(entryname)  ' Eintragszeile erzeugen und einfügen
                    filecontent.Add(entryline)
                Next

                ' Leerzeile trennt Abschnitte voneinander.
                filecontent.Add($"")

            Next

            Me._FileContent = filecontent.ToArray ' Dateiinhalt erzeugen

        End Sub

        ''' <summary>
        ''' Analysiert den aktuellen Dateiinhalt zeilenweise und überführt ihn in die interne
        ''' Abschnitts-/Eintragsstruktur.
        ''' </summary>
        Private Sub ParseFileContent()

            ' Parserzustand vor jeder Analyse komplett zurücksetzen.
            Me.InitParseVariables()
            Me._CurrentSectionName = $""

            ' Zeilenweise analysieren und in die interne Struktur überführen.
            For Each line As String In Me._FileContent
                line = line.Trim
                Me.LineAnalyse(line) ' aktuelle Zeile analysieren
            Next

        End Sub

        ''' <summary>
        ''' Analysiert eine einzelne Zeile und überführt sie in die interne Abschnitts-/Eintragsstruktur.
        ''' </summary>
        ''' <param name="LineContent"></param>
        Private Sub LineAnalyse(LineContent As String)

            ' Je nach Parserzustand und Präfix den Zeilentyp bestimmen.
            If String.IsNullOrEmpty(Me._CurrentSectionName) AndAlso LineContent.Length > 0 AndAlso LineContent.Chars(0) = Me.CommentPrefix Then
                Me.AddFileCommentLine(LineContent)
            ElseIf LineContent.Length >= 2 AndAlso LineContent.Chars(0) = "["c AndAlso LineContent.Chars(LineContent.Length - 1) = "]"c Then
                Me.AddSectionNameLine(LineContent)
            ElseIf (Not String.IsNullOrEmpty(Me._CurrentSectionName)) AndAlso LineContent.Length > 0 AndAlso LineContent.Chars(0) = Me.CommentPrefix Then
                Me.AddSectionCommentLine(LineContent)
            ElseIf (Not String.IsNullOrEmpty(Me._CurrentSectionName)) And LineContent.Contains("="c) Then
                Me.AddEntryLine(LineContent)
            End If

        End Sub

        ''' <summary>
        ''' Erkennt eine Eintragszeile im Format "Name = Wert" und fügt sie dem aktuellen Abschnitt hinzu.
        ''' </summary>
        ''' <param name="LineContent"></param>
        Private Sub AddEntryLine(LineContent As String)

            ' Zeile im Format "Name = Wert" aufteilen und in den aktuellen Abschnitt schreiben.
            Dim separatorIndex As Integer = LineContent.IndexOf("="c)
            Dim name As String = LineContent.Substring(0, separatorIndex).Trim
            Dim value As String = LineContent.Substring(separatorIndex + 1).Trim
            Me._Sections.Item(Me._CurrentSectionName).Add(name, value)

        End Sub

        ''' <summary>
        ''' Erkennt eine Kommentarzeile im aktuellen Abschnitt und fügt sie der Abschnittskommentarliste hinzu.
        ''' </summary>
        ''' <param name="LineContent"></param>
        Private Sub AddSectionCommentLine(LineContent As String)

            ' Präfix entfernen und Kommentarzeile dem aktuellen Abschnitt zuordnen.
            Dim line As String = LineContent.Substring(1).Trim
            Me._SectionsComments.Item(Me._CurrentSectionName).Add(line)

        End Sub

        ''' <summary>
        ''' Erkennt eine Abschnittszeile im Format "[Name]" und legt einen neuen Abschnitt an.
        ''' </summary>
        ''' <param name="LineContent"></param>
        Private Sub AddSectionNameLine(LineContent As String)

            ' Abschnittsnamen aus "[Name]" extrahieren.
            Dim line = LineContent.Substring(1, LineContent.Length - 2).Trim

            ' Neue Zielcontainer für den erkannten Abschnitt anlegen.
            Me._CurrentSectionName = line
            Me._Sections.Add(Me._CurrentSectionName, New Dictionary(Of String, String))
            Me._SectionsComments.Add(Me._CurrentSectionName, New List(Of String))

        End Sub

        ''' <summary>
        ''' Erkennt eine Kommentarzeile im Dateikopf und fügt sie der Kopfkommentarliste hinzu.
        ''' </summary>
        ''' <param name="LineContent"></param>
        Private Sub AddFileCommentLine(LineContent As String)

            ' Präfix entfernen und als Dateikopf-Kommentarzeile speichern.
            Dim line = LineContent.Substring(1).Trim
            Me._FileComment.Add(line)

        End Sub

        ''' <summary>
        ''' Initialisiert die Parserzustandsvariablen vor jeder Analyse.
        ''' </summary>
        Private Sub InitParseVariables()

            ' Alle Parser-Zielcontainer leeren, damit keine Altzustände erhalten bleiben.
            Me._FileComment = New List(Of String)
            Me._Sections = New Dictionary(Of String, Dictionary(Of String, String))
            Me._SectionsComments = New Dictionary(Of String, List(Of String))

        End Sub

        'Private Sub InitializeComponent()
        'End Sub

#End Region

    End Class

End Namespace