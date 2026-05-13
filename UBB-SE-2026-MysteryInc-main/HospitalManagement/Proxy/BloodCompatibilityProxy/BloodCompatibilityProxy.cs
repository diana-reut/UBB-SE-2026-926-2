using Common.Data.Entity;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace HospitalManagement.Proxy.BloodCompatibilityProxy;

internal class BloodCompatibilityProxy
    : ProxyBase, IBloodCompatibilityProxy
{
    private const string BaseUri = "api/bloodcompatibilities";

    public BloodCompatibilityProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId)
    {
        return await PostAsync<object, List<Patient>>(
                   $"{BaseUri}/top-donors",
                   new { RecipientId = recipientId })
               ?? [];
    }
}