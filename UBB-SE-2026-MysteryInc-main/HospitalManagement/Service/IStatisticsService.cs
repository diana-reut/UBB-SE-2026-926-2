using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IStatisticsService
{
    public Dictionary<string, int> GetActiveVsArchivedRatio();

    public Dictionary<string, int> GetAgeDistribution();

    public Dictionary<string, int> GetConsultationDistribution();

    public Dictionary<string, int> GetMostPrescribedMeds();

    public Dictionary<string, int> GetPatientGenderDistribution();

    public Dictionary<string, int> GetPatientsByBloodType();

    public Dictionary<string, int> GetPatientsByRh();

    public Dictionary<string, int> GetTopDiagnoses();
}
