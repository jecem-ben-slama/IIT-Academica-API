// Services/IFileStorageService.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IFileStorageService
{
    /// <summary>
    /// Saves the file to storage, using a base name for the filename.
    /// </summary>
    /// <param name="file">The uploaded file stream.</param>
    /// <param name="subjectId">The subject ID for organization.</param>
    /// <param name="fileNameBase">The base string (e.g., DTO description) for the filename.</param>
    /// <returns>The stored relative path/URL.</returns>
    Task<string> SaveFileAsync(IFormFile file, int subjectId, string fileNameBase);

    /// <summary>
    /// Deletes the physical file from storage based on its path/URL.
    /// </summary>
    /// <param name="filePathOrUrl">The relative path of the file stored in the DB.</param>
    /// <returns>True if deletion was attempted, false otherwise.</returns>
    Task<bool> DeleteFileAsync(string filePathOrUrl);
}