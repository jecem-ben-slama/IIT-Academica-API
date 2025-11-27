// Repositories/CourseMaterialRepository.cs
using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CourseMaterialRepository : ICourseMaterialRepository
{
    private readonly ApplicationDbContext _context;

    public CourseMaterialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseMaterial> AddAsync(CourseMaterial material)
    {
        _context.CourseMaterials.Add(material);
        return material;
    }

    public async Task<CourseMaterial?> GetByIdAsync(int id)
    {
        return await _context.CourseMaterials.FindAsync(id);
    }

    public async Task<CourseMaterial> UpdateAsync(CourseMaterial material)
    {
        _context.CourseMaterials.Update(material);
        return material;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.CourseMaterials.FindAsync(id);
        if (entity == null) return false;

        _context.CourseMaterials.Remove(entity);
        return true;
    }

    public async Task<IEnumerable<CourseMaterial>> GetBySubjectIdAsync(int subjectId)
    {
        return await _context.CourseMaterials
            .Where(m => m.SubjectId == subjectId)
            .AsNoTracking()
            .ToListAsync();
    }

    // 🚀 Implementation for Teacher Ownership Check
    public async Task<int?> GetTeacherIdForMaterial(int materialId)
    {
        return await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            .Select(m => (int?)m.Subject.TeacherId) // Navigate to Subject to get TeacherId
            .FirstOrDefaultAsync();
    }
    // Repositories/CourseMaterialRepository.cs

    // ... existing methods ...

    public async Task<(string? filePathOrUrl, int? subjectId, string? materialType)> GetDownloadDetailsAsync(int materialId)
    {
        var result = await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            // 🚀 FIX: Project to an anonymous type only, and use FirstOrDefaultAsync() 
            // to execute the query.
            .Select(m => new { m.FilePathOrUrl, m.SubjectId, m.MaterialType })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
        {
            // Return null values in the tuple if no material is found
            return (null, null, null);
        }

        // 🚀 FIX: Convert the anonymous type to the required tuple type *after* // the database query has executed (i.e., outside the expression tree).
        return (result.FilePathOrUrl, result.SubjectId, result.MaterialType);
    }
    // Repositories/CourseMaterialRepository.cs

    // ... existing methods ...

    public async Task<(int? teacherId, string? filePathOrUrl)> GetTeacherAndFilePathForDeletionAsync(int materialId)
    {
        var result = await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            // Join implicitly to Subject to get TeacherId
            .Select(m => new { TeacherId = (int?)m.Subject.TeacherId, m.FilePathOrUrl })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return (null, null);
        }

        // Convert the anonymous type to the required tuple after query execution
        return (result.TeacherId, result.FilePathOrUrl);
    }
}