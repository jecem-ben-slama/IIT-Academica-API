public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    ITeacherSubjectRepository TeacherSubjects { get; }

    Task<int> CompleteAsync();
}