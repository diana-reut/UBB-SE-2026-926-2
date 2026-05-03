using System;
using System.Threading.Tasks;
using ERManagementSystem.DataAccess;
using ERManagementSystem.Helpers;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ERManagementSystem
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; } = null!;
        public static Window? MainAppWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
            ConfigureGlobalExceptionHandling(); // 1.11 - Global exception handling - for unexpected issues that escape everything else
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // ── Core infrastructure ──────────────────────────────────────────
            services.AddSingleton<DatabaseConnection>();
            services.AddSingleton<SqlHelper>();

            // ── Navigation ───────────────────────────────────────────────────
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<NavigationService>());

            // ── Repositories ─────────────────────────────────────────────────
            services.AddTransient<IPatientRepository, PatientRepository>();
            services.AddTransient<IERVisitRepository, ERVisitRepository>();
            services.AddTransient<ITriageRepository, TriageRepository>();
            services.AddTransient<ITriageParametersRepository, TriageParametersRepository>();
            services.AddTransient<IExaminationRepository, ExaminationRepository>();
            services.AddTransient<ITransferLogRepository, TransferLogRepository>();
            services.AddTransient<IRoomRepository, RoomRepository>();

            // ── Services ─────────────────────────────────────────────────────
            services.AddTransient<IRegistrationService, RegistrationService>();
            // Task 5.13: use factory so StateManagementService always gets RoomRepository
            // enabling auto-set room to cleaning when visit is TRANSFERRED or CLOSED.
            services.AddTransient<IStateManagementService>(sp =>
                new StateManagementService(
                    sp.GetRequiredService<IERVisitRepository>(),
                    sp.GetRequiredService<IRoomRepository>()));
            services.AddSingleton<NurseService>();
            services.AddTransient<ITriageService, TriageService>();
            services.AddTransient<IQueueService, QueueService>();
            services.AddTransient<IRoomAssignmentService, RoomAssignmentService>();
            services.AddTransient<IRoomManagementService, RoomManagementService>();
            services.AddSingleton<MockStaffService>();
            services.AddTransient<IExaminationService, ExaminationService>();
            services.AddTransient<ITransferService, TransferService>();

            // ── ViewModels ───────────────────────────────────────────────────
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<TriageViewModel>();
            services.AddTransient<QueueViewModel>();
            services.AddTransient<ExaminationViewModel>();
            services.AddTransient<TransferLogViewModel>();
            services.AddTransient<RoomAssignmentViewModel>();     // Alex
            services.AddTransient<RoomManagementViewModel>();     // Alex

            // ── Views ────────────────────────────────────────────────────────
            services.AddTransient<PatientRegistrationView>();
            services.AddTransient<TriageView>();
            services.AddTransient<QueueView>();
            services.AddTransient<ExaminationView>();
            services.AddTransient<TransferLogView>();
            services.AddTransient<RoomAssignmentView>();          // Alex
            services.AddTransient<RoomManagementView>();          // Alex

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }

        private void ConfigureGlobalExceptionHandling()
        {
            this.UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Error("Unhandled UI exception.", e.Exception);
            e.Handled = true;

            await ErrorDialogHelper.ShowErrorAsync(
                "Unexpected Error",
                "Something went wrong. The error was logged.");
        }

        private void CurrentDomain_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Error("Unhandled non-UI exception.", ex);
            }
            else
            {
                Logger.Error("Unhandled non-UI exception with unknown exception object.");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error("Unobserved task exception.", e.Exception);
            e.SetObserved();
        }
    }
}
