# Code-Erklärung: Morse-Übersetzer

Hier steht kurz und einfach, was jede Datei macht.

## Wie hängt alles zusammen?

```
Form1 (Oberfläche)
 ├── MorseTable   → übersetzt Text ↔ Morse
 ├── MorseAudio   → macht den Ton
 ├── MorseKeyer   → erkennt Punkt/Strich bei der Leertaste
 └── LearnTrainer → Lern-Modus: Zeichen auswählen, Punkte zählen, speichern
```

`Form1` ist der „Chef“. Wenn ich einen Button drücke, ruft `Form1` die anderen Klassen auf.

## MorseTable.cs – der Übersetzer

- `Codes` ist ein `Dictionary`: Zu jedem Buchstaben gehört ein Code, z. B. `'A' → ".-"`.
- `Reverse` ist dasselbe umgekehrt: `".-" → 'A'`.
- **Text → Morse:** Den Text in Wörter teilen, jeden Buchstaben in der Tabelle nachschauen, die Codes mit Leerzeichen verbinden und die Wörter mit ` / `.
- **Morse → Text:** Bei `/` teilen (das sind die Wörter), dann bei Leerzeichen teilen (das sind die Buchstaben) und jeden Code in `Reverse` nachschauen.

## MorseAudio.cs – der Ton

- Ein Ton ist eine **Sinuswelle**: `Math.Sin(...)` gibt Zahlen zwischen -1 und 1. Das sind die Samples, die der Lautsprecher abspielt.
- 1 Einheit = `1200 / Tempo` Millisekunden. Ein Punkt dauert 1 Einheit, ein Strich 3.
- Abgespielt wird mit `waveOut` aus Windows (`winmm.dll`). Vorher war es `SoundPlayer`, aber dessen `Stop()` hat bis zu 1 Sekunde gebraucht. Dann hat die Leertaste nicht mehr richtig funktioniert.

## MorseKeyer.cs – die Leertaste

- `Press()` startet eine `Stopwatch`, `Release()` stoppt sie.
- Gedrückt **kürzer als 2 Einheiten** → Punkt, sonst → Strich.
- `Tick()` läuft alle 15 ms (über einen Timer) und misst die Pause:
  - Pause ≥ 3 Einheiten → Buchstabe fertig (Event `LetterCompleted`)
  - Pause ≥ 7 Einheiten → Wort fertig (Event `WordCompleted`)
- **Event** heisst: Der Keyer sagt „Buchstabe fertig!“, und `Form1` reagiert darauf.

## LearnTrainer.cs – der Lern-Modus

- `Next()` wählt ein zufälliges Zeichen. Zeichen mit Fehlern kommen in eine Art „Lostopf“, und zwar mehrmals. So werden sie öfter gezogen.
- `CheckCode()` / `CheckChar()` prüfen die Antwort und zählen richtig, falsch und die Serie.
- `Save()` / `Load()` speichern den Punktestand als JSON in `%AppData%\Morse\lernstand.json`.

## Form1.cs – die Oberfläche

- `KeyPreview = true`: Die Form bekommt die Tasten zuerst. So drückt die Leertaste keinen Button.
- `e.SuppressKeyPress = true`: Die Leertaste schreibt kein Leerzeichen in ein Textfeld.
- Wenn ich das Fenster wechsle, während die Taste gedrückt ist (`Deactivate`), wird sie automatisch „losgelassen“.

## Neu: Chat (aus dem Projekt MorseCode übernommen)

| Datei | Was sie macht |
|-------|---------------|
| `ChatNode.cs` | Verbindet zwei Programme direkt per **TCP**. Jede Nachricht wird mit **AES-GCM** verschlüsselt. Der Schlüssel wird aus dem Passwort berechnet. |
| `LanDiscovery.cs` | Ruft alle 2 Sekunden per **UDP-Broadcast** ins WLAN: „Ich bin da!“. So findet man Freunde automatisch. |
| `WebBridge.cs` | Ein kleiner **Webserver** im Programm. Das Handy öffnet die Seite und redet per **WebSocket** mit dem Programm. |
| `WebPage.cs` + `web/index.html` | Die Webseite für das Handy. Sie ist ins Programm eingebaut. |
| `TunnelManager.cs` | Startet *cloudflared*. Das macht aus dem Webserver einen öffentlichen Internet-Link. |
| `QrWindow.cs` | Zeigt den Link als QR-Code (mit dem NuGet-Paket `QRCoder`). |
| `ChatHistory.cs` | Speichert den Chatverlauf als JSON. |
| `AppSettings.cs` | Speichert Tempo, Ton, Lautstärke, Pausen und Namen. |
| `Form1.Chat.cs` | Der Chat-Tab. Er wird im Code gebaut, nicht im Designer. |

