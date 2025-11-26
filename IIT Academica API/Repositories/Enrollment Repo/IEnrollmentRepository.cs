// Repositories/IEnrollmentRepository.cs
using IIT_Academica_API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEnrollmentRepository
{
    // --- Core Enrollment CRUD ---
    Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment);
    Task<bool> DeleteEnrollmentAsync(int enrollmentId); // For hard deletion

    // --- Student Viewing ---
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);

    // --- Student Drop Helper (Ensures security) ---
    Task<bool> DeleteEnrollmentByStudentAndIdAsync(int enrollmentId, int studentId);

    // --- Helpers for Enrollment Process ---
    Task<Subject?> GetSubjectByRegistrationCodeAsync(string registrationCode);
    Task<bool> IsStudentAlreadyEnrolledAsync(int studentId, int subjectId);
}