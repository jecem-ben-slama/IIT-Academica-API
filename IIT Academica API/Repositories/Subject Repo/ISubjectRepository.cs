// Repositories/ISubjectRepository.cs
using IIT_Academica_API.Entities;

public interface ISubjectRepository
{
    // --- Basic CRUD ---
    Task<Subject> AddAsync(Subject Subject); // CREATE
    Task<Subject?> GetByIdAsync(int id); // READ (Simple)
    Task<Subject> UpdateAsync(Subject Subject); // UPDATE
    Task<bool> DeleteAsync(int id); // DELETE

    // --- Helper ---
    Task<bool> CodeExistsAsync(string registrationCode);

    // --- Eager Loading for DTOs ---
    Task<Subject?> GetByIdWithTeacherAndEnrollmentsAsync(int id);
    Task<IEnumerable<Subject>> GetAllWithTeacherAndEnrollmentsAsync();
}