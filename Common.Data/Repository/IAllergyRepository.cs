using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

public interface IAllergyRepository
{
    IEnumerable<Allergy> GetAllergies();
    Task<IEnumerable<Allergy>> GetAllergiesAsync();
    Allergy? GetById(int id);
    Task<Allergy?> GetByIdAsync(int id);
}
