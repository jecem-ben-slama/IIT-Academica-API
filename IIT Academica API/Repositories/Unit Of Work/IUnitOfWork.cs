
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    ISubjectRepository Subjects { get; }
    IEnrollmentRepository Enrollments { get; }
    ICourseMaterialRepository courseMaterials { get; }

    Task<int> CompleteAsync();
}