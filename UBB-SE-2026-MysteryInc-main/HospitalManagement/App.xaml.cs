using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.Data.Data;
using Common.Data.Repository;
using ERManagementSystem.Infrastructure;
using ERManagementSystem.Proxy.ERRoomProxy;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.ExaminationProxy;
using ERManagementSystem.Proxy.TransferLogProxy;
using ERManagementSystem.Proxy.TriageParametersProxy;
using ERManagementSystem.Proxy.TriageProxy;
using HospitalManagement.Integration.Export;
using HospitalManagement.Integration.External;
using HospitalManagement.Auth;
using HospitalManagement.Proxy.AddictDetectionProxy;
using HospitalManagement.Proxy.AllergyProxy;
using HospitalManagement.Proxy.AuthProxy;
using HospitalManagement.Proxy.BillingProxy;
using HospitalManagement.Proxy.BloodCompatibilityProxy;
using HospitalManagement.Proxy.PatientProxy;
using HospitalManagement.Proxy.PrescriptionProxy;
using HospitalManagement.Proxy.StatisticsProxy;
using HospitalManagement.Proxy.TransplantProxy;
using HospitalManagement.Service;
using HospitalManagement.View;
using HospitalManagement.View.DialogServiceAdmin;
using HospitalManagement.ViewModel;
using Microsoft.EntityFrameworkCore;
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
    private const string LocalConfigurationRelativePath = "config\\appsettings.local.json";
    private static readonly string StartupLogPath = Path.Combine(AppContext.BaseDirectory, "startup-errors.log");

    private Window? window;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        UnhandledException += App_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        Services = ConfigureServices();
        HospitalManagement.Infrastructure.ServiceRegistry.Configure(Services);
        ERManagementSystem.Infrastructure.ServiceRegistry.Configure(Services);
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        EnsureDatabaseCreated();
        window = new MainWindow();
        HospitalManagement.Infrastructure.ServiceRegistry.SetMainWindow(window);
        ERManagementSystem.Infrastructure.ServiceRegistry.SetMainWindow(window);
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(AppConfiguration);


        _ = services.AddHttpClient<IBloodCompatibilityProxy, BloodCompatibilityProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        _ = services.AddTransient<IExportService, ExportService>();
        _ = services.AddTransient<IImportService, ImportService>();
        _ = services.AddSingleton<IGhostService, GhostService>();
        _ = services.AddTransient<AuthTokenForwardingHandler>();
        _ = services.AddHttpClient<IStatisticsProxy, StatisticsProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<IPrescriptionProxy, PrescriptionProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        _ = services.AddHttpClient<IAllergyProxy, AllergyProxy>((client) =>
        {
            string? uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        _ = services.AddHttpClient<IPatientProxy, PatientProxy>((client) =>
        {
            string? uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();
        _ = services.AddHttpClient("ERPatientProxy", (client) =>
        {
            string? uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();
        _ = services.AddTransient<ERManagementSystem.Proxy.PatientProxy.IPatientProxy>((serviceProvider) =>
            new ERManagementSystem.Proxy.PatientProxy.PatientProxy(
                serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ERPatientProxy")));

        _ = services.AddHttpClient<IERVisitProxy, ERVisitProxy>((client) =>
        {
            string? uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<IERRoomProxy, ERRoomProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<ITriageProxy, TriageProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<ITriageParametersProxy, TriageParametersProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<IExaminationProxy, ExaminationProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<ITransferLogProxy, TransferLogProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthTokenForwardingHandler>();

        _ = services.AddHttpClient<ITransplantProxy, TransplantProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });


        _ = services.AddHttpClient<IBillingProxy, BillingProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });


        _ = services.AddHttpClient<IAddictDetectionProxy, AddictDetectionProxy>((client) =>
        {
            var uriString = AppConfiguration["ApiSettings:BaseUri"];

            if (string.IsNullOrEmpty(uriString))
            {
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            }

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

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
                return serviceProvider.GetRequiredService<TransplantRequestViewModel>();
            });
        _ = services.AddSingleton<DiscountRouletteViewModel>();
        _ = services.AddSingleton<Func<PrescriptionView>>(sp => () => sp.GetRequiredService<PrescriptionView>());

        _ = services.AddSingleton<IExternalProvider, MockStaffProxy>();
        _ = services.AddSingleton<IExternalPatientPublisher, ExternalPatientPublisher>();
        _ = services.AddSingleton<IDialogService, DialogService>();

        _ = services.AddTransient<AdminDashboardPage>();
        _ = services.AddTransient<MedicalStaffDashboardPage>();
        _ = services.AddTransient<PharmacistDashboardPage>();

        // Auth
        _ = services.AddSingleton<SessionContext>();
        _ = services.AddHttpClient<IAuthProxy, AuthProxy>((client) =>
        {
            string? uriString = AppConfiguration["ApiSettings:BaseUri"];
            if (string.IsNullOrEmpty(uriString))
                throw new InvalidOperationException("BaseUri is missing from appsettings.local.json");
            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        _ = services.AddTransient<LoginViewModel>();

        _ = services.AddERManagementSystem();


        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, LocalConfigurationRelativePath);

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                $"Missing local configuration file '{LocalConfigurationRelativePath}'. Copy 'config\\\\appsettings.example.json' to 'config\\\\appsettings.local.json' and set your local connection string.");
        }

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(LocalConfigurationRelativePath, optional: false, reloadOnChange: true)
            .Build();
    }

    private void EnsureDatabaseCreated()
    {
        using IServiceScope scope = Services.CreateScope();
        // EFHospitalDbContext dbContext = scope.ServiceProvider.GetRequiredService<EFHospitalDbContext>();
        // dbContext.Database.EnsureCreated();
    }

    private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        LogUnhandledException("AppDomain.CurrentDomain.UnhandledException", e.ExceptionObject as Exception);
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogUnhandledException("TaskScheduler.UnobservedTaskException", e.Exception);
    }

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        LogUnhandledException("Application.UnhandledException", e.Exception);
    }

    private static void LogUnhandledException(string source, Exception? exception)
    {
        try
        {
            string entry = $"[{DateTime.Now:O}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(StartupLogPath, entry);
        }
        catch
        {
        }
    }
}
