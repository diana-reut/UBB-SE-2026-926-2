using Common.API.Services;
using Common.Data.Data;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

using HospitalManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<EFHospitalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(client =>
{
    string apiBaseUri = builder.Configuration["ApiSettings:BaseUri"]
        ?? "http://localhost:5059/";

    client.BaseAddress = new Uri(apiBaseUri);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.Use(async (context, next) =>
{
    PathString path = context.Request.Path;
    bool isAuthenticationPath = path.StartsWithSegments("/Authentication");
    bool isStaticAsset =
        path.StartsWithSegments("/lib")
        || path.StartsWithSegments("/css")
        || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/favicon.ico")
        || path.Value?.Contains('.', StringComparison.Ordinal) == true;

    string? token = context.Session.GetString("AccessToken");
    bool isLoggedIn = !string.IsNullOrWhiteSpace(token);

    if (!isLoggedIn && !isAuthenticationPath && !isStaticAsset)
    {
        context.Response.Redirect("/Authentication/AuthenticationView");
        return;
    }

    if (isLoggedIn && isAuthenticationPath)
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
