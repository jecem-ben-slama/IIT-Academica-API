using System.Net.Http.Json;
using System.Net.Http.Headers;
using IIT_Academica_Front.Models; // Ensure UserReadDto, UserUpdateDto, UserDeleteDto, AuthResponseDto, RegisterDto, and ApiErrorResponse are here

namespace IIT_Academica_Front.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService; 

        public UserService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        // Helper method to set the Authorization header
        private async Task SetAuthorizationHeader()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            // IMPORTANT: Clear the header if the token is null/empty 
            // to prevent sending old/stale headers if the underlying client is reused.
            else
            {
                 _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // --- AUTHENTICATION/REGISTER (Usually in AuthService, but kept here) ---

         public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
         {
             var response = await _httpClient.PostAsJsonAsync("api/user/register", model);
             return await response.Content.ReadFromJsonAsync<AuthResponseDto>() ?? new AuthResponseDto { IsSuccess = false, Message = "API communication error." };
         }


        // --- GET ALL USERS ---

        public async Task<List<UserReadDto>?> GetAllUsersAsync()
        {
            await SetAuthorizationHeader();
            
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
                    await _authService.Logout(); // Force logout
                }

                // Throw an exception for the UI to handle, if not 401/403
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to load users. Status: {response.StatusCode}. Details: {errorContent}");
            }
            catch (Exception ex)
            {
                // Rethrow a clear exception
                throw new HttpRequestException($"Error fetching users: {ex.Message}", ex);
            }
        }

        // --- GET USER BY ID ---

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
        
        // --- UPDATE USER ---

        public async Task<UserReadDto?> UpdateUserAsync(UserUpdateDto model)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync("api/User/update", model);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserReadDto>();
            }
            
            // Note: Consider throwing an exception here if the update fails, similar to DeleteUserAsync
            return null;
        }

        // --- DELETE USER ---

       /// <summary>
       /// Deletes a user by ID (requires Admin role). Maps to [HttpDelete("delete")].
       /// Throws InvalidOperationException on Foreign Key Conflict (409).
       /// </summary>
       public async Task DeleteUserAsync(int id) 
       {
           await SetAuthorizationHeader();

           // The API route is "api/user/delete", expecting the ID in the body
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
           
           // Handle 409 Conflict (e.g., Foreign Key Violation)
           else if (response.StatusCode == System.Net.HttpStatusCode.Conflict) 
           {
               var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(); 
               
               // Throw a specific, client-friendly exception
               throw new InvalidOperationException(errorResponse?.Message ?? "Cannot delete user due to existing linked records.");
           }
           
           else 
           {
               // Handle other non-successful status codes (400, 404, 401, 500)
               var content = await response.Content.ReadAsStringAsync();
               throw new HttpRequestException($"Deletion failed: API returned status code {(int)response.StatusCode}. Details: {content}");
           }
       }
    }
}