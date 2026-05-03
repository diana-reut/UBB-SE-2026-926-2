using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IPatientRepository
    {
        void Add(Patient patient);
        Patient? GetById(string id);
        void Update(Patient patient);
        void Delete(Patient patient);
    }
}
