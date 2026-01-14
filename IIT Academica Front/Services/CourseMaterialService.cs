using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;
using IIT_Academica_Front.Models; // Assumed location for DTOs
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile

namespace IIT_Academica_Front.Services
{

    public class CourseMaterialService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public CourseMaterialService(HttpClient httpClient, AuthService authService)
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

        // ===============================================
        // CREATE / UPLOAD OPERATION (Teacher)
        // ===============================================

        /// <summary>
        /// Uploads a new course material file along with its metadata via multipart/form-data.
        /// </summary>
        public async Task<CourseMaterialDto> UploadMaterialAsync(CreateCourseMaterialDto dto, IBrowserFile file)
        {
            await SetAuthorizationHeader();

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "The file to upload cannot be null.");
            }

            // MultipartFormDataContent is essential for sending both text data and a file
            using var content = new MultipartFormDataContent();

            // 1. Add DTO properties as form fields
            content.Add(new StringContent(dto.SubjectId.ToString()), nameof(dto.SubjectId));
            content.Add(new StringContent(dto.Title), nameof(dto.Title));
            content.Add(new StringContent(dto.Description), nameof(dto.Description));

            // 2. Add the file content
            // The method requires specifying the size limit for security/performance
            const long maxFileSize = 1024 * 1024 * 50; // Example: 50 MB limit
            var fileStream = file.OpenReadStream(maxFileSize);

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            // The name "file" MUST match the parameter name in the controller's UploadMaterial method: IFormFile file
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/CourseMaterials/upload", content);

            if (response.IsSuccessStatusCode)
            {
                // API returns 201 Created with the CourseMaterialDto in the body
                return await response.Content.ReadFromJsonAsync<CourseMaterialDto>()
                    ?? throw new InvalidOperationException("Failed to deserialize created course material.");
            }

            // Handle specific status codes (e.g., 403 Forbidden, 400 Bad Request)
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Material upload failed. Status: {(int)response.StatusCode}. Details: {errorContent}");
        }


        // ===============================================
        // READ OPERATIONS (By Subject & By ID)
        // ===============================================

        /// <summary>
        /// Retrieves all materials for a specific subject (Teacher/Student access).
        /// </summary>
        public async Task<List<CourseMaterialDto>?> GetMaterialsBySubjectAsync(int subjectId)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/CourseMaterials/subject/{subjectId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CourseMaterialDto>>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _authService.Logout();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch materials. Status: {response.StatusCode}. Details: {errorContent}");
        }


        /// 
        public async Task<CourseMaterialDto> UpdateMaterialAsync(
     UpdateCourseMaterialDto
      materialToUpdate,
     IBrowserFile? file = null)
        {
            HttpResponseMessage response;

            if (file != null)
            {
                // 1. Case: File is provided (Use multipart/form-data)
                using var content = new MultipartFormDataContent();

                // Add the DTO fields as StringContent
                content.Add(new StringContent(materialToUpdate.Id.ToString()), "Id");
                content.Add(new StringContent(materialToUpdate.Title), "Title");
                content.Add(new StringContent(materialToUpdate.Description), "Description");

                // Add the file as StreamContent
                var fileStreamContent = new StreamContent(file.OpenReadStream(file.Size));
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                // Add the file to the content with the name 'file' (must match IFormFile parameter name in controller)
                content.Add(fileStreamContent, "file", file.Name);

                // Send the PUT request
                response = await _httpClient.PutAsync($"api/CourseMaterials/update/{materialToUpdate.Id}", content);
            }
            else
            {
                // 2. Case: Only metadata is changing (Use standard JSON PUT, but API is now expecting form data)
                // To handle this, we still must send form data, even if the file part is empty. 
                // We create a minimal MultipartFormDataContent with just the metadata.
                using var content = new MultipartFormDataContent();

                // Add the DTO fields as StringContent
                content.Add(new StringContent(materialToUpdate.Id.ToString()), "Id");
                content.Add(new StringContent(materialToUpdate.Title), "Title");
                content.Add(new StringContent(materialToUpdate.Description), "Description");

                response = await _httpClient.PutAsync($"api/CourseMaterials/update/{materialToUpdate.Id}", content);
            }

            // Throw an exception for bad status codes
            response.EnsureSuccessStatusCode();

            // Deserialize and return the updated material DTO
            var updatedMaterial = await response.Content.ReadFromJsonAsync<CourseMaterialDto>();

            if (updatedMaterial == null)
            {
                throw new HttpRequestException("Failed to deserialize the updated course material.");
            }

            return updatedMaterial;
        }

        // stuident/teacher
        public async Task<CourseMaterialDto?> GetMaterialByIdAsync(int id)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/CourseMaterials/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CourseMaterialDto>();
            }

            return null; // Return null on 404 Not Found
        }

        // ===============================================
        // DELETE OPERATION (Teacher)
        // ===============================================

        /// <summary>
        /// Deletes a course material by ID, including the physical file on the server.
        /// </summary>
        public async Task DeleteMaterialAsync(int id)
        {
            await SetAuthorizationHeader();

            // The API route takes the ID in the path: /api/CourseMaterials/{id}
            var response = await _httpClient.DeleteAsync($"api/CourseMaterials/{id}");

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Material deletion failed. Status: {(int)response.StatusCode}. Details: {content}");
        }

        // ===============================================
        // DOWNLOAD OPERATION (Student/Teacher)
        // ===============================================

        /// <summary>
        /// Initiates a secure file download for the course material.
        /// </summary>
        public async Task<HttpResponseMessage> DownloadMaterialAsync(int materialId)
        {
            await SetAuthorizationHeader();

            // Use HttpCompletionOption.ResponseHeadersRead to get the response immediately 
            // and handle the stream manually, which is crucial for large files.
            return await _httpClient.GetAsync($"api/CourseMaterials/{materialId}/download", HttpCompletionOption.ResponseHeadersRead);

            // The calling Blazor component will need to process the stream from the HttpResponseMessage 
            // and use JavaScript interop to save the file on the user's client machine.
        }
    }
}