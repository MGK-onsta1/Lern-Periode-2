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
- [ ] Mit dem Budget Manager starten
