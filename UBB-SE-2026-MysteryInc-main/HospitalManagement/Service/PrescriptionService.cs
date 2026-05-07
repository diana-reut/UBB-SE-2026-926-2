using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Entity;
using Common.Data.Repository;
using Common.Data.Integration;

namespace HospitalManagement.Service;

internal class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository ?? throw new ArgumentNullException(nameof(prescriptionRepository));
    }

    public List<Prescription> GetLatestPrescriptions(int n, int page)
    {
        return _prescriptionRepository.GetTopN(n, page);
    }

    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page)
    {
        return _prescriptionRepository.GetTopNAsync(n, page);
    }

    public Prescription GetPrescriptionDetails(int id)
    {
        var filter = new PrescriptionFilter { PrescriptionId = id, };
        Prescription? prescription = _prescriptionRepository.GetFiltered(filter).FirstOrDefault() ?? throw new ArgumentException($"Prescription with ID {id} does not exist.");

        return prescription;
    }

    public async Task<Prescription> GetPrescriptionDetailsAsync(int id)
    {
        var filter = new PrescriptionFilter { PrescriptionId = id, };
        List<Prescription> prescriptions = await _prescriptionRepository.GetFilteredAsync(filter);
        return prescriptions.FirstOrDefault() ?? throw new ArgumentException($"Prescription with ID {id} does not exist.");
    }

    public List<Prescription> ApplyFilter(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return _prescriptionRepository.GetTopN(20, 1);
        }

        try
        {
            return _prescriptionRepository.GetFiltered(filter);
        }
        catch (Exception)
        {
            throw new MyNotImplementedException("The medication search could not be completed at this time due to high system load or complex parameters. Please try simplifying your search or try again later.");
        }
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter)
    {
        if (filter is null)
        {
            return await _prescriptionRepository.GetTopNAsync(20, 1);
        }

        try
        {
            return await _prescriptionRepository.GetFilteredAsync(filter);
        }
        catch (Exception)
        {
            throw new MyNotImplementedException("The medication search could not be completed at this time due to high system load or complex parameters. Please try simplifying your search or try again later.");
        }
    }
}
