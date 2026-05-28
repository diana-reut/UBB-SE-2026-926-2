using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Common.Data.Entity.DTOs;
using HospitalManagement.Auth;
using HospitalManagement.Proxy.AuthProxy;

namespace HospitalManagement.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthProxy _authProxy;
    private readonly SessionContext _session;

    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginViewModel(IAuthProxy authProxy, SessionContext session)
    {
        _authProxy = authProxy;
        _session = session;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsBusy = true;
        try
        {
            AuthResponseDto response = await _authProxy.LoginAsync(new LoginDto
            {
                Username = Username,
                Password = Password
            });
            _session.SetSession(response.Token, response.Username, response.Role);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
