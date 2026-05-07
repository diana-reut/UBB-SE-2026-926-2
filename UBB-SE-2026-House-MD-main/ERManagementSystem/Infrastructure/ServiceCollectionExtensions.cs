using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ERManagementSystem.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddERManagementSystem(this IServiceCollection services)
        {
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<NavigationService>());

            services.AddTransient<IPatientRepository, PatientRepository>();
            services.AddTransient<IERVisitRepository, ERVisitRepository>();
            services.AddTransient<ITriageRepository, TriageRepository>();
            services.AddTransient<ITriageParametersRepository, TriageParametersRepository>();
            services.AddTransient<IExaminationRepository, ExaminationRepository>();
            services.AddTransient<ITransferLogRepository, TransferLogRepository>();
            services.AddTransient<IRoomRepository, RoomRepository>();

            services.AddTransient<IRegistrationService, RegistrationService>();
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

            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<TriageViewModel>();
            services.AddTransient<QueueViewModel>();
            services.AddTransient<ExaminationViewModel>();
            services.AddTransient<TransferLogViewModel>();
            services.AddTransient<RoomAssignmentViewModel>();
            services.AddTransient<RoomManagementViewModel>();

            services.AddTransient<PatientRegistrationView>();
            services.AddTransient<TriageView>();
            services.AddTransient<QueueView>();
            services.AddTransient<ExaminationView>();
            services.AddTransient<TransferLogView>();
            services.AddTransient<RoomAssignmentView>();
            services.AddTransient<RoomManagementView>();

            return services;
        }
    }
}
