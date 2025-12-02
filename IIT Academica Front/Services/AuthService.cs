namespace IIT_Academica_Front.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Linq; 
using IIT_Academica_DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;

// NOTE: ApiService Base Class is assumed to exist and define HttpClient, LocalStorage, and necessary abstract methods.
// For example, ApiService might look like this:
// public abstract class ApiService
// {
//     protected HttpClient HttpClient { get; }
//     protected ILocalStorageService LocalStorage { get; }
//     // ... other base properties ...
// }

public class AuthService : ApiService
{
    private const string AuthTokenKey = "authToken";
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient,
                        ILocalStorageService localStorage,
                        AuthenticationStateProvider authStateProvider)
        : base(httpClient, localStorage)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
    }

    /// <summary>
    /// Retrieves the JWT token from local storage.
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        return await LocalStorage.GetItemAsStringAsync(AuthTokenKey);
    }
    
    /// <summary>
    /// Ensures the Authorization header is set on the HttpClient for subsequent requests.
    /// </summary>

    // --- Core Authentication Methods ---

    public async Task<AuthResponseDto> Login(string email, string password)
    {
        var loginModel = new LoginDto { Email = email, Password = password };
        try
        {
            // Post the login credentials to the backend API
            var response = await HttpClient.PostAsJsonAsync("api/user/login", loginModel);

            // Attempt to read the body regardless of HTTP status for detailed error messages
            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // If deserialization failed, create a fallback authResponse (e.g., bad format from API)
            if (authResponse == null)
            {
                authResponse = new AuthResponseDto 
                { 
                    IsSuccess = false, 
                    Message = $"Login failed: HTTP Status Code {response.StatusCode}. Could not parse response body." 
                };
            }

            // --- Primary Success Check ---
            // Check DTO's internal flag
            if (authResponse.IsSuccess && !string.IsNullOrWhiteSpace(authResponse.Token))
            {
                // SUCCESS PATH
                await LocalStorage.SetItemAsStringAsync(AuthTokenKey, authResponse.Token);
                
                // Notify the AuthenticationStateProvider of the new state
                _authStateProvider.MarkUserAsAuthenticated(authResponse.Token);
                
                // Set the header for immediate use in other services (like UserService)
                await EnsureAuthorizationHeaderAsync();
                
                return authResponse; 
            }

            // --- Failure Path --- (The DTO already contains failure details from the API)
            return authResponse;
        }
        catch (HttpRequestException)
        {
            // Connection/Network failure (API not running or unreachable)
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = "API connection error. Please ensure the backend server is running and accessible." 
            };
        }
        catch (Exception ex)
        {
            // Unexpected client-side error
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = $"An unexpected client error occurred: {ex.Message}" 
            };
        }
    }

    public async Task Logout()
    {
        // 1. Notify AuthenticationStateProvider
        _authStateProvider.MarkUserAsLoggedOut();
        
        // 2. Remove token from persistent storage
        await LocalStorage.RemoveItemAsync(AuthTokenKey); 
        
        // 3. Clear the Authorization header on the HttpClient
        await EnsureAuthorizationHeaderAsync();
    }
}