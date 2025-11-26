// Controllers/SubjectController.cs
using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class SubjectsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SubjectsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private ISubjectRepository Repository => _unitOfWork.Subjects;

    // -------------------------------------------
    // C R E A T E (POST)
    // -------------------------------------------
    [HttpPost("createSubject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDTO>> CreateSubject([FromBody] CreateSubjectDto createDto)
    {
        if (await Repository.CodeExistsAsync(createDto.RegistrationCode))
        {
            return Conflict($"Registration Code '{createDto.RegistrationCode}' already exists.");
        }

        var SubjectEntity = new Subject
        {
            RegistrationCode = createDto.RegistrationCode,
            Title = createDto.SubjectName,
            TeacherId = createDto.TeacherId
        };

        var createdEntity = await Repository.AddAsync(SubjectEntity);
        await _unitOfWork.CompleteAsync();

        var entityWithTeacher = await Repository.GetByIdWithTeacherAndEnrollmentsAsync(createdEntity.Id);

        if (entityWithTeacher == null) return NotFound();

        var returnDto = new SubjectDTO
        {
            Id = entityWithTeacher.Id,
            RegistrationCode = entityWithTeacher.RegistrationCode,
            SubjectName = entityWithTeacher.Title,
            TeacherId = entityWithTeacher.TeacherId,
            TeacherFullName = entityWithTeacher.Teacher?.Name + " " + entityWithTeacher.Teacher?.LastName,
            EnrollmentCount = 0
        };

        // Uses the simple GET method defined below
        return CreatedAtAction(nameof(GetSubjectById), new { id = returnDto.Id }, returnDto);
    }

    // -------------------------------------------
    // R E A D A L L (GET)
    // -------------------------------------------
    [HttpGet("getAll")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetAllSubjects()
    {
        var entities = await Repository.GetAllWithTeacherAndEnrollmentsAsync();

        var dtos = entities.Select(e => new SubjectDTO
        {
            Id = e.Id,
            RegistrationCode = e.RegistrationCode,
            SubjectName = e.Title,
            TeacherId = e.TeacherId,
            TeacherFullName = e.Teacher?.Name + " " + e.Teacher?.LastName,
            EnrollmentCount = e.Enrollments?.Count ?? 0
        }).ToList();

        return Ok(dtos);
    }

    // -------------------------------------------
    // R E A D B Y I D (GET) (Used by GetAll and CreatedAtAction)
    // -------------------------------------------
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDTO>> GetSubjectById(int id)
    {
        var entity = await Repository.GetByIdWithTeacherAndEnrollmentsAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        var dto = new SubjectDTO
        {
            Id = entity.Id,
            RegistrationCode = entity.RegistrationCode,
            SubjectName = entity.Title,
            TeacherId = entity.TeacherId,
            TeacherFullName = entity.Teacher?.Name + " " + entity.Teacher?.LastName,
            EnrollmentCount = entity.Enrollments?.Count ?? 0
        };

        return Ok(dto);
    }

    // --- Missing CRUD Actions ---

    // -------------------------------------------
    // U P D A T E (PUT)
    // -------------------------------------------
    [HttpPut("updateSubject/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubject(int id, UpdateSubjectDTO updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest("ID mismatch between route and request body.");
        }

        var existingEntity = await Repository.GetByIdAsync(id);

        if (existingEntity == null)
        {
            return NotFound();
        }

        // 1. Map DTO changes onto the existing Entity
        existingEntity.Title = updateDto.SubjectName;
        existingEntity.TeacherId = updateDto.TeacherId;

        // 2. Mark as modified and commit
        await Repository.UpdateAsync(existingEntity);
        await _unitOfWork.CompleteAsync();

        // Return 204 No Content for successful update
        return NoContent();
    }

    // -------------------------------------------
    // D E L E T E (DELETE)
    // -------------------------------------------
    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        // DeleteAsync finds the entity and removes it
        var deleted = await Repository.DeleteAsync(id);

        // Commit the transaction to save the deletion (assuming DeleteAsync does NOT call SaveChangesAsync)
        // NOTE: If your Repository.DeleteAsync calls SaveChangesAsync internally, remove this line.
        // Based on your provided Repository, it seems DeleteAsync only removes the entity.
        await _unitOfWork.CompleteAsync();

        if (!deleted)
        {
            return NotFound();
        }

        // Return 204 No Content for successful deletion
        return NoContent();
    }
    [HttpGet("mySections")]
    [Authorize(Roles = "Teacher")] // Secured for the Teacher role
    public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetSubjectsByTeacher()
    {
        // 1. Get Teacher ID from JWT claims
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int teacherId))
        {
            return Unauthorized("Teacher identity could not be retrieved from the token.");
        }

        // 2. Retrieve entities assigned to this teacher, including enrollments
        var entities = await Repository.GetSubjectsByTeacherIdWithEnrollmentsAsync(teacherId);

        // 3. Map Entity List to DTO List
        var dtos = entities.Select(e => new SubjectDTO
        {
            Id = e.Id,
            RegistrationCode = e.RegistrationCode,
            SubjectName = e.Title,
            TeacherId = e.TeacherId,
            TeacherFullName = e.Teacher?.Name + " " + e.Teacher?.LastName,
            // Calculate Enrollment Count from the loaded collection
            EnrollmentCount = e.Enrollments?.Count ?? 0
        }).ToList();

        return Ok(dtos);
    }
}