**Wie eine Nachricht reist:**
`Eingabe → TextToMorse → ChatNode.Broadcast (verschlüsselt) → Freund → entschlüsseln → MorseToText → Anzeige`

## Neu: Ton-Einstellungen und Wörter

- **Lautstärke:** Die Sinuswelle wird mit einer kleineren Zahl multipliziert, dann ist sie leiser.
- **Pausen ×:** Nur die Pausen zwischen Buchstaben und Wörtern werden länger. Die Punkte und Striche bleiben schnell. Das heisst **Farnsworth-Methode**, damit lernt man den „Klang“ der Buchstaben.
- **Wörter:** `LearnTrainer.Words` ist eine Wortliste. `WordsForLevel()` nimmt nur Wörter, deren Buchstaben man in der Stufe schon kann.

## ChatNode.cs genau erklärt (AP 4)

So reist eine Nachricht von mir zu meinem Freund:

```
"SOS"  →  "... --- ..."  →  Packet("msg", "kosta", "... --- ...")  →  JSON  →  verschlüsselt  →  Base64-Zeile  →  TCP  →  Freund
```

**1. Verbinden.** `Start(port)` öffnet einen `TcpListener`: Das Programm wartet auf Port 47800, ob jemand anklopft.
`ConnectAsync(ip, port)` klopft selbst bei einem Freund an. In beiden Fällen entsteht ein `Peer`, das ist **eine Verbindung zu einem Freund**.

**2. Hallo sagen.** Gleich nach dem Verbinden schickt jeder ein `Packet("hello", Name, "")`.
Erst wenn das „Hallo“ des Freundes ankommt und sich **entschlüsseln** lässt, ist er wirklich drin (`PeerConnected`).

**3. Ein Paket.** `record Packet(string T, string N, string M)`:
- `T` = Typ (`"hello"` oder `"msg"`)
- `N` = Name vom Absender
- `M` = die Nachricht als Morsecode

**4. Der Schlüssel.** `DeriveKey(passwort)` macht aus dem Passwort mit **PBKDF2** einen Schlüssel aus 32 Bytes.
Es rechnet dafür 100 000 Mal hin und her, damit man Passwörter nicht schnell durchprobieren kann.

**5. Verschlüsseln** (`Encrypt`):
- Das Paket wird zu JSON, also zu Bytes.
- Ein **Nonce** (12 zufällige Bytes) sorgt dafür, dass dieselbe Nachricht jedes Mal anders aussieht.
- **AES-GCM** verschlüsselt die Bytes und macht einen **Tag** (16 Bytes). Das ist wie ein Siegel: Wurde etwas verändert oder ist das Passwort falsch, passt das Siegel nicht.
- Nonce + Tag + verschlüsselte Bytes werden mit **Base64** zu Text und als **eine Zeile** verschickt.

**6. Empfangen** (`Peer.RunAsync`): Eine Schleife liest Zeile für Zeile (`ReadLineAsync`) und ruft `Decrypt` auf.
Gibt `Decrypt` `null` zurück, war das Passwort falsch → Verbindung zu.

**7. An alle schicken.** `Broadcast` geht durch alle `Peers` und schickt jedem dasselbe Paket.

**Neu (26.11.):** Wenn ein Freund die Verbindung zumacht, bevor er „Hallo“ gesagt hat, zeigt das Programm jetzt:
„Verbindung abgebrochen – habt ihr beide dasselbe Passwort?“ Vorher hat man nichts gesehen.

