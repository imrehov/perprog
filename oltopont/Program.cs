using System.Diagnostics;
using System.Text;

namespace oltopont;

internal class Program
{
    internal const int PatientCount = 60;
    internal const int StartMinute = 8 * 60 + 45;
    internal const int MillisecondsPerMinute = 1000;
    internal static readonly Stopwatch Clock = new();

    internal static void SleepMinutes(int minutes) => Thread.Sleep(minutes * MillisecondsPerMinute);

    static void Main()
    {
        var doctors = Enumerable.Range(1, 3).Select(_ => new VaccineDoc()).ToList();
        var patients = Enumerable.Range(1, PatientCount).Select(_ => new Patient()).ToList();
        Clock.Start();

        var tasks = doctors.Select(d => Task.Factory.StartNew(d.DoWork, TaskCreationOptions.LongRunning)).ToList();
        tasks.Add(Task.Factory.StartNew(Patient.DoAdministration, TaskCreationOptions.LongRunning));
        tasks.AddRange(patients.Select(p => Task.Factory.StartNew(p.DoWork, TaskCreationOptions.LongRunning)));
        var completion = Task.WhenAll(tasks);

        if (!Console.IsOutputRedirected) Console.CursorVisible = false;
        try
        {
            while (!completion.IsCompleted)
            {
                if (!Console.IsOutputRedirected) Render(false);
                Thread.Sleep(100);
            }
            completion.GetAwaiter().GetResult();
            Clock.Stop();
            Debug.Assert(patients.All(p => p.Status == "Hazament"));
            Debug.Assert(doctors.Sum(d => d.VaccinatedCount) == PatientCount);
            Debug.Assert(doctors.All(d => d.Status == "Végzett" && d.Patients.IsCompleted));
            Debug.Assert(Patient.WaitingCount == 0 && Patient.AdminList.Count == 0);
            Render(true);
        }
        finally
        {
            if (!Console.IsOutputRedirected) Console.CursorVisible = true;
        }

        void Render(bool finished)
        {
            int width = Console.IsOutputRedirected ? 120 : Math.Max(1, Console.WindowWidth - 1);
            int columnWidth = width / 2;
            int rows = Console.IsOutputRedirected ? 30 : Math.Min(30, Math.Max(0, Console.WindowHeight - 7));
            var frame = new StringBuilder();
            var time = TimeSpan.FromMinutes(StartMinute + Clock.Elapsed.TotalMilliseconds / MillisecondsPerMinute);

            void Line(string text) => frame.AppendLine(text.PadRight(width)[..width]);

            Line($"Idő: {time:hh\\:mm} | Váróban: {Patient.WaitingCount} fő | 1 perc = 1 másodperc");
            foreach (var doctor in doctors)
                Line($"{doctor.DocId}. orvos: {doctor.Status} | Beoltva: {doctor.VaccinatedCount} | Sorban: {doctor.Patients.Count}");
            Line($"Ügyintéző: {Patient.AdminStatus} | Sorban: {Patient.AdminList.Count}");
            Line("Páciensek 1–30".PadRight(columnWidth) + "Páciensek 31–60");
            for (int i = 0; i < rows; i++)
                Line(Describe(patients[i]).PadRight(columnWidth)[..columnWidth] + Describe(patients[i + 30]));

            // shortcut: clips to the terminal; use at least 120 columns and 37 rows for the full view.
            string footer = finished ? $"Szimuláció vége: {time:hh\\:mm}" : "Szimuláció folyamatban";
            if (rows < 30) footer += " | Mind a 60 pácienshez legalább 37 sor szükséges.";
            frame.Append(footer.PadRight(width)[..width]);
            if (!Console.IsOutputRedirected) Console.SetCursorPosition(0, 0);
            Console.Write(frame.ToString());
            if (finished) Console.WriteLine();
        }
    }

    private static string Describe(Patient patient) =>
        $"{patient.PatientId,2} ({TimeSpan.FromMinutes(patient.AppointmentTime):hh\\:mm}, érk. {TimeSpan.FromMinutes(patient.ArrivalTime):hh\\:mm}): {patient.Status}";
}
