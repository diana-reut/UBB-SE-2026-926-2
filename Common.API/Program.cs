using System.Text;
using Common.API.Service;
using Common.API.Services;
using Common.Data.Data;
using Common.Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

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
builder.Services.AddScoped<ITriageDecisionService, TriageDecisionService>();

builder.Services.AddScoped<ITriageParametersRepository, TriageParametersRepository>();
builder.Services.AddScoped<ITriageParametersService, TriageParametersService>();

builder.Services.AddScoped<IExaminationRepository, ExaminationRepository>();
builder.Services.AddScoped<IExaminationService, ExaminationService>();

builder.Services.AddScoped<ITransferLogRepository, TransferLogRepository>();
builder.Services.AddScoped<ITransferLogService, TransferLogService>();

builder.Services.AddScoped<ITransplantRepository, TransplantRepository>();
builder.Services.AddScoped<ITransplantService, TransplantService>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();

builder.Services.AddScoped<IBillingService, BillingService>();

builder.Services.AddScoped<IAddictDetectionService, AddictDetectionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT authentication
string jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured in appsettings.");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HospitalAPI";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HospitalClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hospital Management API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here. Obtain one from POST /api/auth/login."
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            []
        }
    });
});

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

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
