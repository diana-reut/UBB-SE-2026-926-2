using Common.Data.Entity;

namespace Common.API.Services;

public interface IAllergyService
{
    public Task<IEnumerable<Allergy>> GetAllergiesAsync();
}
