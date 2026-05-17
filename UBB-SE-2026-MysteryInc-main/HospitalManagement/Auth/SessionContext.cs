namespace HospitalManagement.Auth;

public sealed class SessionContext
{
    public string Token { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void SetSession(string token, string username, string role)
    {
        Token = token;
        Username = username;
        Role = role;
    }

    public void Clear()
    {
        Token = string.Empty;
        Username = string.Empty;
        Role = string.Empty;
    }
}
