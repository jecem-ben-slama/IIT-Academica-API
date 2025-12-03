using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<IdentityResult> RegisterAndAssignRoleAsync(ApplicationUser user, string password, string role);


    Task<ApplicationUser?> GetByIdAsync(int id);

    Task<ApplicationUser?> GetByEmailAsync(string email);

    Task<IEnumerable<ApplicationUser>> GetAllAsync();

    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);

   Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password);

  
    Task<IdentityResult> UpdateAsync(ApplicationUser user);

    
    Task<IdentityResult> DeleteAsync(ApplicationUser user);

    Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string roleName);
    Task<int?> GetUserIdByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetStudentsInTeacherSubjectAsync(int spaceId);
    Task<string?> GetUserRoleAsync(ApplicationUser user);
    Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);


    
}