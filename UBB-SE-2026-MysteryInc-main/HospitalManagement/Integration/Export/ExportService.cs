using System.Collections.Generic;
using Common.Data.Entity;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using HospitalManagement.Proxy.PatientProxy;

namespace HospitalManagement.Integration.Export;

internal class ExportService : IExportService
{
    private readonly IPatientProxy _patientProxy;

    public ExportService(IPatientProxy patientProxy)
    {
        _patientProxy = patientProxy;
    }

    public async Task<string> ExportRecordToPDFAsync(int recordId)
    {
        RecordExportDataDto exportData = await _patientProxy.GetRecordExportDataAsync(recordId);
        return PDFGenerator.GenerateRecordPDF(exportData.Record, exportData.Patient, exportData.Prescription, exportData.Items);
    }
}
