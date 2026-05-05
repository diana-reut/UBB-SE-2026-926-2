using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Repository;

internal interface IAllergyRepository
{
    Task<IEnumerable<Allergy>> GetAllergiesAsync();
    Task<Allergy?> GetByIdAsync(int id);
}

