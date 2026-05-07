using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using HospitalManagement.Service;
using System.Collections.ObjectModel;
using HospitalManagement.View;
using System.Threading.Tasks;
using Common.Data.Entity.Enums;
using Common.Data.Entity;
using HospitalManagement.Proxy.AllergyProxy;

namespace HospitalManagement.ViewModel;


internal class MedicalHistoryDialogViewModel
{
    private readonly IAllergyProxy _allergyProxy;

    public ObservableCollection<AllergyEntry> AllergyList { get; } = [];

    public List<Allergy> AvailableAllergies { get; private set; } = [];

    public MedicalHistory? MedicalHistory { get; private set; }

    public MedicalHistoryDialogViewModel(IAllergyProxy allergyService)
    {
        _allergyProxy = allergyService;
    }

    public void LoadAllergies()
    {
        LoadAllergiesAsync().GetAwaiter().GetResult();
    }

    public async Task LoadAllergiesAsync()
    {
        AvailableAllergies = [.. await _allergyProxy.GetAllAsync()];
    }

    public bool TryAddAllergy(Allergy? selectedAllergy, string? severity)
    {
        if (selectedAllergy is null)
        {
            return false;
        }

        if (AllergyList.Any(a => a.Allergy.AllergyId == selectedAllergy.AllergyId))
        {
            return false;
        }

        AllergyList.Add(new AllergyEntry
        {
            Allergy = selectedAllergy,
            Severity = string.IsNullOrWhiteSpace(severity) ? "Mild" : severity,
        });

        return true;
    }

    public void RemoveAllergy(AllergyEntry allergyEntry)
    {
        if (allergyEntry is not null)
        {
            AllergyList.Remove(allergyEntry);
        }
    }

    public bool TryCreateMedicalHistory(
        string? bloodTypeText,
        string? rhText,
        string? chronicConditionsText)
    {
        if (!Enum.TryParse(bloodTypeText, out BloodType bloodType))
        {
            return false;
        }

        Rh rh = rhText == "Negative" ? Rh.Negative : Rh.Positive;

        List<string> chronicConditions = [];

        if (!string.IsNullOrWhiteSpace(chronicConditionsText))
        {
            chronicConditions = [.. chronicConditionsText
                .Split(',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))];
        }

        List<(Allergy, string)> allergies =
            [.. AllergyList.Select(entry => (entry.Allergy, entry.Severity))];

        MedicalHistory = new MedicalHistory
        {
            BloodType = bloodType,
            Rh = rh,
            ChronicConditions = chronicConditions,
            Allergies = allergies,
        };

        return true;
    }
}
