// Repositories/EnrollmentRepository.cs
using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // --- CREATE ---
    public async Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        return enrollment;
        // SaveChanges is handled by UnitOfWork.CompleteAsync()
    }

    // --- READ HELPERS ---
    public async Task<Subject?> GetSubjectByRegistrationCodeAsync(string registrationCode)
    {
        return await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RegistrationCode == registrationCode);
    }

    public async Task<bool> IsStudentAlreadyEnrolledAsync(int studentId, int subjectId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e =>
                e.StudentId == studentId &&
                e.SubjectId == subjectId);
    }

    // 🚀 NEW: GET BY STUDENT ID (For viewing enrolled courses)
    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            // Eagerly load the Subject and its related Teacher for the DTO
            .Include(e => e.Subject)
                .ThenInclude(s => s.Teacher)
            .AsNoTracking()
            .ToListAsync();
    }

    // --- DELETE (Basic) ---
    public async Task<bool> DeleteEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        return true;
        // SaveChanges is handled by UnitOfWork.CompleteAsync()
    }

    // 🚀 NEW: SECURE DELETE (For student drop/withdrawal)
    public async Task<bool> DeleteEnrollmentByStudentAndIdAsync(int enrollmentId, int studentId)
    {
        // 1. Find the enrollment, ensuring the StudentId matches the authenticated user.
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.StudentId == studentId);

        if (enrollment == null) return false;

        // 2. Remove the entity.
        _context.Enrollments.Remove(enrollment);
        return true;
        // SaveChanges is handled by UnitOfWork.CompleteAsync()
    }
}