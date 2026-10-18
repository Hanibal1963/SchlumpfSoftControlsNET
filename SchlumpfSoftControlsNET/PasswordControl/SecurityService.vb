' --------------------------------------------------------------------------------------------------------
' Datei: SecurityService.vb
' Author: Andreas Sauer
' Datum: 27.07.2026
' --------------------------------------------------------------------------------------------------------

Imports System

Imports System.Security.Cryptography

Namespace PasswordControl

    ''' <summary>
    ''' Stellt Funktionen zum sicheren Hashing (Validierung) und optionalen Schützen/Entschützen (DPAPI) bereit.
    ''' </summary>
    Friend NotInheritable Class SecurityService

        Private Const Iterations As Int32 = 100000
        Private Const SaltSize As Int32 = 16
        Private Const HashSize As Int32 = 32

        ''' <summary>
        ''' Erzeugt einen speicherbaren Passwort-Hash im Format PBKDF2$Iterationen$Salt$Hash.
        ''' </summary>
        ''' <param name="password">Klartext-Passwort.</param>
        ''' <returns>Serialisierter Passwort-Hash.</returns>
        ''' <exception cref="ArgumentException">Wenn das Passwort leer ist.</exception>
        Public Shared Function CreatePasswordHash(password As String) As String

            If String.IsNullOrWhiteSpace(password) Then Throw New ArgumentException("Passwort darf nicht leer sein.", NameOf(password))

            ' Erzeugt für jedes Passwort einen eigenen Zufalls-Salt, damit identische Eingaben unterschiedliche Hashwerte ergeben.
            Dim salt(SaltSize - 1) As Byte

            Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
                rng.GetBytes(salt)
            End Using

            ' Leitet aus Passwort, Salt und Iterationszahl einen festen Hashwert für die spätere Prüfung ab.
            Dim hash As Byte() = DerivePbkdf2(password, salt, Iterations, HashSize)

            ' Serialisiert alle benötigten Bestandteile in ein speicherbares Textformat für die spätere Verifikation.
            Return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"

        End Function

        ''' <summary>
        ''' Validiert ein eingegebenes Passwort gegen einen gespeicherten PBKDF2-Hash.
        ''' </summary>
        ''' <param name="password">Eingegebenes Klartext-Passwort.</param>
        ''' <param name="storedHash">Gespeicherter Hash im Format PBKDF2$Iterationen$Salt$Hash.</param>
        ''' <returns>True, wenn gültig; sonst False.</returns>
        ''' <exception cref="ArgumentException">Wenn das Passwort oder der gespeicherte Hash leer ist.</exception>
        ''' <exception cref="FormatException">Wenn Salt oder Hash im gespeicherten Wert nicht gültig Base64-kodiert sind.</exception>
        Public Shared Function VerifyPassword(password As String, storedHash As String) As Boolean

            If String.IsNullOrWhiteSpace(password) Then
                Throw New ArgumentException("Passwort darf nicht leer sein.", NameOf(password))
            End If

            If String.IsNullOrWhiteSpace(storedHash) Then
                Throw New ArgumentException("Hash darf nicht leer sein.", NameOf(storedHash))
            End If

            ' Zerlegt den gespeicherten Wert in Algorithmuskennung, Iterationszahl, Salt und Hash.
            Dim parts As String() = storedHash.Split("$"c)
            If parts.Length <> 4 OrElse Not String.Equals(parts(0), "PBKDF2", StringComparison.Ordinal) Then Return False

            Dim iterationCount As Int32

            ' Übernimmt die im gespeicherten Hash hinterlegte Iterationszahl, damit Altwerte weiterhin validiert werden können.
            If Not Int32.TryParse(parts(1), iterationCount) Then Return False

            ' Stellt Salt und Ziel-Hash aus der Base64-Darstellung wieder als Bytefolgen her.
            Dim salt As Byte() = Convert.FromBase64String(parts(2))
            Dim expectedHash As Byte() = Convert.FromBase64String(parts(3))

            ' Berechnet aus dem eingegebenen Passwort erneut den Vergleichswert in derselben Länge wie der gespeicherte Hash.
            Dim actualHash As Byte() = DerivePbkdf2(password, salt, iterationCount, expectedHash.Length)

            ' Vergleicht beide Bytefolgen in konstanter Zeit, um Rückschlüsse über die Laufzeit zu vermeiden.
            Return FixedTimeEquals(actualHash, expectedHash)

        End Function

        ''' <summary>
        ''' Verschlüsselt Daten benutzergebunden mit DPAPI (reversibel).
        ''' </summary>
        ''' <param name="plainText">Zu schützender Klartext.</param>
        ''' <returns>Base64-kodierter Ciphertext.</returns>
        ''' <exception cref="ArgumentNullException">Wenn <paramref name="plainText" /> den Wert <c>Nothing</c> hat.</exception>
        ''' <exception cref="CryptographicException">Wenn der Klartext nicht geschützt werden kann.</exception>
        Public Shared Function ProtectSecret(plainText As String) As String

