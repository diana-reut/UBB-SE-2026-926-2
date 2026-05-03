using HospitalManagement.Database;
using HospitalManagement.Entity;
using System.Collections.Generic;
using System.Data.Common;

namespace HospitalManagement.Repository;

internal class AllergyRepository : IAllergyRepository
{
    private readonly IDbContext _context;

    public AllergyRepository(IDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Allergy> GetAllergies()
    {
        const string Query = "SELECT AllergyId, AllergyName, AllergyType, AllergyCategory FROM Allergy";
        using DbDataReader reader = _context.ExecuteQuery(Query);
        var allergies = new List<Allergy>();
        while (reader.Read())
        {
            allergies.Add(MapToAllergy(reader));
        }

        return allergies;
    }


    private static Allergy MapToAllergy(DbDataReader reader)
    {
        return new Allergy
        {
            AllergyId = (int)reader["AllergyId"],
            AllergyName = reader["AllergyName"]?.ToString() ?? "",
            AllergyType = reader["AllergyType"]?.ToString(),
            AllergyCategory = reader["AllergyCategory"]?.ToString(),
        };
    }
}

