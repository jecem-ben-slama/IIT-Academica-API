using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IIT_Academica_API.Entities; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
   private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IdentityResult> RegisterAndAssignRoleAsync(ApplicationUser user, string password, string role)
    {
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        return result;
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        return isPasswordValid ? user : null;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }


    public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string roleName)
    {
        return await _userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<int?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<IEnumerable<ApplicationUser>> GetStudentsInTeacherSubjectAsync(int SubjectId)
    {
        var students = await _context.Enrollments
             // FIX: Filter by the new foreign key
             .Where(e => e.SubjectId == SubjectId)
             .Select(e => e.Student)
             .Distinct()
             .ToListAsync();

        return students;
    }
}