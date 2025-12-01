// Controllers/CourseMaterialsController.cs
using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class CourseMaterialsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService; // 🚀 NEW DEPENDENCY

    public CourseMaterialsController(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService; // 🚀 Assign NEW
    }

    private ICourseMaterialRepository Repository => _unitOfWork.courseMaterials;
    private ISubjectRepository SubjectRepository => _unitOfWork.Subjects;
    private IEnrollmentRepository EnrollmentRepository => _unitOfWork.Enrollments; // Optional: Accessor

    // 🚀 FIX: This method will now resolve the name error and use the repository.
    private async Task<bool> IsStudentEnrolledInSubject(int studentId, int subjectId)
    {
        // Calling the existing method from the Enrollment Repository
        return await EnrollmentRepository.IsStudentAlreadyEnrolledAsync(studentId, subjectId);
    }

    // Helper to get Teacher ID from claims
    private bool TryGetTeacherId(out int teacherId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out teacherId);
    }
    private bool TryGetStudentId(out int studentId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out studentId);
    }

    // Helper for Teacher Ownership Validation
    private async Task<bool> IsTeacherAuthorizedForSubject(int subjectId)
    {
        if (!TryGetTeacherId(out int teacherId)) return false;

        var subject = await SubjectRepository.GetByIdAsync(subjectId);
        return subject != null && subject.TeacherId == teacherId;
    }

    // -------------------------------------------
    // C R E A T E (POST)
    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    // 🚀 Method now uses [FromForm] for metadata and IFormFile for the binary data
    public async Task<ActionResult<CourseMaterialDto>> UploadMaterial(
    [FromForm] CreateCourseMaterialDto dto,
    IFormFile file)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetTeacherId(out int teacherId)) return Unauthorized();

        // 1. Validate the file
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required for upload.");
        }

        // Ensure the description is provided for the filename base
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            // Require a description since it's used for the file name
            return BadRequest("Description is required to serve as the file name base.");
        }

        // 2. SECURITY CHECK: Ensure the teacher is authorized
        if (!await IsTeacherAuthorizedForSubject(dto.SubjectId))
        {
            return Forbid("You are not authorized to upload materials for this subject.");
        }

        // 3. 💾 SAVE THE FILE TO EXTERNAL STORAGE
        string fileUrl;
        try
        {
            // 🚀 FIX: Pass dto.Description as the third argument (fileNameBase)
            fileUrl = await _fileStorageService.SaveFileAsync(file, dto.SubjectId, dto.Description);
        }
        catch (Exception ex)
        {
            // Handle specific file storage errors here
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"File upload failed: {ex.Message}");
        }

        // 4. Create DB entity using the returned URL/Path
        var material = new CourseMaterial
        {
            SubjectId = dto.SubjectId,
            Title = dto.Title,
            Description = dto.Description,
            FilePathOrUrl = fileUrl, // 🚀 Saved URL from the file service
            MaterialType = file.ContentType, // Use the file's MIME type
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

    [HttpGet("subject/{subjectId}")]
    [Authorize(Roles = "Teacher,Student")] // Both roles can view materials
    public async Task<ActionResult<IEnumerable<CourseMaterialDto>>> GetMaterialsBySubject(int subjectId)
    {
        // For security, a quick check might be needed here to ensure the user is enrolled or teaches the course,
        // but for read-only access, simply checking if the subject exists might suffice initially.
        // A robust API would check: (IsTeacherAuthorizedForSubject OR IsStudentEnrolledInSubject)

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

    // -------------------------------------------
    // R E A D B Y I D (GET) (Helper for CreatedAtAction)
    // -------------------------------------------
    [HttpGet("{id}")]
    [Authorize(Roles = "Teacher,Student")]
    public async Task<ActionResult<CourseMaterialDto>> GetMaterialById(int id)
    {
        var material = await Repository.GetByIdAsync(id);
        if (material == null) return NotFound();

        // Simple DTO mapping (can be enhanced with AutoMapper)
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

    // -------------------------------------------
    // D E L E T E (DELETE)
    // -------------------------------------------
    // Controllers/CourseMaterialsController.cs

    // ... (ensure your controller signature includes IFileStorageService) ...
    // public CourseMaterialsController(IUnitOfWork unitOfWork, IFileStorageService fileStorageService) { ... }
    // ...

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        // 1. Get security and file path info from the database
        var (subjectTeacherId, filePathOrUrl) = await Repository.GetTeacherAndFilePathForDeletionAsync(id);

        // 2. SECURITY CHECK: Ensure the teacher owns the material's subject
        if (!TryGetTeacherId(out int currentTeacherId)) return Unauthorized();

        if (subjectTeacherId == null || subjectTeacherId != currentTeacherId)
        {
            // Material doesn't exist (null) or doesn't belong to the current teacher (mismatch)
            return Forbid("You are not authorized to delete this material.");
        }

        // 3. PHYSICAL FILE DELETION
        if (!string.IsNullOrEmpty(filePathOrUrl))
        {
            try
            {
                // 🚀 Call the file storage service to remove the file
                await _fileStorageService.DeleteFileAsync(filePathOrUrl);
            }
            catch (Exception ex)
            {
                // Log the error. In most cases, you still want to delete the DB record
                // to prevent the API from offering a non-existent file, but logging is essential.
                System.Diagnostics.Debug.WriteLine($"File deletion failed for {filePathOrUrl}: {ex.Message}");
            }
        }

        // 4. Delete the material record from the database
        // Repository.DeleteAsync marks the entity for deletion
        await Repository.DeleteAsync(id);
        await _unitOfWork.CompleteAsync(); // Saves both the file deletion result and the DB change

        return NoContent();
    }
    [HttpGet("{materialId}/download")]
    [Authorize(Roles = "Student,Teacher")] // Only enrolled students and teachers can download
    public async Task<IActionResult> DownloadMaterial(int materialId)
    {
        // 1. Get file metadata from DB
        var (relativePath, subjectId, contentType) = await Repository.GetDownloadDetailsAsync(materialId);

        if (relativePath == null || !subjectId.HasValue)
        {
            return NotFound("Course material not found.");
        }

        // 2. SECURITY CHECK: Verify access
        if (User.IsInRole("Student"))
        {
            if (!TryGetStudentId(out int studentId) || !await IsStudentEnrolledInSubject(studentId, subjectId.Value))
            {
                return Forbid("Access denied. You must be enrolled in this subject to download materials.");
            }
        }
        else if (User.IsInRole("Teacher"))
        {
            // Teachers must be assigned to the subject (IsTeacherAuthorizedForSubject already exists)
            if (!await IsTeacherAuthorizedForSubject(subjectId.Value))
            {
                return Forbid("Access denied. You are not assigned to this subject.");
            }
        }

        // 3. Construct the absolute file path
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

        // 4. Check if the physical file exists
        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The file content could not be located on the server.");
        }

        // 5. Stream the file
        // Use FileStream to read the file
        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        // Use the file name for the download
        string fileName = Path.GetFileName(relativePath);

        // Returns the file stream directly to the client
        // For a PDF, the ContentType should be "application/pdf"
        return File(fileStream, contentType ?? "application/octet-stream", fileName);
    }
}