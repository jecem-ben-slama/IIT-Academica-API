namespace IIT_Academica_Front.Services
{
    // Services/ApiService.cs

    using Blazored.LocalStorage;

    public abstract class ApiService
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILocalStorageService LocalStorage;

        // The token key must match the key used in AuthService and StateProvider
        private const string AuthTokenKey = "authToken";

        public ApiService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            HttpClient = httpClient;
            LocalStorage = localStorage;
        }

        /// <summary>
        /// Ensures the Authorization header is set with the current JWT token.
        /// MUST be called before any authenticated API request.
        protected async Task EnsureAuthorizationHeaderAsync()
        {
            var token = await LocalStorage.GetItemAsStringAsync(AuthTokenKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                // Remove previous header to avoid duplicate headers if token changed
                if (HttpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                }

                // Set the token
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
            else
            {
                // If token is null, clear header to prevent sending old/bad token
                if (HttpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                }
            }
        }
    }
}
