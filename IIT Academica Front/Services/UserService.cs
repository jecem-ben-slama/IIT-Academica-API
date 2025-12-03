using System.Net.Http.Json;
using System.Net.Http.Headers;
using IIT_Academica_Front.Models;

namespace IIT_Academica_Front.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService; // To get the token

        public UserService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                // Ensure the Authorization header is set for protected routes
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // --- AUTHENTICATION/REGISTER/LOGIN (Typically handled by AuthService, but included for completeness) ---

         public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
         {
             var response = await _httpClient.PostAsJsonAsync("api/user/register", model);
             return await response.Content.ReadFromJsonAsync<AuthResponseDto>() ?? new AuthResponseDto { IsSuccess = false, Message = "API communication error." };
         }



        
        public async Task<List<UserReadDto>?> GetAllUsersAsync()
        {
            await SetAuthorizationHeader(); // Ensure token is attached
            
            try
            {
                var response = await _httpClient.GetAsync("api/user/GetAllUsers");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UserReadDto>>();
                }
                
                // Handle 401 Unauthorized or 403 Forbidden
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Optionally force logout if token is expired/invalid
                    await _authService.Logout(); 
                }

                // Log or handle other error status codes
                return null; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retrieves a single user by ID (requires Admin role). Maps to [HttpGet("{id}")].
        /// </summary>
        public async Task<UserReadDto?> GetUserByIdAsync(int id)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/User/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserReadDto>();
            }

            return null;
        }
        public async Task<UserReadDto?> UpdateUserAsync(UserUpdateDto model)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync("api/User/update", model);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserReadDto>();
            }
            
            return null;
        }

        /// <summary>
        /// Deletes a user by ID (requires Admin role). Maps to [HttpDelete("delete")].
        /// </summary>
       public async Task DeleteUserAsync(int id) 
{
    await SetAuthorizationHeader(); // Ensure the token is set

    // The API route is "api/user/delete", expecting the ID in the body (per your controller definition)
    var deleteDto = new UserDeleteDto { Id = id };
    
    var request = new HttpRequestMessage(HttpMethod.Delete, "api/user/delete")
    {
        Content = JsonContent.Create(deleteDto)
    };
    
    var response = await _httpClient.SendAsync(request);
    
    if (response.IsSuccessStatusCode)
    {
        return; // Success (200, 204)
    }
    
    // --- Error Handling for Foreign Key Constraint ---
    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict) // 409 Conflict
    {
        // 1. Read the custom JSON error body from the API
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(); 
        
        // 2. Throw a specific, client-friendly exception using the message from the API
        // This message is the one you customized in the UserController.
        throw new InvalidOperationException(errorResponse?.Message ?? "Cannot delete user due to existing linked records.");
    }
    // --- End of Foreign Key Handling ---
    
    else 
    {
        // Handle other non-successful status codes (400, 404, 401, 500)
        var content = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Deletion failed: API returned status code {(int)response.StatusCode}. Details: {content}");
    }
}}}