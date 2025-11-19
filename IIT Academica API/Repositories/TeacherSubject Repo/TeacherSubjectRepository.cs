using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;

// Repositories/TeacherSubjectRepository.cs
public class TeacherSubjectRepository : ITeacherSubjectRepository
{
    private readonly ApplicationDbContext _context;

    // NOTE: IUserRepository is NOT injected here; the Controller handles user checks.
    public TeacherSubjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<TeacherSubject> AddAsync(TeacherSubject teacherSubject)
    {
        _context.TeacherSubjects.Add(teacherSubject);
        await _context.SaveChangesAsync();
        return teacherSubject;
    }

    // CHECK CODE EXISTENCE
    public async Task<bool> CodeExistsAsync(string registrationCode)
    {
        return await _context.TeacherSubjects
            .AnyAsync(ts => ts.RegistrationCode == registrationCode);
    }

    // READ (Get By Id - With Eager Loading)
    public async Task<TeacherSubject?> GetByIdWithTeacherAsync(int id)
    {
        // Eager load the Teacher object for DTO mapping in the Controller
        return await _context.TeacherSubjects
            .Include(ts => ts.Teacher)
            .FirstOrDefaultAsync(ts => ts.Id == id);
    }

    // --- NEWLY INCORPORATED CRUD METHODS ---

    // READ (Get By Id - Simple)
    public async Task<TeacherSubject?> GetByIdAsync(int id)
    {
        return await _context.TeacherSubjects.FindAsync(id);
    }
        
    // READ (Get All)
    public async Task<IEnumerable<TeacherSubject>> GetAllAsync()
    {
        // Retrieve all TeacherSubject entities without eager loading navigation properties
        return await _context.TeacherSubjects
                    .Include(ts => ts.Teacher)
                    .ToListAsync();
    }

    // UPDATE
    public async Task<TeacherSubject> UpdateAsync(TeacherSubject teacherSubject)
    {
        // Mark the entity as modified and save changes
        _context.TeacherSubjects.Update(teacherSubject);
        await _context.SaveChangesAsync();
        return teacherSubject;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        // 1. Find the entity by ID
        var entity = await _context.TeacherSubjects.FindAsync(id);

        if (entity == null)
        {
            // Entity not found
            return false;
        }

        // 2. Remove the entity
        _context.TeacherSubjects.Remove(entity);

        // 3. Save changes and check if any rows were affected
        var affectedRows = await _context.SaveChangesAsync();

        // Return true if at least one row was affected (i.e., the entity was deleted)
        return affectedRows > 0;
    }
}