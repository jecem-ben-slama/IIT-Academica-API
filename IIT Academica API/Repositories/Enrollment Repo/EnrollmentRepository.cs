// Repositories/EnrollmentRepository.cs
using IIT_Academica_API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Subject?> GetSubjectByRegistrationCodeAsync(string registrationCode)
    {
        return await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.RegistrationCode == registrationCode);
    }

    public async Task<bool> IsStudentAlreadyEnrolledAsync(int studentId, int SubjectId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e =>
                e.StudentId == studentId &&
                e.SubjectId == SubjectId);
    }

    public async Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        return enrollment;
    }
}