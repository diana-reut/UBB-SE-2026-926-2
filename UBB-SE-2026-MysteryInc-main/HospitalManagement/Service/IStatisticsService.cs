using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IStatisticsService
{
    public Dictionary<string, int> GetActiveVsArchivedRatio();
    public Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync();

    public Dictionary<string, int> GetAgeDistribution();
    public Task<Dictionary<string, int>> GetAgeDistributionAsync();

    public Dictionary<string, int> GetConsultationDistribution();
    public Task<Dictionary<string, int>> GetConsultationDistributionAsync();

    public Dictionary<string, int> GetMostPrescribedMeds();
    public Task<Dictionary<string, int>> GetMostPrescribedMedsAsync();

    public Dictionary<string, int> GetPatientGenderDistribution();
    public Task<Dictionary<string, int>> GetPatientGenderDistributionAsync();

    public Dictionary<string, int> GetPatientsByBloodType();
    public Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync();

    public Dictionary<string, int> GetPatientsByRh();
    public Task<Dictionary<string, int>> GetPatientsByRhAsync();

    public Dictionary<string, int> GetTopDiagnoses();
    public Task<Dictionary<string, int>> GetTopDiagnosesAsync();
}
