using System;
using System.Collections.Generic;
using System.Text;

namespace oltopont
{
    public class Patient
    {
        private static int _appointment = 9 * 60;
        private static int _id = 1;
        public int PatientId { get; private set; }

        public int AppointmentTime { get; private set; }

        public string Status { get; set; }

        public static List<Patient> AdminList { get; private set; } = new();

        private object _lock = new();

        public Patient()
        {
            PatientId = _id++;

            int r = Random.Shared.Next(60);

            if (r < 12)
            {
                AppointmentTime = _appointment;
            }
            else
            {
                if (r < 30)
                {
                    AppointmentTime = _appointment - r / 4;
                }
                else
                {
                    AppointmentTime = _appointment + r / 4;
                }
            }
            _appointment += 5;

        }

        public void DoWork()
        {
            
            lock (_lock)
            {
                AdminList.Add(this);
                Status = $"Arrived at administration";

                Thread.Sleep(Random.Shared.Next(1000, 3001));

                Status = $"Finished at administration";
                AdminList.Remove(this);
            }

            var point = VaccineDoc.AllDocs.OrderBy(x => Random.Shared.Next(100)).First();

            point.Patients.Enqueue(this);

            Status = $"Went to doc nr {point.DocId}.";


        }
    }
}
