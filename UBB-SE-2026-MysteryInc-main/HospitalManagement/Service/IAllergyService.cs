using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Service;

internal interface IAllergyService
{
    public IEnumerable<Allergy> GetAllergies();
}
