using System;
using System.IO;

string dateiPfad = "budget.txt";

decimal einnahmen = 0;
decimal ausgaben = 0;
string sparzielName = "";
decimal sparzielBetrag = 0;
decimal bereitsGespart = 0;

if (File.Exists(dateiPfad))
{
    string[] zeilen = File.ReadAllLines(dateiPfad);
    einnahmen = decimal.Parse(zeilen[0]);
    ausgaben = decimal.Parse(zeilen[1]);
    sparzielName = zeilen[2];
    sparzielBetrag = decimal.Parse(zeilen[3]);
    bereitsGespart = decimal.Parse(zeilen[4]);
}

int auswahl = 0;

while (auswahl != 5)
{
    Console.WriteLine();
    Console.WriteLine("=== Budget Manager ===");
    Console.WriteLine("1) Einnahme eintragen");
    Console.WriteLine("2) Ausgabe eintragen");
    Console.WriteLine("3) Übersicht anzeigen");
    Console.WriteLine("4) Sparziel festlegen");
    Console.WriteLine("5) Beenden");
    Console.Write("Deine Wahl: ");

    if (!int.TryParse(Console.ReadLine(), out auswahl) || auswahl < 1 || auswahl > 5)
    {
        Console.WriteLine("Bitte eine Zahl von 1 bis 5 eingeben.");
        auswahl = 0;
        continue;
    }

    if (auswahl == 1)
    {
        Console.Write("Wie viel hast du eingenommen? ");
        if (decimal.TryParse(Console.ReadLine(), out decimal betrag))
            einnahmen += betrag;
        else
            Console.WriteLine("Das war keine gültige Zahl.");
    }
    else if (auswahl == 2)
    {
        Console.Write("Wie viel hast du ausgegeben? ");
        if (decimal.TryParse(Console.ReadLine(), out decimal betrag))
            ausgaben += betrag;
        else
            Console.WriteLine("Das war keine gültige Zahl.");
    }
    else if (auswahl == 3)
    {
        Console.WriteLine($"Einnahmen: {einnahmen}");
        Console.WriteLine($"Ausgaben: {ausgaben}");
        Console.WriteLine($"Übrig: {einnahmen - ausgaben}");
        if (sparzielName != "")
            Console.WriteLine($"Sparziel '{sparzielName}': {bereitsGespart} von {sparzielBetrag}");
    }
    else if (auswahl == 4)
    {
        Console.Write("Name vom Sparziel: ");
        sparzielName = Console.ReadLine();
        Console.Write("Betrag vom Sparziel: ");
        decimal.TryParse(Console.ReadLine(), out sparzielBetrag);
        Console.Write("Wie viel hast du schon gespart? ");
        decimal.TryParse(Console.ReadLine(), out bereitsGespart);
    }
}

File.WriteAllLines(dateiPfad, new string[] { einnahmen.ToString(), ausgaben.ToString(), sparzielName, sparzielBetrag.ToString(), bereitsGespart.ToString() });