using HospitalManagement.Infrastructure;
using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace HospitalManagement.View;

internal sealed partial class TransplantRequestView : Page
{
    public TransplantRequestViewModel ViewModel { get; }

    private readonly Window _parentWindow;
    private readonly int _patientId;

    public TransplantRequestView(int patientId, Window parentWindow)
    {
        var factory = ServiceRegistry.Services.GetRequiredService<Func<int, TransplantRequestViewModel>>();
        ViewModel = factory(patientId);
        InitializeComponent();
        _parentWindow = parentWindow;
        _patientId = patientId;

        DataContext = ViewModel;

        ViewModel.CloseWindowAction = () => _parentWindow.Close();
        ViewModel.ShowDialogAction = ShowDialogAsync;
    }

    public Task InitializeAsync() => ViewModel.InitializeAsync(_patientId);

    private async Task ShowDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot,
        };

        await dialog.ShowAsync();
    }
}