#If NET6_0_OR_GREATER Then
            ArgumentNullException.ThrowIfNull(plainText)
#Else
            If plainText Is Nothing Then Throw New ArgumentNullException(NameOf(plainText))
#End If

            ' Wandelt den Klartext in UTF-8-Bytes um, damit die DPAPI mit einem stabilen Binärformat arbeiten kann.
            Dim data As Byte() = Text.Encoding.UTF8.GetBytes(plainText)

            ' Schützt die Daten benutzergebunden, sodass nur derselbe Windows-Benutzer sie wieder entschlüsseln kann.
            Dim protectedBytes As Byte() = ProtectedData.Protect(data, Nothing, DataProtectionScope.CurrentUser)
            ' Kodiert die geschützten Bytes als Text, damit sie gespeichert oder übertragen werden können.
            Return Convert.ToBase64String(protectedBytes)

        End Function

        ''' <summary>
        ''' Entschlüsselt benutzergebundene DPAPI-Daten.
        ''' </summary>
        ''' <param name="protectedBase64">Base64-kodierter Ciphertext.</param>
        ''' <returns>Entschlüsselter Klartext.</returns>
        ''' <exception cref="ArgumentException">Wenn <paramref name="protectedBase64" /> leer ist.</exception>
        ''' <exception cref="FormatException">Wenn <paramref name="protectedBase64" /> kein gültiger Base64-Text ist.</exception>
        ''' <exception cref="CryptographicException">Wenn die DPAPI-Daten nicht für den aktuellen Benutzer entschlüsselt werden können.</exception>
        Public Shared Function UnprotectSecret(protectedBase64 As String) As String

            If String.IsNullOrWhiteSpace(protectedBase64) Then Throw New ArgumentException("Wert darf nicht leer sein.", NameOf(protectedBase64))

            ' Wandelt den gespeicherten Base64-Text zurück in die ursprünglich geschützte Bytefolge.
            Dim protectedBytes As Byte() = Convert.FromBase64String(protectedBase64)

            ' Hebt den benutzergebundenen DPAPI-Schutz wieder auf und liefert die ursprünglichen Nutzdaten zurück.
            Dim data As Byte() = ProtectedData.Unprotect(protectedBytes, Nothing, DataProtectionScope.CurrentUser)
            ' Rekonstruiert den entschlüsselten Klartext aus den UTF-8-Bytes.
            Return Text.Encoding.UTF8.GetString(data)

        End Function

        ''' <summary>
        ''' Vergleicht zwei Byte-Arrays in konstanter Zeit, um Timing-Angriffe zu verhindern.
        ''' </summary>
        ''' <param name="a">Erstes Byte-Array.</param>
        ''' <param name="b">Zweites Byte-Array.</param>
        ''' <returns>True, wenn die Arrays identisch sind; sonst False.</returns>
        Private Shared Function FixedTimeEquals(a As Byte(), b As Byte()) As Boolean

            If a Is Nothing OrElse b Is Nothing Then Return False
            If a.Length <> b.Length Then Return False

            ' Verknüpft alle Byte-Unterschiede bitweise, damit die Schleife unabhängig vom ersten Treffer vollständig durchläuft.
            Dim result As Int32 = 0
            For i As Int32 = 0 To a.Length - 1
                result = result Or (a(i) Xor b(i))
            Next

            Return result = 0

        End Function

        Private Shared Function DerivePbkdf2(password As String, salt As Byte(), iterationCount As Int32, outputLength As Int32) As Byte()

#If NET8_0_OR_GREATER Or NET9_0_OR_GREATER Or NET10_0_OR_GREATER Then
            Return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterationCount, HashAlgorithmName.SHA256, outputLength)
#ElseIf NET472_OR_GREATER Then
            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iterationCount, HashAlgorithmName.SHA256)
                Return pbkdf2.GetBytes(outputLength)
            End Using
#Else
#Disable Warning CA5379
            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iterationCount)
                Return pbkdf2.GetBytes(outputLength)
            End Using
#Enable Warning CA5379
#End If

        End Function

    End Class

End Namespace


