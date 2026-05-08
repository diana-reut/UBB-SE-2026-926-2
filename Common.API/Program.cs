using Common.API.Service;
using Common.API.Services;
using Common.Data.Data;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddLogging();

// Section for services guys
builder.Services.AddDbContext<EFHospitalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAllergyService, AllergyService>();

builder.Services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

// This hook runs exactly when the server is ready
app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = string.Join(", ", app.Urls);
    Console.WriteLine("----------------------------------------------");
    Console.WriteLine($"🚀 Allergy API is running on: {urls}");
    Console.WriteLine("----------------------------------------------");
});

app.Run();
