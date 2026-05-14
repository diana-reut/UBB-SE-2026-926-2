using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using ERManagementSystem.Services;

namespace ERManagementSystem.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddERManagementSystem(this IServiceCollection services)
        {
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<NavigationService>());
            services.AddSingleton<NurseService>();
            services.AddSingleton<MockStaffService>();

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
