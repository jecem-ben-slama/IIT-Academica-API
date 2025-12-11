using IIT_Academica_API.Entities;

public interface ISubjectRepository
{
    Task<Subject> AddAsync(Subject Subject);
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject> UpdateAsync(Subject Subject);
    Task<bool> DeleteAsync(int id);

    Task<bool> CodeExistsAsync(string registrationCode);

    Task<Subject?> GetByIdWithTeacherAndEnrollmentsAsync(int id);
    Task<IEnumerable<Subject>> GetAllWithTeacherAndEnrollmentsAsync();
    Task<IEnumerable<Subject>> GetSubjectsByTeacherIdWithEnrollmentsAsync(int teacherId);
}