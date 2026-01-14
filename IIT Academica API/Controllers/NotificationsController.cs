using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    private const int NotificationFolderId = 9999;

    //! Admin
    //^ Create
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
            if (imageFile != null && imageFile.Length > 0)
            {
                imageUrl = await _fileStorageService.SaveFileAsync(
                    imageFile,
                    NotificationFolderId,
                    dto.Title + "_Image");
            }

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



    //^ Update
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateNotification(
        int id,
        [FromForm] CreateNotificationDto dto,
        IFormFile? imageFile,
        IFormFile? attachedFile)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!TryGetUserId(out int adminId)) return Unauthorized();

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        string? newImageUrl = entity.ImageUrl;
        string? newFileUrl = entity.FileUrl;

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(entity.ImageUrl))
                    await _fileStorageService.DeleteFileAsync(entity.ImageUrl);

                newImageUrl = await _fileStorageService.SaveFileAsync(
                    imageFile,
                    NotificationFolderId,
                    dto.Title + "_Image");
            }
            if (attachedFile != null && attachedFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(entity.FileUrl))
                    await _fileStorageService.DeleteFileAsync(entity.FileUrl);

                newFileUrl = await _fileStorageService.SaveFileAsync(
                    attachedFile,
                    NotificationFolderId,
                    dto.Title + "_Attachment");
            }
        }
        catch (Exception ex)
        {
            if (newImageUrl != entity.ImageUrl && !string.IsNullOrEmpty(newImageUrl))
                await _fileStorageService.DeleteFileAsync(newImageUrl);
            if (newFileUrl != entity.FileUrl && !string.IsNullOrEmpty(newFileUrl))
                await _fileStorageService.DeleteFileAsync(newFileUrl);

            return StatusCode(StatusCodes.Status500InternalServerError, $"File re-upload failed: {ex.Message}");
        }

        entity.Title = dto.Title;
        entity.Content = dto.Content;
        entity.ImageUrl = newImageUrl;
        entity.FileUrl = newFileUrl;

        await Repository.UpdateAsync(entity);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    //^ delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        if (!string.IsNullOrEmpty(entity.ImageUrl))
            await _fileStorageService.DeleteFileAsync(entity.ImageUrl);

        if (!string.IsNullOrEmpty(entity.FileUrl))
            await _fileStorageService.DeleteFileAsync(entity.FileUrl);

        await Repository.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
    //^ Get By Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<NotificationDto>> GetNotificationById(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

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
    //^ Feed
    [HttpGet("feed")]
    [Authorize(Roles = "Admin,Student,Teacher")]
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

    //^ Download File
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
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/'));

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The file content could not be located on the server.");
        }

        string contentType = "application/octet-stream";
        string fileName = Path.GetFileName(relativePath);

        if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            contentType = "application/pdf";
        else if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            contentType = "application/zip";
        else if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            contentType = "image/jpeg";
        else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            contentType = "image/png";
        else if (fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, contentType, fileName);
    }


    //^ Download Image
    [HttpGet("{id}/downloadimage")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> DownloadNotificationImage(int id)
    {
        var notification = await Repository.GetByIdAsync(id);

        if (notification == null || string.IsNullOrEmpty(notification.ImageUrl))
        {
            return NotFound("Notification or attached image not found.");
        }

        string relativePath = notification.ImageUrl;
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/'));

        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound("The image file could not be located on the server.");
        }

        string contentType = "application/octet-stream";
        string fileName = Path.GetFileName(relativePath);

        if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            contentType = "image/jpeg";
        else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            contentType = "image/png";
        else if (fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            contentType = "image/gif";


        var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, contentType, fileName);
    }
}
