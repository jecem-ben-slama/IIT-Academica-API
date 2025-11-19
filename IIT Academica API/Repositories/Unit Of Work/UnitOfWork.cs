using Microsoft.AspNetCore.Identity;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;

    private IUserRepository _userRepository;
    private ITeacherSubjectRepository _teacherSubjectRepository;

    public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IUserRepository Users =>
        _userRepository ??= new UserRepository(_context, _userManager);
    public ITeacherSubjectRepository TeacherSubjects =>
        _teacherSubjectRepository ??= new TeacherSubjectRepository(_context);


    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}