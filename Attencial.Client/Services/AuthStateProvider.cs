using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Attencial.Client.Services
{
    /// <summary>
    /// Reads the stored JWT token and tells Blazor who is logged in and what role they have.
    /// </summary>
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthService _authService;

        public AuthStateProvider(IJSRuntime jsRuntime, AuthService authService)
        {
            _jsRuntime = jsRuntime;
            _authService = authService;
        }

        /// <summary>
        /// Called automatically by Blazor to get the current user's auth state.
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("authStorage.getToken");

            // No token stored → user is anonymous
            if (string.IsNullOrWhiteSpace(token))
            {
                return AnonymousState();
            }

            // Parse claims out of the JWT payload
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        /// <summary>
        /// Call this right after a successful login so the UI updates immediately.
        /// </summary>
        public async Task NotifyUserLoggedIn()
        {
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        /// <summary>
        /// Call this right after logout so the UI reverts to anonymous.
        /// </summary>
        public async Task NotifyUserLoggedOut()
        {
            await _authService.LogoutAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private static AuthenticationState AnonymousState()
        {
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));
        }

        /// <summary>
        /// Manually decodes the JWT payload (the middle part) without any external library.
        /// The payload is Base64Url encoded JSON containing the claims.
        /// </summary>
        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            // JWT = header.payload.signature  — we only need the payload
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return claims;

            // Fix Base64Url → standard Base64
            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var jsonString = Encoding.UTF8.GetString(jsonBytes);

            // Parse the JSON dictionary
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
            if (keyValuePairs == null)
                return claims;

            foreach (var kvp in keyValuePairs)
            {
                // Map well-known JWT claim names to .NET ClaimTypes
                var claimType = kvp.Key switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "email" => ClaimTypes.Email,
                    "role" => ClaimTypes.Role,
                    _ => kvp.Key
                };

                var value = kvp.Value.ValueKind == JsonValueKind.String
                    ? kvp.Value.GetString() ?? string.Empty
                    : kvp.Value.ToString();

                claims.Add(new Claim(claimType, value));
            }

            return claims;
        }
    }
}