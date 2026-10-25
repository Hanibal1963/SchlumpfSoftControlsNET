' --------------------------------------------------------------------------------------------------------
' Datei: CharacterPattern.vb
' Author: Andreas Sauer
' Datum: 06.05.2026
' --------------------------------------------------------------------------------------------------------

Namespace SevenSegmentControl

    ''' <summary>
    '''  Dies sind die verschiedenen Bitmuster, die Zeichen auf dem Siebensegment-Display abbilden.
    ''' </summary>
    ''' <remarks>
    ''' Die Bits 0 bis 6 entsprechen den einzelnen Segmenten; ein gesetztes Bit aktiviert das jeweilige Segment.
    ''' </remarks>
    Friend Enum CharacterPattern

        None = &H0 ' Kein Segment aktiv (alles aus).
        Zero = &H77 ' Darstellung der Ziffer 0.
        One = &H24 ' Darstellung der Ziffer 1.
        Two = &H5D  ' Darstellung der Ziffer 2.
        Three = &H6D ' Darstellung der Ziffer 3.
        Four = &H2E ' Darstellung der Ziffer 4.
        Five = &H6B ' Darstellung der Ziffer 5.
        Six = &H7B ' Darstellung der Ziffer 6.
        Seven = &H25  ' Darstellung der Ziffer 7.
        Eight = &H7F  ' Darstellung der Ziffer 8 (alle Segmente an).
        Nine = &H6F  ' Darstellung der Ziffer 9.
        A = &H3F  ' Großbuchstabe A.
        B = &H7A ' Großbuchstabe B.
        C = &H53 ' Großbuchstabe C.
        cField = &H58 ' Kleinbuchstabe c (abgekürzte Form / Feldbezeichnung).
        D = &H7C ' Großbuchstabe D.
        E = &H5B ' Großbuchstabe E.
        F = &H1B ' Großbuchstabe F.
        G = &H73 ' Großbuchstabe G.
        H = &H3E ' Großbuchstabe H.
        hField = &H3A ' Kleinbuchstabe h (abgekürzte Form / Feldbezeichnung).
        i = &H20 ' Kleinbuchstabe i.
        J = &H74 ' Großbuchstabe J.
        L = &H52 ' Großbuchstabe L.
        N = &H38 ' Großbuchstabe N.
        o = &H78 ' Kleinbuchstabe o.
        P = &H1F ' Großbuchstabe P.
        Q = &H2F ' Großbuchstabe Q.
        R = &H18 ' Großbuchstabe R.
        T = &H5A ' Großbuchstabe T.
        U = &H76 ' Großbuchstabe U.
        uField = &H70 ' Kleinbuchstabe u (abgekürzte Form / Feldbezeichnung).
        Y = &H6E ' Großbuchstabe Y.
        Dash = &H8 ' Bindestrich / Minuszeichen.
        Equals = &H48 ' Gleichheitszeichen (=).
        Degrees = &HF ' Gradzeichen (°).
        Apostrophe = &H2 ' Apostroph (').
        Quote = &H6 ' Anführungszeichen (").
        RBracket = &H65 ' Rechte Klammer (]).
        Underscore = &H40 ' Unterstrich (_).
        Identical = &H49 ' Identisch-Zeichen (≡).
        [Not] = &H28 ' Logisches NOT-Zeichen (¬).

    End Enum

End Namespace
