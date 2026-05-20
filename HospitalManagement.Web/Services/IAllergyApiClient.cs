using Common.Data.Entity;

namespace HospitalManagement.Web.Services;

public interface IAllergyApiClient
{
    Task<List<Allergy>> GetAllergiesAsync(CancellationToken cancellationToken = default);
}
