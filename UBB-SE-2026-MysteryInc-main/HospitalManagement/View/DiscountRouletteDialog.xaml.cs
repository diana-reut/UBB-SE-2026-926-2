using HospitalManagement.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.ComponentModel;

namespace HospitalManagement.View;

internal sealed partial class DiscountRouletteDialog : ContentDialog
{
    public DiscountRouletteViewModel ViewModel { get; }

    public int SelectedDiscountPercentage { get; private set; }

    public Action<int, decimal>? OnSpinComplete { get; set; }

    public DiscountRouletteDialog()
    {
        ViewModel = (Application.Current as App)!.Services.GetRequiredService<DiscountRouletteViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        ViewModel.SpinCompleted += OnSpinCompleted;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnSpinCompleted(int percent, decimal price)
    {
        OnSpinComplete?.Invoke(percent, price);
        Hide();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsSpinning) && ViewModel.IsSpinning)
        {
            SpinAnimation.Begin();
        }
    }
}
