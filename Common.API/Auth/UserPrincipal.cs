namespace Common.API.Auth;

public sealed class UserPrincipal
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
