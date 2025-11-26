// Controllers/TeacherSubjectsController.cs
using IIT_Academica_API.Entities;
using IIT_Academica_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    // C R E A T E (POST) - EnrollmentCount is 0
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

        // Must call the new method to load Teacher (and Enrollments, though count is 0 here)
        var entityWithTeacher = await Repository.GetByIdWithTeacherAndEnrollmentsAsync(createdEntity.Id);

        if (entityWithTeacher == null) return NotFound();

        var returnDto = new SubjectDTO
        {
            Id = entityWithTeacher.Id,
            RegistrationCode = entityWithTeacher.RegistrationCode,
            SubjectName = entityWithTeacher.Title,
            TeacherId = entityWithTeacher.TeacherId,
            TeacherFullName = entityWithTeacher.Teacher?.Name + " " + entityWithTeacher.Teacher?.LastName,
            EnrollmentCount = 0 // Newly created subject has 0 enrollments
        };

        return CreatedAtAction(nameof(GetSubjectById), new { id = returnDto.Id }, returnDto);
    }

    // -------------------------------------------
    // R E A D A L L (GET) - Uses GetAllWithTeacherAndEnrollmentsAsync
    // -------------------------------------------
    [HttpGet("getAll")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetAllSubjects()
    {
        // 🚀 NEW: Call the method that includes Enrollments
        var entities = await Repository.GetAllWithTeacherAndEnrollmentsAsync();

        var dtos = entities.Select(e => new SubjectDTO
        {
            Id = e.Id,
            RegistrationCode = e.RegistrationCode,
            SubjectName = e.Title,
            TeacherId = e.TeacherId,
            TeacherFullName = e.Teacher?.Name + " " + e.Teacher?.LastName,
            // 🚀 NEW: Use the loaded collection count
            EnrollmentCount = e.Enrollments?.Count ?? 0
        }).ToList();

        return Ok(dtos);
    }

    // -------------------------------------------
    // R E A D B Y I D (GET) - Uses GetByIdWithTeacherAndEnrollmentsAsync
    // -------------------------------------------
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDTO>> GetSubjectById(int id)
    {
        // 🚀 NEW: Call the method that includes Enrollments
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
            // 🚀 NEW: Use the loaded collection count
            EnrollmentCount = entity.Enrollments?.Count ?? 0
        };

        return Ok(dto);
    }

    // ... UPDATE and DELETE methods (No change needed) ...

}