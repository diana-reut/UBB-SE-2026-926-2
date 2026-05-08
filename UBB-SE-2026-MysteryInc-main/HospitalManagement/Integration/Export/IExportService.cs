using System.Threading.Tasks;

namespace HospitalManagement.Integration.Export;

internal interface IExportService
{
    public Task<string> ExportRecordToPDFAsync(int recordId);
}
