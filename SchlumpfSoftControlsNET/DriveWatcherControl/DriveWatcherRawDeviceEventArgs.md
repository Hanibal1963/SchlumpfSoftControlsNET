# DriveWatcherRawDeviceEventArgs

Unveränderliche Ereignisdaten einer vom Empfangsfenster beobachteten `WM_DEVICECHANGE`-Nachricht. Sie werden bei aktiviertem `DriveWatcher.DiagnosticMode` über `RawDeviceEventReceived` bereitgestellt.

## 1. Konstruktoren

Es gibt keinen öffentlichen Konstruktor. Die Komponente erstellt die Ereignisdaten beim Empfang der Nachricht.

## 2. Ereignisse

Keine.

## 3. Eigenschaften

- `EventCode As Long`: Numerischer Wert von `wParam`, beispielsweise `&H8000` für Ankunft oder `&H8004` für vollständige Entfernung.

- `DeviceType As Integer?`: Gerätetyp aus einem gültigen Header einer Ankunfts- oder vollständigen Entfernungsmeldung, beispielsweise `2` für ein Volume. Ist der Header nicht verfügbar oder wird für den Ereigniscode nicht ausgewertet, ist der Wert `Nothing`.

- `UnitMask As Integer?`: Unveränderte 32-Bit-Bitmaske einer ausreichend langen Volume-Struktur. Bit 0 steht für A, Bit 25 für Z. Auch eine ungültige oder mehrere Laufwerke umfassende Maske wird unverändert gemeldet. Ohne gültige Volume-Struktur ist der Wert `Nothing`.

- `Flags As Short?`: Unveränderte 16-Bit-Volume-Flags. Bit 0 kennzeichnet einen Medienwechsel, Bit 1 ein Netzlaufwerk. Ohne gültige Volume-Struktur ist der Wert `Nothing`. Das Vorzeichen ändert das native Bitmuster nicht.

- `TimestampUtc As DateTimeOffset`: Zeitpunkt der Erfassung auf dem Empfangsthread mit UTC-Offset null.

Alle Eigenschaften sind schreibgeschützt. `Nothing` unterscheidet ein nicht verfügbares Feld von einem gelesenen Wert `0`.

## 4. Funktionen

Keine eigenen öffentlichen Funktionen.

## 5. Hilfstypen/Strukturen

Keine.

## 6. Hinweise

- Der Feed enthält alle empfangenen `WM_DEVICECHANGE`-Codes, unabhängig von Laufwerksfiltern, Entprellung und Polling. Andere Windows-Nachrichten und rein durch Polling erkannte Änderungen erzeugen keine Rohereignisse.

- Bei Ankunft und vollständiger Entfernung werden vorhandene Header und Volume-Strukturen nach den Größenprüfungen der Komponente kopiert. Für andere Ereigniscodes werden keine `lParam`-Strukturen ausgewertet. Das Ereignis stellt keine nativen Zeiger oder vollständigen gerätespezifischen Nutzdaten bereit.

- `SynchronizingObject` steuert die Zustellung. Ein vorgemerktes Ereignis behält seine kopierten Werte auch nach Rückkehr aus der Fensterprozedur; beim Freigeben der Komponente werden noch nicht begonnene Zustellungen verworfen.

- Die Zustellung vor der normalen Laufwerksverarbeitung erfolgt synchron oder wird asynchron vorgemerkt. Handlerfehler folgen den Zustellungs- und Windows-Forms-Fehlerregeln der Komponente.

## 7. geplante Optimierungen

Keine geplant.

### 7.1. Reihenfolge zur Durchführung

Keine offenen Optimierungen.

## 8. geplante Erweiterungen

Keine geplant.

### 8.1. Reihenfolge zur Durchführung

Keine offenen Erweiterungen.