using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "HospitalManagement.Web.Antiforgery";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/AuthenticationView";
        options.AccessDeniedPath = "/Authentication/AuthenticationView";
        options.Cookie.Name = "HospitalManagement.Web.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddTransient<AuthTokenForwardingHandler>();
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IPatientApiClient, PatientApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<AuthTokenForwardingHandler>();
builder.Services.AddHttpClient<IAllergyApiClient, AllergyApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<AuthTokenForwardingHandler>();
builder.Services.AddHttpClient<IGhostApiClient, GhostApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<AuthTokenForwardingHandler>();
builder.Services.AddHttpClient<IStatisticsApiClient, StatisticsApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<AuthTokenForwardingHandler>();

builder.Services.AddHttpClient<IErWorkflowApiClient, ErWorkflowApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IAddictDetectionApiClient, AddictDetectionApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<IPrescriptionApiClient, PrescriptionApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client))
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddSingleton<IErStaffService, ErStaffService>();

builder.Services.AddHttpClient<IBloodCompatibilityApiClient, BloodCompatibilityApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client)).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<ITransplantApiClient, TransplantApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client)).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddSingleton<IAppointmentImportProvider, MockAppointmentImportProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void ConfigureHospitalApiClient(IConfiguration configuration, HttpClient client)
{
    string apiBaseUri = configuration["ApiSettings:BaseUri"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUri is not configured.");

    client.BaseAddress = new Uri(apiBaseUri);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
}
