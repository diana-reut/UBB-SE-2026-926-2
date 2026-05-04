using System;
using System.Runtime.CompilerServices;
using ERManagementSystem.Infrastructure;
using HospitalManagement.Database;
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
        window = Services.GetRequiredService<MainWindow>();
        ERManagementSystem.Infrastructure.ServiceRegistry.SetMainWindow(window);
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(AppConfiguration);

        _ = services.AddSingleton<IDbContext, HospitalDbContext>();

        _ = services.AddSingleton<IPatientRepository, PatientRepository>();
        _ = services.AddSingleton<IMedicalHistoryRepository, MedicalHistoryRepository>();
        _ = services.AddSingleton<IMedicalRecordRepository, MedicalRecordRepository>();
        _ = services.AddSingleton<IAllergyRepository, AllergyRepository>();
        _ = services.AddSingleton<ITransplantRepository, TransplantRepository>();
        _ = services.AddSingleton<IPrescriptionRepository, PrescriptionRepository>();

        _ = services.AddSingleton<IBloodCompatibilityService, BloodCompatibilityService>();
        _ = services.AddSingleton<IPatientService, PatientService>();
        _ = services.AddSingleton<IAllergyService, AllergyService>();
        _ = services.AddSingleton<ITransplantService, TransplantService>();
        _ = services.AddSingleton<IExportService, ExportService>();
        _ = services.AddSingleton<IImportService, ImportService>();
        _ = services.AddSingleton<IBillingService, BillingService>();
        _ = services.AddTransient<IAddictDetectionService, AddictDetectionService>();
        _ = services.AddTransient<IPrescriptionService, PrescriptionService>();
        _ = services.AddSingleton<IStatisticsService, StatisticsService>();
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

        _ = services.AddSingleton<MainWindow>();
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
