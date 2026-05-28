namespace HospitalManagement.Web.Models.Statistics;

public enum StatisticsType
{
    PatientDistribution,
    ConsultationSource,
    TopDiagnoses,
    TopMedications,
    Demographics,
}

public class StatisticsModel
{
    public StatisticsType SelectedType { get; set; } = StatisticsType.PatientDistribution;
    public Dictionary<string, int> PrimaryData { get; set; } = new ();
    public Dictionary<string, int> SecondaryData { get; set; } = new ();
    public string? ErrorMessage { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
