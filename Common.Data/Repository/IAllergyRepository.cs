using Common.Data.Entity;

namespace Common.Data.Repository;

public interface IAllergyRepository
{
    IEnumerable<Allergy> GetAllergies();
    Task<IEnumerable<Allergy>> GetAllergiesAsync();
    Allergy? GetById(int id);
    Task<Allergy?> GetByIdAsync(int id);
}
