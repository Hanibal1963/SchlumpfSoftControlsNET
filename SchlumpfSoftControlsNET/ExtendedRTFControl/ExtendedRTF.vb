' --------------------------------------------------------------------------------------------------------
' Datei: ExtendedRTF.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Imports System
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text

Namespace ExtendedRTFControl

    ''' <summary>
    ''' Erweiterte <see cref="RichTextBox"/> mit bequemen Formatierungs- und Abfrage-Hilfen für<br/> Auswahl und Caret
    ''' (u.a. Schriftgröße, Stil-Flags, Vorder-/Hintergrundfarbe, Absatz-Einzüge, Ausrichtung)<br/> sowie Batch-Updates
    ''' über Redraw-Suppression.
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet"><item><description>Redraw-Suppression zur Verringerung von Flackern und zur
    ''' Leistungssteigerung via <c>WM_SETREDRAW</c> (siehe <see cref="BeginUpdate"/> /<see
    ''' cref="EndUpdate"/>).</description> </item> </list> <list type="bullet"><item><description>Mischzustände in der
    ''' Auswahl werden (soweit implementiert) als <c>Nothing</c> gemeldet (z.B. bei Stil-Flags, Schriftgröße,
    ''' Einzug).<br/> Farben melden derzeit keinen Mischzustand.</description> </item> </list> <list type="bullet">
    ''' <item><description>Mischzustände werden ohne temporäre Änderungen der Auswahl ermittelt.</description> </item> </list>
    '''
    ''' <para><b>Hinweis:</b><br/> Transformationen über die gesamte Auswahl erfolgen per Zeichen und können bei sehr
    ''' großen Texten zeitintensiv sein;<br/> nutzen Sie nach Möglichkeit Batch-Blöcke.</para>
    ''' </remarks>
    <Description("Erweiterte RichTextBox mit bequemen Formatierungs- und Abfrage-Hilfen für Auswahl und Caret (u.a. Schriftgröße, Stil-Flags, Vorder-/Hintergrundfarbe, Absatz-Einzüge, Ausrichtung) sowie Batch-Updates über Redraw-Suppression.")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(ExtendedRTF), "ExtendedRTFControl.ExtendedRTF.bmp")>
    Public Class ExtendedRTF

#Region "Variablen"

        Private _updateNesting As Int32 ' Zähler für geschachtelte Update-Blöcke.
        Private _redrawHandle As IntPtr ' Handle, für das Redraw tatsächlich unterdrückt wurde.

#End Region

#Region "Eigenschaften"

        ''' <summary>
        ''' Liest oder setzt die Schriftgröße der aktuellen Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).<br/> Die Zuweisung
        ''' validiert gegen <c>MIN_FONT_SIZE</c>.
        ''' </remarks>
        ''' <value>
        ''' <see cref="Single?"/>: Konkreter Wert oder <see langword="Nothing"/> bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionFontSize As Single?
            Get
                If Me.SelectionLength = 0 Then
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Return f.Size
                End If
                Dim format = Me.GetSelectionCharacterFormat()
                If (format.Mask And CFM_SIZE) = 0 Then Return Nothing
                Return CSng(format.Height) / 20.0F
            End Get
            Set(value As Single?)
                If Not value.HasValue Then Exit Property
                If value.Value < MIN_FONT_SIZE Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), $"Schriftgröße muss mindestens {MIN_FONT_SIZE} sein.")
                End If
                Me.SetSelectionFontSize(value.Value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt den Fettdruck der aktuellen Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).
        ''' </remarks>
        ''' <value>
        ''' <see cref="Boolean?"/>: <see langword="True"/> oder <see langword="False"/>, bzw. <see langword="Nothing"/>
        ''' bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionBold As Boolean?
            Get
                If Me.SelectionLength = 0 Then
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Return f.Bold
                End If
                Return Me.GetUniformFontFlag(FontStyle.Bold)
            End Get
            Set(value As Boolean?)
                If Not value.HasValue Then Exit Property
                Me.ApplyStyleFlag(FontStyle.Bold, value.Value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt Kursiv (Italic) der aktuellen Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).
        ''' </remarks>
        ''' <value>
        ''' <see cref="Boolean?"/>: <see langword="True"/> oder <see langword="False"/>, bzw. <see langword="Nothing"/>
        ''' bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionItalic As Boolean?
            Get
                If Me.SelectionLength = 0 Then
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Return f.Italic
                End If
                Return Me.GetUniformFontFlag(FontStyle.Italic)
            End Get
            Set(value As Boolean?)
                If Not value.HasValue Then Exit Property
                Me.ApplyStyleFlag(FontStyle.Italic, value.Value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt Unterstreichung der aktuellen Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).
        ''' </remarks>
        ''' <value>
        ''' <see cref="Boolean?"/>: <see langword="True"/> oder <see langword="False"/>, bzw. <see langword="Nothing"/>
        ''' bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionUnderline As Boolean?
            Get
                If Me.SelectionLength = 0 Then
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Return f.Underline
                End If
                Return Me.GetUniformFontFlag(FontStyle.Underline)
            End Get
            Set(value As Boolean?)
                If Not value.HasValue Then Exit Property
                Me.ApplyStyleFlag(FontStyle.Underline, value.Value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt Durchstreichung der aktuellen Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).
        ''' </remarks>
        ''' <value>
        ''' <see cref="Boolean?"/>: <see langword="True"/> oder <see langword="False"/>, bzw. <see langword="Nothing"/>
        ''' bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionStrikeout As Boolean?
            Get
                If Me.SelectionLength = 0 Then
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Return f.Strikeout
                End If
                Return Me.GetUniformFontFlag(FontStyle.Strikeout)
            End Get
            Set(value As Boolean?)
                If Not value.HasValue Then Exit Property
                Me.ApplyStyleFlag(FontStyle.Strikeout, value.Value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt die aktuelle Vordergrundfarbe (Textfarbe) der Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Meldet keinen Mischzustand (immer konkreter Wert).<br/> Für echte Mischzustandserkennung wäre eine
        ''' per-Zeichen-Prüfung analog zu den Stil-Flags nötig.
        ''' </remarks>
        ''' <value>
        ''' <see cref="Color"/> der Auswahl bzw. am Caret.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionForeColor As Color
            Get
                Return Me.SelectionColor
            End Get
            Set(value As Color)
                Me.ExecuteBatch(Sub() Me.SelectionColor = value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt die aktuelle Hintergrund-/Highlightfarbe der Auswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Meldet keinen Mischzustand (immer konkreter Wert).<br/> Für echte Mischzustandserkennung wäre eine
        ''' per-Zeichen-Prüfung analog zu den Stil-Flags nötig.
        ''' </remarks>
        ''' <value>
        ''' <see cref="Color"/> der Markierung/Hinterlegung.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overloads Property SelectionBackColor As Color
            Get
                Return MyBase.SelectionBackColor
            End Get
            Set(value As Color)
                Me.ExecuteBatch(Sub() MyBase.SelectionBackColor = value)
            End Set
        End Property

        ''' <summary>
        ''' Liest oder setzt den linken Absatz-Einzug (in Pixel) der aktuellen Absatz-/Absatzauswahl bzw. am Caret.
        ''' </summary>
        ''' <remarks>
        ''' Bei uneinheitlicher Auswahl wird <see langword="Nothing"/> zurückgegeben (Mischzustand).<br/> Der Einzug
        ''' wirkt absatzweise; die Auswahl wird intern absatzweise behandelt.
        ''' </remarks>
        ''' <value>
        ''' <see cref="Int32"/>: konkreter Einzug oder <see langword="Nothing"/> bei Mischzustand.
        ''' </value>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionLeftIndent As Int32?
            Get
                If Me.SelectionLength = 0 Then Return Me.SelectionIndent
                Dim format As New ParagraphFormat With {.Size = Marshal.SizeOf(Of ParagraphFormat)()}
                Dim unused = GetParagraphFormat(Me.Handle, EM_GETPARAFORMAT, IntPtr.Zero, format)
                If (format.Mask And PFM_STARTINDENT) = 0 Then Return Nothing
                Return Me.SelectionIndent
            End Get
            Set(value As Int32?)
                If Not value.HasValue Then Exit Property
                If value.Value < 0 Then Throw New ArgumentOutOfRangeException(NameOf(value), "Einzug darf nicht negativ sein.")
                Me.ExecuteBatch(Sub() Me.SelectionIndent = value.Value)
            End Set
        End Property

#End Region

#Region "Öffentliche Methoden"

        Public Sub New()

            ' Dieser Aufruf ist für den Designer erforderlich.
            Me.InitializeComponent()
            ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        End Sub

        ''' <summary>Lädt RTF oder übernimmt bei Ablehnung durch Rich Edit den unveränderten Eingabetext als Klartext.</summary>
        ''' <param name="content">Bereits dekodierter RTF- oder Klartext; darf nicht <see langword="Nothing"/> sein.</param>
        ''' <returns><see langword="True"/> bei RTF-Import, sonst <see langword="False"/> für Klartext einschließlich leerer Eingaben.</returns>
        ''' <exception cref="ArgumentNullException"><paramref name="content"/> ist <see langword="Nothing"/>.</exception>
        ''' <remarks>Das Ergebnis meldet den gewählten Importweg; die geerbte Rtf-Eigenschaft bleibt unverändert.</remarks>
        Public Function LoadRtfOrPlainText(content As String) As Boolean
