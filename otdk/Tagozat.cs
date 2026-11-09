using otdk.Enums;
using otdk.Utils;
using System.Data;

namespace otdk;

public class Tagozat
{
    public TagozatAllapot Allapot { get; private set; }

    public int PresentationCount { get; private set; } = 1;
    public static List<Tagozat> Tagozatok { get; set; } = new();

    public List<Hallgato> JelenlegiHallgatok { get; private set; } = new();

    public List<Hallgato> UniqueHallgatok { get; private set; } = new();

    public int Id { get; private set; }

    public object Lock { get; set; } = new();

    public Tagozat()
    {
        Id = Utils.Utils.TagozatId++;
        Tagozatok.Add(this);
    }

    public void DoWork()
    {
        while (PresentationCount < 9)
        {
            // kb 1percben felkészül
            Allapot = TagozatAllapot.Preparation;
            Thread.Sleep(Random.Shared.Next(750, 1500));

            // kb 15p előadás, tilos járkálni
            Allapot = TagozatAllapot.Presentation;
            Thread.Sleep(Random.Shared.Next(10000, 20000));

            //max 10p diskusszió, lehet járkálni
            Allapot = TagozatAllapot.Discussion;

            lock (Lock) Monitor.PulseAll(Lock);

            Thread.Sleep(Random.Shared.Next(5000, 10001));


            PresentationCount++;
        }

        Allapot = TagozatAllapot.Finished;

        lock (Lock) Monitor.PulseAll(Lock);
    }


}
