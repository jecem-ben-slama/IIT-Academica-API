using IIT_Academica_API.Entities;
using IIT_Academica_API.Models.DTOs;
using IIT_Academica_DTOs.Enrollment_DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private IEnrollmentRepository Repository => _unitOfWork.Enrollments;

    private bool TryGetStudentId(out int studentId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out studentId);
    }

    //^ Enroll
    [HttpPost("enroll")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetStudentId(out int studentId)) return Unauthorized("User identity could not be retrieved.");


        var subject = await _unitOfWork.Subjects.GetByIdAsync(dto.SubjectId);

        if (subject == null)
        {
            return BadRequest(new { Message = $"Subject with ID {dto.SubjectId} not found." });
        }

        if (subject.RegistrationCode != dto.RegistrationCode)
        {
            return BadRequest(new { Message = "Invalid registration code provided for the specified Subject ID." });
        }


        var alreadyEnrolled = await Repository.IsStudentAlreadyEnrolledAsync(studentId, subject.Id);
        if (alreadyEnrolled) return Conflict(new { Message = "Student is already enrolled in this course section." });

        var newEnrollment = new Enrollment
        {
            StudentId = studentId,
            SubjectId = subject.Id,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Active"
        };

        await Repository.AddEnrollmentAsync(newEnrollment);
        await _unitOfWork.CompleteAsync();

        var responseDto = new EnrollmentResponseDto
        {
            EnrollmentId = newEnrollment.Id,
            StudentId = newEnrollment.StudentId,
            SubjectId = newEnrollment.SubjectId,
            SubjectTitle = subject.Title,
            EnrollmentDate = newEnrollment.EnrollmentDate,
            Status = newEnrollment.Status
        };

        return CreatedAtAction(nameof(Enroll), new { id = responseDto.EnrollmentId }, responseDto);
    }
    //^ MyCourses
    [HttpGet("myCourses")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentCourseDto>>> GetStudentEnrollments()
    {
        if (!TryGetStudentId(out int studentId)) return Unauthorized("User identity could not be retrieved.");

        var enrollments = await Repository.GetEnrollmentsByStudentIdAsync(studentId);

        var dtos = enrollments.Select(e => new StudentCourseDto
        {
            EnrollmentId = e.Id,
            RegistrationCode = e.Subject.RegistrationCode,
            CourseTitle = e.Subject.Title,
            EnrollmentDate = e.EnrollmentDate,
            TeacherFullName = e.Subject.Teacher?.Name + " " + e.Subject.Teacher?.LastName,
            SubjectId = e.Subject.Id
        }).ToList();

        return Ok(dtos);
    }

    //^ Drop
    [HttpDelete("drop/{enrollmentId}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DropCourse(int enrollmentId)
    {
        if (!TryGetStudentId(out int studentId)) return Unauthorized("User identity could not be retrieved.");

        var deleted = await Repository.DeleteEnrollmentByStudentAndIdAsync(enrollmentId, studentId);

        await _unitOfWork.CompleteAsync();

        if (!deleted)
        {
            return NotFound("Enrollment not found or unauthorized.");
        }

        return NoContent();
    }
}