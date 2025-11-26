
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    ISubjectRepository Subjects { get; }
    IEnrollmentRepository Enrollments { get; }

    Task<int> CompleteAsync();
}