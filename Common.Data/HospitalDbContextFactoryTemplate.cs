// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;

// namespace Common.Data.Data;

//// create another class
//// the class name will be HospitalDbContextFactory instead of HospitalDbContextFactoryTemplate

//// for the appsettings.json file, look inside the appsettings.template.json file !!!!!!!!!!!!!
// public class HospitalDbContextFactoryTemplate : IDesignTimeDbContextFactory<EFHospitalDbContext>
// {
//    public EFHospitalDbContext CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<EFHospitalDbContext>();

// //here change the server name and the database name (if needed)
//        optionsBuilder.UseSqlServer("Server=SERVER_NAME;Database=HospitalManagementDbEF;Trusted_Connection=True;TrustServerCertificate=True;");

// return new EFHospitalDbContext(optionsBuilder.Options);
//    }
// }
