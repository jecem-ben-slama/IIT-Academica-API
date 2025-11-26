// Controllers/EnrollmentController.cs
using IIT_Academica_API.Entities;
using IIT_Academica_API.Models.DTOs;
using IIT_Academica_DTOs.Enrollment_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")] // Base route: api/Enrollment
public class EnrollmentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ----------------------------------------------------------------------
    // POST /api/enrollment/enroll
    // Allows an authenticated Student to enroll in a subject via a unique code.
    // ----------------------------------------------------------------------
    [HttpPost("enroll")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int studentId))
        {
            return Unauthorized("User identity could not be retrieved from the token.");
        }

        try
        {
            // A. Find the Subject Section by its unique registration code
            var Subject = await _unitOfWork.Enrollments
                .GetSubjectByRegistrationCodeAsync(dto.RegistrationCode);

            if (Subject == null)
            {
                return BadRequest(new { Message = "Invalid registration code provided." });
            }

            // B. Check if the student is already enrolled in this specific section
            var alreadyEnrolled = await _unitOfWork.Enrollments
                .IsStudentAlreadyEnrolledAsync(studentId, Subject.Id);

            if (alreadyEnrolled)
            {
                return Conflict(new { Message = "Student is already enrolled in this course section." });
            }

            // C. Create the Enrollment Entity
            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                SubjectId = Subject.Id,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active"
            };

            // D. Add to the context and commit the transaction
            await _unitOfWork.Enrollments.AddEnrollmentAsync(newEnrollment);
            await _unitOfWork.CompleteAsync(); // Saves the new Enrollment entity

            // 4. MAP ENTITY TO DTO AND RETURN 201 CREATED
            var responseDto = new EnrollmentResponseDto
            {
                EnrollmentId = newEnrollment.Id,
                StudentId = newEnrollment.StudentId,
                SubjectId = newEnrollment.SubjectId,
                SubjectTitle = Subject.Title,
                EnrollmentDate = newEnrollment.EnrollmentDate
            };

            return CreatedAtAction(
                nameof(Enroll),
                new { id = responseDto.EnrollmentId },
                responseDto
            );
        }
        catch (Exception ex)
        {
            // Log exception here
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during enrollment.");
        }
    }
}