### Teste dich selbst
- Warum braucht jede Nachricht ein neues, zufälliges Nonce?
- Was passiert in `RunAsync`, wenn mein Freund ein anderes Passwort hat?
- Wo steht der Port 47800, und was müsste ich ändern, damit das Programm Port 48000 benutzt?

### Antworten (27.11.2026 – geschrieben von Claude, nicht von mir)

**1. Warum ein neues Nonce?**
AES-GCM macht aus Schlüssel + Nonce einen „Zufallsstrom“, der mit der Nachricht gemischt wird.
Würde man dasselbe Nonce zweimal benutzen, wäre auch der Strom zweimal gleich. Ein Angreifer könnte dann
die zwei verschlüsselten Nachrichten miteinander vergleichen und Teile vom Text herausfinden, und er könnte sogar
das Siegel (Tag) fälschen. Mit `RandomNumberGenerator.GetBytes(12)` ist das Nonce jedes Mal neu.
Darum sieht auch dieselbe Nachricht „SOS“ jedes Mal anders aus.

**2. Was passiert in `RunAsync` bei einem anderen Passwort?**
Der Freund hat einen anderen Schlüssel. Seine erste Zeile (das `hello`) lässt sich bei mir nicht entschlüsseln:
Das Siegel passt nicht, `aes.Decrypt` wirft einen Fehler, und `Decrypt` gibt `null` zurück.
Dann setzt `RunAsync` den Grund „abgelehnt: falsches Passwort“, verlässt die Schleife mit `break`,
schliesst die Verbindung (`Close()`) und meldet es über `OnClosed` → `Log` im Chat.
Beim Freund geht die Verbindung zu, bevor ich „Hallo“ sagen konnte. Er sieht darum
„Verbindung abgebrochen – habt ihr beide dasselbe Passwort?“.

**3. Port 48000 statt 47800?**
Der Port steht an einer einzigen Stelle: `ChatNode.cs`, Zeile 15: `public const int DefaultPort = 47800;`.
Man ändert nur diese Zahl auf `48000`. Alle anderen Stellen (`GoOnline` und `ConnectToHost` in `Form1.Chat.cs`)
benutzen `ChatNode.DefaultPort`, und die WLAN-Suche schickt den Port automatisch mit.
**Aber:** Alle Freunde brauchen dann die neue Version, sonst klopfen sie noch an Port 47800 an.
Der Browser-Chat hat einen eigenen Port (`WebBridge.DefaultPort = 47802`), der bleibt gleich.

---

## Deine Aufgabe (ohne KI!)

Such dir **eine** Aufgabe aus und mach sie selbst:

1. **Farbe ändern:** Wenn die Leertaste gedrückt ist, wird das Feld gelb. Mach es grün.
   *Tipp:* `Form1.cs`, suche nach `Color.Gold`.
2. **Eigene Stufe:** Mach im Lern-Modus eine neue Stufe nur mit Ziffern 0–9.
   *Tipp:* `LearnTrainer.cs`, schau dir an, wie `Levels` aufgebaut ist.
3. **Start-Tempo ändern:** Das Programm soll mit Tempo 8 statt 12 starten.
   *Tipp:* `Form1.Designer.cs`, suche nach `numWpm.Value`.
4. **Schneller weiter:** Nach einer richtigen Antwort soll das nächste Zeichen schon nach 0,8 Sekunden kommen.
   *Tipp:* `Form1.cs`, suche nach `timerLearnNext.Interval`.

5. **ChatNode: eigene Meldung (AP 4):** Wenn ein Freund mit falschem Passwort kommt, steht „…abgelehnt: falsches Passwort.“ im Chat. Schreib einen eigenen, freundlicheren Satz.
   *Tipp:* `ChatNode.cs`, suche nach `falsches Passwort`.
6. **ChatNode: Name begrenzen (AP 4):** Ein Freund soll höchstens 20 Zeichen als Namen haben.
   *Tipp:* `ChatNode.cs`, in `OnPacket` steht `peer.Name = packet.N;`. Schau dir an, wie `WebBridge.cs` das mit `Math.Min` macht.

Danach mit **F5** starten und schauen, ob es funktioniert.
