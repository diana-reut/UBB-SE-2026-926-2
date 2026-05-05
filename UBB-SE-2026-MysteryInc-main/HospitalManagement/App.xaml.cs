using System;
using System.Runtime.CompilerServices;
using ERManagementSystem.Infrastructure;
using HospitalManagement.Data;
using HospitalManagement.Infrastructure;
using HospitalManagement.Integration.Export;
using HospitalManagement.Integration.External;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using HospitalManagement.View;
using HospitalManagement.View.DialogServiceAdmin;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

[assembly: InternalsVisibleTo("HospitalManagementTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace HospitalManagement;

public partial class App : Application
{
    public IServiceProvider Services { get; }
    private static readonly IConfiguration AppConfiguration = BuildConfiguration();

    private Window? window;

    public App()
    {
        Services = ConfigureServices();
        HospitalManagement.Infrastructure.ServiceRegistry.Configure(Services);
        ERManagementSystem.Infrastructure.ServiceRegistry.Configure(Services);
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        HospitalManagement.Infrastructure.ServiceRegistry.SetMainWindow(window);
        ERManagementSystem.Infrastructure.ServiceRegistry.SetMainWindow(window);
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(AppConfiguration);

        _ = services.AddDbContext<EFHospitalDbContext>(options =>
            options.UseSqlServer(AppConfiguration.GetConnectionString("DefaultConnection")));

        _ = services.AddScoped<IPatientRepository, PatientRepository>();
        _ = services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
        _ = services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        _ = services.AddScoped<IAllergyRepository, AllergyRepository>();
        _ = services.AddScoped<ITransplantRepository, TransplantRepository>();
        _ = services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

        _ = services.AddTransient<IBloodCompatibilityService, BloodCompatibilityService>();
        _ = services.AddTransient<IPatientService, PatientService>();
        _ = services.AddTransient<IAllergyService, AllergyService>();
        _ = services.AddTransient<ITransplantService, TransplantService>();
        _ = services.AddTransient<IExportService, ExportService>();
        _ = services.AddTransient<IImportService, ImportService>();
        _ = services.AddTransient<IBillingService, BillingService>();
        _ = services.AddTransient<IAddictDetectionService, AddictDetectionService>();
        _ = services.AddTransient<IPrescriptionService, PrescriptionService>();
        _ = services.AddTransient<IStatisticsService, StatisticsService>();
        _ = services.AddSingleton<IGhostService, GhostService>();

        _ = services.AddTransient<AdminViewModel>();
        _ = services.AddTransient<AdminView>();
        _ = services.AddTransient<PatientViewModel>();
        _ = services.AddTransient<PatientView>();
        _ = services.AddTransient<AddictViewModel>();
        _ = services.AddTransient<AddictView>();
        _ = services.AddTransient<PharmacistViewModel>();
        _ = services.AddTransient<PharmacistView>();
        _ = services.AddTransient<PrescriptionViewModel>();
        _ = services.AddTransient<PrescriptionView>();
        _ = services.AddTransient<OrganDonorDialogViewModel>();
        _ = services.AddTransient<OrganDonorDialog>();
        _ = services.AddTransient<BloodDonorsViewModel>();
        _ = services.AddTransient<BloodDonorsView>();
        _ = services.AddTransient<StatisticsViewModel>();
        _ = services.AddTransient<StatisticsView>();
        _ = services.AddTransient<PatientProfileViewModel>();
        _ = services.AddTransient<PatientProfileView>();
        _ = services.AddTransient<MedicalStaffViewModel>();
        _ = services.AddTransient<MedicalHistoryDialogViewModel>();
        _ = services.AddTransient<MedicalHistoryDialog>();
        _ = services.AddTransient<TransplantRequestViewModel>();
        _ = services.AddTransient<AddPatientDialogViewModel>();
        _ = services.AddSingleton<Func<int, TransplantRequestViewModel>>(serviceProvider =>
            id =>
            {
                TransplantRequestViewModel vm = serviceProvider.GetRequiredService<TransplantRequestViewModel>();
                vm.Initialize(id);
                return vm;
            });
        _ = services.AddSingleton<DiscountRouletteViewModel>();
        _ = services.AddSingleton<Func<PrescriptionView>>(sp => () => sp.GetRequiredService<PrescriptionView>());

        _ = services.AddSingleton<IExternalProvider, MockERProxy>();
        _ = services.AddSingleton<IExternalProvider, MockStaffProxy>();
        _ = services.AddSingleton<IExternalPatientPublisher, ExternalPatientPublisher>();
        _ = services.AddSingleton<IDialogService, DialogService>();

        _ = services.AddTransient<AdminDashboardPage>();
        _ = services.AddTransient<MedicalStaffDashboardPage>();
        _ = services.AddTransient<PharmacistDashboardPage>();

        _ = services.AddERManagementSystem();

        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