#If NET6_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(content, NameOf(content))
#Else
            If content Is Nothing Then Throw New ArgumentNullException(NameOf(content))
#End If
            If content.Length = 0 Then
                Me.Text = String.Empty
                Return False
            End If

            Using candidate As New RichTextBox()
                Dim handle = candidate.Handle
                Try
                    candidate.Rtf = content
                Catch ex As ArgumentException
                    Me.Text = content
                    Return False
                End Try
                Me.Rtf = candidate.Rtf
            End Using
            Return True
        End Function

        ''' <summary>Lädt RTF-Bytes oder dekodiert bei Ablehnung den Inhalt als Klartext.</summary>
        ''' <param name="content">Bytes des RTF- oder Klartextdokuments; dürfen nicht <see langword="Nothing"/> sein.</param>
        ''' <param name="plainTextEncoding">Explizite Kodierung für den Klartext-Fallback; darf nicht <see langword="Nothing"/> sein.</param>
        ''' <returns><see langword="True"/> bei RTF-Import, sonst <see langword="False"/> für Klartext einschließlich leerer Eingaben.</returns>
        ''' <exception cref="ArgumentNullException">Ein Parameter ist <see langword="Nothing"/>.</exception>
        ''' <exception cref="DecoderFallbackException">Die Bytes sind mit der angegebenen Klartextkodierung nicht dekodierbar.</exception>
        ''' <remarks>RTF wird aus den Originalbytes gelesen, damit die im Dokument angegebene Codepage erhalten bleibt.
        ''' Eine zur Klartextkodierung passende Byte-Reihenfolge-Markierung wird entfernt; andere Kodierungen werden nicht erraten.</remarks>
        Public Function LoadRtfOrPlainText(content As Byte(), plainTextEncoding As Encoding) As Boolean
