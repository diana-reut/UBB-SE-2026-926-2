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
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IERVisitRepository, ERVisitRepository>();
builder.Services.AddScoped<IERVisitService, ERVisitService>();
builder.Services.AddScoped<IERRoomRepository, ERRoomRepository>();
builder.Services.AddScoped<IERRoomService, ERRoomService>();
builder.Services.AddScoped<ITriageRepository, TriageRepository>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<ITriageParametersRepository, TriageParametersRepository>();
builder.Services.AddScoped<ITriageParametersService, TriageParametersService>();
builder.Services.AddScoped<IExaminationRepository, ExaminationRepository>();
builder.Services.AddScoped<IExaminationService, ExaminationService>();
builder.Services.AddScoped<ITransferLogRepository, TransferLogRepository>();
builder.Services.AddScoped<ITransferLogService, TransferLogService>();
builder.Services.AddScoped<ITransplantRepository, TransplantRepository>();
builder.Services.AddScoped<ITransplantsService, TransplantsService>();

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
