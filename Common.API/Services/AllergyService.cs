using Common.API.Services;
using Common.Data;
using Common.Data.Entity;
using Common.Data.Repository;

namespace Common.API.Service;

internal class AllergyService : IAllergyService
{
    private readonly IAllergyRepository _repository;

    public AllergyService(IAllergyRepository allergyRepository)
    {
        _repository = allergyRepository;
    }

    public Task<IEnumerable<Allergy>> GetAllergiesAsync()
    {
        return _repository.GetAllergiesAsync();
    }
}
