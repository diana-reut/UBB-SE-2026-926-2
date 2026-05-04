using System;

namespace HospitalManagement.Infrastructure;

internal static class ServiceRegistry
{
    private static IServiceProvider? services;

    internal static IServiceProvider Services =>
        services ?? throw new InvalidOperationException("HospitalManagement services have not been configured.");

    internal static void Configure(IServiceProvider serviceProvider)
    {
        services = serviceProvider;
    }
}
