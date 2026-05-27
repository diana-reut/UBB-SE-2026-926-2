using System.Security.Cryptography;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Common.Data.Integration;
using HospitalManagement.Web.Models.Prescription;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]
public class PrescriptionController : Controller
{
    private readonly IPrescriptionApiClient prescriptionApiClient;
    private const int PageSize = 9;

    public PrescriptionController(IPrescriptionApiClient prescriptionApiClient)
    {
        this.prescriptionApiClient = prescriptionApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Feed(
        string? searchIdText,
        string? searchName,
        string? searchMedication,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page = 1,
        int? returnPatientId = null,
        CancellationToken cancellationToken = default)
    {
        var model = new PrescriptionFeedViewModel
        {
            SearchIdText = searchIdText,
            SearchName = searchName,
            SearchMedication = searchMedication,
            DateFrom = dateFrom,
            DateTo = dateTo,
            CurrentPage = page,
            ReturnPatientId = returnPatientId
        };

        try
        {
            List<Prescription> prescriptions;

            bool hasFilter =
                !string.IsNullOrWhiteSpace(searchIdText) ||
                !string.IsNullOrWhiteSpace(searchName) ||
                !string.IsNullOrWhiteSpace(searchMedication) ||
                dateFrom.HasValue ||
                dateTo.HasValue;

            if (hasFilter)
            {
                var filter = new PrescriptionFilter
                {
                    PrescriptionId = TryParseInt(searchIdText),
                    PatientName = searchName,
                    DoctorName = searchName,
                    MedName = searchMedication,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                prescriptions = (await prescriptionApiClient.ApplyFilterAsync(filter, cancellationToken))
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            else
            {
                prescriptions = await prescriptionApiClient.GetLatestPrescriptionsAsync(PageSize, page, cancellationToken);
            }

            if (prescriptions.Count == 0)
            {
                model.InfoMessage = "No prescriptions found.";
            }

            model.Prescriptions = prescriptions.Select(p => new PrescriptionCardViewModel
            {
                Id = p.Id,
                PatientName = p.PatientName ?? string.Empty,
                DoctorName = ResolveDoctorName(p.DoctorName),
                DoctorNotes = p.DoctorNotes ?? string.Empty,
                Date = p.Date,
                Items = p.MedicationList?.Select(i => new PrescriptionItemViewModel
                {
                    MedName = i.MedName,
                    Quantity = i.Quantity
                }).ToList() ?? new List<PrescriptionItemViewModel>()
            }).ToList();
        }
        catch (Exception ex)
        {
            model.InfoMessage = "Error loading prescriptions: " + ex.Message;
        }

        return View(model);
    }

    private static string ResolveDoctorName(string? doctorName)
    {
        if (!string.IsNullOrEmpty(doctorName) &&
            !doctorName.Contains("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return doctorName;
        }

        var fakeDoctors = MockDoctorProvider.FakeDoctors;
        int index = RandomNumberGenerator.GetInt32(fakeDoctors.Count);
        var doc = fakeDoctors[index];
        return $"Dr. {doc.FirstName} {doc.LastName}";
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out int result) ? result : null;
}
