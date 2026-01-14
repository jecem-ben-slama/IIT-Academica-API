using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class CourseMaterialsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    public CourseMaterialsController(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    private ICourseMaterialRepository Repository => _unitOfWork.courseMaterials;
    private ISubjectRepository SubjectRepository => _unitOfWork.Subjects;
    private IEnrollmentRepository EnrollmentRepository => _unitOfWork.Enrollments;

    private async Task<bool> IsStudentEnrolledInSubject(int studentId, int subjectId)
    {
        return await EnrollmentRepository.IsStudentAlreadyEnrolledAsync(studentId, subjectId);
    }

    private bool TryGetTeacherId(out int teacherId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out teacherId);
    }
    private bool TryGetStudentId(out int studentId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out studentId);
    }

    private async Task<bool> IsTeacherAuthorizedForSubject(int subjectId)
    {
        if (!TryGetTeacherId(out int teacherId)) return false;

        var subject = await SubjectRepository.GetByIdAsync(subjectId);
        return subject != null && subject.TeacherId == teacherId;
    }

    //^ Upload Material (POST)
    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<CourseMaterialDto>> UploadMaterial(
    [FromForm] CreateCourseMaterialDto dto,
    IFormFile file)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetTeacherId(out int teacherId)) return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required for upload.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest("Description is required to serve as the file name base.");
        }

        if (!await IsTeacherAuthorizedForSubject(dto.SubjectId))
        {
            return Forbid("You are not authorized to upload materials for this subject.");
        }

        string fileUrl;
        try
        {
            fileUrl = await _fileStorageService.SaveFileAsync(file, dto.SubjectId, dto.Description);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"File upload failed: {ex.Message}");
        }

        var material = new CourseMaterial
        {
            SubjectId = dto.SubjectId,
            Title = dto.Title,
            Description = dto.Description,
            FilePathOrUrl = fileUrl,
            MaterialType = file.ContentType,
            UploadDate = DateTime.UtcNow
        };

        await Repository.AddAsync(material);
        await _unitOfWork.CompleteAsync();

        var materialDto = new CourseMaterialDto
        {
            Id = material.Id,
            SubjectId = material.SubjectId,
            Title = material.Title,
            Description = material.Description,
            FilePathOrUrl = material.FilePathOrUrl,
            MaterialType = material.MaterialType,
            UploadDate = material.UploadDate
        };

        return CreatedAtAction(nameof(GetMaterialById), new { id = materialDto.Id }, materialDto);
    }
    //^ Get Materials by Subject
    [HttpGet("subject/{subjectId}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<IEnumerable<CourseMaterialDto>>> GetMaterialsBySubject(int subjectId)
    {
        var materials = await Repository.GetBySubjectIdAsync(subjectId);

        var dtos = materials.Select(m => new CourseMaterialDto
        {
            Id = m.Id,
            SubjectId = m.SubjectId,
            Title = m.Title,
            Description = m.Description,
            FilePathOrUrl = m.FilePathOrUrl,
            MaterialType = m.MaterialType,
            UploadDate = m.UploadDate
        }).ToList();

        return Ok(dtos);
    }

    //^ Get By Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<CourseMaterialDto>> GetMaterialById(int id)
    {
        var material = await Repository.GetByIdAsync(id);
        if (material == null) return NotFound();

        var dto = new CourseMaterialDto
        {
            Id = material.Id,
            SubjectId = material.SubjectId,
            Title = material.Title,
            Description = material.Description,
            FilePathOrUrl = material.FilePathOrUrl,
            MaterialType = material.MaterialType,
            UploadDate = material.UploadDate
        };
        return Ok(dto);
    }

    //^ Update
    [HttpPut("update/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<CourseMaterialDto>> UpdateMaterial(
        int id,
        [FromForm] UpdateCourseMaterialDto dto,
        IFormFile? file = null)
    {
        if (id != dto.Id) return BadRequest("Material ID mismatch.");
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetTeacherId(out int currentTeacherId)) return Unauthorized();

        var material = await Repository.GetByIdAsync(id);
        if (material == null) return NotFound("Course material not found.");

        if (!await IsTeacherAuthorizedForSubject(material.SubjectId))
        {
            return Forbid("You are not authorized to edit materials for this subject.");
        }

        material.Title = dto.Title;
        material.Description = dto.Description;

        if (file != null && file.Length > 0)
        {

            if (!string.IsNullOrEmpty(material.FilePathOrUrl))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(material.FilePathOrUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Old file deletion failed: {ex.Message}");
                }
            }
            string newFileUrl;
            try
            {
                newFileUrl = await _fileStorageService.SaveFileAsync(file, material.SubjectId, material.Description);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"File replacement failed: {ex.Message}");
            }

            material.FilePathOrUrl = newFileUrl;
            material.MaterialType = file.ContentType;
        }
        await Repository.UpdateAsync(material);
        await _unitOfWork.CompleteAsync();
        var materialDto = new CourseMaterialDto
        {
            Id = material.Id,
            SubjectId = material.SubjectId,
            Title = material.Title,
            Description = material.Description,
            FilePathOrUrl = material.FilePathOrUrl,
            MaterialType = material.MaterialType,
            UploadDate = material.UploadDate
        };

        return Ok(materialDto);
    }
    //^ Delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var (subjectTeacherId, filePathOrUrl) = await Repository.GetTeacherAndFilePathForDeletionAsync(id);

        if (!TryGetTeacherId(out int currentTeacherId)) return Unauthorized();

        if (subjectTeacherId == null || subjectTeacherId != currentTeacherId)
        {
            return Forbid("You are not authorized to delete this material.");
        }

        if (!string.IsNullOrEmpty(filePathOrUrl))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(filePathOrUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File deletion failed for {filePathOrUrl}: {ex.Message}");
            }
        }

        await Repository.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
    //^ Download
    [HttpGet("{materialId}/download")]
    //    [Authorize(Roles = "Student,Teacher")]
    public async Task<IActionResult> DownloadMaterial(int materialId)
    {

        var (relativePath, subjectId, contentType) = await Repository.GetDownloadDetailsAsync(materialId);

        if (relativePath == null || !subjectId.HasValue)
        {
            return NotFound("Course material not found.");
        }

        if (User.IsInRole("Student"))
        {
            if (!TryGetStudentId(out int studentId) || !await IsStudentEnrolledInSubject(studentId, subjectId.Value))
            {
                return Forbid("Access denied. You must be enrolled in this subject to download materials.");
            }
        }
        else if (User.IsInRole("Teacher"))
        {
            if (!await IsTeacherAuthorizedForSubject(subjectId.Value))
            {
                return Forbid("Access denied. You are not assigned to this subject.");
            }
        }

        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The file content could not be located on the server.");
        }


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        string fileName = Path.GetFileName(relativePath);

        return File(fileStream, contentType ?? "application/octet-stream", fileName);
    }
}