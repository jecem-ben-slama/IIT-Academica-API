using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [StringLength(50,MinimumLength =20,ErrorMessage ="Email Address Length problem")]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Role { get; set; }
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Name Length problem")]
    public string? Name { get; set; }
    [StringLength(15, MinimumLength = 3, ErrorMessage = "LastName Length problem")]
    public string? LastName { get; set; }
}