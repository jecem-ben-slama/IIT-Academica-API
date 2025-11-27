// Repositories/ICourseMaterialRepository.cs
using IIT_Academica_API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICourseMaterialRepository
{
    Task<CourseMaterial> AddAsync(CourseMaterial material);
    Task<CourseMaterial?> GetByIdAsync(int id);
    Task<CourseMaterial> UpdateAsync(CourseMaterial material);
    Task<bool> DeleteAsync(int id);
    Task<(string? filePathOrUrl, int? subjectId, string? materialType)> GetDownloadDetailsAsync(int materialId);
    Task<(int? teacherId, string? filePathOrUrl)> GetTeacherAndFilePathForDeletionAsync(int materialId);
    // Get all materials for a specific subject
    Task<IEnumerable<CourseMaterial>> GetBySubjectIdAsync(int subjectId);

    // Helper to check ownership (Teacher security check)
    Task<int?> GetTeacherIdForMaterial(int materialId);
}