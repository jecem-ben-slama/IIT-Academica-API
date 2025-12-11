using IIT_Academica_API.Entities;
public interface IEnrollmentRepository
{
    Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment);
    Task<bool> DeleteEnrollmentAsync(int enrollmentId);

    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);

    Task<bool> DeleteEnrollmentByStudentAndIdAsync(int enrollmentId, int studentId);

    Task<Subject?> GetSubjectByRegistrationCodeAsync(string registrationCode);
    Task<bool> IsStudentAlreadyEnrolledAsync(int studentId, int subjectId);
    Task<bool> HasActiveEnrollmentsForSubject(int subjectId);
}