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
    public async Task<string?> GetUserRoleAsync(ApplicationUser user)
{
    var roles = await _userManager.GetRolesAsync(user);
    return roles.FirstOrDefault();
}
public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
{
    return await _userManager.AddToRoleAsync(user, role);
}

public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
{
    return await _userManager.RemoveFromRoleAsync(user, role);
}
// --- NEW FORGOT PASSWORD IMPLEMENTATION ---

    /// <summary>
    /// Generates a secure, time-sensitive token for password reset using ASP.NET Identity's built-in mechanism.
    /// </summary>
    /// <param name="email">The email of the user requesting the reset.</param>
    /// <returns>The reset token string, or null if the user is not found.</returns>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Do not throw an error or indicate user not found for security reasons
            return null; 
        }

        // Generate the token
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
    // <summary>
    /// Resets the user's password using the generated token and a new password.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="token">The password reset token received via email.</param>
    /// <param name="newPassword">The new password the user wishes to set.</param>
    /// <returns>The result of the Identity operation (Success or Failure).</returns>
    public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Returning a success result here helps prevent user enumeration attacks, 
            // though the calling service/controller should check the email first.
            return IdentityResult.Failed(new IdentityError { Description = "User not found or token is invalid." });
        }

        // The built-in method handles token validation and password hashing.
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }


}