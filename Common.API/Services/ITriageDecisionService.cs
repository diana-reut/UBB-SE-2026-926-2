using Common.Data.Models;

namespace Common.API.Services;

public interface ITriageDecisionService
{
    int CalculateTriageLevel(Triage_Parameters parameters);
    string DetermineSpecialization(Triage_Parameters parameters);
}
