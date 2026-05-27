using Common.API.Services;
using Common.Data;
using Common.Data.Entity;
using Common.Data.Repository;

namespace Common.API.Services;

public class AllergyService : IAllergyService
{
    private readonly IAllergyRepository repository;

    public AllergyService(IAllergyRepository allergyRepository)
    {
        repository = allergyRepository;
    }

    public Task<List<Allergy>> GetAllergiesAsync()
    {
        return repository.GetAllergiesAsync();
    }
}
