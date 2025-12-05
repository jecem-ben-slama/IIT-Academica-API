// Controllers/NotificationsController.cs
using IIT_Academica_API.Entities;
using IIT_Academica_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // IFormFile
using System.Security.Claims;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public NotificationsController(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    private INotificationRepository Repository => _unitOfWork.Notifications;
    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    // Constant ID used for Notification folder in the file system (e.g., Uploads/Subject-9999/)
    private const int NotificationFolderId = 9999;

    // ===============================================
    // ADMIN ENDPOINTS (CRUD & Upload)
    // ===============================================

    // C R E A T E (POST) - /api/Notifications/create
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NotificationDto>> PostNotification(
        [FromForm] CreateNotificationDto dto,
        IFormFile? imageFile,
        IFormFile? attachedFile)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetUserId(out int adminId)) return Unauthorized();

        string? imageUrl = null;
        string? fileUrl = null;

        try
        {
            // 1. Handle Image File Upload (Displayable)
            if (imageFile != null && imageFile.Length > 0)
            {
                imageUrl = await _fileStorageService.SaveFileAsync(
                    imageFile,
                    NotificationFolderId,
                    dto.Title + "_Image");
            }

            // 2. Handle Attached File Upload (Downloadable)
            if (attachedFile != null && attachedFile.Length > 0)
            {
                fileUrl = await _fileStorageService.SaveFileAsync(
                    attachedFile,
                    NotificationFolderId,
                    dto.Title + "_Attachment");
            }
        }
        catch (Exception ex)
        {
            // Clean up any partially saved files
            if (!string.IsNullOrEmpty(imageUrl))
                await _fileStorageService.DeleteFileAsync(imageUrl);
            if (!string.IsNullOrEmpty(fileUrl))
                await _fileStorageService.DeleteFileAsync(fileUrl);

            return StatusCode(StatusCodes.Status500InternalServerError, $"File upload failed: {ex.Message}");
        }

        var entity = new Notification
        {
            Title = dto.Title,
            Content = dto.Content,
            ImageUrl = imageUrl,
            FileUrl = fileUrl,
            PostedDate = DateTime.UtcNow,
            PostedByUserId = adminId
        };

        await Repository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        var resultDto = new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            ImageUrl = entity.ImageUrl,
            FileUrl = entity.FileUrl,
            PostedDate = entity.PostedDate,
            PostedByUserId = entity.PostedByUserId
        };

        return CreatedAtAction(nameof(GetNotificationById), new { id = resultDto.Id }, resultDto);
    }

    // R E A D B Y I D (GET) - /api/Notifications/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<NotificationDto>> GetNotificationById(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        // Simple DTO mapping (using AutoMapper is recommended for real projects)
        var dto = new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            ImageUrl = entity.ImageUrl,
            FileUrl = entity.FileUrl,
            PostedDate = entity.PostedDate,
            PostedByUserId = entity.PostedByUserId
        };
        return Ok(dto);
    }

    // U P D A T E (PUT) - /api/Notifications/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNotification(
        int id, 
        [FromForm] CreateNotificationDto dto, // Use [FromForm] to accept files
        IFormFile? imageFile,
        IFormFile? attachedFile)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetUserId(out int adminId)) return Unauthorized(); // Optional: Check if adminId is needed for logging/auditing

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        string? newImageUrl = entity.ImageUrl;
        string? newFileUrl = entity.FileUrl;

        try
        {
            // 1. Handle Image File Re-upload (Displayable)
            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(entity.ImageUrl))
                    await _fileStorageService.DeleteFileAsync(entity.ImageUrl);
                
                // Upload new image
                newImageUrl = await _fileStorageService.SaveFileAsync(
                    imageFile,
                    NotificationFolderId,
                    dto.Title + "_Image");
            }
            // NOTE: If imageFile is null, the existing ImageUrl is kept.

            // 2. Handle Attached File Re-upload (Downloadable)
            if (attachedFile != null && attachedFile.Length > 0)
            {
                // Delete old file if it exists
                if (!string.IsNullOrEmpty(entity.FileUrl))
                    await _fileStorageService.DeleteFileAsync(entity.FileUrl);

                // Upload new file
                newFileUrl = await _fileStorageService.SaveFileAsync(
                    attachedFile,
                    NotificationFolderId,
                    dto.Title + "_Attachment");
            }
            // NOTE: If attachedFile is null, the existing FileUrl is kept.
        }
        catch (Exception ex)
        {
            // Clean up any files saved during this update attempt
            // We only clean up if the URL is different from the original entity's URL, 
            // indicating a new file was successfully uploaded before the failure.
            if (newImageUrl != entity.ImageUrl && !string.IsNullOrEmpty(newImageUrl))
                await _fileStorageService.DeleteFileAsync(newImageUrl);
            if (newFileUrl != entity.FileUrl && !string.IsNullOrEmpty(newFileUrl))
                await _fileStorageService.DeleteFileAsync(newFileUrl);

            return StatusCode(StatusCodes.Status500InternalServerError, $"File re-upload failed: {ex.Message}");
        }

        // Update entity properties
        entity.Title = dto.Title;
        entity.Content = dto.Content;
        entity.ImageUrl = newImageUrl; // Assign potentially new/updated URL
        entity.FileUrl = newFileUrl;   // Assign potentially new/updated URL

        await Repository.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    // D E L E T E (DELETE) - /api/Notifications/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        // 1. Delete physical files first
        if (!string.IsNullOrEmpty(entity.ImageUrl))
            await _fileStorageService.DeleteFileAsync(entity.ImageUrl);

        if (!string.IsNullOrEmpty(entity.FileUrl))
            await _fileStorageService.DeleteFileAsync(entity.FileUrl);

        // 2. Delete database record
        await Repository.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    // ===============================================
    // STUDENT ENDPOINT (READ ALL & Download)
    // ===============================================

    // R E A D A L L (GET) - /api/Notifications/feed
    [HttpGet("feed")]
    [Authorize(Roles = "Admin,Student,Teacher")] // Updated to include Teacher
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationFeed()
    {
        var entities = await Repository.GetAllAsync();

        var dtos = entities.Select(e => new NotificationDto
        {
            Id = e.Id,
            Title = e.Title,
            Content = e.Content,
            ImageUrl = e.ImageUrl,
            FileUrl = e.FileUrl,
            PostedDate = e.PostedDate,
            PostedByUserId = e.PostedByUserId
        }).ToList();

        return Ok(dtos);
    }

    // R E A D / D O W N L O A D FILE (GET) - /api/Notifications/{id}/download
    [HttpGet("{id}/download")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> DownloadNotificationFile(int id)
    {
        var notification = await Repository.GetByIdAsync(id);

        // Check for attached file (FileUrl)
        if (notification == null || string.IsNullOrEmpty(notification.FileUrl))
        {
            return NotFound("Notification or attached file not found.");
        }

        string relativePath = notification.FileUrl;
        // IMPORTANT: Assuming relativePath is already correctly stored (e.g., /Uploads/Subject-9999/file.pdf)
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/')); 

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The file content could not be located on the server.");
        }

        string contentType = "application/octet-stream";
        string fileName = Path.GetFileName(relativePath);

        // Basic MIME type mapping (can be improved)
        if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            contentType = "application/pdf";
        else if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            contentType = "application/zip";
        else if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            contentType = "image/jpeg";
        else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            contentType = "image/png"; // Added common image type for robustness
        else if (fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, contentType, fileName);
    }
    
    
    // 🆕 N E W : R E A D / D O W N L O A D IMAGE (GET) - /api/Notifications/{id}/downloadimage
    [HttpGet("{id}/downloadimage")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> DownloadNotificationImage(int id)
    {
        var notification = await Repository.GetByIdAsync(id);

        // Check for attached image (ImageUrl)
        if (notification == null || string.IsNullOrEmpty(notification.ImageUrl))
        {
            return NotFound("Notification or attached image not found.");
        }

        string relativePath = notification.ImageUrl;
        // IMPORTANT: Assuming relativePath is already correctly stored (e.g., /Uploads/Subject-9999/image.png)
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/'));

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The image file could not be located on the server.");
        }

        string contentType = "application/octet-stream";
        string fileName = Path.GetFileName(relativePath);

        // Basic MIME type mapping for images
        if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            contentType = "image/jpeg";
        else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            contentType = "image/png";
        else if (fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            contentType = "image/gif";


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        // Returns the file stream, forcing the browser to download it.
        return File(fileStream, contentType, fileName);
    }
}