namespace IIT_Academica_Front.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Linq; 
using IIT_Academica_DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;

// NOTE: ApiService Base Class is assumed to exist and define HttpClient, LocalStorage, and necessary abstract methods.
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
    // Assuming this method exists in your ApiService base class or needs to be added:
    // This is required to clear the header on Logout or set it on Login.
    public async Task EnsureAuthorizationHeaderAsync()
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            HttpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

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
    
    // --------------------------------------------------------------------------------
    // --- NEW PASSWORD RESET METHODS ---
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Initiates the Forgot Password process by sending the user's email to the API.
    /// </summary>
    /// <param name="model">The DTO containing the user's email.</param>
    /// <returns>A generic success/failure message DTO.</returns>
    public async Task<AuthResponseDto> ForgotPassword(ForgetPasswordDto model)
    {
        try
        {
            // Note: The API is designed to return a 200 OK with a generic message even if the email doesn't exist.
            var response = await HttpClient.PostAsJsonAsync("api/user/forgot-password", model);
            
            // Read the generic response body
            var responseBody = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            // If the response is success, we assume the API handled the request successfully 
            // and sent (or pretended to send) the email.
            if (response.IsSuccessStatusCode)
            {
                return new AuthResponseDto 
                { 
                    IsSuccess = true, 
                    Message = "If an account with that email exists, a password reset link has been sent." 
                };
            }
            
            // If the status code is a non-success code (e.g., 400 Bad Request)
            return responseBody ?? new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = "An unexpected error occurred during password reset initiation." 
            };
        }
        catch (HttpRequestException)
        {
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = "API connection error. The server is unreachable." 
            };
        }
    }

    /// <summary>
    /// Completes the Password Reset process by sending the token, email, and new password to the API.
    /// </summary>
    /// <param name="model">The DTO containing the email, token, and new password.</param>
    /// <returns>A success/failure message DTO.</returns>
    public async Task<AuthResponseDto> ResetPassword(ResetPasswordDto model)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/user/reset-password", model);
            
            // Attempt to read the body for success or detailed error messages
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (response.IsSuccessStatusCode)
            {
                // Successful reset
                return authResponse ?? new AuthResponseDto 
                { 
                    IsSuccess = true, 
                    Message = "Password has been successfully reset. You can now log in." 
                };
            }
            
            // Handle API failure response (e.g., invalid token, password strength failure)
            return authResponse ?? new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = $"Password reset failed. Check token validity and password strength. Status: {response.StatusCode}." 
            };
        }
        catch (HttpRequestException)
        {
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = "API connection error. The server is unreachable." 
            };
        }
    }
}