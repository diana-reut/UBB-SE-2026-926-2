using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.StatisticsProxy;

internal interface IStatisticsProxy
{
    Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync();
    Task<Dictionary<string, int>> GetAgeDistributionAsync();
    Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync();
    Task<Dictionary<string, int>> GetPatientsByRhAsync();
    Task<Dictionary<string, int>> GetPatientGenderDistributionAsync();
    Task<Dictionary<string, int>> GetConsultationDistributionAsync();
    Task<Dictionary<string, int>> GetTopDiagnosesAsync();
    Task<Dictionary<string, int>> GetMostPrescribedMedsAsync();
}