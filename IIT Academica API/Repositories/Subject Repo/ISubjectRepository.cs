// Repositories/ITeacherSubjectRepository.cs
using IIT_Academica_API.Entities;

public interface ISubjectRepository
{
    // --- TeacherSubject CRUD ---
    Task<Subject> AddAsync(Subject Subject);
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject> UpdateAsync(Subject Subject);
    Task<bool> DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string registrationCode);

    // 🚀 UPDATED: Includes Teacher and Enrollments for DTO mapping
    Task<Subject?> GetByIdWithTeacherAndEnrollmentsAsync(int id);

    // 🚀 UPDATED: Includes Teacher and Enrollments for DTO mapping
    Task<IEnumerable<Subject>> GetAllWithTeacherAndEnrollmentsAsync();
}