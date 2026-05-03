using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using HospitalManagement.Service;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Runtime.CompilerServices;

namespace HospitalManagement.ViewModel;

internal class StatisticsViewModel : INotifyPropertyChanged
{
    private readonly IStatisticsService _statisticsService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ISeries> GenderSeries { get; set; } = [];

    public ObservableCollection<ISeries> AgeSeries { get; set; } = [];

    public ObservableCollection<Axis> AgeXAxes { get; } = [];

    public ObservableCollection<ISeries> CartesianSeries { get; } = [];


    public ObservableCollection<string> MenuOptions { get; } = [
        "Patient Distribution",
        "Consultation Source",
        "Top Diagnoses",
        "Top Medications",
        "Demographics"
    ];

    private StatisticsType _selectedType;

    public StatisticsType SelectedType
    {
        get => _selectedType;

        set
        {
            _selectedType = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ISeries> CurrentSeries { get; } = [];

    public ObservableCollection<Axis> XAxes { get; } = [];

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;

        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ISeries> PatientDistributionSeries { get; set; } = [];

    public ObservableCollection<ISeries> ConsultationSourceSeries { get; set; } = [];

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public void LoadDataForSelection(string selection)
    {
        ErrorMessage = "";

        StatisticsType result = GetByString(selection);

        switch (result)
        {
            case StatisticsType.PatientDistribution:
            {
                LoadPatientDistribution();
                break;
            }

            case StatisticsType.ConsultationSource:
            {
                LoadConsultationSources();
                break;
            }

            case StatisticsType.TopDiagnoses:
            {
                LoadTopDiagnoses();
                break;
            }

            case StatisticsType.TopMedications:
            {
                LoadTopMedications();
                break;
            }

            case StatisticsType.Demographics:
            {
                LoadDemographics();
                break;
            }

            default:
            {
                ErrorMessage = "Unknown statistic selected";
                CurrentSeries.Clear();
                break;
            }
        }
    }

    public static StatisticsType GetByString(string value)
    {
        return value switch
        {
            "Patient Distribution" => StatisticsType.PatientDistribution,
            "Consultation Source" => StatisticsType.ConsultationSource,
            "Top Diagnoses" => StatisticsType.TopDiagnoses,
            "Top Medications" => StatisticsType.TopMedications,
            "Demographics" => StatisticsType.Demographics,
            _ => StatisticsType.PatientDistribution,
        };
    }

    /// <summary>
    /// VM1: LoadPatientDistribution
    /// Calls StatisticsService.GetActiveVsArchivedRatio() and populates a pie chart.
    /// showing the ratio of active patients to those who are no longer under current care.
    /// </summary>
    public void LoadPatientDistribution()
    {
        try
        {
            Dictionary<string, int> data = _statisticsService.GetActiveVsArchivedRatio();

            if (data is null || data.Count == 0)
            {
                ErrorMessage = "No patient distribution data available";
                CurrentSeries.Clear();
                return;
            }

            var series = new List<ISeries>();
            foreach (KeyValuePair<string, int> kvp in data)
            {
                if (kvp.Value >= 0) // Ensure non-negative values
                {
                    series.Add(new PieSeries<int>
                    {
                        Name = kvp.Key ?? "Unknown",
                        Values = new int[] { kvp.Value, },
                    });
                }
            }

            CurrentSeries.Clear();

            foreach (ISeries s in series)
            {
                CurrentSeries.Add(s);
            }

            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load patient distribution: {ex.Message}";
            CurrentSeries.Clear();
        }
    }

    // VM2: LoadConsultationSources
    // Implements LoadConsultationSources() to format the distribution of ER visits vs. appointments.
    // for graphical display with a pie chart. Maps raw Map keys (e.g., "ER", "Staff") into UI-friendly.
    // labels (e.g., "Emergency Department", "Scheduled Appointments").
    private void LoadConsultationSources()
    {
        try
        {
            Dictionary<string, int> data = _statisticsService.GetConsultationDistribution();

            if (data is null || data.Count == 0)
            {
                ErrorMessage = "No consultation source data available";
                CurrentSeries.Clear();
                return;
            }

            var series = new List<ISeries>();
            foreach (KeyValuePair<string, int> kvp in data)
            {
                if (kvp.Value >= 0) // Ensure non-negative values
                {
                    series.Add(new PieSeries<int>
                    {
                        Name = MapConsultationSourceLabel(kvp.Key ?? "Unknown"),
                        Values = new int[] { kvp.Value, },
                    });
                }
            }

            CurrentSeries.Clear();

            foreach (ISeries s in series)
            {
                CurrentSeries.Add(s);
            }

            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            // Error Handling: Populate ErrorMessage property to alert user
            ErrorMessage = $"Failed to load consultation sources: {ex.Message}";
            CurrentSeries.Clear();
        }
    }

    // Helper method to map consultation source keys to UI-friendly labels.

    private static string MapConsultationSourceLabel(string sourceType)
    {
        return sourceType switch
        {
            "ER" => "Emergency Department",
            "Emergency" => "Emergency Department",
            "Scheduled" => "Scheduled Appointments",
            "Appointment" => "Scheduled Appointments",
            _ => sourceType, // Return original if no mapping found
        };
    }

    /// <summary>
    /// VM3: LoadTopDiagnoses
    /// Implements LoadTopDiagnoses() to fetch from StatisticsService.GetTopDiagnoses().
    /// the most common clinical diagnoses for bar chart rendering.
    /// </summary>
    private void LoadTopDiagnoses()
    {
        try
        {
            Dictionary<string, int> data = _statisticsService.GetTopDiagnoses();

            if (data is null || data.Count == 0)
            {
                ErrorMessage = "No diagnosis data available";
                CartesianSeries.Clear();
                XAxes.Clear();
                return;
            }

            // Validate and filter data with non-negative values
            var validatedData = data
                .Where(kvp => kvp.Value >= 0 && !string.IsNullOrWhiteSpace(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (validatedData.Count == 0)
            {
                ErrorMessage = "No valid diagnosis data available";
                CartesianSeries.Clear();
                XAxes.Clear();
                return;
            }

            string[] newLabels = [.. validatedData.Keys];
            int[] newValues = [.. validatedData.Values];

            XAxes.Clear();

            XAxes.Add(new Axis
            {
                Labels = newLabels,
                LabelsRotation = 15,
            });

            CartesianSeries.Clear();
            CartesianSeries.Add(new ColumnSeries<int>
            {
                Values = newValues,
                Name = "Diagnoses",
            });

            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            // Error Handling: Catch any service-level exceptions
            ErrorMessage = $"Failed to load top diagnoses: {ex.Message}";
            CartesianSeries.Clear();
            XAxes.Clear();
        }
    }

    /// <summary>
    /// VM4: LoadTopMedications
    /// Implements LoadTopMedications() to retrieve frequency data for prescribed medications.
    /// for bar chart rendering. Calls statisticsService.GetMostPrescribedMeds().
    /// </summary>
    private void LoadTopMedications()
    {
        try
        {
            Dictionary<string, int> data = _statisticsService.GetMostPrescribedMeds();

            if (data is null || data.Count == 0)
            {
                ErrorMessage = "No medication data available";
                CartesianSeries.Clear();
                XAxes.Clear();
                return;
            }

            // Validate and filter data with non-negative values
            var validatedData = data
                .Where(kvp => kvp.Value >= 0 && !string.IsNullOrWhiteSpace(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (validatedData.Count == 0)
            {
                ErrorMessage = "No valid medication data available";
                CartesianSeries.Clear();
                XAxes.Clear();
                return;
            }

            // Set X-axis labels from medication names
            XAxes.Clear();

            XAxes.Add(new Axis
            {
                Labels = validatedData.Keys.ToArray(),
            });

            // Create column series for bar chart with validated values
            CartesianSeries.Clear();

            CartesianSeries.Add(new ColumnSeries<int>
            {
                Values = validatedData.Values.ToArray(),
                Name = "Prescriptions",
            });

            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            // Error Handling: Catch any service-level exceptions
            ErrorMessage = $"Failed to load top medications: {ex.Message}";
            CartesianSeries.Clear();
            XAxes.Clear();
        }
    }

    /// <summary>
    /// VM5: LoadDemographics
    /// Implements LoadDemographics() to fetch age "buckets" (0-17, 18-64, 65+) and sex distribution data.
    /// by calling GetAgeDistribution() and GetPatientGenderDistribution() from StatisticsService.
    /// Maps the SexEnum counts into a two-segment pie chart and the age buckets to a bar chart.
    /// </summary>
    private void LoadDemographics()
    {
        try
        {
            // 1. Fetch Gender Data (Pie Chart) - Map SexEnum counts into pie chart
            Dictionary<string, int> genderData = _statisticsService.GetPatientGenderDistribution();

            if (genderData is null || genderData.Count == 0)
            {
                ErrorMessage = "No demographic data available";
                GenderSeries.Clear();
                AgeSeries.Clear();
                AgeXAxes.Clear();
                return;
            }

            GenderSeries.Clear();
            foreach (KeyValuePair<string, int> entry in genderData)
            {
                if (entry.Value >= 0 && !string.IsNullOrWhiteSpace(entry.Key))
                {
                    GenderSeries.Add(new PieSeries<int>
                    {
                        Name = MapSexEnumLabel(entry.Key),
                        Values = new int[] { entry.Value, },
                    });
                }
            }

            // 2. Fetch Age Data (Bar Chart) - Map age buckets to bar chart
            Dictionary<string, int>? ageData = _statisticsService.GetAgeDistribution();

            if (ageData is null || ageData.Count == 0)
            {
                ErrorMessage = "No age distribution data available";
                AgeSeries.Clear();
                AgeXAxes.Clear();
                return;
            }

            // Validate age data
            var validatedAgeData = ageData
                .Where(kvp => kvp.Value >= 0 && !string.IsNullOrWhiteSpace(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            AgeSeries.Clear();
            if (validatedAgeData.Count > 0)
            {
                AgeSeries.Add(new ColumnSeries<int>
                {
                    Name = "Patients",
                    Values = validatedAgeData.Values.ToArray(),
                });

                AgeXAxes.Clear();

                AgeXAxes.Add(new Axis
                {
                    Labels = validatedAgeData.Keys.ToArray(),
                });
            }

            // Trigger UI update for the Axes
            OnPropertyChanged(nameof(AgeXAxes));
            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            // Error Handling: Catch and display service-level exceptions
            ErrorMessage = $"Failed to load demographics: {ex.Message}";
            GenderSeries.Clear();
            AgeSeries.Clear();
            AgeXAxes.Clear();
        }
    }

    // Helper method to map SexEnum display values to UI-friendly labels
    private static string MapSexEnumLabel(string sexEnumValue)
    {
        return sexEnumValue switch
        {
            "Male" => "Male",
            "Female" => "Female",
            _ => sexEnumValue,
        };
    }
}

