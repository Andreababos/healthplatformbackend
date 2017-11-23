using System.Linq;

namespace RS.NetDiet.Therapist.DataModel
{
    public class PatientsRepository
    {
        public string GetTherapistIdOfPatient(long patientPk)
        {
            using (var db  = new NdEdModel())
            {
                var patient = db.Patients.FirstOrDefault(x => x.PK == patientPk);
                if (patient == null)
                {
                    return null;
                }

                return patient.TherapistId;
            }
        }
    }
}
