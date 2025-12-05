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

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorageService.GetItemAsStringAsync(AuthTokenKey);

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var claims = ParseClaimsFromJwt(savedToken);
            var identity = new ClaimsIdentity(claims, "jwtAuthType");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", savedToken);

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            MarkUserAsLoggedOut();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    public void MarkUserAsLoggedOut()
    {
        _localStorageService.RemoveItemAsync(AuthTokenKey);

        _httpClient.DefaultRequestHeaders.Authorization = null;

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    // -------- JWT Parsing (FIXED ROLE HANDLING) --------

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                switch (kvp.Key)
                {
                    case "role":
                        // Map to ClaimTypes.Role
                        claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()!));
                        break;

                    case "roles": // if backend sends array of roles
                        if (kvp.Value is JsonElement rolesElem && rolesElem.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var r in rolesElem.EnumerateArray())
                                claims.Add(new Claim(ClaimTypes.Role, r.GetString()!));
                        }
                        break;

                    case "sub":
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, kvp.Value.ToString()!));
                        break;

                    case "email":
                        claims.Add(new Claim(ClaimTypes.Email, kvp.Value.ToString()!));
                        break;

                    default:
                        // Prevent overriding role by excluding raw "role"
                        if (kvp.Key != "role" && kvp.Key != "roles")
                            claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                        break;
                }
            }
        }

        return claims;
    }

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
