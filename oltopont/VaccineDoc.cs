using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace oltopont
{
    public class VaccineDoc
    {
        private static int _id {  get;  set; }

        public int DocId { get; private set; }

        public static List<VaccineDoc> AllDocs { get; set; } = new();

        public ConcurrentQueue<Patient> Patients { get; set; } = new();

        public VaccineDoc()
        {
            DocId = _id++;

            AllDocs.Add(this);
        }

        public void DoWork()
        {
            while(!Patients.IsEmpty)
            {

                Patient next;
                while (!Patients.TryDequeue(out next))
                {
                    Thread.Sleep(1000);
                }

                next.Status = $"Patient nr{next.PatientId} getting vaccinated";

                Thread.Sleep()
            }
        }
    }
}
