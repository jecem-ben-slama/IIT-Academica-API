using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;

using IIT_Academica_DTOs.Enrollment_DTOs;
using IIT_Academica_API.Models.DTOs;
using IIT_Academica_Front.Models;

namespace IIT_Academica_Front.Services
{
    // Make sure to define the necessary DTOs in IIT_Academica_Front.Models:
    // EnrollmentRequestDto, EnrollmentResponseDto, StudentCourseDto, ApiErrorResponse

    public class EnrollmentService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public EnrollmentService(HttpClient httpClient, AuthService authService)
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
        // ENROLL (POST) - /api/enrollment/enroll
        // ===============================================

        /// <summary>
        /// Enrolls the current student into a course using Subject ID and Registration Code.
        /// Throws InvalidOperationException on 409 Conflict (already enrolled).
        /// </summary>
        public async Task<EnrollmentResponseDto> EnrollAsync(EnrollmentRequestDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("api/Enrollment/enroll", dto);

            if (response.IsSuccessStatusCode)
            {
                // API returns 201 Created with the EnrollmentResponseDto in the body
                return await response.Content.ReadFromJsonAsync<EnrollmentResponseDto>()
                    ?? throw new InvalidOperationException("Failed to deserialize enrollment response.");
            }
            // Handle 409 Conflict (Already enrolled)
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // The API sends a JSON response like { Message: "..." }
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                // Throw a clear exception using the message from the controller
                throw new InvalidOperationException(errorResponse?.Message ?? "You are already enrolled in this subject.");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                // Handle specific bad request errors (e.g., Invalid registration code, Subject not found)
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                throw new ArgumentException(errorResponse?.Message ?? "Enrollment failed due to bad data or invalid code.");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Enrollment failed. Status: {(int)response.StatusCode}. Details: {content}");
            }
        }

        // ===============================================
        // READ ALL (GET) - /api/enrollment/myCourses
        // ===============================================

        /// <summary>
        /// Retrieves all courses the current student is enrolled in.
        /// </summary>
        public async Task<List<StudentCourseDto>?> GetStudentEnrollmentsAsync()
        {
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("api/Enrollment/myCourses");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<StudentCourseDto>>();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _authService.Logout();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch my courses. Status: {response.StatusCode}. Details: {errorContent}");
        }

        // ===============================================
        // DROP COURSE (DELETE) - /api/enrollment/drop/{enrollmentId}
        // ===============================================

        /// <summary>
        /// Drops the student from a course by deleting the enrollment record.
        /// </summary>
        public async Task DropCourseAsync(int enrollmentId)
        {
            await SetAuthorizationHeader();

            // The API route takes the ID in the path: /api/enrollment/drop/{enrollmentId}
            var response = await _httpClient.DeleteAsync($"api/Enrollment/drop/{enrollmentId}");

            if (response.IsSuccessStatusCode)
            {
                return; // 204 No Content is expected
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // The enrollment wasn't found or didn't belong to the student (as determined by the controller)
                throw new KeyNotFoundException($"Enrollment ID {enrollmentId} not found or you are unauthorized to drop it.");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Course drop failed. Status: {(int)response.StatusCode}. Details: {content}");
            }
        }
    }
}