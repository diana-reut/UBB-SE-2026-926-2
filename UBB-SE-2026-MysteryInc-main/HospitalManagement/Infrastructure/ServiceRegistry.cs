using System;
using Microsoft.UI.Xaml;

namespace HospitalManagement.Infrastructure;

internal static class ServiceRegistry
{
    private static IServiceProvider? services;
    private static Window? mainWindow;

    internal static IServiceProvider Services =>
        services ?? throw new InvalidOperationException("HospitalManagement services have not been configured.");

    internal static Window MainWindow =>
        mainWindow ?? throw new InvalidOperationException("HospitalManagement main window has not been configured.");

    internal static void Configure(IServiceProvider serviceProvider)
    {
        services = serviceProvider;
    }

    internal static void SetMainWindow(Window window)
    {
        mainWindow = window;
    }
}
