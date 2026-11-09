using System.Text;

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
            Console.CursorVisible = false;
            try
            {
                while (hallgatok.Any(h => h.State != "Végzett."))
                {
                    Render(false);
                    Thread.Sleep(100);
                }

                Render(true);
            }
            finally
            {
                Console.CursorVisible = true;
            }
        }, TaskCreationOptions.LongRunning).Start();

        Console.ReadLine();

        void Render(bool finished)
        {
            // shortcut: clips to the terminal size; enlarge it to see every student and full status.
            int width = Math.Max(1, Console.WindowWidth - 1);
            int columnWidth = width / 2;
            int rows = Math.Min(Math.Max(Tagozat.Tagozatok.Count, hallgatok.Count), Math.Max(0, Console.WindowHeight - 3));
            var frame = new StringBuilder();

            frame.AppendLine(("Tagozatok".PadRight(columnWidth) + "Hallgatók").PadRight(width)[..width]);
            for (int i = 0; i < rows; i++)
            {
                string left = "";
                if (i < Tagozat.Tagozatok.Count)
                {
                    var t = Tagozat.Tagozatok[i];
                    left = $"{t.Id}: {t.PresentationCount}, {t.Allapot}, látogatók: {t.JelenlegiHallgatok.Count}";
                }

                string right = i < hallgatok.Count ? $"{hallgatok[i].Id}: {hallgatok[i].State}" : "";
                frame.AppendLine((left.PadRight(columnWidth)[..columnWidth] + right).PadRight(width)[..width]);
            }

            int total = Tagozat.Tagozatok.Sum(t => t.UniqueHallgatok.Count);

            frame.AppendLine($"Az egyedi látogatások összesen: {total}".PadRight(width)[..width]);
            frame.Append((finished ? "Szimuláció vége" : "Szimuláció folyamatban").PadRight(width)[..width]);

            Console.SetCursorPosition(0, 0);
            Console.Write(frame.ToString());
        }
    }
}
