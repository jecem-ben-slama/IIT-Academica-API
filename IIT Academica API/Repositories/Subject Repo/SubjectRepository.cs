// Repositories/TeacherSubjectRepository.cs
using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _context;

    public SubjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ... CREATE, CHECK CODE EXISTENCE methods (No change) ...
    public async Task<Subject> AddAsync(Subject Subject)
    {
        _context.Subjects.Add(Subject);
        return Subject; // Will be saved by UoW.CompleteAsync()
    }

    public async Task<bool> CodeExistsAsync(string registrationCode)
    {
        return await _context.Subjects.AnyAsync(ts => ts.RegistrationCode == registrationCode);
    }

    // 🚀 UPDATED: READ (Get By Id - With Eager Loading)
    public async Task<Subject?> GetByIdWithTeacherAndEnrollmentsAsync(int id)
    {
        return await _context.Subjects
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Enrollments) // <--- ADDED: Load Enrollments
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _context.Subjects.FindAsync(id);
    }

    // 🚀 UPDATED: READ (Get All - With Eager Loading)
    public async Task<IEnumerable<Subject>> GetAllWithTeacherAndEnrollmentsAsync()
    {
        return await _context.Subjects
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Enrollments) // <--- ADDED: Load Enrollments
            .ToListAsync();
    }
    public async Task<IEnumerable<Subject>> GetSubjectsByTeacherIdWithEnrollmentsAsync(int teacherId)
    {
        return await _context.Subjects
            .Where(s => s.TeacherId == teacherId) // Filter by the provided TeacherId
            .Include(s => s.Teacher)              // Load the Teacher details
            .Include(s => s.Enrollments)          // Load the Enrollments collection
            .AsNoTracking()
            .ToListAsync();
    }

    // ... UPDATE, DELETE methods (No change) ...
    public async Task<Subject> UpdateAsync(Subject Subject)
    {
        _context.Subjects.Update(Subject);
        return Subject; // Will be saved by UoW.CompleteAsync()
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Subjects.FindAsync(id);
        if (entity == null) return false;
        _context.Subjects.Remove(entity);
        return true; // UoW will determine if rows were affected
    }
}