using otdk.Enums;

namespace otdk;

public class Hallgato
{
    public int Erdeklodes { get; private set; } = 100;

    public int Id { get;  set; }

    public string State { get; set; }

    public Hallgato()
    {
        Id = Utils.Utils.HallgatoId++;
    }

    public void DoWork()
    {
        while (Tagozat.Tagozatok.Any(t => t.Allapot != TagozatAllapot.Finished))
        {
            var t = Tagozat.Tagozatok.Where(x => x.Allapot != TagozatAllapot.Finished)
                .OrderBy(x => Random.Shared.Next(1, 100))
                .FirstOrDefault();

            if (t == null)
                break;

            State = $"{t.Id} tagozatba szeretne belépni.";

            lock (t.Lock)
            {
                bool tried = false;

                while (t.Allapot == TagozatAllapot.Presentation || t.JelenlegiHallgatok.Count == 10)
                {
                    if (tried) break;

                    Monitor.Wait(t.Lock);
                    tried = true;
                }
                
                if (t.Allapot == TagozatAllapot.Presentation || t.JelenlegiHallgatok.Count == 10)
                {
                    continue;
                }

                t.JelenlegiHallgatok.Add(this);
                if (!t.UniqueHallgatok.Contains(this)) t.UniqueHallgatok.Add(this);
            }

            Erdeklodes = 100;

            while (Erdeklodes > 0 && t.Allapot != TagozatAllapot.Finished)
            {
                Thread.Sleep(1000);

                Erdeklodes -= Random.Shared.Next(1, Math.Min(5, Erdeklodes) + 1);

                State = $"{t.Id} tagozatban ül, érdeklődés: {Erdeklodes}";
            }

            State = $"{t.Id} tagozatból szeretne kilépni";

            lock (t.Lock)
            {
                while (t.Allapot == TagozatAllapot.Presentation)
                {
                    Monitor.Wait(t.Lock);
                }

                t.JelenlegiHallgatok.Remove(this);
                Monitor.Pulse(t.Lock);
            }
        }

        State = $"Végzett.";
    }

}
