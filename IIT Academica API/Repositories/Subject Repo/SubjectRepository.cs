using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;

public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _context;

    public SubjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Subject> AddAsync(Subject Subject)
    {
        _context.Subjects.Add(Subject);
        return Subject;
    }

    public async Task<bool> CodeExistsAsync(string registrationCode)
    {
        return await _context.Subjects.AnyAsync(ts => ts.RegistrationCode == registrationCode);
    }

    public async Task<Subject?> GetByIdWithTeacherAndEnrollmentsAsync(int id)
    {
        return await _context.Subjects
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Enrollments) 
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _context.Subjects.FindAsync(id);
    }

    public async Task<IEnumerable<Subject>> GetAllWithTeacherAndEnrollmentsAsync()
    {
        return await _context.Subjects
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Enrollments) 
            .ToListAsync();
    }
    public async Task<IEnumerable<Subject>> GetSubjectsByTeacherIdWithEnrollmentsAsync(int teacherId)
    {
        return await _context.Subjects
            .Where(s => s.TeacherId == teacherId) 
            .Include(s => s.Teacher)              
            .Include(s => s.Enrollments)          
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Subject> UpdateAsync(Subject Subject)
    {
        _context.Subjects.Update(Subject);
        return Subject; 
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Subjects.FindAsync(id);
        if (entity == null) return false;
        _context.Subjects.Remove(entity);
        return true; 
    }
}