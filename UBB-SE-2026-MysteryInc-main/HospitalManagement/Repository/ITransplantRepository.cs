using HospitalManagement.Entity;
using System.Collections.Generic;

namespace HospitalManagement.Repository;

internal interface ITransplantRepository
{
    public void Add(Transplant transplant);

    public List<Transplant> GetByDonorId(int donorId);

    public Transplant? GetById(int id);

    public List<Transplant> GetByReceiverId(int receiverId);

    public List<Transplant> GetTopMatches(string organType);

    public List<Transplant> GetWaitingByOrgan(string organType);

    public void Update(int id, int donorId, float score);
}
