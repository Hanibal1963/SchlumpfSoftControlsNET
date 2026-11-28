Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' Die folgenden Ereignisse stehen für MyApplication zur Verfügung:
    ' Startup: Wird beim Start der Anwendung ausgelöst, bevor das Startformular erstellt wird.
    ' Shutdown: Wird ausgelöst, nachdem alle Anwendungsformulare geschlossen wurden. Bei einer abnormalen Beendigung der Anwendung wird dieses Ereignis nicht ausgelöst.
    ' UnhandledException: Wird ausgelöst, wenn in der Anwendung eine unbehandelte Ausnahme auftritt.
    ' StartupNextInstance: Wird beim Starten einer Anwendung mit nur einer Instanz ausgelöst, wenn die Anwendung bereits aktiv ist.
    ' NetworkAvailabilityChanged: Wird ausgelöst, wenn die Netzwerkverbindung hergestellt oder getrennt wird.

    ' **NEU** ApplyApplicationDefaults: Wird ausgelöst, wenn die Anwendung die festzulegenden Standardwerte abfragt.

    ' Beispiel:
    ' Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
    '
    '   ' Festlegen der anwendungsweiten Standardschriftart:
    '   e.Font = New Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
    '
    '   ' Festlegen des HighDpiMode für die Anwendung:
    '   e.HighDpiMode = HighDpiMode.PerMonitorV2
    '
    '   ' Bei Verwendung eines Startdialogs wird hiermit die Mindestanzeigedauer festgelegt:
    '   e.MinimumSplashScreenDisplayTime = 4000
    ' End Sub

    Partial Friend Class MyApplication

    End Class

End Namespace
