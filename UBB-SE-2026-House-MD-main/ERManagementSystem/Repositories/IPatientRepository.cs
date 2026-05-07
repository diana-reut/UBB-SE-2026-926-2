using System.Threading.Tasks;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public interface IPatientRepository
    {
        void Add(Patient patient);
        Task AddAsync(Patient patient);
        Patient? GetById(string id);
        Task<Patient?> GetByIdAsync(string id);
        void Update(Patient patient);
        Task UpdateAsync(Patient patient);
        void Delete(Patient patient);
        Task DeleteAsync(Patient patient);
    }
}
