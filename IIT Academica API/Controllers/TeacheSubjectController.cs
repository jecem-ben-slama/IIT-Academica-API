using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
[ApiController]
public class TeacherSubjectsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherSubjectsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Helper property to access the specific repository via the Unit of Work
    private ITeacherSubjectRepository Repository => _unitOfWork.TeacherSubjects;

    // -------------------------------------------
    // C R E A T E (POST)
    // -------------------------------------------
    [HttpPost("createSubject")]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<TeacherSubjectDto>> CreateTeacherSubject([FromBody] CreateTeacherSubjectDto createDto)
    {
        // 1. Validation Check: Code Exists
        if (await Repository.CodeExistsAsync(createDto.RegistrationCode))
        {
            return Conflict($"Registration Code '{createDto.RegistrationCode}' already exists.");
        }

        // 2. Map DTO to Entity (FIXED: Mapping SubjectName to Entity.Title)
        var teacherSubjectEntity = new TeacherSubject
        {
            RegistrationCode = createDto.RegistrationCode,
            Title = createDto.SubjectName, // <-- FIX: Use Title property
            TeacherId = createDto.TeacherId
        };

        // 3. Add Entity (Tracked by DbContext)
        var createdEntity = await Repository.AddAsync(teacherSubjectEntity);

        // 4. Commit Transaction
        await _unitOfWork.CompleteAsync();

        // 5. Retrieve entity with Teacher details for the response DTO
        var entityWithTeacher = await Repository.GetByIdWithTeacherAsync(createdEntity.Id);

        if (entityWithTeacher == null) return NotFound();

        // 6. Map Entity back to DTO for response (FIXED: Mapping Entity.Title to DTO.SubjectName)
        var returnDto = new TeacherSubjectDto
        {
            Id = entityWithTeacher.Id,
            RegistrationCode = entityWithTeacher.RegistrationCode,
            SubjectName = entityWithTeacher.Title, // <-- FIX: Use Title property
            TeacherId = entityWithTeacher.TeacherId,
            // Assuming ApplicationUser has FirstName and LastName properties
            TeacherFullName = entityWithTeacher.Teacher?.Name + " " + entityWithTeacher.Teacher?.LastName
        };

        // 7. Return 201 Created status
        return CreatedAtAction(nameof(GetTeacherSubjectById), new { id = returnDto.Id }, returnDto);
    }

    // -------------------------------------------
    // R E A D A L L (GET)
    // -------------------------------------------
    [HttpGet("getAll")]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<IEnumerable<TeacherSubjectDto>>> GetAllTeacherSubjects()
    {
        var entities = await Repository.GetAllAsync();

        // Map Entity List to DTO List (FIXED: Mapping Entity.Title to DTO.SubjectName)
        var dtos = entities.Select(e => new TeacherSubjectDto
        {
            Id = e.Id,
            RegistrationCode = e.RegistrationCode,
            SubjectName = e.Title, // <-- FIX: Use Title property
            TeacherId = e.TeacherId,
            TeacherFullName = e.Teacher?.Name + " " + e.Teacher?.LastName
        }).ToList();

        return Ok(dtos);
    }

    // -------------------------------------------
    // R E A D B Y I D (GET)
    // -------------------------------------------
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<TeacherSubjectDto>> GetTeacherSubjectById(int id)
    {
        var entity = await Repository.GetByIdWithTeacherAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        // Map Entity to DTO (FIXED: Mapping Entity.Title to DTO.SubjectName)
        var dto = new TeacherSubjectDto
        {
            Id = entity.Id,
            RegistrationCode = entity.RegistrationCode,
            SubjectName = entity.Title, // <-- FIX: Use Title property
            TeacherId = entity.TeacherId,
            TeacherFullName = entity.Teacher?.Name + " " + entity.Teacher?.LastName
        };

        return Ok(dto);
    }

    // -------------------------------------------
    // U P D A T E (PUT)
    // -------------------------------------------
    [HttpPut("updatesubject/{id}")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> UpdateTeacherSubject(int id, [FromBody] UpdateTeacherSubjectDto updateDto)
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

        // 1. Map DTO changes onto the existing Entity (FIXED: Mapping DTO.SubjectName to Entity.Title)
        existingEntity.Title = updateDto.SubjectName; // <-- FIX: Use Title property
        existingEntity.TeacherId = updateDto.TeacherId;

        // 2. Mark as modified (handled by repository method, tracked by DbContext)
        await Repository.UpdateAsync(existingEntity);

        // 3. Commit Transaction
        await _unitOfWork.CompleteAsync();

        // Return 204 No Content for successful update
        return NoContent();
    }

    // -------------------------------------------
    // D E L E T E (DELETE)
    // -------------------------------------------
    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> DeleteTeacherSubject(int id)
    {
        // The Repository.DeleteAsync method performs the necessary steps (Find and Remove)
        var deleted = await Repository.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        // Assuming your DeleteAsync internally calls SaveChangesAsync, 
        // if not, you would uncomment the line below:
        // await _unitOfWork.CompleteAsync(); 

        // Return 204 No Content for successful deletion
        return NoContent();
    }
}