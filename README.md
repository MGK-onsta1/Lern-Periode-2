# Lern-Periode-2

# 16.10.2026 | Projekt: Number Guesser

## Heute

Ich habe den Number Guesser neu programmiert. Der Computer denkt sich eine Zahl von 1 bis 100 aus. Ich rate, und der Computer sagt mir, ob die Zahl grösser oder kleiner ist. Mit einer Schleife kann ich so lange raten, bis ich die Zahl finde.

Ich hatte einige Fehler: Ich habe einen falschen Namen benutzt, Semikolons an falschen Stellen gesetzt und `else` falsch geschrieben. Ausserdem lief das Programm endlos, weil die Eingabe an der falschen Stelle stand.

Gelernt habe ich:
- Bei `Next(1, 101)` ist die 101 nicht dabei, die Zahlen gehen also nur bis 100.
- Eine Variable muss ich vor der Schleife erstellen.
- Die Eingabe muss in der Schleife stehen, damit ich jedes Mal neu raten kann.

**Status:** noch nicht fertig – ich muss das Programm noch testen.

Der Computer denkt sich eine Zahl aus. Du rätst, und der Computer sagt dir, ob die Zahl grösser oder kleiner ist. Das geht so lange, bis du die richtige Zahl hast.

<img width="571" height="599" alt="image" src="https://github.com/user-attachments/assets/2d615303-49ba-4135-8483-3bcbb4d00773" />

## Nächstes Mal

- [x] Verstehen, was `int tipp = 0;` bedeutet, und das Programm testen
- [x] Die Versuche zählen und am Ende anzeigen
- [x] Falsche Eingaben abfangen (z. B. Buchstaben)
- [x] Code aufräumen und einen Screenshot machen

Danach will ich einen Budget Manager programmieren.

# 23.10.2026 | Projekt: Number Guesser

## Heute
Ich habe die 4 Punkte vom letzten Mal gelöst. `int tipp = 0;` hab ich verstanden: das ist nur ein Startwert, der sicher nicht die gesuchte Zahl ist, damit die Schleife überhaupt beginnt.

Dann habe ich einen Versuchszähler eingebaut, der bei jedem gültigen Tipp hochzählt. Ausserdem fange ich jetzt falsche Eingaben ab (z. B. Buchstaben) mit `int.TryParse` statt `Convert.ToInt32` – das Programm stürzt also nicht mehr ab, sondern fragt einfach nochmal. Zum Schluss habe ich den Code aufgeräumt und eine unnötige `using`-Zeile entfernt.

**Status:** fertig – das Programm zählt die Versuche und stürzt bei falscher Eingabe nicht mehr ab.

**Programmbeschreibung:** Der Computer denkt sich eine Zahl aus. Du rätst, und der Computer sagt dir, ob die Zahl grösser oder kleiner ist, bis du sie richtig hast. Am Ende siehst du, wie viele Versuche du gebraucht hast.

<img width="495" height="459" alt="image" src="https://github.com/user-attachments/assets/20b59f7f-eeac-47b5-8abf-23cc4529f96d" />


## Nächstes Mal
- [x] Mit dem Budget Manager starten


# 23.10.2026 | Projekt: Budget Manager

## Heute
Heute habe ich mit meinem neuen Projekt, dem Budget Manager, angefangen. Ich mache es als Konsolen-App in C#.

Zuerst habe ich mir überlegt, welche Daten mein Programm braucht, und dafür Variablen erstellt: Einnahmen, Ausgaben, Name vom Sparziel, Betrag vom Sparziel, wie viel ich schon gespart habe und den Dateinamen zum Speichern.

Ich habe gelernt, dass man für Geld `decimal` nimmt und nicht `double`, weil `decimal` genauer rechnet. Ausserdem habe ich gelernt, was `string dateiPfad = "budget.txt";` bedeutet: Die Variable speichert den Namen der Datei, und die Datei liegt im gleichen Ordner wie das Programm.

Dann habe ich den Code geschrieben, der beim Start die Daten aus der Datei lädt. Mit `File.Exists` prüfe ich, ob die Datei schon da ist. Mit `File.ReadAllLines` lese ich alle Zeilen in ein Array, und mit `decimal.Parse` mache ich aus dem Text wieder Zahlen. Dabei habe ich gemerkt, dass die Reihenfolge der Zeilen sehr wichtig ist.

Ich hatte einen Fehler: Ich habe die Variable `zeile` genannt, aber danach `zeilen` geschrieben.

**Status:** Variablen und Laden aus der Datei sind geschrieben. Menü fehlt noch. Es hat sehr lang gedauert, weil ich dieses YouTube Tutorial schauen musste: https://www.youtube.com/watch?v=GsBqMESx1SA

## Programmbeschreibung
Der Budget Manager ist ein Programm, mit dem ich meine Einnahmen und Ausgaben eintragen kann. Es zeigt mir, wie viel Geld ich noch übrig habe. Ich kann auch ein Sparziel festlegen (z. B. ein Auto) und sehen, wie viel ich schon habe und wie viel noch fehlt. Das Programm speichert alles in einer Datei, damit die Daten beim nächsten Mal noch da sind.

<img width="924" height="495" alt="image" src="https://github.com/user-attachments/assets/13df8abf-b6be-4334-858b-ba0ab4d30f0c" />


## Nächstes Mal
- [x] Fehler `zeile` / `zeilen` korrigieren
- [x] Menü mit `while`-Schleife bauen (1 bis 5)
- [x] Falsche Eingaben abfangen
- [x] Einnahme und Ausgabe eintragen
- [x] Übersicht anzeigen

# 30.10.2026 | Projekt: Budget Manager

