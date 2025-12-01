namespace IIT_Academica_Front.Services;
// Services/CustomAuthenticationStateProvider.cs

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly HttpClient _httpClient;

    private const string AuthTokenKey = "authToken";

    public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
    {
        _localStorageService = localStorageService;
        _httpClient = httpClient;
    }

    /// <summary>
    /// The primary method that Blazor calls to determine the current user's identity.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorageService.GetItemAsStringAsync(AuthTokenKey);

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            // Clear the HttpClient header in case a bad/old token was previously set
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // Not authenticated: return an empty identity
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Token exists: Validate and set identity
        try
        {
            var claims = ParseClaimsFromJwt(savedToken);
            var identity = new ClaimsIdentity(claims, "jwtAuthType");

            // Attach token to default HttpClient for all future requests
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", savedToken);

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception)
        {
            // Token was invalid, expired, or malformed. Force logout.
            MarkUserAsLoggedOut();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Called after a successful login to update the state and UI.
    /// </summary>
    public void MarkUserAsAuthenticated(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));

        // Update the state and inform all Blazor components
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    /// <summary>
    /// Called during logout to clear the token and update the state.
    /// </summary>
    public void MarkUserAsLoggedOut()
    {
        _localStorageService.RemoveItemAsync(AuthTokenKey);

        // Clear Auth header on the HttpClient
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // Update the state and inform all Blazor components (unauthenticated)
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    // --- JWT Parsing Helpers ---

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        // Base64 decode and padding fix
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                // Identity roles are often mapped to 'role' or 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
                // We use ClaimTypes.Role, ClaimTypes.NameIdentifier, and ClaimTypes.Email for standard mapping.

                if (kvp.Key == "role" && kvp.Value is string role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                else if (kvp.Key == "sub" && kvp.Value is string sub) // 'sub' is standard for user ID
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
                }
                else if (kvp.Key == "email" && kvp.Value is string email)
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                }
                else
                {
                    // Add all other claims by their raw key name
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
                }
            }
        }
        return claims;
    }

    // Fixes padding issues common with Base64URL encoding used in JWTs
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}