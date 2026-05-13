using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.API.Services;

public interface IStatisticsService
{
    public Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync();

    public Task<Dictionary<string, int>> GetAgeDistributionAsync();

    public Task<Dictionary<string, int>> GetConsultationDistributionAsync();

    public Task<Dictionary<string, int>> GetMostPrescribedMedsAsync();

    public Task<Dictionary<string, int>> GetPatientGenderDistributionAsync();

    public Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync();

    public Task<Dictionary<string, int>> GetPatientsByRhAsync();

    public Task<Dictionary<string, int>> GetTopDiagnosesAsync();
}
