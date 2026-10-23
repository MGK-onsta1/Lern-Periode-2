using System.Globalization;

decimal einnahmen = 0;
decimal ausgaben = 0;
string zielName = "";
decimal zielBetrag = 0;
decimal gespart = 0;
string dateiPfad = "budget.txt";

if (File.Exists(dateiPfad))
{
    string[] zeilen = File.ReadAllLines(dateiPfad);
    einnahmen = decimal.Parse(zeilen[0], CultureInfo.InvariantCulture);
    ausgaben = decimal.Parse(zeilen[1], CultureInfo.InvariantCulture);
    zielName = zeilen[2];
    zielBetrag = decimal.Parse(zeilen[3], CultureInfo.InvariantCulture);
    gespart = decimal.Parse(zeilen[4], CultureInfo.InvariantCulture);
    Console.WriteLine("Daten vom letzten Mal geladen.");
}
