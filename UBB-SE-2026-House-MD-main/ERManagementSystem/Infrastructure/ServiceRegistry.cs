using System;
using Microsoft.UI.Xaml;

namespace ERManagementSystem.Infrastructure
{
    public static class ServiceRegistry
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public static Window? MainWindow { get; private set; }

        public static void Configure(IServiceProvider services)
        {
            Services = services;
        }

        public static void SetMainWindow(Window window)
        {
            MainWindow = window;
        }
    }
}
