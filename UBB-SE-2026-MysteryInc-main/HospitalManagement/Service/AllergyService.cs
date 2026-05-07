using HospitalManagement.Entity;
using HospitalManagement.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalManagement.Service;

internal class AllergyService : IAllergyService
{
    private readonly IAllergyRepository _repository;

    public AllergyService(IAllergyRepository allergyRepository)
    {
        _repository = allergyRepository;
    }

    public IEnumerable<Allergy> GetAllergies()
    {
        return _repository.GetAllergies();
    }

    public Task<IEnumerable<Allergy>> GetAllergiesAsync()
    {
        return _repository.GetAllergiesAsync();
    }
}
