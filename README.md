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

Ich habe gelernt, dass man für Geld `decimal` nimmt und nicht `double`, weil `decimal` genauer rechnet. Ausserdem habe ich gelernt, was `string dateiPfad = "budget.txt";` bedeutet: Die Variable speichert den Namen der Datei, und die Datei liegt im gleichen Ordner wie das Programm (relativer Pfad).

Dann habe ich den Code geschrieben, der beim Start die Daten aus der Datei lädt. Mit `File.Exists` prüfe ich, ob die Datei schon da ist. Mit `File.ReadAllLines` lese ich alle Zeilen in ein Array, und mit `decimal.Parse` mache ich aus dem Text wieder Zahlen. Dabei habe ich gemerkt, dass die Reihenfolge der Zeilen sehr wichtig ist.

Ich hatte einen Fehler: Ich habe die Variable `zeile` genannt, aber danach `zeilen` geschrieben. C# merkt schon den kleinsten Unterschied im Namen.

**Status:** Variablen und Laden aus der Datei sind geschrieben. Menü fehlt noch.

## Programmbeschreibung
Der Budget Manager ist ein Programm, mit dem ich meine Einnahmen und Ausgaben eintragen kann. Es zeigt mir, wie viel Geld ich noch übrig habe. Ich kann auch ein Sparziel festlegen (z. B. ein Auto) und sehen, wie viel ich schon habe und wie viel noch fehlt. Das Programm speichert alles in einer Datei, damit die Daten beim nächsten Mal noch da sind.

<img width="924" height="495" alt="image" src="https://github.com/user-attachments/assets/13df8abf-b6be-4334-858b-ba0ab4d30f0c" />


## Nächstes Mal
- [ ] Fehler `zeile` / `zeilen` korrigieren
- [ ] Menü mit `while`-Schleife bauen (1 bis 5)
- [ ] Falsche Eingaben abfangen
- [ ] Einnahme und Ausgabe eintragen
- [ ] Übersicht anzeigen
