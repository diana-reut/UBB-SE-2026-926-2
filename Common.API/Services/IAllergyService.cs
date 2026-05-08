using Common.Data.Entity;

namespace Common.API.Services;

public interface IAllergyService
{
    public Task<List<Allergy>> GetAllergiesAsync();
}
