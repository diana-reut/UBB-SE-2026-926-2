using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;

namespace ERManagementSystem.Proxy.PatientProxy;

public class PatientProxy : ProxyBase, IPatientProxy
{
    private const string BaseUri = "api/patients";

    public PatientProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<Patient?> GetByCnpAsync(string cnp)
    {
        SearchPatientsDto dto = new SearchPatientsDto()
        {
            Cnp = cnp,
        };

        Patient[] results = await PostAsync<SearchPatientsDto, Patient[]>($"{BaseUri}/search", dto) ?? Array.Empty<Patient>();
        return results.FirstOrDefault(patient => string.Equals(patient.Patient_ID, cnp, StringComparison.Ordinal));
    }

    public Task<bool> ExistsAsync(string cnp)
    {
        return GetAsync<bool>($"{BaseUri}/exists/{cnp}");
    }

    public async Task<Patient> CreatePatientAsync(CreatePatientDto dto)
    {
        return await PostAsync<CreatePatientDto, Patient>(BaseUri, dto)
            ?? throw new InvalidOperationException("Failed to create patient: no response from server.");
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        Patient existingPatient = await GetByCnpAsync(patient.Patient_ID)
            ?? throw new InvalidOperationException($"Patient with CNP {patient.Patient_ID} was not found.");

        UpdatePatientDto dto = new UpdatePatientDto()
        {
            FirstName = patient.First_Name,
            LastName = patient.Last_Name,
            Cnp = patient.Patient_ID,
            Dob = patient.Date_of_Birth,
            Sex = patient.Sex,
            PhoneNo = patient.Phone,
            EmergencyContact = patient.Emergency_Contact,
            IsDonor = patient.IsDonor,
            Transferred = patient.Transferred,
            Dod = patient.Dod,
            IsArchived = patient.IsArchived,
        };

        await PutAsync($"{BaseUri}/{existingPatient.Id}", dto);
    }
}
