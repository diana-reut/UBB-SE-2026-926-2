using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Common.Data.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<EFHospitalDbContext>
{
    public EFHospitalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EFHospitalDbContext>();

        optionsBuilder.UseSqlServer("Server=localhost;Database=HospitalManagementDbEF;Trusted_Connection=True;TrustServerCertificate=True;");
        
        return new EFHospitalDbContext(optionsBuilder.Options);
    }
}