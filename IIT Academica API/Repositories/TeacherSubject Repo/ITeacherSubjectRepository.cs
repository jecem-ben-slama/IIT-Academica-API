using IIT_Academica_API.Entities;

// Repositories/ITeacherSubjectRepository.cs
public interface ITeacherSubjectRepository
{
    // --- Existing Methods ---

    // Method to check if the unique registration code already exists
    Task<bool> CodeExistsAsync(string registrationCode);

    // Method to add the new entity to the database
    // (This fulfills the 'Create' requirement)
    Task<TeacherSubject> AddAsync(TeacherSubject teacherSubject);

    // Method to retrieve the entity including navigation properties
    Task<TeacherSubject?> GetByIdWithTeacherAsync(int id);

    // --- Requested New Methods ---

    // READ (GetById)
    // You already have a specialized GetByIdWithTeacherAsync, 
    // but here is a simpler one if needed, or you can rename/reuse the existing one.
    Task<TeacherSubject?> GetByIdAsync(int id);

    // READ (GetAll)
    // Method to retrieve all entities
    Task<IEnumerable<TeacherSubject>> GetAllAsync();

    // UPDATE
    // Method to update an existing entity
    Task<TeacherSubject> UpdateAsync(TeacherSubject teacherSubject);

    // DELETE
    // Method to delete an entity by its ID
    Task<bool> DeleteAsync(int id);
    // Alternatively: Task DeleteAsync(TeacherSubject teacherSubject);
}