#If NET6_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(content, NameOf(content))
            ArgumentNullException.ThrowIfNull(plainTextEncoding, NameOf(plainTextEncoding))
#Else
            If content Is Nothing Then Throw New ArgumentNullException(NameOf(content))
            If plainTextEncoding Is Nothing Then Throw New ArgumentNullException(NameOf(plainTextEncoding))
#End If

            If content.Length > 0 Then
                Using candidate As New RichTextBox(), source As New MemoryStream(content, False)
                    Dim handle = candidate.Handle
                    Dim parsed = True
                    Try
                        candidate.LoadFile(source, RichTextBoxStreamType.RichText)
                    Catch ex As ArgumentException
                        parsed = False
                    End Try
                    If parsed Then
                        Me.Rtf = candidate.Rtf
                        Return True
                    End If
                End Using
            End If

            Dim strictEncoding = DirectCast(plainTextEncoding.Clone(), Encoding)
            strictEncoding.DecoderFallback = DecoderFallback.ExceptionFallback
            Dim preamble = plainTextEncoding.GetPreamble()
            Dim offset = 0
            If preamble.Length > 0 AndAlso content.Length >= preamble.Length Then
                Dim matches = True
                For i = 0 To preamble.Length - 1
                    If content(i) <> preamble(i) Then
                        matches = False
                        Exit For
                    End If
                Next
                If matches Then offset = preamble.Length
            End If
            Dim text = strictEncoding.GetString(content, offset, content.Length - offset)
            Me.Text = text
            Return False
        End Function

        ''' <summary>
        ''' Entfernt Formatierungen (Schriftstil, Vorder-/Hintergrundfarbe, Bullet-Aufzählung) vollständig aus aktueller
        ''' Auswahl oder – ohne Auswahl – ab der Caret-Position.
        ''' </summary>
        ''' <remarks>
        ''' Optimiert: Wendet die Normalisierung einmal auf die gesamte Auswahl an (statt per Zeichen).<br/> Bei keiner
        ''' Auswahl wird das Format an der aktuellen Einfügemarke (Caret) zurückgesetzt.
        ''' </remarks>
        Public Sub ClearFormatting()
            Me.ExecuteBatch(Sub()
                                Dim baseFont = Me.SelectionFont
                                Dim mixedFont = baseFont Is Nothing
                                If mixedFont Then baseFont = Me.Font
                                Dim forceReset = mixedFont
                                If Me.SelectionLength > 0 Then
                                    Dim format = Me.GetSelectionCharacterFormat()
                                    Const requiredMask As Int32 = CFM_SIZE Or CFM_FACE Or CFM_CHARSET Or CFM_FONT_STYLES
                                    forceReset = forceReset OrElse (format.Mask And requiredMask) <> requiredMask
                                End If
                                Dim resetFont = CreateFont(baseFont, baseFont.Size, FontStyle.Regular, forceReset)
                                If resetFont IsNot baseFont Then
                                    Using resetFont
                                        Me.SelectionFont = resetFont
                                    End Using
                                End If
                                Me.SelectionColor = Me.ForeColor
                                Me.SelectionBackColor = Me.BackColor
                                Me.SelectionBullet = False
                            End Sub)
        End Sub

        ''' <summary>
        ''' Setzt die horizontale Ausrichtung der aktuellen Absatz-/Absatzauswahl.
        ''' </summary>
        ''' <remarks>
        ''' Wirkt absatzweise. Bei keiner Auswahl wird der aktuelle Absatz ausgerichtet.
        ''' </remarks>
        ''' <param name="alignment">Die gewünschte horizontale Ausrichtung.</param>
        Public Sub SetSelectionAlignment(alignment As HorizontalAlignment)
            Me.ExecuteBatch(Sub() Me.SelectionAlignment = alignment)
        End Sub

        ''' <summary>
        ''' Schaltet Fettdruck für aktuelle Auswahl bzw. Caret um.
        ''' </summary>
        ''' <remarks>
        ''' Bei Auswahl mit Mischzustand wird für alle ausgewählten Zeichen auf den invertierten Zustand umgestellt.
        ''' </remarks>
        Public Sub ToggleBold()
            Me.SelectionBold = Not Me.SelectionBold.GetValueOrDefault(False)
        End Sub

        ''' <summary>
        ''' Schaltet Kursiv für aktuelle Auswahl bzw. Caret um.
        ''' </summary>
        ''' <remarks>
        ''' Bei Auswahl mit Mischzustand wird für alle ausgewählten Zeichen auf den invertierten Zustand umgestellt.
        ''' </remarks>
        Public Sub ToggleItalic()
            Me.SelectionItalic = Not Me.SelectionItalic.GetValueOrDefault(False)
        End Sub

        ''' <summary>
        ''' Schaltet Unterstreichung für aktuelle Auswahl bzw. Caret um.
        ''' </summary>
        ''' <remarks>
        ''' Bei Auswahl mit Mischzustand wird für alle ausgewählten Zeichen auf den invertierten Zustand umgestellt.
        ''' </remarks>
        Public Sub ToggleUnderline()
            Me.SelectionUnderline = Not Me.SelectionUnderline.GetValueOrDefault(False)
        End Sub

        ''' <summary>
        ''' Schaltet Durchstreichung für aktuelle Auswahl bzw. Caret um.
        ''' </summary>
        ''' <remarks>
        ''' Bei Auswahl mit Mischzustand wird für alle ausgewählten Zeichen auf den invertierten Zustand umgestellt.
        ''' </remarks>
        Public Sub ToggleStrikeout()
            Me.SelectionStrikeout = Not Me.SelectionStrikeout.GetValueOrDefault(False)
        End Sub

        ''' <summary>
        ''' Schaltet Bullet-Aufzählung für aktuelle Absatz-/Absatzauswahl um.
        ''' </summary>
        ''' <remarks>
        ''' Wirkt absatzweise. Funktioniert nur auf Absatzebene (SelectionLength = 0 -&gt; aktueller Absatz).
        ''' </remarks>
        Public Sub ToggleBullet()
            Me.ExecuteBatch(Sub() Me.SelectionBullet = Not Me.SelectionBullet)
        End Sub

#End Region

#Region "Interne Methoden"

        ''' <summary>
        ''' Liefert ein einheitliches Bool-Stil-Flag für die aktuelle Auswahl oder Nothing bei Mischzustand.
        ''' </summary>
        ''' <param name="flag">Das abzufragende Stil-Flag.</param>
        ''' <returns>Ein einheitliches Bool-Stil-Flag oder Nothing bei Mischzustand.</returns>
        Private Function GetUniformFontFlag(flag As FontStyle) As Boolean?
            Dim format = Me.GetSelectionCharacterFormat()
            Dim mask = CInt(flag)
            If (format.Mask And mask) = 0 Then Return Nothing
            Return (format.Effects And mask) <> 0
        End Function

        ''' <summary>Ermittelt die einheitlichen Zeichenattribute ohne Änderung der Auswahl.</summary>
        ''' <returns>Die temporär gespeicherten nativen Formatierungsattribute.</returns>
        Private Function GetSelectionCharacterFormat() As CharacterFormat
            Dim format As New CharacterFormat With {.Size = Marshal.SizeOf(Of CharacterFormat)()}
            Dim unused = GetCharacterFormat(Me.Handle, EM_GETCHARFORMAT, New IntPtr(SCF_SELECTION), format)
            Return format
        End Function

        ''' <summary>
        ''' Setzt die Schriftgröße der aktuellen Auswahl oder des Carets.
        ''' </summary>
        ''' <param name="newSize">Die bereits durch die Eigenschaft validierte Schriftgröße.</param>
        Private Sub SetSelectionFontSize(newSize As Single)
            Dim currentSize = Me.SelectionFontSize
            If currentSize.HasValue AndAlso currentSize.Value = newSize Then Return
            Me.ApplyFontTransformation(
                    Function(f)
                        Return CreateFont(f, newSize, f.Style)
                    End Function)
        End Sub

        ''' <summary>
        ''' Wendet oder entfernt ein einzelnes FontStyle-Flag auf die aktuelle Auswahl oder den Caret an.
        ''' </summary>
        ''' <param name="flag">Das FontStyle-Flag, das angewendet oder entfernt werden soll.</param>
        ''' <param name="enabled">Gibt an, ob das Flag angewendet (True) oder entfernt (False) werden soll.</param>
        Private Sub ApplyStyleFlag(flag As FontStyle, enabled As Boolean)
            Dim current = If(Me.SelectionLength = 0, CType(Nothing, Boolean?), Me.GetUniformFontFlag(flag))
            If current.HasValue AndAlso current.Value = enabled Then Return
            Me.ApplyFontTransformation(
                    Function(f)
                        Dim targetStyle = If(enabled, f.Style Or flag, f.Style And Not flag)
                        Return CreateFont(f, f.Size, targetStyle)
                    End Function)
        End Sub

        ''' <summary>Erstellt nur bei einer Änderung der Schriftattribute einen neuen Font.</summary>
        ''' <param name="source">Schrift mit den zu erhaltenden Eigenschaften.</param>
        ''' <param name="size">Gewünschte Schriftgröße in der Einheit der Ausgangsschrift.</param>
        ''' <param name="style">Gewünschter Schriftstil.</param>
        ''' <param name="force">Erstellt auch bei gleichen Attributen einen Font für gemischte Auswahlen.</param>
        ''' <returns>Den Ausgangsfont oder einen vom Aufrufer freizugebenden neuen Font.</returns>
        Private Shared Function CreateFont(source As Font, size As Single, style As FontStyle, Optional force As Boolean = False) As Font
            If Not force AndAlso source.Size = size AndAlso source.Style = style Then Return source
            Return New Font(source.FontFamily, size, style, source.Unit, source.GdiCharSet, source.GdiVerticalFont)
        End Function

        ''' <summary>
        ''' Wendet eine Transformation auf den aktuellen Font der Auswahl oder des Carets an.
        ''' </summary>
        ''' <param name="transform">Eine Funktion, die den aktuellen Font nimmt und einen neuen Font zurückgibt.</param>
        Private Sub ApplyFontTransformation(transform As Func(Of Font, Font))
            Me.ExecuteBatch(Sub() Me.ApplyFontTransformationCore(transform))
        End Sub

        ''' <summary>Transformiert die Auswahl innerhalb des bereits gestarteten Batch-Blocks.</summary>
        ''' <param name="transform">Erzeugt den gewünschten Font aus dem aktuellen Font.</param>
        Private Sub ApplyFontTransformationCore(transform As Func(Of Font, Font))
            If Me.SelectionLength = 0 Then
                Dim f = Me.SelectionFont ' Nur Caret: Einfach einmal transformieren
                If f Is Nothing Then f = Me.Font
                Dim nf = transform(f)
                If nf Is Nothing Then Exit Sub
                If nf Is f Then Exit Sub ' keine Änderung
                Try
                    Me.SelectionFont = nf
                Finally
                    If nf IsNot f Then nf.Dispose()
                End Try
                Return
            End If
            Dim start = Me.SelectionStart ' Auswahl: pro Zeichen anwenden (RichTextBox hat kein native Multi-Teil-Transform API auf .NET-Level)
            Dim len = Me.SelectionLength
            Dim cache As New Collections.Generic.Dictionary(Of String, Font)(StringComparer.Ordinal)
            Try
                For i = 0 To len - 1
                    Me.[Select](start + i, 1)
                    Dim f = Me.SelectionFont
                    If f Is Nothing Then f = Me.Font
                    Dim key = FontCacheKey(f)
                    Dim apply As Font = Nothing
                    If Not cache.TryGetValue(key, apply) Then
                        Dim transformed = transform(f)
                        If transformed IsNot f Then apply = transformed
                        cache.Add(key, apply)
                    End If
                    If apply IsNot Nothing Then Me.SelectionFont = apply
                Next
            Finally
                Try
                    Me.[Select](start, len)
                Finally
                    For Each cachedFont In cache.Values
                        If cachedFont IsNot Nothing Then cachedFont.Dispose()
                    Next
                End Try
            End Try
        End Sub

        ''' <summary>
        ''' Erzeugt einen konsistenten Cache-Schlüssel für einen Font basierend auf seinen Eigenschaften.
        ''' </summary>
        ''' <param name="f">Der Font, für den der Cache-Schlüssel erstellt werden soll.</param>
        ''' <returns>Ein String, der den Font eindeutig identifiziert.</returns>
        Private Shared Function FontCacheKey(f As Font) As String
            Return $"{f.FontFamily.Name}|{f.Size}|{CInt(f.Style)}|{CInt(f.Unit)}|{f.GdiCharSet}|{f.GdiVerticalFont}"
        End Function

        ''' <summary>Führt eine Formatierung aus und beendet den Batch auch bei Fehlern.</summary>
        ''' <param name="operation">Die im Batch auszuführende Formatierung.</param>
        Private Sub ExecuteBatch(operation As Action)
            Me.BeginUpdate()
            Try
                operation()
            Finally
                Me.EndUpdate()
            End Try
        End Sub

        ''' <summary>
        ''' Startet einen verschachtelbaren Batch-Block, der das Neuzeichnen des Controls unterdrückt, um Flackern zu
        ''' vermeiden und die Leistung zu verbessern. Bei verschachtelten Aufrufen wird das Neuzeichnen erst wieder
        ''' aktiviert, wenn alle Blöcke beendet wurden.
        ''' </summary>
        Private Sub BeginUpdate()
            If Interlocked.Increment(Me._updateNesting) = 1 Then
                Me.SuppressRedraw()
            End If
        End Sub

        ''' <summary>
        ''' Beendet einen verschachtelbaren Batch-Block, der das Neuzeichnen des Controls unterdrückt.
        ''' </summary>
        Private Sub EndUpdate()
            If Interlocked.Decrement(Me._updateNesting) = 0 AndAlso Me._redrawHandle <> IntPtr.Zero Then
                Dim handle = Me._redrawHandle
                Me._redrawHandle = IntPtr.Zero
                Dim unused = SendMessage(handle, WM_SETREDRAW, True, IntPtr.Zero)
                Me.Invalidate() ' Neuzeichnen anfordern
                Me.Update()     ' Sofortige Ausführung (reduziert wahrnehmbares Flackern)
            End If
        End Sub

        ''' <summary>Übernimmt einen laufenden Batch auf ein neu erzeugtes Fensterhandle.</summary>
        ''' <param name="e">Die Ereignisdaten der Handle-Erzeugung.</param>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            If Me._updateNesting > 0 Then
                Me.SuppressRedraw()
            End If
        End Sub

        ''' <summary>Vergisst ein zerstörtes Handle, damit es nicht erneut verwendet wird.</summary>
        ''' <param name="e">Die Ereignisdaten der Handle-Zerstörung.</param>
        Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
            Me._redrawHandle = IntPtr.Zero
            MyBase.OnHandleDestroyed(e)
        End Sub

        ''' <summary>Unterdrückt Redraw nur für sichtbare Controls, da WM_SETREDRAW deren Sichtbarkeit verändert.</summary>
        Private Sub SuppressRedraw()
            If Me.IsHandleCreated AndAlso Me.Visible Then
                Me._redrawHandle = Me.Handle
                Dim unused = SendMessage(Me._redrawHandle, WM_SETREDRAW, False, IntPtr.Zero)
            End If
        End Sub

#End Region

    End Class

End Namespace
