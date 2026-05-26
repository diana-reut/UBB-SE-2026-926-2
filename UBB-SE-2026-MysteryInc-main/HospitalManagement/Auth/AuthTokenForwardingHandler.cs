using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagement.Auth;

internal class AuthTokenForwardingHandler : DelegatingHandler
{
    private readonly SessionContext session;

    public AuthTokenForwardingHandler(SessionContext session)
    {
        this.session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (session.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
