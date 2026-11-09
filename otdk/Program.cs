namespace otdk;

class Program
{
    static void Main(string[] args)
    {
        Enumerable.Range(1, 5)
            .Select(x => new Tagozat())
            .Select(t => new Task(() =>
            {
                t.DoWork();
            }, TaskCreationOptions.LongRunning))
            .ToList()
            .ForEach(t => t.Start());


        List<Hallgato> hallgatok = Enumerable.Range(1, 30)
            .Select(x => new Hallgato())
            .ToList();

        hallgatok.Select(x => new Task(() =>
        {
            x.DoWork();
        }, TaskCreationOptions.LongRunning))
            .ToList()
            .ForEach(t => t.Start());

        new Task(() =>
        {
            while (hallgatok.Any( h => h.State != "Végzett"))
            {
                Console.Clear();
                foreach (var t in Tagozat.Tagozatok)
                {
                    Console.WriteLine($"{t.Id}: {t.PresentationCount}, {t.Allapot}, latogatok: {t.JelenlegiHallgatok.Count}");

                    foreach (var h in hallgatok)
                    {

                        Console.WriteLine($"{h.Id}: {h.State}");
                    }

                    Console.WriteLine();

                    Console.WriteLine("Az egyedi latogatasok osszesen: " + Tagozat.Tagozatok.Select(t => t.UniqueHallgatok.Count).Sum());

                    Thread.Sleep(100);
                }
            }

            Console.Clear();
            foreach (var t in Tagozat.Tagozatok)
            {
                Console.WriteLine($"{t.Id}: {t.PresentationCount}, {t.Allapot}, latogatok: {t.JelenlegiHallgatok.Count}");
            }

            foreach (var h in hallgatok)
            { 

                Console.WriteLine($"{h.Id}: {h.State}");
            }

            Console.WriteLine();

            Console.WriteLine("Az egyedi latogatasok osszesen: " + Tagozat.Tagozatok.Select(t => t.UniqueHallgatok.Count).Sum());

            Console.WriteLine("Szimuláció vége");
        }, TaskCreationOptions.LongRunning).Start();

        Console.ReadLine();
    }
}
