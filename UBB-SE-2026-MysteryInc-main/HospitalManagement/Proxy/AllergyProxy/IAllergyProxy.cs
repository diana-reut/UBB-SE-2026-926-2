using Common.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Proxy.AllergyProxy;

internal interface IAllergyProxy
{
    Task<List<Allergy>> GetAllAsync();
}