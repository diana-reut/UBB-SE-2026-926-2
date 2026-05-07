using Common.API.Service;
using Common.API.Services;
using Common.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddLogging();

// Section for services guys
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAllergyService, AllergyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run();
