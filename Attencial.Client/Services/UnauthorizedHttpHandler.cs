using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Attencial.Client.Services
{
    public class UnauthorizedHttpHandler : DelegatingHandler
    {
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;

        public UnauthorizedHttpHandler(NavigationManager nav, IJSRuntime js)
        {
            _nav = nav;
            _js = js;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var path = request.RequestUri?.AbsolutePath ?? "";
                if (!path.EndsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase))
                {
                    await _js.InvokeVoidAsync("authStorage.removeToken");
                    _nav.NavigateTo("/login", forceLoad: true);
                }
            }

            return response;
        }
    }
}
