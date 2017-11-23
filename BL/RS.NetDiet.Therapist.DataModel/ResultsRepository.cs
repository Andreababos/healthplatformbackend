using RS.NetDiet.Therapist.DataModel.DTOs;
using System.Linq;

namespace RS.NetDiet.Therapist.DataModel
{
    public class ResultsRepository
    {
        public void CreateResultEntry(long patientPk, string originalFileName, string generatedFileName)
        {
            using (var db = new NdEdModel())
            {
                db.Results.Add(new Result()
                {
                    GeneratedFileName = generatedFileName,
                    OriginalFileName = originalFileName,
                    PatientPK = patientPk
                });
                db.SaveChanges();
            }
        }

        public FileInfoDto GetFileInfo(long filePk)
        {
            using (var db = new NdEdModel())
            {
                var result = db.Results.FirstOrDefault(x => x.PK == filePk);
                if (result == null)
                {
                    return null;
                }

                return new FileInfoDto()
                {
                    GeneratedFileName = result.GeneratedFileName,
                    OriginalFileName = result.OriginalFileName,
                    TherapistId = result.Patient.TherapistId
                };
            }
        } 
    }
}
