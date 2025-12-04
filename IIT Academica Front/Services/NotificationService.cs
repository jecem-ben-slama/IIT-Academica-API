using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;
using IIT_Academica_Front.Models; // Assumed location for DTOs
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms; // For IBrowserFile

namespace IIT_Academica_Front.Services
{
    // Make sure to define the necessary DTOs and models:
    // NotificationDto, CreateNotificationDto, ApiErrorResponse
    
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public NotificationService(HttpClient httpClient, AuthService authService)
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
        // READ OPERATIONS (Feed & By ID)
        // ===============================================

        /// <summary>
        /// Retrieves the notification feed (accessible by Admin/Student).
        /// </summary>
        public async Task<List<NotificationDto>?> GetNotificationFeedAsync()
        {
            await SetAuthorizationHeader();
            
            var response = await _httpClient.GetAsync("api/Notifications/feed");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _authService.Logout(); 
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch notification feed. Status: {response.StatusCode}. Details: {errorContent}");
        }

        /// <summary>
        /// Retrieves a single notification by ID (Admin/Student access).
        /// </summary>
        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync($"api/Notifications/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NotificationDto>();
            }

            return null; // Return null on 404 Not Found
        }
        
        // ===============================================
        // CREATE OPERATION (Admin)
        // ===============================================

        /// <summary>
        /// Creates a new notification, handling file uploads via multipart/form-data.
        /// </summary>
        public async Task<NotificationDto> CreateNotificationAsync(
            CreateNotificationDto dto, 
            IBrowserFile? imageFile, 
            IBrowserFile? attachedFile)
        {
            await SetAuthorizationHeader();

            using var content = new MultipartFormDataContent();

            // 1. Add DTO properties as form fields
            content.Add(new StringContent(dto.Title), nameof(dto.Title));
            content.Add(new StringContent(dto.Content), nameof(dto.Content));

            // 2. Add IFormFile equivalents (using IBrowserFile from Blazor)
            if (imageFile != null)
            {
                var imageContent = new StreamContent(imageFile.OpenReadStream(imageFile.Size));
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                // The name must match the parameter name in the controller: "imageFile"
                content.Add(imageContent, "imageFile", imageFile.Name);
            }

            if (attachedFile != null)
            {
                var fileContent = new StreamContent(attachedFile.OpenReadStream(attachedFile.Size));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(attachedFile.ContentType);
                 // The name must match the parameter name in the controller: "attachedFile"
                content.Add(fileContent, "attachedFile", attachedFile.Name);
            }

            var response = await _httpClient.PostAsync("api/Notifications/create", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NotificationDto>()
                    ?? throw new InvalidOperationException("Failed to deserialize created notification.");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Notification creation failed. Status: {(int)response.StatusCode}. Details: {errorContent}");
        }

        // ===============================================
        // UPDATE OPERATION (Admin)
        // ===============================================
        
        /// <summary>
        /// Updates a notification's core text content (excluding files, per controller design).
        /// </summary>
        public async Task UpdateNotificationAsync(int id, CreateNotificationDto dto)
        {
            await SetAuthorizationHeader();

            // Note: The controller takes CreateNotificationDto for simplicity, 
            // but a separate UpdateNotificationDto might be cleaner.
            var response = await _httpClient.PutAsJsonAsync($"api/Notifications/{id}", dto);

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Notification update failed. Status: {(int)response.StatusCode}. Details: {content}");
        }

        // ===============================================
        // DELETE OPERATION (Admin)
        // ===============================================

        /// <summary>
        /// Deletes a notification by ID.
        /// </summary>
        public async Task DeleteNotificationAsync(int id)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.DeleteAsync($"api/Notifications/{id}");

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Notification deletion failed. Status: {(int)response.StatusCode}. Details: {content}");
        }
        
        // ===============================================
        // DOWNLOAD OPERATION (All Users)
        // ===============================================

        /// <summary>
        /// Initiates file download for an attached notification file.
        /// </summary>
        public async Task<HttpResponseMessage> DownloadNotificationFileAsync(int id)
        {
            await SetAuthorizationHeader();

            // Ensure HttpClient does not automatically dispose of the stream
            return await _httpClient.GetAsync($"api/Notifications/{id}/download", HttpCompletionOption.ResponseHeadersRead);
            
            // The calling component will need to handle the stream response and trigger the download via JS interop.
        }
    }
}