using System.Text.Json;

namespace HospitalManagement.Web.Services;

internal static class ApiErrorReader
{
    public static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            if (responseBody.TrimStart().StartsWith('"'))
            {
                return JsonSerializer.Deserialize<string>(responseBody) ?? "Request failed.";
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("detail", out JsonElement detail))
            {
                return detail.GetString() ?? "Request failed.";
            }

            if (root.TryGetProperty("title", out JsonElement title))
            {
                return title.GetString() ?? "Request failed.";
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }
}
