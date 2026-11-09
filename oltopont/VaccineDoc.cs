using System.Collections.Concurrent;

namespace oltopont;

public class VaccineDoc
{
    private static readonly CountdownEvent FinishedVaccinating = new(3);

    public int DocId { get; }
    public string Status { get; private set; } = "Páciensre vár";
    public int VaccinatedCount { get; private set; }
    public static List<VaccineDoc> AllDocs { get; } = new();
    public BlockingCollection<Patient> Patients { get; } = new();

    public VaccineDoc()
    {
        DocId = AllDocs.Count + 1;
        AllDocs.Add(this);
    }

    public void DoWork()
    {
        int remainingDoses = Random.Shared.Next(5, 8);
        foreach (var patient in Patients.GetConsumingEnumerable())
        {
            if (remainingDoses == 0)
            {
                Status = "Oltóanyagot vált";
                Program.SleepMinutes(Random.Shared.Next(1, 4));
                remainingDoses = Random.Shared.Next(5, 8);
            }

            Status = $"{patient.PatientId}. pácienst oltja";
            patient.Status = $"Oltás a(z) {DocId}. orvosnál";
            Program.SleepMinutes(Random.Shared.Next(3, 8));
            remainingDoses--;
            VaccinatedCount++;
            patient.Vaccinated.Set();
            Status = "Páciensre vár";
        }

        Status = "A többi orvos befejezésére vár";
        FinishedVaccinating.Signal();
        FinishedVaccinating.Wait();
        Status = "Végzett";
    }
}
