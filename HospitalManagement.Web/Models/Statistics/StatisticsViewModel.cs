namespace HospitalManagement.Web.Models.Statistics;

public class StatisticsViewModel
{
    public string SelectedKey { get; set; } = "patient-distribution";
    public string Title { get; set; } = "Patient Distribution";
    public bool IsDemographics { get; set; }
    public List<StatisticOptionViewModel> Options { get; set; } = new ();
    public List<StatisticDataPointViewModel> PrimaryData { get; set; } = new ();
    public List<StatisticDataPointViewModel> SecondaryData { get; set; } = new ();
    public string? ErrorMessage { get; set; }

    public static StatisticsViewModel FromModel(StatisticsModel model)
    {
        string selectedKey = ToKey(model.SelectedType);

        return new StatisticsViewModel
        {
            SelectedKey = selectedKey,
            Title = ToTitle(model.SelectedType),
            IsDemographics = model.SelectedType == StatisticsType.Demographics,
            ErrorMessage = model.ErrorMessage,
            Options = GetOptions(selectedKey),
            PrimaryData = MapData(model.PrimaryData),
            SecondaryData = MapData(model.SecondaryData),
        };
    }

    public static StatisticsType FromKey(string? key)
    {
        return key switch
        {
            "consultation-source" => StatisticsType.ConsultationSource,
            "top-diagnoses" => StatisticsType.TopDiagnoses,
            "top-medications" => StatisticsType.TopMedications,
            "demographics" => StatisticsType.Demographics,
            _ => StatisticsType.PatientDistribution,
        };
    }

    private static string ToKey(StatisticsType type)
    {
        return type switch
        {
            StatisticsType.ConsultationSource => "consultation-source",
            StatisticsType.TopDiagnoses => "top-diagnoses",
            StatisticsType.TopMedications => "top-medications",
            StatisticsType.Demographics => "demographics",
            _ => "patient-distribution",
        };
    }

    private static string ToTitle(StatisticsType type)
    {
        return type switch
        {
            StatisticsType.ConsultationSource => "Consultation Source",
            StatisticsType.TopDiagnoses => "Top Diagnoses",
            StatisticsType.TopMedications => "Top Medications",
            StatisticsType.Demographics => "Demographics",
            _ => "Patient Distribution",
        };
    }

    private static List<StatisticOptionViewModel> GetOptions(string selectedKey)
    {
        string[] keys =
        [
            "patient-distribution",
            "consultation-source",
            "top-diagnoses",
            "top-medications",
            "demographics",
        ];

        return keys
            .Select(key => new StatisticOptionViewModel
            {
                Key = key,
                Label = ToTitle(FromKey(key)),
                IsSelected = key == selectedKey,
            })
            .ToList();
    }

    private static List<StatisticDataPointViewModel> MapData(Dictionary<string, int> data)
    {
        int maxValue = data.Count == 0 ? 0 : data.Values.Max();

        return data
            .Where(item => item.Value >= 0)
            .OrderByDescending(item => item.Value)
            .Select(item => new StatisticDataPointViewModel
            {
                Label = MapLabel(item.Key),
                Value = item.Value,
                WidthPercent = maxValue == 0
                    ? 0
                    : Math.Max(4, (int)Math.Round(item.Value * 100m / maxValue)),
            })
            .ToList();
    }

    private static string MapLabel(string label)
    {
        return label switch
        {
            "ER" or "Emergency" => "Emergency Department",
            "Scheduled" or "Appointment" => "Scheduled Appointments",
            _ => label,
        };
    }
}

public class StatisticOptionViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class StatisticDataPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int WidthPercent { get; set; }
}
