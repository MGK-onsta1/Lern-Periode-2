# Morse-Übersetzer – Anleitung

## Starten

1. `Morse.exe` herunterladen und doppelklicken. Du musst nichts installieren.
2. Windows zeigt vielleicht **„Der Computer wurde durch Windows geschützt“**. Das kommt, weil das Programm nicht von einer bekannten Firma ist. Klicke auf **„Weitere Informationen“** und dann auf **„Trotzdem ausführen“**.
3. Beim ersten „Online gehen“ fragt die **Windows-Firewall** nach. Setze den Haken bei **„Private Netzwerke“** und klicke auf **„Zugriff zulassen“**. Sonst können dich Freunde nicht erreichen.

## Die Tabs

| Tab | Was man machen kann |
|-----|---------------------|
| **Übersetzer** | Text oder Morsecode eingeben → **Übersetzen**. Mit **▶ Abspielen** hörst du den Code. |
| **Leertasten-Eingabe** | Selbst morsen: **Leertaste** kurz drücken = Punkt, lang drücken = Strich. |
| **Lern-Modus** | Zeichen und **ganze Wörter** üben, in 5 Stufen. Der Punktestand wird gespeichert. |
| **Chat** | Mit Freunden Morse-Nachrichten schreiben (WLAN, Browser oder Internet). |

**Oben** stellst du den Ton ein:
- **Tempo:** Wörter pro Minute. Für Anfänger 6–10.
- **Ton:** die Tonhöhe in Hz
- **Lautstärke**
- **Pausen ×:** längere Pausen zwischen Buchstaben, so hört man sie besser. Für Anfänger 2.
- **🔊 Test:** spielt „MORSE“ zum Ausprobieren

## Chatten

**Alle müssen dasselbe Passwort eingeben.** Die Nachrichten sind verschlüsselt. Wer das Passwort nicht hat, kann nichts mitlesen.

### Im gleichen WLAN (beide haben das Programm)
1. Namen und Passwort eingeben und auf **„Online gehen“** klicken.
2. Dein Freund erscheint unter **„Im WLAN gefunden“** → **Doppelklick** darauf.
3. Klappt das nicht, gib seine IP-Adresse unter **„Freund verbinden“** ein. Seine IP steht bei ihm unter „Deine IP“.

### Mit dem Handy (ohne Programm)
1. **„Online gehen“** klicken. Der Haken „Browser-Zugang“ muss gesetzt sein.
2. Auf **„📱 QR-Code“** klicken. Dein Freund scannt ihn mit der Handy-Kamera und gibt das Passwort ein.
3. Das geht nur, wenn das Handy im **gleichen WLAN** ist. In Schul-WLANs ist das oft blockiert, dann den **Internet-Link** nehmen.

### Über das Internet
1. **„Online gehen“** und dann **„🌍 Internet-Link“** klicken.
2. Beim ersten Mal wird das kostenlose Programm *cloudflared* heruntergeladen (ca. 53 MB).
3. Nach ein paar Sekunden erscheint ein Link `https://….trycloudflare.com`. Schick ihn deinem Freund, oder er scannt den QR-Code. **Klick auf den Link** im QR-Fenster kopiert ihn.
4. Im Schul-WLAN wird der Link oft blockiert. Dann am Handy **WLAN aus** und **mobile Daten** benutzen.

### Mit der Leertaste morsen
1. Unten im Chat den Haken bei **„🔑 Mit Leertaste morsen“** setzen.
2. **Leertaste** kurz = Punkt, lang = Strich. Die Buchstaben erscheinen im Eingabefeld.
3. **Rücktaste** löscht den letzten Buchstaben, **Enter** sendet.
4. Zu schnell? Oben **„Pausen ×“** grösser machen, dann hast du mehr Zeit zwischen den Buchstaben.

### Verlauf
- Der Chatverlauf wird automatisch gespeichert und ist beim nächsten Start wieder da.
- **💾 Als Text…** speichert den Verlauf als `.txt`-Datei.
- **🗑 Verlauf löschen** löscht alles.

## Wo werden die Daten gespeichert?

Im Ordner `%AppData%\Morse` (in die Adresszeile vom Explorer eingeben):
- `einstellungen.json`: Ton-Einstellungen und Name (das Passwort wird **nicht** gespeichert)
- `lernstand.json`: Punktestand vom Lern-Modus
- `chatverlauf.json`: Chatverlauf

## Probleme?

| Problem | Lösung |
|---------|--------|
| Freund wird nicht gefunden | Seid ihr im gleichen WLAN? Manche Schul-WLANs blockieren das. Probiert den **Internet-Link**. |
| „Falsches Passwort“ | Das Passwort muss genau gleich sein, auch Groß- und Kleinschreibung. |
| Kein Ton | Lautstärke oben und am PC prüfen und auf **🔊 Test** klicken. |
| Leertaste macht nichts | Im Lern-Modus zuerst **Start / Weiter** klicken. Im Chat den Haken **„Mit Leertaste morsen“** setzen. |
| „Verbindung abgebrochen – habt ihr beide dasselbe Passwort?“ | Jemand hat es mit einem anderen Passwort versucht. Vergleicht das Passwort. |
| „Schon in einem anderen Fenster online“ | Das Programm läuft zweimal. Schliesse ein Fenster. |
| Handy: „Keine Verbindung“ | Im Schul-WLAN blockiert. Am Handy WLAN ausschalten und mit mobilen Daten den Internet-Link öffnen. |
