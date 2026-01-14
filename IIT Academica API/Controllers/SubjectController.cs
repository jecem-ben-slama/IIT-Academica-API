using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class SubjectsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public SubjectsController(IUnitOfWork unitOfWork, IEnrollmentRepository enrollmentRepository)
    {
        _unitOfWork = unitOfWork;
        _enrollmentRepository = enrollmentRepository;
    }

    private ISubjectRepository Repository => _unitOfWork.Subjects;

    //^ Create
    [HttpPost("createSubject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDTO>> CreateSubject([FromBody] CreateSubjectDto createDto)
    {
        if (await Repository.CodeExistsAsync(createDto.RegistrationCode!))
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

        return CreatedAtAction(nameof(GetSubjectById), new { id = returnDto.Id }, returnDto);
    }

    //^ GetAll
    [HttpGet("getAll")]
    [Authorize(Roles = "Admin,Student")]
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

    //^ Get By Id
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

    //^ Update
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

        existingEntity.Title = updateDto.SubjectName;
        existingEntity.TeacherId = updateDto.TeacherId;
        existingEntity.RegistrationCode = updateDto.RegistrationCode;


        await Repository.UpdateAsync(existingEntity);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    //^ Delete
    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await Repository.GetByIdAsync(id);
        if (subject == null)
        {
            return NotFound();
        }

        var hasEnrollments = await _enrollmentRepository.HasActiveEnrollmentsForSubject(id);

        if (hasEnrollments)
        {
            return Conflict(new
            {
                message = "Cannot delete subject: Active enrollments exist.",
                subjectId = id
            });
        }

        var deleted = await Repository.DeleteAsync(id);

        await _unitOfWork.CompleteAsync();

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    //^  MySections
    [HttpGet("mySections")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetSubjectsByTeacher()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int teacherId))
        {
            return Unauthorized("Teacher identity could not be retrieved from the token.");
        }

        var entities = await Repository.GetSubjectsByTeacherIdWithEnrollmentsAsync(teacherId);

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
}
