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

    // C R E A T E (POST) - /api/Notifications
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
    // Note: For simplicity, this update does NOT handle re-uploading files. 
    // It assumes a separate DELETE/POST logic or a complex PATCH.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNotification(int id, [FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.Title = dto.Title;
        entity.Content = dto.Content;
        // ImageUrl and FileUrl are kept as-is unless a file is explicitly re-uploaded

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
    [Authorize(Roles = "Admin,Student")]
    [Authorize(Roles = "Admin,Student")]
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

    // R E A D / D O W N L O A D (GET) - /api/Notifications/{id}/download
    [HttpGet("{id}/download")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> DownloadNotificationFile(int id)
    {
        var notification = await Repository.GetByIdAsync(id);

        if (notification == null || string.IsNullOrEmpty(notification.FileUrl))
        {
            return NotFound("Notification or attached file not found.");
        }

        string relativePath = notification.FileUrl;
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

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


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, contentType, fileName);
    }
}