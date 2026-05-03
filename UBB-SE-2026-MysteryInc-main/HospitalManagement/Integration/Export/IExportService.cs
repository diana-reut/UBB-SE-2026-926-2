namespace HospitalManagement.Integration.Export;

internal interface IExportService
{
    public string ExportRecordToPDF(int recordId);
}
