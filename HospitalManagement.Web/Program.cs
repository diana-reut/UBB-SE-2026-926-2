using HospitalManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IPatientApiClient, PatientApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IAllergyApiClient, AllergyApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddHttpClient<IErWorkflowApiClient, ErWorkflowApiClient>(client =>
    ConfigureHospitalApiClient(builder.Configuration, client));
builder.Services.AddSingleton<IErStaffService, ErStaffService>();

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
