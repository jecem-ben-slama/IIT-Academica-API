public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath = "Uploads"; // Folder name
    private readonly string _baseDirectory;

    public LocalFileStorageService()
    {
        // Get the absolute path to the uploads folder within the running application directory
        _baseDirectory = Directory.GetCurrentDirectory();

        // Ensure the root upload directory exists
        string absolutePath = Path.Combine(_baseDirectory, _uploadPath);
        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, int subjectId, string fileNameBase)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is null or empty.", nameof(file));
        }

        // 1. Setup path
        string subjectFolderName = $"Subject-{subjectId}";
        string subjectPath = Path.Combine(_uploadPath, subjectFolderName); // Relative path segment
        string absoluteSubjectPath = Path.Combine(_baseDirectory, subjectPath); // Absolute path on disk

        if (!Directory.Exists(absoluteSubjectPath))
        {
            Directory.CreateDirectory(absoluteSubjectPath);
        }

        // 2. Generate unique filename based on fileNameBase (Description)

        // Sanitize the base name by removing invalid file system characters
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        string sanitizedBaseName = string.Join("_", fileNameBase.Split(invalidChars.ToArray(), StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(sanitizedBaseName))
        {
            sanitizedBaseName = "Material";
        }

        // Ensure the base name isn't too long (optional, but good practice)
        sanitizedBaseName = sanitizedBaseName.Length > 50 ? sanitizedBaseName.Substring(0, 50) : sanitizedBaseName;

        // Use the sanitized name + a unique GUID suffix to guarantee no collision
        string fileExtension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{sanitizedBaseName}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExtension}";

        // 3. Define the full file path on the server
        string filePath = Path.Combine(absoluteSubjectPath, uniqueFileName);

        // 4. Save the file stream to the disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 5. Return the relative path to be stored in the DB
        return Path.Combine(subjectPath, uniqueFileName).Replace('\\', '/');
    }

    public Task<bool> DeleteFileAsync(string filePathOrUrl)
    {
        if (string.IsNullOrEmpty(filePathOrUrl))
        {
            return Task.FromResult(false);
        }

        // Construct the absolute path from the relative path stored in the DB
        string absolutePath = Path.Combine(_baseDirectory, filePathOrUrl);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
