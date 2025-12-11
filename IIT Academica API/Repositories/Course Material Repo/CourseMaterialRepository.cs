using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<int?> GetTeacherIdForMaterial(int materialId)
    {
        return await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            .Select(m => (int?)m.Subject!.TeacherId)
            .FirstOrDefaultAsync();
    }


    public async Task<(string? filePathOrUrl, int? subjectId, string? materialType)> GetDownloadDetailsAsync(int materialId)
    {
        var result = await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            .Select(m => new { m.FilePathOrUrl, m.SubjectId, m.MaterialType })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return (null, null, null);
        }

        return (result.FilePathOrUrl, result.SubjectId, result.MaterialType);
    }


    public async Task<(int? teacherId, string? filePathOrUrl)> GetTeacherAndFilePathForDeletionAsync(int materialId)
    {
        var result = await _context.CourseMaterials
            .Where(m => m.Id == materialId)
            .Select(m => new { TeacherId = (int?)m.Subject!.TeacherId, m.FilePathOrUrl })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return (null, null);
        }

        return (result.TeacherId, result.FilePathOrUrl);
    }
}