using System.Net.Http.Json;
using System.Net.Http.Headers;
using IIT_Academica_Front.Models; // Assumed location for DTOs
using System.Net;

namespace IIT_Academica_Front.Services
{
    // Make sure to define the necessary DTOs in IIT_Academica_Front.Models:
    // SubjectDTO, CreateSubjectDto, UpdateSubjectDTO, ApiErrorResponse
    
    public class SubjectService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public SubjectService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // --- READ ALL (Admin) ---
        
        /// <summary>
        /// Retrieves all subjects with teacher and enrollment details (Admin access).
        /// </summary>
        public async Task<List<SubjectDTO>?> GetAllSubjectsAsync()
        {
            await SetAuthorizationHeader();
            
            var response = await _httpClient.GetAsync("api/Subjects/getAll");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SubjectDTO>>();
            }
            
            // Handle Unauthorized/Forbidden
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _authService.Logout(); 
            }

            // Throw exception for component error handling
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch subjects. Status: {response.StatusCode}. Details: {errorContent}");
        }

        // --- READ BY TEACHER (Teacher) ---

        /// <summary>
        /// Retrieves subjects assigned to the currently logged-in teacher.
        /// </summary>
        public async Task<List<SubjectDTO>?> GetMySectionsAsync()
        {
            await SetAuthorizationHeader();
            
            var response = await _httpClient.GetAsync("api/Subjects/mySections");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SubjectDTO>>();
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _authService.Logout(); 
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch teacher sections. Status: {response.StatusCode}. Details: {errorContent}");
        }

        // --- READ BY ID (Admin) ---

        /// <summary>
        /// Retrieves a single subject by ID (Admin access).
        /// </summary>
        public async Task<SubjectDTO?> GetSubjectByIdAsync(int id)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/Subjects/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SubjectDTO>();
            }

            return null; // Return null on 404 Not Found or other non-success codes
        }
        
        // --- CREATE (Admin) ---

        /// <summary>
        /// Creates a new subject. Throws InvalidOperationException on 409 Conflict.
        /// </summary>
        public async Task<SubjectDTO> CreateSubjectAsync(CreateSubjectDto createDto)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("api/Subjects/createSubject", createDto);

            if (response.IsSuccessStatusCode)
            {
                // API returns 201 Created with the SubjectDTO in the body
                return await response.Content.ReadFromJsonAsync<SubjectDTO>()
                    ?? throw new InvalidOperationException("Failed to deserialize created subject.");
            }
            // Handle 409 Conflict (Duplicate Registration Code)
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(); 
                // Throw a clear exception using the message from the controller
                throw new InvalidOperationException(errorResponse?.Message ?? $"Subject creation failed: Registration Code already exists.");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Subject creation failed: API returned status code {(int)response.StatusCode}. Details: {content}");
            }
        }

        // --- UPDATE (Admin) ---

        /// <summary>
        /// Updates an existing subject.
        /// </summary>
        public async Task UpdateSubjectAsync(UpdateSubjectDTO updateDto)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync($"api/Subjects/updateSubject/{updateDto.Id}", updateDto);

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Subject update failed: API returned status code {(int)response.StatusCode}. Details: {content}");
        }

        // --- DELETE (Admin) ---

        /// <summary>
        /// Deletes a subject by ID.
        /// </summary>
        public async Task DeleteSubjectAsync(int id)
        {
            await SetAuthorizationHeader();

            // The API route takes the ID in the path: /api/Subjects/delete/{id}
            var response = await _httpClient.DeleteAsync($"api/Subjects/delete/{id}");

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }

            // No specific 409 handling was defined in your controller for delete, 
            // so we handle it as a general error.
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Subject deletion failed: API returned status code {(int)response.StatusCode}. Details: {content}");
        }
    }
}