using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace HospitalManagement.Web.Services;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public BearerTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = httpContextAccessor.HttpContext?.Session.GetString(WebSessionKeys.AccessToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
