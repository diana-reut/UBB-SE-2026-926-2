using Common.Data.Entity;

namespace Common.Data.Repository;

public interface IAllergyRepository
{
    Task<List<Allergy>> GetAllergiesAsync();
    Allergy? GetById(int id);
    Task<Allergy?> GetByIdAsync(int id);
}
