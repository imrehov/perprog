using System.Collections.Concurrent;

namespace oltopont;

public class Patient
{
    private static int _appointment = 9 * 60;
    private static int _id = 1;
    private static int _waitingCount;

    public int PatientId { get; } = _id++;
    public int AppointmentTime { get; }
    public int ArrivalTime { get; }
    public string Status { get; set; } = "Még nem érkezett meg";
    public ManualResetEventSlim Vaccinated { get; } = new(false);

    public static BlockingCollection<Patient> AdminList { get; } = new();
    public static int WaitingCount => Volatile.Read(ref _waitingCount);
    public static string AdminStatus { get; private set; } = "Páciensre vár";

    public Patient()
    {
        AppointmentTime = _appointment;

        _appointment += 5;
        
        int offset = Random.Shared.Next(5) == 0 ? 0 : Random.Shared.Next(1, 16) * (Random.Shared.Next(2) == 0 ? -1 : 1);
        ArrivalTime = AppointmentTime + offset;
    }

    public void DoWork()
    {
        int delay = (ArrivalTime - Program.StartMinute) * Program.MillisecondsPerMinute - (int)Program.Clock.ElapsedMilliseconds;

        Thread.Sleep(Math.Max(0, delay));
        Status = "Adminisztrációra vár";
        AdminList.Add(this);

        Vaccinated.Wait();
        Interlocked.Increment(ref _waitingCount);

        Status = "Váróban pihen";
        Program.SleepMinutes(Random.Shared.Next(15, 31));

        Interlocked.Decrement(ref _waitingCount);
        Status = "Hazament";
    }

    public static void DoAdministration()
    {
        for (int i = 0; i < Program.PatientCount; i++)
        {
            AdminStatus = "Páciensre vár";

            var patient = AdminList.Take();
            AdminStatus = $"{patient.PatientId}. páciens adminisztrációja";

            patient.Status = "Adminisztráció";
            Program.SleepMinutes(Random.Shared.Next(1, 4));

            var doctor = VaccineDoc.AllDocs[Random.Shared.Next(VaccineDoc.AllDocs.Count)];
            patient.Status = $"{doctor.DocId}. orvosra vár";
            doctor.Patients.Add(patient);
        }

        AdminStatus = "Végzett";

        foreach (var doctor in VaccineDoc.AllDocs)
            doctor.Patients.CompleteAdding();
    }
}
