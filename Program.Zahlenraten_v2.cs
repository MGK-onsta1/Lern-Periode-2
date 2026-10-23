Console.WriteLine("Kannst du die Zahl finden?");

Random random = new Random();
int number = random.Next(1, 101);

Console.WriteLine("Die Zahl ist zwischen 1 und 100.");
int tipp = 0;
int versuche = 0;

while (tipp != number)
{
    Console.Write("Dein Tipp: ");
    string eingabe = Console.ReadLine();

    if (!int.TryParse(eingabe, out tipp))
    {
        Console.WriteLine("Das war keine gültige Zahl. Versuch's nochmal.");
        continue;
    }

    versuche++;

    if (tipp > number)
    {
        Console.WriteLine("Die Zahl ist kleiner.");
    }
    else if (tipp < number)
    {
        Console.WriteLine("Die Zahl ist grösser.");
    }
    else
    {
        Console.WriteLine("Richtig!");
    }
}

Console.WriteLine($"Du hast {versuche} Versuche gebraucht!");
