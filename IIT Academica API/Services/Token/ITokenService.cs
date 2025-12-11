public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
}