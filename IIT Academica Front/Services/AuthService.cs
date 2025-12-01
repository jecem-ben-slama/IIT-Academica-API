namespace IIT_Academica_Front.Services;

// Services/AuthService.cs (Revised to use AuthResponseDto)

using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Linq; 
using IIT_Academica_DTOs;
using System.Net; // Added for HttpClient status checking

public class AuthService : ApiService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient,
                        ILocalStorageService localStorage,
                        AuthenticationStateProvider authStateProvider)
        : base(httpClient, localStorage)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
    }

   
    public async Task<AuthResponseDto> Login(string email, string password)
    {
        var loginModel = new LoginDto { Email = email, Password = password };
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/user/login", loginModel);

            // Attempt to read the body regardless of HTTP status for detailed error messages
            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            // If deserialization failed, create a fallback authResponse
            if (authResponse == null)
            {
                authResponse = new AuthResponseDto 
                { 
                    IsSuccess = false, 
                    Message = $"Login failed: HTTP Status Code {response.StatusCode}." 
                };
            }

            // --- Primary Success Check ---
            // Check DTO's internal flag (HTTP success is often implicitly handled by IsSuccess)
            if (authResponse.IsSuccess && !string.IsNullOrWhiteSpace(authResponse.Token))
            {
                // SUCCESS PATH
                await LocalStorage.SetItemAsStringAsync("authToken", authResponse.Token);
                _authStateProvider.MarkUserAsAuthenticated(authResponse.Token);
                await EnsureAuthorizationHeaderAsync();
                
                // 🛑 FIX 2: Return the successful DTO object
                return authResponse; 
            }

            // --- Failure Path --- (The DTO already contains failure details)

            // If DTO was successfully read and indicates failure, we return it as is.
            if (authResponse.Errors != null && authResponse.Errors.Any())
            {
                 // Join errors for a cleaner message on the front end if needed,
                 // but returning the full DTO is enough for the front-end to handle.
            }
            
            // 🛑 FIX 2: Return the failed DTO object
            return authResponse;
        }
        catch (HttpRequestException)
        {
            // 🛑 FIX 2: Return a new DTO indicating connection failure
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = "API connection error. Please ensure the backend server is running and accessible." 
            };
        }
        catch (Exception ex)
        {
            // 🛑 FIX 2: Return a new DTO indicating unexpected failure
            return new AuthResponseDto 
            { 
                IsSuccess = false, 
                Message = $"An unexpected client error occurred: {ex.Message}" 
            };
        }
    }

    public async Task Logout()
    {
        _authStateProvider.MarkUserAsLoggedOut();
        await LocalStorage.RemoveItemAsync("authToken"); // Removed missing line
        await EnsureAuthorizationHeaderAsync();
    }
}