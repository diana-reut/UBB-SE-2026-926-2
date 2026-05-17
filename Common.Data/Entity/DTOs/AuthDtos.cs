namespace Common.Data.Entity.DTOs;

public record LoginDto(string Username, string Password);

public record RegisterDto(string Username, string Password, string Role = "User");

public record AuthResponseDto(string Token, string Username, string Role);
