using Common.Data.Entity;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy;

public class AllergyProxy : ProxyBase
{
    private const string BaseUri = "api/allergies";

    public AllergyProxy(HttpClient httpClient)
        : base(httpClient) { }

    // GET ALL
    public async Task<List<Allergy>> GetAllAsync()
    {
        return await GetAsync<List<Allergy>>(BaseUri) ?? [];
    }
}
