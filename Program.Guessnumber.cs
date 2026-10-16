using System.ComponentModel.Design;

Console.WriteLine("Kannst du die Zahl finden?");

Random random = new Random();
int number = random.Next (1, 101);

Console.WriteLine("Die Zahl ist zwischen 1 und 100.");
int tipp = 0;

while (tipp != number)
{
    tipp = Convert.ToInt32(Console.ReadLine());
    
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


