using System;
using System.IO;
using System.Text.Json;

namespace HospitalManagement.Configuration;

internal static class Config
{
    public static string ConnectionString { get; private set; } = "";

    public static void Load()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Configuration", "appsettings.local.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Configuration file appsettings.local.json was not found.");
        }

        string jsonContent = File.ReadAllText(filePath);

        using var document = JsonDocument.Parse(jsonContent);

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out JsonElement connectionStringsSection))
        {
            throw new InvalidDataException("Missing 'ConnectionStrings' section in appsettings.local.json.");
        }

        if (!connectionStringsSection.TryGetProperty("DefaultConnection", out JsonElement defaultConnectionElement))
        {
            throw new InvalidDataException("Missing 'DefaultConnection' in appsettings.local.json.");
        }

        string? connectionString = defaultConnectionElement.GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidDataException("The connection string is empty.");
        }

        ConnectionString = connectionString;
    }
}
