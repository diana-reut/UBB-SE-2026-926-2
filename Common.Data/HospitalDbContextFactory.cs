using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Common.Data.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<EFHospitalDbContext>
{
    public EFHospitalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EFHospitalDbContext>();

        optionsBuilder.UseSqlServer("Server=DESKTOP-G90IV3T\\MSSQLSERVER01;Database=HospitalManagementDb;Trusted_Connection=True;TrustServerCertificate=True;");
        
        return new EFHospitalDbContext(optionsBuilder.Options);
    }
}