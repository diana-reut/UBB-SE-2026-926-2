using Common.Data.Models;

namespace Common.API.Services;

public class TriageDecisionService : ITriageDecisionService
{
    public int CalculateTriageLevel(Triage_Parameters parameters)
    {
        parameters.ValidateParameters();

        if (parameters.Consciousness == 3 ||
            parameters.Breathing == 3 ||
            parameters.Injury_Type == 3 ||
            parameters.Bleeding == 3)
        {
            return 1;
        }

        int severityScore =
            (parameters.Consciousness * 3) +
            (parameters.Breathing * 3) +
            (parameters.Bleeding * 2) +
            (parameters.Injury_Type * 2) +
            parameters.Pain_Level;

        if (severityScore >= 20)
        {
            return 2;
        }

        if (severityScore >= 16)
        {
            return 3;
        }

        if (severityScore >= 12)
        {
            return 4;
        }

        return 5;
    }

    public string DetermineSpecialization(Triage_Parameters parameters)
    {
        parameters.ValidateParameters();

        if (parameters.Bleeding == 3 || parameters.Injury_Type == 3)
        {
            return "General Surgery";
        }

        if (parameters.Injury_Type == 2)
        {
            return "Orthopedics";
        }

        if (parameters.Breathing == 2)
        {
            return "Pulmonology";
        }

        if (parameters.Consciousness is 2 or 3)
        {
            return "Neurology";
        }

        return "Emergency Medicine";
    }
}
