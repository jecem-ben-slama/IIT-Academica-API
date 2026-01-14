using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        return enrollment;
    }

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

    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Subject)
                .ThenInclude(s => s!.Teacher)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> DeleteEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        return true;
    }

    public async Task<bool> DeleteEnrollmentByStudentAndIdAsync(int enrollmentId, int studentId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e =>
                e.Id == enrollmentId &&
                e.StudentId == studentId);

        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        return true;
    }

    public async Task<bool> HasActiveEnrollmentsForSubject(int subjectId)
    {


        return await _context.Enrollments
                             .AnyAsync(e => e.SubjectId == subjectId);
    }
}
