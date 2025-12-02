using System.Net.Http.Json;
using System.Net.Http.Headers;

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

        // public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        // {
        //     var response = await _httpClient.PostAsJsonAsync("api/User/register", model);
        //     return await response.Content.ReadFromJsonAsync<AuthResponseDto>() ?? new AuthResponseDto { IsSuccess = false, Message = "API communication error." };
        // }

        // public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        // {
        //     var response = await _httpClient.PostAsJsonAsync("api/User/login", model);
        //     return await response.Content.ReadFromJsonAsync<AuthResponseDto>() ?? new AuthResponseDto { IsSuccess = false, Message = "API communication error." };
        // }


        // --- ADMIN USER MANAGEMENT ENDPOINTS ---

        /// <summary>
        /// Retrieves all users (requires Admin role). Maps to [HttpGet("GetAllUsers")].
        /// </summary>
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
            
            // In a real app, you would parse the error list from the BadRequest response
            return null;
        }

        /// <summary>
        /// Deletes a user by ID (requires Admin role). Maps to [HttpDelete("delete")].
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            await SetAuthorizationHeader();

            // Note: The API endpoint uses UserDeleteDto, but typically a DELETE operation uses a URI parameter for the ID.
            // Assuming the API expects an ID either in the URI or in the body (sending it in the body for now matching the DTO usage).
            var deleteDto = new UserDeleteDto { Id = id };

            // Using SendAsync with HttpMethod.Delete and content, which is slightly less common but matches the API structure expecting a body.
            // If the API was DELETE /api/User/delete/{id}, we would use DeleteAsync($"api/User/delete/{id}") instead.
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/User/delete")
            {
                Content = JsonContent.Create(deleteDto)
            };
            
            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode; // Should return true for 204 NoContent
        }
    }

    // You need to define these DTOs in your IIT_Academica_DTOs project:
}