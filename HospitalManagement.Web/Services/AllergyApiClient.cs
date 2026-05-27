using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public class AllergyApiClient : HospitalApiClientBase, IAllergyApiClient
{
    public AllergyApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        : base(httpClient, httpContextAccessor)
    {
    }

    public async Task<List<Allergy>> GetAllergiesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Allergy>>("api/allergies", cancellationToken) ?? new List<Allergy>();

    public Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllergiesAsync(cancellationToken);
}
