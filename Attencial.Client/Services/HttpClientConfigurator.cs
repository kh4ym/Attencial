using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Attencial.Client.Services
{
    /// <summary>
    /// A custom HTTP handler that intercepts outgoing HTTP requests 
    /// and automatically attaches the JWT token to the Authorization header.
    /// </summary>
    public class HttpClientConfigurator : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public HttpClientConfigurator(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Try to get the token from local storage
            var token = await _jsRuntime.InvokeAsync<string>("authStorage.getToken", cancellationToken);

            // If a token exists, add it to the request header
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Continue sending the request
            return await base.SendAsync(request, cancellationToken);
        }
    }
}