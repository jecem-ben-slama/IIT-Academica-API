namespace IIT_Academica_Front.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using IIT_Academica_DTOs;
using System.Net.Http.Headers;

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


    public async Task<string?> GetTokenAsync()
    {
        return await LocalStorage.GetItemAsStringAsync(AuthTokenKey);
    }

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


    public async Task<AuthResponseDto> Login(string email, string password)
    {
        var loginModel = new LoginDto { Email = email, Password = password };
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/user/login", loginModel);

            AuthResponseDto? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (authResponse == null)
            {
                authResponse = new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = $"Login failed: HTTP Status Code {response.StatusCode}. Could not parse response body."
                };
            }


            if (authResponse.IsSuccess && !string.IsNullOrWhiteSpace(authResponse.Token))
            {
                await LocalStorage.SetItemAsStringAsync(AuthTokenKey, authResponse.Token);

                _authStateProvider.MarkUserAsAuthenticated(authResponse.Token);

                await EnsureAuthorizationHeaderAsync();

                return authResponse;
            }

            return authResponse;
        }
        catch (HttpRequestException)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "API connection error. Please ensure the backend server is running and accessible."
            };
        }
        catch (Exception ex)
        {
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

        await LocalStorage.RemoveItemAsync(AuthTokenKey);

        await EnsureAuthorizationHeaderAsync();
    }

    public async Task<AuthResponseDto> ForgotPassword(ForgetPasswordDto model)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/user/forgot-password", model);

            var responseBody = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }

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

    public async Task<AuthResponseDto> ResetPassword(ResetPasswordDto model)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/user/reset-password", model);

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            if (response.IsSuccessStatusCode)
            {
                return authResponse ?? new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password has been successfully reset. You can now log in."
                };
            }

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