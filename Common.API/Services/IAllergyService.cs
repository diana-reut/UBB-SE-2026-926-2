using Common.Data.Entity;

namespace Common.API.Services;

internal interface IAllergyService
{
    public Task<IEnumerable<Allergy>> GetAllergiesAsync();
}
