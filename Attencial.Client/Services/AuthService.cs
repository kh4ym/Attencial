using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Attencial.Shared.Dtos;
using Microsoft.JSInterop;

namespace Attencial.Client.Services
{
    /// <summary>
    /// Handles all authentication‑related API calls and token storage.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        // Keys used by the auth.js helper
        private const string TokenKey = "authToken";

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        #region Public API

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">Email, password and role.</param>
        /// <returns>true if registration succeeded, otherwise false.</returns>
        public async Task<bool> RegisterAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Logs the user in and stores the JWT token.
        /// </summary>
        /// <param name="request">Email and password.</param>
        /// <returns>LoginResponse containing the token if successful; otherwise null.</returns>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content
                                            .ReadFromJsonAsync<ApiResponse<LoginResponse>>();

            var token = apiResponse?.Data?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                // Persist token in local storage (auth.js helper)
                await _jsRuntime.InvokeVoidAsync("authStorage.setToken", token);
            }

            return apiResponse?.Data;
        }

        /// <summary>
        /// Clears the stored token – effectively logging the user out.
        /// </summary>
        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("authStorage.removeToken");
        }

        /// <summary>
        /// Retrieves the current logged‑in user information from the backend.
        /// </summary>
        /// <returns>The ApiResponse with user data, or null if not authenticated.</returns>
        public async Task<ApiResponse<object>?> GetCurrentUserAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return null; // No token → not logged in

            var response = await _httpClient.GetAsync("api/auth/me");
            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content
                                            .ReadFromJsonAsync<ApiResponse<object>>();
            return apiResponse;
        }

        /// <summary>
        /// Helper that reads the JWT token from local storage.
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("authStorage.getToken");
        }

        #endregion
    }
}
