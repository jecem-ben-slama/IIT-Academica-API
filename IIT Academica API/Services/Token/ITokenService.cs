using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
}