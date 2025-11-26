using Microsoft.AspNetCore.Identity;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;

    private IUserRepository _userRepository;
    private ISubjectRepository _subjectRepository;
    private IEnrollmentRepository _enrollmentRepository;

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_context, _userManager);
    public ISubjectRepository Subjects =>
        _subjectRepository ??= new SubjectRepository(_context);
    public IEnrollmentRepository Enrollments =>
        _enrollmentRepository ??= new EnrollmentRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}