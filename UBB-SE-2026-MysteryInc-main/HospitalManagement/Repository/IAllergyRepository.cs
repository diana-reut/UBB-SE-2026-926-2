using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface IAllergyRepository
{
    public IEnumerable<Allergy> GetAllergies();
}

