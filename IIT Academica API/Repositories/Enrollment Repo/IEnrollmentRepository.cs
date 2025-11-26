// Repositories/IEnrollmentRepository.cs
using IIT_Academica_API.Entities;
using System.Threading.Tasks;

public interface IEnrollmentRepository
{
    Task<Subject?> GetSubjectByRegistrationCodeAsync(string registrationCode);
    Task<bool> IsStudentAlreadyEnrolledAsync(int studentId, int SubjectId);
    Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment);
}