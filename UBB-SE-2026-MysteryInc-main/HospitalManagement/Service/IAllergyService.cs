using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal interface IAllergyService
{
    public IEnumerable<Allergy> GetAllergies();
    public Task<IEnumerable<Allergy>> GetAllergiesAsync();
}
