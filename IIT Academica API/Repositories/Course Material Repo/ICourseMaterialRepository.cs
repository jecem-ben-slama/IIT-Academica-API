using IIT_Academica_API.Entities;
public interface ICourseMaterialRepository
{
    Task<CourseMaterial> AddAsync(CourseMaterial material);
    Task<CourseMaterial?> GetByIdAsync(int id);
    Task<CourseMaterial> UpdateAsync(CourseMaterial material);
    Task<bool> DeleteAsync(int id);
    Task<(string? filePathOrUrl, int? subjectId, string? materialType)> GetDownloadDetailsAsync(int materialId);
    Task<(int? teacherId, string? filePathOrUrl)> GetTeacherAndFilePathForDeletionAsync(int materialId);
    Task<IEnumerable<CourseMaterial>> GetBySubjectIdAsync(int subjectId);
    Task<int?> GetTeacherIdForMaterial(int materialId);
}