## Heute
Ich habe die 5 offenen Punkte vom letzten Mal gelöst: den Fehler mit `zeile`/`zeilen` korrigiert, ein Menü mit einer `while`-Schleife (Auswahl 1 bis 5) gebaut, falsche Eingaben mit `TryParse` abgefangen, und das Eintragen von Einnahmen und Ausgaben sowie die Übersicht eingebaut.

Ich habe mich aber entschieden, das Projekt jetzt zu pausieren, weil ich zuerst am Morsecode-Projekt weiterarbeiten will. Den Budget Manager mache ich später fertig.

**Status:** pausiert – Lösung für die 5 Punkte ist vorhanden, muss ich nur noch einbauen und testen, wenn ich weitermache.

**Programmbeschreibung:** Der Budget Manager ist ein Programm, mit dem ich meine Einnahmen und Ausgaben eintragen kann. Es zeigt mir, wie viel Geld ich noch übrig habe. Ich kann auch ein Sparziel festlegen und sehen, wie viel ich schon habe und wie viel noch fehlt.

<img width="842" height="863" alt="image" src="https://github.com/user-attachments/assets/58e29a97-327a-413e-bc3e-997ac3cc3439" />


## Nächstes Mal
- [x] Lösung einbauen und testen
- [x] Code aufräumen
- [x] Mit dem Morsecode-Projekt starten

# 30.10.2026 | Projekt: MorseCode

## Heute
Ich habe mit dem Morsecode-Projekt angefangen und die Codetabelle erstellt, die Buchstaben, Zahlen und Sonderzeichen auf Morsecode abbildet.

**Status:** gestartet – Grundgerüst (Form, Codetabelle, Audio-Datei) steht. Ich habe diese Video angeschaut: https://www.youtube.com/watch?v=ObcGBT4ZWEU&t=127s, https://www.youtube.com/watch?v=dYkpTvgCclo&pp=ygUQTW9yc2UgQ29kZSBpbiBDI9IHCQkrDAGHKiGM7w%3D%3D
<img width="1403" height="572" alt="image" src="https://github.com/user-attachments/assets/fb953ac5-acbb-4fd2-8da7-fe118a4f5166" />


## Ausblick
Als Nächstes geht's an die eigentliche Übersetzungslogik und die Benutzeroberfläche, danach an Lern-Modus und Chat-Funktionen.

## Nächstes Mal (5 Arbeitspakete)
- [x] Übersetzer Text ↔ Morse mit der Codetabelle fertigstellen
- [x] Form1-Oberfläche bauen (Textfeld, Übersetzen-Button, Anzeige)
- [x] MorseAudio fertigstellen (Signalton für Punkt/Strich abspielen)
- [x] Eingabe über die Leertaste einbauen (kurz = Punkt, lang = Strich)
- [x] Lern-Modus einbauen

# Lern-Periode-2

# 06.11.2026 | Projekt: Morse-Übersetzer

## Heute

Ich habe den Morse-Übersetzer fertig gemacht. Er übersetzt Text in Morsecode und zurück und kann den Code als Ton abspielen. Mit der Leertaste kann ich selbst morsen: kurz ist ein Punkt, lang ein Strich. Im Lern-Modus übe ich die Buchstaben. Dabei hatte ich grosse Hilfe von KI.

Ich hatte einige Fehler: Die Codetabelle stand in der falschen Datei, und die Umlaute waren kaputt.

Gelernt habe ich:
- Ein Strich ist 3-mal so lang wie ein Punkt.
- Mit einer `Stopwatch` messe ich, wie lange die Leertaste gedrückt ist.

**Status:** fertig – den Ton und die Leertaste muss ich noch testen.

<img width="721" height="483" alt="image" src="https://github.com/user-attachments/assets/70dfb54e-bc95-4315-9f4d-7cfded9393fd" />


## Nächstes Mal

- [x] Den Ton und die Leertaste testen
- [x] Mit Hilfe der KI den Code besser verstehen
- [x] Den Punktestand im Lern-Modus speicher     
- [x] Einen kleinen Teil des Codes selbst ohne KI ändern

Danach will ich im Lern-Modus ganze Wörter üben.

# 06.11.2026 (Nachmittag) | Projekt: Morse-Übersetzer

## Heute

Ich habe die Ziele von heute Morgen bearbeitet. Ich habe den Ton und die Leertaste getestet, und der Lern-Modus speichert jetzt meinen Punktestand. Auch die neue Erklärung zum Code habe ich gelesen. Dabei hatte ich wieder grosse Hilfe von KI.

Ich hatte einen grossen Fehler: Die Leertaste hat nicht funktioniert. Das Feld blieb gelb, und es kam kein Buchstabe. Der Grund war, dass das Stoppen des Tons fast 1 Sekunde gedauert hat und das Programm so lange eingefroren war.

Gelernt habe ich:
- `SoundPlayer.Stop()` ist zu langsam. Mit `waveOut` von Windows stoppt der Ton sofort.
- Mit JSON kann ich den Punktestand in einer Datei speichern und beim Start wieder laden.

**Status:** fertig – die Leertaste erkennt jetzt „SOS SOS“ richtig.

<img width="720" height="483" alt="image" src="https://github.com/user-attachments/assets/67422c05-33b5-4bb3-b03d-e26307acc461" />


## Nächstes Mal

- [ ] Einen kleinen Teil des Codes selbst ohne KI ändern
- [ ] Mit Hilfe der KI die Datei `MorseAudio.cs` besser verstehen
- [ ] Den Ton selbst anhören und die beste Tonhöhe finden
- [ ] Im Lern-Modus alle 5 Stufen ausprobieren

Danach will ich im Lern-Modus ganze Wörter üben.


