public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, int subjectId, string fileNameBase);
    Task<bool> DeleteFileAsync(string filePathOrUrl);
}