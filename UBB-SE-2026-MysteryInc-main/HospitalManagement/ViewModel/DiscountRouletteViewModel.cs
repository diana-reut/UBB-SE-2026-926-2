using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.ViewModel;
internal partial class DiscountRouletteViewModel : ObservableObject
{
    [ObservableProperty]
    private decimal _basePrice;

    [ObservableProperty]
    private int _discountPercentage;

    [ObservableProperty]
    private decimal _finalPrice;

    [ObservableProperty]
    private bool _isSpinning;

    [ObservableProperty]
    private bool _isPrimaryButtonEnabled;

    [ObservableProperty]
    private bool _isResultVisible;

    private readonly int[] _discountOptions = [0, 10, 25, 50, 100,];

    public DiscountRouletteViewModel()
    {

    }

    public void Initialize(decimal price)
    {
        BasePrice = price;
        IsResultVisible = false;
        IsPrimaryButtonEnabled = true;
    }

    public event Action<int, decimal>? SpinCompleted;

    [RelayCommand]
    public async Task SpinWheel()
    {
        IsSpinning = true;
        IsPrimaryButtonEnabled = false;

        await Task.Delay(3000);

        int randomIndex = RandomNumberGenerator.GetInt32(_discountOptions.Length);
        DiscountPercentage = _discountOptions[randomIndex];
        FinalPrice = BasePrice * (1 - DiscountPercentage / 100m);

        IsResultVisible = true;

        await Task.Delay(2200);
        SpinCompleted?.Invoke(DiscountPercentage, FinalPrice);
        IsSpinning = false;

    }

}
